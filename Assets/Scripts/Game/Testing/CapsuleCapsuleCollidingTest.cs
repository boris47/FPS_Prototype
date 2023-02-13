using UnityEngine;

public class CapsuleCapsuleCollidingTest : MonoBehaviour
{
	[SerializeField, ReadOnly]
	private CapsuleCollider m_Capsule1 = null;

	[SerializeField, ReadOnly]
	private CapsuleCollider m_Capsule2 = null;


	protected virtual void OnDrawGizmos()
	{
		if (m_Capsule1.IsNull())
		{
			CreateCapsuleCollider(transform, ref m_Capsule1, "Capsule1");
			m_Capsule1.transform.localPosition = Vector3.right;
			m_Capsule1.transform.SetSiblingIndex(0);
		}

		if (m_Capsule2.IsNull())
		{
			CreateCapsuleCollider(transform, ref m_Capsule2, "Capsule2");
			m_Capsule2.transform.localPosition = Vector3.left;
			m_Capsule2.transform.SetSiblingIndex(1);
		}

		m_Capsule1.enabled = false;
		m_Capsule2.enabled = false;

		m_Capsule1.GetPoints(out Vector3 outWorld1P1, out Vector3 outWorld1P2);
		m_Capsule2.GetPoints(out Vector3 outWorld2P1, out Vector3 outWorld2P2);

		bool bColliding = Utils.Math.IsCapsuleCapsuleColliding(outWorld1P1, outWorld1P2, m_Capsule1.radius, outWorld2P1, outWorld2P2, m_Capsule2.radius, out Vector3 p1, out Vector3 p2);
		{
			Utils.Editor.GizmosHelper.DrawCollider(m_Capsule1, bColliding ? Color.red: Color.green);
			Utils.Editor.GizmosHelper.DrawCollider(m_Capsule2, bColliding ? Color.red: Color.green);

			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.yellow))
			{
				Gizmos.DrawSphere(outWorld1P1, 0.2f);
				Gizmos.DrawSphere(outWorld1P2, 0.2f);

				Gizmos.DrawSphere(outWorld2P1, 0.2f);
				Gizmos.DrawSphere(outWorld2P2, 0.2f);
			}

			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.blue))
			{
				Gizmos.DrawSphere(p1, 0.2f);
				Gizmos.DrawSphere(p2, 0.2f);
			}
		}
	}

	private static void CreateCapsuleCollider(in Transform parent, ref CapsuleCollider InOutData, in string InGOName)
	{
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
		go.name = InGOName;
		go.transform.SetParent(parent);
		go.transform.localPosition = Vector3.zero;
		InOutData = go.GetComponent<CapsuleCollider>();
		go.GetComponent<Renderer>().Destroy();
	}
}