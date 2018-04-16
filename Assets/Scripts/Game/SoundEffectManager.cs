
using UnityEngine;
using System.Collections.Generic;
using FMODUnity;

public interface ISoundEffectManager {

	float		Volume				{ get; set; }
	float		Pitch				{ get; set; }

	void		RegisterSource		( ref AudioSource audioSource );
	void		RegisterSource		( ref StudioEventEmitter emitter );
	bool		UnRegisterSource	( ref AudioSource audioSource );
	bool		UnRegisterSource	( ref StudioEventEmitter emitter );


	void		UpdateVolume		( float value );
	void		UpdatePitch			( float value );
}

[ExecuteInEditMode]
public class SoundEffectManager : MonoBehaviour, ISoundEffectManager {

	public static		ISoundEffectManager		Instance		= null;

	[SerializeField]
	private				float					m_MainVolume	= 1f;

	[SerializeField]
	private				float					m_MainPitch		= 1f;

	public				float					Volume
	{
		get { return m_MainVolume; }
		set
		{
			m_MainVolume = value;
			UpdateVolume( value );
		}
	}

	public				float					Pitch
	{
		get { return m_MainPitch; }
		set
		{
			m_MainPitch = value;
			UpdatePitch( value );
		}
	}

	private		List<AudioSource>				m_Sources		= new List<AudioSource>();
	private		List<StudioEventEmitter>		m_Emitters		= new List<StudioEventEmitter>();
//	private		FMOD.Studio.Bus					m_FMODBus;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		if ( Instance != null )
		{
			print( "SoundEffectManager: Object set inactive" );
			gameObject.SetActive( false );
			return;
		}

		/*
		if ( FMODUnity.RuntimeManager.IsInitialized )
		{
			m_FMODBus = FMODUnity.RuntimeManager.GetBus( "Bus:/" );
			m_FMODBus.setVolume( m_MainVolume );
		}
		*/
#if UNITY_EDITOR
			if ( UnityEditor.EditorApplication.isPlaying == true )
				DontDestroyOnLoad( this );
#else
			DontDestroyOnLoad( this );
#endif

		Instance = this as ISoundEffectManager;
	}


	//////////////////////////////////////////////////////////////////////////
	// RegisterSource ( AudioSource )
	public	void	RegisterSource( ref AudioSource audioSource )
	{
		if ( audioSource == null )
			return;

		if ( m_Sources.Contains( audioSource ) == true )
			return;

		m_Sources.Add( audioSource );
	}


	//////////////////////////////////////////////////////////////////////////
	// RegisterSource ( EventInstance )
	public	void		RegisterSource( ref StudioEventEmitter emitter )
	{
		m_Emitters.Add( emitter );
		if ( m_Emitters.Contains( emitter ) == true )
			return;

		m_Emitters.Add( emitter );
	}


	//////////////////////////////////////////////////////////////////////////
	// UnRegisterSource ( AudioSource )
	public	bool	UnRegisterSource( ref AudioSource audioSource )
	{
		if ( audioSource == null )
		{
			Debug.LogError( "SoundEffectManager::UnRegisterSource: Trying to unregister a null ref of AudioSource !!" );
			return false;
		}

		if ( m_Sources.Contains( audioSource ) == false )
		{
			Debug.LogError( "SoundEffectManager::UnRegisterSource: Trying to unregister a non registered AudioSource !!" );
			return false;
		}

		m_Sources.Remove( audioSource );
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// UnRegisterSource ( EventInstance )
	public	bool		UnRegisterSource( ref StudioEventEmitter emitter )
	{
		if ( m_Emitters.Contains( emitter ) == false )
		{
			Debug.LogError( "SoundEffectManager::UnRegisterSource: Trying to unregister a non registered AudioSource !!" );
			return false;
		}

		m_Emitters.Remove( emitter );
		return true;
	}
	

	//////////////////////////////////////////////////////////////////////////
	// UpdateVolume
	public	void	UpdateVolume( float value )
	{
		for ( int i = m_Sources.Count - 1; i > 0; i-- )
		{
			AudioSource source = m_Sources [ i ];
			if ( source == null )
			{
				m_Sources.RemoveAt( i );
				continue;
			}

			source.volume = value;
		}

		for ( int i = m_Emitters.Count - 1; i > 0; i-- )
		{
			StudioEventEmitter emitter = m_Emitters[ i ];
			if ( emitter == null )
			{
				m_Emitters.RemoveAt( i );
				continue;
			}

			emitter.EventInstance.setVolume( value );
		}

//		if ( FMODUnity.RuntimeManager.IsInitialized )
//			m_FMODBus.setVolume( value );
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdatePitch
	public	void	UpdatePitch( float value )
	{
		for ( int i = m_Sources.Count - 1; i > 0; i-- )
		{
			AudioSource source = m_Sources [ i ];
			if ( source == null )
			{
				m_Sources.RemoveAt( i );
				continue;
			}
			source.pitch = value;
		}

		for ( int i = m_Emitters.Count - 1; i > 0; i-- )
		{
			StudioEventEmitter emitter = m_Emitters[ i ];
			if ( emitter == null )
			{
				m_Emitters.RemoveAt( i );
				continue;
			}

			emitter.EventInstance.setPitch( value );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDestroy
	private void OnDestroy()
	{
		Instance = null;
	}

}
