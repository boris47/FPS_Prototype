using UnityEngine;

namespace UserSettings
{
	// ---------------------------
	public struct AudioData
	{
		public float MusicVolume;
		public float SoundVolume;
		public bool isDirty;
	}


	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////


	public static class AudioSettings
	{
		// Registry Keys
		private const string FLAG_SAVED_AUDIO_SETTINGS = "bSavedAudioSettings";
		private const string VAR_AUDIO_SETTINGS = "sAudioSettings";
		
		private static AudioData m_AudioData = new AudioData();

		public static AudioData GetAudioData() => m_AudioData;

		//////////////////////////////////////////////////////////////////////////
		static AudioSettings()
		{
			LoadOrSetDefaults();

			OnApplyChanges();
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OnMusicVolumeSet(float value)
		{
			m_AudioData.MusicVolume = value;
			m_AudioData.isDirty = true;
			SoundManager.MusicVolume = value;
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OnSoundsVolumeSet(float value)
		{
			m_AudioData.SoundVolume = value;
			m_AudioData.isDirty = true;
			SoundManager.SoundVolume = value;
		}

		//////////////////////////////////////////////////////////////////////////
		public static void LoadOrSetDefaults()
		{
			if (PlayerPrefs.HasKey(FLAG_SAVED_AUDIO_SETTINGS))
			{
				ReadFromRegistry();
			}
			else
			{
				ApplyDefaults();

				PlayerPrefs.SetString(FLAG_SAVED_AUDIO_SETTINGS, "1");
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public static void ApplyDefaults()
		{
			// Remove keys from registry
			Reset();
			{
				m_AudioData.MusicVolume = 1f;
				m_AudioData.SoundVolume = 1f;
				m_AudioData.isDirty = false;
			}
			// Save new keys into registry
			SaveToRegistry();

			// Apply the default settings
			OnApplyChanges();
		}

		//////////////////////////////////////////////////////////////////////////
		public static void ReadFromRegistry()
		{
			string settings = PlayerPrefs.GetString(VAR_AUDIO_SETTINGS);
			JsonUtility.FromJsonOverwrite(settings, m_AudioData);
			m_AudioData.isDirty = true;
		}

		//////////////////////////////////////////////////////////////////////////
		public static void SaveToRegistry()
		{
			string setting = JsonUtility.ToJson(m_AudioData);

			// Save settings
			PlayerPrefs.SetString(VAR_AUDIO_SETTINGS, setting);
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OnApplyChanges()
		{
			if (m_AudioData.isDirty)
			{
				m_AudioData.isDirty = false;
				SoundManager.MusicVolume = m_AudioData.MusicVolume;
				SoundManager.SoundVolume = m_AudioData.SoundVolume;
			}
			// Save settings
			SaveToRegistry();
		}

		//////////////////////////////////////////////////////////////////////////
		public static void Reset()
		{
			PlayerPrefs.DeleteKey(FLAG_SAVED_AUDIO_SETTINGS);
			PlayerPrefs.DeleteKey(VAR_AUDIO_SETTINGS);
		}
	}
}