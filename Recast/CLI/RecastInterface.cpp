#include "RecastInterface.h"
#include "RecastClientTiled.h"
#include "RecastClientSoloMesh.h"

RecastWrapper::RecastInterface::RecastInterface() :
	clients(nullptr),
	isTiled(false)
{	
}

bool RecastWrapper::RecastInterface::Initialize(unsigned int area, unsigned int layer, bool tiled)
{
	isTiled = tiled;
	if (isTiled) {
		clients[area][layer] = new RecastClientTiled();
	} else
	{
		clients[area][layer] = new RecastClientSoloMesh();
	}

	return true;
}

bool RecastWrapper::RecastInterface::Destroy(unsigned int area, unsigned int layer) const
{
	if(clients[area][layer] == nullptr)
	{
		return false;
	}

	delete clients[area][layer];
	return true;
}

bool RecastWrapper::RecastInterface::Update(unsigned int area, unsigned int layer, float delta) const
{
	dtStatus status = clients[area][layer]->update(delta);
	return dtStatusFailed(status);
}

bool RecastWrapper::RecastInterface::LoadSettings(unsigned int area, unsigned int layer, char* data, int size) const
{
	GOOGLE_PROTOBUF_VERIFY_VERSION;
	
	GDX::AI::ProtoRecastSettings* proto = new GDX::AI::ProtoRecastSettings();
	if (!proto->ParseFromArray(data, size))
	{
		return false;
	}

	return clients[area][layer]->configure(proto);
}

bool RecastWrapper::RecastInterface::Build(unsigned int area, unsigned int layer, char* data, int size) const
{
	if (isTiled)
	{
		RecastClientTiled* typed = static_cast<RecastClientTiled*>(clients[area][layer]);

		std::string path(data);
		return typed->generate(path);
	}
	
	RecastClientSoloMesh* typed = static_cast<RecastClientSoloMesh*>(clients[area][layer]);

	std::string path(data);
	return typed->build(path);
}

bool RecastWrapper::RecastInterface::Load(unsigned int area, unsigned int layer, char* data, int size) const
{
	if (!isTiled)
	{
		return false;
	}

	RecastClientTiled* typed = static_cast<RecastClientTiled*>(clients[area][layer]);

	GOOGLE_PROTOBUF_VERIFY_VERSION;

	GDX::AI::ProtoRecastTiledNavMesh* proto = new GDX::AI::ProtoRecastTiledNavMesh();
	if (!proto->ParseFromArray(data, size))
	{
		return false;
	}

	return typed->load(proto);
}

bool RecastWrapper::RecastInterface::Save(unsigned int area, unsigned int layer, char* &data, int &size) const
{
	if (!isTiled)
	{
		return false;
	}

	RecastClientTiled* typed = static_cast<RecastClientTiled*>(clients[area][layer]);

	GOOGLE_PROTOBUF_VERIFY_VERSION;

	GDX::AI::ProtoRecastTiledNavMesh* proto = new GDX::AI::ProtoRecastTiledNavMesh();
	if (!typed->save(proto))
	{
		delete proto;
		return false;
	}

	uint32_t byteSize = proto->ByteSize();
	data = static_cast<char*>(malloc(byteSize));
	proto->SerializeToArray(data, byteSize);

	size = byteSize;

	// clean up
	delete proto;
	google::protobuf::ShutdownProtobufLibrary();

	return true;
}

bool RecastWrapper::RecastInterface::GetDebugNavMesh(unsigned int area, unsigned int layer, char* &data, int &size) const
{
	if (!isTiled)
	{
		return false;
	}

	RecastClientTiled* typed = static_cast<RecastClientTiled*>(clients[area][layer]);

	GOOGLE_PROTOBUF_VERIFY_VERSION;

	GDX::AI::ProtoRecastDebugNavMesh* proto = new GDX::AI::ProtoRecastDebugNavMesh();
	if (!typed->getDebugNavMesh(POLYFLAGS_DISABLED, proto))
	{
		delete proto;
		return false;
	}

	uint32_t byteSize = proto->ByteSize();
	data = static_cast<char*>(malloc(byteSize));
	proto->SerializeToArray(data, byteSize);

	size = byteSize;

	// clean up
	delete proto;
	google::protobuf::ShutdownProtobufLibrary();

	return true;
}

static void logLine(rcContext& ctx, rcTimerLabel label, const char* name, const float pc)
{
	const int t = ctx.getAccumulatedTime(label);
	if (t < 0) return;
	ctx.log(RC_LOG_PROGRESS, "%s:\t%.2fms\t(%.1f%%)", name, t / 1000.0f, t*pc);
}

bool RecastWrapper::RecastInterface::LogBuildTimes(unsigned int area, unsigned int layer) const
{
	BuildContext* context = clients[area][layer]->getContext();

	int totalTime = context->getAccumulatedTime(RC_TIMER_TOTAL);
	const float pc = 100.0f / totalTime;
	
	context->log(RC_LOG_PROGRESS, "Build Times");
	logLine(*context, RC_TIMER_RASTERIZE_TRIANGLES, "- Rasterize", pc);
	logLine(*context, RC_TIMER_BUILD_COMPACTHEIGHTFIELD, "- Build Compact", pc);
	logLine(*context, RC_TIMER_FILTER_BORDER, "- Filter Border", pc);
	logLine(*context, RC_TIMER_FILTER_WALKABLE, "- Filter Walkable", pc);
	logLine(*context, RC_TIMER_ERODE_AREA, "- Erode Area", pc);
	logLine(*context, RC_TIMER_MEDIAN_AREA, "- Median Area", pc);
	logLine(*context, RC_TIMER_MARK_BOX_AREA, "- Mark Box Area", pc);
	logLine(*context, RC_TIMER_MARK_CONVEXPOLY_AREA, "- Mark Convex Area", pc);
	logLine(*context, RC_TIMER_MARK_CYLINDER_AREA, "- Mark Cylinder Area", pc);
	logLine(*context, RC_TIMER_BUILD_DISTANCEFIELD, "- Build Distance Field", pc);
	logLine(*context, RC_TIMER_BUILD_DISTANCEFIELD_DIST, "    - Distance", pc);
	logLine(*context, RC_TIMER_BUILD_DISTANCEFIELD_BLUR, "    - Blur", pc);
	logLine(*context, RC_TIMER_BUILD_REGIONS, "- Build Regions", pc);
	logLine(*context, RC_TIMER_BUILD_REGIONS_WATERSHED, "    - Watershed", pc);
	logLine(*context, RC_TIMER_BUILD_REGIONS_EXPAND, "      - Expand", pc);
	logLine(*context, RC_TIMER_BUILD_REGIONS_FLOOD, "      - Find Basins", pc);
	logLine(*context, RC_TIMER_BUILD_REGIONS_FILTER, "    - Filter", pc);
	logLine(*context, RC_TIMER_BUILD_LAYERS, "- Build Layers", pc);
	logLine(*context, RC_TIMER_BUILD_CONTOURS, "- Build Contours", pc);
	logLine(*context, RC_TIMER_BUILD_CONTOURS_TRACE, "    - Trace", pc);
	logLine(*context, RC_TIMER_BUILD_CONTOURS_SIMPLIFY, "    - Simplify", pc);
	logLine(*context, RC_TIMER_BUILD_POLYMESH, "- Build Polymesh", pc);
	logLine(*context, RC_TIMER_BUILD_POLYMESHDETAIL, "- Build Polymesh Detail", pc);
	logLine(*context, RC_TIMER_MERGE_POLYMESH, "- Merge Polymeshes", pc);
	logLine(*context, RC_TIMER_MERGE_POLYMESHDETAIL, "- Merge Polymesh Details", pc);
	context->log(RC_LOG_PROGRESS, "=== TOTAL:\t%.2fms", totalTime / 1000.0f);

	return true;
}

bool RecastWrapper::RecastInterface::GetLog(unsigned int area, unsigned int layer, char* &data, int &size) const
{
	GOOGLE_PROTOBUF_VERIFY_VERSION;

	GDX::AI::ProtoRecastLog* proto = new GDX::AI::ProtoRecastLog();

	BuildContext* context = clients[area][layer]->getContext();
	for (int i = 0; i < context->getLogCount(); i++)
	{
		const char* text = context->getLogText(i);
		proto->add_messages(text);
	}

	uint32_t byteSize = proto->ByteSize();
	data = static_cast<char*>(malloc(byteSize));
	proto->SerializeToArray(data, byteSize);

	size = byteSize;

	// clean up
	delete proto;
	google::protobuf::ShutdownProtobufLibrary();

	return true;
}

static void GetParamsFromProto(GDX::AI::ProtoCrowdAgentParameters* proto, dtCrowdAgentParams* &params)
{
	params->radius = proto->radius();
	params->height = proto->height();
	params->maxAcceleration = proto->max_acceleration();
	params->maxSpeed = proto->max_speed();
	params->collisionQueryRange = proto->collision_query_range();
	params->pathOptimizationRange = proto->path_optimization_range();
	params->separationWeight = proto->separation_weight();
	params->obstacleAvoidanceType = proto->obstacle_avoidance_type();
	params->queryFilterType = proto->query_filter_type();
	params->updateFlags = proto->update_flags();
}

int RecastWrapper::RecastInterface::AddAgent(unsigned int area, unsigned int layer, char* data, int size) const
{
	GOOGLE_PROTOBUF_VERIFY_VERSION;

	GDX::AI::ProtoCrowdAgentParameters* proto = new GDX::AI::ProtoCrowdAgentParameters();
	if (!proto->ParseFromArray(data, size))
	{
		return false;
	}

	dtCrowdAgentParams* params = new dtCrowdAgentParams();
	GetParamsFromProto(proto, params);

	float initialPos[3] = { proto->initial_position().x(), proto->initial_position().y(), proto->initial_position().z() };

	int id = clients[area][layer]->addAgent(initialPos, params);
	delete params;

	return id;
}

bool RecastWrapper::RecastInterface::UpdateAgent(unsigned int area, unsigned int layer, int index, char* data, int size) const
{
	GOOGLE_PROTOBUF_VERIFY_VERSION;

	GDX::AI::ProtoCrowdAgentParameters* proto = new GDX::AI::ProtoCrowdAgentParameters();
	if (!proto->ParseFromArray(data, size))
	{
		return false;
	}

	dtCrowdAgentParams* params = new dtCrowdAgentParams();
	GetParamsFromProto(proto, params);
	clients[area][layer]->updateAgent(index, params);
	delete params;

	return true;
}

bool RecastWrapper::RecastInterface::RemoveAgent(unsigned int area, unsigned int layer, int index) const
{
	clients[area][layer]->removeAgent(index);
	return true;
}

bool RecastWrapper::RecastInterface::RequestMoveTarget(unsigned int area, unsigned int layer, int index, unsigned int polyRef, float x, float y, float z) const
{
	float pos[3] = { x, y, z };
	return clients[area][layer]->requestMoveTarget(index, polyRef, pos);
}

bool RecastWrapper::RecastInterface::ResetMoveTarget(unsigned int area, unsigned int layer, int index) const
{
	return clients[area][layer]->resetMoveTarget(index);
}

bool RecastWrapper::RecastInterface::FindNearestPoly(unsigned int area, unsigned int layer, float centerX, float centerY, float centerZ, float extendX, float extendY, float extendZ, unsigned int &nearestRef, float &nearestX, float &nearestY, float &nearestZ) const
{
	float center[3] = { centerX, centerY, centerZ };
	float extends[3] = { extendX, extendY, extendZ };
	float point[3] = {};
	dtPolyRef ref;
	dtStatus status = clients[area][layer]->findNearestPoly(center, extends, &ref, point);
	nearestRef = ref;
	if (ref != 0) {
		nearestX = point[0];
		nearestY = point[1];
		nearestZ = point[2];
	}

	return dtStatusSucceed(status);
}

bool RecastWrapper::RecastInterface::GetAgentInfo(unsigned int area, unsigned int layer, int index, char* &data, int &size) const
{
	const dtCrowdAgent* agent = clients[area][layer]->getAgent(index);

	GOOGLE_PROTOBUF_VERIFY_VERSION;

	GDX::AI::ProtoCrowdAgentInfo* proto = new GDX::AI::ProtoCrowdAgentInfo();
		
	GDX::AI::ProtoNavMeshVector* position = new GDX::AI::ProtoNavMeshVector();
	position->set_x(agent->npos[0]);
	position->set_y(agent->npos[1]);
	position->set_z(agent->npos[2]);
	proto->set_allocated_position(position);

	GDX::AI::ProtoNavMeshVector* velocity = new GDX::AI::ProtoNavMeshVector();
	velocity->set_x(agent->vel[0]);
	velocity->set_y(agent->vel[1]);
	velocity->set_z(agent->vel[2]);
	proto->set_allocated_velocity(velocity);

	GDX::AI::ProtoNavMeshVector* targetPosition = new GDX::AI::ProtoNavMeshVector();
	targetPosition->set_x(agent->targetPos[0]);
	targetPosition->set_y(agent->targetPos[1]);
	targetPosition->set_z(agent->targetPos[2]);
	proto->set_allocated_target_position(targetPosition);

	proto->set_targetstate(agent->targetState);

	uint32_t byteSize = proto->ByteSize();
	data = static_cast<char*>(malloc(byteSize));
	proto->SerializeToArray(data, byteSize);

	size = byteSize;

	// clean up
	delete proto;
	google::protobuf::ShutdownProtobufLibrary();

	return true;
}

bool RecastWrapper::RecastInterface::FindRandomPointAroundCircle(unsigned int area, unsigned int layer, unsigned int startRef, float centerX, float centerY,
	float centerZ, float maxRadius, unsigned int &randomRef, float &randomPositionX, float &randomPositionY,
	float &randomPositionZ) const
{
	float center[3] = { centerX, centerY, centerZ };

	float point[3] = {};
	dtPolyRef ref;
	dtStatus status = clients[area][layer]->findRandomPointAroundCircle(startRef, center, maxRadius, &ref, point);
	randomRef = ref;
	if (ref != 0) {
		randomPositionX = point[0];
		randomPositionY = point[1];
		randomPositionZ = point[2];
	}

	return dtStatusSucceed(status);
}

bool RecastWrapper::RecastInterface::GetPath(unsigned int area, unsigned int layer, unsigned int startRef, unsigned int endRef,
	float startX, float startY, float startZ,
	float endX, float endY, float endZ,
	bool detailed, char* &data, int &size) const
{
	GOOGLE_PROTOBUF_VERIFY_VERSION;

	GDX::AI::ProtoRecastPathInfo* proto = new GDX::AI::ProtoRecastPathInfo();

	float start[3] = { startX, startY, startZ };
	float end[3] = { endX, endY, endZ };
	dtPolyRef path[RecastClient::MAX_PATH_POLYS];
	int pathCount;
	dtStatus status = clients[area][layer]->findPath(startRef, endRef, start, end, path, &pathCount);
	if(!dtStatusSucceed(status))
	{
		return false;
	}

	for (int i = 0; i < pathCount; i++)
	{
		proto->add_path_refs(path[i]);
	}

	if (detailed && pathCount > 0) {
		float pathPointData[RecastClient::MAX_PATH_SMOOTH * 3];
		int smoothPathPointCount;
		dtStatus detailStatus = clients[area][layer]->getSmoothPath(startRef, start, end, path, pathCount, pathPointData, &smoothPathPointCount);
				if(!dtStatusSucceed(detailStatus))
		{
			return false;
		}

		for (int i = 0; i < smoothPathPointCount * 3; i++)
		{
			proto->add_path_details(pathPointData[i]);
		}
	}
	
	uint32_t byteSize = proto->ByteSize();
	data = static_cast<char*>(malloc(byteSize));
	proto->SerializeToArray(data, byteSize);

	size = byteSize;

	// clean up
	delete proto;
	google::protobuf::ShutdownProtobufLibrary();

	return true;
}

bool RecastWrapper::RecastInterface::AddObstacle(unsigned int area, unsigned int layer, float positionX, float positionY, float positionZ, float radius, float height, unsigned int &obstacleRef) const
{
	float pos[3] = { positionX, positionY, positionZ };
	dtObstacleRef localRef;
	dtStatus status = clients[area][layer]->addObstacle(pos, radius, height, &localRef);
	obstacleRef = localRef;
	return dtStatusSucceed(status);
}

bool RecastWrapper::RecastInterface::AddObstacleBox(unsigned int area, unsigned int layer, float minX, float minY, float minZ, float maxX, float maxY, float maxZ, unsigned int &obstacleRef) const
{
	float min[3] = { minX, minY, minZ };
	float max[3] = { maxX, maxY, maxZ };
	dtObstacleRef localRef;
	dtStatus status = clients[area][layer]->addObstacleBox(min, max, &localRef);
	obstacleRef = localRef;
	return dtStatusSucceed(status);
}

bool RecastWrapper::RecastInterface::RemoveObstacle(unsigned int area, unsigned int layer, unsigned int obstacleRef) const
{
	dtStatus status = clients[area][layer]->removeObstacle(obstacleRef);
	return dtStatusSucceed(status);
}

bool RecastWrapper::RecastInterface::ClearObstacles(unsigned int area, unsigned int layer) const
{
	clients[area][layer]->clearObstacles();
	return true;
}