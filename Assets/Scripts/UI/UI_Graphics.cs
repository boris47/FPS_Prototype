
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_Graphics : UI_Base, IStateDefiner
{
	// UI Components
	private				Dropdown			m_ResolutionDropDown				= null;
	private				Toggle				m_FullScreenToggle					= null;
	private				Toggle				m_AnisotropicFilterToggle			= null;
	private				Dropdown			m_AntialiasingDropDown				= null;
	private				Dropdown			m_QualityLevelDropDown				= null;
	private				Toggle				m_MotionBlurToggle					= null;
	private				Toggle				m_BloomToggle						= null;
	private				Toggle				m_ChromaticAberrationToggle			= null;

	private				Toggle				m_AmbientOcclusionToggle			= null;
	private				Dropdown			m_AmbientOcclusionDropDown			= null;
	private				Toggle				m_ScreenSpaceReflectionToggle		= null;
	private				Dropdown			m_ScreenSpaceReflectionDropDown		= null;
	private				Toggle				m_DepthOfFieldToggle				= null;
	private				Dropdown			m_DepthOfFieldDropDown				= null;

	private				Button				m_ApplyButton						= null;
	private				Button				m_ResetButton						= null;
	private				Button				m_BackButton						= null;

	private				bool				m_IsInitialized						= false;
						bool				IStateDefiner.IsInitialized			=> m_IsInitialized;


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.PreInit()
	{

	}

	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.Initialize()
	{
		if (!m_IsInitialized)
		{
			// Set default data or load saved ones
			UserSettings.VideoSettings.LoadOrSetDefaults();

			// Resolutions
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("ResolutionsDropDown", out m_ResolutionDropDown)))
			{
				Resolution[] availableResolutions = UserSettings.VideoSettings.GetAvailableResolutions();
				m_ResolutionDropDown.AddOptions
				(
					new List<Resolution>(availableResolutions)
					.ConvertAll(new System.Converter<Resolution, string>(res => res.ToString()))
				);
				m_ResolutionDropDown.value = UserSettings.VideoSettings.GetScreenData().iResolutionIndex;
				m_ResolutionDropDown.onValueChanged.AddListener(index =>
				{
					UserSettings.VideoSettings.OnResolutionChosen(availableResolutions[index], index);
					m_ApplyButton.interactable = true;
				});
			}

			// Quality level
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("QualityLevelDropDown", out m_QualityLevelDropDown)))
			{
				m_QualityLevelDropDown.AddOptions(new List<string>(QualitySettings.names));
				m_QualityLevelDropDown.value = UserSettings.VideoSettings.GetQualityData().iQualityLevel;
				m_QualityLevelDropDown.onValueChanged.AddListener(newIndex =>
				{
					UserSettings.VideoSettings.OnQualityLevelSet(newIndex);
					m_ApplyButton.interactable = true;
				});
			}

			// Full Screen Toggle
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("FullScreenToggle", out m_FullScreenToggle)))
			{
				m_FullScreenToggle.isOn = UserSettings.VideoSettings.GetScreenData().bIsFullScreen;
#if UNITY_EDITOR
			{	// TODO Not Working -.-
				System.Reflection.Assembly assembly = typeof(UnityEditor.EditorWindow).Assembly;
				System.Type type = assembly.GetType("UnityEditor.PlayModeView");
				UnityEditor.EditorWindow.GetWindow(type).maximized = m_FullScreenToggle.isOn;
			}
#endif
				m_FullScreenToggle.onValueChanged.AddListener(newValue =>
				{
					UserSettings.VideoSettings.OnFullScreenSet(newValue);
#if UNITY_EDITOR
				{
					System.Reflection.Assembly assembly = typeof(UnityEditor.EditorWindow).Assembly;
					System.Type type = assembly.GetType("UnityEditor.PlayModeView");
					UnityEditor.EditorWindow.GetWindow(type).maximized = newValue;
				}
#endif
					m_ApplyButton.interactable = true;
				});
			}

			// Anisotropic Filter Toggle
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("AnisotropicFilterToggle", out m_AnisotropicFilterToggle)))
			{
				m_AnisotropicFilterToggle.isOn = UserSettings.VideoSettings.GetFiltersData().bHasAnisotropicFilter;
				m_AnisotropicFilterToggle.onValueChanged.AddListener(newValue =>
				{
					UserSettings.VideoSettings.OnAnisotropicFilterSet(newValue);
					m_AntialiasingDropDown.interactable = newValue;
					m_ApplyButton.interactable = true;
				});
			}

			// Anti-aliasing
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("AntialiasingDropDown", out m_AntialiasingDropDown)))
			{
				m_AntialiasingDropDown.AddOptions
				(
					new List<string>(new string[] { "None", "2x", "4x", "8x", "16x", "32x" })
				);
				m_AntialiasingDropDown.value = (int)UserSettings.VideoSettings.GetPostProcessingData().eAntialiasingPreset;
				m_AntialiasingDropDown.onValueChanged.AddListener(newIndex =>
				{
					UserSettings.VideoSettings.OnAntialiasingSet(newIndex);
					m_ApplyButton.interactable = true;
				});
			}

			// Motion Blur Toggle
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("MotionBlurToggle", out m_MotionBlurToggle)))
			{
				m_MotionBlurToggle.isOn = UserSettings.VideoSettings.GetPostProcessingData().bIsMotionBlurEnabled;
				m_MotionBlurToggle.onValueChanged.AddListener(newValue =>
				{
					UserSettings.VideoSettings.OnMotionBlurSetEnabled(newValue);
					m_ApplyButton.interactable = true;
				});
			}

			// Bloom Toggle
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("BloomToggle", out m_BloomToggle)))
			{
				m_BloomToggle.isOn = UserSettings.VideoSettings.GetPostProcessingData().bIsBloomEnabled;
				m_BloomToggle.onValueChanged.AddListener(newValue =>
				{
					UserSettings.VideoSettings.OnBloomSetEnabled(newValue);
					m_ApplyButton.interactable = true;
				});
			}

			// Chromatic Aberration Toggle
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("ChromaticAberrationToggle", out m_ChromaticAberrationToggle)))
			{
				m_ChromaticAberrationToggle.isOn = UserSettings.VideoSettings.GetPostProcessingData().bIsChromaticAberrationEnabled;
				m_ChromaticAberrationToggle.onValueChanged.AddListener(newValue =>
				{
					UserSettings.VideoSettings.OnChromaticAberrationSetEnabled(newValue);
					m_ApplyButton.interactable = true;
				});
			}

			// Ambient Occlusion Toggle
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("AmbientOcclusionToggle", out m_AmbientOcclusionToggle)))
			{
				m_AmbientOcclusionToggle.isOn = UserSettings.VideoSettings.GetPostProcessingData().bIsAmbientOcclusionEnabled;
				m_AmbientOcclusionToggle.onValueChanged.AddListener(newValue =>
				{
					UserSettings.VideoSettings.OnAmbientOcclusionSetEnabled(newValue);
					m_AmbientOcclusionDropDown.interactable = newValue;
					m_ApplyButton.interactable = true;
				});
			}

			//  Ambient Occlusion DropDown
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("AmbientOcclusionDropDown", out m_AmbientOcclusionDropDown)))
			{
				m_AmbientOcclusionDropDown.AddOptions
				(
					new List<string>(new string[3] { "Low", "Normal", "High" })
				);
				m_AmbientOcclusionDropDown.interactable = m_AmbientOcclusionToggle.isOn;
				m_AmbientOcclusionDropDown.value = UserSettings.VideoSettings.GetPostProcessingData().iAmbientOcclusionLvlIdx;
				m_AmbientOcclusionDropDown.onValueChanged.AddListener(newIndex =>
				{
					UserSettings.VideoSettings.OnAmbientOcclusionSetLvl(newIndex);
					m_ApplyButton.interactable = true;
				});
			}

			// Screen Space Reflection Toggle
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("ScreenSpaceReflectionToggle", out m_ScreenSpaceReflectionToggle)))
			{
				m_ScreenSpaceReflectionToggle.isOn = UserSettings.VideoSettings.GetPostProcessingData().bIsScreenSpaceReflectionEnabled;
				m_ScreenSpaceReflectionToggle.onValueChanged.AddListener(newValue =>
				{
					UserSettings.VideoSettings.OnScreenSpaceReflectionSetEnabled(newValue);
					m_ScreenSpaceReflectionDropDown.interactable = newValue;
					m_ApplyButton.interactable = true;
				});
			}

			//  Ambient Occlusion DropDown
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("ScreenSpaceReflectionDropDown", out m_ScreenSpaceReflectionDropDown)))
			{
				m_ScreenSpaceReflectionDropDown.AddOptions
				(
					new List<string>(new string[3] { "Low", "Normal", "High" })
				);
				m_ScreenSpaceReflectionDropDown.interactable = m_ScreenSpaceReflectionToggle.isOn;
				m_ScreenSpaceReflectionDropDown.value = UserSettings.VideoSettings.GetPostProcessingData().iAmbientOcclusionLvlIdx;
				m_ScreenSpaceReflectionDropDown.onValueChanged.AddListener(newIndex =>
				{
					UserSettings.VideoSettings.OnScreenSpaceReflectionSetLvl(newIndex);
					m_ApplyButton.interactable = true;
				});
			}

			// Depth Of Field Toggle
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("DepthOfFieldToggle", out m_DepthOfFieldToggle)))
			{
				m_DepthOfFieldToggle.isOn = UserSettings.VideoSettings.GetPostProcessingData().bIsDepthOfFieldEnabled;
				m_DepthOfFieldToggle.onValueChanged.AddListener(newValue =>
				{
					UserSettings.VideoSettings.OnDepthOfFieldSetEnabled(newValue);
					m_DepthOfFieldDropDown.interactable = newValue;
					m_ApplyButton.interactable = true;
				});
			}

			// Depth Of Field DropDown
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("DepthOfFieldDropDown", out m_DepthOfFieldDropDown)))
			{
				m_DepthOfFieldDropDown.AddOptions
				(
					new List<string>(new string[3] { "Low", "Normal", "High" })
				);
				m_DepthOfFieldDropDown.interactable = m_DepthOfFieldToggle.isOn;
				m_DepthOfFieldDropDown.value = UserSettings.VideoSettings.GetPostProcessingData().iDepthOfFieldLvlIdx;
				m_DepthOfFieldDropDown.onValueChanged.AddListener(newIndex =>
				{
					UserSettings.VideoSettings.OnDepthOfFieldSetLvl(newIndex);
					m_ApplyButton.interactable = true;
				});
			}

			// Apply button
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("ApplyButton", out m_ApplyButton)))
			{
				void OnConfirm()
				{
					UserSettings.VideoSettings.OnApplyChanges();

					m_ApplyButton.interactable = false;
				}
				m_ApplyButton.onClick.AddListener(() => UIManager.Confirmation.Show("Apply Changes?", OnConfirm));
			}


			// Reset button
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("ResetButton", out m_ResetButton)))
			{
				void OnConfirm()
				{
					UserSettings.VideoSettings.Reset();

					// Update UI elements
					UpdateUI();

					m_ApplyButton.interactable = false;
					m_ResetButton.interactable = false;
				}
				m_ResetButton.onClick.AddListener(() => UIManager.Confirmation.Show("Reset?", OnConfirm));
			}

			// Back button
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Button_Back", out m_BackButton)))
			{
				m_BackButton.onClick.AddListener(() => UIManager.Instance.GoBack());
			}

			// disable navigation for everything
			Navigation noNavigationMode = new Navigation() { mode = Navigation.Mode.None };
			foreach (Selectable s in GetComponentsInChildren<Selectable>())
			{
				s.navigation = noNavigationMode;
			}

			m_IsInitialized = true;
		}

		// Update UI elements
		UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.ReInit()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	bool IStateDefiner.Finalize()
	{
		return m_IsInitialized;
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		CustomAssertions.IsTrue(m_IsInitialized);

		m_ApplyButton.interactable = false;
	}


	//////////////////////////////////////////////////////////////////////////
	public void UpdateUI()
	{
		m_ResolutionDropDown.value = UserSettings.VideoSettings.GetScreenData().iResolutionIndex;
		m_FullScreenToggle.isOn = UserSettings.VideoSettings.GetScreenData().bIsFullScreen;
		m_AnisotropicFilterToggle.isOn = UserSettings.VideoSettings.GetFiltersData().bHasAnisotropicFilter;
		m_AntialiasingDropDown.value = UserSettings.VideoSettings.GetFiltersData().iAntialiasing;
		m_QualityLevelDropDown.value = UserSettings.VideoSettings.GetQualityData().iQualityLevel;
	}
}
