#pragma once

#include <DetourCrowd.h>

namespace RecastWrapper {

	public ref class ManagedDtCrowdAgentParams
	{
	public:

		float radius;						///< Agent radius. [Limit: >= 0]
		float height;						///< Agent height. [Limit: > 0]
		float maxAcceleration;				///< Maximum allowed acceleration. [Limit: >= 0]
		float maxSpeed;						///< Maximum allowed speed. [Limit: >= 0]

											/// Defines how close a collision element must be before it is considered for steering behaviors. [Limits: > 0]
		float collisionQueryRange;

		float pathOptimizationRange;		///< The path visibility optimization range. [Limit: > 0]

											/// How aggresive the agent manager should be at avoiding collisions with this agent. [Limit: >= 0]
		float separationWeight;

		/// Flags that impact steering behavior. (See: #UpdateFlags)
		unsigned char updateFlags;

		/// The index of the avoidance configuration to use for the agent. 
		/// [Limits: 0 <= value <= #DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS]
		float obstacleAvoidanceType;

		/// The index of the query filter used by this agent.
		unsigned char queryFilterType;

		/// User defined data attached to the agent.
		void* userData;

	public:
		ManagedDtCrowdAgentParams() { }
		~ManagedDtCrowdAgentParams() { this->!ManagedDtCrowdAgentParams(); }
		!ManagedDtCrowdAgentParams() { }

		const dtCrowdAgentParams GetUnmanaged()
		{
			dtCrowdAgentParams result;
			result.radius = radius;
			result.height = height;
			result.maxAcceleration = maxAcceleration;
			result.maxSpeed = maxSpeed;
			result.collisionQueryRange = collisionQueryRange;
			result.pathOptimizationRange = pathOptimizationRange;
			result.separationWeight = separationWeight;
			result.updateFlags = updateFlags;
			result.obstacleAvoidanceType = (unsigned char)obstacleAvoidanceType;
			result.queryFilterType = queryFilterType;
			result.userData = userData;
			return result;
		}
	};

}