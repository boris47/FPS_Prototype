using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_Audio : MonoBehaviour, IUIOptions, IStateDefiner
{
	// UI Components
	private	Slider			m_MusicSlider				= null;
	private	Slider			m_SoundSlider				= null;
	private	Button			m_ApplyButton				= null;
	private	Button			m_ResetButton				= null;

	private	bool			m_IsInitialized			= false;

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
		{
			yield break;
		}

		OnEnable();
		OnApplyChanges();

		CoroutinesManager.AddCoroutineToPendingCount(1);

		m_IsInitialized = true;
		{
			if (m_IsInitialized &= transform.SearchComponentInChild("Slider_MusicVolume", ref m_MusicSlider))
			{
				m_MusicSlider.onValueChanged.AddListener((float newValue) =>
				{
					UserSettings.AudioSettings.OnMusicVolumeSet(newValue);
					m_ApplyButton.interactable = true;
				});
			}

			yield return null;

			if (m_IsInitialized &= transform.SearchComponentInChild("Slider_SoundVolume", ref m_SoundSlider))
			{
				m_SoundSlider.onValueChanged.AddListener((float newValue) =>
				{
					UserSettings.AudioSettings.OnSoundsVolumeSet(newValue);
					m_ApplyButton.interactable = true;
				});
			}

			yield return null;

			if (m_IsInitialized &= transform.SearchComponentInChild("ApplyButton", ref m_ApplyButton))
			{
				m_ApplyButton.onClick.AddListener(() =>
				{
					UIManager.Confirmation.Show("Apply Changes?", OnApplyChanges, () => { UserSettings.AudioSettings.ReadFromRegistry(); UpdateUI(); });
				});
				m_ApplyButton.interactable = false;
			}

			yield return null;

			if (m_IsInitialized &= transform.SearchComponentInChild("ResetButton", ref m_ResetButton))
			{
				m_ResetButton.onClick.AddListener(() =>
				{
					UIManager.Confirmation.Show("Reset?", () => { UserSettings.AudioSettings.ApplyDefaults(); UpdateUI(); });
				});
			}

			yield return null;

			if (m_IsInitialized)
			{
				OnEnable();

				yield return null;

				OnApplyChanges();

				CoroutinesManager.RemoveCoroutineFromPendingCount(1);

				yield return null;
			}
			else
			{
				Debug.LogError("UI_Audio: Bad initialization!!!");
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

		UserSettings.AudioSettings.OnEnable();
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

		UserSettings.AudioSettings.OnApplyChanges();
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

		UserSettings.AudioSettings.AudioData data = UserSettings.AudioSettings.GetAudioData();
		m_MusicSlider.value = data.MusicVolume;
		m_SoundSlider.value = data.SoundVolume;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Remove key from registry </summary>
	public void Reset()
	{
		if (m_IsInitialized == false)
		{
			return;
		}

		UserSettings.AudioSettings.Reset();
		UpdateUI();
	}

}
