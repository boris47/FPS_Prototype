
using UnityEngine;

namespace Entities.AI.Components
{
	public class AIEntityMovementComponent : AIEntityComponent
	{
		[SerializeField]
		protected AIMovementCollider m_MovementCollider			= null;

		[SerializeField, ReadOnly]
		/// <summary>  </summary>
		protected Vector3 m_TargetPosition = Vector3.zero;

		[SerializeField, ReadOnly]
		/// <summary>  </summary>
		protected Vector3 m_TargetPositionOffset = Vector3.zero;

		[SerializeField, ReadOnly]
		/// <summary>  </summary>
		protected Vector3 m_TargetPositionOffsetDefault = Vector3.zero;


		//////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			if (gameObject.GetIfNotAssigned(ref m_MovementCollider))
			{
				m_MovementCollider.OnTriggerEnterEv += OnTriggerEnterEv;
				m_MovementCollider.OnTriggerExitEv  += OnTriggerExitEv;
			}
		}

		//////////////////////////////////////////////////////////////////
		protected virtual void OnTriggerEnterEv(in Collider other)
		{

		}

		//////////////////////////////////////////////////////////////////
		protected virtual void OnTriggerExitEv(in Collider other)
		{

		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (m_MovementCollider.IsNotNull())
			{
				m_MovementCollider.OnTriggerEnterEv -= OnTriggerEnterEv;
				m_MovementCollider.OnTriggerExitEv -= OnTriggerExitEv;
			}
		}
	}
}
