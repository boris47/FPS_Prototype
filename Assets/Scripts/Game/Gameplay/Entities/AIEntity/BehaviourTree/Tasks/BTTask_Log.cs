using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Log Action", "Log a message to the console")]
	public class BTTask_Log : BTTaskNode
	{
		[SerializeField, ToNodeInspector(bShowLabel: true)]
		public string			m_Message				= string.Empty;


		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnActivation()
		{
			Debug.Log(m_Message);

			return EBTNodeState.SUCCEEDED;
		}
	}
}
