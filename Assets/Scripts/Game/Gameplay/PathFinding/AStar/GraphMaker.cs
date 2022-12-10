using UnityEngine;

namespace AI.Pathfinding
{
	public partial class GraphMaker : MonoBehaviour
	{
		[SerializeField, ReadOnly]
		private AINode[] m_Nodes = null;

		[SerializeField]
		private ProviderBase m_NodesProvider = null;

		internal int NodeCount => m_Nodes.Length;
		internal AINode[] Nodes => m_Nodes;


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
