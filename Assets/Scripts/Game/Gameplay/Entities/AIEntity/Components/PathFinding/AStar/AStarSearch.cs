using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

namespace AI.Pathfinding
{
	public static class AStarSearch
	{
		//////////////////////////////////////////////////////////////////////////
		public static bool FindPath(in Vector3 InStartPosition, in Vector3 InEndPosition, out Vector3[] OutPath)
		{
			AINode startNode = GraphMaker.Instance.GetNearestNode(InStartPosition);
			AINode endNode = GraphMaker.Instance.GetNearestNode(InEndPosition);
			return FindPath(startNode, endNode, out OutPath);
		}

		//////////////////////////////////////////////////////////////////////////
		private static bool FindPath(in AINode InStartNode, in AINode InEndNode, out Vector3[] OutPath)
		{
			OutPath = new Vector3[0];
			if (GraphMaker.Instance.NodeCount == 0)
			{
				Debug.Log("AStarSearch::FindPath:Node graph has to be build !!");
				return false;
			}

			if (InStartNode.IsWalkable && InEndNode.IsWalkable)
			{
				InEndNode.gCost = 0;
				InEndNode.Heuristic = (InEndNode.Position - InStartNode.Position).sqrMagnitude;

				Heap<AINode> openSet = new Heap<AINode>(GraphMaker.Instance.NodeCount);

				// First node is always discovered
				openSet.Add(InEndNode);

				// Start scan
				while (openSet.Count > 0)
				{
					AINode currentNode = openSet.RemoveFirst();
					if (currentNode.ID == InStartNode.ID)
					{
						//	Debug.Log("We found the end node!");
						RetracePath(InEndNode, InStartNode, out OutPath);
						return true;
					}

					currentNode.Visited = true;

					AINode[] neighbors = currentNode.Neighbours;
					for (int i = 0, length = neighbors.Length; i < length; i++)
					{
						AINode iNeighbor = neighbors[i];

						// Ignore the neighbor which is already evaluated.
						if (iNeighbor.IsWalkable && !iNeighbor.Visited)
						{
							float gCost = currentNode.gCost + (currentNode.Position - iNeighbor.Position).sqrMagnitude;
							bool notContainsNehigbour = !openSet.Contains(iNeighbor);
							if (gCost < iNeighbor.gCost || notContainsNehigbour)
							{
								iNeighbor.gCost = gCost;
								iNeighbor.Heuristic = (iNeighbor.Position - InStartNode.Position).sqrMagnitude;
								iNeighbor.Parent = currentNode;

								if (notContainsNehigbour)
								{
									openSet.Add(iNeighbor);
								}
							}
						}
					}
				}
			}
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		private static void RetracePath(AINode startNode, AINode endNode, out Vector3[] path)
		{
			List<Vector3> nodes = new List<Vector3>();
			AINode currentNode = endNode;

			while (currentNode.ID != startNode.ID)
			{
				nodes.Add(currentNode.Position);
				currentNode = currentNode.Parent;
			}
			nodes.Add(currentNode.Position);

			GraphMaker.Instance.ResetNodes();
			path = nodes.ToArray();
		}

		//////////////////////////////////////////////////////////////////////////
		private static AINode GetBestNode(IEnumerable<AINode> set, bool useHeuristic)
		{
			return set
				// Only walkable
				.Where(n => n.IsWalkable)
				// Only the best
				.MinBy(n => useHeuristic ? n.gCost + n.Heuristic : n.gCost);
		}
	}
}
