using System.Collections;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	/// <summary>
	/// Repeater composite node. <br/>
	/// Repeater Nodes repeats their own child execution n-times ignoring the result. <br/>
	/// </summary>
	[BTNodeDetails("Sight to Target Entity", "Repeats its child execution n-times ignoring the result")]
	public class BTDecorator_SightEventToTargetEntity : BTDecoratorNode
	{
		[SerializeReference, ToNodeInspector, BlackboardKeyType(typeof(BBEntry_SightEvent))]
		private				BlackboardEntryKey								m_BlackboardEventKey									= null;

		[SerializeReference, ToNodeInspector, BlackboardKeyType(typeof(BBEntry_Entity))]
		private				BlackboardEntryKey								m_TargetBlackboardKey									= null;


		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			EBTNodeState OutState = EBTNodeState.FAILED;
			if (Child.IsNotNull())
			{
				BlackboardInstanceData bbInstanceData = InThisNodeInstanceData.BehaviourTreeInstanceData.BlackboardInstanceData;
				if (bbInstanceData.TryGetEntry(m_BlackboardEventKey, out BBEntry_SightEvent sightEventEntry))
				{
					if (sightEventEntry.Value.TargetInfoType != Senses.ESightTargetEventType.LOST)
					{
						bbInstanceData.SetEntryValue<BBEntry_Entity, Entity>(m_TargetBlackboardKey, sightEventEntry.Value.EntitySeen);
						OutState = EBTNodeState.RUNNING;
					}
				}
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeAbort(in BTNodeInstanceData InThisNodeInstanceData)
		{
			BlackboardInstanceData bbInstanceData = InThisNodeInstanceData.BehaviourTreeInstanceData.BlackboardInstanceData;
			bbInstanceData.RemoveEntry(m_TargetBlackboardKey);

			base.OnNodeAbort(InThisNodeInstanceData);
		}
	}
}
