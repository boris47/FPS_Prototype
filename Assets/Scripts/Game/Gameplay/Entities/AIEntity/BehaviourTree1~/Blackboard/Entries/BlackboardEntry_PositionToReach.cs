using System.Collections;
using UnityEngine;

namespace Entities.AI
{
	[System.Serializable]
	public class BlackboardEntry_PositionToReach : BlackboardEntryKeyValue<Vector3>
	{

		public BlackboardEntry_PositionToReach(in BlackboardEntryKey InBlackboardKey, in OnChangeDel InKeyObservers) : base(InBlackboardKey, InKeyObservers)
		{

		}
	}
}

