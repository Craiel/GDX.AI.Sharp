#pragma once

#include <msclr\marshal_cppstd.h>

#include "RecastClient.h"

using namespace System::Collections;
using namespace System::Collections::Generic;

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
		ManagedRecastClient() { unmanaged = new RecastClient(); }
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
	};
}