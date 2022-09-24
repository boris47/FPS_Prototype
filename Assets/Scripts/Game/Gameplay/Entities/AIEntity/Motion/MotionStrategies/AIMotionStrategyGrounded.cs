
using UnityEngine;

namespace Entities.AI.Components
{
	public class AIMotionStrategyGrounded : AIMotionStrategyBase
	{

		[SerializeField, ReadOnly]
		private AIEntityMotionControllerBase m_MotionController = null;

		public override Vector3 Position => throw new System.NotImplementedException();


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
		public override bool RequireMovementTo(in Vector3 InDestination)
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
