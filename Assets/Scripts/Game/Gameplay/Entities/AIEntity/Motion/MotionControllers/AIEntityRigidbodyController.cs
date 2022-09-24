
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components
{
	public class AIEntityRigidbodyController : AIEntityMotionControllerBase
	{
		public override Vector3 Position => transform.position;

		public override Vector3 Destination => m_Destination;

		private Vector3 m_Destination = Vector3.zero;

		//////////////////////////////////////////////////////////////////////////
		public override bool RequireMovementTo(in Vector3 InDestination)
		{
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public override void StopMovement(in bool bImmediately)
		{
			
		}
	}
}
