﻿
using System;
using UnityEngine;


namespace QuestSystem {

	[RequireComponent(typeof(Collider), typeof(Entity))]
	public class Objective_Destroy : Objective_Base {

		private	Entity				m_Target						= null;


		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		public		override	bool		Initialize()
		{
			if ( m_IsInitialized == true )
				return true;

			m_IsInitialized = true;

			m_Target = GetComponent<Entity>();
			m_Target.OnEvent_Killed += OnKill;

			return m_IsInitialized;
		}


		//////////////////////////////////////////////////////////////////////////
		// ReInit ( IStateDefiner )
		public		override	bool		ReInit()
		{
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// Finalize ( IStateDefiner )
		public		override	bool		Finalize()
		{
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// Activate ( IObjective )
		public		override	void		Activate()
		{
			UI.Instance.Indicators.EnableIndicator( m_Target.gameObject, IndicatorType.TARGET_TO_KILL );

			m_IsCurrentlyActive = true;
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		public		override	void		Deactivate()
		{
			m_IsCurrentlyActive = false;

			UI.Instance.Indicators.DisableIndicator( gameObject );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnKill
		private void OnKill()
		{
			Deactivate();

			OnObjectiveCompleted();
		}

	}

}
