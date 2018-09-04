
using UnityEngine;
using System.Collections.ObjectModel;

namespace AI.Pathfinding
{
	public interface IGraphMaker {

		int					NodeCount { get; }

		IAINode				GetNearestNode( Vector3 position );
	}

	public interface IGraphManagerInternal {

		void	ResetNodes();

	}

	public class GraphMaker : MonoBehaviour, IGraphMaker, IGraphManagerInternal
	{

		public		static	IGraphMaker					Instance			= null;
		internal	static	IGraphManagerInternal		Internal			= null;

		private static	AINode[]		m_Nodes				= null;
		public	static	AINode[] Nodes { get { return m_Nodes; } }

		public	bool					IsGraphReady
		{
			get { return m_IsGraphReady; }
		}
		public	int						NodeCount
		{
			get
			{
				if ( m_Nodes != null )
					return m_Nodes.Length;
				return 0;
			}
		}

		private	bool					m_IsGraphReady		= false;

		public static void CollectNodes()
		{
			m_Nodes = FindObjectsOfType<AINode>();
		}

		public static void ClearNodes()
		{
			m_Nodes = null;
		}


		//////////////////////////////////////////////////////////////////////////
		// AWAKE
		private	void	Awake ()
		{
			Instance = this as IGraphMaker;
			Internal = this as IGraphManagerInternal;

			// Find all nodes
			m_Nodes = FindObjectsOfType<AINode>();

			AStarSearch.SetSize( m_Nodes.Length );
		}


		//////////////////////////////////////////////////////////////////////////
		// ONDESTROY
		private void OnDestroy()
		{
			AStarSearch.ClearSet();
		}

		//////////////////////////////////////////////////////////////////////////
		// UpdaeNeighbours
		public static  void	UpdateNeighbours( IAINode iNode, float scanRadius, bool isUpdate )
		{
			if (  iNode is IAINodeLinker )
				return;

			if ( m_Nodes == null )
				m_Nodes = FindObjectsOfType<AINode>();

			// UPDATE PREVIOUS NEIGHBOURS
			if ( isUpdate == true )
			{
				// update previous neighbours
				foreach( IAINode neigh in iNode.Neighbours )
				{
					if ( neigh != null )
						UpdateNeighbours( neigh, scanRadius, false );
				}
			}

			// Get neighbours by distance
			IAINode[] neighbours = System.Array.FindAll<AINode>
			( 
				m_Nodes, 
				n =>
				( n.transform.position - iNode.Position ).sqrMagnitude <= scanRadius * scanRadius
				&& (AINode)n != (AINode)iNode
//				&& Physics.CheckCapsule( iNode.Position, n.transform.position, 0.5f ) == false
				&& Physics.Raycast( iNode.Position, ( n.transform.position - iNode.Position ), scanRadius ) == false
			);

			// create temporary array of neighbours and copy neighbours found
			bool hasLinker = iNode.Linker != null;
			AINode[] nodeNeighbours = new AINode[ neighbours.Length + ( hasLinker ? 1 : 0 ) ];
			System.Array.Copy( neighbours, nodeNeighbours, neighbours.Length );


			// LINKER ASSIGNMENT
			if ( hasLinker )
			{
				// add linker to this node
				nodeNeighbours[ nodeNeighbours.Length - 1 ] = iNode.Linker as AINode;
				
				IAINode			ILinker		= iNode.Linker;

				// resize Neighbours array
				var tmpNeighbours = ILinker.Neighbours;
				System.Array.Resize( ref tmpNeighbours, ( ILinker.Neighbours != null ) ? ILinker.Neighbours.Length + 1 : 1 );
				// add this node to linker
				tmpNeighbours[ tmpNeighbours.Length - 1 ] = iNode as AINode;
				ILinker.Neighbours = tmpNeighbours;
			}
		
			iNode.Neighbours = nodeNeighbours;
			

			// UPDATE CURRENT NEIGHBOURS
			if ( isUpdate == true )
			{
				// update previous neighbours
				foreach( IAINode neigh in iNode.Neighbours )
				{
					if ( neigh != null )
						UpdateNeighbours( neigh, scanRadius, false );
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// GetNearestNode
		IAINode	IGraphMaker.GetNearestNode( Vector3 position )
		{
			float currentDistance = float.MaxValue;
			AINode result = null;

			foreach ( AINode node in Nodes )
			{
				float distance = ( node.transform.position - position ).magnitude;
				if ( distance < currentDistance )
				{
					currentDistance = distance;
					result = node;
				}
			}
			return result as IAINode;
		}


		//////////////////////////////////////////////////////////////////////////
		// ResetCosts
		void	IGraphManagerInternal.ResetNodes()
		{
			foreach ( IAINode node in Nodes )
			{
				node.Heuristic	= 0f;
				node.gCost		= float.MaxValue;
				node.Parent		= null;
				node.Visited	= false;
			}
		}

	}

}