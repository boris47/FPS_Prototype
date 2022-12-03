using UnityEngine;

namespace Entities.AI.Components
{
	public class AIEntityRigidbodyController : AIEntityMotionControllerBase
	{
		public override Vector3 Position => m_Rigidbody.position;
		public override Vector3 Velocity => m_Rigidbody.IsNotNull() ? m_Rigidbody.velocity : Vector3.zero;
		public override Vector3 Destination => m_Destination;

		[SerializeField, ReadOnly]
		private Rigidbody m_Rigidbody = null;


		private Vector3 m_Destination = Vector3.zero;


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			if (enabled = Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_Rigidbody)))
			{
				
			}
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
		public override bool RequireMovementTo(in Vector3 InDestination)
		{
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public override void StopMovement(in bool bImmediately)
		{
			
		}
	}
}
