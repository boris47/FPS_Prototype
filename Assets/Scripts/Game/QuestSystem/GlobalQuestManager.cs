
using UnityEngine;

namespace QuestSystem {

	public class GlobalQuestManager : MonoBehaviour {

		public static GlobalQuestManager	Instance						= null;

		private	LocalQuestMnanager			m_currentLocalQuestMnanager		= null;
		public	LocalQuestMnanager			CurrentLocalQuestManager
		{
			get { return m_currentLocalQuestMnanager; }
		}


		//////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void Awake()
		{
			Instance = this;

			m_currentLocalQuestMnanager = FindObjectOfType<LocalQuestMnanager>();
		}


		//////////////////////////////////////////////////////////////////////////
		// OnLevelWasLoaded
		private void OnLevelWasLoaded( int sceneIdx )
		{
			m_currentLocalQuestMnanager = FindObjectOfType<LocalQuestMnanager>();
		}


		//////////////////////////////////////////////////////////////////////////
		// UpdateInstance
		private void UpdateInstance()
		{
			Instance.m_currentLocalQuestMnanager = FindObjectOfType<LocalQuestMnanager>();
		}

	}

}