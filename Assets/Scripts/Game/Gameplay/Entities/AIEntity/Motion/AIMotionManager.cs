
using UnityEngine;

namespace Entities.AI.Components
{
	public sealed class AIMotionManager : MotionManager
	{
		[SerializeField, ReadOnly]
		private		AIMotionStrategyBase				m_CurrentMotionStrategy			= null;


		//--------------------
		public		Vector3								Position						=> m_CurrentMotionStrategy?.Position ?? Vector3.zero;


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
			//	SetMotionType<AIMotionStrategyGrounded>();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetMotionType<T>() where T : AIMotionStrategyBase, new()
		{
			if (m_CurrentMotionStrategy.IsNotNull() && m_CurrentMotionStrategy.GetType() != typeof(T))
			{
				m_CurrentMotionStrategy.enabled = false;

				m_CurrentMotionStrategy.StopAllCoroutines();

				Destroy(m_CurrentMotionStrategy);
			}

			m_CurrentMotionStrategy = gameObject.AddComponent<T>();
		}

		//////////////////////////////////////////////////////////////////////////
		public void	SetDestination(in Vector3 InDestination)
		{
			m_CurrentMotionStrategy.SetNewDestination(InDestination);
		}


		//////////////////////////////////////////////////////////////////////////
		public override bool CanSwim(SwimVolume swimVolume)
		{
			throw new System.NotImplementedException();
		}

		public override void OnSwimVolumeEnter(SwimVolume swimVolume)
		{
			
		}

		public override void OnSwimVolumeExit(SwimVolume swimVolume)
		{

		}
	}
}
