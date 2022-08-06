using UnityEngine;

namespace AI.Pathfinding
{
	internal partial class GraphMaker : MonoBehaviour
	{
		public static GraphMaker Instance { get; private set; } = null;

		[SerializeField, ReadOnly]
		private AINode[] m_Nodes = null;

		internal int NodeCount => m_Nodes?.Length ?? 0;


		//////////////////////////////////////////////////////////////////////////
		private void Awake()
		{
			Instance = this;

			// Find all nodes
			m_Nodes = FindObjectsOfType<AINode>();
		}

		//////////////////////////////////////////////////////////////////////////
		internal AINode GetNearestNode(Vector3 position) => m_Nodes.MinBy(node => Vector3.SqrMagnitude(node.Position - position));

		//////////////////////////////////////////////////////////////////////////
		internal void ResetNodes()
		{
			foreach (AINode node in m_Nodes)
			{
				node.Heuristic = 0f;
				node.gCost = float.MaxValue;
				node.Parent = null;
				node.Visited = false;
			}
		}
	}
}
