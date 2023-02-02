using UnityEngine;

public class LineSphereHitTest : MonoBehaviour
{
	[SerializeField, ReadOnly]
	private Transform m_LineStart = null;
	
	[SerializeField, ReadOnly]
	private Transform m_LineEnd = null;

	[SerializeField, ReadOnly]
	private Transform m_Sphere = null;

	[SerializeField, Min(0.1f)]
	private float m_SphereRadius = 0.1f;

	private Vector3? m_Intersection = null;


	protected virtual void OnDrawGizmos()
	{
		if (m_LineStart.IsNull())
		{
			CreatePoint(transform, ref m_LineStart, "LineStart");
			m_LineStart.localPosition = Vector3.forward;
		}
		if (m_LineEnd.IsNull())
		{
			CreatePoint(transform, ref m_LineEnd, "LineEnd");
			m_LineEnd.localPosition = Vector3.back;
		}
		if (m_Sphere.IsNull())
		{
			m_Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
			m_Sphere.SetParent(transform);
			{
				m_Sphere.GetComponent<Collider>().Destroy();
			}
			m_Sphere.localPosition = Vector3.zero;
		}

		m_LineStart.SetSiblingIndex(0);
		m_LineEnd.SetSiblingIndex(1);
		m_Sphere.SetSiblingIndex(2);
		m_Sphere.localScale = m_SphereRadius * 2f * Vector3.one;

		if (Utils.Math.HasSegmentSphereIntersection(m_Sphere.position, m_SphereRadius, m_LineStart.position, m_LineEnd.position, out Vector3 OutClosestPoint))
		{
			m_Intersection = OutClosestPoint;
		}
		else
		{
			m_Intersection = null;
		}

		Gizmos.DrawLine(m_LineStart.position, m_LineEnd.position);

		using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.red))
		{
			Gizmos.DrawSphere(m_LineStart.position, 0.2f);
		}

		if (m_Intersection.HasValue)	
		{
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.green))
			{
				Gizmos.DrawSphere(m_Intersection.Value, 0.01f);
			}
		}

		using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.blue))
		{
			Gizmos.DrawSphere(m_LineEnd.position, 0.2f);
		}
	}

	private static void CreatePoint(in Transform parent, ref Transform InTransform, in string InGOName)
	{
		InTransform = new GameObject(InGOName).transform;
		InTransform.SetParent(parent);
		InTransform.localPosition = Vector3.zero;
	}
}
