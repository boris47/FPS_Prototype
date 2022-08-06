
using UnityEngine;

namespace Entities.AI.Components.Senses
{
	[System.Serializable]
	public class DamageEvent : SenseEvent
	{
		public override ESenses			SenseType				=> ESenses.DAMAGE;
		public readonly Vector3			HitterPosition			= Vector3.zero;
		public readonly Vector3			HittedPosition			= Vector3.zero;
		public readonly EDamageType		DamageType				= default;

		public static DamageEvent NewDamageEvent(in Vector3 hitterPosition, in Vector3 hittedPosition, in EDamageType damageType)
			=> new DamageEvent(hitterPosition, hittedPosition, damageType);

		private DamageEvent(in Vector3 hitterPosition, in Vector3 hittedPosition, in EDamageType damageType)
		{
			HitterPosition = hitterPosition;
			HittedPosition = hittedPosition;
			DamageType = damageType;
		}

		public void Deconstruct(out Vector3 hitterPosition, out Vector3 hittedPosition, out EDamageType damageType)
		{
			hitterPosition = HitterPosition;
			hittedPosition = HittedPosition;
			damageType = DamageType;
		}
	}


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
			m_PerceptionComponent.Senses.GetSense<Team>()?.Notify(hittedPosition, hitterPosition - hittedPosition, damageType);
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
