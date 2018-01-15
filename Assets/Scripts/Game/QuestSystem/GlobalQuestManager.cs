
using UnityEngine;

namespace QuestSystem {

	public class GlobalQuestManager : MonoBehaviour {

		public static GlobalQuestManager	Instance = null;

		private	LocalQuestMnanager			m_currentLocalQuestMnanager = null;
		public	LocalQuestMnanager			CurrentLocalQuestManager
		{
			get { return m_currentLocalQuestMnanager; }
		}


		//////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void Awake()
		{
			if ( Instance != null )
			{
				Instance.UpdateInstance();
				Destroy( gameObject );
				return;
			}
			DontDestroyOnLoad( Instance = this );

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