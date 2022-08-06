using UnityEngine;


public class AroundReferenceObjectRotator : MonoBehaviour
{
	[SerializeField]
	private			float			m_RoundsPerMinute			= 1f;

	[SerializeField]
	private			Vector3			m_RotationAxis				= Vector3.up;

	[SerializeField]
	private			Transform		m_Reference					= null;


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		enabled = Utils.CustomAssertions.IsNotNull(m_Reference);
	}


	//////////////////////////////////////////////////////////////////////////
	private void Update()
	{
		// Ref: https://answers.unity.com/questions/1287465/converting-a-rotation-spin-speed-to-rpm.html
		transform.RotateAround(m_Reference.position, m_RotationAxis, 60f / m_RoundsPerMinute * Time.deltaTime);
	}
}
