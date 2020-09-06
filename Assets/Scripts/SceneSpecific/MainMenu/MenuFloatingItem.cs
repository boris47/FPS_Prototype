using UnityEngine;
using System.Collections;

public class MenuFloatingItem : MonoBehaviour {

	private	float	m_Speed = 1f;

	private void Awake()
	{
		this.transform.rotation = Random.rotation;
		this.m_Speed = Random.value * 3f + 0.1f;
	}

	private void Update()
	{
		this.transform.Rotate( Vector3.up, this.m_Speed * Time.deltaTime, Space.Self );
	}

}
