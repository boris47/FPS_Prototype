#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace AI.Pathfinding
{
	internal partial class AINode
	{
		[Header("Editor Only")]
		[Range(0.1f, 100f)]
		private float m_ScanRadius = 1f;

		//////////////////////////////////////////////////////////////////////////
		public void FindNeighbours(in float InRadius, AINode[] nodes = null)
		{
			nodes ??= FindObjectsOfType<AINode>();

			float squaredRadius = InRadius * InRadius;
			m_Neighbors = System.Array.FindAll(nodes, n => n.ID != m_ID && (n.transform.position - transform.position).sqrMagnitude <= squaredRadius);
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnDrawGizmosSelected()
		{
			if (Selection.activeTransform == transform)
			{
				Gizmos.DrawSphere(transform.position, 0.15f);

				foreach (AINode node in m_Neighbors)
				{
					Gizmos.DrawLine(transform.position, node.transform.position);
					Gizmos.DrawSphere(node.transform.position, 0.15f);
				}
			}
		}

		[CustomEditor(typeof(AINode))]
		private class AINodeEditor : Editor
		{
			public override void OnInspectorGUI()
			{
				DrawDefaultInspector();

				AINode thisNode = (AINode)target;
				if (GUILayout.Button("Update Neighbours"))
				{
					thisNode.FindNeighbours(thisNode.m_ScanRadius, null);
				}
			}
		}
	}
}
#endif
