using System.Collections;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	/// <summary>
	/// Repeater composite node. <br/>
	/// Repeater Nodes repeats their own child execution n-times ignoring the result. <br/>
	/// </summary>
	[BTNodeDetails("Force result", "Repeats its child execution n-times ignoring the result")]
	public class BTDecorator_ForceResult : BTDecoratorNode
	{
		private enum EResult
		{
			FAILED = EBTNodeState.FAILED,
			SUCCEEDED = EBTNodeState.SUCCEEDED,
		}

		[SerializeField, ToNodeInspector]
		private				EResult											m_OutResult										= EResult.SUCCEEDED;


		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			EBTNodeState OutState = base.OnUpdate(InThisNodeInstanceData, InDeltaTime);
			if (BTNode.IsFinished(OutState))
			{
				// Override result
				OutState = (EBTNodeState)m_OutResult;
			}
			return OutState;
		}

	}
}
