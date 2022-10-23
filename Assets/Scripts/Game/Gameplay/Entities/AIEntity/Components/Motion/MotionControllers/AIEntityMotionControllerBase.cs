
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components
{
	public abstract class AIEntityMotionControllerBase : AIEntityComponent
	{
		public abstract Vector3 Position { get; }

		public abstract Vector3 Destination { get; }

		public abstract bool RequireMovementTo(in Vector3 InDestination);

		public abstract void StopMovement(in bool bImmediately);
	}
}
