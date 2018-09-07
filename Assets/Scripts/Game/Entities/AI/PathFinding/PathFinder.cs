
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;


namespace AI.Pathfinding
{


	[ExecuteInEditMode]
	public class PathFinder : MonoBehaviour
	{
		private	const	string	NODE_PREFAB_PATH = "Prefabs/AI/AINode";

		// Node graph
		public		static	int							NodeCount			{ get { return ( m_Nodes != null ) ? m_Nodes.Count : 0; } }
		private		static	AINode						m_NodeModel			= null;
		private		static	List<AINode>				m_Nodes				= new List<AINode>();

		// Pathfinding
		private		static	Heap<AINode>				m_OpenSet			= null;
		private		static	int							m_PathNodeCount		= 0;


		//////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////
		private		static	void	CreateNodeModel()
		{
			if ( m_NodeModel == null )
			{
				m_NodeModel = Resources.Load<AINode>( NODE_PREFAB_PATH );
			}
		}


		//////////////////////////////////////////////////////////////////////////
		private		static	AINode	GenerateNode( Vector3 Position, NavMeshVolume Volume )
		{
			// Create AINode
			AINode node = Instantiate( m_NodeModel, Position, Quaternion.identity );

			node.SetID();

			node.Volume = Volume;

			node.transform.SetParent( Volume.transform );

			m_Nodes.Add( node );

			return node;
		}


		//////////////////////////////////////////////////////////////////////////
		private		static	void	ReleaseNode( AINode node, int index )
		{
			RemoveFromNeighbours( node );

			node.transform.SetParent( null );

			m_Nodes.RemoveAt( index );

			DestroyImmediate( node.gameObject );
		}


		//////////////////////////////////////////////////////////////////////////
		public		static	void	ReleaseNodesByIDs( List<uint> NodesIDs )
		{
			for ( int i = m_Nodes.Count - 1; i > -1; i-- )
			{
				AINode node = m_Nodes[i];

				ReleaseNode( node, i );
			}
		}


		//////////////////////////////////////////////////////////////////////////
		public	static	void	ReleaseNodesByVolume( NavMeshVolume Volume )
		{
			if ( Volume.gameObject.activeSelf )
			{
				// removing overlapping nodes
				for ( int i = m_Nodes.Count - 1; i > -1; i-- )
				{
					AINode node = m_Nodes[i];
					if ( Volume.IsPositionInside( node.Position ) && node.Volume == Volume )
					{
						ReleaseNode( node, i );
					}
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		public		static	void	ClearAllNodes()
		{
			for ( int i = m_Nodes.Count - 1; i > -1; i-- )
			{
				AINode node = m_Nodes[i];

				ReleaseNode( node, i );
			}

			m_NodeModel = null;
		}


		//////////////////////////////////////////////////////////////////////////
		private		static	bool	IsValidNeighbour( AINode node, AINode neigh, float scanRadius )
		{
			Vector3 position1Up = node.transform.position  + node.transform.up  * 0.6f;
			Vector3 position2Up = neigh.transform.position + neigh.transform.up * 0.6f;

			return
				node.ID != neigh.ID
				&& ( neigh.Position - node.Position ).sqrMagnitude <= ( scanRadius * scanRadius ) * 2.4f
				&& Physics.Linecast( position1Up, position2Up ) == false
			;
		}


		//////////////////////////////////////////////////////////////////////////
		private		static	void	FindNeighbours( AINode node )
		{
			float scanRadius = node.Volume.StepSize;

			foreach( AINode neigh in m_Nodes )
			{
				if ( IsValidNeighbour( node, neigh, scanRadius ) == true )
				{
					node.Neighbours.Add( neigh );
					neigh.Neighbours.Add( node );
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		private		static	void	RemoveFromNeighbours( AINode node )
		{
			foreach( AINode neigh in node.Neighbours )
			{
				neigh.Neighbours.Remove( node );
			}
		}


		private	static LayerMask terrainLayer = 0;
		//////////////////////////////////////////////////////////////////////////
		private		static	bool	CanSpawnNode( Vector3 position )
		{
			LayerMask validLayers = Physics.AllLayers | terrainLayer;

			RaycastHit[] hits = Physics.SphereCastAll( position, 1.0f, Vector3.zero, Mathf.Infinity );
			return hits.Length == 0;

		}


		//////////////////////////////////////////////////////////////////////////
		public		static	bool	Build()
		{
			terrainLayer = LayerMask.NameToLayer( "Terrain" );

			ClearAllNodes();

			CreateNodeModel();

			foreach( NavMeshVolume volume in FindObjectsOfType<NavMeshVolume>() )
			{
				if ( volume.gameObject.activeSelf )
				{
					// removing overlapping nodes
					for ( int i = m_Nodes.Count - 1; i > -1; i-- )
					{
						AINode node = m_Nodes[i];
						if ( volume.IsPositionInside( node.Position ) && node.Volume != volume )
						{
							ReleaseNode( node, i );
						}
					}

					Quaternion volumetStartRotation = volume.transform.rotation;
					volume.transform.rotation = Quaternion.identity;

					volume.IterateOver( 
						( Vector3 position ) => {
							if ( CanSpawnNode( position ) )
							{
								AINode node =  GenerateNode( position, volume );
								FindNeighbours( node );
							}
						} 
					);

					volume.transform.rotation = volumetStartRotation;
				}
			}

			return true;
		}




		//////////////////////////////////////////////////////////////////////////
		private		static	void	SetOpenSetSize( int size )
		{
			if ( m_OpenSet == null || m_OpenSet.Capacity != size )
			{
				m_OpenSet = new Heap<AINode>( size );
			}
		}


		//////////////////////////////////////////////////////////////////////////
		public		static	void	ClearSet()
		{
			if ( m_OpenSet != null)
			{
				m_OpenSet.Reset();
				m_OpenSet = null;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		private		static	int 	GetNearestNodeIdx( Vector3 Position )
		{
			float currentDistance = float.MaxValue;
			int result = -1;

			for ( int i = 0; i < m_Nodes.Count; i++ )
			{
				AINode node = m_Nodes[i];

				float distance = ( node.Position - Position ).magnitude;
				if ( distance < currentDistance )
				{
					currentDistance = distance;
					result = i;
				}
			}

			return result;
		}

		//////////////////////////////////////////////////////////////////////////
		// FindPath
		static		public	uint	FindPath( Vector3 startPosition, Vector3 endPosition, ref Vector3[] path )
		{
			int startNodeID		= GetNearestNodeIdx( startPosition );
			int endNodeID		= GetNearestNodeIdx( endPosition );

			if ( startNodeID == endNodeID )
				return 0;

			if ( NodeCount == 0 )
			{
				UnityEngine.Debug.Log( "PathFinder::FindPath:Node graph has to be build !!" );
				return 0;
			}

			AINode startNode	= m_Nodes[ startNodeID ];
			AINode endNode		= m_Nodes[ endNodeID ];

			if ( endNode.IsWalkable == false )
				return 0;

			if ( m_OpenSet == null || m_OpenSet.Capacity != NodeCount )
			{
				m_OpenSet = new Heap<AINode>( NodeCount );

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
				AINode currentNode = m_OpenSet.RemoveFirst();
				if ( currentNode.ID == startNode.ID )
				{
				//	Debug.Log("We found the end node!");
					return RetracePath( endNode, startNode, ref path );
				}

//				if ( currentNode == null )	return null;

				currentNode.Visited = true;

				// Setup its neighbours
				for ( int i = 0; i < currentNode.Neighbours.Count; i++ )
				{
					AINode iNeighbour = currentNode.Neighbours[ i ];
					if ( iNeighbour == null )
					{
						UnityEngine.Debug.Log( "node " + currentNode.name + " has neighbour as null " );
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
		private		static	uint	RetracePath( AINode startNode, AINode endNode, ref Vector3[] path )
		{
			uint currentNodeCount = 1;
			AINode currentNode = endNode;
			
			// Count path nodes
			while ( currentNode.Equals( startNode ) == false )
			{
				currentNode = currentNode.Parent;
				currentNodeCount ++;
			}

			// Create path nodes array
			path = new Vector3[ currentNodeCount ];

			// Reset for insert
			currentNodeCount = 0;
			currentNode = endNode;

			// Insert nodes in path
			while ( currentNode.Equals( startNode ) == false )
			{
				path[ currentNodeCount ] = currentNode.Position;
				currentNode = currentNode.Parent;
				currentNodeCount ++;
			}

			// Insert the start node manually
			path[ currentNodeCount ] = startNode.Position;

			// Last node needs to be inserted manually
//			path[ currentNodeCount ] = currentNode;
//			currentNodeCount ++;

//			sw.Stop();
//			print( "Node count: " + currentNodeCount + ", path found in " + sw.ElapsedMilliseconds + "ms" );

			// Reset nodes internals
			ResetNodes();
			m_OpenSet.Reset();
			m_OpenSet = null;
			return currentNodeCount + 1;
		}


		//////////////////////////////////////////////////////////////////////////
		private		static	void	ResetNodes()
		{
			foreach ( AINode node in m_Nodes )
			{
				node.Heuristic	= 0f;
				node.gCost		= float.MaxValue;
				node.Parent		= null;
				node.Visited	= false;
			}
		}

















		//////////////////////////////////////////////////////////////////////////
		private		void	Awake ()
		{
			// Find all nodes
			m_Nodes = new List<AINode>( FindObjectsOfType<AINode>() );

			SetOpenSetSize( m_Nodes.Count );
		}


		//////////////////////////////////////////////////////////////////////////
		private		void	OnEnable()
		{
			m_Nodes = new List<AINode>( FindObjectsOfType<AINode>() );
		}


		//////////////////////////////////////////////////////////////////////////
		private		void	OnDestroy()
		{
			ClearSet();
		}


		//////////////////////////////////////////////////////////////////////////
		private		void	OnDrawGizmosSelected()
		{
			foreach( AINode node in m_Nodes )
			{
				if ( node != null && node.transform != null )
				{
					Gizmos.DrawSphere( node.Position, 0.5f );

					foreach( AINode neigh in node.Neighbours )
					{
						Debug.DrawLine( node.Position, neigh.Position );
					}
				}
			}
		}

	}

}