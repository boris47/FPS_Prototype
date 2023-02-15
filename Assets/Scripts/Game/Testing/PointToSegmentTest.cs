using UnityEngine;

public class PointToSegmentTest : TestBase
{
	[SerializeField, ReadOnly]
	private Transform m_LineStart = null;
	
	[SerializeField, ReadOnly]
	private Transform m_LineEnd = null;

	[SerializeField, ReadOnly]
	private Transform m_Point = null;


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
		if (m_Point.IsNull())
		{
			CreateGOChild(transform, out m_Point, "Point");
			m_Point.localPosition = Vector3.left;
		}

		m_LineStart.SetSiblingIndex(0);
		m_LineEnd.SetSiblingIndex(1);
		m_Point.SetSiblingIndex(2);

		// Line
		Gizmos.DrawLine(m_LineStart.position, m_LineEnd.position);

		// Point
		Gizmos.DrawSphere(m_Point.position, 0.2f);

		// Projection
		if (Utils.Math.HasPointOnSegmentProjection(m_LineStart.position, m_LineEnd.position, m_Point.position, out Vector3 OutProjection))
		{
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.red))
			{
				Gizmos.DrawLine(m_Point.position, OutProjection);
			}
			Gizmos.DrawSphere(OutProjection, 0.07f);
		}
	}
}
