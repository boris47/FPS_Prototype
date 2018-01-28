using UnityEngine;
using System.Collections;

public class WindManager : MonoBehaviour {

	public	static	WindManager		Instance						= null;

	[ SerializeField ]
	private		AudioClip			m_AudioClip						= null;

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

	[Tooltip("Wheather wind should be enabled.")]
	[SerializeField]
	private		bool				m_EnableWind					= true;


	private		WindZone			m_WindZone						= null;
	private		LoopingAudioSource	m_AudioSourceWind				= null;
	private		IEnumerator			m_WindUpdateCoroutine			= null;


	private void OnEnable()
	{
		m_WindZone = GetComponent<WindZone>();
		if ( m_WindZone == null || m_AudioClip == null )
			return;
		

		{
			AudioSource audioSource = GetComponent<AudioSource>();
			audioSource.clip = m_AudioClip;

			m_AudioSourceWind = new LoopingAudioSource();
			m_AudioSourceWind.AudioSource = audioSource;
			m_AudioSourceWind.Silence();
		}

	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateWind
	private void	UpdateWind()
	{
		if ( m_WindZone == null )
			return;

		if ( m_EnableWind && m_WindSpeedRange.y > 1.0f )
		{
//			m_WindZone.transform.position = m_Camera.transform.position;
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
			m_AudioSourceWind.SetVolume( ( m_WindZone.windMain / m_WindSpeedRange.z ) * m_WindSoundVolumeModifier );
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


	public void Update()
	{
		UpdateWind();
		m_AudioSourceWind.Update();
	}

}
