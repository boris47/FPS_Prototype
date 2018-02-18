
using UnityEngine;
using FMOD.Studio;
using System.Collections;

[ExecuteInEditMode]
public class RainManager : MonoBehaviour {

	public	static	RainManager		Instance						= null;

	[FMODUnity.EventRef]
    public string					m_RainSound						= "";

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

	// FMOD
	private		EventInstance		m_RainEvent;
	private		ParameterInstance	m_RainIntensityEvent;

	// RAIN
	private		ParticleSystem		m_RainFallParticleSystem		= null;
	private		ParticleSystem		m_RainExplosionParticleSystem	= null;
	private		Material			m_RainMaterial					= null;
	private		Material			m_RainExplosionMaterial			= null;
	private		float				m_RainIntensityInternal			= 0.0f;

	// THUNDERBOLTS
	private		AudioClip[]			m_ThundersCollection			= null;
	[SerializeField]
	private		float				m_NextThunderTimer				= 0f;
	private		Light				m_ThunderLight					= null;

	private		float				m_ThunderTimerMin				= 6f;
	private		float				m_ThunderTimerMax				= 25f;
	private		float				m_ThunderLifeTimeMin			= 0.08f;
	private		float				m_ThunderLifeTimeMax			= 0.5f;
	private		float				m_ThunderStepsMin				= 3f;
	private		float				m_ThunderStepsMax				= 9f;

	private		Camera				m_Camera						= null;


	//////////////////////////////////////////////////////////////////////////
	// START
	private void Start()
	{
#if UNITY_EDITOR
		if ( UnityEditor.EditorApplication.isPlaying == false )
			return;
#endif
		AudioCollection audioCollection = Resources.Load<AudioCollection>( "Weather/Sounds/Thunderbolts/Thunders" );
		if ( audioCollection != null )
			m_ThundersCollection = audioCollection.AudioSources;

		m_NextThunderTimer = Random.Range( 6f, 25f );

		m_RainEvent = FMODUnity.RuntimeManager.CreateInstance( m_RainSound );
		m_RainEvent.getParameter( "RainIntensity", out m_RainIntensityEvent );
		m_RainEvent.start();	
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void	OnEnable()
	{
		m_Camera = Camera.current;

		//	m_RainFallParticleSystem Child
		{
			Transform child = transform.Find( "RainFallParticleSystem" );;
			if ( child )
				m_RainFallParticleSystem = child.GetComponent<ParticleSystem>();

			if ( m_RainFallParticleSystem == null )
			{
				enabled = false;
				return;
			}
		}

		//	m_RainExplosionParticleSystem Child
		{
			Transform child = transform.Find( "RainExplosionParticleSystem" );;
			if ( child )
				m_RainExplosionParticleSystem = child.GetComponent<ParticleSystem>();

			if ( m_RainExplosionParticleSystem == null )
			{
				enabled = false;
				return;
			}
		}

		// ThunderLight
		{
			Transform child = transform.Find( "ThunderLight" );
			if ( child )
				m_ThunderLight = child.GetComponent<Light>();

			if ( m_ThunderLight == null )
			{
				enabled = false;
				return;
			}
		}


		// m_RainFallParticleSystem Setup
		{
			Renderer rainRenderer = m_RainFallParticleSystem.GetComponent<Renderer>();
			if ( rainRenderer != null && rainRenderer.sharedMaterial != null )
			{
				m_RainMaterial = new Material( rainRenderer.sharedMaterial );
				m_RainMaterial.EnableKeyword( "SOFTPARTICLES_OFF" );
				rainRenderer.material = m_RainMaterial;
			}
		}

		// m_RainExplosionParticleSystem Setup
		{
			Renderer rainRenderer = m_RainExplosionParticleSystem.GetComponent<Renderer>();
			if ( rainRenderer != null && rainRenderer.sharedMaterial != null )
			{
				m_RainExplosionMaterial = new Material( rainRenderer.sharedMaterial );
				m_RainExplosionMaterial.EnableKeyword( "SOFTPARTICLES_OFF" );
				rainRenderer.material = m_RainExplosionMaterial;
			}
		}

		// Get info from settings file
		if ( GLOBALS.Configs != null )
		{
			Section pSection = GLOBALS.Configs.GetSection( "Thunderbolts" );
			if ( pSection != null )
			{
				pSection.bAsFloat( "ThunderTimerMin",		ref m_ThunderTimerMin );
				pSection.bAsFloat( "ThunderTimerMax",		ref m_ThunderTimerMax );

				pSection.bAsFloat( "ThunderLifeTimeMin",	ref m_ThunderLifeTimeMin );
				pSection.bAsFloat( "ThunderLifeTimeMax",	ref m_ThunderLifeTimeMax );

				pSection.bAsFloat( "ThunderStepsMin",		ref m_ThunderStepsMin );
				pSection.bAsFloat( "ThunderStepsMin",		ref m_ThunderStepsMax );

			}
		}

		m_RainIntensityInternal = m_RainIntensity;
		Instance = this;
		/*
#if UNITY_EDITOR
		UnityEditor.EditorApplication.update += Update;
#endif
*/
	}

	//////////////////////////////////////////////////////////////////////////
	// OnDisable
	private void OnDisable()
	{
		Instance = null;
		m_RainExplosionMaterial = null;
		m_RainMaterial = null;
		m_RainIntensityInternal = m_RainIntensity = 0f;
		m_Camera = null;

		m_RainEvent.stop( STOP_MODE.IMMEDIATE );
		m_RainEvent.release();
		/*
#if UNITY_EDITOR
		UnityEditor.EditorApplication.update -= Update;
#endif
*/
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

//		if ( m_Camera == null )
//			m_Camera = Camera.current;
		if ( m_Camera == null )
			m_Camera = Camera.main;
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
	{
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
	}


	//////////////////////////////////////////////////////////////////////////
	// ThunderCoroutine ( Coroutine )
	private	IEnumerator	ThunderCoroutine()
	{
		float	thunderLifeTime = Random.Range( m_ThunderLifeTimeMin, m_ThunderLifeTimeMax );
		float	thunderSteps	= Random.Range( m_ThunderStepsMin, m_ThunderStepsMax );
		float	thunderLifeStep	= thunderLifeTime / thunderSteps;
		float	currentLifeTime	= 0f;
		bool	lightON			= false;

		// Random rotation for thunder light
		Quaternion thunderLightRotation = Quaternion.Euler( m_ThunderLight.transform.rotation.eulerAngles + Vector3.up * Random.Range( -360f, 360f ) );
		m_ThunderLight.transform.rotation = thunderLightRotation;
		
		// Lighting effect
		while ( currentLifeTime < thunderLifeTime )
		{
			m_ThunderLight.transform.rotation = thunderLightRotation;
			m_ThunderLight.intensity = lightON ?
				0.001f
			:
				Random.value + 1f;
			lightON = !lightON;
			currentLifeTime += thunderLifeStep;
			yield return new WaitForSeconds ( Random.Range( thunderLifeStep, thunderLifeTime - currentLifeTime ) );
		}
		m_ThunderLight.intensity = 0.001f;


		// Play Clip
		AudioClip thunderClip =  m_ThundersCollection[ Random.Range( 0, m_ThundersCollection.Length ) ];
		Vector3 thunderDirection = m_ThunderLight.transform.rotation.eulerAngles.normalized * Random.Range( 1f, 10f );
		Vector3 thunderPosition = Player.CurrentActivePlayer.transform.position + thunderDirection;
		AudioSource.PlayClipAtPoint( thunderClip, thunderPosition );
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateRain
	private void	UpdateThunderbols()
	{
		if ( m_RainIntensity > 0.4f )
		{
			m_NextThunderTimer -= Time.deltaTime;
			if ( m_NextThunderTimer < 0f )
			{
				StartCoroutine( ThunderCoroutine() );

				m_NextThunderTimer = Random.Range( 6f, 25f );
			}
		}
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
		CheckForRainChange();
		UpdateRain();

#if UNITY_EDITOR
		if ( UnityEditor.EditorApplication.isPlaying == false )
			return;
#endif
		if ( EnableInEditor == false )
		{
			m_RainIntensityEvent.setValue( m_RainIntensity );

			UpdateThunderbols();
		}
	}
	
}