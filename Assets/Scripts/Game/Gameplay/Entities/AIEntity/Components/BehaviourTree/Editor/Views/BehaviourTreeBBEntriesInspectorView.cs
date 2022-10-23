using UnityEngine.UIElements;
using UnityEditor;

namespace Entities.AI.Components.Behaviours
{
	internal class BehaviourTreeBBEntriesInspectorView : VisualElement, IBlackboardView
	{
		public new class UxmlFactory : UxmlFactory<BehaviourTreeBBEntriesInspectorView, VisualElement.UxmlTraits> { }

		private BlackboardInstanceData m_InstanceData = null;
		

		public BehaviourTreeBBEntriesInspectorView()
		{
			Add(new Label("Blackboard Entries"));
			Add(new IMGUIContainer(OnGUIHandler));
		}
		
		private void OnGUIHandler()
		{
			if (m_InstanceData.IsNotNull())
			{
				using (new EditorGUI.DisabledScope(disabled: true))
				{
					using (new EditorGUILayout.VerticalScope())
					{
						foreach (BlackboardEntryBase entry in m_InstanceData.Entries)
						{
							using (new EditorGUILayout.HorizontalScope())
							{
								EditorGUILayout.LabelField(entry.BlackboardEntryKey.Name);

								UnityEngine.GUILayout.FlexibleSpace();

								if (entry.HasValue())
								{
									//BlackboardEntryBase.Editor.OnGUILayoutValue(entry);
								}
								else
								{
									EditorGUILayout.LabelField("No Value");
								}
							}
						}
					}
				}
			}
		}

		void IBlackboardView.UpdateSelection(in Blackboard InBlackboardAsset, in BehaviourTreeInstanceData InBehaviourTreeInstanceData)
		{
			m_InstanceData = null;
			if (InBehaviourTreeInstanceData.IsNotNull())
			{
				m_InstanceData = InBehaviourTreeInstanceData.Blackboard;
				m_InstanceData.OnEntriesUpdate += () => this.MarkDirtyRepaint();
			}
		}

		void IBlackboardView.ClearSelection()
		{
			m_InstanceData = null;
		}
	}
}
