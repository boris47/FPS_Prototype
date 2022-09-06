using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Localization
{
	public sealed class LocalizationData : ConfigurationBase, ISerializationCallbackReceiver
	{
		public const string ResourcePath = "Systems/Localization/LocalizationData";

		[SerializeField]
		private				List<SystemLanguage>						m_SupportedLanguages					= new List<SystemLanguage>();


		[SerializeField]
		private				List<LocalizationTable>						m_Tables								= new List<LocalizationTable>();


		public				SystemLanguage[]							AvailableLanguages						=> m_SupportedLanguages.ToArray();

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetValue(SystemLanguage InLanguage, out LocalizationTable OutValue)
		{
			return m_Tables.TryFind(out OutValue, out int _, t => t.Language == InLanguage);
		}

		#region SERIALIZATION

		//////////////////////////////////////////////////////////////////////////
		// Save
		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		
		}

		//////////////////////////////////////////////////////////////////////////
		// Load
		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{

		}

		#endregion

		//////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR

		/*[System.NonSerialized]*/[SerializeField]
		private List<string> m_EDITOR_Keys = new List<string>();

		public static class Editor
		{
			//////////////////////////////////////////////////////////////////////////
			public static void Sync(in LocalizationData InLocalizationData)
			{
				using (new Utils.Editor.MarkAsDirty(InLocalizationData))
				{
					foreach (SystemLanguage systemLanguage in InLocalizationData.m_SupportedLanguages)
					{
						// Ensure every language has a paired table
						if (!InLocalizationData.m_Tables.Exists(t => t.Language == systemLanguage))
						{
							InLocalizationData.m_Tables.Add(new LocalizationTable(systemLanguage));
						}
					}
		
					// Removed orphan tables
					for (int i = InLocalizationData.m_Tables.Count - 1; i >= 0; i--)
					{
						LocalizationTable localizationTable = InLocalizationData.m_Tables[i];
						if (!InLocalizationData.m_SupportedLanguages.Contains(localizationTable.Language))
						{
							InLocalizationData.m_Tables.RemoveAt(i);
						}
					}

					// Sync keys list
					InLocalizationData.m_EDITOR_Keys.Clear();
					if (InLocalizationData.m_Tables.Any())
					{
						var table = InLocalizationData.m_Tables.First();
						int count = LocalizationTable.Editor.GetItemsCount(table);
						for (int i = 0; i < count; i++)
						{
							InLocalizationData.m_EDITOR_Keys.Add(LocalizationTable.Editor.GetKeyAt(table, i));
						}
					}
				}
			}

			//////////////////////////////////////////////////////////////////////////
			public static void AddLanguage(in LocalizationData InLocalizationData, in SystemLanguage InLanguage)
			{
				if (!InLocalizationData.m_SupportedLanguages.Contains(InLanguage))
				{
					using (new Utils.Editor.MarkAsDirty(InLocalizationData))
					{
						InLocalizationData.m_SupportedLanguages.Add(InLanguage);
						LocalizationTable table = new LocalizationTable(InLanguage);
						{
							InLocalizationData.m_EDITOR_Keys.ForEach(key => LocalizationTable.Editor.AddKeyOrGet(table, key));
						}
						InLocalizationData.m_Tables.Add(table);
					}
				}
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool HasLanguage(in LocalizationData InLocalizationData, in SystemLanguage InLanguage)
			{
				return InLocalizationData.m_SupportedLanguages.Contains(InLanguage);
			}

			//////////////////////////////////////////////////////////////////////////
			public static void RemoveLanguage(in LocalizationData InLocalizationData, SystemLanguage InLanguage)
			{
				using (new Utils.Editor.MarkAsDirty(InLocalizationData))
				{
					if (InLocalizationData.m_SupportedLanguages.Remove(InLanguage))
					{
						if (Utils.CustomAssertions.IsTrue(InLocalizationData.m_Tables.TryFind(out LocalizationTable _, out int index, t => t.Language == InLanguage)))
						{
							InLocalizationData.m_Tables.RemoveAt(index);
						}
					}
				}
			}

			//////////////////////////////////////////////////////////////////////////
			public static void AddKey(in LocalizationData InLocalizationData, in string InKey)
			{
				using (new Utils.Editor.MarkAsDirty(InLocalizationData))
				{
					foreach (LocalizationTable localizationTable in InLocalizationData.m_Tables)
					{
						LocalizationTable.Editor.AddKeyOrGet(localizationTable, InKey);
					}
					InLocalizationData.m_EDITOR_Keys.Add(InKey);
				}
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool KeyExists(in LocalizationData InLocalizationData, in string InKey)
			{
				return InLocalizationData.m_EDITOR_Keys.Contains(InKey);
			}

			//////////////////////////////////////////////////////////////////////////
			public static int GetKeysCount(in LocalizationData InLocalizationData)
			{
				return InLocalizationData.m_EDITOR_Keys.Count;
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool TryGetKeyAt(in LocalizationData InLocalizationData, in int InIndex, out string OutValue)
			{
				return InLocalizationData.m_EDITOR_Keys.TryGetByIndex(InIndex, out OutValue);
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool TryGetTableAt(in LocalizationData InLocalizationData, in int InIndex, out LocalizationTable OutLocalizationTable)
			{
				return InLocalizationData.m_Tables.TryGetByIndex(InIndex, out OutLocalizationTable);
			}

			//////////////////////////////////////////////////////////////////////////
			public static void RemoveKey(in LocalizationData InLocalizationData, in string InKey)
			{
				using (new Utils.Editor.MarkAsDirty(InLocalizationData))
				{
					foreach (LocalizationTable localizationTable in InLocalizationData.m_Tables)
					{
						LocalizationTable.Editor.RemoveKey(localizationTable, InKey);
					}
					InLocalizationData.m_EDITOR_Keys.Remove(InKey);
				}
			}
		}
#endif
	}
}

