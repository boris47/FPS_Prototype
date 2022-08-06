
using UnityEngine;

namespace Entities.Player.Components
{
	public sealed class MotionManager : PlayerEntityComponent
	{
		[SerializeField, ReadOnly]
		private		MotionStrategyBase				m_CurrentMotionStrategy			= null;

		//////////////////////////////////////////////////////////////////////////
		protected override void OnValidate()
		{
			base.OnValidate();

			if (!m_CurrentMotionStrategy.IsNotNull())
			{
				m_CurrentMotionStrategy = GetComponent<MotionStrategyBase>();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();

			if (!m_CurrentMotionStrategy.IsNotNull())
			{
				SetMotionType<MotionStrategyGrounded_Controller>();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetMotionType<T>() where T : MotionStrategyBase, new()
		{
			if (m_CurrentMotionStrategy.IsNotNull() && m_CurrentMotionStrategy.GetType() != typeof(T))
			{
				m_CurrentMotionStrategy.enabled = false;

				m_CurrentMotionStrategy.StopAllCoroutines();

				Destroy(m_CurrentMotionStrategy);
			}

			m_CurrentMotionStrategy = gameObject.AddComponent<T>();
		}

		
	}
}
