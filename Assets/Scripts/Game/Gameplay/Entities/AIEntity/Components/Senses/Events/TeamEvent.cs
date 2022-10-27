
using UnityEngine;


namespace Entities.AI.Components.Senses
{
	[System.Serializable]
	public enum ETeamMessageType
	{
		NONE,
		DAMAGE,
		SOUND,
		HOSTILE,
		HOSTILE_LOST
	}

	[System.Serializable]
	public class TeamEvent : SenseEvent
	{
		[SerializeField, ReadOnly]
		private			ETeamMessageType			m_MessageType						= ETeamMessageType.NONE;
		/// <summary> DAMAGE (hitter position), SOUND (sound position), HOSTILE(Entity seen) </summary>
		[SerializeField, ReadOnly]
		private			Vector3						m_EventPosition						= Vector3.zero;
		/// <summary> DAMAGE, HOSTILE </summary>
		[SerializeField, ReadOnly]
		private			EDamageType					m_DamageType						= default;
		/// <summary> DAMAGE, HOSTILE </summary>
		[SerializeField, ReadOnly]
		private			Vector3						m_Direction							= Vector3.zero;
		/// <summary> SOUND </summary>
		[SerializeField, ReadOnly]
		private			ESoundType					m_SoundType							= default;
		/// <summary> HOSTILE </summary>
		[SerializeField, ReadOnly]
		private			Entity						m_EntitySeen						= null;


		public override ESenses						SenseType							=> ESenses.TEAM;
		public			ETeamMessageType			MessageType							=> m_MessageType;
		/// <summary> DAMAGE (hitter position), SOUND (sound position), HOSTILE(Entity seen) </summary>
		public			Vector3						EventPosition						=> m_EventPosition;
		/// <summary> DAMAGE, HOSTILE </summary>
		public			EDamageType					DamageType							=> m_DamageType;
		/// <summary> DAMAGE, HOSTILE </summary>
		public			Vector3						Direction							=> m_Direction;
		/// <summary> SOUND </summary>
		public			ESoundType					SoundType							=> m_SoundType;
		/// <summary> HOSTILE </summary>
		public			Entity						EntitySeen							=> m_EntitySeen;


		public static TeamEvent NewDamageEvent(in Vector3 worldPosition, in Vector3 direction, in EDamageType damageType)
		{
			TeamEvent @event = CreateInstance<TeamEvent>();
			{
				@event.m_MessageType = ETeamMessageType.DAMAGE;
				@event.m_EventPosition = worldPosition;
				@event.m_Direction = direction;
				@event.m_DamageType = damageType;
			}
			return @event;
		}

		public static TeamEvent NewSoundEvent(in Vector3 worldPosition, in ESoundType soundType)
		{
			TeamEvent @event = CreateInstance<TeamEvent>();
			{
				@event.m_MessageType = ETeamMessageType.SOUND;
				@event.m_EventPosition = worldPosition;
				@event.m_SoundType = soundType;
			}
			return @event;
		}

		public static TeamEvent NewHostileEvent(in Vector3 worldPosition, in Vector3 lastDirection, in Entity entitySeen)
		{
			TeamEvent @event = CreateInstance<TeamEvent>();
			{
				@event.m_MessageType = entitySeen.IsNotNull() ? ETeamMessageType.HOSTILE : ETeamMessageType.HOSTILE_LOST;
				@event.m_EventPosition = worldPosition;
				@event.m_Direction = lastDirection;
				@event.m_EntitySeen = entitySeen;
			}
			return @event;
		}

		public (Vector3 HittedPosition, Vector3 Direction, EDamageType DamageType) AsDamageMessage()
		{
			return (m_EventPosition, m_Direction, m_DamageType);
		}

		public (Vector3 SoundPosition, ESoundType SoundType) AsSoundMessage()
		{
			return (m_EventPosition, m_SoundType);
		}

		public (Vector3 EntityLastPosition, Vector3 LastDirection, Entity Entity) AsHostileEvent()
		{
			return (m_EventPosition, m_Direction, m_EntitySeen);
		}
	}
}
