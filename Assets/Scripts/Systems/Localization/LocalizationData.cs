using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Localization
{
	public sealed class LocalizationData : ConfigurationBase, ISerializationCallbackReceiver
	{
		public const		string										ResourcePath							= "Systems/Localization/LocalizationData";
		public const		SystemLanguage								DefaultSystemLanguage					= SystemLanguage.English;

		[SerializeField, ReadOnly]
		private				List<SystemLanguage>						m_SupportedLanguages					= new List<SystemLanguage>() { DefaultSystemLanguage };

		[SerializeField, ReadOnly]
		private				List<LocalizationTable>						m_Tables								= new List<LocalizationTable>() { new LocalizationTable(DefaultSystemLanguage) };

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

		[SerializeField]
		private List<string> m_EDITOR_KeyNames = new List<string>();

		public static class Editor
		{
			//////////////////////////////////////////////////////////////////////////
			public static void Sync(in LocalizationData InLocalizationData)
			{
				using (new Utils.Editor.MarkAsDirty(InLocalizationData))
				{
					if (InLocalizationData.m_SupportedLanguages.Count == 0 || InLocalizationData.m_Tables.Count == 0)
					{
						InLocalizationData.m_SupportedLanguages.Clear();
						InLocalizationData.m_SupportedLanguages.Add(DefaultSystemLanguage);
						InLocalizationData.m_Tables.Clear();
						InLocalizationData.m_Tables.Add(new LocalizationTable(DefaultSystemLanguage));
					}
					LocalizationKey.Editor.Reset();
					foreach (string keyName in InLocalizationData.m_EDITOR_KeyNames)
					{
						LocalizationKey.Editor.CreateAsset(keyName);
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
							foreach (string keyName in InLocalizationData.m_EDITOR_KeyNames)
							{
								// Emplace values foreach available key
								LocalizationTable.Editor.AddOrGetValue(table, keyName);
							}
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
						else
						{
							Debug.LogError($"Cannot find a table associated with language {InLanguage}.");
						}
					}
					else
					{
						Debug.LogError($"Trying to remove language {InLanguage}, but it cannnot be found.");
					}
				}
			}

			//////////////////////////////////////////////////////////////////////////
			public static void AddKey(in LocalizationData InLocalizationData, in LocalizationKey InKey)
			{
				using (new Utils.Editor.MarkAsDirty(InLocalizationData))
				{
					foreach (LocalizationTable localizationTable in InLocalizationData.m_Tables)
					{
						LocalizationTable.Editor.AddOrGetValue(localizationTable, InKey);
					}
					InLocalizationData.m_EDITOR_KeyNames.Add(InKey);
				}
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool KeyExists(in LocalizationData InLocalizationData, in LocalizationKey InKey) => InLocalizationData.m_EDITOR_KeyNames.Contains(InKey);
			
			//////////////////////////////////////////////////////////////////////////
			public static bool KeyExists(string InKeyName) => LocalizationKey.Editor.Contains(InKeyName);

			//////////////////////////////////////////////////////////////////////////
			public static bool KeyExists(uint InKeyId) => LocalizationKey.Editor.Contains(InKeyId);
			

			//////////////////////////////////////////////////////////////////////////
			public static void RemoveKey(in LocalizationData InLocalizationData, in LocalizationKey InKey)
			{
				using (new Utils.Editor.MarkAsDirty(InLocalizationData))
				{
					foreach (LocalizationTable localizationTable in InLocalizationData.m_Tables)
					{
						LocalizationTable.Editor.RemoveValue(localizationTable, InKey);
					}
					InLocalizationData.m_EDITOR_KeyNames.Remove(InKey);
				}
			}
		}
#endif
	}
}

