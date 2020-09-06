using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPositionViewer : MonoBehaviour {

	[SerializeField, ReadOnly]
	private	Vector3			m_WorldPosition = Vector3.zero;

	private void OnValidate()
	{
		this.m_WorldPosition = this.transform.position;
	}

	private void OnEnable()
	{
		this.m_WorldPosition = this.transform.position;
	}

	private void OnDisable()
	{
		this.m_WorldPosition = this.transform.position;
	}

	private void Awake()
	{
		this.m_WorldPosition = this.transform.position;
	}

	
	void Start ()
	{
		this.m_WorldPosition = this.transform.position;
	}
	
}
