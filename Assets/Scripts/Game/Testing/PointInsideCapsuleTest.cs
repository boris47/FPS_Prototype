using UnityEngine;

public class PointInsideCapsuleTest : TestBase
{
	[SerializeField, ReadOnly]
	private Transform m_Point = null;
	
	[SerializeField, ReadOnly]
	private CapsuleCollider m_Capsule = null;


	protected virtual void OnDrawGizmos()
	{
		if (m_Point.IsNull())
		{
			CreateGOChild(transform, out m_Point, "Point");
			m_Point.localPosition = Vector3.right;
		}

		if (m_Capsule.IsNull())
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			go.transform.SetParent(transform);
			go.transform.localPosition = Vector3.zero;
			m_Capsule = go.GetComponent<CapsuleCollider>();
		}

		m_Point.SetSiblingIndex(0);
		m_Capsule.transform.SetSiblingIndex(1);
		m_Capsule.enabled = false;
		
		m_Capsule.GetPoints(out Vector3 outP1, out Vector3 outP2);

		// Capsule and capsule points
		{
			Utils.Editor.GizmosHelper.DrawCollider(m_Capsule, Color.gray);
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.yellow))
			{
				Gizmos.DrawSphere(outP1, 0.2f);
				Gizmos.DrawSphere(outP2, 0.2f);
			}
		}

		// Point
		Color colorToUse = Utils.Math.IsPointInsideCapsule(outP1, outP2, m_Capsule.radius, m_Point.position) ? Color.green : Color.red;
		using (new Utils.Editor.GizmosHelper.UseGizmoColor(colorToUse))
		{
			Gizmos.DrawSphere(m_Point.position, 0.05f);
		}
	}
}
