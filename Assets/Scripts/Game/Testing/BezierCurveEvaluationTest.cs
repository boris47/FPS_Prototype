using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BezierCurveEvaluationTest : TestBase
{
	[SerializeField, Range(-1f, 1f)]
	private float m_Time = 0f;

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

		// Waypoints
		using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.yellow))
		{
			foreach (Vector3 position in positions)
			{
				Gizmos.DrawSphere(position, 0.1f);
			}
		}

		// Curve
		using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.red))
		{
			const uint kSteps = 100u;
			foreach (Vector3 worldPosition in Utils.Math.BezierCurve.GetDensePositions(positions, kSteps))
			{
				Gizmos.DrawSphere(worldPosition, 0.015f);
			}
		}

		// Point
		using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.blue))
		{
			if (Utils.Math.BezierCurve.Evaluate(m_Time, positions, out Vector3 OutPosition))
			{
				Gizmos.DrawSphere(OutPosition, 0.1f);
			}
		}
	}
}
