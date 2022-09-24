using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Relations
{
	public enum EFactionRelationType
	{
		NONE,
		Enemy,
		Neutral,
		Friendly
	}

	[Configurable(nameof(m_RelationsData), RelationsData.ResourcePath)]
	public sealed class RelationsBoard : GlobalMonoBehaviourSingleton<RelationsBoard>
	{
		[SerializeField, ReadOnly]
		private				RelationsData								m_RelationsData							= null;


		//////////////////////////////////////////////////////////////////////////
		protected override void OnInitialize()
		{
			Utils.CustomAssertions.IsTrue(this.TryGetConfiguration(out m_RelationsData));
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetRelationBetweenFactions(in EntityFaction InFaction1, in EntityFaction InFaction2, in float InNewalue)
		{
			m_RelationsData.OverrideRelations(InFaction1, InFaction2, InNewalue);
			m_RelationsData.OverrideRelations(InFaction2, InFaction1, InNewalue);
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetRelationBetweenFactions(in EntityFaction InFaction1, in EntityFaction InFaction2, in EFactionRelationType InNewalue)
		{
			m_RelationsData.OverrideRelations(InFaction1, InFaction2, InNewalue);
			m_RelationsData.OverrideRelations(InFaction2, InFaction1, InNewalue);
		}

		//////////////////////////////////////////////////////////////////////////
		public EFactionRelationType RelationTo(in Entity InEntity1, in Entity InEntity2) => m_RelationsData.GetRelationType(InEntity1.Faction, InEntity2.Faction);

		public EntityFaction[] GetAllFactionsWithRelation(in Entity InEntity, in EFactionRelationType InRelationType) => m_RelationsData.GetAllFactionsWithRelation(InEntity.Faction, InRelationType);

		//////////////////////////////////////////////////////////////////////////
		public float GetRelationValue(in Entity InEntity1, in Entity InEntity2) => m_RelationsData.GetRelationValue(InEntity1.Faction, InEntity2.Faction);

		//////////////////////////////////////////////////////////////////////////
		public Entity GetMostHostile(Entity InEntity, in ICollection<Entity> InEntities) => InEntities.MinBy(e => GetRelationValue(InEntity, e));

		//////////////////////////////////////////////////////////////////////////
		public Entity GetMostFriendly(Entity InEntity, in ICollection<Entity> InEntities) => InEntities.MaxBy(e => GetRelationValue(InEntity, e));
	}
}
