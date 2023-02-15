using UnityEngine;

public class TorqueToProjectedPoint : PointToLineTest
{
	[SerializeField, ReadOnly]
	private CapsuleCollider m_CapsuleCollider = null;

	[SerializeField, ReadOnly]
	private Rigidbody m_Rigidbody = null;

	[SerializeField, HideInInspector]
	private Transform m_TopCapsuleLocator = null;


#if UNITY_EDITOR
	protected override void OnDrawGizmos()
	{
		if (m_CapsuleCollider.IsNull())
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			m_CapsuleCollider = go.GetComponent<CapsuleCollider>();
			m_CapsuleCollider.isTrigger = false;
			
			go.transform.SetParent(transform);
			go.transform.localPosition = Vector3.right;
			go.transform.Translate(0.5f * m_CapsuleCollider.height * Vector3.up);
			{
				m_TopCapsuleLocator = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
				m_TopCapsuleLocator.SetParent(go.transform);
				Vector3 worldTopPoint = m_CapsuleCollider.ClosestPoint(go.transform.position + (Vector3.up * 100f));
				m_TopCapsuleLocator.position = worldTopPoint;
				m_TopCapsuleLocator.localScale = Vector3.one * 0.1f;
			}
			m_Rigidbody = go.AddComponent<Rigidbody>();
			m_Rigidbody.useGravity = false;	
		}

		Utils.Editor.GizmosHelper.DrawCollider(m_CapsuleCollider, Color.gray);
		base.OnDrawGizmos();
	}

	private void FixedUpdate()
	{
		if (m_Projection.HasValue && m_CapsuleCollider.IsNotNull())
		{
			Vector3 direction = (m_Projection.Value - m_CapsuleCollider.transform.position);
			Vector3 torqueVec = Vector3.Cross(m_CapsuleCollider.transform.up, direction.normalized);

			m_Rigidbody.AddTorque(torqueVec, ForceMode.Acceleration);
			Debug.DrawLine(m_CapsuleCollider.transform.position, m_CapsuleCollider.transform.position + direction);
		}
	}

#endif

}
