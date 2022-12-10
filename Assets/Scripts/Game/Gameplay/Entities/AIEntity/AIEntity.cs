
using System.Linq;
using UnityEngine;

namespace Entities.AI
{
	using Components;
	using Relations;

	[RequireComponent(typeof(AIController))]
	public abstract class AIEntity : Entity
	{
		public new				AIController					Controller							=> m_Controller as AIController;

		[SerializeField, ReadOnly]
		private					AIMotionManager					m_AIMotionManager					= null;


		//--------------------
		public					AIMotionManager					AIMotionManager						=> m_AIMotionManager;


		//////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			if (m_AIMotionManager.IsNotNull() || Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_AIMotionManager)))
			{

			}
		}

		//////////////////////////////////////////////////////////////////
		// This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only)
		protected virtual void OnValidate()
		{
			gameObject.TryGetIfNotAssigned(ref m_AIMotionManager);
		}

		//////////////////////////////////////////////////////////////////
		public override bool IsInterestedAt(in Entity source)
		{
			EntityFaction[] enemyFactions = Controller.BrainComponent.GetEnemyFactions();
			return enemyFactions.Contains(source.Faction);
		}

		//////////////////////////////////////////////////////////////////
		public bool IsInterestedAt(in Entity source, in ESoundType soundType)
		{
			// TODO
			EntityFaction[] enemyFactions = Controller.BrainComponent.GetEnemyFactions();
			return enemyFactions.Contains(source.Faction);
		}


		//////////////////////////////////////////////////////////////////
		public bool RequestMoveTowardsEntity(in Entity InTargetEntity)
		{
			return m_AIMotionManager.IsNotNull() ? m_AIMotionManager.RequestMoveTowardsEntity(InTargetEntity) : false;
		}

		//////////////////////////////////////////////////////////////////
		public bool RequestMoveToPosition(in Vector3 InVector3)
		{
			return m_AIMotionManager.IsNotNull() ? m_AIMotionManager.RequireMovementTo(InVector3) : false;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool IsCloseEnoughTo(in Entity InTargetEntity) => (InTargetEntity.Body.position - GetBody().position).magnitude < 0.5f;

		//////////////////////////////////////////////////////////////////////////
		public bool IsCloseEnoughTo(in Vector3 InTargetPosition) => (InTargetPosition - GetBody().position).magnitude < 0.5f;

		//////////////////////////////////////////////////////////////////
		public void Stop(in bool bImmediately)
		{
			if (m_AIMotionManager.IsNotNull())
			{
				m_AIMotionManager.Stop(bImmediately);
			}
		}


		//////////////////////////////////////////////////////////////////
		protected override Transform GetHead() => transform;

		//////////////////////////////////////////////////////////////////
		protected override Transform GetBody() => transform;

		//////////////////////////////////////////////////////////////////
		protected override Transform GetTargetable() => transform;

		//////////////////////////////////////////////////////////////////
		protected override Collider GetPrimaryCollider()
		{
			return null;
		}

		//////////////////////////////////////////////////////////////////////////
		public override Vector3 GetVelocity() => m_AIMotionManager.IsNotNull() ? m_AIMotionManager.Velocity : Vector3.zero;
	}
}

