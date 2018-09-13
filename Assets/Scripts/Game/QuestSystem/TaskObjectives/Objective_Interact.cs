
using UnityEngine;

namespace QuestSystem {

	public class Objective_Interact : Objective_Base, IInteractable {

		private	Rigidbody	m_RigidBody		= null;
		public	Rigidbody	RigidBody		{ get { return m_RigidBody; } }

		private	Collider	m_Collider		= null;
		public	Collider	Collider		{ get { return m_Collider; } }

		[SerializeField]
		private GameEvent			m_OnInteraction					= null;

		[SerializeField]
		private bool				m_Interactable				  = false;
		bool						IInteractable.CanInteract		{ get { return m_Interactable; } set { m_Interactable = value; } }


		//////////////////////////////////////////////////////////////////////////
		// Awake
		private void Awake()
		{
			m_RigidBody = GetComponent<Rigidbody>();
			m_Collider	= GetComponent<Collider>();
		}

		//////////////////////////////////////////////////////////////////////////
		// Enable ( Override )
		public override void Enable()
		{
			m_Interactable = true;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnInteraction
		public void	OnInteraction()
		{
			if ( Completed == true )
				return;

			if ( m_OnInteraction != null && m_OnInteraction.GetPersistentEventCount() > 0 )
				m_OnInteraction.Invoke();
			
///			print( "Task Objective completed: " + name );

			Completed = true;
			RelatedTask.UpdateStatus();
		}

	}

}