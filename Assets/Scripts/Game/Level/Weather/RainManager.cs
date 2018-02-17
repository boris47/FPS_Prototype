
using UnityEngine;

[ExecuteInEditMode]
public class RainManager : MonoBehaviour {


	[FMODUnity.EventRef]
    public string m_RainSound;

	FMOD.Studio.EventInstance m_RainEvent;
	FMOD.Studio.ParameterInstance m_RainIntensityEvent;


	public	static	RainManager		Instance						= null;

	[SerializeField]
	private	bool					EnableInEditor					= false;
	
	[Header("Rain Properties")]
	[Tooltip("Intensity of rain (0-1)")]
	[Range(0.0f, 1.0f)]
	[SerializeField]
	private		float				m_RainIntensity					= 0.0f;
	public		float				RainIntensity
	{
		get { return m_RainIntensity; }
		set { m_RainIntensity = value; }
	}

	[Tooltip("The height above the camera that the rain will start falling from")]
	[SerializeField]
	private		float				m_RainHeight					= 25.0f;

	[Tooltip("How far the rain particle system is ahead of the player")]
	[SerializeField]
	private		float				m_RainForwardOffset				= -1.5f;



	private		ParticleSystem		m_RainFallParticleSystem		= null;
	private		ParticleSystem		m_RainExplosionParticleSystem	= null;

	private		LoopingAudioSource	m_AudioSourceRainLight			= null;
	private		LoopingAudioSource	m_AudioSourceRainMedium			= null;
	private		LoopingAudioSource	m_AudioSourceRainHeavy			= null;
	private		LoopingAudioSource	m_AudioSourceRainCurrent		= null;

	private		Material			m_RainMaterial					= null;
	private		Material			m_RainExplosionMaterial			= null;

	private		Camera				m_Camera						= null;
	private		float				m_RainIntensityInternal			= 0.0f;


	//////////////////////////////////////////////////////////////////////////
	// START
	private void Start()
	{
#if UNITY_EDITOR
		if ( UnityEditor.EditorApplication.isPlaying == false )
			return;
#endif
		m_RainEvent = FMODUnity.RuntimeManager.CreateInstance( m_RainSound );
		m_RainEvent.getParameter( "RainIntensity", out m_RainIntensityEvent );
		m_RainEvent.start();	
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void	OnEnable()
	{
		m_Camera = Camera.current;

		{//	m_RainFallParticleSystem Child
			Transform child = transform.Find( "RainFallParticleSystem" );;
			if ( child )
				m_RainFallParticleSystem = child.GetComponent<ParticleSystem>();

			if ( m_RainFallParticleSystem == null )
			{
				enabled = false;
				return;
			}
		}

		{//	m_RainExplosionParticleSystem Child
			Transform child = transform.Find( "RainExplosionParticleSystem" );;
			if ( child )
				m_RainExplosionParticleSystem = child.GetComponent<ParticleSystem>();

			if ( m_RainExplosionParticleSystem == null )
			{
				enabled = false;
				return;
			}
		}

		// m_RainFallParticleSystem Setup
		{
			Renderer rainRenderer = m_RainFallParticleSystem.GetComponent<Renderer>();
			m_RainMaterial = new Material( rainRenderer.sharedMaterial );
			m_RainMaterial.EnableKeyword( "SOFTPARTICLES_OFF" );
			rainRenderer.material = m_RainMaterial;
		}

		// m_RainExplosionParticleSystem Setup
		{
			Renderer rainRenderer = m_RainExplosionParticleSystem.GetComponent<Renderer>();
			m_RainExplosionMaterial = new Material( rainRenderer.sharedMaterial );
			m_RainExplosionMaterial.EnableKeyword( "SOFTPARTICLES_OFF" );
			rainRenderer.material = m_RainExplosionMaterial;
		}

		/*
		// Audio Sources Setup
		Transform audioSources = transform.Find( "AudioSources" );
		{
			m_AudioSourceRainLight = new LoopingAudioSource();
			m_AudioSourceRainLight.AudioSource	= audioSources.Find( "RainLight" ).GetComponent<AudioSource>();
			m_AudioSourceRainLight.Silence();
		}
		{
			m_AudioSourceRainMedium = new LoopingAudioSource();
			m_AudioSourceRainMedium.AudioSource	= audioSources.Find( "RainMedium" ).GetComponent<AudioSource>();
			m_AudioSourceRainMedium.Silence();
		}
		{
			m_AudioSourceRainHeavy = new LoopingAudioSource();
			m_AudioSourceRainHeavy.AudioSource	= audioSources.Find( "RainHeavy" ).GetComponent<AudioSource>();
			m_AudioSourceRainHeavy.Silence();
		}
		*/

		m_RainIntensityInternal = m_RainIntensity;
		Instance = this;

#if UNITY_EDITOR
		UnityEditor.EditorApplication.update += Update;
#endif
	}

	//////////////////////////////////////////////////////////////////////////
	// OnDisable
	private void OnDisable()
	{
		m_RainEvent.stop( FMOD.Studio.STOP_MODE.IMMEDIATE );
		m_RainEvent.release();
#if UNITY_EDITOR
		UnityEditor.EditorApplication.update -= Update;
#endif
	}

	//////////////////////////////////////////////////////////////////////////
	// UpdateRain
	private void	UpdateRain()
	{
#if UNITY_EDITOR
		if ( UnityEditor.EditorApplication.isPlaying == false )
			if ( UnityEditor.SceneView.lastActiveSceneView != null )
				m_Camera = UnityEditor.SceneView.lastActiveSceneView.camera;
#endif

		if ( m_Camera == null )
			m_Camera = Camera.current;
		if ( m_Camera == null )
			return;
		
		// Keep rain particle system above the player
		m_RainFallParticleSystem.transform.position = m_Camera.transform.position;
		m_RainFallParticleSystem.transform.Translate( 0.0f, m_RainHeight, m_RainForwardOffset );
		m_RainFallParticleSystem.transform.rotation = Quaternion.Euler( 0.0f, m_Camera.transform.rotation.eulerAngles.y, 0.0f );
	}


	//////////////////////////////////////////////////////////////////////////
	// CheckForRainChange
	private void	CheckForRainChange()
	{/*
		m_RainIntensityInternal = Mathf.Lerp( m_RainIntensityInternal, m_RainIntensity, Time.deltaTime );
	//	if ( m_RainIntensityInternal != m_RainIntensity )
		{
			// If rain intensity is too low stop particle system and audiosource
			if ( m_RainIntensityInternal <= 0.01f )
			{
				m_AudioSourceRainCurrent = null;
				m_AudioSourceRainLight.Silence();
				m_AudioSourceRainMedium.Silence();
				m_AudioSourceRainHeavy.Silence();
				m_RainFallParticleSystem.Stop();
				return;
			}
	

			LoopingAudioSource newSource = null;
			{	// Get new AudioSource
				if ( m_RainIntensityInternal < 0.33f )
				{
					newSource = m_AudioSourceRainLight;
				}
				if ( m_RainIntensityInternal >= 0.33f )
				{
					newSource = m_AudioSourceRainMedium;
				}
				if ( m_RainIntensityInternal >= 0.67f )
				{
					newSource = m_AudioSourceRainHeavy;
				}
			}

			// If necessary chancge current AudioSource
			if ( newSource != null )
			{
				if ( m_AudioSourceRainCurrent != null)
				{
					m_AudioSourceRainCurrent.Silence();
				}
				m_AudioSourceRainCurrent = newSource;
				m_AudioSourceRainCurrent.SetVolume( 1.0f );
			}
				*/
			// Update particle system rate of emission
			{
				ParticleSystem.EmissionModule e = m_RainFallParticleSystem.emission;
				if ( !m_RainFallParticleSystem.isPlaying )
				{
					m_RainFallParticleSystem.Play();
				}
				ParticleSystem.MinMaxCurve rate = e.rateOverTime;
				rate.mode			= ParticleSystemCurveMode.Constant;
				rate.constantMin	= rate.constantMax = RainFallEmissionRate();
				e.rateOverTime		= rate;
			}
//		}
	}


	//////////////////////////////////////////////////////////////////////////
	// RainFallEmissionRate
	private float	RainFallEmissionRate()
	{
		return ( m_RainFallParticleSystem.main.maxParticles / m_RainFallParticleSystem.main.startLifetime.constant ) * m_RainIntensity;
	}


	//////////////////////////////////////////////////////////////////////////
	// UNITY
	private void	Update()
	{
		/*
		FMOD.Studio.PLAYBACK_STATE playState;
		m_RainEvent.getPlaybackState( out playState );

		if ( EnableInEditor == false )
		{
			m_RainEvent.stop( FMOD.Studio.STOP_MODE.IMMEDIATE );
			m_RainIntensity = 0;
			return;
		}

		if ( m_RainIntensity > 0f && playState != FMOD.Studio.PLAYBACK_STATE.PLAYING )
			m_RainEvent.start();
			*/
		m_RainIntensityEvent.setValue( m_RainIntensity );

		CheckForRainChange();
		UpdateRain();

		/*
		m_AudioSourceRainLight.Update();
		m_AudioSourceRainMedium.Update();
		m_AudioSourceRainHeavy.Update();
		*/
	}
	
}