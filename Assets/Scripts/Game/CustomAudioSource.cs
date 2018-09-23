
using UnityEngine;
using System.Collections;
using FMODUnity;

public interface ICustomAudioSource {

	Transform		Transform			{ get; }

	AudioSource		AudioSource			{ get; }
	float			Volume				{ get; set; }
	float			Pitch				{ get; set; }
	AudioClip		Clip				{ get; set; }
	bool			IsFading			{ get; }
	bool			IsPlaying			{ get; }

	void			Play				();
	void			Stop				();
	void			Pause				();
	void			Resume				();
	void			FadeIn				( float time );
	void			FadeOut				( float time );

}

/// <summary> Provides an easy wrapper to looping audio sources with nice transitions for volume when starting and stopping </summary>
public class CustomAudioSource : MonoBehaviour, ICustomAudioSource {

	[SerializeField]
	private		AudioSource			m_AudioSource			= null;

	[SerializeField]
	private		StudioEventEmitter	m_AudioEmitter			= null;

	[SerializeField]
	private		float				m_InternalVolume		= 1f;

	[SerializeField]
	private		float				m_InternalPitch			= 1f;

	[SerializeField, ReadOnly]
	private		float				m_Volume				= 1f;

	[SerializeField, ReadOnly]
	private		float				m_Pitch					= 1f;

	[SerializeField, ReadOnly]
	private		bool				m_IsFading				= false;


	private		bool				m_IsUnityAudioSource	= true;

	// INTERFACE START
			Transform		ICustomAudioSource.Transform			{ get { return transform; } }
			AudioSource		ICustomAudioSource.AudioSource			{ get { return m_AudioSource; } }
			float			ICustomAudioSource.Volume				{ get { return m_Volume; }			 set { m_Volume = value; this.UpdateInternal(); } }
			float			ICustomAudioSource.Pitch				{ get { return m_Pitch; }			 set { m_Pitch = value;  this.UpdateInternal(); } }
			AudioClip		ICustomAudioSource.Clip					{ get { return m_AudioSource.clip; } set { m_AudioSource.clip = value; } }
			bool			ICustomAudioSource.IsFading				{ get { return m_IsFading; } }
			bool			ICustomAudioSource.IsPlaying			{ get { return m_AudioSource.isPlaying; } }
	// INTERFACE END

	
	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		if ( m_AudioSource == null && m_AudioEmitter == null )
		{
			print( gameObject.name + ": custom audio source with no reference assigned !!" );
			Destroy( this );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
		m_IsUnityAudioSource = m_AudioSource != null;
		if ( m_IsUnityAudioSource == true )
		{
			SoundManager.OnSoundVolumeChange += OnSoundVolumeChange;
			OnSoundVolumeChange( SoundManager.Instance.SoundVolume );
		}
		else
		{
			SoundManager.OnMusicVolumeChange += OnMusicVolumeChange;
			OnMusicVolumeChange( SoundManager.Instance.MusicVolume );
		}

		SoundManager.OnPitchChange += OnPitchChange;
		GameManager.PauseEvents.OnPauseSet += OnPauseSet;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnVolumeChange
	private	void	OnSoundVolumeChange( float value )
	{
		m_Volume = value;
		float currentVolume = m_InternalVolume * m_Volume;
		m_AudioSource.volume = currentVolume;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnVolumeChange
	private	void	OnMusicVolumeChange( float value )
	{
		m_Volume = value;
		float currentVolume = m_InternalVolume * m_Volume;
		m_AudioEmitter.EventInstance.setVolume( currentVolume );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnPitchChange
	private	void	OnPitchChange( float value )
	{
		m_Pitch = value;
		float currentPitch = m_InternalPitch * m_Pitch;
		if ( m_IsUnityAudioSource == true )
		{
			m_AudioSource.pitch = currentPitch;
		}
		else
		{
			m_AudioEmitter.EventInstance.setPitch( currentPitch );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// OnPitchChange
	private	void	OnPauseSet( bool isPaused )
	{
		if ( isPaused == true )
		{
			( this as ICustomAudioSource ).Pause();
		}
		else
		{
			( this as ICustomAudioSource ).Resume();
		}
	}



	//////////////////////////////////////////////////////////////////////////
	// SetParamenter
	public	void	SetParamenter( float value )
	{
		if ( m_IsUnityAudioSource == false )
		{
			m_AudioEmitter.SetParameter( "Phase", value );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FadeIn
	private void	UpdateInternal()
	{
		if ( m_IsFading == true )
			return;

		float volume = m_InternalVolume * m_Volume;
		float pitch = m_InternalPitch  * m_Pitch;

		if ( m_IsUnityAudioSource == true )
		{
			m_AudioSource.volume = volume;
			m_AudioSource.pitch	 = pitch;
		}
		else
		{
			m_AudioEmitter.EventInstance.setVolume( volume );
			m_AudioEmitter.EventInstance.setPitch( pitch );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Play
	void	ICustomAudioSource.Play()
	{
		if ( m_IsUnityAudioSource == true )
		{
			m_AudioSource.Play();
		}
		else
		{
			m_AudioEmitter.Play();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Play
	void	ICustomAudioSource.Stop()
	{
		if ( m_IsUnityAudioSource == true )
		{
			m_AudioSource.Stop();
		}
		else
		{
			m_AudioEmitter.Stop();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FadeIn
	void	ICustomAudioSource.Pause()
	{
		if ( m_IsUnityAudioSource == true )
		{
			m_AudioSource.Pause();
		}
		else
		{
			m_AudioEmitter.EventInstance.setPaused( true );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FadeIn
	void	ICustomAudioSource.Resume()
	{
		if ( m_IsUnityAudioSource == true )
		{
			m_AudioSource.UnPause();
		}
		else
		{
			m_AudioEmitter.EventInstance.setPaused( false );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FadeIn
	void	ICustomAudioSource.FadeIn( float time )
	{
		m_IsFading = true;
		StartCoroutine( FadeCO( time, fadeIn : true ) );
	}


	//////////////////////////////////////////////////////////////////////////
	// FadeOut
	void	ICustomAudioSource.FadeOut( float time )
	{
		m_IsFading = true;
		StartCoroutine( FadeCO( time, fadeIn : false ) );
	}


	//////////////////////////////////////////////////////////////////////////
	// FadeCO ( Coroutine )
	private	IEnumerator	FadeCO( float time, bool fadeIn )
	{
		float startMul = ( fadeIn == true ) ? 0f : 1f;
		float endMul	 = ( fadeIn == true ) ? 1f : 0f;

		float interpolant = 0f;
		float currentTime = 0;

		while( interpolant < 1f )
		{
			currentTime += Time.unscaledDeltaTime;
			interpolant = currentTime / time;

			float volume = m_InternalVolume * Mathf.Lerp( startMul, endMul, interpolant );

			if ( m_IsUnityAudioSource == true )
			{
				m_AudioSource.volume = volume;
			}
			else
			{
				m_AudioEmitter.EventInstance.setVolume( volume );
			}
			yield return null;
		}

		if ( m_IsUnityAudioSource == true )
		{
			m_AudioSource.volume = m_Volume = m_InternalVolume * endMul;
		}
		else
		{
			m_AudioEmitter.EventInstance.setVolume( m_Volume = m_InternalVolume * endMul );
		}

		m_IsFading = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDestroy
	private void OnDisable()
	{
		if ( m_IsUnityAudioSource == true )
		{
			SoundManager.OnSoundVolumeChange -= OnSoundVolumeChange;
			m_AudioSource.Stop();
		}
		else
		{
			SoundManager.OnMusicVolumeChange -= OnMusicVolumeChange;
			m_AudioEmitter.Stop();
		}
		SoundManager.OnPitchChange -= OnPitchChange;
	}
}