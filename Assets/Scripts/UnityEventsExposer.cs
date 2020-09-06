using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityEventsExposer : MonoBehaviour
{

	public	GameEvent			m_OnAwake				= null;
	public	GameEvent			m_OnEnable				= null;
	public	GameEvent			m_OnStart				= null;
	public	GameEvent			m_OnDisable				= null;
	public	GameEvent			m_OnDestroy				= null;

	// Awake is called when the script instance is being loaded
	private void Awake()
	{
		this.m_OnAwake?.Invoke();
	}

	// This function is called when the object becomes enabled and active
	private void OnEnable()
	{
		this.m_OnEnable?.Invoke();
	}

	// Start is called just before any of the Update methods is called the first time
	private void Start()
	{
		this.m_OnStart?.Invoke();
	}

	// This function is called when the behaviour becomes disabled or inactive
	private void OnDisable()
	{
		this.m_OnDestroy?.Invoke();
	}

	// This function is called when the MonoBehaviour will be destroyed
	private void OnDestroy()
	{
		this.m_OnDestroy?.Invoke();
	}

}
