
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

public sealed class UI_Graphics : UI_Base, IUIOptions, IStateDefiner
{
	// UI Components
	private	Dropdown					m_ResolutionDropDown				= null;
	private	Toggle						m_FullScreenToogle					= null;
	private	Toggle						m_AnisotropicFilterToogle			= null;
	private	Dropdown					m_AntialiasingDropDown				= null;
	private	Dropdown					m_QualityLevelDropDown				= null;
	private	Toggle						m_MotionBlurToggle					= null;
	private	Toggle						m_BloomToggle						= null;
	private	Toggle						m_ChromaticAberrationToggle			= null;

	private	Toggle						m_AmbientOcclusionToggle			= null;
	private	Dropdown					m_AmbientOcclusionDropDown			= null;
	private	Toggle						m_ScreenSpaceReflectionToggle		= null;
	private	Dropdown					m_ScreenSpaceReflectionDropDown		= null;
	private	Toggle						m_DepthOfFieldToggle				= null;
	private	Dropdown					m_DepthOfFieldDropDown				= null;

	private	Button						m_ApplyButton						= null;
	private	Button						m_ResetButton						= null;

	private	bool						m_IsInitialized					= false;

	#region IStateDefiner

	//------------------------------------------------------------
	bool IStateDefiner.IsInitialized => m_IsInitialized = false;


	//------------------------------------------------------------
	string IStateDefiner.StateName => name;


	//////////////////////////////////////////////////////////////////////////
	public void PreInit() { }

	//////////////////////////////////////////////////////////////////////////
	IEnumerator IStateDefiner.Initialize()
	{
		if (m_IsInitialized == true)
			yield break;

		CoroutinesManager.AddCoroutineToPendingCount(1);

		m_IsInitialized = true;
		{
			Navigation noNavigationMode = new Navigation() { mode = Navigation.Mode.None };

			yield return null;

			if (m_IsInitialized &= transform.TrySearchComponentByChildName("ResolutionsDropDown", out m_ResolutionDropDown))
			{
				Resolution[] availableResolutions = UserSettings.VideoSettings.GetAvailableResolutions();
				m_ResolutionDropDown.onValueChanged.AddListener((int index) =>
				{
					UserSettings.VideoSettings.OnResolutionChosen(availableResolutions[index], index);
					m_ApplyButton.interactable = true;
				});
				m_ResolutionDropDown.AddOptions(
					new List<Resolution>(availableResolutions).ConvertAll
					(
						new System.Converter<Resolution, string>((Resolution res) => { return res.ToString(); })
					)
				);
			}

			yield return null;

			if (m_IsInitialized &= transform.TrySearchComponentByChildName("QualityLevelDropDown", out m_QualityLevelDropDown))
			{
				m_QualityLevelDropDown.onValueChanged.AddListener((int newiIndex) =>
				{
					UserSettings.VideoSettings.OnQualityLevelSet(newiIndex);
					m_ApplyButton.interactable = true;
				});
				m_QualityLevelDropDown.AddOptions(new List<string>(QualitySettings.names));
			}

			yield return null;

			if (m_IsInitialized &= transform.TrySearchComponentByChildName("FullScreenToggle", out m_FullScreenToogle))
			{
				m_FullScreenToogle.onValueChanged.AddListener((bool newValue) =>
				{
					UserSettings.VideoSettings.OnFullScreenSet(newValue);
					m_ApplyButton.interactable = true;
				});
			}

			yield return null;

			if (m_IsInitialized &= transform.TrySearchComponentByChildName("AnisotropicFilterToogle", out m_AnisotropicFilterToogle))
			{
				m_AnisotropicFilterToogle.onValueChanged.AddListener((bool newValue) =>
				{
					UserSettings.VideoSettings.OnAnisotropicFilterSet(newValue);
					m_ApplyButton.interactable = true;
				});
			}

			yield return null;

			if (m_IsInitialized &= transform.TrySearchComponentByChildName("AntialiasingDropDown", out m_AntialiasingDropDown))
			{
				m_AntialiasingDropDown.onValueChanged.AddListener((int newIndex) =>
				{
					UserSettings.VideoSettings.OnAntialiasingSet(newIndex);
					m_ApplyButton.interactable = true;
				});
				m_AntialiasingDropDown.AddOptions(
					new List<string>(new string[7] { "None", "2x", "4x", "8x", "16x", "32x", "64x" })
				);
			}

			yield return null;

			if (m_IsInitialized &= transform.TrySearchComponentByChildName("MotionBlurToogle", out m_MotionBlurToggle))
			{
				m_MotionBlurToggle.onValueChanged.AddListener((bool bIsEnabled) =>
				{
					UserSettings.VideoSettings.OnMotionBlurSetEnabled(bIsEnabled);
					m_ApplyButton.interactable = true;
				});
			}

			yield return null;

			if (m_IsInitialized &= transform.TrySearchComponentByChildName("BloomToogle", out m_BloomToggle))
			{
				m_BloomToggle.onValueChanged.AddListener((bool bIsEnabled) =>
				{
					UserSettings.VideoSettings.OnBloomSetEnabled(bIsEnabled);
					m_ApplyButton.interactable = true;
				});
			}

			yield return null;

			if (m_IsInitialized &= transform.TrySearchComponentByChildName("ChromaticAberrationToogle", out m_ChromaticAberrationToggle))
			{
				m_ChromaticAberrationToggle.onValueChanged.AddListener((bool bIsEnabled) =>
				{
					UserSettings.VideoSettings.OnChromaticAberrationSetEnabled(bIsEnabled);
					m_ApplyButton.interactable = true;
				});
			}

			if (m_IsInitialized &= transform.TrySearchComponentByChildName("AmbientOcclusionToggle", out m_AmbientOcclusionToggle))
			{
				m_AmbientOcclusionToggle.onValueChanged.AddListener((bool bIsEnabled) =>
				{
					UserSettings.VideoSettings.OnAmbientOcclusionSetEnabled(bIsEnabled);
					m_ApplyButton.interactable = true;
					m_AmbientOcclusionDropDown.interactable = bIsEnabled;
				});
			}

			if (m_IsInitialized &= transform.TrySearchComponentByChildName("AmbientOcclusionDropDown", out m_AmbientOcclusionDropDown))
			{
				m_AmbientOcclusionDropDown.interactable = m_AmbientOcclusionToggle.isOn;
				m_AmbientOcclusionDropDown.onValueChanged.AddListener((int level) =>
				{
					UserSettings.VideoSettings.OnAmbientOcclusionSetLvl(level);
					m_ApplyButton.interactable = true;
				});
				m_AmbientOcclusionDropDown.AddOptions(
					new List<string>(new string[3] { "Low", "Normal", "High" })
				);
			}

			yield return null;

			if (m_IsInitialized &= transform.TrySearchComponentByChildName("ScreenSpaceReflectionToggle", out m_ScreenSpaceReflectionToggle))
			{
				m_ScreenSpaceReflectionToggle.onValueChanged.AddListener((bool bIsEnabled) =>
				{
					UserSettings.VideoSettings.OnScreenSpaceReflectionSetEnabled(bIsEnabled);
					m_ApplyButton.interactable = true;
					m_ScreenSpaceReflectionDropDown.interactable = bIsEnabled;
				});
			}

			if (m_IsInitialized &= transform.TrySearchComponentByChildName("ScreenSpaceReflectionDropDown", out m_ScreenSpaceReflectionDropDown))
			{
				m_ScreenSpaceReflectionDropDown.interactable = m_ScreenSpaceReflectionToggle.isOn;
				m_ScreenSpaceReflectionDropDown.onValueChanged.AddListener((int level) =>
				{
					UserSettings.VideoSettings.OnScreenSpaceReflectionSetLvl(level);
					m_ApplyButton.interactable = true;
				});
				m_ScreenSpaceReflectionDropDown.AddOptions(
					new List<string>(new string[3] { "Low", "Normal", "High" })
				);
			}

			yield return null;

			if (m_IsInitialized &= transform.TrySearchComponentByChildName("DepthOfFieldToggle", out m_DepthOfFieldToggle))
			{
				m_DepthOfFieldToggle.onValueChanged.AddListener((bool bIsEnabled) =>
				{
					UserSettings.VideoSettings.OnDepthOfFieldSetEnabled(bIsEnabled);
					m_ApplyButton.interactable = true;
					m_DepthOfFieldDropDown.interactable = bIsEnabled;
				});
			}

			if (m_IsInitialized &= transform.TrySearchComponentByChildName("DepthOfFieldDropDown", out m_DepthOfFieldDropDown))
			{
				m_DepthOfFieldDropDown.interactable = m_DepthOfFieldToggle.isOn;
				m_DepthOfFieldDropDown.onValueChanged.AddListener((int level) =>
				{
					UserSettings.VideoSettings.OnDepthOfFieldSetLvl(level);
					m_ApplyButton.interactable = true;
				});
				m_DepthOfFieldDropDown.AddOptions(
					new List<string>(new string[3] { "Low", "Normal", "High" })
				);
			}

			yield return null;


			if (m_IsInitialized &= transform.TrySearchComponentByChildName("ApplyButton", out m_ApplyButton))
			{
				m_ApplyButton.onClick.AddListener
				(
					delegate ()
					{
						UIManager.Confirmation.Show("Apply Changes?", OnApplyChanges, () => { UserSettings.VideoSettings.ReadFromRegistry(); UpdateUI(); });
					}
				);
				m_ApplyButton.interactable = false;
			}

			yield return null;

			if (m_IsInitialized &= transform.TrySearchComponentByChildName("ResetButton", out m_ResetButton))
			{
				m_ResetButton.onClick.AddListener
				(
					delegate ()
					{
						UIManager.Confirmation.Show("Reset?", () => { UserSettings.VideoSettings.ApplyDefaults(); UpdateUI(); });
					}
				);
			}

			// disable navigation for everything
			foreach (Selectable s in GetComponentsInChildren<Selectable>())
			{
				s.navigation = noNavigationMode;
			}

			yield return null;

			if (m_IsInitialized)
			{
				OnEnable();

				yield return null;

				OnApplyChanges();

				CoroutinesManager.RemoveCoroutineFromPendingCount(1);
			}
			else
			{
				Debug.LogError("UI_Graphics: Bad initialization!!!");
			}

		}
	}


	//////////////////////////////////////////////////////////////////////////
	IEnumerator IStateDefiner.ReInit()
	{
		yield return null;
	}


	//////////////////////////////////////////////////////////////////////////
	bool IStateDefiner.Finalize()
	{
		return m_IsInitialized;
	}

	#endregion

	//////////////////////////////////////////////////////////////////////////
	public void OnEnable()
	{
		if (m_IsInitialized == false)
		{
			return;
		}

		UserSettings.VideoSettings.OnEnable();
		m_ApplyButton.interactable = false;
		UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Apply changes </summary>
	public void OnApplyChanges()
	{
		if (m_IsInitialized == false)
		{
			return;
		}

		UserSettings.VideoSettings.OnApplyChanges();
		m_ApplyButton.interactable = false;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Updates UI Components </summary>
	public void UpdateUI()
	{
		if (m_IsInitialized == false)
		{
			return;
		}

		m_ResolutionDropDown.value = UserSettings.VideoSettings.GetScreenData().iResolutionIndex;
		m_FullScreenToogle.isOn = UserSettings.VideoSettings.GetScreenData().bIsFullScreen;
		m_AnisotropicFilterToogle.isOn = UserSettings.VideoSettings.GetFiltersData().bHasAnisotropicFilter;
		m_AntialiasingDropDown.value = UserSettings.VideoSettings.GetFiltersData().iAntialiasing;
		m_QualityLevelDropDown.value = UserSettings.VideoSettings.GetQualityData().iQualityLevel;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Remove key from registry </summary>
	public void Reset()
	{
		if (m_IsInitialized == false)
		{
			return;
		}

		UserSettings.VideoSettings.Reset();
	}

}
