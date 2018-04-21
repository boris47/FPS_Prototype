
using UnityEngine;
using System.Collections;
using FMODUnity;

public interface ICustomAudioSource {

	Transform		Transform			{ get; }

	AudioSource		AudioSource			{ get; }
//	float			InternalVolume		{ get; set; }
	float			Volume				{ get; set; }
//	float			InternalPitch		{ get; set; }
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


	// INTERFACE START
			Transform		ICustomAudioSource.Transform			{ get { return transform; } }
			AudioSource		ICustomAudioSource.AudioSource			{ get { return m_AudioSource; } }
//			float			ICustomAudioSource.InternalVolume		{ get { return m_InternalVolume; }	set { m_InternalVolume = value; } }
			float			ICustomAudioSource.Volume				{ get { return m_Volume; }			set { m_Volume = value; this.UpdateInternal(); } }
//			float			ICustomAudioSource.InternalPitch		{ get { return m_InternalPitch; }	set { m_InternalPitch = value; } }
			float			ICustomAudioSource.Pitch				{ get { return m_Pitch; }			set { m_Pitch = value; this.UpdateInternal(); } }
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
		m_Volume = SoundEffectManager.Instance.Volume;

		if ( m_AudioSource != null )
		{
			SoundEffectManager.Instance.RegisterSource( ref m_AudioSource );
			m_AudioSource.volume = m_InternalVolume * m_Volume;
			return;
		}

		if ( m_AudioEmitter != null )
		{
			SoundEffectManager.Instance.RegisterSource( ref m_AudioEmitter );
			m_AudioEmitter.EventInstance.setVolume( m_InternalVolume * m_Volume );
		}
	}
	

	//////////////////////////////////////////////////////////////////////////
	// SetParamenter
	public	void	SetParamenter( float value )
	{
		if ( m_AudioEmitter != null )
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

		if ( m_AudioSource != null )
		{
			m_AudioSource.volume = m_InternalVolume * m_Volume;
			m_AudioSource.pitch	 = m_InternalPitch  * m_Pitch;
			return;
		}

		if ( m_AudioEmitter != null )
		{
			m_AudioEmitter.EventInstance.setVolume( m_InternalVolume * m_Volume );
			m_AudioEmitter.EventInstance.setPitch( m_InternalPitch  * m_Pitch );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Play
	void	ICustomAudioSource.Play()
	{
		if ( m_AudioSource != null )
		{
			m_AudioSource.Play();
			return;
		}

		if ( m_AudioEmitter != null )
		{
			m_AudioEmitter.Play();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Play
	void	ICustomAudioSource.Stop()
	{
		if ( m_AudioSource != null )
		{
			m_AudioSource.Stop();
			return;
		}

		if ( m_AudioEmitter != null )
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

			if ( m_AudioSource != null )
			{
				m_AudioSource.volume = m_InternalVolume * Mathf.Lerp( startMul, endMul, interpolant );
			}
			if ( m_AudioEmitter != null )
			{
				m_AudioEmitter.EventInstance.setVolume( m_InternalVolume * Mathf.Lerp( startMul, endMul, interpolant ) );
			}
			yield return null;
		}

		if ( m_AudioSource != null )
		{
			m_AudioSource.volume = m_Volume = m_InternalVolume * endMul;
		}
		if ( m_AudioEmitter != null )
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

		if ( m_AudioSource != null )
		{
			bool result =  SoundEffectManager.Instance.UnRegisterSource( ref m_AudioSource );
			if ( result == false )
				print( name );
			return;
		}

		if ( m_AudioEmitter != null )
		{
			bool result =  SoundEffectManager.Instance.UnRegisterSource( ref m_AudioEmitter );
			if ( result == false )
				print( name );
		}
		
	}
}