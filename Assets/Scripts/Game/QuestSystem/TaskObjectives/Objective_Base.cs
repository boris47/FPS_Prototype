

using UnityEngine;

namespace QuestSystem {

	public abstract class Objective_Base : MonoBehaviour {

	
		public	Task					RelatedTask			{ protected get; set; }
		public	bool					Completed			{ get; protected set; }

		public	abstract	void		Enable();

	}

}