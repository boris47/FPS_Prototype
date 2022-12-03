
using UnityEngine;

namespace Entities.AI.Components
{
	[Configurable(nameof(m_Config), "AI/MotionStrategies/" + nameof(AIMotionStrategyFly))]
	public class AIMotionStrategyFly : AIMotionStrategyBase
	{
		[SerializeField, ReadOnly]
		private AIConfigurationFly m_Config = null;

		public override Vector3 Position => transform.position;
		public override Vector3 Velocity => Vector3.zero;


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			Utils.CustomAssertions.IsTrue(this.TryGetConfiguration(out m_Config));
		}

		//////////////////////////////////////////////////////////////////////////
		public override AIEntityMotionTransitionSnapshot CreateSnapshot()
		{
			return new AIEntityMotionTransitionSnapshot()
			{
				CurrentVelocity = Vector3.zero
			};
		}

		//////////////////////////////////////////////////////////////////////////
		public override void PorcessSnapshot(AIEntityMotionTransitionSnapshot InSnapShot)
		{

		}

		//////////////////////////////////////////////////////////////////////////
		public override bool RequestMoveTowardsEntity(in Entity InTargetEntity)
		{
			return false;
		}

		public override bool StopMovingTowardsEntity(in Entity InTargetEntity)
		{
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public override bool RequestMoveToPosition(in Vector3 InDestination)
		{
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public override void Stop(in bool bImmediately)
		{
			
		}
	}
}
