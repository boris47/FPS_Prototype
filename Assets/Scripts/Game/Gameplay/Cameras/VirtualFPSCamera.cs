
using UnityEngine;
using Cinemachine;

[ExecuteInEditMode]
public class VirtualFPSCamera : GameCameraBase
{
	[SerializeField, Range(0.1f, 180f)]
	private			float									m_FieldOfViewRef					= 80f;

	[SerializeField, ReadOnly]
	private			CinemachineVirtualCamera				m_CinemachineVirtualCamera			= null;

	[SerializeField, ReadOnly]
	private			CinemachineBasicMultiChannelPerlin		m_BasicMultiChannelPerlin			= null;


	//////////////////////////////////////////////////////////////////////////
	protected override void Awake()
	{
		base.Awake();

		if (Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_CinemachineVirtualCamera)))
		{
			m_BasicMultiChannelPerlin = m_CinemachineVirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;

			if (m_BasicMultiChannelPerlin == null)
			{
				m_BasicMultiChannelPerlin = m_CinemachineVirtualCamera.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
			}

			m_CinemachineVirtualCamera.m_Lens.FieldOfView = m_FieldOfViewRef;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnValidate()
	{
		if (m_BasicMultiChannelPerlin == null && gameObject.TryGetComponent(out m_CinemachineVirtualCamera))
		{
			m_BasicMultiChannelPerlin = m_CinemachineVirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;

			m_CinemachineVirtualCamera.m_Lens.FieldOfView = m_FieldOfViewRef;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void Update()
	{
		if (m_CinemachineVirtualCamera.IsNotNull() && m_BasicMultiChannelPerlin.IsNotNull())
		{
			float fieldOfView = m_CinemachineVirtualCamera.m_Lens.FieldOfView;
		//	float amplitudeGain = m_BasicMultiChannelPerlin.m_AmplitudeGain;
		//	float frequencyGain = m_BasicMultiChannelPerlin.m_FrequencyGain;

			// The lower fov the lower gains
			float normalized = Mathf.Pow(fieldOfView / m_FieldOfViewRef, 1.5f);
			m_BasicMultiChannelPerlin.m_AmplitudeGain = normalized;
		//	m_BasicMultiChannelPerlin.m_FrequencyGain = normalized;
		}
	}
}