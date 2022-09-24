using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Relations
{
	[System.Serializable]
	public sealed class TableCell
	{
		[SerializeField,ReadOnly]
		private uint m_Faction1Id = 0u;

		[SerializeField, ReadOnly]
		private uint m_Faction2Id = 0u;

		[SerializeField, ReadOnly]
		private float m_Relation1To2Value = RelationsData.NeutralRelationValue;

		[SerializeField, ReadOnly]
		private float m_Relation2To1Value = RelationsData.NeutralRelationValue;

		public uint Faction1Id => m_Faction1Id;
		public uint Faction2Id => m_Faction2Id;
		public float Relation1To2Value => m_Relation1To2Value;
		public float Relation2To1Value => m_Relation2To1Value;


		//////////////////////////////////////////////////////////////////////////
		private TableCell(in uint Faction1Id, in uint Faction2Id)
		{
			m_Faction1Id = Faction1Id;
			m_Faction2Id = Faction2Id;
		}

		//////////////////////////////////////////////////////////////////////////
		public void OverrideValue(in EntityFaction from, in EntityFaction To, in float InNewValue)
		{
			if (from == m_Faction1Id)
			{
				m_Relation1To2Value = InNewValue;
			}
			else
			{
				if (from == m_Faction2Id)
				{
					m_Relation2To1Value = InNewValue;
				}
				else
				{
					//TODO Handle warning
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetRelationValueTo(in EntityFaction InToEntityFaction, out float OutValue)
		{
			bool bResult = false;
			OutValue = RelationsData.NeutralRelationValue;
			if (InToEntityFaction == m_Faction1Id)
			{
				OutValue = m_Relation2To1Value;
				bResult = true;
			}
			else
			{
				if (InToEntityFaction == m_Faction2Id)
				{
					OutValue = m_Relation1To2Value;
					bResult = true;
				}
				else
				{
					//TODO Handle warning
				}
			}
			return bResult;
		}

		//////////////////////////////////////////////////////////////////////////
		public float GetRelationValueTo(in EntityFaction InEntityFaction)
		{
			float OutValue = RelationsData.NeutralRelationValue;
			TryGetRelationValueTo(InEntityFaction, out OutValue);
			return OutValue;
		}

		public EFactionRelationType GetRelationTo(in EntityFaction InEntityFaction)
		{
			EFactionRelationType outValue = EFactionRelationType.NONE;
			if (TryGetRelationValueTo(InEntityFaction, out float outRelationValue))
			{
				outValue = RelationsData.GetRelationTypeFromValue(outRelationValue);
			}
			return outValue;
		}

#if UNITY_EDITOR
		//////////////////////////////////////////////////////////////////////////
		public static class Editor
		{
			//////////////////////////////////////////////////////////////////////////
			public static TableCell Create(in EntityFaction InFaction1, in EntityFaction InFaction2)
			{
				return new TableCell(InFaction1, InFaction2);
			}

			//////////////////////////////////////////////////////////////////////////
			internal static void OnEntityFactionNewUniqueId(in TableCell InTableCell, in uint InPreviousUniqueId, in uint InNewUniqueId)
			{
				if (InTableCell.m_Faction1Id == InPreviousUniqueId)
				{
					InTableCell.m_Faction1Id = InNewUniqueId;
				}

				if (InTableCell.m_Faction2Id == InPreviousUniqueId)
				{
					InTableCell.m_Faction2Id = InNewUniqueId;
				}
			}
		}
#endif
	}

	public sealed class RelationsData : ConfigurationBase
	{
		public const string ResourcePath = "Entities/RelationsData";
		public const string DefaultFactionName = "Default";
		public const float MinRelationValue = 0f;
		public const float MaxRelationValue = 1f;
		public const float NeutralRelationValue = (MinRelationValue + MaxRelationValue) * 0.5f;

		[SerializeField, ReadOnly]
		private List<EntityFaction> m_Factions = new List<EntityFaction>();

		[SerializeField, ReadOnly]
		private List<TableCell> m_Cells = new List<TableCell>() {  };

		public EntityFaction[] Factions => m_Factions.ToArray();


		//////////////////////////////////////////////////////////////////////////
		private static bool PredicateFind(TableCell cell, EntityFaction InEntityFaction1, EntityFaction InEntityFaction2)
		{
			return (cell.Faction1Id == InEntityFaction1 && cell.Faction2Id == InEntityFaction2)
				|| (cell.Faction1Id == InEntityFaction2 && cell.Faction2Id == InEntityFaction1);
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetRelationValue(EntityFaction InEntityFaction1, EntityFaction InEntityFaction2, out float OutValue)
		{
			bool bResult = false;
			OutValue = RelationsData.NeutralRelationValue;
			if (m_Cells.TryFind(out TableCell cell, out int index, tc => PredicateFind(tc, InEntityFaction1, InEntityFaction2)))
			{
				bResult = cell.TryGetRelationValueTo(InEntityFaction2, out OutValue);
			}
			return bResult;
		}

		//////////////////////////////////////////////////////////////////////////
		public void OverrideRelations(EntityFaction InEntityFaction1, EntityFaction InEntityFaction2, in float InNewValue)
		{
			if (m_Cells.TryFind(out TableCell cell, out int index, tc => PredicateFind(tc, InEntityFaction1, InEntityFaction2)))
			{
				cell.OverrideValue(InEntityFaction1, InEntityFaction2, InNewValue);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void OverrideRelations(EntityFaction InEntityFaction1, EntityFaction InEntityFaction2, in EFactionRelationType InNewRelationType)
		{
			if (m_Cells.TryFind(out TableCell cell, out int index, tc => PredicateFind(tc, InEntityFaction1, InEntityFaction2)))
			{
				cell.OverrideValue(InEntityFaction1, InEntityFaction2, GetValueFromRelation(InNewRelationType));
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public EntityFaction[] GetAllFactionsWithRelation(EntityFaction InEntityFaction, EFactionRelationType InRelationType)
		{
			var outList = new List<EntityFaction>();
			foreach (TableCell cell in m_Cells)
			{
				if (cell.Faction1Id == InEntityFaction && cell.GetRelationTo(GetEntityFactionFromId(cell.Faction2Id)) == InRelationType)
				{
					outList.Add(GetEntityFactionFromId(cell.Faction2Id));
				}
			}
			return outList.ToArray();
		}

		//////////////////////////////////////////////////////////////////////////
		public float GetRelationValue(in EntityFaction InEntityFaction1, in EntityFaction InEntityFaction2)
		{
			float outValue = RelationsData.NeutralRelationValue;
			TryGetRelationValue(InEntityFaction1, InEntityFaction2, out outValue);
			return outValue;
		}

		//////////////////////////////////////////////////////////////////////////
		public EFactionRelationType GetRelationType(in EntityFaction InEntityFaction1, in EntityFaction InEntityFaction2)
		{
			float outValue = RelationsData.NeutralRelationValue;
			TryGetRelationValue(InEntityFaction1, InEntityFaction2, out outValue);
			return GetRelationTypeFromValue(outValue);
		}

		//////////////////////////////////////////////////////////////////////////
		public static float GetValueFromRelation(in EFactionRelationType InRelationType)
		{
			float outResult = default;
			switch (InRelationType)
			{
				case EFactionRelationType.NONE:
				case EFactionRelationType.Neutral: outResult = RelationsData.NeutralRelationValue; break;
				case EFactionRelationType.Enemy: outResult = RelationsData.MinRelationValue; break;
				case EFactionRelationType.Friendly: outResult = RelationsData.MaxRelationValue; break;
				default: Utils.CustomAssertions.IsTrue(false, $"Invalid new value of {nameof(EFactionRelationType)}: value received is '{InRelationType}'"); break;
			}
			return outResult;
		}

		//////////////////////////////////////////////////////////////////////////
		public static EFactionRelationType GetRelationTypeFromValue(in float InValue)
		{
			float neutralValue = RelationsData.NeutralRelationValue;
			if (InValue < neutralValue)
			{
				return EFactionRelationType.Enemy;
			}
			else if (InValue > neutralValue)
			{
				return EFactionRelationType.Friendly;
			}
			return EFactionRelationType.Neutral;
		}

		//////////////////////////////////////////////////////////////////////////
		private EntityFaction GetEntityFactionFromId(uint InFactionId) => m_Factions.Find(f => f.UniqueId == InFactionId);


		//////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
		public static class Editor
		{
			public static void Sync(in RelationsData InRelationsData)
			{
				using (new Utils.Editor.MarkAsDirty(InRelationsData))
				{
					if (InRelationsData.m_Factions.Count == 0 || InRelationsData.m_Cells.Count == 0)
					{
						InRelationsData.m_Factions.ForEach(f => UnityEditor.AssetDatabase.RemoveObjectFromAsset(f));
						InRelationsData.m_Factions.Clear();
						InRelationsData.m_Factions.Add(EntityFaction.Default);
						InRelationsData.m_Cells.Clear();
						UnityEditor.AssetDatabase.AddObjectToAsset(EntityFaction.Default, InRelationsData);
					}
				}
			}

			//////////////////////////////////////////////////////////////////////////
			public static string[] GetFactionsNames(in RelationsData InRelationsData) => InRelationsData.m_Factions
					.Where(ef => !EntityFaction.Editor.IsDefault(ef))
					.Select(ef => ef.FactionName)
					.ToArray();

			//////////////////////////////////////////////////////////////////////////
			public static EntityFaction CreateFaction(in RelationsData InRelationsData, string InFactionName)
			{
				EntityFaction newFaction = null;
				if (!InRelationsData.m_Factions.Exists(f => f.FactionName == InFactionName))
				{
					using (new Utils.Editor.MarkAsDirty(InRelationsData))
					{
						newFaction = EntityFaction.Editor.Create(InFactionName);
						foreach (EntityFaction otherFaction in InRelationsData.m_Factions)
						{
							InRelationsData.m_Cells.Add(TableCell.Editor.Create(newFaction, otherFaction));
						}
						InRelationsData.m_Factions.Add(newFaction);
					}
				}
				return newFaction;
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool Contains(in RelationsData InRelationsData, string InFactionName)
			{
				return InRelationsData.m_Factions.Exists(f => f.FactionName == InFactionName);
			}

			//////////////////////////////////////////////////////////////////////////
			public static EntityFaction GetFactionById(in RelationsData InRelationsData, uint InFactionId)
			{
				return InRelationsData.m_Factions.Find(f => f.UniqueId == InFactionId);
			}

			//////////////////////////////////////////////////////////////////////////
			public static EntityFaction GetFactionByName(in RelationsData InRelationsData, string InFactionName)
			{
				return InRelationsData.m_Factions.Find(f => f.FactionName == InFactionName);
			}

			//////////////////////////////////////////////////////////////////////////
			public static void DeleteFaction(in RelationsData InRelationsData, string InFactionName)
			{
				using (new Utils.Editor.MarkAsDirty(InRelationsData))
				{
					uint uniqueId = EntityFaction.GetUniqueIdFor(InFactionName);
					if (InRelationsData.m_Factions.TryFind(out EntityFaction faction, out int index, f => f.UniqueId == uniqueId))
					{
						InRelationsData.m_Factions.RemoveAt(index);
						for (int i = InRelationsData.m_Cells.Count - 1; i >= 0; i--)
						{
							TableCell cell = InRelationsData.m_Cells[i];
							uniqueId = EntityFaction.GetUniqueIdFor(InFactionName);
							if (cell.Faction1Id == uniqueId || cell.Faction2Id == uniqueId)
							{
								InRelationsData.m_Cells.RemoveAt(i);
							}
						}
					}
					else
					{
						Debug.LogError($"Trying to remove {InFactionName}, but it cannnot be found.");
					}
				}
			}

			//////////////////////////////////////////////////////////////////////////
			public static void Rename(in RelationsData InRelationsData, EntityFaction InEntityFaction, in string InNewEntityFactionName)
			{
				uint newUniqueId = EntityFaction.GetUniqueIdFor(InNewEntityFactionName);
				foreach (TableCell cell in InRelationsData.m_Cells)
				{
					if (cell.Faction1Id == InEntityFaction.UniqueId || cell.Faction2Id == InEntityFaction.UniqueId)
					{
						TableCell.Editor.OnEntityFactionNewUniqueId(cell, InEntityFaction.UniqueId, newUniqueId);
					}
				}

				EntityFaction.Editor.Rename(InEntityFaction, InNewEntityFactionName);
			}
		}
#endif
	}
}
