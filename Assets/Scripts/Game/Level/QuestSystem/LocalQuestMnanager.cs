
using UnityEngine;

namespace QuestSystem {

	public class LocalQuestMnanager : MonoBehaviour {


		private	Quest[]		m_Quests			= null;
		



		//////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void Awake()
		{
			m_Quests = FindObjectsOfType<Quest>();
			m_Quests[0].Activate();
		}


		//////////////////////////////////////////////////////////////////////////
		// UpdateStatus
		public void	UpdateStatus()
		{
			bool result = true;
			foreach( Quest quest in m_Quests )
			{
				if ( quest.Completed == false )
				{
					quest.Activate();
					print( "Next Quest is " + quest.name );
					result = false;
					return;
				}
			}

			if ( result == true )
				print( "bravo" );
		}


		public	void	Print()
		{
			print( "Now u are a Guardion!" );
		}

	}

}