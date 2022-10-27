using System.Collections;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	/// <summary>
	/// Repeater composite node. <br/>
	/// Repeater Nodes repeats their own child execution n-times ignoring the result. <br/>
	/// </summary>
	[BTNodeDetails("Sight to Lost Target Entity", "Repeats its child execution n-times ignoring the result")]
	public class BTDecorator_SightEventToTargetLost : BTDecoratorNode
	{
		[SerializeReference, ToNodeInspector, BlackboardKeyType(typeof(BBEntry_SightEvent))]
		private				BlackboardEntryKey								m_InputEventKey										= null;

		[SerializeReference, ToNodeInspector, BlackboardKeyType(typeof(BBEntry_Entity))]
		private				BlackboardEntryKey								m_OutputEventKey									= null;


		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			EBTNodeState OutState = EBTNodeState.FAILED;
			if (Child.IsNotNull())
			{
				BlackboardInstanceData bbInstanceData = InThisNodeInstanceData.BehaviourTreeInstanceData.BlackboardInstanceData;
				if (bbInstanceData.TryGetEntry(m_InputEventKey, out BBEntry_SightEvent sightEventEntry))
				{
					bbInstanceData.SetEntryValue<BBEntry_Entity, Entity>(m_OutputEventKey, sightEventEntry.Value.EntitySeen);
					OutState = EBTNodeState.RUNNING;
				}
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeAbort(in BTNodeInstanceData InThisNodeInstanceData)
		{
			BlackboardInstanceData bbInstanceData = InThisNodeInstanceData.BehaviourTreeInstanceData.BlackboardInstanceData;
			bbInstanceData.RemoveEntry(m_OutputEventKey);

			base.OnNodeAbort(InThisNodeInstanceData);
		}
	}
}
