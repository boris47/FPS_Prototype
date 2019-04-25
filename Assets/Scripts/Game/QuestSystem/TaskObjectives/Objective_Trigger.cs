﻿
using UnityEngine;


namespace QuestSystem {

	[RequireComponent(typeof(Collider))]
	public class Objective_Trigger : Objective_Base {

		private	Collider			m_Collider						= null;


		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		public		override	bool		Initialize()
		{
			if ( m_IsInitialized == true )
				return true;

			m_IsInitialized = true;

			bool result = false;

			m_Collider = GetComponent<Collider>();
			if ( m_Collider )
			{
				m_Collider.isTrigger = true;
				m_Collider.enabled = false;
				result = true;
			}

			return result;
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
			m_Collider.enabled = true;

			m_IsCurrentlyActive = true;

			UI.Instance.Indicators.EnableIndicator( m_Collider.gameObject, IndicatorType.AREA_TO_REACH );
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		public		override	void		Deactivate()
		{
			m_Collider.enabled = false;

			m_IsCurrentlyActive = false;

			UI.Instance.Indicators.DisableIndicator( gameObject );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTriggerEnter
		private void OnTriggerEnter( Collider other )
		{
			if ( m_IsCurrentlyActive == false )
				return;

			if ( other != Player.Entity.PhysicCollider )
				return;

			Deactivate();

			OnObjectiveCompleted();
		}

	}

}
