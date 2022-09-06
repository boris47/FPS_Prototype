using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// TODO: Salvare le key come scriptable objects ed usare un componente per gestire la localizzazione
/// nel custom editor di questo componente mostrare le key dome una dropdown
/// </summary>
namespace Localization
{
	[Configurable(nameof(m_LocalizationData), LocalizationData.ResourcePath)]
	public sealed class LocalizationManager : GlobalMonoBehaviourSingleton<LocalizationManager>
	{
		private const string k_LANGUAGE_KEY = "UserLangauge";

		[SerializeField]
		private				SystemLanguage								m_CurrentSystemLanguage					= SystemLanguage.Unknown;

		[SerializeField, ReadOnly]
		private				LocalizationData							m_LocalizationData						= null;


		public event		System.Action								OnLanguageChanged						= delegate { };

		public				SystemLanguage								CurrentSystemLanguage					=> m_CurrentSystemLanguage;



		//////////////////////////////////////////////////////////////////////////
		protected override void OnInitialize()
		{
			base.OnInitialize();

			if (!PlayerPrefs.HasKey(k_LANGUAGE_KEY))
			{
				SystemLanguage[] availableLanguages = m_LocalizationData.AvailableLanguages;
				if (availableLanguages.Contains(Application.systemLanguage))
				{
					PlayerPrefs.SetString(k_LANGUAGE_KEY, Application.systemLanguage.ToString());
				}
				else
				{
					if (availableLanguages.TryGetByIndex(0, out SystemLanguage firstAvailable))
					{
						PlayerPrefs.SetString(k_LANGUAGE_KEY, firstAvailable.ToString());
					}
				}
			}

			if (PlayerPrefs.HasKey(k_LANGUAGE_KEY))
			{
				string currentLanguage = PlayerPrefs.GetString(k_LANGUAGE_KEY);
				if (System.Enum.TryParse(currentLanguage, out SystemLanguage OutLanguage))
				{
					m_CurrentSystemLanguage = OutLanguage;
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		public bool SwitchTo(SystemLanguage newSystemLanguage)
		{
			bool bOutValue = false;
			if (m_LocalizationData.AvailableLanguages.Contains(newSystemLanguage))
			{
				m_CurrentSystemLanguage = newSystemLanguage;
				PlayerPrefs.SetString(k_LANGUAGE_KEY, m_CurrentSystemLanguage.ToString());
				OnLanguageChanged();
				bOutValue = true;
			}
			return bOutValue;
		}


		//////////////////////////////////////////////////////////////////////////
		public string GetLocalized(string key, string @default = null, SystemLanguage? languageOverride = null)
		{
			if (m_LocalizationData.TryGetValue(languageOverride ?? m_CurrentSystemLanguage, out LocalizationTable outLocalizationTable))
			{
				if (outLocalizationTable.TryGetValue(key, out string @value))
				{
					return @value;
				}
			}
			return @default ?? key;
		}


		//////////////////////////////////////////////////////////////////////////
		public bool TryGetLocalized(string key, out string OutValue, string @default = null)
		{
			OutValue = null;
			bool bResult = false;
			if (m_LocalizationData.TryGetValue(m_CurrentSystemLanguage, out LocalizationTable outLocalizationTable))
			{
				if (outLocalizationTable.TryGetValue(key, out string @value))
				{
					OutValue = @value;
					bResult = true;
				}
			}

			OutValue ??= @default ?? key;
			return bResult;
		}
	}
}

