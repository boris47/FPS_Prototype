using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Localization
{
	[System.Serializable]
	public class LocalizationPair
	{
		[SerializeField]
		private string m_Key = string.Empty;

		[SerializeField]
		private string m_Value = string.Empty;

		public string Key => m_Key;
		public string Value => m_Value;

		//////////////////////////////////////////////////////////////////////////
		public LocalizationPair(in string InKey)
		{
			m_Key = InKey;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool Equals(in string InValue) => m_Value.Equals(InValue);
		public bool Equals(in LocalizationPair InItem) => m_Value.Equals(InItem.Value);

		//////////////////////////////////////////////////////////////////////////
		public static implicit operator LocalizationPair(in string s) => new LocalizationPair(s);
		public static implicit operator string(in LocalizationPair i) => i.Value;

		//////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
		public static class Editor
		{
			//////////////////////////////////////////////////////////////////////////
			public static void SetValue(in LocalizationPair InLocalizationPair, in string InValue)
			{
				InLocalizationPair.m_Value = InValue;
			}
		}
#endif
	}

	[System.Serializable]
	public sealed class LocalizationTable
	{
		public readonly SystemLanguage Language = SystemLanguage.Unknown;

		[SerializeField]
		private List<LocalizationPair> m_Items = new List<LocalizationPair>();


		//////////////////////////////////////////////////////////////////////////
		public LocalizationTable(in SystemLanguage InSystemLanguage)
		{
			Language = InSystemLanguage;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetValue(string InKey, out string OutValue)
		{
			OutValue = string.Empty;
			bool bResult = m_Items.TryFind(out LocalizationPair pair, out int index, item => item.Key.Equals(InKey));
			if (bResult)
			{
				OutValue = pair.Value;
			}
			return bResult;
		}

		//////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
		public static class Editor
		{
			//////////////////////////////////////////////////////////////////////////
			public static LocalizationPair AddKeyOrGet(in LocalizationTable InLocalizationTable, string InKey)
			{
				LocalizationPair OutValue = null;
				if (!InLocalizationTable.m_Items.TryFind(out OutValue, out int _, pair => pair.Key == InKey))
				{
					OutValue = InLocalizationTable.m_Items.AddRef(new LocalizationPair(InKey));
				}
				return OutValue;
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool HasKey(in LocalizationTable InLocalizationTable, string InKey)
			{
				return InLocalizationTable.m_Items.TryFind(out LocalizationPair _, out int _, pair => pair.Key == InKey);
			}

			//////////////////////////////////////////////////////////////////////////
			public static void RemoveKey(in LocalizationTable InLocalizationTable, string InKey)
			{
				if (InLocalizationTable.m_Items.TryFind(out LocalizationPair _, out int index, pair => pair.Key == InKey))
				{
					InLocalizationTable.m_Items.RemoveAt(index);
				}
			}

			//////////////////////////////////////////////////////////////////////////
			public static int GetItemsCount(in LocalizationTable InLocalizationTable)
			{
				return InLocalizationTable.m_Items.Count();
			}

			//////////////////////////////////////////////////////////////////////////
			public static string GetKeyAt(in LocalizationTable InLocalizationTable, in int InIndex)
			{
				string outValue = string.Empty;
				if (InLocalizationTable.m_Items.TryGetByIndex(InIndex, out LocalizationPair pair))
				{
					outValue = pair.Key;
				}
				return outValue;
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool TryGetKeyAt(in LocalizationTable InLocalizationTable, in int InIndex, out string OutValue)
			{
				OutValue = string.Empty;
				bool bResult = false;
				if (InLocalizationTable.m_Items.TryGetByIndex(InIndex, out LocalizationPair pair))
				{
					OutValue = pair.Key;
					bResult = true;
				}
				return bResult;
			}

			//////////////////////////////////////////////////////////////////////////
			public static string GetValueAt(in LocalizationTable InLocalizationTable, in int InIndex)
			{
				string outValue = string.Empty;
				if (InLocalizationTable.m_Items.TryGetByIndex(InIndex, out LocalizationPair pair))
				{
					outValue = pair.Value;
				}
				return outValue;
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool TryGetValueAt(in LocalizationTable InLocalizationTable, in int InIndex, out string OutValue)
			{
				OutValue = string.Empty;
				bool bResult = false;
				if (InLocalizationTable.m_Items.TryGetByIndex(InIndex, out LocalizationPair pair))
				{
					OutValue = pair.Value;
					bResult = true;
				}
				return bResult;
			}

			//////////////////////////////////////////////////////////////////////////
			public static (string, string) GetKeyAndValueAt(in LocalizationTable InLocalizationTable, in int InIndex)
			{
				string OutKey = string.Empty;
				string OutValue = string.Empty;
				if (InLocalizationTable.m_Items.TryGetByIndex(InIndex, out LocalizationPair pair))
				{
					OutKey = pair.Key;
					OutValue = pair.Value;
				}
				return (OutKey, OutValue);
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool TryGetKeyAndValueAt(in LocalizationTable InLocalizationTable, in int InIndex, out string OutKey, out string OutValue)
			{
				OutKey = string.Empty;
				OutValue = string.Empty;
				bool bResult = false;
				if (InLocalizationTable.m_Items.TryGetByIndex(InIndex, out LocalizationPair pair))
				{
					OutKey = pair.Key;
					OutValue = pair.Value;
					bResult = true;
				}
				return bResult;
			}

			//////////////////////////////////////////////////////////////////////////
			public static void SetKeyValue(in LocalizationTable InLocalizationTable, string InKey, in string InValue)
			{
				LocalizationPair pair = AddKeyOrGet(InLocalizationTable, InKey);
				LocalizationPair.Editor.SetValue(pair, InValue);
			}
		}
#endif
	}
}

