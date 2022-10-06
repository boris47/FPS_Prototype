
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components
{
	using UnityEngine.AI;
	
	public class AIEntityNavMeshAgentController : AIEntityMotionControllerBase
	{
		[SerializeField, ReadOnly]
		private		NavMeshAgent						m_Agent							= null;

		public override Vector3 Destination => m_Agent.IsNotNull() ? m_Agent.pathEndPosition : Vector3.zero;
		public override Vector3 Position => m_Agent.IsNotNull() ? m_Agent.transform.position : transform.position;

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

		//////////////////////////////////////////////////////////////////////////
		public override bool RequireMovementTo(in Vector3 InDestination)
		{
			bool bResult = false;
			if (bResult = Utils.CustomAssertions.IsTrue(m_Agent.isOnNavMesh))
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
