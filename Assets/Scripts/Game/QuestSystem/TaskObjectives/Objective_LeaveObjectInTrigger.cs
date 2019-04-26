
using UnityEngine;


namespace QuestSystem {

	[RequireComponent(typeof(Collider))]
	public class Objective_LeaveObjectInTrigger : Objective_Base {

		[SerializeField]
		private	Collider			m_ObjectThatTrigger				= null;

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
			if ( m_ObjectThatTrigger.IsNotNull() )
			{
				m_Collider.enabled = true;

				UI.Instance.Indicators.EnableIndicator( m_Collider.gameObject, IndicatorType.AREA_WHERE_PLACE_OBJECT );
			
				m_IsCurrentlyActive = true;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		public		override	void		Deactivate()
		{
			if ( m_ObjectThatTrigger.IsNotNull() )
			{
				m_Collider.enabled = false;

				UI.Instance.Indicators.DisableIndicator( gameObject );
			
				m_IsCurrentlyActive = false;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// SetObjectToTrigger
		public	void	SetObjectToTrigger( Collider objCollider )
		{
			if ( objCollider && objCollider.isTrigger == false )
			{
				m_ObjectThatTrigger = objCollider;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTriggerEnter
		private void OnTriggerEnter( Collider other )
		{
			if ( m_IsCurrentlyActive == false )
				return;

			if ( other.GetInstanceID() != m_ObjectThatTrigger.GetInstanceID() )
				return;

			// Require dependencies to be completed
			if ( m_Dependencies.Count > 0 && m_Dependencies.FindIndex( o => o.IsCompleted == false ) > -1 )
				return;

			Deactivate();

			OnObjectiveCompleted();
		}

	}

}
