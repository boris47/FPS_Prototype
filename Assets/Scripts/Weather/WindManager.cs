
using UnityEngine;
using System.Collections;

namespace WeatherSystem
{
	[ExecuteInEditMode]
	public class WindManager : MonoBehaviour
	{
	//	public	static	WindManager		Instance						= null;

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
			get { return m_Target; }
			set
			{
				m_Target = value;
				OnTargetSet ( value == null );
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
			UnityEditor.EditorApplication.update += Update;
#endif

			m_WindZone = transform.Find( "WindZone" ).GetComponent<WindZone>();
			if (m_WindZone == null )
				return;

			// Audio Sources Setup
			//			Transform audioSource = transform.Find( "AudioSources" ).Find( "Wind" );

			m_AudioSourceWind = GetComponent<ICustomAudioSource>();

			//			AudioSource source = audioSource.GetComponent<AudioSource>();
			//			m_AudioSourceWind.AudioSource = source;
			//			SoundEffectManager.Instance.RegisterSource( ref source );
			m_AudioSourceWind.Volume	= 0f;

			m_State1.windMain			= 0f;
			m_State1.windTurbolence		= 0f;
			m_State1.windZoneRotation	= transform.rotation;

			m_State2.windMain			= Random.Range(m_WindSpeedRange.x, m_WindSpeedRange.y );
			m_State2.windTurbolence		= Random.Range(m_WindSpeedRange.x, m_WindSpeedRange.y );
			m_State2.windZoneRotation	= Quaternion.Euler( 0f, Random.Range(0f, 360f), 0f );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnDisable
		private void OnDisable()
		{
			m_AudioSourceWind		= null;
			m_CurrentWindTime		= 0f;
			m_Interpolant			= 0f;

	#if UNITY_EDITOR
			UnityEditor.EditorApplication.update -= Update;
	#endif
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTargetSet
		private void OnTargetSet( bool IsNull )
		{
			if ( IsNull == true )
			{
				m_State2.windMain			= Random.Range(m_WindSpeedRange.x, m_WindSpeedRange.y );
				m_State2.windTurbolence		= Random.Range(m_WindSpeedRange.x, m_WindSpeedRange.y );
				m_State2.windZoneRotation	= Quaternion.Euler( 0f, Random.Range(0f, 360f), 0f );
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// UpdateWind
		private void	UpdateWind()
		{
			if (m_WindZone == null )
				return;

			if (m_EnableWind == true && m_WindSpeedRange.y > 1.0f )
			{

				m_CurrentWindTime += Time.deltaTime;
				m_Interpolant = m_CurrentWindTime / m_NextWindTime;
				if (m_Interpolant  < 1f )
				{
					m_WindZone.windMain				= Mathf.Lerp(m_State1.windMain, m_State2.windMain, m_Interpolant );
					m_WindZone.windTurbulence		= Mathf.Lerp(m_State1.windTurbolence, m_State2.windTurbolence, m_Interpolant );

					if (m_WindZone.windMain > 0.01f )
					{
						if (m_Target != null )
						{
							m_WindZone.transform.LookAt(m_Target );
						}
						else
						{
							m_WindZone.transform.rotation = Quaternion.Lerp(m_State1.windZoneRotation, m_State2.windZoneRotation, m_Interpolant );
						}

					}
				}
				else
				{
					m_NextWindTime				= Random.Range(m_WindChangeInterval.x, m_WindChangeInterval.y );
					m_CurrentWindTime			= 0f;
					m_Interpolant				= 0f;

					// set as current state
					m_State1 = m_State2;

					// Generate next state
					m_State2.windMain			= Random.Range(m_WindSpeedRange.x, m_WindSpeedRange.y );
					m_State2.windTurbolence		= Random.Range(m_WindSpeedRange.x, m_WindSpeedRange.y );
					m_State2.windZoneRotation	= Quaternion.Euler( 0f, Random.Range(0.0f, 360f), 0f );
			
				}

				m_AudioSourceWind.Volume = ( (m_WindZone.windMain / m_WindSpeedRange.z ) * m_WindSoundVolumeModifier );
			}
			else
			{
				m_WindZone.windMain = 0f;
				m_AudioSourceWind.Volume = 0f;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// Update
		private void Update()
		{
			if (EnableInEditor == false )
				return;

			UpdateWind();
		}

	}

}