#pragma once

#include <DetourCommon.h>
#include <DetourTileCache.h>
#include <DetourTileCacheBuilder.h>
#include <fastlz.h>
#include <Recast.h>
#include <NavMesh.pb.h>
#include <algorithm>
#include <iostream>
#include <iterator>

#include "RecastClient.h"

struct FastLZCompressor : public dtTileCacheCompressor
{
	virtual int maxCompressedSize(const int bufferSize)
	{
		return (int)(bufferSize* 1.05f);
	}

	virtual dtStatus compress(const unsigned char* buffer, const int bufferSize,
		unsigned char* compressed, const int /*maxCompressedSize*/, int* compressedSize)
	{
		*compressedSize = fastlz_compress((const void *const)buffer, bufferSize, compressed);
		return DT_SUCCESS;
	}

	virtual dtStatus decompress(const unsigned char* compressed, const int compressedSize,
		unsigned char* buffer, const int maxBufferSize, int* bufferSize)
	{
		*bufferSize = fastlz_decompress(compressed, compressedSize, buffer, maxBufferSize);
		return *bufferSize < 0 ? DT_FAILURE : DT_SUCCESS;
	}
};

struct LinearAllocator : public dtTileCacheAlloc
{
	unsigned char* buffer;
	size_t capacity;
	size_t top;
	size_t high;

	LinearAllocator(const size_t cap) : buffer(0), capacity(0), top(0), high(0)
	{
		resize(cap);
	}

	~LinearAllocator()
	{
		dtFree(buffer);
	}

	void resize(const size_t cap)
	{
		if (buffer) dtFree(buffer);
		buffer = (unsigned char*)dtAlloc(cap, DT_ALLOC_PERM);
		capacity = cap;
	}

	virtual void reset()
	{
		high = dtMax(high, top);
		top = 0;
	}

	virtual void* alloc(const size_t size)
	{
		if (!buffer)
			return 0;
		if (top + size > capacity)
			return 0;
		unsigned char* mem = &buffer[top];
		top += size;
		return mem;
	}

	virtual void free(void* /*ptr*/)
	{
		// Empty
	}
};

struct MeshProcess : public dtTileCacheMeshProcess
{
	InputGeom* m_geom;

	inline MeshProcess() : m_geom(0)
	{
	}

	inline void init(InputGeom* geom)
	{
		m_geom = geom;
	}

	virtual void process(struct dtNavMeshCreateParams* params,
		unsigned char* polyAreas, unsigned short* polyFlags)
	{
		// Update poly flags from areas.
		for (int i = 0; i < params->polyCount; ++i)
		{
			if (polyAreas[i] == DT_TILECACHE_WALKABLE_AREA)
				polyAreas[i] = RecastWrapper::POLYAREA_GROUND;

			if (polyAreas[i] == RecastWrapper::POLYAREA_GROUND ||
				polyAreas[i] == RecastWrapper::POLYAREA_GRASS ||
				polyAreas[i] == RecastWrapper::POLYAREA_ROAD)
			{
				polyFlags[i] = RecastWrapper::POLYFLAGS_WALK;
			}
			else if (polyAreas[i] == RecastWrapper::POLYAREA_WATER)
			{
				polyFlags[i] = RecastWrapper::POLYFLAGS_SWIM;
			}
			else if (polyAreas[i] == RecastWrapper::POLYAREA_DOOR)
			{
				polyFlags[i] = RecastWrapper::POLYFLAGS_WALK | RecastWrapper::POLYFLAGS_DOOR;
			}
		}

		// Pass in off-mesh connections.
		if (m_geom)
		{
			params->offMeshConVerts = m_geom->getOffMeshConnectionVerts();
			params->offMeshConRad = m_geom->getOffMeshConnectionRads();
			params->offMeshConDir = m_geom->getOffMeshConnectionDirs();
			params->offMeshConAreas = m_geom->getOffMeshConnectionAreas();
			params->offMeshConFlags = m_geom->getOffMeshConnectionFlags();
			params->offMeshConUserID = m_geom->getOffMeshConnectionId();
			params->offMeshConCount = m_geom->getOffMeshConnectionCount();
		}
	}
};

struct TileCacheData
{
	unsigned char* data;
	int dataSize;
};

namespace RecastWrapper {
	class RecastClientTiled : public RecastClient
	{
	public:
		static const int EXPECTED_LAYERS_PER_TILE = 4;
		static const int MAX_LAYERS = 32;
		int m_maxObstables = 2048;

		RecastClientTiled();
		~RecastClientTiled();

		bool Save(GDX::AI::ProtoRecastTiledNavMesh* proto);
		bool Load(GDX::AI::ProtoRecastTiledNavMesh* proto);

		virtual dtStatus update(float delta)
		{
			dtStatus status = RecastClient::update(delta);
			if(dtStatusFailed(status))
			{
				return status;
			}

			return m_tileCache->update(delta, m_navMesh);
		}

		virtual dtStatus addObstacle(const float* pos, float radius, float height, dtObstacleRef* ref);
		virtual dtStatus addObstacleBox(const float* bmin, const float* bmax, dtObstacleRef* ref);
		virtual dtStatus removeObstacle(dtObstacleRef ref);
		virtual void clearObstacles();

	protected:
		int m_maxTiles; // set by code
		int m_maxPolysPerTile; // set by code
		float m_tileSize = 48;

		int m_cacheCompressedSize;
		int m_cacheRawSize;
		int m_cacheLayerCount;

		dtTileCacheParams m_tcparams;

		struct ::LinearAllocator* m_talloc;
		struct ::FastLZCompressor* m_tcomp;
		struct ::MeshProcess* m_tmproc;

		class dtTileCache* m_tileCache;

		virtual bool prepareBuild();
		virtual bool doBuild();

	private:
		static int calcLayerBufferSize(const int gridWidth, const int gridHeight)
		{
			const int headerSize = dtAlign4(sizeof(dtTileCacheLayerHeader));
			const int gridSize = gridWidth * gridHeight;
			return headerSize + gridSize * 4;
		}

		void buildStep1InitConfig();
		int rasterizeTileLayers(const int tx, const int ty,	TileCacheData* tiles, const int maxTiles);

		bool finalizeLoad(bool traceMemory);
	};
}
