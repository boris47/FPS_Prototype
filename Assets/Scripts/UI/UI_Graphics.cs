
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

public sealed class UI_Graphics : MonoBehaviour, IUIOptions, IStateDefiner
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
	bool IStateDefiner.IsInitialized => this.m_IsInitialized = false;


	//------------------------------------------------------------
	string IStateDefiner.StateName => this.name;


	//////////////////////////////////////////////////////////////////////////
	IEnumerator IStateDefiner.Initialize()
	{
		if (this.m_IsInitialized == true)
			yield break;

		CoroutinesManager.AddCoroutineToPendingCount(1);

		this.m_IsInitialized = true;
		{
			Navigation noNavigationMode = new Navigation() { mode = Navigation.Mode.None };

			yield return null;

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("ResolutionsDropDown", ref this.m_ResolutionDropDown))
			{
				Resolution[] availableResolutions = UserSettings.VideoSettings.GetAvailableResolutions();
				this.m_ResolutionDropDown.onValueChanged.AddListener((int index) =>
				{
					UserSettings.VideoSettings.OnResolutionChosen(availableResolutions[index], index);
					this.m_ApplyButton.interactable = true;
				});
				this.m_ResolutionDropDown.AddOptions(
					new List<Resolution>(availableResolutions).ConvertAll
					(
						new System.Converter<Resolution, string>((Resolution res) => { return res.ToString(); })
					)
				);
			}

			yield return null;

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("QualityLevelDropDown", ref this.m_QualityLevelDropDown))
			{
				this.m_QualityLevelDropDown.onValueChanged.AddListener((int newiIndex) =>
				{
					UserSettings.VideoSettings.OnQualityLevelSet(newiIndex);
					this.m_ApplyButton.interactable = true;
				});
				this.m_QualityLevelDropDown.AddOptions(new List<string>(QualitySettings.names));
			}

			yield return null;

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("FullScreenToggle", ref this.m_FullScreenToogle))
			{
				this.m_FullScreenToogle.onValueChanged.AddListener((bool newValue) =>
				{
					UserSettings.VideoSettings.OnFullScreenSet(newValue);
					this.m_ApplyButton.interactable = true;
				});
			}

			yield return null;

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("AnisotropicFilterToogle", ref this.m_AnisotropicFilterToogle))
			{
				this.m_AnisotropicFilterToogle.onValueChanged.AddListener((bool newValue) =>
				{
					UserSettings.VideoSettings.OnAnisotropicFilterSet(newValue);
					this.m_ApplyButton.interactable = true;
				});
			}

			yield return null;

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("AntialiasingDropDown", ref this.m_AntialiasingDropDown))
			{
				this.m_AntialiasingDropDown.onValueChanged.AddListener((int newIndex) =>
				{
					UserSettings.VideoSettings.OnAntialiasingSet(newIndex);
					this.m_ApplyButton.interactable = true;
				});
				this.m_AntialiasingDropDown.AddOptions(
					new List<string>(new string[7] { "None", "2x", "4x", "8x", "16x", "32x", "64x" })
				);
			}

			yield return null;

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("MotionBlurToogle", ref this.m_MotionBlurToggle))
			{
				this.m_MotionBlurToggle.onValueChanged.AddListener((bool bIsEnabled) =>
				{
					UserSettings.VideoSettings.OnMotionBlurSetEnabled(bIsEnabled);
					this.m_ApplyButton.interactable = true;
				});
			}

			yield return null;

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("BloomToogle", ref this.m_BloomToggle))
			{
				this.m_BloomToggle.onValueChanged.AddListener((bool bIsEnabled) =>
				{
					UserSettings.VideoSettings.OnBloomSetEnabled(bIsEnabled);
					this.m_ApplyButton.interactable = true;
				});
			}

			yield return null;

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("ChromaticAberrationToogle", ref this.m_ChromaticAberrationToggle))
			{
				this.m_ChromaticAberrationToggle.onValueChanged.AddListener((bool bIsEnabled) =>
				{
					UserSettings.VideoSettings.OnChromaticAberrationSetEnabled(bIsEnabled);
					this.m_ApplyButton.interactable = true;
				});
			}

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("AmbientOcclusionToggle", ref this.m_AmbientOcclusionToggle))
			{
				this.m_AmbientOcclusionToggle.onValueChanged.AddListener((bool bIsEnabled) =>
				{
					UserSettings.VideoSettings.OnAmbientOcclusionSetEnabled(bIsEnabled);
					this.m_ApplyButton.interactable = true;
					this.m_AmbientOcclusionDropDown.interactable = bIsEnabled;
				});
			}

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("AmbientOcclusionDropDown", ref this.m_AmbientOcclusionDropDown))
			{
				this.m_AmbientOcclusionDropDown.interactable = this.m_AmbientOcclusionToggle.isOn;
				this.m_AmbientOcclusionDropDown.onValueChanged.AddListener((int level) =>
				{
					UserSettings.VideoSettings.OnAmbientOcclusionSetLvl(level);
					this.m_ApplyButton.interactable = true;
				});
				this.m_AmbientOcclusionDropDown.AddOptions(
					new List<string>(new string[3] { "Low", "Normal", "High" })
				);
			}

			yield return null;

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("ScreenSpaceReflectionToggle", ref this.m_ScreenSpaceReflectionToggle))
			{
				this.m_ScreenSpaceReflectionToggle.onValueChanged.AddListener((bool bIsEnabled) =>
				{
					UserSettings.VideoSettings.OnScreenSpaceReflectionSetEnabled(bIsEnabled);
					this.m_ApplyButton.interactable = true;
					this.m_ScreenSpaceReflectionDropDown.interactable = bIsEnabled;
				});
			}

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("ScreenSpaceReflectionDropDown", ref this.m_ScreenSpaceReflectionDropDown))
			{
				this.m_ScreenSpaceReflectionDropDown.interactable = this.m_ScreenSpaceReflectionToggle.isOn;
				this.m_ScreenSpaceReflectionDropDown.onValueChanged.AddListener((int level) =>
				{
					UserSettings.VideoSettings.OnScreenSpaceReflectionSetLvl(level);
					this.m_ApplyButton.interactable = true;
				});
				this.m_ScreenSpaceReflectionDropDown.AddOptions(
					new List<string>(new string[3] { "Low", "Normal", "High" })
				);
			}

			yield return null;

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("DepthOfFieldToggle", ref this.m_DepthOfFieldToggle))
			{
				this.m_DepthOfFieldToggle.onValueChanged.AddListener((bool bIsEnabled) =>
				{
					UserSettings.VideoSettings.OnDepthOfFieldSetEnabled(bIsEnabled);
					this.m_ApplyButton.interactable = true;
					this.m_DepthOfFieldDropDown.interactable = bIsEnabled;
				});
			}

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("DepthOfFieldDropDown", ref this.m_DepthOfFieldDropDown))
			{
				this.m_DepthOfFieldDropDown.interactable = this.m_DepthOfFieldToggle.isOn;
				this.m_DepthOfFieldDropDown.onValueChanged.AddListener((int level) =>
				{
					UserSettings.VideoSettings.OnDepthOfFieldSetLvl(level);
					this.m_ApplyButton.interactable = true;
				});
				this.m_DepthOfFieldDropDown.AddOptions(
					new List<string>(new string[3] { "Low", "Normal", "High" })
				);
			}

			yield return null;


			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("ApplyButton", ref this.m_ApplyButton))
			{
				this.m_ApplyButton.onClick.AddListener
				(
					delegate ()
					{
						UIManager.Confirmation.Show("Apply Changes?", this.OnApplyChanges, () => { UserSettings.VideoSettings.ReadFromRegistry(); this.UpdateUI(); });
					}
				);
				this.m_ApplyButton.interactable = false;
			}

			yield return null;

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("ResetButton", ref this.m_ResetButton))
			{
				this.m_ResetButton.onClick.AddListener
				(
					delegate ()
					{
						UIManager.Confirmation.Show("Reset?", () => { UserSettings.VideoSettings.ApplyDefaults(); this.UpdateUI(); });
					}
				);
			}

			// disable navigation for everything
			foreach (Selectable s in this.GetComponentsInChildren<Selectable>())
			{
				s.navigation = noNavigationMode;
			}

			yield return null;

			if (this.m_IsInitialized)
			{
				this.OnEnable();

				yield return null;

				this.OnApplyChanges();

				CoroutinesManager.RemoveCoroutineFromPendingCount(1);

				yield return null;
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
		return this.m_IsInitialized;
	}

	#endregion

	//////////////////////////////////////////////////////////////////////////
	public void OnEnable()
	{
		if (this.m_IsInitialized == false)
		{
			return;
		}

		// Sprites for TargetToKill, LocationToReach or ObjectToInteractWith
		ResourceManager.LoadedData<PostProcessingProfile> cameraPostProcesses = new ResourceManager.LoadedData<PostProcessingProfile>();
		bool bLoadResult = ResourceManager.LoadResourceSync
		(
			ResourcePath: "Scriptables/CameraPostProcesses",
			loadedResource: cameraPostProcesses
		);

		UnityEngine.Assertions.Assert.IsTrue
		(
			bLoadResult,
			"CameraControl::Awake: Failed the load of camera post processes profile"
		);


		PostProcessingProfile PP_Profile = Camera.main.gameObject.GetOrAddIfNotFound<PostProcessingBehaviour>().profile = cameraPostProcesses.Asset;
		UserSettings.VideoSettings.OnEnable(PP_Profile);
		this.m_ApplyButton.interactable = false;
		this.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Apply changes </summary>
	public void OnApplyChanges()
	{
		if (this.m_IsInitialized == false)
		{
			return;
		}

		UserSettings.VideoSettings.OnApplyChanges();
		this.m_ApplyButton.interactable = false;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Updates UI Components </summary>
	public void UpdateUI()
	{
		if (this.m_IsInitialized == false)
		{
			return;
		}

		this.m_ResolutionDropDown.value = UserSettings.VideoSettings.GetScreenData().iResolutionIndex;
		this.m_FullScreenToogle.isOn = UserSettings.VideoSettings.GetScreenData().bIsFullScreen;
		this.m_AnisotropicFilterToogle.isOn = UserSettings.VideoSettings.GetFiltersData().bHasAnisotropicFilter;
		this.m_AntialiasingDropDown.value = UserSettings.VideoSettings.GetFiltersData().iAntialiasing;
		this.m_QualityLevelDropDown.value = UserSettings.VideoSettings.GetQualityData().iQualityLevel;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Remove key from registry </summary>
	public void Reset()
	{
		if (this.m_IsInitialized == false)
		{
			return;
		}

		UserSettings.VideoSettings.Reset();
	}

}
