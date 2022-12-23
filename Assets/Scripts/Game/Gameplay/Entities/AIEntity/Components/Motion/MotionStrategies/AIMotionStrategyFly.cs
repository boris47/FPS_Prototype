
using UnityEngine;

namespace Entities.AI.Components
{
	[RequireComponent(typeof(Rigidbody))]
	public class AIMotionStrategyFly : AIMotionStrategyBase
	{
		[SerializeField, ReadOnly]
		private					Rigidbody								m_Rigidbody						= null;

		public		override	Vector3									Position						=> m_Rigidbody.position;
		public		override	Vector3									Velocity						=> m_Rigidbody.velocity;
		public		override	Vector3									Destination						=> m_Destination;


		private					Vector3									m_Destination					= Vector3.zero;


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			if (enabled = Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_Rigidbody)))
			{
				m_Rigidbody.useGravity = false;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public override AIEntityMotionTransitionSnapshot CreateSnapshot()
		{
			return new AIEntityMotionTransitionSnapshot()
			{
				CurrentVelocity = m_Rigidbody.velocity,
				Destination = AIMotionManager.TargetLocation
			};
		}

		//////////////////////////////////////////////////////////////////////////
		public override void PorcessSnapshot(AIEntityMotionTransitionSnapshot InSnapShot)
		{
			m_Rigidbody.velocity = InSnapShot.CurrentVelocity;

			AIMotionManager.TargetLocation.position = InSnapShot.Destination.position;
			AIMotionManager.TargetLocation.rotation = InSnapShot.Destination.rotation;
		}

		//////////////////////////////////////////////////////////////////////////
		public override bool RequestMoveTowardsEntity(in Entity InTargetEntity)
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
			m_Rigidbody.velocity = Vector3.zero;
		}
	}
}
