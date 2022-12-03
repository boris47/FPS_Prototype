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
		private			Vector3						m_SeenVelocity						= Vector3.zero;
		[SerializeField, ReadOnly]
		private			Vector3						m_ViewerPosition					= Vector3.zero;
		[SerializeField, ReadOnly]
		private			Entity						m_EntitySeen						= null;

		public override ESenses						SenseType							=> ESenses.SIGHT;
		public			ESightTargetEventType		TargetInfoType						=> m_TargetInfoType;
		public			Vector3						SeenPosition						=> m_SeenPosition;
		public			Vector3						SeenVelocity						=> m_SeenVelocity;
		public			Vector3						ViewerPosition						=> m_ViewerPosition;
		public			Entity						EntitySeen							=> m_EntitySeen;

		public static SightEvent NewTargetAcquiredEvent(in Entity entitySeen, in Vector3 seenVelocity, in Vector3 seenPosition, in Vector3 viewerPosition)
		{
			SightEvent @event = CreateInstance<SightEvent>();
			{
				@event.Setup(ESightTargetEventType.ACQUIRED, entitySeen, seenPosition, seenVelocity, viewerPosition);
			}
			return @event;
		}

		public static SightEvent NewTargetChangedEvent(in Entity entitySeen, in Vector3 seenVelocity, in Vector3 seenPosition, in Vector3 viewerPosition)
		{
			SightEvent @event = CreateInstance<SightEvent>();
			{
				@event.Setup(ESightTargetEventType.CHANGED, entitySeen, seenPosition, seenVelocity, viewerPosition);
			}
			return @event;
		}

		public static SightEvent NewTargetLostEvent(in Entity lostTarget, in Vector3 lastSeenPosition, in Vector3 lastSeenVelocity, in Vector3 viewerPosition)
		{
			SightEvent @event = CreateInstance<SightEvent>();
			{
				@event.Setup(ESightTargetEventType.LOST, lostTarget, lastSeenPosition, lastSeenVelocity, viewerPosition);
			}
			return @event;
		}

		public (Entity EntitySeen, Vector3 SeenPosition, Vector3 SeenVelocity, Vector3 ViewerPosition) AsTargetAcquiredEvent()
		{
			return (m_EntitySeen, m_SeenPosition, m_SeenVelocity, m_ViewerPosition);
		}

		public (Entity EntitySeen, Vector3 SeenPosition, Vector3 SeenVelocity, Vector3 ViewerPosition) AsTargetChangedEvent()
		{
			return (m_EntitySeen, m_SeenPosition, m_SeenVelocity, m_ViewerPosition);
		}

		public (Entity LostTarget, Vector3 SeenPosition, Vector3 SeenVelocity, Vector3 ViewerPosition) AsTargetLostEvent()
		{
			return (m_EntitySeen, m_SeenPosition, m_SeenVelocity, m_ViewerPosition);
		}

		private void Setup(in ESightTargetEventType InTargetInfoType, in Entity InEntity, in Vector3 InTargetPosition, in Vector3 InTargetVelocity, in Vector3 InViewerPosition)
		{
			m_TargetInfoType = InTargetInfoType;
			m_EntitySeen = InEntity;
			m_SeenPosition = InTargetPosition;
			m_SeenVelocity = InTargetVelocity;
			m_ViewerPosition = InViewerPosition;
		}
	}
}
