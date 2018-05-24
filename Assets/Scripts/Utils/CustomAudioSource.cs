
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
			Destroy( this );
			return;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Start
	private void Start()
	{
		if ( m_AudioSource == null && m_AudioEmitter == null )
		{
			print( gameObject.name + ": custom audio source with no reference assigned !!" );
			return;
		}

		m_Volume = SoundEffectManager.Instance.Volume;
		m_IsUnityAudioSource = m_AudioSource != null;

		if ( m_IsUnityAudioSource == true )
		{
			m_AudioSource.volume = m_InternalVolume * m_Volume;
		}
		else
		{
			m_AudioEmitter.EventInstance.setVolume( m_InternalVolume * m_Volume );
		}

		SoundEffectManager.Instance.OnVolumeChange += OnVolumeChange;
		SoundEffectManager.Instance.OnPitchChange  += OnPitchChange;
	}
	

	//////////////////////////////////////////////////////////////////////////
	// OnVolumeChange
	private	void	OnVolumeChange( float value )
	{
		m_Volume = value;
		float currentVolume = m_InternalVolume * m_Volume;
		if ( m_IsUnityAudioSource == true )
		{
			m_AudioSource.volume = currentVolume;
		}
		else
		{
			m_AudioEmitter.EventInstance.setVolume( currentVolume );
		}
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

		if ( m_IsUnityAudioSource == true )
		{
			m_AudioSource.volume = m_InternalVolume * m_Volume;
			m_AudioSource.pitch	 = m_InternalPitch  * m_Pitch;
		}
		else
		{
			m_AudioEmitter.EventInstance.setVolume( m_InternalVolume * m_Volume );
			m_AudioEmitter.EventInstance.setPitch( m_InternalPitch  * m_Pitch );
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

			if ( m_IsUnityAudioSource == true )
			{
				m_AudioSource.volume = m_InternalVolume * Mathf.Lerp( startMul, endMul, interpolant );
			}
			else
			{
				m_AudioEmitter.EventInstance.setVolume( m_InternalVolume * Mathf.Lerp( startMul, endMul, interpolant ) );
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
	private void OnDestroy()
	{
		if ( SoundEffectManager.Instance == null || enabled == false )
			return;

		SoundEffectManager.Instance.OnVolumeChange -= OnVolumeChange;
		SoundEffectManager.Instance.OnPitchChange  -= OnPitchChange;
	}
}