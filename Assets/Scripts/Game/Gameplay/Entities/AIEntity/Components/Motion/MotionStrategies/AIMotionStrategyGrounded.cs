
using UnityEngine;

namespace Entities.AI.Components
{
	public class AIMotionStrategyGrounded : AIMotionStrategyBase
	{
		[SerializeReference, ReadOnly]
		private AIEntityMotionControllerBase m_MotionController = null;

		public override Vector3 Position => m_Owner.Body.position;

		public override Vector3 Velocity => m_MotionController.Velocity;


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			if (enabled = Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_MotionController)))
			{

			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnValidate()
		{
			base.OnValidate();
			gameObject.TryGetComponent(out m_MotionController);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();
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
			return m_MotionController.RequestMoveTowardsEntity(InTargetEntity);
		}

		public override bool StopMovingTowardsEntity(in Entity InTargetEntity)
		{
			return m_MotionController.StopMovingTowardsEntity(InTargetEntity);
		}

		//////////////////////////////////////////////////////////////////////////
		public override bool RequestMoveToPosition(in Vector3 InDestination)
		{
			return m_MotionController.RequireMovementTo(InDestination);
		}

		//////////////////////////////////////////////////////////////////////////
		public override void Stop(in bool bImmediately)
		{
			m_MotionController.StopMovement(bImmediately);
		}
	}
}
