
using UnityEngine;


public		delegate	void	OnValueChange( float value );

public interface ISoundManager {

	float				MusicVolume				{ get; set; }
	float				SoundVolume				{ get; set; }
	float				Pitch					{ get; set; }

	void				OnSceneLoaded			();
}


public class SoundManager : MonoBehaviour, ISoundManager {

	// STATIC
	private	static	ISoundManager						m_Instance				= null;
	public	static	ISoundManager						Instance
	{
		get { return m_Instance; }
	}

	private	static	event		OnValueChange			m_OnMusicVolumeChange	= delegate { };
	private	static	event		OnValueChange			m_OnSoundVolumeChange	= delegate { };
	private	static	event		OnValueChange			m_OnPitchChange			= delegate { };

	public	static	event		OnValueChange			OnMusicVolumeChange
	{
		add { if ( value != null )
			m_OnMusicVolumeChange += value;
		}

		remove { if ( value != null )
			m_OnMusicVolumeChange -= value;
		}
	}

	public	static	event		OnValueChange			OnSoundVolumeChange
	{
		add { if ( value != null )
			m_OnSoundVolumeChange += value;
		}

		remove { if ( value != null )
			m_OnSoundVolumeChange -= value;
		}
	}

	public	static	event		OnValueChange			OnPitchChange
	{
		add { if ( value != null )
			m_OnPitchChange += value;
		}

		remove { if ( value != null )
			m_OnPitchChange -= value;
		}
	}

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
			UpdateMusicVolume( value );
		}
	}

	public				float					SoundVolume
	{
		get { return m_SoundVolume; }
		set
		{
			UpdateSoundVolume( value );
		}
	}

	public				float					Pitch
	{
		get { return m_Pitch; }
		set
		{
			UpdatePitch( value );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		// Singleton
		if ( m_Instance != null )
		{
#if UNITY_EDITOR
			if ( UnityEditor.EditorApplication.isPlaying == true )
				DestroyImmediate( gameObject );
			else
				Destroy( gameObject );
#else
			Destroy( gameObject );
#endif
			return;
		}
		m_OnMusicVolumeChange	= delegate { };
		m_OnSoundVolumeChange	= delegate { };	
		m_OnPitchChange			= delegate { };
		m_Instance = this as ISoundManager;

#if UNITY_EDITOR
		if ( UnityEditor.EditorApplication.isPlaying == true )
				DontDestroyOnLoad( this );
#else
		DontDestroyOnLoad( this );
#endif
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateMusicVolume
	private	void	UpdateMusicVolume( float value )
	{
		m_MusicVolume = value;

		m_OnMusicVolumeChange( value );
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateSoundVolume
	private	void	UpdateSoundVolume( float value )
	{
		m_SoundVolume = value;

		m_OnSoundVolumeChange( value );
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdatePitch
	private	void	UpdatePitch( float value )
	{
		m_Pitch = value;

		m_OnPitchChange( value );
	}

	//

	public void OnSceneLoaded()
	{
		m_OnMusicVolumeChange( m_MusicVolume );
		m_OnSoundVolumeChange( m_SoundVolume );

		m_OnPitchChange( m_Pitch );
	}

}
