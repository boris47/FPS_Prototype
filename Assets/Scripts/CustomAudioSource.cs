
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
	public		Transform			Transform				{ get { return this.transform; } }
	public		AudioSource			AudioSource				{ get { return this.m_AudioSource; } }
	public		StudioEventEmitter	Emitter					{ get { return this.m_AudioEmitter; } }
	public		float				Volume					{ get { return this.m_Volume; }			 set { this.m_Volume = value; this.UpdateInternal(); } }
	public		float				Pitch					{ get { return this.m_Pitch; }			 set { this.m_Pitch = value;  this.UpdateInternal(); } }
	public		AudioClip			Clip					{ get { return this.m_AudioSource.clip; } set { this.m_AudioSource.clip = value; } }
	public		bool				IsFading				{ get { return this.m_IsFading; } }
	public		bool				IsPlaying
	{
		get { return this.m_IsUnityAudioSource ? this.m_AudioSource.isPlaying : this.m_AudioEmitter.IsPlaying(); }
	}
	// INTERFACE END

	
	//////////////////////////////////////////////////////////////////////////
	// Awake
	protected virtual	void Awake()
	{
		if (this.m_AudioSource == null && this.m_AudioEmitter == null )
		{
			print(this.gameObject.name + ": custom audio source with no reference assigned !!" );
			Destroy( this );
		}

	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	protected virtual void OnEnable()
	{
		this.m_IsUnityAudioSource = this.m_AudioSource != null;
		if (this.m_IsUnityAudioSource == true)
		{
			SoundManager.OnSoundVolumeChange += this.OnSoundVolumeChange;
			this.OnSoundVolumeChange(SoundManager.SoundVolume);
		}
		else
		{
			SoundManager.OnMusicVolumeChange += this.OnMusicVolumeChange;
			this.OnMusicVolumeChange(SoundManager.MusicVolume);
		}
		SoundManager.OnPauseSet += this.OnPauseStateSet;
		SoundManager.OnPitchChange += this.OnPitchChange;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnPauseStateSet(bool value)
	{
		if (this.m_IsUnityAudioSource)
		{
			if (value) this.m_AudioSource?.Pause(); else this.m_AudioSource?.UnPause();
		}
		else
		{
			this.m_AudioEmitter?.EventInstance.setPaused(value);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	OnSoundVolumeChange( float value )
	{
		if (this.m_AudioSource)
		{
			this.m_Volume = value;
			float currentVolume = this.m_InternalVolume * this.m_Volume;
			this.m_AudioSource.volume = currentVolume;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	OnMusicVolumeChange( float value )
	{
		if (this.m_AudioEmitter && this.m_AudioEmitter.EventInstance.isValid())
		{
			this.m_Volume = value;
			float currentVolume = this.m_InternalVolume * this.m_Volume;
			this.m_AudioEmitter.EventInstance.setVolume( currentVolume );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	OnPitchChange( float value )
	{
		this.m_Pitch = value;
		float currentPitch = this.m_InternalPitch * this.m_Pitch;
		if (this.m_IsUnityAudioSource == true )
		{
			if (this.m_AudioSource)
			{
				this.m_AudioSource.pitch = currentPitch;
			}
		}
		else
		{
			if (this.m_AudioEmitter && this.m_AudioEmitter.EventInstance.isValid())
			{
				this.m_AudioEmitter.EventInstance.setPitch( currentPitch );
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	OnPauseSet( bool isPaused )
	{
		if ( isPaused == true )
		{
			this.Pause();
		}
		else
		{
			this.Resume();
		}
	}



	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	SetParamenter( string name, float value )
	{
		if (this.m_IsUnityAudioSource == false )
		{
			this.m_AudioEmitter.SetParameter( name, value );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual	void	UpdateInternal()
	{
		if (this.m_IsFading == true )
			return;

		float volume = this.m_InternalVolume * this.m_Volume;
		float pitch  = this.m_InternalPitch  * this.m_Pitch;

		if (this.m_IsUnityAudioSource == true )
		{
			this.m_AudioSource.volume = volume;
			this.m_AudioSource.pitch	 = pitch;
		}
		else
		{
			this.m_AudioEmitter.EventInstance.setVolume( volume );
			this.m_AudioEmitter.EventInstance.setPitch( pitch );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual	void	Play()
	{
		if (this.m_IsUnityAudioSource == true )
		{
			this.m_AudioSource.Play();
		}
		else
		{
			this.m_AudioEmitter.Play();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual	void	Stop()
	{
		if (this.m_IsUnityAudioSource == true )
		{
			this.m_AudioSource.Stop();
		}
		else
		{
			this.m_AudioEmitter.Stop();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual	void	Pause()
	{
		if (this.m_IsUnityAudioSource == true )
		{
			this.m_AudioSource.Pause();
		}
		else
		{
			this.m_AudioEmitter.EventInstance.setPaused( true );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual	void	Resume()
	{
		if (this.m_IsUnityAudioSource == true )
		{
			this.m_AudioSource.UnPause();
		}
		else
		{
			this.m_AudioEmitter.EventInstance.setPaused( false );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual	void	FadeIn( float time )
	{
		this.m_IsFading = true;
		CoroutinesManager.Start(this.FadeCO( time, fadeIn : true ), "CustomAudioSource::FadeIn: Fade in of " + this.name );
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual	void	FadeOut( float time )
	{
		this.m_IsFading = true;
		CoroutinesManager.Start(this.FadeCO( time, fadeIn : false ), "CustomAudioSource::FadeOut: Fade out of " + this.name );
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

			float volume = this.m_InternalVolume * Mathf.Lerp( startMul, endMul, interpolant );
			if (this.m_IsUnityAudioSource == true )
			{
				this.m_AudioSource.volume = volume;
			}
			else
			{
				this.m_AudioEmitter.EventInstance.setVolume( volume );
			}
			yield return null;
		}

		this.m_Volume = this.m_InternalVolume * endMul;
		if (this.m_IsUnityAudioSource == true )
		{
			this.m_AudioSource.volume = this.m_Volume;
		}
		else
		{
			this.m_AudioEmitter.EventInstance.setVolume(this.m_Volume);
		}

		this.m_IsFading = false;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual	void OnDisable()
	{
		if (this.m_IsUnityAudioSource == true )
		{
			SoundManager.OnSoundVolumeChange -= this.OnSoundVolumeChange;
			if (this.m_AudioSource )
				this.m_AudioSource.Stop();
		}
		else
		{
			SoundManager.OnMusicVolumeChange -= this.OnMusicVolumeChange;
			if (this.m_AudioEmitter )
				this.m_AudioEmitter.Stop();
		}
		SoundManager.OnPauseSet -= this.OnPauseStateSet;
		SoundManager.OnPitchChange -= this.OnPitchChange;
	}
}



public class DynamicCustomAudioSource : CustomAudioSource
{

	protected override void Awake()
	{
		
	}

	public	bool	Setup( AudioSource source )
	{
		bool bIsValid = source != null;
		if ( bIsValid )
		{
			this.m_AudioSource = source;
			this.m_IsUnityAudioSource = true;
			this.OnSoundVolumeChange( SoundManager.SoundVolume );
		}
		return bIsValid;
	}

	public	bool	Setup( StudioEventEmitter emitter )
	{
		bool bIsValid = emitter != null;
		if ( bIsValid )
		{
			this.m_AudioEmitter = emitter;
			this.m_IsUnityAudioSource = false;
			this.OnMusicVolumeChange( SoundManager.MusicVolume );
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