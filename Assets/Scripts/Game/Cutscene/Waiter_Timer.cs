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
		if (m_CurrentTime < m_TimeToWait )
		{
			m_CurrentTime += (m_ScaledTime ? Time.deltaTime : Time.fixedUnscaledDeltaTime );
		}
		m_HasToWait = m_CurrentTime < m_TimeToWait;
	}
}