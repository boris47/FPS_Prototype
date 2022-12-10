
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.AI;

namespace Entities.AI.Components
{
	public class AIMotionStrategyGrounded : AIMotionStrategyBase
	{
		[SerializeField, ReadOnly]
		private					NavMeshAgent							m_Agent							= null;


		public		override	Vector3									Position						=> m_Owner.Body.position;
		public		override	Vector3									Destination						=> m_Agent.pathEndPosition;
		public		override	Vector3									Velocity						=> m_Agent.velocity;


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			if (enabled = Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_Agent)))
			{
				
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();

			if (Utils.CustomAssertions.IsNotNull(GameManager.CyclesEvents))
			{
				GameManager.CyclesEvents.OnFrame += OnFrame;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnFrame(float InDeltaTime)
		{
			m_Agent.speed = AIMotionManager.MaxMoveSpeed;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();

			if (GameManager.CyclesEvents.IsNotNull())
			{
				GameManager.CyclesEvents.OnFrame -= OnFrame;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public override AIEntityMotionTransitionSnapshot CreateSnapshot()
		{
			return new AIEntityMotionTransitionSnapshot()
			{
				CurrentVelocity = m_Agent.velocity,
				Destination = AIMotionManager.TargetLocation
			};
		}

		//////////////////////////////////////////////////////////////////////////
		public override void PorcessSnapshot(AIEntityMotionTransitionSnapshot InSnapShot)
		{
			m_Agent.velocity = InSnapShot.CurrentVelocity;

			AIMotionManager.TargetLocation.SetPositionAndRotation(InSnapShot.Destination.position, InSnapShot.Destination.rotation);
		}

		//////////////////////////////////////////////////////////////////////////
		public override bool RequestMoveTowardsEntity(in Entity InTargetEntity)
		{
			if (GameManager.CyclesEvents.IsNotNull())
			{
				GameManager.CyclesEvents.OnLateFrame -= UpdateTargetLocation;
			}

			bool bResult = false;
			if (bResult = m_Agent.SetDestination(InTargetEntity.Body.position))
			{
				if (GameManager.CyclesEvents.IsNotNull())
				{
					GameManager.CyclesEvents.OnFrame += CheckPathStatus;
				}

				AIMotionManager.TargetLocation.SetParent(InTargetEntity.Body);
				AIMotionManager.TargetLocation.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

				if (Utils.CustomAssertions.IsNotNull(GameManager.CyclesEvents))
				{
					GameManager.CyclesEvents.OnLateFrame += UpdateTargetLocation;
					m_Agent.isStopped = false;
				}				
			}

			return bResult;
		}

		//////////////////////////////////////////////////////////////////////////
		public override bool RequestMoveToPosition(in Vector3 InDestination)
		{
			if (GameManager.CyclesEvents.IsNotNull())
			{
				GameManager.CyclesEvents.OnLateFrame -= UpdateTargetLocation;
			}

			bool bResult = false;
			if (bResult = m_Agent.SetDestination(InDestination))
			{
				if (GameManager.CyclesEvents.IsNotNull())
				{
					GameManager.CyclesEvents.OnFrame += CheckPathStatus;
				}

				AIMotionManager.TargetLocation.SetParent(null);
				AIMotionManager.TargetLocation.SetPositionAndRotation(InDestination, Quaternion.identity);
				m_Agent.isStopped = false;
			}

			return bResult;
		}

		//////////////////////////////////////////////////////////////////////////
		public override void Stop(in bool bImmediately)
		{
			if (GameManager.CyclesEvents.IsNotNull())
			{
				GameManager.CyclesEvents.OnLateFrame -= UpdateTargetLocation;
			}

			AIMotionManager.TargetLocation.SetParent(transform);
			AIMotionManager.TargetLocation.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

			m_Agent.isStopped = true;
		}

		//////////////////////////////////////////////////////////////////////////
		private void UpdateTargetLocation(float InDeltaTime)
		{
			m_Agent.SetDestination(AIMotionManager.TargetLocation.position);
		}

		//////////////////////////////////////////////////////////////////////////
		private void CheckPathStatus(float InDeltaTime)
		{
			if (CheckDestinationReached())
			{
				Stop(bImmediately: true);

				GameManager.CyclesEvents.OnFrame -= CheckPathStatus;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private bool CheckDestinationReached()
		{
			bool bResult = false;
			// Check if we've reached the destination
			if (!m_Agent.pathPending)
			{
				if (m_Agent.remainingDistance <= m_Agent.stoppingDistance)
				{
					if (!m_Agent.hasPath || Mathf.Approximately(m_Agent.velocity.sqrMagnitude, 0f))
					{
						// Done
						bResult = true;
					}
				}
			}
			return bResult;
		}
	}
}
