using UnityEngine;
using UnityEditor;

namespace AI.Pathfinding {

	[CustomEditor(typeof(NavMeshVolume))]
	public class NavMeshVolumeCustomEditor : Editor {

		public override void OnInspectorGUI()
		{
			NavMeshVolume navMeshVolume = ( NavMeshVolume ) target;

			if ( GUILayout.Button( "Clear" ) )
			{
				navMeshVolume.Clear();
			}

			DrawDefaultInspector();

		}

	}


}