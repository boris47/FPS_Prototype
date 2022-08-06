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
		protected override EBTNodeState OnActivation()
		{
			Debug.Log(message);

			return EBTNodeState.SUCCEEDED;
		}
	}
}
