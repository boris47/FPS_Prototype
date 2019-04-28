
using UnityEngine;


namespace QuestSystem {

	[RequireComponent(typeof(Collider))]
	public class Objective_LeaveObjectInTrigger : Objective_Base {

		[SerializeField]
		private	Collider			m_ObjectThatTrigger				= null;

		private	Collider			m_Collider						= null;



		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		public		override	bool		Initialize( ITask motherTask )
		{
			if ( m_IsInitialized == true )
				return true;

			m_IsInitialized = true;

			bool bIsGoodResult = Utils.Base.SearchComponent( gameObject, ref m_Collider, SearchContext.LOCAL );
			if ( bIsGoodResult )
			{
				m_Collider.isTrigger = true;
				m_Collider.enabled = false;
				
				motherTask.AddObjective( this );
			}

			return bIsGoodResult;
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
		// OnSave
		public override void OnSave( StreamUnit streamUnit )
		{
			
		}


		//////////////////////////////////////////////////////////////////////////
		// OnLoad
		public override void OnLoad( StreamUnit streamUnit )
		{
			
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

			Deactivate();

			// Require dependencies to be completed
			int dependencyIndex = m_Dependencies.Count > 0 ? m_Dependencies.FindLastIndex( o => o.IsCompleted == false ) : -1;
			if ( dependencyIndex > -1 )
			{
				m_Dependencies[dependencyIndex].Activate();
				return;
			}

			OnObjectiveCompleted();
		}

	}

}
