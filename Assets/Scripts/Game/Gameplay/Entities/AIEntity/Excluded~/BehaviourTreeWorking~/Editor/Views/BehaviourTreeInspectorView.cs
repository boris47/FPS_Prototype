using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;

namespace Entities.AI.Components.Behaviours
{
	internal class BehaviourTreeInspectorView : VisualElement
	{
		public new class UxmlFactory : UxmlFactory<BehaviourTreeInspectorView, VisualElement.UxmlTraits> { }

		private Editor editor = null;

		public BehaviourTreeInspectorView()
		{
			
		}
		
		private void RegisterOnInspectorGUI()
		{
			if (editor.serializedObject?.targetObject.IsNotNull() ?? false)
			{
				editor.OnInspectorGUI();
			}
			else
			{
			//	System.Diagnostics.Debugger.Break();
				ClearSelection();
			}
		}

		internal void UpdateSelection(NodeViewBase node)
		{
			Clear();
			UnityEngine.Object.DestroyImmediate(editor);
			editor = Editor.CreateEditor(node.BehaviourTreeNode);
			IMGUIContainer container = new IMGUIContainer(RegisterOnInspectorGUI);
			Add(container);
		}

		internal void ClearSelection()
		{
			if (editor.IsNotNull())
			{
				Clear();

				UnityEngine.Object.DestroyImmediate(editor);
			}
		}
	}
}
