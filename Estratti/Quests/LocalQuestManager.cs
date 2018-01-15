
using UnityEngine;

namespace QuestSystem {

	public class LocalQuestManager : MonoBehaviour {

		private	static	LocalQuestManager Instance = null;

		[SerializeField]
		private	Quest[]			m_Quests	= null;


		// Use this for initialization
		void Start()
		{
			if ( Instance != null )
			{
				Destroy( gameObject );
				return;
			}

			m_Quests = FindObjectsOfType<Quest>();
		}

		
	


	}

}