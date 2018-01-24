using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Sun : MonoBehaviour {
	
	public		float		m_Speed			= 0.05f;

	private		Vector3		m_Rotation		= Vector3.zero;

	/////////////////////////////////////////////////////////////////////////////
	// AWAKE
	private void Awake()
	{
		m_Rotation = new Vector3( m_Speed, 0.0f, 0.0f );
	}

	/////////////////////////////////////////////////////////////////////////////
	// UNITY
	void FixedUpdate()
	{
		transform.Rotate( m_Rotation, Space.Self );
	}
}
