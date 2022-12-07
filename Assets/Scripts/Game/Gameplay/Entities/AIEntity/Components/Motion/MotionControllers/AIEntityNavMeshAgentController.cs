
using UnityEngine;

namespace Entities.AI.Components
{
	using UnityEngine.AI;

	[RequireComponent(typeof(NavMeshAgent))]
	public class AIEntityNavMeshAgentController : AIEntityMotionControllerBase
	{
		[SerializeField, ReadOnly]
		private		NavMeshAgent						m_Agent							= null;


		public override Vector3 Position => m_Agent.transform.position;
		public override Vector3 Velocity => m_Agent.velocity;
		public override Vector3 Destination => m_Agent.pathEndPosition;


		private		LinkTransformPosition				m_PositionLinker				= null;
		private		GameObject							m_TargetLocation				= null;


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			m_Agent = gameObject.AddChildWithComponent<NavMeshAgent>(nameof(NavMeshAgent), false, true);
			m_Agent.transform.position = m_Owner.transform.position;
			m_Agent.transform.SetParent(transform, worldPositionStays: true);
			// TODO Configure NavMeshAgent
			{

			}
			m_Agent.gameObject.SetActive(true);

			m_PositionLinker = gameObject.AddComponent<LinkTransformPosition>();
			m_PositionLinker.SetSource(m_Agent.transform);
			m_PositionLinker.SetTarget(transform);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDestroy()
		{
			base.OnDestroy();

			m_TargetLocation.Destroy();

			m_Agent.gameObject.Destroy();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnValidate()
		{
			base.OnValidate();
			gameObject.TryGetComponent(out m_Agent);
		}

		//////////////////////////////////////////////////////////////////////////
		public override bool RequestMoveTowardsEntity(in Entity InTargetEntity)
		{
			if (InTargetEntity.IsNotNull())
			{
				if (m_TargetLocation == null)
				{
					m_TargetLocation = new GameObject($"{name}: Position target");
				}
				m_TargetLocation.transform.SetParent(InTargetEntity.Body);
				m_TargetLocation.transform.localPosition = Vector3.zero;
				m_TargetLocation.transform.rotation = Quaternion.identity;

				if (Utils.CustomAssertions.IsNotNull(GameManager.CyclesEvents))
				{
					GameManager.CyclesEvents.OnLateFrame -= UpdateTargetLocation;
					GameManager.CyclesEvents.OnLateFrame += UpdateTargetLocation;
					m_Agent.isStopped = false;
				}
			}

			return InTargetEntity.IsNotNull();
		}

		//////////////////////////////////////////////////////////////////////////
		public override bool RequireMovementTo(in Vector3 InDestination)
		{
			m_TargetLocation.Destroy();
			m_Agent.isStopped = false;

			if (Utils.CustomAssertions.IsNotNull(GameManager.CyclesEvents))
			{
				GameManager.CyclesEvents.OnLateFrame -= UpdateTargetLocation;
				GameManager.CyclesEvents.OnLateFrame += UpdateTargetLocation;
			}
			return true;
		}

		//////////////////////////////////////////////////////////////////////////
		public override void StopMovement(in bool bImmediately)
		{
			if (Utils.CustomAssertions.IsNotNull(GameManager.CyclesEvents))
			{
				GameManager.CyclesEvents.OnLateFrame -= UpdateTargetLocation;
			}
			m_Agent.isStopped = true;
		}

		//////////////////////////////////////////////////////////////////////////
		private void UpdateTargetLocation(float InDeltaTime)
		{
			if (m_TargetLocation.IsNotNull())
			{
				m_Agent.SetDestination(m_TargetLocation.transform.position);
			}
		}
	}
}
