using UnityEditor;
using UnityEngine;

namespace Entities.AI.Components.Behaviours.Editor
{
	[CustomEditor(typeof(BehaviourTree))]
	public class BehaviourTreeEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI ();

			if (GUILayout.Button("Open Behaviour Tree"))
			{
				var bt = target as BehaviourTree;
				GraphEditorWindow.Show(bt);
			}
		}
	}

}
