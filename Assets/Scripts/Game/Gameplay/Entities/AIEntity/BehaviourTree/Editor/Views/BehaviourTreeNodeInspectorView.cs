using UnityEngine.UIElements;
using UnityEditor;

namespace Entities.AI.Components.Behaviours
{
	internal class BehaviourTreeNodeInspectorView : VisualElement
	{
		public new class UxmlFactory : UxmlFactory<BehaviourTreeNodeInspectorView, VisualElement.UxmlTraits> { }

		private Editor editor = null;

		public BehaviourTreeNodeInspectorView()
		{
			Add(new Label("Node inspector"));
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

		public void UpdateSelection(NodeViewBase node)
		{
		//	Clear();
			UnityEngine.Object.DestroyImmediate(editor);
			editor = Editor.CreateEditor(node.BehaviourTreeNode, node.InsideNodeEditorType);
		}

		public void ClearSelection()
		{
			if (editor.IsNotNull())
			{
				UnityEngine.Object.DestroyImmediate(editor);
			}
		}
	}
}
