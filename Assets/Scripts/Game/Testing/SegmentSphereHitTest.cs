using UnityEngine;

public class SegmentSphereHitTest : TestBase
{
	[SerializeField, ReadOnly]
	private Transform m_LineStart = null;
	
	[SerializeField, ReadOnly]
	private Transform m_LineEnd = null;

	[SerializeField, ReadOnly]
	private Transform m_Sphere = null;

	[SerializeField, Min(0.1f)]
	private float m_SphereRadius = 0.1f;


	protected virtual void OnDrawGizmos()
	{
		if (m_LineStart.IsNull())
		{
			CreateGOChild(transform, out m_LineStart, "LineStart");
			m_LineStart.localPosition = Vector3.forward;
		}
		if (m_LineEnd.IsNull())
		{
			CreateGOChild(transform, out m_LineEnd, "LineEnd");
			m_LineEnd.localPosition = Vector3.back;
		}
		if (m_Sphere.IsNull())
		{
			CreateSphereTransform(transform, out m_Sphere, "Sphere");
		}

		m_LineStart.SetSiblingIndex(0);
		m_LineEnd.SetSiblingIndex(1);
		m_Sphere.SetSiblingIndex(2);
		m_Sphere.localScale = m_SphereRadius * 2f * Vector3.one;

		// Sphere
		{
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.blue))
			{
				Gizmos.DrawSphere(m_Sphere.position, m_SphereRadius);
			}
		}

		// Segment
		{
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.red))
			{
				Gizmos.DrawSphere(m_LineStart.position, 0.2f);
			}
			Gizmos.DrawLine(m_LineStart.position, m_LineEnd.position);
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.blue))
			{
				Gizmos.DrawSphere(m_LineEnd.position, 0.2f);
			}
		}

		if (Utils.Math.HasSegmentSphereIntersection(m_LineStart.position, m_LineEnd.position, m_Sphere.position, m_SphereRadius, out Vector3 OutClosestPoint))
		{
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.green))
			{
				Gizmos.DrawSphere(OutClosestPoint, 0.01f);
			}
		}
	}
}
