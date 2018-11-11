using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CutScene {

	public class Cutscene_Waiter_Timer : Cutscene_Waiter_Base {

		[SerializeField]
		private		float	m_TimeToWait	= 1.0f;

		[SerializeField]
		private		bool	m_ScaledTime	= true;

		private		float	m_CurrentTime	= 0.0f;


		public override void Wait()
		{
			if ( m_CurrentTime < m_TimeToWait )
			{
				m_CurrentTime += ( m_ScaledTime ? Time.deltaTime : Time.fixedUnscaledDeltaTime );
			}
			m_HasToWait = m_CurrentTime < m_TimeToWait;
		}
	}

}