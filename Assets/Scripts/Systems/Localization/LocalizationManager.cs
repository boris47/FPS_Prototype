using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Localization
{
	[Configurable(nameof(m_LocalizationData), LocalizationData.ResourcePath)]
	public sealed class LocalizationManager : GlobalMonoBehaviourSingleton<LocalizationManager>
	{
		private const string k_LANGUAGE_KEY = "UserLangauge";

		[SerializeField, ReadOnly]
		private				SystemLanguage								m_CurrentSystemLanguage					= LocalizationData.DefaultSystemLanguage;

		[SerializeField, ReadOnly]
		private				LocalizationData							m_LocalizationData						= null;


		public event		System.Action<SystemLanguage>				OnLanguageChanged						= delegate { };

		public				SystemLanguage								CurrentSystemLanguage					=> m_CurrentSystemLanguage;



		//////////////////////////////////////////////////////////////////////////
		protected override void OnInitialize()
		{
			base.OnInitialize();

			Utils.CustomAssertions.IsTrue(this.TryGetConfiguration(out m_LocalizationData));

			if (!PlayerPrefs.HasKey(k_LANGUAGE_KEY))
			{

				SystemLanguage[] availableLanguages = m_LocalizationData.AvailableLanguages;
				if (availableLanguages.Contains(Application.systemLanguage))
				{
					PlayerPrefs.SetString(k_LANGUAGE_KEY, Application.systemLanguage.ToString());
				}
				else
				{
					PlayerPrefs.SetString(k_LANGUAGE_KEY, LocalizationData.DefaultSystemLanguage.ToString());
				}
			}

			if (PlayerPrefs.HasKey(k_LANGUAGE_KEY))
			{
				string currentLanguage = PlayerPrefs.GetString(k_LANGUAGE_KEY);
				if (Utils.CustomAssertions.IsTrue(System.Enum.TryParse(currentLanguage, out SystemLanguage OutLanguage)))
				{
					m_CurrentSystemLanguage = OutLanguage;
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		public bool SwitchTo(in SystemLanguage InSystemLanguage)
		{
			bool bOutValue = false;
			if (bOutValue = m_LocalizationData.AvailableLanguages.Contains(InSystemLanguage))
			{
				m_CurrentSystemLanguage = InSystemLanguage;
				PlayerPrefs.SetString(k_LANGUAGE_KEY, m_CurrentSystemLanguage.ToString());
				OnLanguageChanged(InSystemLanguage);
			}
			return bOutValue;
		}


		//////////////////////////////////////////////////////////////////////////
		public LocalizationValue GetLocalized(in uint InKeyId, LocalizationValue InDefault = null, SystemLanguage? InLanguageOverride = null)
		{
			if (m_LocalizationData.TryGetValue(InLanguageOverride ?? m_CurrentSystemLanguage, out LocalizationTable outLocalizationTable))
			{
				if (outLocalizationTable.TryGetValue(InKeyId, out LocalizationValue @value))
				{
					return @value;
				}
			}
			return InDefault;
		}


		//////////////////////////////////////////////////////////////////////////
		public bool TryGetLocalized(in uint InKeyId, out LocalizationValue OutValue, in LocalizationValue InDefault = null)
		{
			OutValue = default;
			bool bResult = false;
			if (m_LocalizationData.TryGetValue(m_CurrentSystemLanguage, out LocalizationTable outLocalizationTable))
			{
				if (outLocalizationTable.TryGetValue(InKeyId, out LocalizationValue @value))
				{
					OutValue = @value;
					bResult = true;
				}
			}

			OutValue ??= InDefault;
			return bResult;
		}
	}
}

