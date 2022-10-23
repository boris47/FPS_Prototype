
using System.Collections.Generic;
using UnityEngine;

// TODO Proper handling of ontargetlost (target is destroyed or disabled)
namespace Entities.AI.Components.Senses
{
	[System.Serializable]
	public class SightEvent : SenseEvent
	{
		public override ESenses						SenseType						=> ESenses.SIGHT;
		public readonly ESightTargetEventType		TargetInfoType					= default;
		public readonly Vector3						SeenPosition					= Vector3.zero;
		public readonly Vector3						LastDirection					= Vector3.zero;
		public readonly Vector3						ViewerPosition					= Vector3.zero;
		public readonly Entity						EntitySeen						= null;

		public static SightEvent TargetAcquiredEvent(in Entity entitySeen, in Vector3 seenPosition, in Vector3 viewerPosition)
			=> new SightEvent(ESightTargetEventType.ACQUIRED, entitySeen, seenPosition, viewerPosition);
		public static SightEvent TargetChangedEvent	(in Entity entitySeen, in Vector3 seenPosition, in Vector3 viewerPosition)
			=> new SightEvent(ESightTargetEventType.CHANGED, entitySeen, seenPosition, viewerPosition);
		public static SightEvent TargetLostEvent	(in Entity lostTarget, in Vector3 lastSeenPosition, in Vector3 lastDirection, in Vector3 viewerPosition)
			=> new SightEvent(ESightTargetEventType.LOST, lostTarget, lastSeenPosition, lastDirection, viewerPosition);

		private SightEvent(in ESightTargetEventType targetInfoType, in Entity entitySeen, in Vector3 seenPosition, in Vector3 viewerPosition)
		{
			EntitySeen = entitySeen;
			TargetInfoType = targetInfoType;
			SeenPosition = seenPosition;
			ViewerPosition = viewerPosition;
			Utils.CustomAssertions.IsTrue(targetInfoType != ESightTargetEventType.LOST && EntitySeen.IsNotNull());
		}

		private SightEvent(in ESightTargetEventType targetInfoType, in Entity lostTarget, in Vector3 seenPosition, in Vector3 lastDirection, in Vector3 viewerPosition)
		{
			EntitySeen = lostTarget;
			TargetInfoType = targetInfoType;
			SeenPosition = seenPosition;
			ViewerPosition = viewerPosition;
			LastDirection = lastDirection;
		}

		public (Entity EntitySeen, Vector3 SeenPosition, Vector3 ViewerPosition) AsTargetAcquiredEvent() => (EntitySeen, SeenPosition, ViewerPosition);
		public (Entity EntitySeen, Vector3 SeenPosition, Vector3 ViewerPosition) AsTargetChangedEvent() => (EntitySeen, SeenPosition, ViewerPosition);
		public (Entity LostTarget, Vector3 SeenPosition, Vector3 LastDirection, Vector3 ViewerPosition) AsTargetLostEvent() => (EntitySeen, SeenPosition, LastDirection, ViewerPosition);
	}
}
