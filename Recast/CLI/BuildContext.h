#pragma once

#include "PerfTimer.h"
#include "ManagedRcContext.h"

using namespace System;

namespace RecastWrapper {

	class BuildContext : public rcContext
	{
		TimeVal m_startTime[RC_MAX_TIMERS];
		TimeVal m_accTime[RC_MAX_TIMERS];

		static const int MAX_MESSAGES = 1000;
		const char* m_messages[MAX_MESSAGES];
		int m_messageCount;
		static const int TEXT_POOL_SIZE = 8000;
		char m_textPool[TEXT_POOL_SIZE];
		int m_textPoolSize;

	public:
		BuildContext();

		/// Returns number of log messages.
		int getLogCount() const;
		/// Returns log message text.
		const char* getLogText(const int i) const;

	protected:
		/// Virtual functions for custom implementations.
		///@{
		virtual void doResetLog();
		virtual void doLog(const rcLogCategory category, const char* msg, const int len);
		virtual void doResetTimers();
		virtual void doStartTimer(const rcTimerLabel label);
		virtual void doStopTimer(const rcTimerLabel label);
		virtual int doGetAccumulatedTime(const rcTimerLabel label) const;
		///@}
	};

	public ref class ManagedBuildContext : ManagedRcContext
	{
	public:
		ManagedBuildContext() : ManagedRcContext(new BuildContext()) {}

		int getLogCount() { return ((BuildContext*)unmanaged)->getLogCount(); }
		String^ getLogText(int index)
		{
			const char* text = ((BuildContext*)unmanaged)->getLogText(index);
			return gcnew String(text);
		}

		void ResetTimers()
		{
			((BuildContext*)unmanaged)->resetTimers();
		}

		void StartTimer()
		{
			((BuildContext*)unmanaged)->startTimer(RC_MAX_TIMERS);
		}

		void StopTimer()
		{
			((BuildContext*)unmanaged)->stopTimer(RC_TIMER_TOTAL);
		}
		
		void LogBuildTimes()
		{
			int totalTime = ((BuildContext*)unmanaged)->getAccumulatedTime(RC_TIMER_TOTAL);
			const float pc = 100.0f / totalTime;

			unmanaged->log(RC_LOG_PROGRESS, "Build Times");
			logLine(*unmanaged, RC_TIMER_RASTERIZE_TRIANGLES, "- Rasterize", pc);
			logLine(*unmanaged, RC_TIMER_BUILD_COMPACTHEIGHTFIELD, "- Build Compact", pc);
			logLine(*unmanaged, RC_TIMER_FILTER_BORDER, "- Filter Border", pc);
			logLine(*unmanaged, RC_TIMER_FILTER_WALKABLE, "- Filter Walkable", pc);
			logLine(*unmanaged, RC_TIMER_ERODE_AREA, "- Erode Area", pc);
			logLine(*unmanaged, RC_TIMER_MEDIAN_AREA, "- Median Area", pc);
			logLine(*unmanaged, RC_TIMER_MARK_BOX_AREA, "- Mark Box Area", pc);
			logLine(*unmanaged, RC_TIMER_MARK_CONVEXPOLY_AREA, "- Mark Convex Area", pc);
			logLine(*unmanaged, RC_TIMER_MARK_CYLINDER_AREA, "- Mark Cylinder Area", pc);
			logLine(*unmanaged, RC_TIMER_BUILD_DISTANCEFIELD, "- Build Distance Field", pc);
			logLine(*unmanaged, RC_TIMER_BUILD_DISTANCEFIELD_DIST, "    - Distance", pc);
			logLine(*unmanaged, RC_TIMER_BUILD_DISTANCEFIELD_BLUR, "    - Blur", pc);
			logLine(*unmanaged, RC_TIMER_BUILD_REGIONS, "- Build Regions", pc);
			logLine(*unmanaged, RC_TIMER_BUILD_REGIONS_WATERSHED, "    - Watershed", pc);
			logLine(*unmanaged, RC_TIMER_BUILD_REGIONS_EXPAND, "      - Expand", pc);
			logLine(*unmanaged, RC_TIMER_BUILD_REGIONS_FLOOD, "      - Find Basins", pc);
			logLine(*unmanaged, RC_TIMER_BUILD_REGIONS_FILTER, "    - Filter", pc);
			logLine(*unmanaged, RC_TIMER_BUILD_LAYERS, "- Build Layers", pc);
			logLine(*unmanaged, RC_TIMER_BUILD_CONTOURS, "- Build Contours", pc);
			logLine(*unmanaged, RC_TIMER_BUILD_CONTOURS_TRACE, "    - Trace", pc);
			logLine(*unmanaged, RC_TIMER_BUILD_CONTOURS_SIMPLIFY, "    - Simplify", pc);
			logLine(*unmanaged, RC_TIMER_BUILD_POLYMESH, "- Build Polymesh", pc);
			logLine(*unmanaged, RC_TIMER_BUILD_POLYMESHDETAIL, "- Build Polymesh Detail", pc);
			logLine(*unmanaged, RC_TIMER_MERGE_POLYMESH, "- Merge Polymeshes", pc);
			logLine(*unmanaged, RC_TIMER_MERGE_POLYMESHDETAIL, "- Merge Polymesh Details", pc);
			unmanaged->log(RC_LOG_PROGRESS, "=== TOTAL:\t%.2fms", totalTime / 1000.0f);
		}

	private:
		static void logLine(rcContext& ctx, rcTimerLabel label, const char* name, const float pc)
		{
			const int t = ctx.getAccumulatedTime(label);
			if (t < 0) return;
			ctx.log(RC_LOG_PROGRESS, "%s:\t%.2fms\t(%.1f%%)", name, t / 1000.0f, t*pc);
		}		
	};
}