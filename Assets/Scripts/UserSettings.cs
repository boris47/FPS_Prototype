using UnityEngine;
using System.Collections;
using UnityEngine.PostProcessing;
using System.Collections.Generic;

namespace UserSettings
{

	public static class AudioSettings
	{
		// Registry Keys
		private const string FLAG_SAVED_AUDIO_SETTINGS = "bSavedAudioSettings";
		private const string VAR_MUSIC_VOLUME = "iMusicVolume";
		private const string VAR_SOUND_VOLUME = "iSoundVolume";

		// CHANGES STRUCTURES
		// ---------------------------
		public struct AudioData
		{
			public float MusicVolume;
			public float SoundVolume;
			public bool isDirty;
		}
		private static AudioData m_AudioData;

		public static AudioData GetAudioData() => m_AudioData;

		static AudioSettings()
		{
			OnEnable();

			OnApplyChanges();
		}

		public static void OnMusicVolumeSet(float value)
		{
			m_AudioData.MusicVolume = value;
			m_AudioData.isDirty = true;
			SoundManager.MusicVolume = value;
		}

		public static void OnSoundsVolumeSet(float value)
		{
			m_AudioData.SoundVolume = value;
			m_AudioData.isDirty = true;
			SoundManager.SoundVolume = value;
		}

		public static void OnEnable()
		{
			if (PlayerPrefs.HasKey(FLAG_SAVED_AUDIO_SETTINGS) == true)
			{
				ReadFromRegistry();
			}
			else
			{
				ApplyDefaults();

				PlayerPrefs.SetString(FLAG_SAVED_AUDIO_SETTINGS, "1");
			}
		}

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

		public static void ReadFromRegistry()
		{
			m_AudioData.MusicVolume = PlayerPrefs.GetFloat(VAR_MUSIC_VOLUME);
			m_AudioData.SoundVolume = PlayerPrefs.GetFloat(VAR_SOUND_VOLUME);
			m_AudioData.isDirty = true;
		}

		public static void SaveToRegistry()
		{
			// Save settings
			PlayerPrefs.SetFloat(VAR_MUSIC_VOLUME, m_AudioData.MusicVolume);
			PlayerPrefs.SetFloat(VAR_SOUND_VOLUME, m_AudioData.SoundVolume);

		}

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

		public static void Reset()
		{
			PlayerPrefs.DeleteKey(FLAG_SAVED_AUDIO_SETTINGS);
			{
				PlayerPrefs.DeleteKey(VAR_MUSIC_VOLUME);
				PlayerPrefs.DeleteKey(VAR_SOUND_VOLUME);
			}
		}
	}

	public static class VideoSettings
	{
		public delegate void OnResolutionChangedDelegate(float newWidth, float newHeight);

		private static event OnResolutionChangedDelegate m_OnResolutionChanged = delegate { };
		public static event OnResolutionChangedDelegate OnResolutionChanged
		{
			add    { if (value != null) m_OnResolutionChanged += value; }
			remove { if (value != null) m_OnResolutionChanged -= value; }
		}

		// Registry Keys
		private const string FLAG_SAVED_GRAPHIC_SETTINGS = "bSavedVideoSettings";
		private const string VAR_RESOLUTION_INDEX = "iResolutionIndex";
		private const string VAR_IS_FULLSCREEN = "bFullScreen";
		private const string VAR_ANISOTROPIC_FILTERING = "bAnisotropicFiltering";
		private const string VAR_ANTIALIASING_LEVEL = "iAntialiasingLevel";
		private const string VAR_QUALITY_LEVEL = "iQualityLevel";

		// CHANGES STRUCTURES
		// ---------------------------
		public struct ScreenData
		{
			public Resolution resolution;
			public bool bIsFullScreen;
			public int iResolutionIndex;
			public bool isDirty;
		}
		private static ScreenData m_ScreenData;
		public static ScreenData GetScreenData() => m_ScreenData;

		// ---------------------------
		public struct QualityData
		{
			public int iQualityLevel;
			public bool isDirty;
		}
		private static QualityData m_QualityData;
		public static QualityData GetQualityData() => m_QualityData;

		// ---------------------------
		public struct FiltersData
		{
			public bool bHasAnisotropicFilter;
			public int iAntialiasing;
			public bool isDirty;
		}
		private static FiltersData m_FiltersData;
		public static FiltersData GetFiltersData() => m_FiltersData;

		// ---------------------------
		public struct PostProcessingData
		{
			// Antialiasing
			public bool bIsAntialiasingEnabled;
			public AntialiasingModel.FxaaPreset eAntialiasingPreset;

			// Ambient Occlusion
			public bool bIsAmbientOcclusionEnabled;
			public int iAmbientOcclusionLvlIdx;

			// Screen Space Reflection
			public bool bIsScreenSpaceReflectionEnabled;
			public int iScreenSpaceReflectionLvlIdx;

			// Depth Of Field
			public bool bIsDepthOfFieldEnabled;
			public int iDepthOfFieldLvlIdx;

			// MotionBlur
			public bool bIsMotionBlurEnabled;

			// Bloom
			public bool bIsBloomEnabled;

			// Chromatic Aberration
			public bool bIsChromaticAberrationEnabled;

			public bool isDirty;
		}
		private static PostProcessingData m_PostProcessingData;
		public static PostProcessingData GetPostProcessingData() => m_PostProcessingData;
		public static PostProcessingProfile m_PP_Profile { get;  set; } = null;

		private static Resolution[] m_AvailableResolutions = null;
		public static Resolution[] GetAvailableResolutions() => m_AvailableResolutions;

		static VideoSettings()
		{
			int comparer(Resolution a, Resolution b)
			{
				int mulA = a.width * a.height;
				int mulB = b.width * b.height;
				return mulA < mulB ? -1 : mulA > mulB ? 1 : 0;
			}

			List<Resolution> sortedResolutions = new List<Resolution>(Screen.resolutions);
			sortedResolutions.Sort(comparer);
			m_AvailableResolutions = sortedResolutions.ToArray();

			OnEnable(null);

			OnApplyChanges();
		}

		private static int GetResolutionIndex(Resolution res)
		{
			int bestWidthtDelta = int.MaxValue, bestHeightDelta = int.MaxValue, currentIndex = 0;
			int size = m_AvailableResolutions.Length;
			for (int i = 0; i < size;  i++)
			{
				Resolution r = m_AvailableResolutions[i];
				int deltaWidth = Mathf.Abs(res.width - r.width);
				if (deltaWidth < bestWidthtDelta)
				{
					int deltaHeight = Mathf.Abs(res.height - r.height);
					if (deltaHeight < bestHeightDelta)
					{
						currentIndex = i;
						bestHeightDelta = deltaHeight;
					}
					bestWidthtDelta = deltaWidth;
				}
			}

			return currentIndex;
		}

		public static void OnResolutionChosen(Resolution resolution, int resolutionIndex)
		{
			m_ScreenData.resolution = resolution;
			m_ScreenData.iResolutionIndex = resolutionIndex;
			m_ScreenData.isDirty = true;
		}

		public static void OnFullScreenSet(bool newValue)
		{
			m_ScreenData.bIsFullScreen = newValue;
			m_ScreenData.isDirty = true;
		}

		public static void OnAnisotropicFilterSet(bool newValue)
		{
			m_FiltersData.bHasAnisotropicFilter = newValue;
			m_FiltersData.isDirty = true;
		}

		public static void OnAntialiasingSet(int newIndex)
		{
			m_FiltersData.iAntialiasing = newIndex;
			m_FiltersData.isDirty = true;

			if (m_PostProcessingData.bIsAntialiasingEnabled = newIndex > 0)
				m_PostProcessingData.eAntialiasingPreset = (AntialiasingModel.FxaaPreset)(newIndex - 1);
		}

		public static void OnQualityLevelSet(int newiIndex)
		{
			m_QualityData.iQualityLevel = newiIndex;
			m_QualityData.isDirty = true;
		}


		// ////////////////////////


		public static void OnMotionBlurSetEnabled(bool bIsEnabled)
		{
			m_PostProcessingData.bIsMotionBlurEnabled = bIsEnabled;
			m_PostProcessingData.isDirty = true;
		}

		public static void OnBloomSetEnabled(bool bIsEnabled)
		{
			m_PostProcessingData.bIsBloomEnabled = bIsEnabled;
			m_PostProcessingData.isDirty = true;
		}

		public static void OnChromaticAberrationSetEnabled(bool bIsEnabled)
		{
			m_PostProcessingData.bIsChromaticAberrationEnabled = bIsEnabled;
			m_PostProcessingData.isDirty = true;
		}
		
		public static void OnDepthOfFieldSetEnabled(bool bIsEnabled)
		{
			m_PostProcessingData.bIsDepthOfFieldEnabled = bIsEnabled;
			m_PostProcessingData.isDirty = true;
		}
		
		public static void OnDepthOfFieldSetLvl(int level)
		{
			m_PostProcessingData.iDepthOfFieldLvlIdx = level;
			m_PostProcessingData.isDirty = true;
		}
		
		public static void OnScreenSpaceReflectionSetEnabled(bool bIsEnabled)
		{
			m_PostProcessingData.bIsScreenSpaceReflectionEnabled = bIsEnabled;
			m_PostProcessingData.isDirty = true;
		}

		public static void OnScreenSpaceReflectionSetLvl(int level)
		{
			m_PostProcessingData.iScreenSpaceReflectionLvlIdx = level;
			m_PostProcessingData.isDirty = true;
		}

		public static void OnAmbientOcclusionSetEnabled(bool bIsEnabled)
		{
			m_PostProcessingData.bIsAmbientOcclusionEnabled = bIsEnabled;
			m_PostProcessingData.isDirty = true;
		}
		
		public static void OnAmbientOcclusionSetLvl(int level)
		{
			m_PostProcessingData.iAmbientOcclusionLvlIdx = level;
			m_PostProcessingData.isDirty = true;
		}


		public static void OnEnable(PostProcessingProfile PP_Profile)
		{
			m_PP_Profile = PP_Profile;
			if (PlayerPrefs.HasKey(FLAG_SAVED_GRAPHIC_SETTINGS) == true)
			{
				ReadFromRegistry();
			}
			else
			{
				ApplyDefaults();

				PlayerPrefs.SetString(FLAG_SAVED_GRAPHIC_SETTINGS, "1");
			}
		}

		public static void ApplyDefaults()
		{
			// Remove keys from registry
			Reset();
			{
				// Screen
				m_ScreenData.resolution = new Resolution() { width = 800, height = 600, refreshRate = 60 };
				m_ScreenData.iResolutionIndex = GetResolutionIndex(m_ScreenData.resolution);
				m_ScreenData.bIsFullScreen = true;
				m_ScreenData.isDirty = true;

				// Filters
				m_FiltersData.bHasAnisotropicFilter = false;
				m_FiltersData.iAntialiasing = 0;
				m_FiltersData.isDirty = true;

				// Quality
				m_QualityData.iQualityLevel = 0;
				m_QualityData.isDirty = true;
			}
			// Save new keys into registry
			SaveToRegistry();

			// Apply the default settings
			OnApplyChanges();
		}

		public static void ReadFromRegistry()
		{
			// Screen
			m_ScreenData.iResolutionIndex = PlayerPrefs.GetInt(VAR_RESOLUTION_INDEX);
			if (m_ScreenData.iResolutionIndex > -1)
			{
				m_ScreenData.resolution = m_AvailableResolutions[m_ScreenData.iResolutionIndex];
			}

			m_ScreenData.bIsFullScreen = PlayerPrefs.GetInt(VAR_IS_FULLSCREEN) != 0;

			// Filters
			m_FiltersData.bHasAnisotropicFilter = PlayerPrefs.GetInt(VAR_ANISOTROPIC_FILTERING) != 0;
			m_FiltersData.iAntialiasing = PlayerPrefs.GetInt(VAR_ANTIALIASING_LEVEL);

			// Quality
			m_QualityData.iQualityLevel = PlayerPrefs.GetInt(VAR_QUALITY_LEVEL);
		}

		public static void SaveToRegistry()
		{
			PlayerPrefs.SetInt(VAR_RESOLUTION_INDEX, m_ScreenData.iResolutionIndex);
			PlayerPrefs.SetInt(VAR_IS_FULLSCREEN, m_ScreenData.bIsFullScreen ? 1 : 0);

			PlayerPrefs.SetInt(VAR_ANISOTROPIC_FILTERING, m_FiltersData.bHasAnisotropicFilter ? 1 : 0);
			PlayerPrefs.SetInt(VAR_ANTIALIASING_LEVEL, m_FiltersData.iAntialiasing);

			PlayerPrefs.SetInt(VAR_QUALITY_LEVEL, m_QualityData.iQualityLevel);
		}

		public static void OnApplyChanges()
		{
			// Post	Processes
			{
				if (m_PostProcessingData.isDirty && m_PP_Profile)
				{
					m_PostProcessingData.isDirty = false;

					{   // Ambient Occlusion
						m_PP_Profile.ambientOcclusion.enabled = m_PostProcessingData.bIsAmbientOcclusionEnabled;
		//				m_PP_Profile.ambientOcclusion.Reset();
					}
					{   // Screen Space Reflection
						m_PP_Profile.screenSpaceReflection.enabled = m_PostProcessingData.bIsScreenSpaceReflectionEnabled;
		//				m_PP_Profile.screenSpaceReflection.Reset();
					}
					{   // Depth Of Field
						m_PP_Profile.depthOfField.enabled = m_PostProcessingData.bIsDepthOfFieldEnabled;
		//				m_PP_Profile.depthOfField.Reset();
					}
					{   // Motion Blur
						m_PP_Profile.motionBlur.enabled = m_PostProcessingData.bIsMotionBlurEnabled;
					}
					{   // Bloom
						m_PP_Profile.bloom.enabled = m_PostProcessingData.bIsBloomEnabled;
					}
					{   // Chromatic Aberration
						m_PP_Profile.chromaticAberration.enabled = m_PostProcessingData.bIsChromaticAberrationEnabled;
					}
				}
			}

			// Screen
			if (m_ScreenData.isDirty)
			{
				m_ScreenData.isDirty = false;
				Screen.SetResolution( width: m_ScreenData.resolution.width, height: m_ScreenData.resolution.height, fullscreen: m_ScreenData.bIsFullScreen );
				m_OnResolutionChanged(m_ScreenData.resolution.width, m_ScreenData.resolution.height);
			}

			// Filter
			if (m_FiltersData.isDirty)
			{
				m_FiltersData.isDirty = false;

				QualitySettings.anisotropicFiltering = m_FiltersData.bHasAnisotropicFilter ? AnisotropicFiltering.Enable : AnisotropicFiltering.Disable;
				QualitySettings.antiAliasing = m_FiltersData.iAntialiasing * 2;
			}

			// Quality
			if (m_QualityData.isDirty)
			{
				m_QualityData.isDirty = false;

				QualitySettings.SetQualityLevel(m_QualityData.iQualityLevel, applyExpensiveChanges: true);
			}

			// Save settings
			SaveToRegistry();
		}

		public static void Reset()
		{
			PlayerPrefs.DeleteKey(FLAG_SAVED_GRAPHIC_SETTINGS);
			{
				PlayerPrefs.DeleteKey(VAR_RESOLUTION_INDEX);
				PlayerPrefs.DeleteKey(VAR_IS_FULLSCREEN);
				PlayerPrefs.DeleteKey(VAR_ANISOTROPIC_FILTERING);
				PlayerPrefs.DeleteKey(VAR_ANTIALIASING_LEVEL);
				PlayerPrefs.DeleteKey(VAR_QUALITY_LEVEL);
			}
		}
	}

	public static class GameplaySettings
	{

	}
}