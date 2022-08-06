
using UnityEngine;


namespace Entities.AI.Components
{
	public enum ETargetAcquisitionStrategy
	{
		FAREST, CLOSEST, WEAKER, HARDER, TILL_ELIMINATION
	}

	internal interface IBrainTargets
	{
		event System.Action OnRelationChanged;
		event System.Action<ETargetAcquisitionStrategy> OnTargetAcquisitionStrategyChanged;

		ETargetAcquisitionStrategy TargetAcquisitionStrategy { get; }

		void SetTargetAcquisitionStrategy(in ETargetAcquisitionStrategy newStrategy);

		EFactions[] GetEnemyFactions();

		bool IsInterestedAt(in Entity entity);
	}

	public partial class AIBrainComponent : AIEntityComponent, IBrainTargets
	{
		internal IBrainTargets									Targets									=> this;

		[SerializeField]
		protected ETargetAcquisitionStrategy					m_TargetAcquisitionStrategy				= ETargetAcquisitionStrategy.CLOSEST;
		public EFactions										Faction									=> m_Owner.Faction;
		public EFactionRelationType?							RelationOverride						=> m_Owner.GlobalRelationOverride;

		/* IBrainTargets BEGIN */
		private event System.Action								m_OnRelationChanged						= delegate { };
		event System.Action IBrainTargets.OnRelationChanged
		{
			add		=> m_OnRelationChanged += value;
			remove	=> m_OnRelationChanged -= value;
		}

		private event System.Action<ETargetAcquisitionStrategy>	m_OnTargetAcquisitionStrategyChanged	= delegate { };
		event System.Action<ETargetAcquisitionStrategy> IBrainTargets.OnTargetAcquisitionStrategyChanged
		{
			add		=> m_OnTargetAcquisitionStrategyChanged += value;
			remove	=> m_OnTargetAcquisitionStrategyChanged -= value;
		}

		ETargetAcquisitionStrategy IBrainTargets.TargetAcquisitionStrategy => m_TargetAcquisitionStrategy;

		//////////////////////////////////////////////////////////////////////////
		void IBrainTargets.SetTargetAcquisitionStrategy(in ETargetAcquisitionStrategy newStrategy)
		{
			if (m_TargetAcquisitionStrategy != newStrategy)
			{
				m_TargetAcquisitionStrategy = newStrategy;
				m_OnTargetAcquisitionStrategyChanged(newStrategy);
			}
		}

		EFactions[] IBrainTargets.GetEnemyFactions() => RelationsBoard.Instance.GetHostiles(m_Owner.Faction);


		// TODO OnSaveLoad Fire the OnRelationChanged to update all listeners

		//////////////////////////////////////////////////////////////////////////
		bool IBrainTargets.IsInterestedAt(in Entity entity)
		{
			// this entity should evaluate the other entity
			return m_Owner.IsInterestedAt(entity);
		}

		/* IBrainTargets END */
	}
}
