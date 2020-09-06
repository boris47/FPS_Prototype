using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Waiter_Timer : Waiter_Base {

	[SerializeField]
	private		float	m_TimeToWait	= 1.0f;

	[SerializeField]
	private		bool	m_ScaledTime	= true;

	[SerializeField, ReadOnly]
	private		float	m_CurrentTime	= 0.0f;


	public override void Wait()
	{
		if (this.m_CurrentTime < this.m_TimeToWait )
		{
			this.m_CurrentTime += (this.m_ScaledTime ? Time.deltaTime : Time.fixedUnscaledDeltaTime );
		}
		this.m_HasToWait = this.m_CurrentTime < this.m_TimeToWait;
	}
}