
using UnityEngine;


public		delegate	void	OnValueChange( float value );

public interface ISoundEffectManager {

	float				Volume				{ get; set; }
	float				Pitch				{ get; set; }

	OnValueChange		OnVolumeChange		{ get; set; }
	OnValueChange		OnPitchChange		{ get; set; }
}


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

	private	event		OnValueChange			m_OnVolumeChange	= null;
	private	event		OnValueChange			m_OnPitchChange		= null;

	OnValueChange	ISoundEffectManager.OnVolumeChange	{ get { return m_OnVolumeChange; } set { m_OnVolumeChange = value; } }
	OnValueChange	ISoundEffectManager.OnPitchChange	{ get { return m_OnPitchChange; }  set { m_OnPitchChange = value; } }


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
	// UpdateVolume
	private	void	UpdateVolume( float value )
	{
		m_OnVolumeChange( value );
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdatePitch
	private	void	UpdatePitch( float value )
	{
		m_OnPitchChange( value );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDestroy
	private void OnDestroy()
	{
		Instance = null;
	}

}
