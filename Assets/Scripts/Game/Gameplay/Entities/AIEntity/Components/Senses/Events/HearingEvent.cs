
using UnityEngine;


namespace Entities.AI.Components.Senses
{
	[System.Serializable]
	public class HearingEvent : SenseEvent
	{
		[SerializeField, ReadOnly]
		private 		ESoundType					m_SoundType							= default;
		[SerializeField, ReadOnly]
		private			Vector3						m_SoundPosition						= Vector3.zero;
		/// <summary> Hears position to sound position </summary>
		[SerializeField, ReadOnly]
		private			Vector3						m_Direction							= Vector3.zero;

		public override	ESenses						SenseType							=> ESenses.HEARING;
		public			ESoundType					SoundType							=> m_SoundType;
		public			Vector3						SoundPosition						=> m_SoundPosition;
		/// <summary> Hears position to sound position </summary>
		public			Vector3						Direction							=> m_Direction;

		public static HearingEvent NewHearingEvent(in ESoundType soundType, in Vector3 soundPosition, in Vector3 direction)
		{
			HearingEvent @event = CreateInstance<HearingEvent>();
			{
				@event.m_SoundType = soundType;
				@event.m_SoundPosition = soundPosition;
				@event.m_Direction = direction;
			}
			return @event;
		}

		public void Deconstruct(out ESoundType soundType, out Vector3 soundPosition, out Vector3 direction)
		{
			soundType = m_SoundType;
			soundPosition = m_SoundPosition;
			direction = m_Direction;
		}
	}
}
