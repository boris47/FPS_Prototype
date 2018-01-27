
using System.Collections;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class RainScript : MonoBehaviour {

	public	static	RainScript		Instance						= null;

	
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

	[Header("Wind Properties")]
	[Tooltip("Wind sound volume modifier, use this to lower your sound if it's too loud.")]
	[SerializeField]
	private		float				m_WindSoundVolumeModifier		= 0.5f;

	[Tooltip("X = minimum wind speed. Y = maximum wind speed. Z = sound multiplier. Wind speed is divided by Z to get sound multiplier value. Set Z to lower than Y to increase wind sound volume, or higher to decrease wind sound volume.")]
	[SerializeField]
	private		Vector3				m_WindSpeedRange				= new Vector3( 50.0f, 100.0f, 500.0f );

	[Tooltip("How often the wind speed and direction changes (minimum and maximum change interval in seconds)")]
	[SerializeField]
	private		Vector2				m_WindChangeInterval			= new Vector2( 5.0f, 30.0f );

	[Tooltip("Whether wind should be enabled.")]
	[SerializeField]
	private		bool				m_EnableWind					= true;

	[Header("Others")]
	[Tooltip("The height above the camera that the rain will start falling from")]
	[SerializeField]
	private		float				m_RainHeight					= 25.0f;

	[Tooltip("How far the rain particle system is ahead of the player")]
	[SerializeField]
	private		float				m_RainForwardOffset				= -1.5f;



	private		ParticleSystem		m_RainFallParticleSystem		= null;
	private		ParticleSystem		m_RainExplosionParticleSystem	= null;
	private		WindZone			m_WindZone						= null;

	private		LoopingAudioSource	m_AudioSourceRainLight			= null;
	private		LoopingAudioSource	m_AudioSourceRainMedium			= null;
	private		LoopingAudioSource	m_AudioSourceRainHeavy			= null;
	private		LoopingAudioSource	m_AudioSourceRainCurrent		= null;
	private		LoopingAudioSource	m_AudioSourceWind				= null;

	private		Material			m_RainMaterial					= null;
	private		Material			m_RainExplosionMaterial			= null;

	private		Camera				m_Camera						= null;
	private		IEnumerator			m_WindUpdateCoroutine			= null;
	private		float				m_RainIntensityInternal			= 0.0f;



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

		{//	m_RainMistParticleSystem Child
			Transform child = transform.Find( "WindZone" );;
			if ( child )
				m_WindZone = child.GetComponent<WindZone>();

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

		// Audio Sources Setup
		Transform audioSources = transform.Find( "AudioSources" );
		{
			m_AudioSourceRainLight = new LoopingAudioSource();
			m_AudioSourceRainLight.AudioSource	= audioSources.GetChild( 0 ).GetComponent<AudioSource>();
			m_AudioSourceRainLight.Silence();
		}
		{
			m_AudioSourceRainMedium = new LoopingAudioSource();
			m_AudioSourceRainMedium.AudioSource	= audioSources.GetChild( 1 ).GetComponent<AudioSource>();
			m_AudioSourceRainMedium.Silence();
		}
		{
			m_AudioSourceRainHeavy = new LoopingAudioSource();
			m_AudioSourceRainHeavy.AudioSource	= audioSources.GetChild( 2 ).GetComponent<AudioSource>();
			m_AudioSourceRainHeavy.Silence();
		}
		{
			m_AudioSourceWind = new LoopingAudioSource();
			m_AudioSourceWind.AudioSource		= audioSources.GetChild( 3 ).GetComponent<AudioSource>();
			m_AudioSourceWind.Silence();
		}

		m_RainIntensityInternal = m_RainIntensity;
		Instance = this;
	}

	//////////////////////////////////////////////////////////////////////////
	// UpdateWind
	private void	UpdateWind()
	{
		if ( m_WindZone == null )
			return;

		if ( m_EnableWind && m_WindSpeedRange.y > 1.0f )
		{
			m_WindZone.transform.position = m_Camera.transform.position;
			m_WindZone.transform.Translate( 0.0f, m_WindZone.radius, 0.0f );

			if ( m_WindUpdateCoroutine == null )
			{
				StartCoroutine
				( 
					m_WindUpdateCoroutine = UpdateWindCO
					(
						finalWindMain			: UnityEngine.Random.Range( m_WindSpeedRange.x, m_WindSpeedRange.y ),
						finalWindTurbulence		: UnityEngine.Random.Range( m_WindSpeedRange.x, m_WindSpeedRange.y ),
						finalWindZoneRotation	: Quaternion.Euler( UnityEngine.Random.Range(-30.0f, 30.0f), UnityEngine.Random.Range(0.0f, 360.0f), 0.0f )
					)
				);
			}

			m_AudioSourceWind.SetVolume( ( m_WindZone.windMain / m_WindSpeedRange.z ) * m_WindSoundVolumeModifier * m_RainIntensityInternal );
		}
		else
		{
			m_WindZone.windMain = 0f;
			m_AudioSourceWind.Silence();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateWindCO ( Coroutine )
	private	IEnumerator	UpdateWindCO( float finalWindMain, float finalWindTurbulence, Quaternion finalWindZoneRotation )
	{
		float startWindMain			= m_WindZone.windMain;
		float startWindTurbulence	= m_WindZone.windTurbulence;
		Quaternion startRotation	= m_WindZone.transform.rotation;

		float nextWindTime			= UnityEngine.Random.Range( m_WindChangeInterval.x, m_WindChangeInterval.y );
		float currentTime = 0f;
		float interpolant = 0f;

		while ( interpolant < 1f )
		{
			currentTime += Time.deltaTime;
			interpolant = currentTime / nextWindTime;

			m_WindZone.windMain				= Mathf.Lerp( startWindMain, finalWindMain, interpolant );
			m_WindZone.windTurbulence		= Mathf.Lerp( startWindTurbulence, finalWindTurbulence, interpolant );
			m_WindZone.transform.rotation	= Quaternion.Lerp( startRotation, finalWindZoneRotation, interpolant );
			yield return null;
		}

		m_WindZone.windMain				= finalWindMain;
		m_WindZone.windTurbulence		= finalWindTurbulence;
		m_WindZone.transform.rotation	= finalWindZoneRotation;

		m_WindUpdateCoroutine = null;
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateRain
	private void	UpdateRain()
	{
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
	{
		if ( m_RainIntensity == 0.0f )
		{
			m_RainIntensityInternal = 0.0f;
			m_AudioSourceRainLight.Silence();
			m_AudioSourceRainMedium.Silence();
			m_AudioSourceRainHeavy.Silence();
			return;
		}

		m_RainIntensityInternal = Mathf.Lerp( m_RainIntensityInternal, m_RainIntensity, Time.deltaTime );
	//	if ( m_RainIntensityInternal != m_RainIntensity )
		{
			// If rain intensity is too low stop particle system and audiosource
			if ( m_RainIntensityInternal <= 0.01f )
			{
				if ( m_AudioSourceRainCurrent != null )
				{
					m_AudioSourceRainCurrent.Silence();
					m_AudioSourceRainCurrent = null;
				}
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
			if ( newSource != null/* && m_AudioSourceRainCurrent != newSource*/ )
			{
				if ( m_AudioSourceRainCurrent != null)
				{
					m_AudioSourceRainCurrent.Silence();
				}
				m_AudioSourceRainCurrent = newSource;
				m_AudioSourceRainCurrent.SetVolume( 1.0f );
			}
				
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
	}


	//////////////////////////////////////////////////////////////////////////
	// RainFallEmissionRate
	private float	RainFallEmissionRate()
	{
		return ( m_RainFallParticleSystem.main.maxParticles / m_RainFallParticleSystem.main.startLifetime.constant ) * m_RainIntensityInternal;
	}


	//////////////////////////////////////////////////////////////////////////
	// UNITY
	private void	Update()
	{
		CheckForRainChange();
		UpdateRain();
//		UpdateWind();

		m_AudioSourceWind.Update();
		m_AudioSourceRainLight.Update();
		m_AudioSourceRainMedium.Update();
		m_AudioSourceRainHeavy.Update();
	}
	
}




//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////


/// <summary> Provides an easy wrapper to looping audio sources with nice transitions for volume when starting and stopping </summary>
public class LoopingAudioSource {

	public AudioSource AudioSource
	{
		get;
		set;
	}
	public float TargetVolume
	{
		get;
		private set;
	}


	//////////////////////////////////////////////////////////////////////////
	// CONSTRUCTOR
	public LoopingAudioSource()
	{
		TargetVolume = 1.0f;
	}


	//////////////////////////////////////////////////////////////////////////
	// SetVolume
	public void SetVolume( float targetVolume )
	{
		if ( AudioSource == null )
			return;

		if ( !AudioSource.isPlaying )
		{
			AudioSource.volume = 0.0f;
			AudioSource.Play();
		}
		TargetVolume = targetVolume;
	}


	//////////////////////////////////////////////////////////////////////////
	// Silence
	public void Silence()
	{
		TargetVolume = 0.0f;
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	public void Update()
	{
		if ( AudioSource == null )
			return;

		if ( AudioSource.isPlaying && ( AudioSource.volume = Mathf.Lerp( AudioSource.volume, TargetVolume, Time.deltaTime * 2f ) ) == 0.0f )
		{
			AudioSource.Stop();

		}
	}

}