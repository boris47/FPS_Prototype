
using UnityEngine;


namespace Entities.AI.Components.Senses
{
	/// <summary> Represent the communication channel with other entities </summary>
	internal class Team : Sense
	{
		protected override void SetupInternal()
		{

		}

		protected override void OnEnableInternal()
		{

		}

		protected override void OnDisableInternal()
		{

		}


		public void Notify(in Entity entity, in Vector3 worldPosition, in Vector3 lastDirection)
		{
			m_PerceptionComponent.SendSenseEvent(TeamEvent.NewHostileEvent(worldPosition, lastDirection, entity));
		}

		public void Notify(in ESoundType soundType, in Vector3 worldPosition)
		{
			m_PerceptionComponent.SendSenseEvent(TeamEvent.NewSoundEvent(worldPosition, soundType));
		}

		public void Notify(in Vector3 hitterPosition, in Vector3 direction, in EDamageType damageType)
		{
			m_PerceptionComponent.SendSenseEvent(TeamEvent.NewDamageEvent(hitterPosition, direction, damageType));
		}


		protected override void OnThink(float deltaTime)
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
