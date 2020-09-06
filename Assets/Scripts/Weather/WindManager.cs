
using UnityEngine;
using System.Collections;

namespace WeatherSystem {

	[ExecuteInEditMode]
	public class WindManager : MonoBehaviour {

		public	static	WindManager		Instance						= null;

		[SerializeField]
		private	bool					EnableInEditor					= false;

		[Header("Wind Properties")]
		[Tooltip("Wind sound volume modifier, use this to lower your sound if it's too loud.")]
		[SerializeField]
		private		float				m_WindSoundVolumeModifier		= 1.0f;

		[Tooltip("X = minimum wind speed. Y = maximum wind speed. Z = sound multiplier. Wind speed is divided by Z to get sound multiplier value. Set Z to lower than Y to increase wind sound volume, or higher to decrease wind sound volume.")]
		[SerializeField]
		private		Vector3				m_WindSpeedRange				= new Vector3( 0.0f, 30.0f, 500.0f );

		[Tooltip("How often the wind speed and direction changes (minimum and maximum change interval in seconds)")]
		[SerializeField]
		private		Vector2				m_WindChangeInterval			= new Vector2( 5.0f, 30.0f );

		[Tooltip("Wheather wind should be enabled.")]
		[SerializeField]
		private		bool				m_EnableWind					= true;

		private		Transform			m_Target						= null;
		public		Transform			Target
		{
			get { return this.m_Target; }
			set
			{
				this.m_Target = value;
				this.OnTargetSet ( value == null );
			}
		}

		private		struct windData
		{
			public	float		windMain;
			public	float		windTurbolence;
			public	Quaternion	windZoneRotation;
		}

		private		WindZone			m_WindZone						= null;
		private		ICustomAudioSource	m_AudioSourceWind				= null;

		private		float				m_Interpolant					= 0.0f;
		private		float				m_NextWindTime					= 0.0f;
		private		float				m_CurrentWindTime				= 0.0f;
		private		windData			m_State1;
		private		windData			m_State2;



		//////////////////////////////////////////////////////////////////////////
		// OnEnable
		private void OnEnable()
		{
	#if UNITY_EDITOR
			UnityEditor.EditorApplication.update += this.Update;
#endif

			this.m_WindZone = this.transform.Find( "WindZone" ).GetComponent<WindZone>();
			if (this.m_WindZone == null )
				return;

			// Audio Sources Setup
			//			Transform audioSource = transform.Find( "AudioSources" ).Find( "Wind" );

			this.m_AudioSourceWind = this.GetComponent<ICustomAudioSource>();

			//			AudioSource source = audioSource.GetComponent<AudioSource>();
			//			m_AudioSourceWind.AudioSource = source;
			//			SoundEffectManager.Instance.RegisterSource( ref source );
			this.m_AudioSourceWind.Volume	= 0f;

			this.m_State1.windMain			= 0f;
			this.m_State1.windTurbolence		= 0f;
			this.m_State1.windZoneRotation	= this.transform.rotation;

			this.m_State2.windMain			= Random.Range(this.m_WindSpeedRange.x, this.m_WindSpeedRange.y );
			this.m_State2.windTurbolence		= Random.Range(this.m_WindSpeedRange.x, this.m_WindSpeedRange.y );
			this.m_State2.windZoneRotation	= Quaternion.Euler( 0f, Random.Range(0f, 360f), 0f );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnDisable
		private void OnDisable()
		{
			this.m_AudioSourceWind		= null;
			this.m_CurrentWindTime		= 0f;
			this.m_Interpolant			= 0f;

	#if UNITY_EDITOR
			UnityEditor.EditorApplication.update -= this.Update;
	#endif
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTargetSet
		private void OnTargetSet( bool IsNull )
		{
			if ( IsNull == true )
			{
				this.m_State2.windMain			= Random.Range(this.m_WindSpeedRange.x, this.m_WindSpeedRange.y );
				this.m_State2.windTurbolence		= Random.Range(this.m_WindSpeedRange.x, this.m_WindSpeedRange.y );
				this.m_State2.windZoneRotation	= Quaternion.Euler( 0f, Random.Range(0f, 360f), 0f );
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// UpdateWind
		private void	UpdateWind()
		{
			if (this.m_WindZone == null )
				return;

			if (this.m_EnableWind == true && this.m_WindSpeedRange.y > 1.0f )
			{

				this.m_CurrentWindTime += Time.deltaTime;
				this.m_Interpolant = this.m_CurrentWindTime / this.m_NextWindTime;
				if (this.m_Interpolant  < 1f )
				{
					this.m_WindZone.windMain				= Mathf.Lerp(this.m_State1.windMain, this.m_State2.windMain, this.m_Interpolant );
					this.m_WindZone.windTurbulence		= Mathf.Lerp(this.m_State1.windTurbolence, this.m_State2.windTurbolence, this.m_Interpolant );

					if (this.m_WindZone.windMain > 0.01f )
					{
						if (this.m_Target != null )
						{
							this.m_WindZone.transform.LookAt(this.m_Target );
						}
						else
						{
							this.m_WindZone.transform.rotation = Quaternion.Lerp(this.m_State1.windZoneRotation, this.m_State2.windZoneRotation, this.m_Interpolant );
						}

					}
				}
				else
				{
					this.m_NextWindTime				= Random.Range(this.m_WindChangeInterval.x, this.m_WindChangeInterval.y );
					this.m_CurrentWindTime			= 0f;
					this.m_Interpolant				= 0f;

					// set as current state
					this.m_State1 = this.m_State2;

					// Generate next state
					this.m_State2.windMain			= Random.Range(this.m_WindSpeedRange.x, this.m_WindSpeedRange.y );
					this.m_State2.windTurbolence		= Random.Range(this.m_WindSpeedRange.x, this.m_WindSpeedRange.y );
					this.m_State2.windZoneRotation	= Quaternion.Euler( 0f, Random.Range(0.0f, 360f), 0f );
			
				}

				this.m_AudioSourceWind.Volume = ( (this.m_WindZone.windMain / this.m_WindSpeedRange.z ) * this.m_WindSoundVolumeModifier );
			}
			else
			{
				this.m_WindZone.windMain = 0f;
				this.m_AudioSourceWind.Volume = 0f;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// Update
		private void Update()
		{
			if (this.EnableInEditor == false )
				return;

			this.UpdateWind();
		}

	}

}