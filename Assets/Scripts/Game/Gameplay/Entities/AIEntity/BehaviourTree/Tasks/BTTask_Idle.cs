using System.Collections;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Idle Action", "This node will keep the tree on idle state")]
	public class BTTask_Idle : BTTaskNode
	{
		//////////////////////////////////////////////////////////////////////////
		protected override void CopyDataToInstance(in BTNode InNewInstance)
		{
			
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnAwakeInternal(in BehaviourTree InBehaviourTree)
		{
			
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnActivation()
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate(in float InDeltaTime) => EBTNodeState.RUNNING;

		//////////////////////////////////////////////////////////////////////////
		protected override void OnTerminate()
		{
		
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeReset()
		{

		}
	}
}
