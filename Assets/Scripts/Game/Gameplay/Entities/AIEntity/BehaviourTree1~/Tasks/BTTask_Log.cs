using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public class BTTask_Log : BTTaskNode
	{
		public override string NodeName => "Log Action";
		public override string NodeInfo => "Log a message to the console";

		[SerializeField]
		public string message = string.Empty;

		//////////////////////////////////////////////////////////////////////////
		protected override void CopyDataToInstance(in BTNode InNewInstance)
		{
			var node = InNewInstance as BTTask_Log;
			node.message = message;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnAwakeInternal(in BehaviourTree InBehaviourTree)
		{
			
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState Update()
		{
			Debug.Log(message);
			return EBTNodeState.SUCCEEDED;
		}
	}
}
