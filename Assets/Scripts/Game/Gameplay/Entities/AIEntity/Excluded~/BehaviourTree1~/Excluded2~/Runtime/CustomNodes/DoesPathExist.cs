using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Entities.AI.Components.Behaviours
{
	public class DoesPathExist : Conditional
	{
		[SerializeField]
		private NavMeshAgent m_NavMeshAgent = null;

		[SerializeField]
		private Vector3 m_PointA = Vector3.zero;

		[SerializeField]
		private Vector3 m_PointB = Vector3.zero;

		protected override void OnAwake()
		{
			CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_NavMeshAgent));
			base.OnAwake();
		}

		protected override void OnStart()
		{
			base.OnStart();
		}

		protected override Status OnUpdate()
		{
			return base.OnUpdate();
		}

		public override bool CanUpdate()
		{
			return base.CanUpdate();
		}

		public override void Abort()
		{
			base.Abort();
		}

		protected override bool IsUpdatable()
		{
			return false;
		}
	}
}
