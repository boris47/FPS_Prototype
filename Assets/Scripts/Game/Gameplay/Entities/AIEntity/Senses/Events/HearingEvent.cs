
using UnityEngine;


namespace Entities.AI.Components.Senses
{
	[System.Serializable]
	public class HearingEvent : SenseEvent
	{
		public override ESenses						SenseType						=> ESenses.HEARING;
		public readonly ESoundType					SoundType						= default;
		public readonly Vector3						SoundPosition					= Vector3.zero;
		/// <summary> Hears position to sound position </summary>
		public readonly Vector3						Direction						= Vector3.zero;

		public static HearingEvent NewHearingEvent(in ESoundType soundType, in Vector3 soundPosition, in Vector3 direction) => new HearingEvent(soundType, soundPosition, direction);

		private HearingEvent(in ESoundType soundType, in Vector3 soundPosition, in Vector3 direction)
		{
			SoundType = soundType;
			SoundPosition = soundPosition;
			Direction = direction;
		}

		public void Deconstruct(out ESoundType soundType, out Vector3 soundPosition, out Vector3 direction)
		{
			soundType = SoundType;
			soundPosition = SoundPosition;
			direction = Direction;
		}
	}
}
