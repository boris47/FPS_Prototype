using Entities.AI.Components.Senses;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class SegmentCapsuleIntersectionTest : MonoBehaviour
{
	[SerializeField, ReadOnly]
	private Transform m_LinePointA = null;

	[SerializeField, ReadOnly]
	private Transform m_LinePointB = null;

	[SerializeField, ReadOnly]
	private CapsuleCollider m_Capsule = null;

	[SerializeField, ReadOnly]
	private Transform m_CapsulePoint1 = null;

	[SerializeField, ReadOnly]
	private Transform m_CapsulePoint2 = null;


	protected virtual void OnDrawGizmos()
	{
		if (m_LinePointA.IsNull())
		{
			CreatePoint(transform, ref m_LinePointA, "LinePointA");
			m_LinePointA.localPosition = Vector3.right;
		}

		if (m_LinePointB.IsNull())
		{
			CreatePoint(transform, ref m_LinePointB, "LinePointB");
			m_LinePointB.localPosition = Vector3.left;
		}

		if (m_Capsule.IsNull())
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			go.transform.SetParent(transform);
			go.transform.localPosition = Vector3.zero;
			m_Capsule = go.GetComponent<CapsuleCollider>();
		}

		if (m_CapsulePoint1.IsNull())
		{
			CreatePoint(transform, ref m_CapsulePoint1, "CapsulePoint1");
		}

		if (m_CapsulePoint2.IsNull())
		{
			CreatePoint(transform, ref m_CapsulePoint2, "CapsulePoint2");
		}

		m_LinePointA.SetSiblingIndex(0);
		m_LinePointB.SetSiblingIndex(1);
		m_Capsule.transform.SetSiblingIndex(2);
		m_Capsule.enabled = false;
		m_CapsulePoint1.SetSiblingIndex(3);
		m_CapsulePoint2.SetSiblingIndex(4);

		m_Capsule.GetPoints(out Vector3 outWorldP1, out Vector3 outWorldP2);
		m_CapsulePoint1.position = outWorldP1;
		m_CapsulePoint2.position = outWorldP2;

		// Capsule and capsule points
		{
			//Utils.Editor.GizmosHelper.DrawCollider(m_Capsule, Color.gray);
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.yellow))
			{
				Gizmos.DrawSphere(m_CapsulePoint1.position, 0.2f);
				Gizmos.DrawSphere(m_CapsulePoint2.position, 0.2f);
			}
		}

		// Line
		{
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.red))
			{
				Gizmos.DrawSphere(m_LinePointA.position, 0.1f);
			}
			Gizmos.DrawLine(m_LinePointA.position, m_LinePointB.position);
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.blue))
			{
				Gizmos.DrawSphere(m_LinePointB.position, 0.1f);
			}
		}

		// Intersection Point
		if (Utils.Math.HasSegmentCapsuleIntersection(m_LinePointA.position, m_LinePointB.position, outWorldP1, outWorldP2, m_Capsule.radius, out Vector3 intersection))
		{
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.green))
			{
				Gizmos.DrawSphere(intersection, 0.15f);
			}
		}
	}

	private static void CreatePoint(in Transform parent, ref Transform InTransform, in string InGOName)
	{
		InTransform = new GameObject(InGOName).transform;
		InTransform.SetParent(parent);
		InTransform.localPosition = Vector3.zero;
	}
}