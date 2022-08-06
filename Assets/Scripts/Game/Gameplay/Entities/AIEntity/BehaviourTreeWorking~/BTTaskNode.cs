using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public abstract class BTTaskNode : BTNode
	{
		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdateAborting() => EBTNodeState.ABORTED;
	}
}
