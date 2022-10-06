using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;

namespace Entities.AI.Components.Behaviours
{
	internal class BehaviourTreeBBInspectorView : VisualElement
	{
		public new class UxmlFactory : UxmlFactory<BehaviourTreeBBInspectorView, VisualElement.UxmlTraits> { }

		private Editor editor = null;

		public BehaviourTreeBBInspectorView()
		{
			Add(new Label("Blackboard"));
			Add(new IMGUIContainer(RegisterOnInspectorGUI));
		}
		
		private void RegisterOnInspectorGUI()
		{
			if (editor.IsNotNull() && editor.serializedObject.IsNotNull() && editor.serializedObject.targetObject.IsNotNull())
			{
				editor.OnInspectorGUI();
			}
			else
			{
			//	System.Diagnostics.Debugger.Break();
				ClearSelection();
			}
		}

		internal void UpdateSelection(Blackboard blackboard)
		{
		//	Clear();
			UnityEngine.Object.DestroyImmediate(editor);
			editor = Editor.CreateEditor(blackboard);
		}

		internal void ClearSelection()
		{
			if (editor.IsNotNull())
			{
				UnityEngine.Object.DestroyImmediate(editor);
			}
		}
	}
}
