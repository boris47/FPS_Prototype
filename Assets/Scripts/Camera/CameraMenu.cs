using UnityEngine;
using System.Collections;

public class CameraMenu : MonoBehaviour {

//	private	Camera	m_CameraComponent	= null;

//	private	float	m_Theta				= 0f;



	private void Awake()
	{
//		m_CameraComponent = GetComponent<Camera>();
	}

	private void Update()
	{
//		m_Theta += Time.deltaTime * 0.5f;
//		m_CameraComponent.fieldOfView = 80f + ( 20f * Mathf.Cos( m_Theta ) );

		transform.Rotate( Vector3.right, Time.deltaTime );
	}

}
