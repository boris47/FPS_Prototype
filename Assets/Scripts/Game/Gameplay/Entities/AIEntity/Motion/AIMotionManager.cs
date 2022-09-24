
using UnityEngine;

namespace Entities.AI.Components
{
	using TypeReferences;
	
	public sealed class AIMotionManager : MotionManager
	{
		[SerializeField, Inherits(typeof(AIMotionStrategyBase), AllowAbstract = false, ShowNoneElement = false)]
		private		TypeReference							m_DefaultMotionStrategyType		= typeof(AIMotionStrategyGrounded);

		[SerializeField, ReadOnly]
		private		AIMotionStrategyBase					m_CurrentMotionStrategy			= null;

		private		AIEntity								m_Entity						=> m_Owner as AIEntity;


		//--------------------
		public		Vector3									Position						=> m_CurrentMotionStrategy?.Position ?? Vector3.zero;




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
				SetMotionType(m_DefaultMotionStrategyType);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public T SetMotionType<T>() where T : AIMotionStrategyBase, new() => SetMotionType(typeof(T)) as T;

		//////////////////////////////////////////////////////////////////////////
		private AIMotionStrategyBase SetMotionType(in System.Type InMotionType)
		{
			if (m_CurrentMotionStrategy == null || (m_CurrentMotionStrategy.IsNotNull() && m_CurrentMotionStrategy.GetType() != InMotionType))
			{
				AIEntityMotionTransitionSnapshot snapshot = null;
				if (m_CurrentMotionStrategy.IsNotNull())
				{
					snapshot = m_CurrentMotionStrategy.CreateSnapshot();

					Destroy(m_CurrentMotionStrategy);
				}

				m_CurrentMotionStrategy = gameObject.AddComponent(InMotionType) as AIMotionStrategyBase;

				if (snapshot.IsNotNull())
				{
					m_CurrentMotionStrategy.PorcessSnapshot(snapshot);
				}
			}
			return m_CurrentMotionStrategy;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool	RequireMovementTo(in Vector3 InDestination)
		{
			return m_CurrentMotionStrategy.RequireMovementTo(InDestination);
		}

		//////////////////////////////////////////////////////////////////////////
		public void Stop(in bool bImmediately)
		{
			m_CurrentMotionStrategy.Stop(bImmediately);
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
