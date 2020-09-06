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
	bool IStateDefiner.IsInitialized => this.m_IsInitialized = false;


	//------------------------------------------------------------
	string IStateDefiner.StateName => this.name;


	//////////////////////////////////////////////////////////////////////////
	IEnumerator IStateDefiner.Initialize()
	{
		if (this.m_IsInitialized == true)
		{
			yield break;
		}

		this.OnEnable();
		this.OnApplyChanges();

		CoroutinesManager.AddCoroutineToPendingCount(1);

		this.m_IsInitialized = true;
		{
			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("Slider_MusicVolume", ref this.m_MusicSlider))
			{
				this.m_MusicSlider.onValueChanged.AddListener((float newValue) =>
				{
					UserSettings.AudioSettings.OnMusicVolumeSet(newValue);
					this.m_ApplyButton.interactable = true;
				});
			}

			yield return null;

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("Slider_SoundVolume", ref this.m_SoundSlider))
			{
				this.m_SoundSlider.onValueChanged.AddListener((float newValue) =>
				{
					UserSettings.AudioSettings.OnSoundsVolumeSet(newValue);
					this.m_ApplyButton.interactable = true;
				});
			}

			yield return null;

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("ApplyButton", ref this.m_ApplyButton))
			{
				this.m_ApplyButton.onClick.AddListener(() =>
				{
					UIManager.Confirmation.Show("Apply Changes?", this.OnApplyChanges, () => { UserSettings.AudioSettings.ReadFromRegistry(); this.UpdateUI(); });
				});
				this.m_ApplyButton.interactable = false;
			}

			yield return null;

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild("ResetButton", ref this.m_ResetButton))
			{
				this.m_ResetButton.onClick.AddListener(() =>
				{
					UIManager.Confirmation.Show("Reset?", () => { UserSettings.AudioSettings.ApplyDefaults(); this.UpdateUI(); });
				});
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

		UserSettings.AudioSettings.OnEnable();
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

		UserSettings.AudioSettings.OnApplyChanges();
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

		UserSettings.AudioSettings.AudioData data = UserSettings.AudioSettings.GetAudioData();
		this.m_MusicSlider.value = data.MusicVolume;
		this.m_SoundSlider.value = data.SoundVolume;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Remove key from registry </summary>
	public void Reset()
	{
		if (this.m_IsInitialized == false)
		{
			return;
		}

		UserSettings.AudioSettings.Reset();
		this.UpdateUI();
	}

}
