
using UnityEngine;

namespace Entities.Player.Components
{
	public class PlayerMotionTransitionSnapshot : MotionTransitionSnapshot
	{

	}


	public sealed class PlayerMotionManager : MotionManager
	{
		[SerializeField, ReadOnly]
		private		PlayerMotionStrategyBase				m_CurrentMotionStrategy			= null;


		//////////////////////////////////////////////////////////////////
		// This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only)
		protected override void OnValidate()
		{
			base.OnValidate();

			gameObject.TryGetIfNotAssigned(ref m_CurrentMotionStrategy);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();

			if (!m_CurrentMotionStrategy.IsNotNull())
			{
				SetMotionType<PlayerMotionStrategyGrounded>();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public T SetMotionType<T>() where T : PlayerMotionStrategyBase, new()
		{
			if (m_CurrentMotionStrategy == null || (m_CurrentMotionStrategy.IsNotNull() && m_CurrentMotionStrategy.GetType() != typeof(T)))
			{
				PlayerMotionTransitionSnapshot snapshot = null;
				if (m_CurrentMotionStrategy.IsNotNull())
				{
					snapshot = m_CurrentMotionStrategy.CreateSnapshot();

					Destroy(m_CurrentMotionStrategy);
				}

				m_CurrentMotionStrategy = gameObject.AddComponent<T>();

				if (snapshot.IsNotNull())
				{
					m_CurrentMotionStrategy.PorcessSnapshot(snapshot);
				}
			}
			return m_CurrentMotionStrategy as T;
		}


		//////////////////////////////////////////////////////////////////////////
		public override bool CanSwim(SwimVolume swimVolume)
		{
			return true;
		}

		//////////////////////////////////////////////////////////////////////////
		public override void OnSwimVolumeEnter(SwimVolume swimVolume)
		{
			SetMotionType<PlayerMotionStrategySwim>().Configure(swimVolume);
		}

		//////////////////////////////////////////////////////////////////////////
		public override void OnSwimVolumeExit(SwimVolume swimVolume)
		{
			SetMotionType<PlayerMotionStrategyGrounded>();
		}
	}
}
