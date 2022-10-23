
using UnityEngine;

namespace Entities.AI.Components.Senses
{
	internal class Damage : Sense
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

		public void Notify(in Vector3 hitterPosition, in Vector3 hittedPosition, in EDamageType damageType)
		{
			// Notify team if available
			m_PerceptionComponent.GetSense<Team>()?.Notify(hittedPosition, hitterPosition - hittedPosition, damageType);
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
