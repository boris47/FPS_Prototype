
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Entities.AI
{
	using Components;
	using Relations;

	[RequireComponent(typeof(NavMeshAgent))]
	[RequireComponent(typeof(AIController))]
	[RequireComponent(typeof(AIMotionManager))]
	public partial class AIEntity : Entity
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
			EntityFaction[] enemyFactions = Controller.BrainComponent.Targets.GetEnemyFactions();
			return enemyFactions.Contains(source.Faction);
		}


		//////////////////////////////////////////////////////////////////
		public bool IsInterestedAt(in Entity source, in ESoundType soundType)
		{
			EntityFaction[] enemyFactions = Controller.BrainComponent.Targets.GetEnemyFactions();
			return enemyFactions.Contains(source.Faction);
		}


		//////////////////////////////////////////////////////////////////
		protected override Transform GetHead()
		{
			return transform;
		}

		//////////////////////////////////////////////////////////////////
		protected override Transform GetBody()
		{
			return transform;
		}

		//////////////////////////////////////////////////////////////////
		protected override Transform GetTargetable()
		{
			return transform;
		}

		//////////////////////////////////////////////////////////////////
		protected override Collider GetPrimaryCollider()
		{
			return null;
		}
	}
}

