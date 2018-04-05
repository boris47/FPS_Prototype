﻿
using UnityEngine;

namespace AI_Behaviours {

	public class Behaviour_Normal : Behaviour_Base {


		public Behaviour_Normal( Brain brain, MonoBehaviour monoBehaviour )
		{
			this.m_Brain = brain;
			this.m_MonoBehaviour = monoBehaviour;
		}

		public override void OnEnable()
		{
			if ( m_MonoBehaviour != null )
				m_MonoBehaviour.enabled = true;
		}

		public override void OnDisable()
		{
			if ( m_MonoBehaviour != null )
				m_MonoBehaviour.enabled = true;
		}

		public override void OnFrame( float deltaTime )
		{
			
		}


		public override void OnThink()
		{
			
		}

	}

}
