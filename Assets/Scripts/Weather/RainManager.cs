
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
			get { return this.m_RainIntensity; }
			set { this.m_RainIntensity = value; }
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

#endregion

#region INITIALIZATION


		//////////////////////////////////////////////////////////////////////////
		// OnEnable
//		private void			OnEnable()
		private IEnumerator Start()
		{
			m_Instance = this;
			this.m_Camera = Camera.current;

			yield return null;

			//	m_RainFallParticleSystem Child
			if (this.transform.SearchComponentInChild( "RainFallParticleSystem", ref this.m_RainFallParticleSystem ) == false ) 
			{
				this.enabled = false;
				yield break; //return;
			}

			yield return null;

			//	m_RainExplosionParticleSystem Child
			if (this.transform.SearchComponentInChild( "RainExplosionParticleSystem", ref this.m_RainExplosionParticleSystem ) == false ) 
			{
				this.enabled = false;
				yield break; //return;
			}

			yield return null;

			// ThunderLight
			if (this.transform.SearchComponentInChild( "ThunderLight", ref this.m_ThunderLight ) == false ) 
			{
				this.enabled = false;
				yield break; //return;
			}

			yield return null;

			// Thunderbolts audio container
			{
				Transform child = this.transform.Find( "AudioSources" );
				if ( child )
					this.m_ThunderAudioContainer = child.Find( "Thunderbolts" );

				if (this.m_ThunderAudioContainer == null )
				{
					this.enabled = false;
					yield break; //return;
				}

				yield return null;

				this.m_ThunderAudioSources = this.m_ThunderAudioContainer.GetComponentsInChildren<CustomAudioSource>();
			}

			yield return null;

			// m_RainFallParticleSystem Setup
			{
				Renderer rainRenderer = null;
				bool bHasRenderer = this.m_RainFallParticleSystem.transform.SearchComponent( ref rainRenderer, ESearchContext.LOCAL );
				if ( bHasRenderer && rainRenderer.sharedMaterial != null )
				{
					this.m_RainMaterial = Resources.Load<Material>( RAIN_MATERIAL );
					this.m_RainMaterial.EnableKeyword( "SOFTPARTICLES_OFF" );
					rainRenderer.material = this.m_RainMaterial;
				}
			}

			yield return null;

			// m_RainExplosionParticleSystem Setup
			{
				Renderer rainRenderer = null;
				bool bHasRenderer = this.m_RainExplosionParticleSystem.transform.SearchComponent( ref rainRenderer, ESearchContext.LOCAL );
				if ( bHasRenderer && rainRenderer.sharedMaterial != null )
				{
					this.m_RainExplosionMaterial = Resources.Load<Material>( RAIN_EXPLOSION_MATERIAL );
					this.m_RainExplosionMaterial.EnableKeyword( "SOFTPARTICLES_OFF" );
					rainRenderer.material = this.m_RainExplosionMaterial;
				}
			}

			yield return null;

			// Get info from settings file
			if ( GlobalManager.Configs != null && !GlobalManager.Configs.GetSection("Thunderbolts", this.m_ThunderboltsSectionData))
			{
				Debug.LogError("Cannot load Thunderbolts Section");
				this.enabled = false;
				yield break;
			}

			yield return null;

			{
				AudioCollection thunderCollection = Resources.Load<AudioCollection>( THUNDERS_DISTANT );
				if (thunderCollection != null)
				{
					this.m_ThundersDistantCollection = thunderCollection.AudioClips;
				}
				else
				{
					Debug.LogError("Cannot load scriptable " + THUNDERS_DISTANT);
					this.enabled = false;
					yield break;
				}
			}
			yield return null;
			{
				AudioCollection thunderCollection = Resources.Load<AudioCollection>( THUNDERS_DURING_RAIN );
				if (thunderCollection != null)
				{
					this.m_ThundersDuringRainCollection = thunderCollection.AudioClips;
				}
				else
				{
					Debug.LogError("Cannot load scriptable " + THUNDERS_DISTANT);
					this.enabled = false;
					yield break;
				}
			}

			this.m_NextThunderTimer = Random.Range(this.m_ThunderboltsSectionData.ThunderTimerMin, this.m_ThunderboltsSectionData.ThunderTimerMax );
/*
#if UNITY_EDITOR
			if ( UnityEditor.EditorApplication.isPlaying == false )
				yield break; //return;
#endif
*/
			this.m_AudioEmitter = this.GetComponent<StudioEventEmitter>();
			this.m_AudioEmitter.EventInstance.getParameter( "RainIntensity", out this.m_RainIntensityEvent );

			this.m_RainFallParticleSystem.Play( withChildren: true );

			this.m_IsFullyLoaded = true;

			yield return null;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnDisable
		private void			OnDisable()
		{
			if (this.m_RainFallParticleSystem )
				this.m_RainFallParticleSystem.Stop( withChildren:true, stopBehavior: ParticleSystemStopBehavior.StopEmittingAndClear );

			m_Instance				= null;
			this.m_RainExplosionMaterial	= null;
			this.m_RainMaterial			= null;
			this.m_RainIntensity			= 0f;
			this.m_Camera				= null;
		}

#endregion

#region INTERNAL METHODS

		//////////////////////////////////////////////////////////////////////////
		// UpdateRain
		private void			UpdateRainPosition()
		{
			// Keep rain particle system above the player
			this.m_RainFallParticleSystem.transform.position = this.m_Camera.transform.position + (Vector3.up * this.m_RainHeight);
		}


		//////////////////////////////////////////////////////////////////////////
		// CheckForRainChange
		private void			CheckForRainChange()
		{
			// Update particle system rate of emission
			{
				ParticleSystem.EmissionModule e = this.m_RainFallParticleSystem.emission;
				if (this.m_RainFallParticleSystem.isPlaying == false )
				{
					this.m_RainFallParticleSystem.Play();
				}
				ParticleSystem.MinMaxCurve rate = e.rateOverTime;
				rate.mode			= ParticleSystemCurveMode.Constant;
				rate.constantMin	= rate.constantMax = this.RainFallEmissionRate();
				e.rateOverTime		= rate;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// GetFreeAudioSource
		private	ICustomAudioSource		GetFreeAudioSource()
		{
			return System.Array.Find(this.m_ThunderAudioSources, source => !source.IsPlaying);
		}


		//////////////////////////////////////////////////////////////////////////
		// ThunderCoroutine ( Coroutine )
		private	IEnumerator		ThunderCoroutine( bool lighting )
		{
			// check fo available audiosource
			ICustomAudioSource thunderAudioSource = this.GetFreeAudioSource();
			if ( thunderAudioSource == null )
				yield break;

			float	thunderLifeTime = Random.Range(this.m_ThunderboltsSectionData.ThunderLifeTimeMin, this.m_ThunderboltsSectionData.ThunderLifeTimeMax );
			float	thunderSteps	= Random.Range(this.m_ThunderboltsSectionData.ThunderStepsMin, this.m_ThunderboltsSectionData.ThunderStepsMax );
			float	thunderLifeStep	= thunderLifeTime / thunderSteps;
			float	currentLifeTime	= 0f;
			bool	lightON			= false;
			
			// Random rotation for thunder light
			Quaternion thunderLightRotation = Quaternion.Euler(this.m_ThunderLight.transform.rotation.eulerAngles + Vector3.up * Random.Range( -360f, 360f ) );
			this.m_ThunderLight.transform.rotation = thunderLightRotation;
			
			if ( lighting == true )
			{
			// Thunder light rotation
			this.m_ThunderLight.transform.rotation = thunderLightRotation;

			// Lighting effect
				float resetExposure = WeatherManager.Cycles.GetSkyExposure();
				while ( currentLifeTime < thunderLifeTime )
				{
					float randomIntensity = Random.Range( 0.06f, 0.2f );

					// thunder light intensity
					this.m_ThunderLight.intensity = lightON ? randomIntensity : 0.001f;

					// Sky color
					WeatherManager.Cycles.SetSkyExposure( resetExposure + (lightON ? resetExposure : 0.0f));

					lightON = !lightON;
					currentLifeTime += thunderLifeStep;
					yield return new WaitForSeconds ( Random.Range( thunderLifeStep, thunderLifeTime - currentLifeTime ) );
				}
				this.m_ThunderLight.intensity = 0.001f;
				WeatherManager.Cycles.SetSkyExposure(resetExposure);
			}

			yield return new WaitForSeconds ( Random.Range( 0.1f, 3f ) );


			AudioClip[] collection = lighting == true ? this.m_ThundersDuringRainCollection : this.m_ThundersDistantCollection;

			// Play Clip
			AudioClip thunderClip =  collection[ Random.Range( 0, collection.Length ) ];
			thunderAudioSource.Clip = thunderClip;

			thunderAudioSource.Pitch	= Random.Range( 1.0f, 1.6f );
			thunderAudioSource.Volume	= Random.Range( 1f, 2f );
			
			Vector3 thunderDirection = this.m_ThunderLight.transform.forward * Random.Range( 15f, 25f );
			thunderAudioSource.Transform.localPosition = thunderDirection;

			thunderAudioSource.Play();
		}


		//////////////////////////////////////////////////////////////////////////
		// UpdateRain
		private void			UpdateThunderbols()
		{
			if (this.m_RainIntensity > 0.1f )
			{
				this.m_NextThunderTimer -= Time.deltaTime;
				if (this.m_NextThunderTimer < 0f )
				{
					CoroutinesManager.Start(this.ThunderCoroutine(this.m_RainIntensity > 0.2f ), "RainManager::UpdateThunderbolts: New thunderbolt" );

					this.m_NextThunderTimer = Random.Range
					(
						(this.m_ThunderboltsSectionData.ThunderTimerMin * 0.5f ) + (this.m_ThunderboltsSectionData.ThunderTimerMin * ( 1f - this.m_RainIntensity ) ),
						(this.m_ThunderboltsSectionData.ThunderTimerMax * 0.5f ) + (this.m_ThunderboltsSectionData.ThunderTimerMax * ( 1f - this.m_RainIntensity ) )
					);
				}
			}
			this.m_ThunderAudioContainer.position = this.m_Camera.transform.position;
		}


		//////////////////////////////////////////////////////////////////////////
		// RainFallEmissionRate
		private float			RainFallEmissionRate()
		{
			return (this.m_RainFallParticleSystem.main.maxParticles / this.m_RainFallParticleSystem.main.startLifetime.constant ) * this.m_RainIntensity;
		}


		//////////////////////////////////////////////////////////////////////////
		// UNITY
		private void			Update()
		{
			if (this.m_IsFullyLoaded == false )
				return;

			this.m_Camera = Camera.main;
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
			if ( UnityEditor.EditorApplication.isPlaying == false && this.EnableInEditor == false )
			{
				return;
			}
#endif
			this.CheckForRainChange();
			this.UpdateRainPosition();
			this.UpdateThunderbols();
			this.m_RainIntensityEvent.setValue(this.m_RainIntensity );
		}

		#endregion

	}

}