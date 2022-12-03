
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components
{
	using UnityEngine.AI;

	[RequireComponent(typeof(NavMeshAgent))]
	public class AIEntityNavMeshAgentController : AIEntityMotionControllerBase
	{
		[SerializeField, ReadOnly]
		private		NavMeshAgent						m_Agent							= null;

		public override Vector3 Position => m_Agent.IsNotNull() ? m_Agent.transform.position : transform.position;
		public override Vector3 Velocity => m_Agent.IsNotNull() ? m_Agent.velocity : Vector3.zero;
		public override Vector3 Destination => m_Agent.IsNotNull() ? m_Agent.pathEndPosition : Vector3.zero;

		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			if (enabled = Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_Agent)))
			{
				Utils.CustomAssertions.IsTrue(m_Agent.isOnNavMesh);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnValidate()
		{
			base.OnValidate();
			gameObject.TryGetComponent(out m_Agent);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();
		}

		private Entity m_EntityTarget = null;

		//////////////////////////////////////////////////////////////////////////
		public override bool RequestMoveTowardsEntity(in Entity InTargetEntity)
		{
			if (InTargetEntity.IsNotNull())
			{
				if (Utils.CustomAssertions.IsNotNull(GameManager.CyclesEvents))
				{
					GameManager.CyclesEvents.OnLateFrame += UpdateTargetLocation;
					m_Agent.isStopped = false;
					m_EntityTarget = InTargetEntity;
				}
			}

			return InTargetEntity.IsNotNull();
		}

		//////////////////////////////////////////////////////////////////////////
		public override bool StopMovingTowardsEntity(in Entity InTargetEntity)
		{
			bool outValue = false;
			if (m_EntityTarget.IsNotNull() && InTargetEntity.IsNotNull() && m_EntityTarget.Id == InTargetEntity.Id)
			{
				if (Utils.CustomAssertions.IsNotNull(GameManager.CyclesEvents))
				{
					GameManager.CyclesEvents.OnLateFrame -= UpdateTargetLocation;
					m_EntityTarget = null;
					outValue = true;
				}
			}
			return outValue;
		}

		//////////////////////////////////////////////////////////////////////////
		private void UpdateTargetLocation(float InDeltaTime)
		{
			m_Agent.SetDestination(m_EntityTarget.Body.position);
		}

		//////////////////////////////////////////////////////////////////////////
		public override bool RequireMovementTo(in Vector3 InDestination)
		{
			if (m_EntityTarget.IsNotNull())
			{
				if (Utils.CustomAssertions.IsNotNull(GameManager.CyclesEvents))
				{
					GameManager.CyclesEvents.OnLateFrame -= UpdateTargetLocation;
					m_EntityTarget = null;
				}
			}

			bool bResult = false;
			if (Utils.CustomAssertions.IsTrue(m_Agent.isOnNavMesh))
			{
				m_Agent.isStopped = false;
			//	if (!m_Agent.hasPath)
				{
					bResult = m_Agent.SetDestination(InDestination);
				}
			}
			return bResult;
		}

		//////////////////////////////////////////////////////////////////////////
		public override void StopMovement(in bool bImmediately)
		{
			m_Agent.isStopped = true;
		}
	}
}
