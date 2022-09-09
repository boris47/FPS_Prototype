using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Localization
{
	[System.Serializable]
	public sealed class LocalizationTable
	{
		[SerializeField]
		private SystemLanguage m_Language = SystemLanguage.Unknown;

		[SerializeField]
		private List<LocalizationValue> m_Values = new List<LocalizationValue>();

		public SystemLanguage Language => m_Language;


		//////////////////////////////////////////////////////////////////////////
		public LocalizationTable(in SystemLanguage InSystemLanguage)
		{
			m_Language = InSystemLanguage;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetValue(uint InKeyId, out LocalizationValue OutValue)
		{
			return m_Values.TryFind(out OutValue, out var _, v => v.ReferencedKeyId == InKeyId);
		}

		//////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
		public static class Editor
		{
			//////////////////////////////////////////////////////////////////////////
			public static LocalizationValue AddOrGetValue(in LocalizationTable InLocalizationTable, in string InKeyName)
			{
				LocalizationValue OutValue = default;
				LocalizationKey key = LocalizationKey.Editor.Get(InKeyName);
				if (!InLocalizationTable.TryGetValue(key, out OutValue))
				{
					OutValue = InLocalizationTable.m_Values.AddRef(new LocalizationValue(key));
				}
				return OutValue;
			}

			//////////////////////////////////////////////////////////////////////////
			public static LocalizationKey[] GetKeys(in LocalizationTable InLocalizationTable)
			{
				return InLocalizationTable.m_Values.Select(kv => LocalizationKey.Editor.Get(kv.ReferencedKeyId)).ToArray();
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool TryGetValue(in LocalizationTable InLocalizationTable, in string InKeyName, out LocalizationValue OutValue)
			{
				OutValue = default;
				LocalizationKey key = LocalizationKey.Editor.Get(InKeyName);
				if (!InLocalizationTable.TryGetValue(key, out OutValue))
				{
					OutValue = InLocalizationTable.m_Values.AddRef(new LocalizationValue(key));
				}
				return OutValue != default;
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool HasKey(in LocalizationTable InLocalizationTable, LocalizationKey InKey)
			{
				return InLocalizationTable.m_Values.TryFind(out LocalizationValue _, out int _, v => v.ReferencedKeyId == InKey);
			}

			//////////////////////////////////////////////////////////////////////////
			public static void RemoveValue(in LocalizationTable InLocalizationTable, LocalizationKey InKey)
			{
				if (InLocalizationTable.m_Values.TryFind(out LocalizationValue _, out int index, pair => pair.ReferencedKeyId == InKey))
				{
					InLocalizationTable.m_Values.RemoveAt(index);
				}
			}

			//////////////////////////////////////////////////////////////////////////
			public static int GetItemsCount(in LocalizationTable InLocalizationTable)
			{
				return InLocalizationTable.m_Values.Count();
			}
		}
#endif
	}
}

