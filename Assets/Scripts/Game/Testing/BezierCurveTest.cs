using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BezierCurveTest : MonoBehaviour
{
	[SerializeField, Min(1f)]
	private uint m_Steps = 5u;

	[SerializeField]
	private GameObject m_ToProject = null;

	//////////////////////////////////////////////////////////////////////////
	private Vector3[] CollectChildrenPosition()
	{
		List<Transform> transforms = new List<Transform>(transform.childCount);
		foreach (Transform item in transform)
		{
			if (item.gameObject.activeInHierarchy)
			{
				transforms.Add(item);
			}
		}
		return transforms.Select(t => t.position).ToArray();
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnDrawGizmos()
	{
		Vector3[] positions = CollectChildrenPosition();

		using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.yellow))
		{
			foreach (Vector3 position in positions)
			{
				Gizmos.DrawSphere(position, 0.2f);
			}
		}

		using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.red))
		{
			foreach (Vector3 worldPosition in Utils.Math.BezierCurve.GetDensePositions(positions, m_Steps))
			{
				Gizmos.DrawSphere(worldPosition, 0.1f);
			}
		}

		if (m_ToProject)
		{
			using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.blue))
			{
				Gizmos.DrawSphere(m_ToProject.transform.position, 0.25f);
			}

			Vector3 outPos = Utils.Math.BezierCurve.ClosestPointOnBezier(m_ToProject.transform.position, positions);
			Gizmos.DrawLine(m_ToProject.transform.position, outPos);
		}
	}
}
