using UnityEngine;
using UnityEngine.PostProcessing;
using System.Collections.Generic;

namespace UserSettings
{
	class PPSettings<T> : System.IDisposable where T : struct
	{
		private		PostProcessingModel		m_Model			= null;
		private		T						m_Settings		= default;

		public PPSettings(PostProcessingModel model, out T OutSettings)
		{
			if (ReflectionHelper.GetFieldValue(model, "m_Settings", out OutSettings))
			{
				m_Model = model;
			//	m_Settings = OutSettings;
				AssignByRef(ref OutSettings, ref m_Settings);
			}
		}
		void System.IDisposable.Dispose()
		{
			if (m_Settings.IsNotNull())
			{
				ReflectionHelper.SetFieldValue(m_Model, "m_Settings", m_Settings);
			}
		}

		private static void AssignByRef(ref T objA, ref T objB)
		{
			objB = objA;
		}
	}



	public static class AudioSettings
	{
		// Registry Keys
		private const string FLAG_SAVED_AUDIO_SETTINGS = "bSavedAudioSettings";
		private const string VAR_AUDIO_SETTINGS = "sAudioSettings";
		
		// ---------------------------
		public class AudioData
		{
			public float MusicVolume = 1f;
			public float SoundVolume = 1f;
			public bool isDirty = false;
		}
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

	public static class VideoSettings
	{
		public delegate void OnResolutionChangedDelegate(float newWidth, float newHeight);

		private static event OnResolutionChangedDelegate m_OnResolutionChanged = delegate { };
		public static event OnResolutionChangedDelegate OnResolutionChanged
		{
			add    { if (value.IsNotNull()) m_OnResolutionChanged += value; }
			remove { if (value.IsNotNull()) m_OnResolutionChanged -= value; }
		}

		// Registry Keys
		private const string FLAG_SAVED_GRAPHIC_SETTINGS = "bSavedVideoSettings";
		private const string VAR_VIDEO_SETTINGS = "sVideoSettings";

		// ---------------------------
		[System.Serializable]
		public class ScreenData
		{
			public Resolution resolution = new Resolution();
			public bool bIsFullScreen = false;
			public int iResolutionIndex = 0;
			public bool isDirty = false;
		}
		// ---------------------------
		[System.Serializable]
		public class QualityData
		{
			public int iQualityLevel = 0;
			public bool isDirty = false;
		}
		// ---------------------------
		[System.Serializable]
		public class FiltersData
		{
			public bool bHasAnisotropicFilter = false;
			public int iAntialiasing = 0;
			public bool isDirty = false;
		}
		// ---------------------------
		[System.Serializable]
		public class PostProcessingData
		{
			// Antialiasing
			public bool bIsAntialiasingEnabled = false;
			public AntialiasingModel.FxaaPreset eAntialiasingPreset = AntialiasingModel.FxaaPreset.Default;

			// Ambient Occlusion
			public bool bIsAmbientOcclusionEnabled = false;
			public int iAmbientOcclusionLvlIdx = 0;

			// Screen Space Reflection
			public bool bIsScreenSpaceReflectionEnabled = false;
			public int iScreenSpaceReflectionLvlIdx = 0;

			// Depth Of Field
			public bool bIsDepthOfFieldEnabled = false;
			public int iDepthOfFieldLvlIdx = 0;

			// MotionBlur
			public bool bIsMotionBlurEnabled = false;

			// Bloom
			public bool bIsBloomEnabled = false;

			// Chromatic Aberration
			public bool bIsChromaticAberrationEnabled = false;

			public bool isDirty = false;
		}

		// ---------------------------
		[System.Serializable]
		public class VideoData
		{
			[SerializeField]
			private ScreenData m_ScreenData = new ScreenData();
			[SerializeField]
			private QualityData m_QualityData = new QualityData();
			[SerializeField]
			private FiltersData m_FiltersData = new FiltersData();
			[SerializeField]
			private PostProcessingData m_PostProcessingData = new PostProcessingData();

			public ScreenData ScreenData => m_ScreenData;
			public QualityData QualityData => m_QualityData;
			public FiltersData FiltersData => m_FiltersData;
			public PostProcessingData PostProcessingData => m_PostProcessingData;

		}
		private static VideoData m_VideoData = new VideoData();
		private static Resolution[] m_AvailableResolutions = null;

		public static ScreenData GetScreenData() => m_VideoData.ScreenData;
		public static QualityData GetQualityData() => m_VideoData.QualityData;
		public static FiltersData GetFiltersData() => m_VideoData.FiltersData;
		public static PostProcessingData GetPostProcessingData() => m_VideoData.PostProcessingData;
		public static Resolution[] GetAvailableResolutions() => m_AvailableResolutions;
		public static PostProcessingProfile m_PP_Profile { get;  private set; } = null;

		/////////////////////////////////////////////////////////////////
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

			LoadOrSetDefaults();

			OnApplyChanges();
		}


		/////////////////////////////////////////////////////////////////
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


		/////////////////////////////////////////////////////////////////
		public static void OnResolutionChosen(Resolution resolution, int resolutionIndex)
		{
			m_VideoData.ScreenData.resolution = resolution;
			m_VideoData.ScreenData.iResolutionIndex = resolutionIndex;
			m_VideoData.ScreenData.isDirty = true;
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OnFullScreenSet(bool newValue)
		{
			m_VideoData.ScreenData.bIsFullScreen = newValue;
			m_VideoData.ScreenData.isDirty = true;
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OnAnisotropicFilterSet(bool newValue)
		{
			m_VideoData.FiltersData.bHasAnisotropicFilter = newValue;
			m_VideoData.FiltersData.isDirty = true;
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OnAntialiasingSet(int newIndex)
		{
			m_VideoData.FiltersData.iAntialiasing = Mathf.Clamp(newIndex, 0, 3);// Disabled, 2x, 4x, 8x
			m_VideoData.FiltersData.isDirty = true;
			m_VideoData.PostProcessingData.isDirty = true;

			m_VideoData.PostProcessingData.bIsAntialiasingEnabled = newIndex > 0;
			m_VideoData.PostProcessingData.eAntialiasingPreset = (AntialiasingModel.FxaaPreset)Mathf.Max(0, (newIndex - 1));
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OnQualityLevelSet(int newiIndex)
		{
			m_VideoData.QualityData.iQualityLevel = newiIndex;
			m_VideoData.QualityData.isDirty = true;
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OnMotionBlurSetEnabled(bool bIsEnabled)
		{
			m_VideoData.PostProcessingData.bIsMotionBlurEnabled = bIsEnabled;
			m_VideoData.PostProcessingData.isDirty = true;
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OnBloomSetEnabled(bool bIsEnabled)
		{
			m_VideoData.PostProcessingData.bIsBloomEnabled = bIsEnabled;
			m_VideoData.PostProcessingData.isDirty = true;
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OnChromaticAberrationSetEnabled(bool bIsEnabled)
		{
			m_VideoData.PostProcessingData.bIsChromaticAberrationEnabled = bIsEnabled;
			m_VideoData.PostProcessingData.isDirty = true;
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OnDepthOfFieldSetEnabled(bool bIsEnabled)
		{
			m_VideoData.PostProcessingData.bIsDepthOfFieldEnabled = bIsEnabled;
			m_VideoData.PostProcessingData.isDirty = true;
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OnDepthOfFieldSetLvl(int level)
		{
			m_VideoData.PostProcessingData.iDepthOfFieldLvlIdx = level;
			m_VideoData.PostProcessingData.isDirty = true;
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OnScreenSpaceReflectionSetEnabled(bool bIsEnabled)
		{
			m_VideoData.PostProcessingData.bIsScreenSpaceReflectionEnabled = bIsEnabled;
			m_VideoData.PostProcessingData.isDirty = true;
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OnScreenSpaceReflectionSetLvl(int level)
		{
			m_VideoData.PostProcessingData.iScreenSpaceReflectionLvlIdx = level;
			m_VideoData.PostProcessingData.isDirty = true;
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OnAmbientOcclusionSetEnabled(bool bIsEnabled)
		{
			m_VideoData.PostProcessingData.bIsAmbientOcclusionEnabled = bIsEnabled;
			m_VideoData.PostProcessingData.isDirty = true;
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OnAmbientOcclusionSetLvl(int level)
		{
			m_VideoData.PostProcessingData.iAmbientOcclusionLvlIdx = level;
			m_VideoData.PostProcessingData.isDirty = true;
		}

		//////////////////////////////////////////////////////////////////////////
		public static void LoadOrSetDefaults()
		{
			if (PlayerPrefs.HasKey(FLAG_SAVED_GRAPHIC_SETTINGS))
			{
				ReadFromRegistry();
			}
			else
			{
				ApplyDefaults();

				PlayerPrefs.SetString(FLAG_SAVED_GRAPHIC_SETTINGS, "1");
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public static void ApplyDefaults()
		{
			// Remove keys from registry
			Reset();
			{
				// Screen
				m_VideoData.ScreenData.resolution = new Resolution() { width = 800, height = 600, refreshRate = 60 };
				m_VideoData.ScreenData.iResolutionIndex = GetResolutionIndex(m_VideoData.ScreenData.resolution);
				m_VideoData.ScreenData.bIsFullScreen = true;
				m_VideoData.ScreenData.isDirty = true;

				// Filters
				m_VideoData.FiltersData.bHasAnisotropicFilter = false;
				m_VideoData.FiltersData.iAntialiasing = 0;
				m_VideoData.FiltersData.isDirty = true;

				// Quality
				m_VideoData.QualityData.iQualityLevel = 0;
				m_VideoData.QualityData.isDirty = true;

				// Post-Processing
				m_VideoData.PostProcessingData.bIsAntialiasingEnabled = false;
				m_VideoData.PostProcessingData.eAntialiasingPreset = AntialiasingModel.FxaaPreset.ExtremePerformance;
				m_VideoData.PostProcessingData.bIsAmbientOcclusionEnabled = false;
				m_VideoData.PostProcessingData.iAmbientOcclusionLvlIdx = 0;
				m_VideoData.PostProcessingData.bIsScreenSpaceReflectionEnabled = false;
				m_VideoData.PostProcessingData.iScreenSpaceReflectionLvlIdx = 0;
				m_VideoData.PostProcessingData.bIsDepthOfFieldEnabled = false;
				m_VideoData.PostProcessingData.iDepthOfFieldLvlIdx = 0;
				m_VideoData.PostProcessingData.bIsMotionBlurEnabled = false;
				m_VideoData.PostProcessingData.bIsBloomEnabled = false;
				m_VideoData.PostProcessingData.bIsChromaticAberrationEnabled = false;
			}
			// Save new keys into registry
			SaveToRegistry();

			// Apply the default settings
			OnApplyChanges();
		}

		//////////////////////////////////////////////////////////////////////////
		public static void ReadFromRegistry()
		{
			string settings = PlayerPrefs.GetString(VAR_VIDEO_SETTINGS);
			JsonUtility.FromJsonOverwrite(settings, m_VideoData);

			m_VideoData.ScreenData.isDirty = true;
			m_VideoData.QualityData.isDirty = true;
			m_VideoData.FiltersData.isDirty = true;
			m_VideoData.PostProcessingData.isDirty = true;
		}

		//////////////////////////////////////////////////////////////////////////
		public static void SaveToRegistry()
		{
			string setting = JsonUtility.ToJson(m_VideoData);

			// Save settings
			PlayerPrefs.SetString(VAR_VIDEO_SETTINGS, setting);
		}

		//////////////////////////////////////////////////////////////////////////
		public static void SetPPProfile(PostProcessingProfile PP_Profile)
		{
			m_PP_Profile = PP_Profile;
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OnApplyChanges()
		{
			// Post	Processes
			{
				if (m_VideoData.PostProcessingData.isDirty && m_PP_Profile)
				{
					m_VideoData.PostProcessingData.isDirty = false;

					{   // Ambient Occlusion
						m_PP_Profile.ambientOcclusion.enabled = m_VideoData.PostProcessingData.bIsAmbientOcclusionEnabled;
					}
					{   // Screen Space Reflection
						m_PP_Profile.screenSpaceReflection.enabled = m_VideoData.PostProcessingData.bIsScreenSpaceReflectionEnabled;
					}
					{   // Depth Of Field
						m_PP_Profile.depthOfField.enabled = m_VideoData.PostProcessingData.bIsDepthOfFieldEnabled;
					}
					{   // Motion Blur
						m_PP_Profile.motionBlur.enabled = m_VideoData.PostProcessingData.bIsMotionBlurEnabled;
					}
					{   // Bloom
						m_PP_Profile.bloom.enabled = m_VideoData.PostProcessingData.bIsBloomEnabled;
					}
					{   // Chromatic Aberration
						m_PP_Profile.chromaticAberration.enabled = m_VideoData.PostProcessingData.bIsChromaticAberrationEnabled;

					}
					{   // Anti-Aliasing
						m_PP_Profile.antialiasing.enabled = m_VideoData.PostProcessingData.bIsAntialiasingEnabled;

				//		using (new PPSettings<AntialiasingModel.Settings>(m_PP_Profile.antialiasing, out AntialiasingModel.Settings settings))
						{
				//			settings.fxaaSettings.preset = m_VideoData.PostProcessingData.eAntialiasingPreset;
						}
					}
				}
			}

			// Screen
			if (m_VideoData.ScreenData.isDirty)
			{
				m_VideoData.ScreenData.isDirty = false;
				Screen.SetResolution(
					width: m_VideoData.ScreenData.resolution.width,
					height: m_VideoData.ScreenData.resolution.height,
					fullscreen: m_VideoData.ScreenData.bIsFullScreen,
					preferredRefreshRate: m_VideoData.ScreenData.resolution.refreshRate
				);
				m_OnResolutionChanged(m_VideoData.ScreenData.resolution.width, m_VideoData.ScreenData.resolution.height);
			}

			// Filter
			if (m_VideoData.FiltersData.isDirty)
			{
				m_VideoData.FiltersData.isDirty = false;

				QualitySettings.anisotropicFiltering = m_VideoData.FiltersData.bHasAnisotropicFilter ? AnisotropicFiltering.Enable : AnisotropicFiltering.Disable;
				QualitySettings.antiAliasing = (int)Mathf.Pow(2.0f, m_VideoData.FiltersData.iAntialiasing);
			}

			// Quality
			if (m_VideoData.QualityData.isDirty)
			{
				m_VideoData.QualityData.isDirty = false;

				QualitySettings.SetQualityLevel(m_VideoData.QualityData.iQualityLevel, applyExpensiveChanges: true);
			}

			// Save settings
			SaveToRegistry();
		}

		//////////////////////////////////////////////////////////////////////////
		public static void Reset()
		{
			PlayerPrefs.DeleteKey(FLAG_SAVED_GRAPHIC_SETTINGS);
			PlayerPrefs.DeleteKey(VAR_VIDEO_SETTINGS);
		}
	}

	public static class GameplaySettings
	{

	}
}