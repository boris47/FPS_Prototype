using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace Entities.AI.Components.Behaviours
{
	internal sealed class BehaviourTreeEditorWindow : GuardedEditorWindowSimple<BehaviourTreeEditorWindow>
	{
		private static readonly Vector2 s_MinSize = new Vector2(800f, 400f);
		private static readonly Vector2 s_MaxSize = new Vector2(1680f, 1280f);

		private BehaviourTreeView m_BehaviourTreeView = null;
		private BehaviourTreeNodeInspectorView m_BehaviourTreeInspectorView = null;
		private BehaviourTreeBBKeysInspectorView m_BlackboardKeysInspectorView = null;
		private BehaviourTreeBBEntriesInspectorView m_BlackboardEntriesInspectorView = null;
		private BehaviourTree m_BehaviourTree = null;
		

		//////////////////////////////////////////////////////////////////////////
		public static void OpenWindow(BehaviourTree InBehaviourTree)
		{
			if (Utils.CustomAssertions.IsTrue(AssetDatabase.IsMainAsset(InBehaviourTree)))
			{
				BehaviourTreeEditorWindow window = CreateGuardedWindow<BehaviourTreeEditorWindow>("Behaviour Tree Editor", s_MinSize, s_MaxSize);
				{
					window.m_BehaviourTree = InBehaviourTree;
				}
				window.Show();
				window.OnSelectionChange();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public override void SaveBeforeClosingForReload(in WindowData InWindowData)
		{
			base.SaveBeforeClosingForReload(InWindowData);

			if (m_BehaviourTree.IsNotNull())
			{
				InWindowData.SetData("BehaviourTreeAssetPath", AssetDatabase.GetAssetPath(m_BehaviourTree));
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void BeforeWindowRestore(in WindowData InWindowData)
		{
			if (InWindowData.TryGetValue("BehaviourTreeAssetPath", out string OutBehaviourTreeAssetPath))
			{
				m_BehaviourTree = AssetDatabase.LoadAssetAtPath<BehaviourTree>(OutBehaviourTreeAssetPath);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void AfterWindowRestore()
		{
			OnSelectionChange();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();

			EditorApplication.playModeStateChanged -= OnPaymodeStateChanged;
			EditorApplication.playModeStateChanged += OnPaymodeStateChanged;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();

			EditorApplication.playModeStateChanged -= OnPaymodeStateChanged;
		}
		
		//////////////////////////////////////////////////////////////////////////
		public void CreateGUI()
		{
			VisualElement root = rootVisualElement;
			root.Clear();

			TwoPaneSplitView splitHorizontalView = new TwoPaneSplitView();
			{
				VisualElement leftElement = new VisualElement() { name = "left-panel" };
				{
					TwoPaneSplitView splitVerticalView = new TwoPaneSplitView();
					{
						m_BehaviourTreeInspectorView = new BehaviourTreeNodeInspectorView() { name = "NodeInspectorPanel" };

						splitVerticalView.Add(m_BehaviourTreeInspectorView);

						if (EditorApplication.isPlayingOrWillChangePlaymode)
						{
							m_BlackboardEntriesInspectorView = new BehaviourTreeBBEntriesInspectorView() { name = "BBEntriesInspectorPanel" };
							splitVerticalView.Add(m_BlackboardEntriesInspectorView);
						}
						else
						{
							m_BlackboardKeysInspectorView = new BehaviourTreeBBKeysInspectorView() { name = "BBKeysInspectorPanel" };
							splitVerticalView.Add(m_BlackboardKeysInspectorView);
						}
						
						splitVerticalView.orientation = TwoPaneSplitViewOrientation.Vertical;
						splitVerticalView.fixedPaneInitialDimension = 200f;
					}
					leftElement.Add(splitVerticalView);
				}
				 
				VisualElement rightElement = new VisualElement() { name = "right-panel" };
				{
					m_BehaviourTreeView = new BehaviourTreeView();
					{
						m_BehaviourTreeView.focusable = true;
						m_BehaviourTreeView.style.flexGrow = 1f;
					}
					rightElement.Add(m_BehaviourTreeView);
				}

				splitHorizontalView.Add(leftElement);
				splitHorizontalView.Add(rightElement);

				splitHorizontalView.orientation = TwoPaneSplitViewOrientation.Horizontal;
				splitHorizontalView.fixedPaneInitialDimension = 300f;
			}
			root.Add(splitHorizontalView);
		
		//	OnSelectionChange();
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnSelectionChange()
		{
			if (m_BehaviourTreeView.IsNull())
			{
				CreateGUI();
			}

			BehaviourTree behaviourTreeAsset = null;
			BehaviourTreeInstanceData treeInstanceData = null;

			if (Selection.activeGameObject.IsNotNull() && Selection.activeGameObject.TryGetComponent(out AIBehaviorTreeComponent comp) && comp.BehaviourTreeAsset.IsNotNull())
			{
				behaviourTreeAsset = comp.BehaviourTreeAsset;
				treeInstanceData = AIBehaviorTreeComponent.Editor.GetBehaviourTreeInstanceData(comp);
			}
			else if (Selection.activeObject.IsNotNull() && Selection.activeObject is BehaviourTree behaviourTreeFound)
			{
				behaviourTreeAsset = behaviourTreeFound;
			}
		//	else if (m_BehaviourTree.IsNotNull() && EditorUtility.IsPersistent(m_BehaviourTree))
		//	{
		//		behaviourTreeAsset = m_BehaviourTree;
		//	}
			else
			{
				if (m_BehaviourTreeView.IsNotNull())
				{
					m_BehaviourTreeView.InvalidateView();
					m_BehaviourTreeView.ClearSelection();
				}

				(m_BlackboardKeysInspectorView as IBlackboardView)?.ClearSelection();
				(m_BlackboardEntriesInspectorView as IBlackboardView)?.ClearSelection();
			}

			if (behaviourTreeAsset.IsNotNull())
			{
				IBlackboardView blackboardView = EditorApplication.isPlayingOrWillChangePlaymode ? m_BlackboardEntriesInspectorView : m_BlackboardKeysInspectorView;
				m_BehaviourTreeView.PopulateView(this, behaviourTreeAsset, m_BehaviourTreeInspectorView, blackboardView, treeInstanceData);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnInspectorUpdate()
		{
			if (m_BehaviourTreeView.IsNotNull())
			{
				m_BehaviourTreeView.UpdateNodeState();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private async void DelayedOnEditorModeChanged()
		{
			await System.Threading.Tasks.Task.Delay(200);
			CreateGUI();
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnPaymodeStateChanged(PlayModeStateChange obj)
		{
			DelayedOnEditorModeChanged();
		}
	}
}
