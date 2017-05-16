#include "RecastClientTiled.h"

struct RasterizationContext
{
	RasterizationContext() :
		solid(0),
		triareas(0),
		lset(0),
		chf(0),
		ntiles(0)
	{
		memset(tiles, 0, sizeof(TileCacheData)*RecastWrapper::RecastClientTiled::MAX_LAYERS);
	}

	~RasterizationContext()
	{
		rcFreeHeightField(solid);
		delete[] triareas;
		rcFreeHeightfieldLayerSet(lset);
		rcFreeCompactHeightfield(chf);
		for (int i = 0; i < RecastWrapper::RecastClientTiled::MAX_LAYERS; ++i)
		{
			dtFree(tiles[i].data);
			tiles[i].data = 0;
		}
	}

	rcHeightfield* solid;
	unsigned char* triareas;
	rcHeightfieldLayerSet* lset;
	rcCompactHeightfield* chf;
	TileCacheData tiles[RecastWrapper::RecastClientTiled::MAX_LAYERS];
	int ntiles;
};

RecastWrapper::RecastClientTiled::RecastClientTiled() :
	m_tileCache(0)
{
	m_talloc = new LinearAllocator(32000);
	m_tcomp = new FastLZCompressor;
	m_tmproc = new MeshProcess;
}

RecastWrapper::RecastClientTiled::~RecastClientTiled()
{
}


void RecastWrapper::RecastClientTiled::buildStep1InitConfig()
{
	// Init cache
	int gw = 0, gh = 0;
	rcCalcGridSize(m_worldBoundsMin, m_worldBoundsMax, m_cellSize, &gw, &gh);
	const int ts = (int)m_tileSize;
	const int tw = (gw + ts - 1) / ts;
	const int th = (gh + ts - 1) / ts;

	// Init build configuration
	memset(&m_cfg, 0, sizeof(m_cfg));
	m_cfg.cs = m_cellSize;
	m_cfg.ch = m_cellHeight;
	m_cfg.walkableSlopeAngle = m_agentMaxSlope;
	m_cfg.walkableHeight = (int)ceilf(m_agentHeight / m_cfg.ch);
	m_cfg.walkableClimb = (int)floorf(m_agentMaxClimb / m_cfg.ch);
	m_cfg.walkableRadius = (int)ceilf(m_agentRadius / m_cfg.cs);
	m_cfg.maxEdgeLen = (int)(m_edgeMaxLen / m_cellSize);
	m_cfg.maxSimplificationError = m_edgeMaxError;
	m_cfg.minRegionArea = (int)rcSqr(m_regionMinSize);		// Note: area = size*size
	m_cfg.mergeRegionArea = (int)rcSqr(m_regionMergeSize);	// Note: area = size*size
	m_cfg.maxVertsPerPoly = (int)DT_VERTS_PER_POLYGON;
	m_cfg.tileSize = (int)m_tileSize;
	m_cfg.borderSize = m_cfg.walkableRadius + 3; // Reserve enough padding.
	m_cfg.width = m_cfg.tileSize + m_cfg.borderSize * 2;
	m_cfg.height = m_cfg.tileSize + m_cfg.borderSize * 2;
	m_cfg.detailSampleDist = m_detailSampleDist < 0.9f ? 0 : m_cellSize * m_detailSampleDist;
	m_cfg.detailSampleMaxError = m_cellHeight * m_detailSampleMaxError;
	rcVcopy(m_cfg.bmin, m_worldBoundsMin);
	rcVcopy(m_cfg.bmax, m_worldBoundsMax);

	// Tile cache params.
	memset(&m_tcparams, 0, sizeof(m_tcparams));
	rcVcopy(m_tcparams.orig, m_worldBoundsMin);
	m_tcparams.cs = m_cellSize;
	m_tcparams.ch = m_cellHeight;
	m_tcparams.width = (int)m_tileSize;
	m_tcparams.height = (int)m_tileSize;
	m_tcparams.walkableHeight = m_agentHeight;
	m_tcparams.walkableRadius = m_agentRadius;
	m_tcparams.walkableClimb = m_agentMaxClimb;
	m_tcparams.maxSimplificationError = m_edgeMaxError;
	m_tcparams.maxTiles = tw*th*EXPECTED_LAYERS_PER_TILE;
	m_tcparams.maxObstacles = m_maxObstables;

	// Max tiles and max polys affect how the tile IDs are caculated.
	// There are 22 bits available for identifying a tile and a polygon.
	int tileBits = rcMin((int)dtIlog2(dtNextPow2(tw*th*EXPECTED_LAYERS_PER_TILE)), 14);
	if (tileBits > 14) tileBits = 14;
	int polyBits = 22 - tileBits;
	m_maxTiles = 1 << tileBits;
	m_maxPolysPerTile = 1 << polyBits;
}

bool RecastWrapper::RecastClientTiled::prepareBuild()
{
	buildStep1InitConfig();
	return true;
}

bool RecastWrapper::RecastClientTiled::doBuild()
{
	dtStatus status;
	
	int gw = 0, gh = 0;
	rcCalcGridSize(m_worldBoundsMin, m_worldBoundsMax, m_cellSize, &gw, &gh);
	const int tw = (gw + (int)m_tileSize - 1) / (int)m_tileSize;
	const int th = (gh + (int)m_tileSize - 1) / (int)m_tileSize;

	if (!m_tileCache || !m_ctx->additive) {
		m_tileCache = dtAllocTileCache();
		if (!m_tileCache)
		{
			m_ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not allocate tile cache.");
			return false;
		}

		status = m_tileCache->init(&m_tcparams, m_talloc, m_tcomp, m_tmproc);
		if (dtStatusFailed(status))
		{
			m_ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not init tile cache.");
			return false;
		}

		dtFreeNavMesh(m_navMesh);

		m_navMesh = dtAllocNavMesh();
		if (!m_navMesh)
		{
			m_ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not allocate navmesh.");
			return false;
		}
		
		dtNavMeshParams params;
		memset(&params, 0, sizeof(params));
		rcVcopy(params.orig, m_worldBoundsMin);
		params.tileWidth = m_tileSize*m_cellSize;
		params.tileHeight = m_tileSize*m_cellSize;
		params.maxTiles = m_maxTiles;
		params.maxPolys = m_maxPolysPerTile;

		status = m_navMesh->init(&params);
		if (dtStatusFailed(status))
		{
			m_ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not init navmesh.");
			return false;
		}

		status = m_navQuery->init(m_navMesh, 2048);
		if (dtStatusFailed(status))
		{
			m_ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not init Detour navmesh query");
			return false;
		}
	}

	// Preprocess tiles.

	m_ctx->resetTimers();

	m_cacheLayerCount = 0;
	m_cacheCompressedSize = 0;
	m_cacheRawSize = 0;

	for (int y = 0; y < th; ++y)
	{
		for (int x = 0; x < tw; ++x)
		{
			TileCacheData tiles[MAX_LAYERS];
			memset(tiles, 0, sizeof(tiles));
			int ntiles = rasterizeTileLayers(x, y, tiles, MAX_LAYERS);

			for (int i = 0; i < ntiles; ++i)
			{
				TileCacheData* tile = &tiles[i];
				status = m_tileCache->addTile(tile->data, tile->dataSize, DT_COMPRESSEDTILE_FREE_DATA, 0);
				if (dtStatusFailed(status))
				{
					dtTileCacheLayerHeader* header = (dtTileCacheLayerHeader*)tile->data;
					m_ctx->log(RC_LOG_ERROR, "Tile (%d-%d @ %d-%d) was already occupied: x%.1f y%.1f z%.1f", header->width, header->height, header->tx, header->ty, header->bmin[0], header->bmin[1], header->bmin[2]);

					dtFree(tile->data);
					tile->data = 0;					
					continue;
				}

				m_cacheLayerCount++;
				m_cacheCompressedSize += tile->dataSize;
				m_cacheRawSize += calcLayerBufferSize(m_tcparams.width, m_tcparams.height);
			}
		}
	}

	// Build initial meshes
	m_ctx->startTimer(RC_TIMER_TOTAL);
	for (int y = 0; y < th; ++y)
		for (int x = 0; x < tw; ++x)
			m_tileCache->buildNavMeshTilesAt(x, y, m_navMesh);

	// Initialize crowd
	if (!m_crowd->init(m_maxAgents, m_agentRadius, m_navMesh))
	{
		m_ctx->log(RC_LOG_ERROR, "Coult nod init Crowd");
		return false;
	}

	return finalizeLoad(true);
}

bool RecastWrapper::RecastClientTiled::finalizeLoad(bool traceMemory)
{
	// Initialize crowd
	if (!m_crowd->init(m_maxAgents, m_agentRadius, m_navMesh))
	{
		m_ctx->log(RC_LOG_ERROR, "Coult nod init Crowd");
		return false;
	}

	if (traceMemory) {
		// Print the memory use of the nav mesh
		const dtNavMesh* nav = m_navMesh;
		int navmeshMemUsage = 0;
		for (int i = 0; i < nav->getMaxTiles(); ++i)
		{
			const dtMeshTile* tile = nav->getTile(i);
			if (tile->header)
				navmeshMemUsage += tile->dataSize;
		}

		m_ctx->log(RC_LOG_PROGRESS, "navmeshMemUsage = %.1f kB", navmeshMemUsage / 1024.0f);
	}

	return true;
}

int RecastWrapper::RecastClientTiled::rasterizeTileLayers(
	const int tx, const int ty,
	TileCacheData* tiles,
	const int maxTiles)
{
	if (!m_geom || !m_geom->getMesh() || !m_geom->getChunkyMesh())
	{
		m_ctx->log(RC_LOG_ERROR, "buildTile: Input mesh is not specified.");
		return 0;
	}

	FastLZCompressor comp;
	RasterizationContext rc;

	const float* verts = m_geom->getMesh()->getVerts();
	const int nverts = m_geom->getMesh()->getVertCount();
	const rcChunkyTriMesh* chunkyMesh = m_geom->getChunkyMesh();

	// Tile bounds.
	const float tcs = m_cfg.tileSize * m_cfg.cs;

	rcConfig tcfg;
	memcpy(&tcfg, &m_cfg, sizeof(tcfg));

	tcfg.bmin[0] = m_cfg.bmin[0] + tx*tcs;
	tcfg.bmin[1] = m_cfg.bmin[1];
	tcfg.bmin[2] = m_cfg.bmin[2] + ty*tcs;
	tcfg.bmax[0] = m_cfg.bmin[0] + (tx + 1)*tcs;
	tcfg.bmax[1] = m_cfg.bmax[1];
	tcfg.bmax[2] = m_cfg.bmin[2] + (ty + 1)*tcs;
	tcfg.bmin[0] -= tcfg.borderSize*tcfg.cs;
	tcfg.bmin[2] -= tcfg.borderSize*tcfg.cs;
	tcfg.bmax[0] += tcfg.borderSize*tcfg.cs;
	tcfg.bmax[2] += tcfg.borderSize*tcfg.cs;

	// Allocate voxel heightfield where we rasterize our input data to.
	rc.solid = rcAllocHeightfield();
	if (!rc.solid)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'solid'.");
		return 0;
	}
	if (!rcCreateHeightfield(m_ctx, *rc.solid, tcfg.width, tcfg.height, tcfg.bmin, tcfg.bmax, tcfg.cs, tcfg.ch))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not create solid heightfield.");
		return 0;
	}

	// Allocate array that can hold triangle flags.
	// If you have multiple meshes you need to process, allocate
	// and array which can hold the max number of triangles you need to process.
	rc.triareas = new unsigned char[chunkyMesh->maxTrisPerChunk];
	if (!rc.triareas)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'm_triareas' (%d).", chunkyMesh->maxTrisPerChunk);
		return 0;
	}

	float tbmin[2], tbmax[2];
	tbmin[0] = tcfg.bmin[0];
	tbmin[1] = tcfg.bmin[2];
	tbmax[0] = tcfg.bmax[0];
	tbmax[1] = tcfg.bmax[2];
	int cid[512];// TODO: Make grow when returning too many items.
	const int ncid = rcGetChunksOverlappingRect(chunkyMesh, tbmin, tbmax, cid, 512);
	if (!ncid)
	{
		return 0; // empty
	}

	for (int i = 0; i < ncid; ++i)
	{
		const rcChunkyTriMeshNode& node = chunkyMesh->nodes[cid[i]];
		const int* tris = &chunkyMesh->tris[node.i * 3];
		const int ntris = node.n;

		memset(rc.triareas, 0, ntris * sizeof(unsigned char));
		rcMarkWalkableTriangles(m_ctx, tcfg.walkableSlopeAngle,
			verts, nverts, tris, ntris, rc.triareas);

		if (!rcRasterizeTriangles(m_ctx, verts, nverts, tris, rc.triareas, ntris, *rc.solid, tcfg.walkableClimb))
			return 0;
	}

	// Once all geometry is rasterized, we do initial pass of filtering to
	// remove unwanted overhangs caused by the conservative rasterization
	// as well as filter spans where the character cannot possibly stand.
	if (m_filterLowHangingObstacles)
		rcFilterLowHangingWalkableObstacles(m_ctx, tcfg.walkableClimb, *rc.solid);
	if (m_filterLedgeSpans)
		rcFilterLedgeSpans(m_ctx, tcfg.walkableHeight, tcfg.walkableClimb, *rc.solid);
	if (m_filterWalkableLowHeightSpans)
		rcFilterWalkableLowHeightSpans(m_ctx, tcfg.walkableHeight, *rc.solid);


	rc.chf = rcAllocCompactHeightfield();
	if (!rc.chf)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'chf'.");
		return 0;
	}

	if (!rcBuildCompactHeightfield(m_ctx, tcfg.walkableHeight, tcfg.walkableClimb, *rc.solid, *rc.chf))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build compact data.");
		return 0;
	}

	// Erode the walkable area by agent radius.
	if (!rcErodeWalkableArea(m_ctx, tcfg.walkableRadius, *rc.chf))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not erode.");
		return 0;
	}

	// (Optional) Mark areas.
	const ConvexVolume* vols = m_geom->getConvexVolumes();
	for (int i = 0; i < m_geom->getConvexVolumeCount(); ++i)
	{
		rcMarkConvexPolyArea(m_ctx, vols[i].verts, vols[i].nverts,
			vols[i].hmin, vols[i].hmax,
			(unsigned char)vols[i].area, *rc.chf);
	}

	rc.lset = rcAllocHeightfieldLayerSet();
	if (!rc.lset)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'lset'.");
		return 0;
	}
	if (!rcBuildHeightfieldLayers(m_ctx, *rc.chf, tcfg.borderSize, tcfg.walkableHeight, *rc.lset))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build heighfield layers.");
		return 0;
	}

	rc.ntiles = 0;
	for (int i = 0; i < rcMin(rc.lset->nlayers, MAX_LAYERS); ++i)
	{
		TileCacheData* tile = &rc.tiles[rc.ntiles++];
		const rcHeightfieldLayer* layer = &rc.lset->layers[i];

		// Store header
		dtTileCacheLayerHeader header;
		header.magic = DT_TILECACHE_MAGIC;
		header.version = DT_TILECACHE_VERSION;

		// Tile layer location in the navmesh.
		header.tx = tx;
		header.ty = ty;
		header.tlayer = i;
		dtVcopy(header.bmin, layer->bmin);
		dtVcopy(header.bmax, layer->bmax);

		// Tile info.
		header.width = (unsigned char)layer->width;
		header.height = (unsigned char)layer->height;
		header.minx = (unsigned char)layer->minx;
		header.maxx = (unsigned char)layer->maxx;
		header.miny = (unsigned char)layer->miny;
		header.maxy = (unsigned char)layer->maxy;
		header.hmin = (unsigned short)layer->hmin;
		header.hmax = (unsigned short)layer->hmax;

		dtStatus status = dtBuildTileCacheLayer(&comp, &header, layer->heights, layer->areas, layer->cons,
			&tile->data, &tile->dataSize);
		if (dtStatusFailed(status))
		{
			return 0;
		}
	}

	// Transfer ownership of tile data from build context to the caller.
	int n = 0;
	for (int i = 0; i < rcMin(rc.ntiles, maxTiles); ++i)
	{
		tiles[n++] = rc.tiles[i];
		rc.tiles[i].data = 0;
		rc.tiles[i].dataSize = 0;
	}

	return n;
}

void RecastWrapper::RecastClientTiled::cleanup()
{
	dtFreeTileCache(m_tileCache);
}

bool RecastWrapper::RecastClientTiled::generate(std::string geom_path, bool additive)
{
	if(!additive)
	{
		cleanup();
	}

	m_geom = new InputGeom();
	m_geom->load(m_ctx, geom_path);

	m_ctx = new BuildContext();
	m_ctx->additive = additive;
	
	prepareBuild();

	// Reset build times gathering.
	m_ctx->resetTimers();

	// Start the build process.	
	m_ctx->startTimer(RC_TIMER_TOTAL);

	m_ctx->log(RC_LOG_PROGRESS, "Building navigation:");
	m_ctx->log(RC_LOG_PROGRESS, " - %d x %d cells", m_cfg.width, m_cfg.height);
	m_ctx->log(RC_LOG_PROGRESS, " - %.1fK verts, %.1fK tris", m_geom->getMesh()->getVertCount() / 1000.0f, m_geom->getMesh()->getTriCount() / 1000.0f);

	doBuild();

	m_ctx->stopTimer(RC_TIMER_TOTAL);

	return true;
}

bool RecastWrapper::RecastClientTiled::save(GDX::AI::ProtoRecastTiledNavMesh* proto)
{
	int tileCount = 0;
	for (int i = 0; i < m_tileCache->getTileCount(); ++i)
	{
		const dtCompressedTile* tile = m_tileCache->getTile(i);
		if (!tile || !tile->header || !tile->dataSize) continue;
		tileCount++;
	}

	proto->set_tile_count(tileCount);

	dtNavMeshParams meshParams;
	dtTileCacheParams cacheParams;
	memcpy(&meshParams, m_navMesh->getParams(), sizeof(dtNavMeshParams));
	memcpy(&cacheParams, m_tileCache->getParams(), sizeof(dtTileCacheParams));

	proto->set_nav_mesh_params(reinterpret_cast<char*>(&meshParams), sizeof(dtNavMeshParams));
	proto->set_tile_cache_params(reinterpret_cast<char*>(&cacheParams), sizeof(dtTileCacheParams));

	for(int i = 0; i < tileCount; i++)
	{
		const dtCompressedTile* tile = m_tileCache->getTile(i);
		if (!tile || !tile->header || !tile->dataSize) continue;

		int dataSize = tile->dataSize;

		GDX::AI::ProtoRecastTile* protoTile = proto->add_tiles();
		protoTile->set_compressed_tile_ref(m_tileCache->getTileRef(tile));

		unsigned char* buffer = (unsigned char*)malloc(dataSize);
		memcpy(buffer, tile->data, dataSize);
		protoTile->set_tile_data(buffer, dataSize);
	}

	return true;
}

bool RecastWrapper::RecastClientTiled::load(GDX::AI::ProtoRecastTiledNavMesh* proto, bool additive)
{
	if (!m_navMesh || !additive) {
		dtFreeNavMesh(m_navMesh);
		dtFreeTileCache(m_tileCache);

		m_navMesh = dtAllocNavMesh();
		if (!m_navMesh)
		{
			return false;
		}

		dtNavMeshParams meshParams;
		dtTileCacheParams cacheParams;
		std::string navMeshParamData = proto->nav_mesh_params();
		std::string cacheParamData = proto->tile_cache_params();
		memcpy(&meshParams, &navMeshParamData[0], sizeof(dtNavMeshParams));
		memcpy(&cacheParams, &cacheParamData[0], sizeof(dtTileCacheParams));

		dtStatus status = m_navMesh->init(&meshParams);
		if (dtStatusFailed(status))
		{
			return false;
		}

		m_tileCache = dtAllocTileCache();
		if (!m_tileCache)
		{
			return false;
		}

		status = m_tileCache->init(&cacheParams, m_talloc, m_tcomp, m_tmproc);
		if (dtStatusFailed(status))
		{
			return false;
		}
	}

	// Read tiles
	for (int i = 0; i < proto->tile_count(); ++i)
	{
		GDX::AI::ProtoRecastTile protoTile = proto->tiles().Get(i);
		std::string dataString = protoTile.tile_data();
		int dataSize = dataString.length();

		unsigned char* data = (unsigned char*)dtAlloc(dataSize, DT_ALLOC_PERM);
		if (!data) break;
		copy(dataString.begin(), dataString.end(), data);
				
		dtCompressedTileRef tile = 0;
		dtStatus addTileStatus = m_tileCache->addTile(data, dataSize, DT_COMPRESSEDTILE_FREE_DATA, &tile);
		if (dtStatusFailed(addTileStatus))
		{
			dtFree(data);
			continue;
		}

		if (!tile)
		{
			dtFree(data);
			return false;
		}
	}

	rebuildTiles();
	
	dtStatus status = m_navQuery->init(m_navMesh, 2048);
	if (dtStatusFailed(status))
	{
		m_ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not init Detour navmesh query");
		return false;
	}

	return finalizeLoad(false);
}

void RecastWrapper::RecastClientTiled::rebuildTiles()
{
	int count = m_tileCache->getTileCount();
	for(int i = 0; i < count; i++)
	{
		const dtCompressedTile* tile = m_tileCache->getTile(i);
		
		dtCompressedTileRef tileRef = m_tileCache->getTileRef(tile);
		m_tileCache->buildNavMeshTile(tileRef, m_navMesh);
	}
}

dtStatus RecastWrapper::RecastClientTiled::addObstacle(const float* pos, float radius, float height, dtObstacleRef* ref)
{
	return m_tileCache->addObstacle(pos, radius, height, ref);
}

dtStatus RecastWrapper::RecastClientTiled::addObstacleBox(const float* bmin, const float* bmax, dtObstacleRef* ref)
{
	return m_tileCache->addBoxObstacle(bmin, bmax, ref);
}

dtStatus RecastWrapper::RecastClientTiled::removeObstacle(dtObstacleRef ref)
{
	return m_tileCache->removeObstacle(ref);
}

void RecastWrapper::RecastClientTiled::clearObstacles()
{
	for (int i = 0; i < m_tileCache->getObstacleCount(); ++i)
	{
		const dtTileCacheObstacle* ob = m_tileCache->getObstacle(i);
		if (ob->state == DT_OBSTACLE_EMPTY) continue;
		m_tileCache->removeObstacle(m_tileCache->getObstacleRef(ob));
	}
}

bool RecastWrapper::RecastClientTiled::getDebugNavMesh(const unsigned short polyFlags, GDX::AI::ProtoRecastDebugNavMesh* proto)
{
	if (!m_navMesh)
	{
		return false;
	}

	const dtNavMesh& mesh = *m_navMesh;
	for (int i = 0; i < mesh.getMaxTiles(); ++i)
	{
		const dtMeshTile* tile = mesh.getTile(i);
		if (!tile->header) continue;
		dtPolyRef base = m_navMesh->getPolyRefBase(tile);

		GDX::AI::ProtoRecastDebugNavMeshTile* protoTile = proto->add_tiles();

		GDX::AI::ProtoNavMeshVector* bbmin = protoTile->add_bounds();
		bbmin->set_x(tile->header->bmin[0]);
		bbmin->set_y(tile->header->bmin[1]);
		bbmin->set_z(tile->header->bmin[2]);

		GDX::AI::ProtoNavMeshVector* bbmax = protoTile->add_bounds();
		bbmax->set_x(tile->header->bmax[0]);
		bbmax->set_y(tile->header->bmax[1]);
		bbmax->set_z(tile->header->bmax[2]);

		protoTile->set_tx(tile->header->x);
		protoTile->set_ty(tile->header->y);
				
		int tileVerts = tile->header->vertCount;
		for (int i = 0; i < tileVerts; i++)
		{
			float* vert = &tile->verts[i * 3];
			GDX::AI::ProtoNavMeshVector* vertex = protoTile->add_vertices();
			vertex->set_x(vert[0]);
			vertex->set_y(vert[1]);
			vertex->set_z(vert[2]);
		}

		for (int j = 0; j < tile->header->polyCount; ++j)
		{
			const dtPoly* p = &tile->polys[j];
			if ((p->flags & polyFlags) != 0) continue;

			const dtMeshTile* tile = 0;
			const dtPoly* poly = 0;
			if (dtStatusFailed(m_navMesh->getTileAndPolyByRef(base | (dtPolyRef)j, &tile, &poly)))
			{
				return false;
			}

			const unsigned int ip = (unsigned int)(poly - tile->polys);

			if (poly->getType() == DT_POLYTYPE_OFFMESH_CONNECTION)
			{
				dtOffMeshConnection* con = &tile->offMeshCons[ip - tile->header->offMeshBase];

				GDX::AI::ProtoRecastDebugNavMeshOffMeshConnection* connection = proto->add_off_mesh_connections();

				connection->set_flags(con->flags);

				GDX::AI::ProtoNavMeshVector* point_0 = new GDX::AI::ProtoNavMeshVector();
				point_0->set_x(con->pos[0]);
				point_0->set_y(con->pos[1]);
				point_0->set_z(con->pos[2]);
				connection->set_allocated_p_0(point_0);

				GDX::AI::ProtoNavMeshVector* point_1 = new GDX::AI::ProtoNavMeshVector();
				point_1->set_x(con->pos[3]);
				point_1->set_y(con->pos[4]);
				point_1->set_z(con->pos[5]);
				connection->set_allocated_p_1(point_1);
			}
			else
			{
				const dtPolyDetail* pd = &tile->detailMeshes[ip];
				for (int i = 0; i < pd->triCount; ++i)
				{
					const unsigned char* t = &tile->detailTris[(pd->triBase + i) * 4];
					//GDX::AI::ProtoRecastDebugNavMeshTriangle* triangle = proto->add_triangles();
					unsigned short tri[3];
					for (int j = 0; j < 3; ++j)
					{
						if (t[j] < poly->vertCount)
							tri[j] = poly->verts[t[j]];
						else
							tri[j] = (pd->vertBase + t[j] - poly->vertCount) * 3;
					}

					GDX::AI::ProtoNavMeshTriangle* triangle = protoTile->add_triangles();
					triangle->set_x(tri[0]);
					triangle->set_y(tri[1]);
					triangle->set_z(tri[2]);
				}
			}
		}
	}

	return true;
}