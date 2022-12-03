
using UnityEngine;

namespace Entities.AI.Components
{
	[DefaultExecutionOrder(20)]
	public abstract class AIMotionStrategyBase : AIEntityComponent, IMotionTransition<AIEntityMotionTransitionSnapshot>
	{
		public abstract AIEntityMotionTransitionSnapshot CreateSnapshot();

		public abstract void PorcessSnapshot(AIEntityMotionTransitionSnapshot InSnapShot);

		public abstract Vector3 Position { get; }

		public abstract Vector3 Velocity { get; }

		public abstract bool RequestMoveTowardsEntity(in Entity InTargetEntity);
		public abstract bool StopMovingTowardsEntity(in Entity InTargetEntity);
		public abstract bool RequestMoveToPosition(in Vector3 InDestination);

		public abstract void Stop(in bool bImmediately);
	}
}
