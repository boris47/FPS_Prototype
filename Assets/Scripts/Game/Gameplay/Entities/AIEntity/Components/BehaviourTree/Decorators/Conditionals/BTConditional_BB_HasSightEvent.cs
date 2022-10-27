using UnityEngine;


namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Conditional for sight event", "If a child is assigned verify if condition is satisfied before activate it")]
	public class BTConditional_BB_HasSightEvent : BTConditional_BBBase
	{
		[SerializeReference, ToNodeInspector, BlackboardKeyType(typeof(BBEntry_SightEvent))]
		private				BlackboardEntryKey								m_BlackboardKey									= null;

		protected override	BlackboardEntryKey								BlackboardKey									=> m_BlackboardKey;

		protected override bool GetEvaluation(in BTNodeInstanceData InInstanceData)
		{
			return false;
		}
	}
}
