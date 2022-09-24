
using UnityEngine;


namespace Entities.AI.Components.Senses
{
	[System.Serializable]
	public enum ETeamMessageType
	{
		DAMAGE,
		SOUND,
		HOSTILE,
		HOSTILE_LOST
	}

	[System.Serializable]
	public class TeamEvent : SenseEvent
	{
		public override ESenses						SenseType						=> ESenses.TEAM;
		public ETeamMessageType						MessageType						= default;
		/// <summary> DAMAGE (hitter position), SOUND (sound position), HOSTILE(Entity seen) </summary>
		public readonly Vector3						EventPosition					= Vector3.zero;
		/// <summary> DAMAGE, HOSTILE </summary>
		public readonly EDamageType					DamageType						= default;
		public readonly Vector3						Direction						= Vector3.zero;
		/// <summary> SOUND </summary>
		public readonly ESoundType					SoundType						= default;
		/// <summary> HOSTILE </summary>
		public readonly Entity						EntitySeen						= null;


		public static TeamEvent NewDamageEvent(in Vector3 worldPosition, in Vector3 direction, in EDamageType damageType) => new TeamEvent(worldPosition, direction, damageType);
		public static TeamEvent NewSoundEvent(in Vector3 worldPosition, in ESoundType soundType) => new TeamEvent(worldPosition, soundType);
		public static TeamEvent NewHostileEvent(in Vector3 worldPosition, in Vector3 lastDirection, in Entity entitySeen) => new TeamEvent(worldPosition, lastDirection, entitySeen);

		private TeamEvent(in Vector3 worldPosition, in Vector3 direction, in EDamageType damageType) : this(ETeamMessageType.DAMAGE, worldPosition)
		{
			Direction = direction;
			DamageType = damageType;
		}

		private TeamEvent(in Vector3 worldPosition, in ESoundType soundType) : this(ETeamMessageType.SOUND, worldPosition)
		{
			SoundType = soundType;
		}

		private TeamEvent(in Vector3 worldPosition, in Vector3 lastDirection, in Entity entitySeen) :
			this(entitySeen.IsNotNull() ? ETeamMessageType.HOSTILE : ETeamMessageType.HOSTILE_LOST, worldPosition)
		{
			EntitySeen = entitySeen;
			Direction = lastDirection;
		}

		private TeamEvent(in ETeamMessageType messageType, in Vector3 worldPosition)
		{
			MessageType = messageType;
			EventPosition = worldPosition;
		}

		public (Vector3 HittedPosition, Vector3 Direction, EDamageType DamageType) AsDamageMessage() => (EventPosition, Direction, DamageType);
		public (Vector3 SoundPosition, ESoundType SoundType) AsSoundMessage() => (EventPosition, SoundType);
		public (Vector3 EntityLastPosition, Vector3 LastDirection, Entity Entity) AsHostileEvent() => (EventPosition, Direction, EntitySeen);
	}
}
