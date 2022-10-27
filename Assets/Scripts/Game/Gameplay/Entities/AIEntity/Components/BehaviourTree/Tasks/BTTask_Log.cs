using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Log Action", "Log a message to the console")]
	public class BTTask_Log : BTTaskNode
	{
		[SerializeField, ToNodeInspector(bInShowDefaultLabel: true)]
		public string			m_Message				= string.Empty;

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			Debug.Log(m_Message);

			return base.OnUpdate(InThisNodeInstanceData, InDeltaTime);
		}
	}
}
