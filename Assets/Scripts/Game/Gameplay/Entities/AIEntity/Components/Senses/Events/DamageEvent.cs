
using UnityEngine;

namespace Entities.AI.Components.Senses
{
	[System.Serializable]
	public class DamageEvent : SenseEvent
	{
		[SerializeField, ReadOnly]
		private			Vector3						m_HitterPosition					= Vector3.zero;
		[SerializeField, ReadOnly]
		private			Vector3						m_HittedPosition					= Vector3.zero;
		[SerializeField, ReadOnly]
		private			EDamageType					m_DamageType						= default;

		public override ESenses						SenseType							=> ESenses.DAMAGE;
		public			Vector3						HitterPosition						= Vector3.zero;
		public			Vector3						HittedPosition						= Vector3.zero;
		public			EDamageType					DamageType							= default;

		public static DamageEvent NewDamageEvent(in Vector3 hitterPosition, in Vector3 hittedPosition, in EDamageType damageType)
		{
			DamageEvent @event = CreateInstance<DamageEvent>();
			{
				@event.m_HitterPosition = hitterPosition;
				@event.m_HittedPosition = hittedPosition;
				@event.m_DamageType = damageType;
			}
			return @event;
		}

		public void Deconstruct(out Vector3 hitterPosition, out Vector3 hittedPosition, out EDamageType damageType)
		{
			hitterPosition = m_HitterPosition;
			hittedPosition = m_HittedPosition;
			damageType = m_DamageType;
		}
	}
}
