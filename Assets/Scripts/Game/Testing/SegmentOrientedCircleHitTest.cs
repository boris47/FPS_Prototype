using UnityEngine;

public class SegmentOrientedCircleHitTest : TestBase
{
	[SerializeField, ReadOnly]
	private Transform m_LineStart = null;
	
	[SerializeField, ReadOnly]
	private Transform m_LineEnd = null;

	[SerializeField, ReadOnly]
	private Transform m_CircleCenter = null;

	[SerializeField, Min(0.1f)]
	private float m_CircleRadius = 1f;


	protected virtual void OnDrawGizmos()
	{
		if (m_LineStart.IsNull())
		{
			CreateGOChild(transform, out m_LineStart, "LineStart");
			m_LineStart.localPosition = Vector3.up + Vector3.back;
		}
		if (m_LineEnd.IsNull())
		{
			CreateGOChild(transform, out m_LineEnd, "LineEnd");
			m_LineEnd.localPosition = Vector3.down + Vector3.forward;
		}
		if (m_CircleCenter.IsNull())
		{
			Create3DCircleTransform(transform, out m_CircleCenter, "3DCircle");
		}

		m_LineStart.SetSiblingIndex(0);
		m_LineEnd.SetSiblingIndex(1);
		m_CircleCenter.SetSiblingIndex(2);

		// 3D Cirle
		{
			using (new Utils.Editor.HandlesHelper.UseHandlesColor(Color.green))
			{
				UnityEditor.Handles.DrawWireDisc(m_CircleCenter.position, m_CircleCenter.up, m_CircleRadius);
			}
		}

		// Segment
		{
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.red))
			{
				Gizmos.DrawSphere(m_LineStart.position, 0.015f);
			}
			Gizmos.DrawLine(m_LineStart.position, m_LineEnd.position);
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.blue))
			{
				Gizmos.DrawSphere(m_LineEnd.position, 0.015f);
			}
		}

		// Intersection
		if (Utils.Math.HasSegmentTo3DOrientedCircleIntersection(m_LineStart.position, m_LineEnd.position, m_CircleCenter.position, m_CircleCenter.up, m_CircleRadius, out Vector3 OutIntersectionPoint))
		{
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.green))
			{
				Gizmos.DrawSphere(OutIntersectionPoint, 0.01f);
			}
		}
	}
}
