﻿using UnityEngine;


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
		protected override EBTNodeState OnUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
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
