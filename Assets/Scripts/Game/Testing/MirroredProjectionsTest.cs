using UnityEngine;

public class MirroredProjectionsTest : TestBase
{
	[SerializeField, ReadOnly]
	private Transform m_Line1P1 = null;

	[SerializeField, ReadOnly]
	private Transform m_Line1P2 = null;

	[SerializeField, ReadOnly]
	private Transform m_Line2P1 = null;

	[SerializeField, ReadOnly]
	private Transform m_Line2P2 = null;


	protected virtual void OnDrawGizmos()
	{
		if (!m_Line1P1) { CreateGOChild(transform, out m_Line1P1, "Line1P1"); m_Line1P1.localPosition = Vector3.forward + Vector3.down; }
		if (!m_Line1P2) { CreateGOChild(transform, out m_Line1P2, "Line1P2"); m_Line1P2.localPosition = Vector3.back; }
		if (!m_Line2P1) { CreateGOChild(transform, out m_Line2P1, "Line2P1"); m_Line2P1.localPosition = Vector3.right; }
		if (!m_Line2P2) { CreateGOChild(transform, out m_Line2P2, "Line2P2"); m_Line2P2.localPosition = Vector3.left; }

		m_Line1P1.SetSiblingIndex(0);
		m_Line1P2.SetSiblingIndex(1);
		m_Line2P1.SetSiblingIndex(2);
		m_Line2P2.SetSiblingIndex(3);

		// Draw the lines
		{
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.yellow))
			{
				Gizmos.DrawSphere(m_Line1P1.position, 0.03f);
				Gizmos.DrawLine(m_Line1P1.position, m_Line1P2.position);
				Gizmos.DrawSphere(m_Line1P2.position, 0.03f);
			}

			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.red))
			{
				Gizmos.DrawSphere(m_Line2P1.position, 0.03f);
				Gizmos.DrawLine(m_Line2P1.position, m_Line2P2.position);
				Gizmos.DrawSphere(m_Line2P2.position, 0.03f);
			}
		}

		// Draw the Mirrored Projections
		if (Utils.Math.AreThereMirroredProjectionsOf(m_Line1P1.position, m_Line1P2.position, m_Line2P1.position, m_Line2P2.position, out Vector3 out1, out Vector3 out2))
		{
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.yellow))
			{
				Gizmos.DrawSphere(out1, 0.03f);
			//	Gizmos.DrawLine(m_Line1P1.position, out1);
			}
	
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.red))
			{
				Gizmos.DrawSphere(out2, 0.03f);
			//	Gizmos.DrawLine(m_Line2P2.position, out2);
			}

			Gizmos.DrawLine(out1, out2);
		}
	}
}