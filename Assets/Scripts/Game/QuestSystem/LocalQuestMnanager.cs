
using UnityEngine;

namespace QuestSystem {

	public class LocalQuestMnanager : MonoBehaviour {

//		[SerializeField]
		private	Quest[]		m_Quests			= null;
		



		//////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void Awake()
		{
			m_Quests = GetComponentsInChildren<Quest>();
			m_Quests[0].Activate();
		}


		//////////////////////////////////////////////////////////////////////////
		// UpdateStatus
		public void	UpdateStatus()
		{
			foreach( Quest quest in m_Quests )
			{
				if ( quest.Completed == false )
				{
					quest.Activate();
///					print( "Next Quest is " + quest.name );
					return;
				}
			}

///			if ( result == true )
///				print( "Now u are a Guardion!" );
		}


		public	void	Output()
		{
			print("ciao");
		}

	}

}