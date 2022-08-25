
using UnityEngine;

namespace Entities.AI.Components
{
	[DefaultExecutionOrder(20)]
	public abstract class AIMotionStrategyBase : AIEntityComponent
	{
		public abstract Vector3 Position { get; }

		public abstract void SetNewDestination(in Vector3 InDestination);
	}
}
