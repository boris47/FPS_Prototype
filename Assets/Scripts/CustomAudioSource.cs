
using UnityEngine;
using System.Collections;
using FMODUnity;

public interface ICustomAudioSource {

	Transform				Transform				{ get; }

	AudioSource				AudioSource				{ get; }
	StudioEventEmitter		Emitter					{ get; }
	float					Volume					{ get; set; }
	float					Pitch					{ get; set; }
	AudioClip				Clip					{ get; set; }
	bool					IsFading				{ get; }
	bool					IsPlaying				{ get; }

	void					Play					();
	void					Stop					();
	void					Pause					();
	void					Resume					();
	void					FadeIn					( float time );
	void					FadeOut					( float time );

}

/// <summary> Provides an easy wrapper to looping audio sources with nice transitions for volume when starting and stopping </summary>
public class CustomAudioSource : MonoBehaviour, ICustomAudioSource {

	[SerializeField]
	protected	AudioSource			m_AudioSource			= null;

	[SerializeField]
	protected	StudioEventEmitter	m_AudioEmitter			= null;

	[SerializeField]
	protected	float				m_InternalVolume		= 1f;

	[SerializeField]
	protected	float				m_InternalPitch			= 1f;

	[SerializeField, ReadOnly]
	protected	float				m_Volume				= 1f;

	[SerializeField, ReadOnly]
	protected	float				m_Pitch					= 1f;

	[SerializeField, ReadOnly]
	protected	bool				m_IsFading				= false;


	protected	bool				m_IsUnityAudioSource	= true;

	// INTERFACE START
	public		Transform			Transform				{ get { return transform; } }
	public		AudioSource			AudioSource				{ get { return m_AudioSource; } }
	public		StudioEventEmitter	Emitter					{ get { return m_AudioEmitter; } }
	public		float				Volume					{ get { return m_Volume; }			 set { m_Volume = value; /*UpdateInternal();*/ } }
	public		float				Pitch					{ get { return m_Pitch; }			 set { m_Pitch = value;  /*UpdateInternal();*/ } }
	public		AudioClip			Clip					{ get { return m_AudioSource?.clip; } set { if (m_AudioSource) m_AudioSource.clip = value; } }
	public		bool				IsFading				{ get { return m_IsFading; } }
	public		bool				IsPlaying
	{
		get { return m_IsUnityAudioSource ? m_AudioSource.isPlaying : m_AudioEmitter.IsPlaying(); }
	}
	// INTERFACE END

	
	//////////////////////////////////////////////////////////////////////////
	// Awake
/*	protected virtual	void Awake()
	{
		if (m_AudioSource == null && m_AudioEmitter == null )
		{
			print(gameObject.name + ": custom audio source with no reference assigned !!" );
			Destroy( this );
		}
	}
*/

	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	protected virtual void OnEnable()
	{
		TryGetComponent(out m_AudioSource);
		TryGetComponent(out m_AudioEmitter);

		m_IsUnityAudioSource = m_AudioSource != null;
		if (m_IsUnityAudioSource == true)
		{
			SoundManager.OnSoundVolumeChange += OnSoundVolumeChange;
			OnSoundVolumeChange(SoundManager.SoundVolume);
		}
		else
		{
			SoundManager.OnMusicVolumeChange += OnMusicVolumeChange;
			OnMusicVolumeChange(SoundManager.MusicVolume);
		}
		SoundManager.OnPauseSet += OnPauseStateSet;
		SoundManager.OnPitchChange += OnPitchChange;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnPauseStateSet(bool value)
	{
		if (m_IsUnityAudioSource)
		{
			if (value) m_AudioSource?.Pause(); else m_AudioSource?.UnPause();
		}
		else
		{
			m_AudioEmitter?.EventInstance.setPaused(value);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	OnSoundVolumeChange( float value )
	{
		if (m_AudioSource)
		{
			m_Volume = value;
			float currentVolume = m_InternalVolume * m_Volume;
			m_AudioSource.volume = currentVolume;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	OnMusicVolumeChange( float value )
	{
		if (m_AudioEmitter && m_AudioEmitter.EventInstance.isValid())
		{
			m_Volume = value;
			float currentVolume = m_InternalVolume * m_Volume;
			m_AudioEmitter.EventInstance.setVolume( currentVolume );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	OnPitchChange( float value )
	{
		m_Pitch = value;
		float currentPitch = m_InternalPitch * m_Pitch;
		if (m_IsUnityAudioSource == true )
		{
			if (m_AudioSource)
			{
				m_AudioSource.pitch = currentPitch;
			}
		}
		else
		{
			if (m_AudioEmitter && m_AudioEmitter.EventInstance.isValid())
			{
				m_AudioEmitter.EventInstance.setPitch( currentPitch );
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	OnPauseSet( bool isPaused )
	{
		if ( isPaused == true )
		{
			Pause();
		}
		else
		{
			Resume();
		}
	}



	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	SetParamenter( string name, float value )
	{
		if (m_IsUnityAudioSource == false )
		{
			m_AudioEmitter.SetParameter( name, value );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual	void	UpdateInternal()
	{
		if (m_IsFading == true )
			return;

		float volume = m_InternalVolume * m_Volume;
		float pitch  = m_InternalPitch  * m_Pitch;

		if (m_IsUnityAudioSource == true )
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
	public	virtual	void	Play()
	{
		if (m_IsUnityAudioSource == true )
		{
			m_AudioSource.Play();
		}
		else
		{
			m_AudioEmitter.Play();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual	void	Stop()
	{
		if (m_IsUnityAudioSource == true )
		{
			m_AudioSource.Stop();
		}
		else
		{
			m_AudioEmitter.Stop();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual	void	Pause()
	{
		if (m_IsUnityAudioSource == true )
		{
			m_AudioSource.Pause();
		}
		else
		{
			m_AudioEmitter.EventInstance.setPaused( true );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual	void	Resume()
	{
		if (m_IsUnityAudioSource == true )
		{
			m_AudioSource.UnPause();
		}
		else
		{
			m_AudioEmitter.EventInstance.setPaused( false );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual	void	FadeIn( float time )
	{
		m_IsFading = true;
		CoroutinesManager.Start(FadeCO( time, fadeIn : true ), "CustomAudioSource::FadeIn: Fade in of " + name );
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual	void	FadeOut( float time )
	{
		m_IsFading = true;
		CoroutinesManager.Start(FadeCO( time, fadeIn : false ), "CustomAudioSource::FadeOut: Fade out of " + name );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	IEnumerator	FadeCO( float time, bool fadeIn )
	{
		float startMul	= ( fadeIn == true ) ? 0f : 1f;
		float endMul	= ( fadeIn == true ) ? 1f : 0f;

		float interpolant = 0f, currentTime = 0f;
		while( interpolant < 1f )
		{
			currentTime += Time.unscaledDeltaTime;
			interpolant = currentTime / time;

			float volume = m_InternalVolume * Mathf.Lerp( startMul, endMul, interpolant );
			if (m_IsUnityAudioSource == true )
			{
				m_AudioSource.volume = volume;
			}
			else
			{
				m_AudioEmitter.EventInstance.setVolume( volume );
			}
			yield return null;
		}

		m_Volume = m_InternalVolume * endMul;
		if (m_IsUnityAudioSource == true )
		{
			m_AudioSource.volume = m_Volume;
		}
		else
		{
			m_AudioEmitter.EventInstance.setVolume(m_Volume);
		}

		m_IsFading = false;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual	void OnDisable()
	{
		if (m_IsUnityAudioSource == true )
		{
			SoundManager.OnSoundVolumeChange -= OnSoundVolumeChange;
			if (m_AudioSource )
				m_AudioSource.Stop();
		}
		else
		{
			SoundManager.OnMusicVolumeChange -= OnMusicVolumeChange;
			if (m_AudioEmitter )
				m_AudioEmitter.Stop();
		}
		SoundManager.OnPauseSet -= OnPauseStateSet;
		SoundManager.OnPitchChange -= OnPitchChange;
	}
}



public class DynamicCustomAudioSource : CustomAudioSource
{

/*	protected override void Awake()
	{
		
	}
*/
	public	bool	Setup( AudioSource source )
	{
		bool bIsValid = source != null;
		if ( bIsValid )
		{
			m_AudioSource = source;
			m_IsUnityAudioSource = true;
			OnSoundVolumeChange( SoundManager.SoundVolume );
		}
		return bIsValid;
	}

	public	bool	Setup( StudioEventEmitter emitter )
	{
		bool bIsValid = emitter != null;
		if ( bIsValid )
		{
			m_AudioEmitter = emitter;
			m_IsUnityAudioSource = false;
			OnMusicVolumeChange( SoundManager.MusicVolume );
		}
		return bIsValid;
	}
	/*
	// This override exists because
	protected override void OnEnable()
	{
		if (this.m_IsUnityAudioSource == true )
		{
			SoundManager.OnSoundVolumeChange += this.OnSoundVolumeChange;
		}
		else
		{
			SoundManager.OnMusicVolumeChange += this.OnMusicVolumeChange;
		}

		SoundManager.OnPauseSet += this.OnPauseStateSet;
		SoundManager.OnPitchChange += this.OnPitchChange;
	}

	protected override void OnDisable()
	{
		
	}
	*/
}