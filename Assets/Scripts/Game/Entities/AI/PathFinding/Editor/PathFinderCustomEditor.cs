using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AI.Pathfinding {

	[CustomEditor(typeof(PathFinder))]
	public class GraphMakerCustomEditor : Editor {

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if ( GUILayout.Button( "Node Count" ) )
			{
				Debug.Log( "Node Count: " + PathFinder.NodeCount );
			}

			if ( GUILayout.Button( "Build" ) )
			{
				if ( PathFinder.Build() )
				{
					Debug.Log( "Build done, node count: " + PathFinder.NodeCount );
				}
			}

			if ( GUILayout.Button( "Verify Nodes" ) )
			{
				
				Debug.Log( "Verification Completed" );
			}

			if ( GUILayout.Button( "Clear" ) )
			{
				PathFinder.ClearAllNodes();
			}
		}

	}

}