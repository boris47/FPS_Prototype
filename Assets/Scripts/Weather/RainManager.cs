
using UnityEngine;
using FMOD.Studio;
using System.Collections;

namespace WeatherSystem {

	using FMODUnity;

	[ExecuteInEditMode]
	public class RainManager : MonoBehaviour {

#region VARS

		// STATIC
		private	static	RainManager			m_Instance						= null;
		public	static	RainManager			Instance
		{
			get { return m_Instance; }
		}


		// CONST
		private	const	string				THUNDERS_DISTANT				= "Sounds/Thunders/ThundersDistant";
		private	const	string				THUNDERS_DURING_RAIN			= "Sounds/Thunders/ThundersDuringRain";
		private	const	string				RAIN_MATERIAL					= "Materials/Rain/RainMaterial";
		private	const	string				RAIN_EXPLOSION_MATERIAL			= "Materials/Rain/RainExplosionMaterial";
		private	const	string				RAIN_MIST_MATERIAL				= "Materials/Rain/RainMistMaterial";

		[SerializeField]
		private	bool						EnableInEditor					= false;
	
		[Header("Rain Properties")]
		[SerializeField, Tooltip("Intensity of rain (0-1)"), Range(0.0f, 1.0f)]
		private		float					m_RainIntensity					= 0.0f;
		public		float					RainIntensity
		{
			get { return m_RainIntensity; }
			set { m_RainIntensity = value; }
		}

		[SerializeField, Tooltip("The height above the camera that the rain will start falling from")]
		private		float					m_RainHeight					= 25.0f;

//		[SerializeField, Tooltip("How far the rain particle system is ahead of the player")]
//		private		float					m_RainForwardOffset				= -1.5f;


		// PRIVATE PROPERTIES
		
		// FMOD
		private		StudioEventEmitter		m_AudioEmitter					= null;
		private		ParameterInstance		m_RainIntensityEvent;

		// RAIN
		private		ParticleSystem			m_RainFallParticleSystem		= null;
		private		ParticleSystem			m_RainExplosionParticleSystem	= null;
		private		Material				m_RainMaterial					= null;
		private		Material				m_RainExplosionMaterial			= null;

		// THUNDERBOLTS
		private		AudioClip[]				m_ThundersDistantCollection		= null;
		private		AudioClip[]				m_ThundersDuringRainCollection	= null;
		
		private		float					m_NextThunderTimer				= 0f;
		private		Light					m_ThunderLight					= null;
		private		Transform				m_ThunderAudioContainer			= null;

		[System.Serializable]
		private class ThunderboltsSectionData
		{
			public	float					ThunderTimerMin				= 6f;
			public	float					ThunderTimerMax				= 25f;
			public	float					ThunderLifeTimeMin			= 0.08f;
			public	float					ThunderLifeTimeMax			= 0.5f;
			public	float					ThunderStepsMin				= 3f;
			public	float					ThunderStepsMax				= 9f;
		}
		[SerializeField, ReadOnly]
		private ThunderboltsSectionData		m_ThunderboltsSectionData	= new ThunderboltsSectionData();


		private		CustomAudioSource[]		m_ThunderAudioSources			= null;
		
		private		Camera					m_Camera						= null;

		private		bool					m_IsFullyLoaded					= false;

		
		private void Awake()
		{
			m_Instance = this;
		}

		//////////////////////////////////////////////////////////////////////////
		// OnEnable
		//		private void			OnEnable()
		private IEnumerator Start()
		{
			m_Instance = this;
			m_Camera = Camera.current;

			yield return null;

			//	m_RainFallParticleSystem Child
			if (transform.SearchComponentInChild( "RainFallParticleSystem", ref m_RainFallParticleSystem ) == false ) 
			{
				enabled = false;
				yield break; //return;
			}

			yield return null;

			//	m_RainExplosionParticleSystem Child
			if (transform.SearchComponentInChild( "RainExplosionParticleSystem", ref m_RainExplosionParticleSystem ) == false ) 
			{
				enabled = false;
				yield break; //return;
			}

			yield return null;

			// ThunderLight
			if (transform.SearchComponentInChild( "ThunderLight", ref m_ThunderLight ) == false ) 
			{
				enabled = false;
				yield break; //return;
			}

			yield return null;

			// Thunderbolts audio container
			{
				Transform child = transform.Find( "AudioSources" );
				if ( child )
					m_ThunderAudioContainer = child.Find( "Thunderbolts" );

				if (m_ThunderAudioContainer == null )
				{
					enabled = false;
					yield break; //return;
				}

				yield return null;

				m_ThunderAudioSources = m_ThunderAudioContainer.GetComponentsInChildren<CustomAudioSource>();
			}

			yield return null;

			// m_RainFallParticleSystem Setup
			{
				Renderer rainRenderer = null;
				bool bHasRenderer = m_RainFallParticleSystem.transform.SearchComponent( ref rainRenderer, ESearchContext.LOCAL );
				if ( bHasRenderer && rainRenderer.sharedMaterial != null )
				{
					m_RainMaterial = Resources.Load<Material>( RAIN_MATERIAL );
					m_RainMaterial.EnableKeyword( "SOFTPARTICLES_OFF" );
					rainRenderer.material = m_RainMaterial;
				}
			}

			yield return null;

			// m_RainExplosionParticleSystem Setup
			{
				Renderer rainRenderer = null;
				bool bHasRenderer = m_RainExplosionParticleSystem.transform.SearchComponent( ref rainRenderer, ESearchContext.LOCAL );
				if ( bHasRenderer && rainRenderer.sharedMaterial != null )
				{
					m_RainExplosionMaterial = Resources.Load<Material>( RAIN_EXPLOSION_MATERIAL );
					m_RainExplosionMaterial.EnableKeyword( "SOFTPARTICLES_OFF" );
					rainRenderer.material = m_RainExplosionMaterial;
				}
			}

			yield return null;
			
			// Get info from settings file
			Database.Section thunderboltsSection = null;
			if ( !(GlobalManager.Configs.GetSection("Thunderbolts", ref thunderboltsSection) && GlobalManager.Configs.bSectionToOuter(thunderboltsSection, m_ThunderboltsSectionData)) )
			{
				Debug.LogError("Cannot load Thunderbolts Section");
				enabled = false;
				yield break;
			}

			yield return null;

			{
				AudioCollection thunderCollection = Resources.Load<AudioCollection>( THUNDERS_DISTANT );
				if (thunderCollection != null)
				{
					m_ThundersDistantCollection = thunderCollection.AudioClips;
				}
				else
				{
					Debug.LogError("Cannot load scriptable " + THUNDERS_DISTANT);
					enabled = false;
					yield break;
				}
			}
			yield return null;
			{
				AudioCollection thunderCollection = Resources.Load<AudioCollection>( THUNDERS_DURING_RAIN );
				if (thunderCollection != null)
				{
					m_ThundersDuringRainCollection = thunderCollection.AudioClips;
				}
				else
				{
					Debug.LogError("Cannot load scriptable " + THUNDERS_DISTANT);
					enabled = false;
					yield break;
				}
			}

			m_NextThunderTimer = Random.Range(m_ThunderboltsSectionData.ThunderTimerMin, m_ThunderboltsSectionData.ThunderTimerMax );
/*
#if UNITY_EDITOR
			if ( UnityEditor.EditorApplication.isPlaying == false )
				yield break; //return;
#endif
*/
			m_AudioEmitter = GetComponent<StudioEventEmitter>();
			m_AudioEmitter.EventInstance.getParameter( "RainIntensity", out m_RainIntensityEvent );

			m_RainFallParticleSystem.Play( withChildren: true );

			m_IsFullyLoaded = true;

			yield return null;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnDisable
		private void			OnDisable()
		{
			if (m_RainFallParticleSystem )
				m_RainFallParticleSystem.Stop( withChildren:true, stopBehavior: ParticleSystemStopBehavior.StopEmittingAndClear );

			m_Instance				= null;
			m_RainExplosionMaterial	= null;
			m_RainMaterial			= null;
			m_RainIntensity			= 0f;
			m_Camera				= null;
		}

#endregion

#region INTERNAL METHODS

		//////////////////////////////////////////////////////////////////////////
		// UpdateRain
		private void			UpdateRainPosition()
		{
			// Keep rain particle system above the player
			m_RainFallParticleSystem.transform.position = m_Camera.transform.position + (Vector3.up * m_RainHeight);
		}


		//////////////////////////////////////////////////////////////////////////
		// CheckForRainChange
		private void			CheckForRainChange()
		{
			// Update particle system rate of emission
			{
				ParticleSystem.EmissionModule e = m_RainFallParticleSystem.emission;
				if (m_RainFallParticleSystem.isPlaying == false )
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
		private	ICustomAudioSource		GetFreeAudioSource()
		{
			return System.Array.Find(m_ThunderAudioSources, source => !source.IsPlaying);
		}


		//////////////////////////////////////////////////////////////////////////
		// ThunderCoroutine ( Coroutine )
		private	IEnumerator		ThunderCoroutine( bool lighting )
		{
			// check fo available audiosource
			ICustomAudioSource thunderAudioSource = GetFreeAudioSource();
			if ( thunderAudioSource == null )
				yield break;

			float	thunderLifeTime = Random.Range(m_ThunderboltsSectionData.ThunderLifeTimeMin, m_ThunderboltsSectionData.ThunderLifeTimeMax );
			float	thunderSteps	= Random.Range(m_ThunderboltsSectionData.ThunderStepsMin, m_ThunderboltsSectionData.ThunderStepsMax );
			float	thunderLifeStep	= thunderLifeTime / thunderSteps;
			float	currentLifeTime	= 0f;
			bool	lightON			= false;
			
			// Random rotation for thunder light
			Quaternion thunderLightRotation = Quaternion.Euler(m_ThunderLight.transform.rotation.eulerAngles + (Vector3.up * Random.Range( -360f, 360f )) );
			m_ThunderLight.transform.rotation = thunderLightRotation;
			
			if ( lighting == true )
			{
			// Thunder light rotation
			m_ThunderLight.transform.rotation = thunderLightRotation;

			// Lighting effect
				float resetExposure = WeatherManager.Cycles.GetSkyExposure();
				while ( currentLifeTime < thunderLifeTime )
				{
					float randomIntensity = Random.Range( 0.06f, 0.2f );

					// thunder light intensity
					m_ThunderLight.intensity = lightON ? randomIntensity : 0.001f;

					// Sky color
					WeatherManager.Cycles.SetSkyExposure( resetExposure + (lightON ? resetExposure : 0.0f));

					lightON = !lightON;
					currentLifeTime += thunderLifeStep;
					yield return new WaitForSeconds ( Random.Range( thunderLifeStep, thunderLifeTime - currentLifeTime ) );
				}
				m_ThunderLight.intensity = 0.001f;
				WeatherManager.Cycles.SetSkyExposure(resetExposure);
			}

			yield return new WaitForSeconds ( Random.Range( 0.1f, 3f ) );


			AudioClip[] collection = lighting == true ? m_ThundersDuringRainCollection : m_ThundersDistantCollection;

			// Play Clip
			AudioClip thunderClip =  collection[ Random.Range( 0, collection.Length ) ];
			thunderAudioSource.Clip = thunderClip;

			thunderAudioSource.Pitch	= Random.Range( 1.0f, 1.6f );
			thunderAudioSource.Volume	= Random.Range( 1f, 2f );
			
			Vector3 thunderDirection = m_ThunderLight.transform.forward * Random.Range( 15f, 25f );
			thunderAudioSource.Transform.localPosition = thunderDirection;

			thunderAudioSource.Play();
		}


		//////////////////////////////////////////////////////////////////////////
		// UpdateRain
		private void			UpdateThunderbols()
		{
			if (m_RainIntensity > 0.1f )
			{
				m_NextThunderTimer -= Time.deltaTime;
				if (m_NextThunderTimer < 0f )
				{
					CoroutinesManager.Start(ThunderCoroutine(m_RainIntensity > 0.2f ), "RainManager::UpdateThunderbolts: New thunderbolt" );

					m_NextThunderTimer = Random.Range
					(
						(m_ThunderboltsSectionData.ThunderTimerMin * 0.5f ) + (m_ThunderboltsSectionData.ThunderTimerMin * ( 1f - m_RainIntensity ) ),
						(m_ThunderboltsSectionData.ThunderTimerMax * 0.5f ) + (m_ThunderboltsSectionData.ThunderTimerMax * ( 1f - m_RainIntensity ) )
					);
				}
			}
			m_ThunderAudioContainer.position = m_Camera.transform.position;
		}


		//////////////////////////////////////////////////////////////////////////
		// RainFallEmissionRate
		private float			RainFallEmissionRate()
		{
			return m_RainFallParticleSystem.main.maxParticles / m_RainFallParticleSystem.main.startLifetime.constant * m_RainIntensity;
		}


		//////////////////////////////////////////////////////////////////////////
		// UNITY
		private void			Update()
		{
			if (m_IsFullyLoaded == false )
				return;

			m_Camera = Camera.main;
			/*

#if UNITY_EDITOR
			if ( UnityEditor.EditorApplication.isPlaying == false )
				if ( UnityEditor.SceneView.lastActiveSceneView != null )
					this.m_Camera = UnityEditor.SceneView.lastActiveSceneView.camera;
#endif
			if (this.m_Camera == null )
			{
				this.m_Camera = Camera.main;
				if (this.m_Camera == null )
					//					m_Camera = Camera.main;
					//				if ( m_Camera == null )
					this.m_Camera = CameraControl.Instance?.MainCamera;
				if (this.m_Camera == null )
					return;
			}
			*/
#if UNITY_EDITOR
			if ( UnityEditor.EditorApplication.isPlaying == false && EnableInEditor == false )
			{
				return;
			}
#endif
			if (m_Camera)
			{
				UpdateRainPosition();
				UpdateThunderbols();
			}
			CheckForRainChange();
			m_RainIntensityEvent.setValue(m_RainIntensity );
		}

		#endregion

	}

}