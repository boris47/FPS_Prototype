using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Diagnostics;

namespace AI.Pathfinding
{

	public static class AStarSearch
	{
//		Stopwatch sw = new Stopwatch();

		static private	Heap<IAINode>			m_OpenSet			= null;

		static private	int						m_PathNodeCount		= 0;

		//////////////////////////////////////////////////////////////////////////
		// SetSize
		static	public	void	SetSize( int size )
		{
			if ( m_OpenSet == null || m_OpenSet.Capacity != size )
			{
				m_OpenSet = new Heap<IAINode>( size );
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// ClearSet
		static	public	void	ClearSet()
		{
			if ( m_OpenSet != null)
			{
				m_OpenSet.Reset();
				m_OpenSet = null;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// FindPath
		static	public	uint	FindPath( Vector3 startPosition, Vector3 endPosition, ref IAINode[] path )
		{
			if ( GraphMaker.Instance == null )
				return 0;

			IAINode startNode	= GraphMaker.Instance.GetNearestNode( startPosition );
			IAINode endNode		= GraphMaker.Instance.GetNearestNode( endPosition );

			if ( startNode.ID == endNode.ID )
				return 0;

			return FindPath( startNode, endNode, ref path );
		}


		//////////////////////////////////////////////////////////////////////////
		// FindPath
		static	public	uint	FindPath( IAINode startNode, IAINode endNode, ref IAINode[] path )
		{
			if ( GraphMaker.Instance == null )
				return 0;

			if ( GraphMaker.Instance.NodeCount == 0 )
			{
				UnityEngine.Debug.Log( "AStarSearch::FindPath:Node graph has to be build !!" );
				return 0;
			}

			if ( endNode.IsWalkable == false )
				return 0;

			if ( m_OpenSet == null || m_OpenSet.Capacity != GraphMaker.Instance.NodeCount )
			{
				m_OpenSet = new Heap<IAINode>( GraphMaker.Instance.NodeCount );

			}

			endNode.gCost = 0;
			endNode.Heuristic = ( endNode.Position - startNode.Position ).sqrMagnitude;

			// First node is always discovered
			m_OpenSet.Add( endNode );

//			sw.Reset();
//			sw.Start();

			// Start scan
			while ( m_OpenSet.Count > 0 )
			{
				IAINode currentNode = m_OpenSet.RemoveFirst();
				if ( currentNode.ID == startNode.ID )
				{
				//	Debug.Log("We found the end node!");
					return RetracePath( endNode, startNode, ref path );
				}

//				if ( currentNode == null )	return null;

				currentNode.Visited = true;

				// Setup its neighbours
				for ( int i = 0; i < currentNode.Neighbours.Length; i++ )
				{
					IAINode iNeighbour = currentNode.Neighbours[ i ];
					if ( iNeighbour == null )
					{
						UnityEngine.Debug.Log( "node " + ( currentNode as AINode ).name + " has neighbour as null " );
						return 0;
					}

					// Ignore the neighbour which is already evaluated.
					if ( iNeighbour.IsWalkable == false ||  iNeighbour.Visited == true )
						continue;


					float gCost = currentNode.gCost + ( currentNode.Position - iNeighbour.Position ).sqrMagnitude;
					bool containsNehigbour = m_OpenSet.Contains( iNeighbour );
					if ( gCost < iNeighbour.gCost || containsNehigbour == false )
					{
						iNeighbour.gCost		= gCost;
						iNeighbour.Heuristic	= ( iNeighbour.Position - startNode.Position ).sqrMagnitude;
						iNeighbour.Parent		= currentNode;

						if ( containsNehigbour == false )
						{
							m_OpenSet.Add( iNeighbour );
							m_PathNodeCount ++;
						}
					}

				}
			}

			// no path found
			return 0;
		}


		//////////////////////////////////////////////////////////////////////////
		// GetBestNode
		static	private	IAINode	GetBestNode( IEnumerable set, bool useHeuristic )
		{
			IAINode bestNode = null;
			float bestTotal = float.MaxValue;

			foreach( IAINode n in set )
			{
				if ( n.IsWalkable == false )
					continue;

				float totalCost = useHeuristic ? n.gCost + n.Heuristic : n.gCost;
				if ( totalCost < bestTotal )
				{
					bestTotal = totalCost;
					bestNode = n;
				}
			}
			return bestNode;
		}


		//////////////////////////////////////////////////////////////////////////
		// FindPath
		static	private	uint	RetracePath( IAINode startNode, IAINode endNode, ref IAINode[] path )
		{
			uint currentNodeCount = 1;
			IAINode currentNode = endNode;
			
			// Count path nodes
			while ( currentNode.Equals( startNode ) == false )
			{
				currentNode = currentNode.Parent;
				currentNodeCount ++;
			}

			// Create path nodes array
			path = new IAINode[ currentNodeCount ];

			// Reset for insert
			currentNodeCount = 0;
			currentNode = endNode;

			// Insert nodes in path
			while ( currentNode.Equals( startNode ) == false )
			{
				path[ currentNodeCount ] = currentNode;
				currentNode = currentNode.Parent;
				currentNodeCount ++;
			}

			// Insert the start node manually
			path[ currentNodeCount ] = startNode;

			// Last node needs to be inserted manually
//			path[ currentNodeCount ] = currentNode;
//			currentNodeCount ++;

//			sw.Stop();
//			print( "Node count: " + currentNodeCount + ", path found in " + sw.ElapsedMilliseconds + "ms" );

			// Reset nodes internals
			GraphMaker.Internal.ResetNodes();
			m_OpenSet.Reset();
			m_OpenSet = null;
			return currentNodeCount + 1;
		}
	}

}