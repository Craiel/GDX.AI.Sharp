#pragma once

#include <msclr\marshal_cppstd.h>

#include "RecastClientTiled.h"
#include "RecastClientSoloMesh.h"
#include "ManagedDtCrowdAgentInfo.h"
#include "ManagedDtCrowdAgentParams.h"

using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

namespace RecastWrapper
{
	public ref class ManagedRecastClient
	{
	private:
		RecastClient* unmanaged;

		static void logLine(rcContext& ctx, rcTimerLabel label, const char* name, const float pc)
		{
			const int t = ctx.getAccumulatedTime(label);
			if (t < 0) return;
			ctx.log(RC_LOG_PROGRESS, "%s:\t%.2fms\t(%.1f%%)", name, t / 1000.0f, t*pc);
		}

	public:
		ManagedRecastClient(bool soloMesh)
		{
			if (soloMesh) {
				unmanaged = new RecastClientSoloMesh();
			} else
			{
				unmanaged = new RecastClientTiled();
			}
		}

		~ManagedRecastClient() { delete unmanaged; }
	internal:
		RecastClient* GetUnmanaged() { return unmanaged; }

	public:
		bool Load(String^ path) {
			std::string unmanagedPath = msclr::interop::marshal_as<std::string>(path);
			return unmanaged->build(unmanagedPath);
		}

		void LogBuildTimes()
		{
			BuildContext* context = unmanaged->getContext();

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
		}

		List<String^>^ GetLogText()
		{
			List<String^>^ result = gcnew List<String^>();
			BuildContext* context = unmanaged->getContext();
			for(int i = 0; i < context->getLogCount(); i++)
			{
				const char* text = context->getLogText(i);
				result->Add(gcnew String(text));

			}

			return result;
		}

		void Update(float delta)
		{
			unmanaged->update(delta);
		}

		bool FindRandomPointAroundCircle(unsigned int% startRef, array<float>^ centerPosition, float maxRadius, [Out] unsigned int% randomRef, [Out] array<float>^% randomPoint)
		{
			dtPolyRef startRefLocal = startRef;
			dtPolyRef randomRefLocal;
			float pt[3];
			pin_ptr<float> center_start = &centerPosition[0];
			dtStatus status = unmanaged->findRandomPointAroundCircle(startRefLocal, center_start, maxRadius, &randomRefLocal, pt);
			startRef = startRefLocal;
			randomRef = randomRefLocal;
			randomPoint = gcnew array<float>(3);
			randomPoint[0] = pt[0];
			randomPoint[1] = pt[1];
			randomPoint[2] = pt[2];

			return dtStatusSucceed(status);
		}

		bool FindNearestPoly(array<float>^ center, array<float>^ extents, [Out] unsigned int% nearestPolyRef, [Out] array<float>^% nearestPoint)
		{
			pin_ptr<float> center_start = &center[0];
			pin_ptr<float> extents_start = &extents[0];
			dtPolyRef nearestPoly;
			float pt[3];
			dtStatus status = unmanaged->findNearestPoly(center_start, extents_start, &nearestPoly, pt);
			nearestPolyRef = nearestPoly;
			if (nearestPoly != 0) {
				nearestPoint = gcnew array<float>(3);
				nearestPoint[0] = pt[0];
				nearestPoint[1] = pt[1];
				nearestPoint[2] = pt[2];
			}

			return dtStatusSucceed(status);
		}

		int AddAgent(array<float>^ position, ManagedDtCrowdAgentParams^ params)
		{
			pin_ptr<float> position_start = &position[0];
			return unmanaged->addAgent(position_start, &params->GetUnmanaged());
		}

		bool RequestMoveTarget(int index, unsigned int polyRef, array<float>^ position)
		{
			pin_ptr<float> position_start = &position[0];
			return unmanaged->requestMoveTarget(index, polyRef, position_start);
		}

		bool ResetMoveTarget(int index)
		{
			return unmanaged->resetMoveTarget(index);
		}

		void GetAgentInfo(int index, [Out] ManagedDtCrowdAgentInfo^% agentInfo)
		{
			const dtCrowdAgent* agent = unmanaged->getAgent(index);

			agentInfo = gcnew ManagedDtCrowdAgentInfo();
			agentInfo->npos[0] = agent->npos[0];
			agentInfo->npos[1] = agent->npos[1];
			agentInfo->npos[2] = agent->npos[2];
			agentInfo->vel[0] = agent->vel[0];
			agentInfo->vel[1] = agent->vel[1];
			agentInfo->vel[2] = agent->vel[2];

			agentInfo->targetPos[0] = agent->targetPos[0];
			agentInfo->targetPos[1] = agent->targetPos[1];
			agentInfo->targetPos[2] = agent->targetPos[2];
			agentInfo->targetState = agent->targetState;
		}
	};
}