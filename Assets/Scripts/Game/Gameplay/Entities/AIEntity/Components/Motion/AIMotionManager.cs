
using UnityEngine;

namespace Entities.AI.Components
{
	using TypeReferences;

	public sealed class AIMotionManager : AIEntityComponent, IMotionManager
	{
		[SerializeField, Inherits(typeof(AIMotionStrategyBase), AllowAbstract = false, ShowNoneElement = false)]
		private		TypeReference					m_DefaultMotionStrategyType		= typeof(AIMotionStrategyGrounded);

		[SerializeField, Min(0f)]
		private		float							m_MaxMoveSpeed					= 1f;

		[SerializeField, ReadOnly]
		private		AIMotionStrategyBase			m_CurrentMotionStrategy			= null;

		[SerializeField, ReadOnly]
		private		Transform						m_TargetLocation				= null;

		//--------------------
		public		Transform						TargetLocation					=> m_TargetLocation;
		public		float							MaxMoveSpeed					=> m_MaxMoveSpeed;
		public		Vector3							Position						=> m_CurrentMotionStrategy.Position;
		public		Vector3							Destination						=> m_CurrentMotionStrategy.Destination;
		public		Vector3							Velocity						=> m_CurrentMotionStrategy.Velocity;


		//////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			m_TargetLocation = new GameObject($"{name}: Position target").transform;
			m_TargetLocation.SetParent(transform);
			m_TargetLocation.localPosition = Vector3.zero;
			m_TargetLocation.localRotation = Quaternion.identity;
		}

		//////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();
			
			if (m_CurrentMotionStrategy.IsNull())
			{
				SetMotionType(m_DefaultMotionStrategyType);
			}
			else
			{
				// We just ensure valid configuration for this strategy
				m_CurrentMotionStrategy.Configure(this);
			}
		}

		//////////////////////////////////////////////////////////////////
		// This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only)
		protected override void OnValidate()
		{
			base.OnValidate();

			gameObject.TryGetIfNotAssigned(ref m_CurrentMotionStrategy);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDestroy()
		{
			base.OnDestroy();

			m_TargetLocation.gameObject.Destroy();
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
				m_CurrentMotionStrategy.Configure(this);

				if (snapshot.IsNotNull())
				{
					m_CurrentMotionStrategy.PorcessSnapshot(snapshot);
				}
			}
			else
			{
				// We just ensure valid configuration for this strategy
				m_CurrentMotionStrategy.Configure(this);
			}
			return m_CurrentMotionStrategy;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool	RequestMoveTowardsEntity(in Entity InTargetEntity)
		{
			return m_CurrentMotionStrategy.RequestMoveTowardsEntity(InTargetEntity);
		}

		//////////////////////////////////////////////////////////////////////////
		public bool	RequireMovementTo(in Vector3 InDestination)
		{
			return m_CurrentMotionStrategy.RequestMoveToPosition(InDestination);
		}

		//////////////////////////////////////////////////////////////////////////
		public void Stop(in bool bImmediately)
		{
			m_CurrentMotionStrategy.Stop(bImmediately);
		}
	}
}
