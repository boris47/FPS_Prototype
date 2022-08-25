
using UnityEngine;

namespace Entities.Player.Components
{
	[DefaultExecutionOrder(20)]
	public abstract class PlayerMotionStrategyBase : PlayerEntityComponent, IMotionTransition<PlayerMotionTransitionSnapshot>
	{
		public abstract PlayerMotionTransitionSnapshot CreateSnapshot();

		public abstract void PorcessSnappshot(PlayerMotionTransitionSnapshot InSnapShot);
	}
}
