using UnityEngine.UI;

public sealed class UI_Audio : UI_Base, IStateDefiner
{
	private				Slider				m_MusicSlider					= null;
	private				Slider				m_SoundSlider					= null;
	private				Button				m_ApplyButton					= null;
	private				Button				m_ResetButton					= null;
	private				Button				m_BackButton					= null;

	private				bool				m_IsInitialized					= false;
						bool				IStateDefiner.IsInitialized		=> m_IsInitialized;


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
			UserSettings.AudioSettings.LoadOrSetDefaults();

			// Music Volume Slider
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Slider_MusicVolume", out m_MusicSlider)))
			{
				m_MusicSlider.onValueChanged.AddListener((float newValue) =>
				{
					UserSettings.AudioSettings.OnMusicVolumeSet(newValue);
					m_ResetButton.interactable = true;
					m_ApplyButton.interactable = true;
				});
			}

			// Sound Volume Slider
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Slider_SoundVolume", out m_SoundSlider)))
			{
				m_SoundSlider.onValueChanged.AddListener((float newValue) =>
				{
					UserSettings.AudioSettings.OnSoundsVolumeSet(newValue);
					m_ResetButton.interactable = true;
					m_ApplyButton.interactable = true;
				});
			}

			// Apply button
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("ApplyButton", out m_ApplyButton)))
			{
				void OnConfirm()
				{
					UserSettings.AudioSettings.OnApplyChanges();

					m_ApplyButton.interactable = false;
				}
				m_ApplyButton.onClick.AddListener(() => UIManager.Confirmation.Show("Apply Changes?", OnConfirm));
			}

			// Reset Button
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("ResetButton", out m_ResetButton)))
			{
				void OnConfirm()
				{
					UserSettings.AudioSettings.Reset();

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
	private void UpdateUI()
	{
		UserSettings.AudioSettings.AudioData data = UserSettings.AudioSettings.GetAudioData();
		m_MusicSlider.value = data.MusicVolume;
		m_SoundSlider.value = data.SoundVolume;
	}
}
