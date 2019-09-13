using UnityEngine;
using System.Collections;

public class MenuFloatingItem : MonoBehaviour {

	private	float	m_Speed = 1f;

	private void Awake()
	{
		transform.rotation = Random.rotation;
		m_Speed = Random.value * 3f + 0.1f;
	}

/*	private void Update()
	{
		transform.Rotate( Vector3.up, m_Speed * Time.deltaTime, Space.Self );
	}
*/
}
