
using UnityEngine;

namespace QuestSystem {

	public class Task : MonoBehaviour {
		
		[SerializeField]
		private	Interactable	m_Interactalbe = null;
		public	Interactable	Interactable
		{
			get { return m_Interactalbe; }
		}

		public Quest	RelatedQuest { get; set; }

		public Task	NextTask = null;

		public bool	Completed { get; set; }
		
	}

}
