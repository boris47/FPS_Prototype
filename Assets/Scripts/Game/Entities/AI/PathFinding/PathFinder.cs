
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
		private		static	Heap<IAINode>				m_OpenSet			= null;
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
		private		static	AINode	GenerateNode( Vector3 Position, NavMeshVolume Volume, bool CanParent )
		{
			// Create AINode
			AINode node = Instantiate( m_NodeModel, Position, Quaternion.identity, ( CanParent ) ? Volume.transform : null );

			node.SetID();

			IAINode iNode = node as IAINode;

			iNode.Volume = Volume;

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
					if ( Volume.IsPositionInside( node.transform.position ) && node.Volume == Volume )
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
				&& ( neigh.transform.position - node.transform.position ).sqrMagnitude <= ( scanRadius * scanRadius ) * 2.4f
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
					( node as IAINode ).Neighbours.Add( neigh );
					( neigh as IAINode ).Neighbours.Add( node );
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		private		static	void	RemoveFromNeighbours( AINode node )
		{
			foreach( IAINode neigh in ( node as IAINode ).Neighbours )
			{
				neigh.Neighbours.Remove( node );
			}
		}


		private	static LayerMask terrainLayer = 0;
		//////////////////////////////////////////////////////////////////////////
		private		static	bool	CanSpawnNode( NavMeshVolume Volume, Vector3 position )
		{
			bool result = true;

			return result;
//			LayerMask validLayers = Physics.AllLayers | terrainLayer;

//			RaycastHit[] hits = Physics.SphereCastAll( position, 1.0f, Vector3.zero, Mathf.Infinity );
//			return hits.Length == 0;

		}


		//////////////////////////////////////////////////////////////////////////
		public		static	bool	Build()
		{
			terrainLayer = LayerMask.NameToLayer( "Terrain" );

			ClearAllNodes();

			CreateNodeModel();

			NavMeshVolume[] volumes = FindObjectsOfType<NavMeshVolume>();

			foreach( NavMeshVolume volume in volumes )
			{
				if ( volume.gameObject.activeSelf )
				{
					for ( int i = m_Nodes.Count - 1; i > -1; i-- )
					{
 						// removing overlapping nodes
 						AINode node = m_Nodes[i];
 						if ( volume.IsPositionInside( node.transform.position ) )
 						{
							ReleaseNode( node, i );
						}
					}

					Quaternion volumetStartRotation = volume.transform.rotation;
					volume.transform.rotation = Quaternion.identity;

					volume.IterateOver( 
						( Vector3 position ) => {
							if ( CanSpawnNode( volume, position ) )
							{
								GenerateNode( position, volume, CanParent: true );
							}
						}
					);

					volume.transform.rotation = volumetStartRotation;
				}
			}
/*
			foreach( NavMeshVolume volume in volumes )
			{
				if ( volume.gameObject.activeSelf )
				{
					for ( int i = m_Nodes.Count - 1; i > -1; i-- )
					{
						// removing overlapping nodes
						AINode node = m_Nodes[i];
						if ( node.Volume.GetInstanceID() != volume.GetInstanceID() && volume.IsPositionInside( node.transform.position ) )
						{
							ReleaseNode( node, i );
						}
					}
				}
			}
*/
			
			foreach ( AINode node in m_Nodes )
			{
				FindNeighbours( node );
			}

			return true;
		}




		//////////////////////////////////////////////////////////////////////////
		private		static	void	SetOpenSetSize( int size )
		{
			if ( m_OpenSet == null || m_OpenSet.Capacity != size )
			{
				m_OpenSet = new Heap<IAINode>( size );
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
		public		static	int 	GetNearestNodeIdx( Vector3 Position )
		{
			float currentDistance = float.MaxValue;
			int result = -1;

			for ( int i = 0; i < m_Nodes.Count; i++ )
			{
				AINode node = m_Nodes[i];

				float distance = ( node.transform.position - Position ).magnitude;
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
		static		public	uint	FindPath( int StartnodeIndex, int EndNodeIndex, ref Vector3[] Path  )
		{
			AINode startNode	= m_Nodes[ StartnodeIndex ];
			AINode endNode		= m_Nodes[ EndNodeIndex ];

			return FindPathInternal( startNode, endNode, ref Path );

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

			return FindPathInternal( startNode, endNode, ref path );
			
		}


		//////////////////////////////////////////////////////////////////////////
		// FindPathInternal
		static		private	uint	FindPathInternal( IAINode StartNode, IAINode EndNode, ref Vector3[] Path )
		{
			if ( EndNode.IsWalkable == false )
				return 0;

			if ( m_OpenSet == null || m_OpenSet.Capacity != NodeCount )
			{
				m_OpenSet = new Heap<IAINode>( NodeCount );

			}

			EndNode.GCost = 0;
			EndNode.Heuristic = ( EndNode.Position - StartNode.Position ).sqrMagnitude;

			// First node is always discovered
			m_OpenSet.Add( EndNode );

//			sw.Reset();
//			sw.Start();

			// Start scan
			while ( m_OpenSet.Count > 0 )
			{
				IAINode currentNode = m_OpenSet.RemoveFirst();
				if ( currentNode.ID == StartNode.ID )
				{
				//	Debug.Log("We found the end node!");
					return RetracePath( EndNode, StartNode, ref Path );
				}

//				if ( currentNode == null )	return null;

				currentNode.Visited = true;

				// Setup its neighbours
				for ( int i = 0; i < currentNode.Neighbours.Count; i++ )
				{
					IAINode iNeighbour = currentNode.Neighbours[ i ];
					if ( iNeighbour == null )
					{
//						UnityEngine.Debug.Log( "node " + currentNode.name + " has neighbour as null " );
						return 0;
					}

					// Ignore the neighbour which is already evaluated.
					if ( iNeighbour.IsWalkable == false ||  iNeighbour.Visited == true )
						continue;


					float gCost = currentNode.GCost + ( currentNode.Position - iNeighbour.Position ).sqrMagnitude;
					bool containsNehigbour = m_OpenSet.Contains( iNeighbour );
					if ( gCost < iNeighbour.GCost || containsNehigbour == false )
					{
						iNeighbour.GCost		= gCost;
						iNeighbour.Heuristic	= ( iNeighbour.Position - StartNode.Position ).sqrMagnitude;
						iNeighbour.Parent		= ( currentNode as AINode );

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
		private		static	uint	RetracePath( IAINode startNode, IAINode endNode, ref Vector3[] path )
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
				node.ResetNode();
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
					Gizmos.DrawSphere( node.transform.position, 0.5f );

					foreach( AINode neigh in ( node as IAINode ).Neighbours )
					{
						Debug.DrawLine( node.transform.position, neigh.transform.position );
					}
				}
			}
		}

	}

}