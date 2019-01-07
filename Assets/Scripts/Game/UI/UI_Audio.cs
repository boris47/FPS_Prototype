using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Audio : MonoBehaviour, IUIOptions {

	// Registry Keys
	private	const	string	FLAG_SAVED_AUDIO_SETTINGS	= "bSavedAudioSettings";
	private	const	string	VAR_MUSIC_VOLUME			= "iMusicVolume";
	private	const	string	VAR_SOUND_VOLUME			= "iSoundVolume";


	// CHANGES STRUCTURES
	// ---------------------------
	private struct VolumeData {
		public	float MusicVolume;
		public	float SoundVolume;
		public	bool isDirty;
	}
	private	VolumeData m_VolumeData;


	// UI Components
	private	Slider			m_MusicSlider				= null;
	private	Slider			m_SoundSlider				= null;
	private	Button			m_ApplyButton				= null;
	private	Button			m_ResetButton				= null;


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	public	 void Initialize()
	{
		transform.SearchComponentInChild( "Slider_MusicVolume", ref m_MusicSlider );
		transform.SearchComponentInChild( "Slider_SoundVolume", ref m_SoundSlider );

		transform.SearchComponentInChild( "ApplyButton", ref m_ApplyButton );
		{
			m_ApplyButton.onClick.AddListener
			(	
				delegate()
				{
					UI.Instance.Confirmation.Show( "Apply Changes?", OnApplyChanges, delegate { ReadFromRegistry(); UpdateUI(); } );
				}
			);
			m_ApplyButton.interactable = false;
		}

		transform.SearchComponentInChild( "ResetButton", ref m_ResetButton );
		{
			m_ResetButton.onClick.AddListener
			(
				delegate()
				{
					UI.Instance.Confirmation.Show( "Reset?", ApplyDefaults, delegate { ReadFromRegistry(); UpdateUI(); } );
				}	
			);
		}

		OnEnable();
		OnApplyChanges();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	public void OnEnable()
	{
		if ( PlayerPrefs.HasKey( FLAG_SAVED_AUDIO_SETTINGS ) == true )
		{
			ReadFromRegistry();

			UpdateUI();
		}
		else
		{
			ApplyDefaults();

			PlayerPrefs.SetString( FLAG_SAVED_AUDIO_SETTINGS, "1" );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnMusicVolumeSet
	public	void	OnMusicVolumeSet( float value )
	{
		SoundManager.Instance.MusicVolume = value;
		m_VolumeData.MusicVolume = value;
		m_VolumeData.isDirty = true;
		m_ApplyButton.interactable = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSoundsVolumeSet
	public	void	OnSoundsVolumeSet( float value )
	{
		SoundManager.Instance.SoundVolume = value;
		m_VolumeData.SoundVolume = value;
		m_VolumeData.isDirty = true;
		m_ApplyButton.interactable = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// ApplyDefaults
	public	void	ApplyDefaults()
	{
		// Remove keys from registry
		Reset();
		{
			m_VolumeData.MusicVolume = 1f;
			m_VolumeData.SoundVolume = 1f;
			m_VolumeData.isDirty = false;
		}
		// Save new keys into registry
		SaveToRegistry();

		//Update UI components
		UpdateUI();

		// Apply the default settings
		OnApplyChanges();

		// Reset buttons state
		m_ApplyButton.interactable = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// ReadFromRegistry
	/// <summary> Read value from Registry </summary>
	public	void	ReadFromRegistry()
	{
		m_VolumeData.MusicVolume = PlayerPrefs.GetFloat( VAR_MUSIC_VOLUME );
		m_VolumeData.SoundVolume = PlayerPrefs.GetFloat( VAR_SOUND_VOLUME );
		m_VolumeData.isDirty = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// ApplyDefaults
	/// <summary> Updates UI Components </summary>
	public	void	UpdateUI()
	{
		m_MusicSlider.value = m_VolumeData.MusicVolume;
		m_SoundSlider.value = m_VolumeData.SoundVolume;
	}


	//////////////////////////////////////////////////////////////////////////
	// SaveToRegistry
	/// <summary> Save settings </summary>
	public	void	SaveToRegistry()
	{
		// Save settings
		{
			PlayerPrefs.SetFloat( VAR_MUSIC_VOLUME, m_VolumeData.MusicVolume );
			PlayerPrefs.SetFloat( VAR_SOUND_VOLUME, m_VolumeData.SoundVolume );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnApplyChanges
	/// <summary> Apply changes </summary>
	public	void	OnApplyChanges()
	{
		if ( m_VolumeData.isDirty )
		{
			m_VolumeData.isDirty = false;
			SoundManager.Instance.MusicVolume = m_VolumeData.MusicVolume;
			SoundManager.Instance.SoundVolume = m_VolumeData.SoundVolume;
		}
		// Save settings
		SaveToRegistry();

		m_ApplyButton.interactable = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// Reset
	/// <summary> Remove key from registry </summary>
	public	void	Reset()
	{
		PlayerPrefs.DeleteKey( FLAG_SAVED_AUDIO_SETTINGS );
		{
			PlayerPrefs.DeleteKey( VAR_MUSIC_VOLUME );
			PlayerPrefs.DeleteKey( VAR_SOUND_VOLUME );
		}
	}

}
