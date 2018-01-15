
using System.Collections;
using UnityEngine;

public class RainScript : MonoBehaviour {

	[Header("Rain Properties")]
	[Tooltip("Light rain looping clip")]
	public		AudioClip			m_RainSoundLight				= null;

	[Tooltip("Medium rain looping clip")]
	public		AudioClip			m_RainSoundMedium				= null;

	[Tooltip("Heavy rain looping clip")]
	public		AudioClip			m_RainSoundHeavy				= null;

	[Tooltip("Intensity of rain (0-1)")]
	[Range(0.0f, 1.0f)]
	public		float				m_RainIntensity					= 0.0f;

	[Tooltip("The threshold for intensity (0 - 1) at which mist starts to appear")]
	[Range(0.0f, 1.0f)]
	public		float				m_RainMistThreshold				= 0.5f;

	[Header("Wind Properties")]
	[Tooltip("Wind looping clip")]
	public		AudioClip			m_WindSound						= null;

	[Tooltip("Wind sound volume modifier, use this to lower your sound if it's too loud.")]
	public		float				m_WindSoundVolumeModifier		= 0.5f;

	[Tooltip("X = minimum wind speed. Y = maximum wind speed. Z = sound multiplier. Wind speed is divided by Z to get sound multiplier value. Set Z to lower than Y to increase wind sound volume, or higher to decrease wind sound volume.")]
	public		Vector3				m_WindSpeedRange				= new Vector3( 50.0f, 100.0f, 500.0f );

	[Tooltip("How often the wind speed and direction changes (minimum and maximum change interval in seconds)")]
	public		Vector2				m_WindChangeInterval			= new Vector2( 5.0f, 30.0f );

	[Tooltip("Whether wind should be enabled.")]
	public		bool				m_EnableWind					= true;

	[Header("Others")]
	[Tooltip("The height above the camera that the rain will start falling from")]
	public		float				m_RainHeight					= 25.0f;

	[Tooltip("How far the rain particle system is ahead of the player")]
	public		float				m_RainForwardOffset				= -1.5f;

	[Tooltip("The top y value of the mist particles")]
	public		float				m_RainMistHeight				= 3.0f;


	protected	ParticleSystem		m_RainFallParticleSystem		= null;
	protected	ParticleSystem		m_RainExplosionParticleSystem	= null;
	protected	ParticleSystem		m_RainMistParticleSystem		= null;
	protected	WindZone			m_WindZone						= null;

	protected	LoopingAudioSource	m_AudioSourceRainLight			= null;
	protected	LoopingAudioSource	m_AudioSourceRainMedium			= null;
	protected	LoopingAudioSource	m_AudioSourceRainHeavy			= null;
	protected	LoopingAudioSource	m_AudioSourceRainCurrent		= null;
	protected	LoopingAudioSource	m_AudioSourceWind				= null;

	protected	Material			m_RainMaterial					= null;
	protected	Material			m_RainExplosionMaterial			= null;
	protected	Material			m_RainMistMaterial				= null;

	private		Camera				m_Camera						= null;
	private		float				m_LastRainIntensityValue		= -1.0f;
	private		IEnumerator			m_WindUpdateCoroutine			= null;



	//////////////////////////////////////////////////////////////////////////
	// AWAKE
	protected virtual void Awake()
	{

		m_Camera = Camera.main;

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
			Transform child = transform.Find( "RainMistParticleSystem" );;
			if ( child )
				m_RainMistParticleSystem = child.GetComponent<ParticleSystem>();

		}

		{//	m_RainMistParticleSystem Child
			Transform child = transform.Find( "WindZone" );;
			if ( child )
				m_WindZone = child.GetComponent<WindZone>();

		}

		// m_RainFallParticleSystem Setup
		{
			ParticleSystem.EmissionModule e = m_RainFallParticleSystem.emission;
			e.enabled = false;
			Renderer rainRenderer = m_RainFallParticleSystem.GetComponent<Renderer>();
			rainRenderer.enabled = false;
			m_RainMaterial = new Material( rainRenderer.material );
			m_RainMaterial.EnableKeyword( "SOFTPARTICLES_OFF" );
			rainRenderer.material = m_RainMaterial;
		}

		// m_RainExplosionParticleSystem Setup
		{
			ParticleSystem.EmissionModule e = m_RainExplosionParticleSystem.emission;
			e.enabled = false;
			Renderer rainRenderer = m_RainExplosionParticleSystem.GetComponent<Renderer>();
			m_RainExplosionMaterial = new Material( rainRenderer.material );
			m_RainExplosionMaterial.EnableKeyword( "SOFTPARTICLES_OFF" );
			rainRenderer.material = m_RainExplosionMaterial;
		}

		// m_RainMistParticleSystem Setup
		if ( m_RainMistParticleSystem != null )
		{
			ParticleSystem.EmissionModule e = m_RainMistParticleSystem.emission;
			e.enabled = false;
			Renderer rainRenderer = m_RainMistParticleSystem.GetComponent<Renderer>();
			rainRenderer.enabled = false;
			m_RainMistMaterial = new Material( rainRenderer.material );
			m_RainMistMaterial.EnableKeyword( "SOFTPARTICLES_ON" );
			rainRenderer.material = m_RainMistMaterial;
		}

		// Audio Sources Setup
		m_AudioSourceRainLight	= new LoopingAudioSource( this, m_RainSoundLight	);
		m_AudioSourceRainMedium	= new LoopingAudioSource( this, m_RainSoundMedium	);
		m_AudioSourceRainHeavy	= new LoopingAudioSource( this, m_RainSoundHeavy	);
		m_AudioSourceWind			= new LoopingAudioSource( this, m_WindSound		);
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateWind
	private void UpdateWind()
	{

		if ( m_EnableWind && m_WindZone != null && m_WindSpeedRange.y > 1.0f )
		{
			if ( m_WindZone.gameObject.activeSelf == false )
				m_WindZone.gameObject.SetActive( true );

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

			m_AudioSourceWind.Play( ( m_WindZone.windMain / m_WindSpeedRange.z ) * m_WindSoundVolumeModifier * m_RainIntensity );
		}
		else
		{
			if ( m_WindZone != null && m_WindZone.gameObject.activeSelf == true )
			{
				m_WindZone.gameObject.SetActive( true );
			}
			m_AudioSourceWind.Stop();
		}

		m_AudioSourceWind.Update();
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

			m_WindZone.windMain		= Mathf.Lerp( startWindMain, finalWindMain, interpolant );
			m_WindZone.windTurbulence	= Mathf.Lerp( startWindTurbulence, finalWindTurbulence, interpolant );
			m_WindZone.transform.rotation		= Quaternion.Lerp( startRotation, finalWindZoneRotation, interpolant );
			yield return null;
		}

		m_WindZone.windMain		= finalWindMain;
		m_WindZone.windTurbulence	= finalWindTurbulence;
		m_WindZone.transform.rotation		= finalWindZoneRotation;

		m_WindUpdateCoroutine = null;
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateRain
	private void UpdateRain()
	{
		// keep rain and mist above the player
		if ( m_RainFallParticleSystem != null )
		{
//			var s = m_RainFallParticleSystem.shape;
//			s.shapeType = ParticleSystemShapeType.ConeVolume;
			m_RainFallParticleSystem.transform.position = m_Camera.transform.position;
			m_RainFallParticleSystem.transform.Translate( 0.0f, m_RainHeight, m_RainForwardOffset );
			m_RainFallParticleSystem.transform.rotation = Quaternion.Euler( 0.0f, m_Camera.transform.rotation.eulerAngles.y, 0.0f );

			if ( m_RainMistParticleSystem != null )
			{
				var s2 = m_RainMistParticleSystem.shape;
				s2.shapeType = ParticleSystemShapeType.HemisphereShell;
				Vector3 pos = m_Camera.transform.position;
				pos.y += m_RainMistHeight;
				m_RainMistParticleSystem.transform.position = pos;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// CheckForRainChange
	private void CheckForRainChange()
	{
		if ( m_LastRainIntensityValue != m_RainIntensity)
		{
			m_LastRainIntensityValue = m_RainIntensity;
			if (m_RainIntensity <= 0.01f)
			{
				if ( m_AudioSourceRainCurrent != null )
				{
					m_AudioSourceRainCurrent.Stop();
					m_AudioSourceRainCurrent = null;
				}
				{
				ParticleSystem.EmissionModule e = m_RainFallParticleSystem.emission;
					e.enabled = false;
					m_RainFallParticleSystem.Stop();
				}
				{
					ParticleSystem.EmissionModule e = m_RainMistParticleSystem.emission;
					e.enabled = false;
					m_RainMistParticleSystem.Stop();
				}
			}
			else
			{
				LoopingAudioSource newSource;
				if ( m_RainIntensity >= 0.67f )
				{
					newSource = m_AudioSourceRainHeavy;
				}
				else if (m_RainIntensity >= 0.33f)
				{
					newSource = m_AudioSourceRainMedium;
				}
				else
				{
					newSource = m_AudioSourceRainLight;
				}
				if ( m_AudioSourceRainCurrent != newSource )
				{
					if ( m_AudioSourceRainCurrent != null)
					{
						m_AudioSourceRainCurrent.Stop();
					}
					m_AudioSourceRainCurrent = newSource;
					m_AudioSourceRainCurrent.Play(1.0f);
				}
				

				{
					ParticleSystem.EmissionModule e = m_RainFallParticleSystem.emission;
					e.enabled = m_RainFallParticleSystem.GetComponent<Renderer>().enabled = true;
					if (!m_RainFallParticleSystem.isPlaying)
					{
						m_RainFallParticleSystem.Play();
					}
					ParticleSystem.MinMaxCurve rate = e.rateOverTime;
					rate.mode = ParticleSystemCurveMode.Constant;
					rate.constantMin = rate.constantMax = RainFallEmissionRate();
					e.rateOverTime = rate;
				}
				
				{
					ParticleSystem.EmissionModule e = m_RainMistParticleSystem.emission;
					e.enabled = m_RainMistParticleSystem.GetComponent<Renderer>().enabled = true;
					if ( !m_RainMistParticleSystem.isPlaying )
					{
						m_RainMistParticleSystem.Play();
					}
					float emissionRate;
					if ( m_RainIntensity < m_RainMistThreshold )
					{
						emissionRate = 0.0f;
					}
					else
					{
						// must have m_RainMistThreshold or higher rain intensity to start seeing mist
						emissionRate = MistEmissionRate();
					}
					ParticleSystem.MinMaxCurve rate = e.rateOverTime;
					rate.mode = ParticleSystemCurveMode.Constant;
					rate.constantMin = rate.constantMax = emissionRate;
					e.rateOverTime = rate;
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// RainFallEmissionRate
	protected virtual float RainFallEmissionRate()
	{
		return ( m_RainFallParticleSystem.main.maxParticles / m_RainFallParticleSystem.main.startLifetime.constant ) * m_RainIntensity;
	}


	//////////////////////////////////////////////////////////////////////////
	// MistEmissionRate
	protected virtual float MistEmissionRate()
	{
		return ( m_RainMistParticleSystem.main.maxParticles / m_RainMistParticleSystem.main.startLifetime.constant ) * m_RainIntensity * m_RainIntensity;
	}


	//////////////////////////////////////////////////////////////////////////
	// UNITY
	protected virtual void Update()
	{
		CheckForRainChange();
		UpdateWind();
		UpdateRain();

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

	public AudioSource AudioSource { get; private set; }
	public float TargetVolume { get; private set; }


	//////////////////////////////////////////////////////////////////////////
	// CONSTRUCTOR
	public LoopingAudioSource( MonoBehaviour script, AudioClip clip )
	{
		AudioSource = script.gameObject.AddComponent<AudioSource>();
		AudioSource.loop = true;
		AudioSource.clip = clip;
		AudioSource.playOnAwake = false;
		AudioSource.volume = 0.0f;
		AudioSource.Stop();
		TargetVolume = 1.0f;
	}


	//////////////////////////////////////////////////////////////////////////
	// Play
	public void Play(float targetVolume)
	{
		if ( !AudioSource.isPlaying )
		{
			AudioSource.volume = 0.0f;
			AudioSource.Play();
		}
		TargetVolume = targetVolume;
	}


	//////////////////////////////////////////////////////////////////////////
	// Stop
	public void Stop()
	{
		TargetVolume = 0.0f;
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	public void Update()
	{
		if (AudioSource.isPlaying && (AudioSource.volume = Mathf.Lerp(AudioSource.volume, TargetVolume, Time.deltaTime)) == 0.0f)
		{
			AudioSource.Stop();
		}
	}

}