using UnityEngine;
using System.Collections;

namespace WeatherSystem {

	public class MistManager : MonoBehaviour {


		[Tooltip("The threshold for intensity (0 - 1) at which mist starts to appear")]
		[Range(0.0f, 1.0f), SerializeField]
		private			float			m_RainMistThreshold			= 0.5f;
	


		protected		Material		m_RainMistMaterial			= null;


		private			ParticleSystem	m_RainMistParticleSystem	= null;
//		private			bool			m_IsEnabled					= false;
		private			Camera			m_Camera					= null;



		//////////////////////////////////////////////////////////////////////////
		// OnEnable
		private		void		OnEnable()
		{
			m_Camera = Camera.current;

			//	m_RainMistParticleSystem Child
			{
				Transform child = transform.Find( "RainMistParticleSystem" );;
				if ( child )
					m_RainMistParticleSystem = child.GetComponent<ParticleSystem>();

				if (m_RainMistParticleSystem == null )
				{
					enabled = false;
					return;
				}
			}

			ParticleSystem.EmissionModule e = m_RainMistParticleSystem.emission;
			e.enabled = true;
			ParticleSystem.MinMaxCurve rate = e.rateOverTime;
			rate.mode = ParticleSystemCurveMode.Constant;
			Renderer rainRenderer = m_RainMistParticleSystem.GetComponent<Renderer>();
			rainRenderer.enabled = true;
			m_RainMistMaterial = new Material( rainRenderer.material );
			m_RainMistMaterial.EnableKeyword( "SOFTPARTICLES_ON" );
			rainRenderer.material = m_RainMistMaterial;
			m_RainMistParticleSystem.Play();
		}


		//////////////////////////////////////////////////////////////////////////
		// MistEmissionRate
		private		float		MistEmissionRate()
		{
			return (m_RainMistParticleSystem.main.maxParticles / m_RainMistParticleSystem.main.startLifetime.constant ) * RainManager.Instance.RainIntensity * 2f;
		}


		//////////////////////////////////////////////////////////////////////////
		// UNITY
		private		void		Update()
		{
#if UNITY_EDITOR
			if ( UnityEditor.EditorApplication.isPlaying == false )
				if ( UnityEditor.SceneView.lastActiveSceneView != null )
					m_Camera = UnityEditor.SceneView.lastActiveSceneView.camera;
#endif
//			if ( m_Camera == null )
			{
				//				m_Camera = Camera.current;
				//				if ( m_Camera == null )
				//					m_Camera = Camera.main;
				//				if ( m_Camera == null )
				m_Camera = CameraControl.Instance != null ? CameraControl.Instance.MainCamera : null;
				if (m_Camera == null )
					return;
			}

			m_RainMistParticleSystem.transform.position = m_Camera.transform.position;

			ParticleSystem.EmissionModule e = m_RainMistParticleSystem.emission;
			ParticleSystem.MinMaxCurve rate = e.rateOverTime;
			float emissionRate = RainManager.Instance.RainIntensity > m_RainMistThreshold ? MistEmissionRate() : 0f;
			rate.constantMin = rate.constantMax = emissionRate;
			e.rateOverTime = rate;
		}

	}

}