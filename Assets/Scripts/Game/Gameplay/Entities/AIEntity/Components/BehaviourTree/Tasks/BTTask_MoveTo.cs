using UnityEngine;


namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("MoveTo Action", "Expecting a position to reach order movement to controller")]
	public class BTTask_MoveTo : BTTaskNode
	{
		[SerializeReference, ToNodeInspector, BlackboardKeyType(typeof(BBEntry_Position), typeof(BBEntry_Entity))]
		private				BlackboardEntryKey								m_BlackboardKey									= null;

		//////////////////////////////////////////////////////////////////////////
		protected bool TryGetKeyData(in BTNodeInstanceData InThisNodeInstanceData, out Vector3? targetPosition)
		{
			targetPosition = null;
			if (InThisNodeInstanceData.TryGetEntry(m_BlackboardKey, out BBEntry_Position position))
			{
				targetPosition = position.Value;
			}
			else if (InThisNodeInstanceData.TryGetEntry(m_BlackboardKey, out BBEntry_Entity target))
			{
				targetPosition = target.Value.Body.position;
			}
			return targetPosition.HasValue;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnNodeUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			EBTNodeState OutState = EBTNodeState.FAILED;
			if (TryGetKeyData(InThisNodeInstanceData, out Vector3? targetPosition))
			{
				InThisNodeInstanceData.BehaviourTreeInstanceData.Controller.RequestMoveTo(targetPosition.Value);
				OutState = EBTNodeState.RUNNING;

				if (InThisNodeInstanceData.BehaviourTreeInstanceData.Controller.IsCloseEnoughTo(targetPosition.Value))
				{
					//OutState = EBTNodeState.SUCCEEDED;
					InThisNodeInstanceData.BehaviourTreeInstanceData.Controller.Stop(bImmediately: false);
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
