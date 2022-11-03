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
		private				BlackboardEntryKey								m_InputEventKey										= null;

		[SerializeReference, ToNodeInspector, BlackboardKeyType(typeof(BBEntry_Entity))]
		private				BlackboardEntryKey								m_OutputEventKey									= null;


		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeInitializationResult OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			EBTNodeInitializationResult OutState = EBTNodeInitializationResult.FAILED;
			if (ChildAsset.IsNotNull())
			{
				BlackboardInstanceData bbInstanceData = InThisNodeInstanceData.BehaviourTreeInstanceData.BlackboardInstanceData;
				if (bbInstanceData.TryGetEntry(m_InputEventKey, out BBEntry_SightEvent sightEventEntry))
				{
					if (sightEventEntry.Value.TargetInfoType != Senses.ESightTargetEventType.LOST)
					{
						bbInstanceData.SetEntryValue<BBEntry_Entity, Entity>(m_OutputEventKey, sightEventEntry.Value.EntitySeen);
						OutState = EBTNodeInitializationResult.RUNNING;
					}
				}
			}
			return OutState;
		}


		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeAbort(in BTNodeInstanceData InThisNodeInstanceData)
		{
			BlackboardInstanceData bbInstanceData = InThisNodeInstanceData.BehaviourTreeInstanceData.BlackboardInstanceData;
			bbInstanceData.RemoveEntry(m_OutputEventKey, false);

			base.OnNodeAbort(InThisNodeInstanceData);
		}
	}
}
