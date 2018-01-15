
using UnityEngine;

namespace QuestSystem {

	public class Objective : MonoBehaviour {

		[SerializeField]
		private string	m_ObjectiveName = "";

		public	bool	Completed
		{
			get;
			private set;
		}

		public	Task Task
		{	set;
			private get;
		}


		private	Interactable m_Interactable = null;

		private void Start()
		{
			GameObject objectToInteract = GameObject.Find( m_ObjectiveName );
			if ( m_ObjectiveName == null || m_ObjectiveName.Length == 0 || objectToInteract == null )
			{
				return;
			}

			Interactable m_Interactable = objectToInteract.GetComponent<Interactable>();
			if ( m_Interactable != null )
			{
				m_Interactable.SetObjective( this );
			}
		}

		public	void	OnInteraction()
		{
			m_Interactable.OnInteraction();
		}

		public	void	Enable()
		{
			GetComponent<Interactable>().CanInteract = true;
		}


		public	void	SetAsCompleted()
		{
			Completed = true;
			Task.CheckStatus();
		}

	}

}