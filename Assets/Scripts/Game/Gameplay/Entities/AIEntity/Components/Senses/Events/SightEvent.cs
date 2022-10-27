
using System.Collections.Generic;
using UnityEngine;

// TODO Proper handling of ontargetlost (target is destroyed or disabled)
namespace Entities.AI.Components.Senses
{
	[System.Serializable]
	public class SightEvent : SenseEvent
	{
		[SerializeField, ReadOnly]
		private			ESightTargetEventType		m_TargetInfoType					= default;
		[SerializeField, ReadOnly]
		private			Vector3						m_SeenPosition						= Vector3.zero;
		[SerializeField, ReadOnly]
		private			Vector3						m_LastDirection						= Vector3.zero;
		[SerializeField, ReadOnly]
		private			Vector3						m_ViewerPosition					= Vector3.zero;
		[SerializeField, ReadOnly]
		private			Entity						m_EntitySeen						= null;

		public override ESenses						SenseType							=> ESenses.SIGHT;
		public			ESightTargetEventType		TargetInfoType						=> m_TargetInfoType;
		public			Vector3						SeenPosition						=> m_SeenPosition;
		public			Vector3						LastDirection						=> m_LastDirection;
		public			Vector3						ViewerPosition						=> m_ViewerPosition;
		public			Entity						EntitySeen							=> m_EntitySeen;

		public static SightEvent TargetAcquiredEvent(in Entity entitySeen, in Vector3 seenPosition, in Vector3 viewerPosition)
		{
			SightEvent @event = CreateInstance<SightEvent>();
			{
				@event.m_TargetInfoType = ESightTargetEventType.ACQUIRED;
				@event.m_EntitySeen = entitySeen;
				@event.m_SeenPosition = seenPosition;
				@event.m_ViewerPosition = viewerPosition;
			}
			return @event;
		}

		public static SightEvent TargetChangedEvent(in Entity entitySeen, in Vector3 seenPosition, in Vector3 viewerPosition)
		{
			SightEvent @event = CreateInstance<SightEvent>();
			{
				@event.m_TargetInfoType = ESightTargetEventType.CHANGED;
				@event.m_EntitySeen = entitySeen;
				@event.m_SeenPosition = seenPosition;
				@event.m_ViewerPosition = viewerPosition;
			}
			return @event;
		}

		public static SightEvent TargetLostEvent(in Entity lostTarget, in Vector3 lastSeenPosition, in Vector3 lastDirection, in Vector3 viewerPosition)
		{
			SightEvent @event = CreateInstance<SightEvent>();
			{
				@event.m_TargetInfoType = ESightTargetEventType.LOST;
				@event.m_EntitySeen = lostTarget;
				@event.m_SeenPosition = lastSeenPosition;
				@event.m_LastDirection = lastDirection;
				@event.m_ViewerPosition = viewerPosition;
			}
			return @event;
		}

		public (Entity EntitySeen, Vector3 SeenPosition, Vector3 ViewerPosition) AsTargetAcquiredEvent()
		{
			return (m_EntitySeen, m_SeenPosition, m_ViewerPosition);
		}

		public (Entity EntitySeen, Vector3 SeenPosition, Vector3 ViewerPosition) AsTargetChangedEvent()
		{
			return (m_EntitySeen, m_SeenPosition, m_ViewerPosition);
		}

		public (Entity LostTarget, Vector3 SeenPosition, Vector3 LastDirection, Vector3 ViewerPosition) AsTargetLostEvent()
		{
			return (m_EntitySeen, m_SeenPosition, m_LastDirection, m_ViewerPosition);
		}
	}
}
