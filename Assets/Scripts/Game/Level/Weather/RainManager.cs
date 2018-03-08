
using UnityEngine;
using FMOD.Studio;
using System.Collections;

namespace WeatherSystem {

	[ExecuteInEditMode]
	public class RainManager : MonoBehaviour {

#region VARS

		// STATIC
		public	static	RainManager		Instance						= null;


		// CONST
		private	const	string			THUNDERS_DISTANT				= "Sounds/Thunders/ThundersDistant";
		private	const	string			THUNDERS_DURING_RAIN			= "Sounds/Thunders/ThundersDuringRain";


		// SERIALIZED
		[FMODUnity.EventRef]
		public string					m_RainSound						= "";

		[SerializeField]
		private	bool					EnableInEditor					= false;
	
		[Header("Rain Properties")]
		[SerializeField, Tooltip("Intensity of rain (0-1)"), Range(0.0f, 1.0f)]
		private		float				m_RainIntensity					= 0.0f;
		public		float				RainIntensity
		{
			get { return m_RainIntensity; }
			set { m_RainIntensity = value; }
		}

		[SerializeField, Tooltip("The height above the camera that the rain will start falling from")]
		private		float				m_RainHeight					= 25.0f;

		[SerializeField, Tooltip("How far the rain particle system is ahead of the player")]
		private		float				m_RainForwardOffset				= -1.5f;


		// PRIVATE PROPERTIES
		
		// FMOD
		private		EventInstance		m_RainEvent;
		private		ParameterInstance	m_RainIntensityEvent;

		// RAIN
		private		ParticleSystem		m_RainFallParticleSystem		= null;
		private		ParticleSystem		m_RainExplosionParticleSystem	= null;
		private		Material			m_RainMaterial					= null;
		private		Material			m_RainExplosionMaterial			= null;

		// THUNDERBOLTS
		private		AudioClip[]			m_ThundersDistantCollection		= null;
		private		AudioClip[]			m_ThundersDuringRainCollection	= null;
		
		private		float				m_NextThunderTimer				= 0f;
		private		Light				m_ThunderLight					= null;
		private		Transform			m_ThunderAudioContainer			= null;

		private		float				m_ThunderTimerMin				= 6f;
		private		float				m_ThunderTimerMax				= 25f;
		private		float				m_ThunderLifeTimeMin			= 0.08f;
		private		float				m_ThunderLifeTimeMax			= 0.5f;
		private		float				m_ThunderStepsMin				= 3f;
		private		float				m_ThunderStepsMax				= 9f;
		private		AudioSource[]		m_ThunderAudioSources			= null;

		private		Camera				m_Camera						= null;

#endregion

#region INITIALIZATION

		//////////////////////////////////////////////////////////////////////////
		// START
		private void			Start()
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
		private void			OnEnable()
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

			// Thunderbolts audio container
			{
				Transform child = transform.Find( "AudioSources" );
				if ( child )
					m_ThunderAudioContainer = child.Find( "Thunderbolts" );

				if ( m_ThunderAudioContainer == null )
				{
					enabled = false;
					return;
				}

				m_ThunderAudioSources = m_ThunderAudioContainer.GetComponentsInChildren<AudioSource>();
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
				var pSection = GLOBALS.Configs.GetSection( "Thunderbolts" );
				if ( pSection != null )
				{
					pSection.bAsFloat( "ThunderTimerMin",		ref m_ThunderTimerMin );
					pSection.bAsFloat( "ThunderTimerMax",		ref m_ThunderTimerMax );

					pSection.bAsFloat( "ThunderLifeTimeMin",	ref m_ThunderLifeTimeMin );
					pSection.bAsFloat( "ThunderLifeTimeMax",	ref m_ThunderLifeTimeMax );

					pSection.bAsFloat( "ThunderStepsMin",		ref m_ThunderStepsMin );
					pSection.bAsFloat( "ThunderStepsMax",		ref m_ThunderStepsMax );
				}
			}

			AudioCollection thundersDistantCollection = Resources.Load<AudioCollection>( THUNDERS_DISTANT );
			if ( thundersDistantCollection != null )
				m_ThundersDistantCollection = thundersDistantCollection.AudioClips;
			else
			{
				print( "Cannot load scriptable " +  THUNDERS_DISTANT );
				enabled = false;
				return;
			}

			AudioCollection thundersDurinRainCollection = Resources.Load<AudioCollection>( THUNDERS_DURING_RAIN );
			if ( thundersDurinRainCollection != null )
				m_ThundersDuringRainCollection = thundersDurinRainCollection.AudioClips;
			else
			{
				print( "Cannot load scriptable " +  THUNDERS_DURING_RAIN );
				enabled = false;
				return;
			}
			
			m_NextThunderTimer = Random.Range( m_ThunderTimerMin, m_ThunderTimerMax );

			Instance = this;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnDisable
		private void			OnDisable()
		{
			Instance = null;
			m_RainExplosionMaterial = null;
			m_RainMaterial = null;
			m_RainIntensity = 0f;
			m_Camera = null;

			m_RainEvent.stop( STOP_MODE.IMMEDIATE );
			m_RainEvent.release();
		}

#endregion

#region INTERNAL METHODS

		//////////////////////////////////////////////////////////////////////////
		// UpdateRain
		private void			UpdateRain()
		{
			// Keep rain particle system above the player
			m_RainFallParticleSystem.transform.position = m_Camera.transform.position;
			m_RainFallParticleSystem.transform.Translate( 0.0f, m_RainHeight, m_RainForwardOffset );
			m_RainFallParticleSystem.transform.rotation = Quaternion.Euler( 0.0f, m_Camera.transform.rotation.eulerAngles.y, 0.0f );
		}


		//////////////////////////////////////////////////////////////////////////
		// CheckForRainChange
		private void			CheckForRainChange()
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
		// GetFreeAudioSource
		private	AudioSource		GetFreeAudioSource()
		{
			foreach( AudioSource a in m_ThunderAudioSources )
			{
				if ( a.isPlaying == false )
					return a;
			}
			return null;
		}


		//////////////////////////////////////////////////////////////////////////
		// ThunderCoroutine ( Coroutine )
		private	IEnumerator		ThunderCoroutine( bool lighting )
		{
			// check fo avaiable audiosource
			AudioSource thunderAudioSource = GetFreeAudioSource();
			if ( thunderAudioSource == null )
				yield break;

			float	thunderLifeTime = Random.Range( m_ThunderLifeTimeMin, m_ThunderLifeTimeMax );
			float	thunderSteps	= Random.Range( m_ThunderStepsMin, m_ThunderStepsMax );
			float	thunderLifeStep	= thunderLifeTime / thunderSteps;
			float	currentLifeTime	= 0f;
			bool	lightON			= false;

			Material skyMixerMaterial = WeatherManager.Internal.SkyMixerMaterial;
			
			// Random rotation for thunder light
			Quaternion thunderLightRotation = Quaternion.Euler( m_ThunderLight.transform.rotation.eulerAngles + Vector3.up * Random.Range( -360f, 360f ) );
			m_ThunderLight.transform.rotation = thunderLightRotation;
			
			if ( lighting == true )
			{
				// Lighting effect
				while ( currentLifeTime < thunderLifeTime )
				{
					// Thunder light rotation
					m_ThunderLight.transform.rotation = thunderLightRotation;

					float randomIntensity = Random.Range( 0.3f, 1.0f );

					// thunder light intensity
					m_ThunderLight.intensity = lightON ? randomIntensity : 0.001f;

					// Sky color
					skyMixerMaterial.SetColor( "_Tint", lightON ? Color.white * randomIntensity * 0.2f : Color.clear );

					lightON = !lightON;
					currentLifeTime += thunderLifeStep;
					yield return new WaitForSeconds ( Random.Range( thunderLifeStep, thunderLifeTime - currentLifeTime ) );
				}
				m_ThunderLight.intensity = 0.001f;
				skyMixerMaterial.SetColor( "_Tint", Color.clear );
			}

			yield return new WaitForSeconds ( Random.Range( 0.1f, 3f ) );

			// Play Clip
			if ( lighting == true )
			{
				AudioClip thunderClip =  m_ThundersDuringRainCollection[ Random.Range( 0, m_ThundersDuringRainCollection.Length ) ];
				thunderAudioSource.clip = thunderClip;
			}
			else
			{
				AudioClip thunderClip =  m_ThundersDistantCollection[ Random.Range( 0, m_ThundersDistantCollection.Length ) ];
				thunderAudioSource.clip = thunderClip;
			}

			thunderAudioSource.pitch	= Random.Range( 1.2f, 1.6f );
			thunderAudioSource.volume	= Random.Range( 1f, 2f );
			
			Vector3 thunderDirection = m_ThunderLight.transform.forward * Random.Range( 15f, 25f );
			thunderAudioSource.transform.localPosition = thunderDirection;

			thunderAudioSource.Play();
		}


		//////////////////////////////////////////////////////////////////////////
		// UpdateRain
		private void			UpdateThunderbols()
		{
			if ( m_RainIntensity > 0.1f )
			{
				m_NextThunderTimer -= Time.deltaTime;
				if ( m_NextThunderTimer < 0f )
				{
					StartCoroutine( ThunderCoroutine( m_RainIntensity > 0.2f ) );

					m_NextThunderTimer = Random.Range
					(
						( m_ThunderTimerMin / 2f ) + ( m_ThunderTimerMin * ( 1f - m_RainIntensity ) ),
						( m_ThunderTimerMax / 2f ) + ( m_ThunderTimerMax * ( 1f - m_RainIntensity ) )
					);
				}
			}
			m_ThunderAudioContainer.position = m_Camera.transform.position;
		}


		//////////////////////////////////////////////////////////////////////////
		// RainFallEmissionRate
		private float			RainFallEmissionRate()
		{
			return ( m_RainFallParticleSystem.main.maxParticles / m_RainFallParticleSystem.main.startLifetime.constant ) * m_RainIntensity;
		}


		//////////////////////////////////////////////////////////////////////////
		// UNITY
		private void			Update()
		{
#if UNITY_EDITOR
			if ( UnityEditor.EditorApplication.isPlaying == false )
				if ( UnityEditor.SceneView.lastActiveSceneView != null )
					m_Camera = UnityEditor.SceneView.lastActiveSceneView.camera;
#endif
			if ( m_Camera == null )
				m_Camera = Camera.current;
			if ( m_Camera == null )
				m_Camera = Camera.main;
			if ( m_Camera == null )
				m_Camera = CameraControl.Instance.MainCamera;
			if ( m_Camera == null )
				return;

			CheckForRainChange();
			UpdateRain();
			UpdateThunderbols();

#if UNITY_EDITOR
			if ( UnityEditor.EditorApplication.isPlaying == false )
				return;
#endif
			if ( EnableInEditor == false )
			{
				m_RainIntensityEvent.setValue( m_RainIntensity );

			}
		}

#endregion

	}

}