using UnityEngine.UIElements;
using UnityEditor;

namespace Entities.AI.Components.Behaviours
{
	internal class BehaviourTreeBBKeysInspectorView : VisualElement, IBlackboardView
	{
		public new class UxmlFactory : UxmlFactory<BehaviourTreeBBKeysInspectorView, VisualElement.UxmlTraits> { }

		private Editor m_BBKeysEditor = null;

		public BehaviourTreeBBKeysInspectorView()
		{
			Add(new Label("Blackboard Keys"));
			Add(new IMGUIContainer(OnGUIHandler));
		}
		
		private void OnGUIHandler()
		{
			if (m_BBKeysEditor.IsNotNull() && m_BBKeysEditor.serializedObject.IsNotNull() && m_BBKeysEditor.serializedObject.targetObject.IsNotNull())
			{
				m_BBKeysEditor.OnInspectorGUI();
			}

			//else
			//{
			////	System.Diagnostics.Debugger.Break();
			//	ClearSelection();
			//}
		}

		void IBlackboardView.UpdateSelection(in Blackboard InBlackboardAsset, in BehaviourTreeInstanceData InBehaviourTreeInstanceData)
		{
			if (InBehaviourTreeInstanceData.IsNotNull())
			{
				
			}
			UnityEngine.Object.DestroyImmediate(m_BBKeysEditor);
			m_BBKeysEditor = Editor.CreateEditor(InBlackboardAsset);
		}

		void IBlackboardView.ClearSelection()
		{
			if (m_BBKeysEditor.IsNotNull())
			{
				UnityEngine.Object.DestroyImmediate(m_BBKeysEditor);
			}
		}
	}
}
