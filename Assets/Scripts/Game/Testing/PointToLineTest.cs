using UnityEngine;

public class PointToLineTest : MonoBehaviour
{
	[SerializeField, ReadOnly]
	private Transform m_LineStart = null;
	
	[SerializeField, ReadOnly]
	private Transform m_LineEnd = null;

	[SerializeField, ReadOnly]
	private Transform m_Point = null;

	private Vector3? m_Projection = null;


	private void OnDrawGizmos()
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
		if (m_Point.IsNull())
		{
			CreatePoint(transform, ref m_Point, "Point");
			m_Point.localPosition = Vector3.left;
		}

		m_LineStart.SetSiblingIndex(0);
		m_LineEnd.SetSiblingIndex(1);
		m_Point.SetSiblingIndex(2);

		if (Utils.Math.HasPointOnLineProjection(out Vector3 OutProjection, out float _, m_Point.position, m_LineStart.position, m_LineEnd.position))
		{
			m_Projection = OutProjection;
		}
		else
		{
			m_Projection = null;
		}

		Gizmos.DrawLine(m_LineStart.position, m_LineEnd.position);

		Gizmos.DrawSphere(m_Point.position, 0.2f);

		if (m_Projection.HasValue)	
		{
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.red))
			{
				Gizmos.DrawLine(m_Point.position, m_Projection.Value);
			}
			Gizmos.DrawSphere(m_Projection.Value, 0.07f);
		}
	}

	private static void CreatePoint(in Transform parent, ref Transform InTransform, in string InGOName)
	{
		InTransform = new GameObject(InGOName).transform;
		InTransform.SetParent(parent);
		InTransform.localPosition = Vector3.zero;
	}
}
