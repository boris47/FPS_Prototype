using System.Linq;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Blackboard Task")]
	public abstract class BTBlackboardTaskNode : BTTaskNode
	{
		[SerializeField]
		private			BlackboardEntryKey				m_BlackboardKey			= null;

		protected		BlackboardEntryKey				BlackboardKey => m_BlackboardKey;
	}
}
