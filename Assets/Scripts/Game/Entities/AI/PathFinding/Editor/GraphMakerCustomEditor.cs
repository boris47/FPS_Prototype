using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AI.Pathfinding {

	[CustomEditor(typeof(GraphMaker))]
	public class GraphMakerCustomEditor : Editor {

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			GraphMaker graphMaker = ( GraphMaker ) target;

			if ( GUILayout.Button( "Node Count" ) )
			{
				Debug.Log( "Node Count: " + FindObjectsOfType<AINode>().Length );
			}

			if ( GUILayout.Button( "Build" ) )
			{
				List<AINode> nodeList = new List<AINode>( FindObjectsOfType<AINode>() );

				// update All nav mesh volumes
				NavMeshVolume[] volumes = FindObjectsOfType<NavMeshVolume>();
				foreach( NavMeshVolume volume in volumes )
				{
					if ( volume.gameObject.activeSelf )
						volume.UpdateWithNodes( ref nodeList );
				}

				// Set nodes into nodegraph interface
				GraphMaker.CollectNodes();

				// Update neighbours for all nodes
				foreach( AINode node in GraphMaker.Nodes )
				{

					NavMeshVolume volume = node.transform.parent.GetComponent<NavMeshVolume>();
					GraphMaker.UpdateNeighbours( node, volume.StepSize * 1.767f, isUpdate: false );
					EditorUtility.SetDirty( node );
				}
				Debug.Log( "Build done, node count: " + GraphMaker.Nodes.Length );
				EditorUtility.SetDirty( graphMaker );
			}

			if ( GUILayout.Button( "Verify Nodes" ) )
			{
				GraphMaker.CollectNodes();
				for ( int i = GraphMaker.Nodes.Length - 1; i >= 0; i-- )
				{
					IAINode node = GraphMaker.Nodes[i];
					if ( node.Neighbours.Length > 0 )
					{
						bool needsUpdate = false;
						foreach( AINode neigh in node.Neighbours )
						{
							if ( neigh == null )
							{
								Debug.Log( "Invalid neighbour for node " + node.ID );
								needsUpdate = true;
							}
						}

						if ( needsUpdate == true )
						{
							NavMeshVolume volume = ( node as AINode ).transform.parent.GetComponent<NavMeshVolume>();
							GraphMaker.UpdateNeighbours( node, volume.StepSize, isUpdate : true );

							Debug.Log( "Neighbours updated for node " + node.ID );
						}
					}
				}
				Debug.Log( "Verification Completed" );
			}

			if ( GUILayout.Button( "Clear" ) )
			{
				AINode[] Nodes = FindObjectsOfType<AINode>();
				NavMeshVolume[] volumes = FindObjectsOfType<NavMeshVolume>();

				for ( int i = Nodes.Length - 1; i >= 0; i-- )
				{
					AINode node = Nodes[i];
					DestroyImmediate( node.gameObject );
				}

				GraphMaker.ClearNodes();

				foreach( NavMeshVolume volume in volumes )
				{
					volume.Clear();
				}
			}
		}

	}

}