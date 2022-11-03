using UnityEngine;


namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Indagate Action", "Expecting a position to reach order movement to controller")]
	public class BTTask_IndagateTargetLost : BTTaskNode
	{
		[SerializeReference, ToNodeInspector, BlackboardKeyType(typeof(BBEntry_SightEvent))]
		private				BlackboardEntryKey								m_BlackboardKey									= null;

		//////////////////////////////////////////////////////////////////////////
		protected bool TryGetKeyData(in BTNodeInstanceData InThisNodeInstanceData, out Vector3? targetPosition, out Vector3? targetDirection)
		{
			targetPosition = null;
			targetDirection = null;
			if (InThisNodeInstanceData.TryGetEntry(m_BlackboardKey, out BBEntry_SightEvent sightEventEntry))
			{
				targetPosition = sightEventEntry.Value.SeenPosition;
				targetDirection = sightEventEntry.Value.LastDirection;
			}
			return targetPosition.HasValue && targetDirection.HasValue;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnNodeUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			EBTNodeState OutState = EBTNodeState.FAILED;
			if (TryGetKeyData(InThisNodeInstanceData, out Vector3? targetPosition, out Vector3? targetDirection))
			{
				InThisNodeInstanceData.BehaviourTreeInstanceData.Controller.RequestMoveTo(targetPosition.Value);
				OutState = EBTNodeState.RUNNING;

				if (InThisNodeInstanceData.BehaviourTreeInstanceData.Controller.IsCloseEnoughTo(targetPosition.Value))
				{
				//	OutState = EBTNodeState.FAILED; // ??? i have to solve this
					InThisNodeInstanceData.BehaviourTreeInstanceData.Controller.Stop(bImmediately: false);

					InThisNodeInstanceData.BehaviourTreeInstanceData.BlackboardInstanceData.RemoveEntry(m_BlackboardKey, false);

					OutState = EBTNodeState.FAILED;
				}
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnTermination(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnTermination(InThisNodeInstanceData);

			bool bImmediately = InThisNodeInstanceData.NodeState == EBTNodeState.FAILED;
			InThisNodeInstanceData.BehaviourTreeInstanceData.Controller.Stop(bImmediately);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeAbort(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnNodeAbort(InThisNodeInstanceData);

			InThisNodeInstanceData.BehaviourTreeInstanceData.Controller.Stop(bImmediately: true);
		}
	}
}
