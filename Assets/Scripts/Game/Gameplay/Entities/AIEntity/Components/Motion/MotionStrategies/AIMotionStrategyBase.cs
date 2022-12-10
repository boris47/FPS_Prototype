
using UnityEngine;

namespace Entities.AI.Components
{
	[DefaultExecutionOrder(-1)]
	public abstract class AIMotionStrategyBase : AIEntityComponent, IMotionTransition<AIEntityMotionTransitionSnapshot>
	{
		private				AIMotionManager				m_AIMotionManager			= null;			

		protected			AIMotionManager				AIMotionManager				=> m_AIMotionManager;			

		//--------------------------
		public abstract		Vector3						Position					{ get; }
		public abstract		Vector3						Destination					{ get; }
		public abstract		Vector3						Velocity					{ get; }


		//////////////////////////////////////////////////////////////////////////
		public void Configure(in AIMotionManager InAIMotionManager)
		{
			m_AIMotionManager = InAIMotionManager;
		}

		//////////////////////////////////////////////////////////////////////////
		public abstract AIEntityMotionTransitionSnapshot CreateSnapshot();
		public abstract void PorcessSnapshot(AIEntityMotionTransitionSnapshot InSnapShot);


		//////////////////////////////////////////////////////////////////////////
		public abstract bool RequestMoveTowardsEntity(in Entity InTargetEntity);
		public abstract bool RequestMoveToPosition(in Vector3 InDestination);
		public abstract void Stop(in bool bImmediately);
	}
}
