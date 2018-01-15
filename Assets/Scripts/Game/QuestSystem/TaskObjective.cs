
using UnityEngine;

namespace QuestSystem {

	public class TaskObjective : Interactable {

		public GameEvent	m_OnInteraction = null;

		private	bool	m_Completed = false;

		//////////////////////////////////////////////////////////////////////////
		// OnInteraction ( Override )
		public override void	OnInteraction()
		{
			if ( m_Completed == true )
				return;

			if ( m_OnInteraction != null )
				m_OnInteraction.Invoke();

			GlobalQuestManager.Instance.CurrentLocalQuestManager.OnTaskCompleted( this );

			m_Completed = true;
		}
		
	}

}