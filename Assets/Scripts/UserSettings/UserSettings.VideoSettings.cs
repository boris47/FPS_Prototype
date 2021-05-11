using UnityEngine;
using UnityEngine.PostProcessing;
using System.Collections.Generic;

namespace UserSettings
{	
	public delegate void OnResolutionChangedDelegate(float newWidth, float newHeight);

	// ---------------------------
	[System.Serializable]
	public class ScreenData
	{
		public static Resolution defaultResolution = new Resolution() { width = 640, height = 480, refreshRate = 50 };
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


	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////


	public static class VideoSettings
	{
		private const string PostProcessResourcePath = "Scriptables/CameraPostProcesses";

		// Registry Keys
		private const string FLAG_SAVED_GRAPHIC_SETTINGS = "bSavedVideoSettings";
		private const string VAR_VIDEO_SETTINGS = "sVideoSettings";


		private static event	OnResolutionChangedDelegate		m_OnResolutionChanged			= delegate { };
		private static			VideoData						m_VideoData						= new VideoData();
		private static			Resolution[]					m_AvailableResolutions			= null;
		private static			PostProcessingProfile			m_PP_Profile					= null;


		public static event OnResolutionChangedDelegate OnResolutionChanged
		{
			add    { if (value.IsNotNull()) m_OnResolutionChanged += value; }
			remove { if (value.IsNotNull()) m_OnResolutionChanged -= value; }
		}

		public static ScreenData GetScreenData() => m_VideoData.ScreenData;
		public static QualityData GetQualityData() => m_VideoData.QualityData;
		public static FiltersData GetFiltersData() => m_VideoData.FiltersData;
		public static PostProcessingData GetPostProcessingData() => m_VideoData.PostProcessingData;
		public static Resolution[] GetAvailableResolutions() => m_AvailableResolutions;

		public static PostProcessingProfile GetPostProcessingProfile() => m_PP_Profile;

		/////////////////////////////////////////////////////////////////
		static VideoSettings()
		{
			static int comparer(Resolution a, Resolution b)
			{
				int mulA = a.width * a.height;
				int mulB = b.width * b.height;
				return mulA < mulB ? -1 : mulA > mulB ? 1 : 0;
			}

			List<Resolution> sortedResolutions = new List<Resolution>(Screen.resolutions);
			sortedResolutions.Sort(comparer);
			m_AvailableResolutions = sortedResolutions.ToArray();
		}

		/*
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
		*/

		/////////////////////////////////////////////////////////////////
		public static void OnResolutionChosen(Resolution resolution, int resolutionIndex)
		{
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
			if (!m_PP_Profile)
			{
				CustomAssertions.IsTrue(ResourceManager.LoadResourceSync(PostProcessResourcePath, out m_PP_Profile));
			}

			if (PlayerPrefs.HasKey(FLAG_SAVED_GRAPHIC_SETTINGS))
			{
				ReadFromRegistry();

				OnApplyChanges();
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
				m_VideoData.ScreenData.iResolutionIndex = 0;
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
					}
				}
			}

			// Screen
			if (m_VideoData.ScreenData.isDirty)
			{
				m_VideoData.ScreenData.isDirty = false;

				Resolution resolution = m_AvailableResolutions.GetByIndex(m_VideoData.ScreenData.iResolutionIndex, ScreenData.defaultResolution);
				Debug.Log($"Setting resolution {resolution.width}x{resolution.height}({resolution.refreshRate})");
				Screen.SetResolution(
					width: resolution.width,
					height: resolution.height,
					fullscreen: m_VideoData.ScreenData.bIsFullScreen,
					preferredRefreshRate: resolution.refreshRate
				);
				m_OnResolutionChanged(resolution.width, resolution.height);
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
}