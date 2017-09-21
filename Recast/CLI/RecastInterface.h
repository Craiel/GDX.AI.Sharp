#pragma once
#include "RecastClient.h"

namespace RecastWrapper {

	class RecastInterface
	{
	private:
		RecastClient*** clients;
		bool isTiled;

	public:
		RecastInterface();
	
		bool Configure(unsigned int totalAreas, unsigned int totalLayers)
		{
			clients = new RecastClient**[totalAreas];
			for (int i = 0; i < totalAreas; ++i)
			{
				clients[i] = new RecastClient*[totalLayers];
			}

			return true;
		}

		bool Initialize(unsigned int area, unsigned int layer, bool isTiled);
		bool Destroy(unsigned int area, unsigned int layer) const;
		bool Update(unsigned int area, unsigned int layer, float delta) const;
		bool LoadSettings(unsigned int area, unsigned int layer, char* data, int size) const;
		bool Build(unsigned int area, unsigned int layer, char* path, int size) const;
		bool Load(unsigned int area, unsigned int layer, char* data, int size) const;
		bool Save(unsigned int area, unsigned int layer, char* &data, int &size) const;
		bool GetDebugNavMesh(unsigned int area, unsigned int layer, char* &data, int &size) const;
		bool LogBuildTimes(unsigned int area, unsigned int layer) const;
		bool GetLog(unsigned int area, unsigned int layer, char* &data, int &size) const;
		int AddAgent(unsigned int area, unsigned int layer, char* data, int size) const;
		bool UpdateAgent(unsigned int area, unsigned int layer, int index, char* data, int size) const;
		bool RemoveAgent(unsigned int area, unsigned int layer, int index) const;
		bool RequestMoveTarget(unsigned int area, unsigned int layer, int index, unsigned int polyRef, float x, float y, float z) const;
		bool ResetMoveTarget(unsigned int area, unsigned int layer, int index) const;
		bool FindNearestPoly(unsigned int area, unsigned int layer, float centerX, float centerY, float centerZ, float extendX, float extendY, float extendZ, unsigned int &nearestRef, float &nearestX, float &nearestY, float &nearestZ) const;
		bool GetAgentInfo(unsigned int area, unsigned int layer, int index, char* &data, int &size) const;
		bool FindRandomPointAroundCircle(unsigned int area, unsigned int layer, unsigned int startRef, float centerX, float centerY,
			float centerZ, float maxRadius, unsigned int &randomRef, float &randomPositionX, float &randomPositionY,
			float &randomPositionZ) const;
		bool GetPath(unsigned int area, unsigned int layer, unsigned int startRef, unsigned int endRef,
			float startX, float startY, float startZ,
			float endX, float endY, float endZ,
			bool detailed, char* &data, int &size) const;
		bool AddObstacle(unsigned int area, unsigned int layer, float positionX, float positionY, float positionZ, float radius, float height, unsigned int &obstacleRef) const;
		bool AddObstacleBox(unsigned int area, unsigned int layer, float minX, float minY, float minZ, float maxX, float maxY, float maxZ, unsigned int &obstacleRef) const;
		bool RemoveObstacle(unsigned int area, unsigned int layer, unsigned int obstacleRef) const;
		bool ClearObstacles(unsigned int area, unsigned int layer) const;		
	};

	static RecastInterface* singleton;
	static void InitializeSingleton()
	{
		if (singleton == nullptr) {
			singleton = new RecastInterface();
		}
	}

	static void DestroySingleton()
	{
		delete singleton;
	}

	static RecastInterface* Instance() {
		return singleton;
	}
}

#ifdef NATIVEDLL_EXPORTS
#define NATIVEDLL_API __declspec(dllexport)
#else
#define NATIVEDLL_API __declspec(dllimport)
#endif

extern "C" { 

	NATIVEDLL_API bool inline __cdecl RecastConfigure(unsigned int totalAreas, unsigned int totalLayers)
	{
		RecastWrapper::InitializeSingleton();
		RecastWrapper::Instance()->Configure(totalAreas, totalLayers);

		return true;
	}

	NATIVEDLL_API bool inline __cdecl RecastInitialize(unsigned int area, unsigned int layer, bool isTiled)
	{
		RecastWrapper::InitializeSingleton();
		RecastWrapper::Instance()->Initialize(area, layer, isTiled);

		return true;
	}

	NATIVEDLL_API bool inline __cdecl RecastDestroy(unsigned int area, unsigned int layer)
	{
		RecastWrapper::Instance()->Destroy(area, layer);
		return true;
	}

	NATIVEDLL_API bool inline __cdecl RecastLoadSettings(unsigned int area, unsigned int layer, char* data, int size)
	{
		return RecastWrapper::Instance()->LoadSettings(area, layer, data, size);
	}

	NATIVEDLL_API bool inline __cdecl RecastUpdate(unsigned int area, unsigned int layer, float delta)
	{
		return RecastWrapper::Instance()->Update(area, layer, delta);
	}

	NATIVEDLL_API bool inline __cdecl RecastBuild(unsigned int area, unsigned int layer, char* data, int size)
	{
		return RecastWrapper::Instance()->Build(area, layer, data, size);
	}

	NATIVEDLL_API bool inline __cdecl RecastLoad(unsigned int area, unsigned int layer, char* data, int size)
	{
		return RecastWrapper::Instance()->Load(area, layer, data, size);
	}

	NATIVEDLL_API bool inline __cdecl RecastSave(unsigned int area, unsigned int layer, char* &data, int &size)
	{
		return RecastWrapper::Instance()->Save(area, layer, data, size);
	}

	NATIVEDLL_API bool inline __cdecl RecastFreePointer(void* ptr)
	{
		free(ptr);
		return true;
	}

	NATIVEDLL_API bool inline __cdecl RecastGetDebugNavMesh(unsigned int area, unsigned int layer, char* &data, int &size)
	{
		return RecastWrapper::Instance()->GetDebugNavMesh(area, layer, data, size);
	}

	NATIVEDLL_API bool inline __cdecl RecastLogBuildTimes(unsigned int area, unsigned int layer)
	{
		return RecastWrapper::Instance()->LogBuildTimes(area, layer);
	}

	NATIVEDLL_API bool inline __cdecl RecastGetLog(unsigned int area, unsigned int layer, char* &data, int &size)
	{
		return RecastWrapper::Instance()->GetLog(area, layer, data, size);
	}

	NATIVEDLL_API int inline __cdecl RecastAddAgent(unsigned int area, unsigned int layer, char* data, int size)
	{
		return RecastWrapper::Instance()->AddAgent(area, layer, data, size);
	}

	NATIVEDLL_API bool inline __cdecl RecastUpdateAgent(unsigned int area, unsigned int layer, int index, char* data, int size)
	{
		return RecastWrapper::Instance()->UpdateAgent(area, layer, index, data, size);
	}

	NATIVEDLL_API bool inline __cdecl RecastRemoveAgent(unsigned int area, unsigned int layer, int index)
	{
		return RecastWrapper::Instance()->RemoveAgent(area, layer, index);
	}

	NATIVEDLL_API bool inline __cdecl RecastRequestMoveTarget(unsigned int area, unsigned int layer, int index, unsigned int polyRef, float x, float y, float z)
	{
		return RecastWrapper::Instance()->RequestMoveTarget(area, layer, index, polyRef, x, y, z);
	}

	NATIVEDLL_API bool inline __cdecl RecastResetMoveTarget(unsigned int area, unsigned int layer, int index)
	{
		return RecastWrapper::Instance()->ResetMoveTarget(area, layer, index);
	}

	NATIVEDLL_API bool inline __cdecl RecastFindNearestPoly(unsigned int area, unsigned int layer, float centerX, float centerY, float centerZ, float extendX, float extendY, float extendZ, unsigned int &nearestRef, float &nearestX, float &nearestY, float &nearestZ)
	{
		return RecastWrapper::Instance()->FindNearestPoly(area, layer, centerX, centerY, centerZ, extendX, extendY, extendZ, nearestRef, nearestX, nearestY, nearestZ);
	}

	NATIVEDLL_API bool inline __cdecl RecastGetAgentInfo(unsigned int area, unsigned int layer, int index, char* &data, int &size)
	{
		return RecastWrapper::Instance()->GetAgentInfo(area, layer, index, data, size);
	}

	NATIVEDLL_API bool inline __cdecl RecastFindRandomPointAroundCircle(unsigned int area, unsigned int layer, unsigned int startRef, float centerX, float centerY,
		float centerZ, float maxRadius, unsigned int &randomRef, float &randomPositionX, float &randomPositionY,
		float &randomPositionZ)
	{
		return RecastWrapper::Instance()->FindRandomPointAroundCircle(area, layer, startRef, centerX, centerY, centerZ, maxRadius, randomRef, randomPositionX, randomPositionY, randomPositionZ);
	}

	NATIVEDLL_API bool inline __cdecl RecastGetPath(unsigned int area, unsigned int layer, unsigned int startRef, unsigned int endRef,
		float startX, float startY, float startZ,
		float endX, float endY, float endZ,
		bool detailed, char* &data, int &size)
	{
		return RecastWrapper::Instance()->GetPath(area, layer, startRef, endRef, startX, startY, startZ, endX, endY, endZ, detailed, data, size);
	}
	
	NATIVEDLL_API bool inline __cdecl RecastAddObstacle(unsigned int area, unsigned int layer, float positionX, float positionY, float positionZ, float radius, float height, unsigned int &obstacleRef)
	{
		return RecastWrapper::Instance()->AddObstacle(area, layer, positionX, positionY, positionZ, radius, height, obstacleRef);
	}

	NATIVEDLL_API bool inline __cdecl RecastAddObstacleBox(unsigned int area, unsigned int layer, float minX, float minY, float minZ, float maxX, float maxY, float maxZ, unsigned int &obstacleRef)
	{
		return RecastWrapper::Instance()->AddObstacleBox(area, layer, minX, minY, minZ, maxX, maxY, maxZ, obstacleRef);
	}

	NATIVEDLL_API bool inline __cdecl RecastRemoveObstacle(unsigned int area, unsigned int layer, unsigned int obstacleRef)
	{
		return RecastWrapper::Instance()->RemoveObstacle(area, layer, obstacleRef);
	}

	NATIVEDLL_API bool inline __cdecl RecastClearObstacles(unsigned int area, unsigned int layer)
	{
		return RecastWrapper::Instance()->ClearObstacles(area, layer);
	}
}