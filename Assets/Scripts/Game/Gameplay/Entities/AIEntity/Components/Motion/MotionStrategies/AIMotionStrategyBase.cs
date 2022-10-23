
using UnityEngine;

namespace Entities.AI.Components
{
	[DefaultExecutionOrder(20)]
	public abstract class AIMotionStrategyBase : AIEntityComponent, IMotionTransition<AIEntityMotionTransitionSnapshot>
	{
		public abstract AIEntityMotionTransitionSnapshot CreateSnapshot();

		public abstract void PorcessSnapshot(AIEntityMotionTransitionSnapshot InSnapShot);

		public abstract Vector3 Position { get; }

		public abstract bool RequireMovementTo(in Vector3 InDestination);

		public abstract void Stop(in bool bImmediately);
	}
}
