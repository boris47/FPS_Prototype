
using UnityEngine;

namespace QuestSystem {

	public class GlobalQuestManager : MonoBehaviour {

		public	static GlobalQuestManager	Instance
		{
			get;
			private set;
		}

		public	LocalQuestManager			CurrentLocalQuestManager
		{
			get;
			private set;
		}


		void Awake()
		{
			// Singleton
			if ( Instance != null )
			{
				Destroy( gameObject );
				return;
			}
			Instance = this;

			DontDestroyOnLoad( this );

		}


		private void OnLevelWasLoaded( int level )
		{
			CurrentLocalQuestManager = FindObjectOfType<LocalQuestManager>();
		}
	
	}

}