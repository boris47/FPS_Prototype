using UnityEngine;

public class TorqueToProjectedPoint : PointToLineTest
{
	[SerializeField, ReadOnly]
	private GameObject m_Capsule = null;

	[SerializeField, ReadOnly]
	private Rigidbody m_Rigidbody = null;

	[SerializeField, HideInInspector]
	private Transform m_TopCapsuleLocator = null;

#if UNITY_EDITOR
	protected override void OnDrawGizmos()
	{
		if (m_Capsule.IsNull())
		{
			m_Capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			CapsuleCollider capsuleCollider = m_Capsule.GetComponent<CapsuleCollider>();
			capsuleCollider.isTrigger = false;
			
			m_Capsule.transform.SetParent(transform);
			m_Capsule.transform.localPosition = Vector3.right;
			m_Capsule.transform.Translate(0.5f * capsuleCollider.height * Vector3.up);
			{
				m_TopCapsuleLocator = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
				m_TopCapsuleLocator.SetParent(m_Capsule.transform);
				Vector3 worldTopPoint = capsuleCollider.ClosestPoint(m_Capsule.transform.position + (Vector3.up * 100f));
				m_TopCapsuleLocator.position = worldTopPoint;
				m_TopCapsuleLocator.localScale = Vector3.one * 0.1f;
			}
			m_Rigidbody = m_Capsule.AddComponent<Rigidbody>();
			m_Rigidbody.useGravity = false;	
		}

		base.OnDrawGizmos();
	}

	private void FixedUpdate()
	{
		if (m_Projection.HasValue && m_Capsule.IsNotNull())
		{
			Vector3 direction = (m_Projection.Value - m_Capsule.transform.position);
			Vector3 torqueVec = Vector3.Cross(m_Capsule.transform.up, direction.normalized);

			m_Rigidbody.AddTorque(torqueVec, ForceMode.Acceleration);
			Debug.DrawLine(m_Capsule.transform.position, m_Capsule.transform.position + direction);
		}
	}

#endif

}
