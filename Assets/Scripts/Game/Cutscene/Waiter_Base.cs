using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Waiter_Base : MonoBehaviour {

	protected		bool	m_HasToWait		= true;
	public			bool	HasToWait
	{
		get { return this.m_HasToWait; }
	}

	public	abstract	void	Wait();

	private void Awake()
	{
		this.enabled = false;
	}

}