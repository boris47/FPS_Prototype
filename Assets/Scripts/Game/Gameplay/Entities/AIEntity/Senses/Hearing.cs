
using UnityEngine;


namespace Entities.AI.Components.Senses
{
	[System.Serializable]
	public class HearingEvent : SenseEvent
	{
		public override ESenses			SenseType				=> ESenses.HEARING;
		public readonly ESoundType		SoundType				= default;
		public readonly Vector3			SoundPosition			= Vector3.zero;
		/// <summary> Hears position to sound position </summary>
		public readonly Vector3			Direction				= Vector3.zero;

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

	/// <summary> The ears of the entity, evaluate the interest to sounds in range </summary>
	internal class Hearing : Sense
	{
		[SerializeField]
		protected		float		m_HearingRange		= 0f;

		protected override void SetupInternal()
		{

		}

		protected override void OnEnableInternal()
		{
			// Register to Sound Manager
			SoundManager.OnSoundPlay += OnSoundPlay;
		}

		protected override void OnDisableInternal()
		{
			SoundManager.OnSoundPlay -= OnSoundPlay;
		}

		private void OnSoundPlay(in Entity source, in ESoundType soundType, in Vector3 worldPosition)
		{
			bool bIsAudible = Vector3.Distance(worldPosition, transform.position) < m_HearingRange;
			if (bIsAudible && m_Owner.IsInterestedAt(source, soundType))
			{
				m_PerceptionComponent.Senses.OnSenseEvent(HearingEvent.NewHearingEvent(soundType, worldPosition, transform.position - worldPosition));

				// Notify team if available
				// TODO let AI Decide
				if (m_PerceptionComponent.Senses.TryGetSense(out Team teamSense))
				{
					teamSense.Notify(soundType, worldPosition);
				}
			}
		}


		protected override void OnThink()
		{

		}

		protected override void OnPhysicFrame(float fixedDeltaTime)
		{

		}

		protected override void OnFrame(float deltaTime)
		{

		}

		protected override void OnLateFrame(float deltaTime)
		{

		}
	}
}
