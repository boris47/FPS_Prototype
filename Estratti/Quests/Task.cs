using UnityEngine;
using System.Collections;

namespace QuestSystem {

	public interface ITask_Setter {

		Quest RelatedQuest { set; }

	}

	public interface ITask {

		Quest RelatedQuest { get; }

		bool Completed { get; }

		void CheckStatus();
	}

	public class Task : MonoBehaviour, ITask, ITask_Setter {

		[SerializeField]
		private Objective	m_Objective		= null;

		[SerializeField]
		private string		m_ObjectiveName = "";

		[SerializeField]
		private	GameEvent	m_OnCompletion	= null;


		private	Quest		m_RelatedQuest	= null;
		public	Quest RelatedQuest
		{
			get { return m_RelatedQuest; }
		}
		Quest ITask_Setter.RelatedQuest
		{
			set { m_RelatedQuest = value; }
		}

		public	bool Completed
		{
			get;
			private set;
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
				m_Interactable.SetRelatedTask( this );
			}
		}

		public void	CheckStatus()
		{
			if ( m_Objective.Completed && !this.Completed )
			{
				if ( m_OnCompletion != null )
					m_OnCompletion.Invoke();

				this.Completed = true;
				m_RelatedQuest.NextTask();
			}
		}
	}

}
