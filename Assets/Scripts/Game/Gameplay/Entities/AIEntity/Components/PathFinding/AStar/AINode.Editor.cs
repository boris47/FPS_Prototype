#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace AI.Pathfinding
{
	internal partial class AINode
	{
		//////////////////////////////////////////////////////////////////////////
		public void FindNeighbours(in float InRadius, AINode[] InNodes)
		{
			float squaredRadius = InRadius * InRadius;
			m_Neighbors = System.Array.FindAll(InNodes, n => n.ID != m_ID && (n.transform.position - transform.position).sqrMagnitude <= squaredRadius);
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnDrawGizmosSelected()
		{
			if (Selection.activeTransform == transform || GraphMaker.ShowNodesOnParentSelection)
			{
				Gizmos.DrawSphere(transform.position, 0.15f);

				foreach (AINode node in m_Neighbors)
				{
					Gizmos.DrawLine(transform.position, node.transform.position);
					Gizmos.DrawSphere(node.transform.position, 0.15f);
				}
			}
		}
	}
}
#endif
