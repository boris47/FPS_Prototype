using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
	public enum EFactionRelationType
	{
		Enemy,
		Neutral,
		Friendly
	}

	public enum EFactions
	{
		// IMPORTANT: After a first release, new faction can only be appended and renaming is not allowed for existing ones
		Player,		// Base
		Civilians,  // Base
		Guards,	    // Base
		Soldiers,   // Base
		SpecOps,    // Base
		Animal,     // Base
	}

	[Configurable(nameof(m_RelationsData), RelationsData.ResourcePath)]
	public sealed class RelationsBoard : GlobalMonoBehaviourSingleton<RelationsBoard>
	{
		[SerializeField, ReadOnly]
		private RelationsData m_RelationsData = null;


		//////////////////////////////////////////////////////////////////////////
		protected override void OnInitialize()
		{
			Utils.CustomAssertions.IsTrue(this.TryGetConfiguration(out m_RelationsData));
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetRelationBetweenFactions(in EFactions InFaction1, in EFactions InFaction2, in short InNewalue)
		{
			m_RelationsData.OverrideRelations(InFaction1, InFaction2, InNewalue);
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetRelationBetweenFactions(in EFactions InFaction1, in EFactions InFaction2, in EFactionRelationType InNewalue)
		{
			m_RelationsData.OverrideRelations(InFaction1, InFaction2, InNewalue);
		}

		//////////////////////////////////////////////////////////////////////////
		public EFactionRelationType RelationBetween(in Entity InEntity1, in Entity InEntity2)
		{
			return m_RelationsData.GetRelationType(InEntity1.Faction, InEntity2.Faction);
		}

		//////////////////////////////////////////////////////////////////////////
		public EFactions[] GetHostiles(in EFactions InFaction)
		{
			return m_RelationsData.GetHostilesFactions(InFaction);
		}

		//////////////////////////////////////////////////////////////////////////
		public short GetRelationValue(in Entity InEntity1, in Entity InEntity2)
		{
			return m_RelationsData.GetRelationValue(InEntity1.Faction, InEntity2.Faction);
		}

		//////////////////////////////////////////////////////////////////////////
		public Entity GetMostHostile(Entity InEntity, in ICollection<Entity> InEntities)
		{
			return InEntities.MinBy(e => GetRelationValue(InEntity, e));
		}

		//////////////////////////////////////////////////////////////////////////
		public Entity GetMostFriendly(Entity InEntity, in ICollection<Entity> InEntities)
		{
			return InEntities.MaxBy(e => GetRelationValue(InEntity, e));
		}
	}
}
