using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
	public class RelationsData : ConfigurationBase, ISerializationCallbackReceiver
	{
		public const string ResourcePath = "Entities/RelationsData";

		private static readonly int				FactionCount			= System.Enum.GetNames(typeof(EFactions)).Length;

		private		short[,]					m_InternalData			= new short[FactionCount, FactionCount];

#if UNITY_EDITOR
		public		short[,]					EDITOR_ONLY_Data
		{
			get => m_InternalData;
			set => m_InternalData = value;
		}
#endif

		#region SERIALIZATION

		[System.Serializable]
		private struct Package<TElement>
		{
			public int Index0;
			public int Index1;
			public TElement Element;

			public Package(int idx0, int idx1, TElement element)
			{
				Index0 = idx0;
				Index1 = idx1;
				Element = element;
			}
		}
		
		// A list that can be serialized
		[SerializeField, HideInInspector]
		private		List<Package<short>>		m_Serializable			= new List<Package<short>>();

		// Save
		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			// Convert our unserializable array into a serializable list
			for (int i = 0; i < m_InternalData.GetLength(0); i++)
			{
				for (int j = 0; j < m_InternalData.GetLength(1); j++)
				{
					if (i == j)
					{
						m_InternalData[i, j] = short.MaxValue;
					}
					m_Serializable.Add(new Package<short>(i, j, m_InternalData[i, j]));
				}
			}
		}

		// Load
		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			// Convert the serializable list into our unserializable array
			m_InternalData = new short[FactionCount, FactionCount];
			foreach (Package<short> package in m_Serializable)
			{
				short value = package.Index0 == package.Index1 ? short.MaxValue : package.Element;
				m_InternalData[package.Index0, package.Index1] = value;
			}
		}

		#endregion


		//////////////////////////////////////////////////////////////////////////
		public void OverrideRelations(in EFactions InFaction1, in EFactions InFaction2, in short InNewalue)
		{
			if (Utils.CustomAssertions.IsTrue(m_InternalData.IsValidIndex((int)InFaction1, (int)InFaction2),
				this, $"Invalid values for InFaction1 and/or InFaction2: valid range 0-${FactionCount}, values are '{InFaction1}' and '{InFaction2}'"))
			{
				m_InternalData[(int)InFaction1, (int)InFaction2] = InNewalue;
				m_InternalData[(int)InFaction2, (int)InFaction1] = InNewalue;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void OverrideRelations(in EFactions InFaction1, in EFactions InFaction2, in EFactionRelationType InNewRelation)
		{
			if (Utils.CustomAssertions.IsTrue(typeof(EFactionRelationType).IsEnumDefined(InNewRelation),
				this, $"Invalid new value of {nameof(EFactionRelationType)}: value received is '{InNewRelation}'"))
			{
				short value = GetValueFromRelation(InNewRelation);
				OverrideRelations(InFaction1, InFaction2, value);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public EFactionRelationType GetRelationType(in EFactions InFaction1, in EFactions InFaction2)
		{
			short value = GetRelationValue(InFaction1, InFaction2);
			return GetRelationTypeFromValue(value);
		}

		//////////////////////////////////////////////////////////////////////////
		public EFactions[] GetHostilesFactions(in EFactions InFaction)
		{
			List<EFactions> outHostiles = new List<EFactions>();
			if (m_InternalData.IsValidIndex((int)InFaction))
			{
				for (int i = 0; i < m_InternalData.GetLength(1); i++)
				{
					if (GetRelationType(InFaction, (EFactions)i) == EFactionRelationType.Enemy)
					{
						outHostiles.Add((EFactions)i);
					}
				}
			}
			return outHostiles.ToArray();
		}

		//////////////////////////////////////////////////////////////////////////
		public short GetRelationValue(in EFactions InFaction1, in EFactions InFaction2)
		{
			short OutValue = default;
			int faction1Idx = (int)InFaction1, faction2Idx = (int)InFaction2;
			if (Utils.CustomAssertions.IsTrue(m_InternalData.IsValidIndex(faction1Idx, faction2Idx),
				this, $"Invalid values for InFaction1 and/or InFaction2: valid range 0-${FactionCount}, values are '{InFaction1}' and '{InFaction2}'"))
			{
				OutValue = m_InternalData[faction1Idx, faction2Idx];
			}
			return OutValue;
		}



		//////////////////////////////////////////////////////////////////////////
		private static short GetValueFromRelation(in EFactionRelationType InRelationType)
		{
			short outResult = default;
			switch (InRelationType)
			{
				case EFactionRelationType.Enemy:
				{
					outResult = short.MinValue;
					break;
				}
				case EFactionRelationType.Neutral:
				{
					outResult = 0;
					break;
				}
				case EFactionRelationType.Friendly:
				{
					outResult = short.MaxValue;
					break;
				}
				default: Utils.CustomAssertions.IsTrue(false, $"Invalid new value of {nameof(EFactionRelationType)}: value received is '{InRelationType}'"); break;
			}
			return outResult;
		}

		//////////////////////////////////////////////////////////////////////////
		private static EFactionRelationType GetRelationTypeFromValue(in short InValue)
		{
			if (InValue < short.MinValue * 0.5f)
			{
				return EFactionRelationType.Enemy;
			}
			else if (InValue > short.MaxValue * 0.5f)
			{
				return EFactionRelationType.Friendly;
			}
			return EFactionRelationType.Neutral;
		}
	}
}
