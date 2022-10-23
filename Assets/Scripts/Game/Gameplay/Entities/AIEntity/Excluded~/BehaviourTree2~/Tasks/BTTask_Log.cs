using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public class BTTask_Log : BTTaskNode
	{
		public override string NodeName		=> "Log Action";
		public override string NodeInfo		=> "Log a message to the console";

		[SerializeField, ToNodeInspector(bShowLabel: true)]
		public string			message				= string.Empty;

		//////////////////////////////////////////////////////////////////////////
		protected override void OnActivation()
		{
			Debug.Log(message);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate() => EBTNodeState.SUCCEEDED;

		//////////////////////////////////////////////////////////////////////////
		protected override void OnTerminate(in bool bIsAbort)
		{
			
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeReset()
		{
			
		}
	}
}
