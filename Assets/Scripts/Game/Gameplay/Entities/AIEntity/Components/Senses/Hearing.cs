
using UnityEngine;


namespace Entities.AI.Components.Senses
{
	/// <summary> The ears of the entity, evaluate the interest to sounds in range </summary>
	internal class Hearing : Sense
	{
		[SerializeField]
		protected		float		m_HearingRange		= 0f;

		//////////////////////////////////////////////////////////////////////////
		protected override void SetupInternal()
		{

		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnableInternal()
		{
			// Register to Sound Manager
			SoundManager.OnSoundPlay += OnSoundPlay;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisableInternal()
		{
			SoundManager.OnSoundPlay -= OnSoundPlay;
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnSoundPlay(in Entity source, in ESoundType soundType, in Vector3 worldPosition)
		{
			bool bIsAudible = Vector3.Distance(worldPosition, transform.position) < m_HearingRange;
			if (bIsAudible && m_Owner.IsInterestedAt(source, soundType))
			{
				m_PerceptionComponent.SendSenseEvent(HearingEvent.NewHearingEvent(soundType, worldPosition, transform.position - worldPosition));

				// Notify team if available
				// TODO let AI Decide
				if (m_PerceptionComponent.TryGetSense(out Team teamSense))
				{
					teamSense.Notify(soundType, worldPosition);
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnThink(float deltaTime)
		{

		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnPhysicFrame(float fixedDeltaTime)
		{

		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnFrame(float deltaTime)
		{

		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnLateFrame(float deltaTime)
		{

		}
	}
}
