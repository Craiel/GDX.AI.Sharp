#pragma once

#include "ManagedDtCrowdAgent.h"
#include "ManagedDtCrowdAgentParams.h"
#include <DetourCrowd.h>

namespace RecastWrapper
{
	public ref class ManagedDtCrowd
	{
	private:
		dtCrowd* unmanaged;
		dtCrowdAgentDebugInfo* debugInfo;
	internal:
		ManagedDtCrowd(dtCrowd* instance) { unmanaged = instance; }
		dtCrowd* GetUnmanaged() { return unmanaged; }

	public:
		bool Init(int maxAgents, float agentRadius, ManagedDtNavMesh^ navMesh)
		{
			dtCrowdAgentDebugInfo info;
			memset(&info, 0, sizeof(info));
			debugInfo = &info;

			return unmanaged->init(maxAgents, agentRadius, navMesh->GetUnmanaged());
		}

		void Update(float delta)
		{
			unmanaged->update(delta, debugInfo);
		}

		int GetAgentCount()
		{
			return unmanaged->getAgentCount();
		}

		ManagedDtCrowdAgent^ GetAgent(int index)
		{
			const dtCrowdAgent* agent = unmanaged->getAgent(index);
			return gcnew ManagedDtCrowdAgent(agent);
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

		void SetDefaultAvoidanceParameters()
		{
			// Setup local avoidance params to different qualities.
			dtObstacleAvoidanceParams params;
			// Use mostly default settings, copy from dtCrowd.
			memcpy(&params, unmanaged->getObstacleAvoidanceParams(0), sizeof(dtObstacleAvoidanceParams));

			// Low (11)
			params.velBias = 0.5f;
			params.adaptiveDivs = 5;
			params.adaptiveRings = 2;
			params.adaptiveDepth = 1;
			unmanaged->setObstacleAvoidanceParams(0, &params);

			// Medium (22)
			params.velBias = 0.5f;
			params.adaptiveDivs = 5;
			params.adaptiveRings = 2;
			params.adaptiveDepth = 2;
			unmanaged->setObstacleAvoidanceParams(1, &params);

			// Good (45)
			params.velBias = 0.5f;
			params.adaptiveDivs = 7;
			params.adaptiveRings = 2;
			params.adaptiveDepth = 3;
			unmanaged->setObstacleAvoidanceParams(2, &params);

			// High (66)
			params.velBias = 0.5f;
			params.adaptiveDivs = 7;
			params.adaptiveRings = 3;
			params.adaptiveDepth = 3;

			unmanaged->setObstacleAvoidanceParams(3, &params);
		}
	};
}