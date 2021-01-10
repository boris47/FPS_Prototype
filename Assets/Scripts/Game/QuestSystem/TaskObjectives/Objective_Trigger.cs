
using UnityEngine;


namespace QuestSystem {

	[RequireComponent(typeof(Collider))]
	public class Objective_Trigger : Objective_Base
	{
		private	Collider			m_Collider						= null;


		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		protected		override	bool		InitializeInternal( Task motherTask, System.Action<Objective_Base> onCompletionCallback, System.Action<Objective_Base> onFailureCallback )
		{
			if (m_IsInitialized == true )
				return true;

			m_IsInitialized = true;

			bool bIsGoodResult = Utils.Base.SearchComponent(gameObject, out m_Collider, ESearchContext.LOCAL );
			if ( bIsGoodResult )
			{
				m_Collider.isTrigger = true;
				m_Collider.enabled = false;

				m_OnCompletionCallback = onCompletionCallback;
				m_OnFailureCallback = onFailureCallback;
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
		protected		override	void		ActivateInternal()
		{
			m_Collider.enabled = true;

			UIManager.Indicators.EnableIndicator(gameObject, EIndicatorType.AREA_TO_REACH, bMustBeClamped: true );
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		protected		override	void		DeactivateInternal()
		{
			m_Collider.enabled = false;

			UIManager.Indicators.DisableIndicator(gameObject );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTriggerEnter
		private void OnTriggerEnter( Collider other )
		{
			if (m_ObjectiveState != EObjectiveState.ACTIVATED )
				return;

			if ( other.transform.root.GetInstanceID() != Player.Instance.transform.root.GetInstanceID() )
				return;

			// Require dependencies to be completed
			if (m_Dependencies.Count > 0 && m_Dependencies.FindIndex( o => o.IsCompleted == false ) > -1 )
				return;

			Deactivate();

			OnObjectiveCompleted();
		}

		private void OnDrawGizmos()
		{
			if (transform.SearchComponent( ref m_Collider, ESearchContext.LOCAL ) )
			{
				Matrix4x4 mat = Gizmos.matrix;
				Gizmos.matrix = transform.localToWorldMatrix;

				Color prevColor = Gizmos.color;
				Color color = Color.grey - new Color( 0f, 0f, 0f, 0.3f );

				if (m_Collider is BoxCollider m_BoxCollider )
				{
					Gizmos.color = color;
					Gizmos.DrawCube( Vector3.zero, m_BoxCollider.size );
				}
		
				if (m_Collider is SphereCollider m_SphereCollider)
				{
					Gizmos.color = color;
					Gizmos.DrawSphere( Vector3.zero, m_SphereCollider.radius );
				}

				Gizmos.color = prevColor;
				Gizmos.matrix = mat;
			}
		}
	}

}
