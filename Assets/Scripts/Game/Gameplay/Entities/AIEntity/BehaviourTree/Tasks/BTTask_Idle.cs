using System.Collections;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Idle Action", "This node will keep the tree on idle state")]
	public class BTTask_Idle : BTTaskNode
	{
		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime) => EBTNodeState.RUNNING;

	}
}
