
using UnityEngine;

namespace QuestSystem {

	public class QuestEntity : Interactable {
		
		private QuestSystem.Objective objective;

		private void Start()
		{
			CanInteract = false;
		}


		public override void OnInteraction()
		{
			objective.SetAsCompleted();

			/// DO THINGS
		}

	}

}