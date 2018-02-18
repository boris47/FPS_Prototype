
using UnityEngine;
using FMOD.Studio;

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
	private		AudioSource			m_ThunderPlayer					= null;
	private		AudioClip[]			m_ThundersCollection			= null;
	private		float				m_NextThunderTimer				= 0f;
	private		float				m_CurrentThunderTimer			= 0f;

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

		m_ThunderPlayer = gameObject.AddComponent<AudioSource>();
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
		Instance = null;
		m_RainExplosionMaterial = null;
		m_RainMaterial = null;
		m_RainIntensityInternal = m_RainIntensity = 0f;
		m_Camera = null;

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
	// UpdateRain
	private void	UpdateThunderbols()
	{
		if ( m_RainIntensity > 0.7f )
		{
			m_CurrentThunderTimer += Time.deltaTime;
			if ( m_CurrentThunderTimer > m_NextThunderTimer )
			{
				if ( m_ThunderPlayer.isPlaying == false )
					m_ThunderPlayer.PlayOneShot( m_ThundersCollection[ Random.Range( 0, m_ThundersCollection.Length ) ] );

				m_CurrentThunderTimer = 0;
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