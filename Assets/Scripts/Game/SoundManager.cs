
using UnityEngine;


public		delegate	void	OnValueChange( float value );

public interface ISoundManager {

	float				MusicVolume				{ get; set; }
	float				SoundVolume				{ get; set; }
	float				Pitch					{ get; set; }
}


public class SoundManager : MonoBehaviour, ISoundManager {

	public	static				ISoundManager			Instance			= null;

	public	static	event		OnValueChange			OnMusicVolumeChange	= null;
	public	static	event		OnValueChange			OnSoundVolumeChange	= null;
	public	static	event		OnValueChange			OnPitchChange		= null;

	// EDITOR ONLY
	[SerializeField, ReadOnly]
	private				float					m_MusicVolume	= 1f;

	[SerializeField, ReadOnly]
	private				float					m_SoundVolume	= 1f;

	[SerializeField, ReadOnly]
	private				float					m_Pitch			= 1f;

	public				float					MusicVolume
	{
		get { return m_MusicVolume; }
		set
		{
			m_MusicVolume = value;
			UpdateMusicVolume( value );
		}
	}

	public				float					SoundVolume
	{
		get { return m_SoundVolume; }
		set
		{
			m_SoundVolume = value;
			UpdateSoundVolume( value );
		}
	}

	public				float					Pitch
	{
		get { return m_Pitch; }
		set
		{
			m_Pitch = value;
			UpdatePitch( value );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		// SINGLETON
		if ( Instance != null )
		{
//			print( "SoundEffectManager: Object destroyied" );
			OnMusicVolumeChange	= null;
			OnSoundVolumeChange	= null;    
			OnPitchChange		= null;
			Destroy( gameObject );
//			gameObject.SetActive( false );
			return;
		}
		Instance = this as ISoundManager;

		if ( PlayerPrefs.HasKey( "MusicVolume" ) )
		{
			m_MusicVolume = PlayerPrefs.GetFloat( "MusicVolume" );
		}

		if ( PlayerPrefs.HasKey( "SoundVolume" ) )
		{
			m_SoundVolume = PlayerPrefs.GetFloat( "SoundVolume" );
		}


		if ( GameManager.InEditor == true )
		{
			if ( UnityEditor.EditorApplication.isPlaying == true )
				DontDestroyOnLoad( this );
		}
		else
		{
			DontDestroyOnLoad( this );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateMusicVolume
	private	void	UpdateMusicVolume( float value )
	{
		if ( OnMusicVolumeChange == null )
			return;

		m_MusicVolume = value;
		OnMusicVolumeChange( value );
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateSoundVolume
	private	void	UpdateSoundVolume( float value )
	{
		if ( OnSoundVolumeChange == null )
			return;

		m_SoundVolume = value;
		OnSoundVolumeChange( value );
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdatePitch
	private	void	UpdatePitch( float value )
	{
		OnPitchChange( value );
	}


	//////////////////////////////////////////////////////////////////////////
	// SaveSettings
	public	void	SaveSettings()
	{
		PlayerPrefs.SetFloat( "MusicVolume", Instance.MusicVolume );
		PlayerPrefs.SetFloat( "SoundVolume", Instance.SoundVolume );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDestroy
	private void OnDestroy()
	{
		SaveSettings();
	}

}
