using UnityEngine;


namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("MoveTo Action", "Expecting a position to reach order movement to controller")]
	public class BTTask_MoveTo : BTTaskNode
	{
		protected class RuntimeData : RuntimeDataBase
		{
			public	Vector3	PositionToReach = Vector3.zero;
		}
	
		[SerializeReference, ToNodeInspector, BlackboardKeyType(typeof(BBEntry_Position), typeof(BBEntry_Entity))]
		private				BlackboardEntryKey								m_BlackboardKey									= null;

		//////////////////////////////////////////////////////////////////////////
		protected override RuntimeDataBase CreateRuntimeDataInstance(in BTNodeInstanceData InThisNodeInstanceData) => new RuntimeData();


		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			RuntimeData nodeRuntimeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);

			Vector3? targetPosition = null;

			if (InThisNodeInstanceData.TryGetEntry(m_BlackboardKey, out BBEntry_Position position))
			{
				targetPosition = position.Value;
			}
			else if (InThisNodeInstanceData.TryGetEntry(m_BlackboardKey, out BBEntry_Entity target))
			{
				targetPosition = target.Value.Body.position;
			}

			EBTNodeState outState = EBTNodeState.FAILED;
			if (targetPosition.HasValue)
			{
				nodeRuntimeData.PositionToReach = position.Value;
				InThisNodeInstanceData.BehaviourTreeInstanceData.Controller.RequestMoveTo(nodeRuntimeData.PositionToReach);
				outState = EBTNodeState.RUNNING;

				if (InThisNodeInstanceData.BehaviourTreeInstanceData.Controller.IsCloseEnoughTo(nodeRuntimeData.PositionToReach))
				{
					outState = EBTNodeState.SUCCEEDED;
				}
			}
			return outState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			RuntimeData nodeRuntimeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);

			EBTNodeState OutState = EBTNodeState.RUNNING;
			if (InThisNodeInstanceData.BehaviourTreeInstanceData.Controller.IsCloseEnoughTo(nodeRuntimeData.PositionToReach))
			{
				OutState = EBTNodeState.SUCCEEDED;
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
