
using UnityEngine;


namespace Entities.AI.Components
{
	using Relations;

	public enum ETargetAcquisitionStrategy
	{
		FAREST, CLOSEST, WEAKER, HARDER, TILL_ELIMINATION
	}

	public class AIBrainComponent : AIEntityComponent
	{
		[SerializeField]
		private				ETargetAcquisitionStrategy						m_TargetAcquisitionStrategy						= ETargetAcquisitionStrategy.CLOSEST;

		//---------------------
		public				ETargetAcquisitionStrategy						TargetAcquisitionStrategy						=> m_TargetAcquisitionStrategy;
		public				EntityFaction									Faction											=> m_Owner.Faction;
		public				EFactionRelationType?							RelationOverride								=> m_Owner.GlobalRelationOverride;
		public event		System.Action									OnRelationChanged								= delegate { };
		public event		System.Action<ETargetAcquisitionStrategy>		OnTargetAcquisitionStrategyChanged				= delegate { };


		//////////////////////////////////////////////////////////////////////////
		public void SetTargetAcquisitionStrategy(in ETargetAcquisitionStrategy newStrategy)
		{
			if (m_TargetAcquisitionStrategy != newStrategy)
			{
				m_TargetAcquisitionStrategy = newStrategy;
				OnTargetAcquisitionStrategyChanged(newStrategy);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public EntityFaction[] GetEnemyFactions()
		{
			return RelationsBoard.Instance.GetAllFactionsWithRelation(m_Owner, EFactionRelationType.Enemy);
		}


		// TODO OnSaveLoad Fire the OnRelationChanged to update all listeners

		//////////////////////////////////////////////////////////////////////////
		public bool IsInterestedAt(in Entity entity)
		{
			// this entity should evaluate the other entity
			return m_Owner.IsInterestedAt(entity);
		}
	}
}
