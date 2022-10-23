
namespace Entities.AI.Components.Behaviours
{
	using UnityEngine;
	using UnityEditor;

	//////////////////////////////////////////////////////////////////////////
	[CustomPropertyDrawer(typeof(BehaviourTree))]
	internal class BehaviourTree_PropertyDrawer : PropertyDrawer
	{
		//////////////////////////////////////////////////////////////////////////
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.PropertyField(position, property, label, true);

			position.y += EditorGUIUtility.singleLineHeight + 5f;
			position.height = EditorGUIUtility.singleLineHeight;

			if (property.objectReferenceValue is BehaviourTree behaviourTree && GUI.Button(position, "Edit Behavior Tree"))
			{
				BehaviourTreeEditorWindow.OpenWindow(behaviourTree);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var BT = property.objectReferenceValue as BehaviourTree;
			return BT ? 48f : base.GetPropertyHeight(property, label);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	[CustomEditor(typeof(BehaviourTree))]
	internal class BehaviourTreeCustomEditor : Editor
	{
		//////////////////////////////////////////////////////////////////////////
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (GUILayout.Button("Edit Behavior Tree"))
			{
				BehaviourTreeEditorWindow.OpenWindow(serializedObject.targetObject as BehaviourTree);
			}
		}
	}
}