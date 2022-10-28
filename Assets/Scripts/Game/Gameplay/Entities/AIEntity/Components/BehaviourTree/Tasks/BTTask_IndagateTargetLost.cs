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
		protected override EBTNodeState OnUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			EBTNodeState OutState = EBTNodeState.FAILED;
			if (TryGetKeyData(InThisNodeInstanceData, out Vector3? targetPosition, out Vector3? targetDirection))
			{
				InThisNodeInstanceData.BehaviourTreeInstanceData.Controller.RequestMoveTo(targetPosition.Value);
				OutState = EBTNodeState.RUNNING;

				if (InThisNodeInstanceData.BehaviourTreeInstanceData.Controller.IsCloseEnoughTo(targetPosition.Value))
				{
					InThisNodeInstanceData.BehaviourTreeInstanceData.Controller.Stop(bImmediately: false);
					OutState = EBTNodeState.FAILED; // ??? i have to solve this
				}
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnTerminateSuccess(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnTerminateSuccess(InThisNodeInstanceData);

			InThisNodeInstanceData.BehaviourTreeInstanceData.Controller.Stop(bImmediately: false);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnTerminateFailure(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnTerminateFailure(InThisNodeInstanceData);

			InThisNodeInstanceData.BehaviourTreeInstanceData.Controller.Stop(bImmediately: true);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeAbort(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnNodeAbort(InThisNodeInstanceData);

			InThisNodeInstanceData.BehaviourTreeInstanceData.Controller.Stop(bImmediately: true);
		}
	}
}
