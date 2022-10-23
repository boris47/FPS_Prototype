using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace Entities.AI.Components.Behaviours
{
	internal sealed class BehaviourTreeEditorWindow : GuardedEditorWindow<BehaviourTreeEditorWindow, BehaviourTree>
	{
		private static Vector2 s_MinSize = new Vector2(800f, 400f);
		private static Vector2 s_MaxSize = new Vector2(1680f, 1280f);

		private static BehaviourTreeEditorWindow m_Window = null;
		private static BehaviourTreeView m_BehaviourTreeView = null;
		private static BehaviourTreeNodeInspectorView m_BehaviourTreeInspectorView = null;
		private static BehaviourTreeBBKeysInspectorView m_BlackboardKeysInspectorView = null;
		private static BehaviourTreeBBEntriesInspectorView m_BlackboardEntriesInspectorView = null;
		private static BehaviourTree m_BehaviourTree = null;

		//////////////////////////////////////////////////////////////////////////
		public static void OpenWindow(BehaviourTree InBehaviourTree)
		{
			if (Utils.Paths.TryConvertFromAssetPathToResourcePath(AssetDatabase.GetAssetPath(InBehaviourTree), out string ResourcePath))
			{
				OpenWindow("Behaviour Tree Editor", ResourcePath, InBehaviourTree, s_MinSize, s_MaxSize);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnBeforeShow()
		{
			base.OnBeforeShow();

			m_BehaviourTree = Data;

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
		private void OnDestroy()
		{
			m_Window = null;
			m_BehaviourTreeView = null;
			m_BehaviourTreeInspectorView = null;
			m_BlackboardKeysInspectorView = null;
			m_BlackboardEntriesInspectorView = null;
		}

		//////////////////////////////////////////////////////////////////////////
		public static bool TryTransformInGraphView(in Vector2 InScreenMousePosition, out Vector2 OutGraphMousePosition)
		{
			OutGraphMousePosition = default;
			m_Window = m_Window.IsNotNull() ? m_Window : GetWindow<BehaviourTreeEditorWindow>();
			if (Utils.CustomAssertions.IsNotNull(m_Window) && Utils.CustomAssertions.IsNotNull(m_BehaviourTreeView))
			{
				Vector2 worldMousePosition = m_Window.rootVisualElement.ChangeCoordinatesTo(m_Window.rootVisualElement.parent, InScreenMousePosition - m_Window.position.position);
				OutGraphMousePosition = m_BehaviourTreeView.contentViewContainer.WorldToLocal(worldMousePosition);
				return true;
			}
			return false;
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
		
			OnSelectionChange();
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnSelectionChange()
		{
			if (m_BehaviourTreeView.IsNull())
			{
				CreateGUI();
				return;
			}	
			m_BehaviourTreeView.InvalidateView();

			BehaviourTree behaviourTreeAsset = null;
			BehaviourTreeInstanceData treeInstanceData = null;

			{
				if (Selection.activeGameObject && Selection.activeGameObject.TryGetComponent(out AIBehaviorTreeComponent comp))
				{
					treeInstanceData = AIBehaviorTreeComponent.Editor.GetBehaviourTreeInstanceData(comp);
				}
			}

			{
				// If is passed from outside
				if (m_BehaviourTree.IsNotNull())
				{
					behaviourTreeAsset = m_BehaviourTree;
				}
				// If a behaviour tree is selected
				else if (Selection.activeObject is BehaviourTree behaviourTreeFound
				//	&& EditorUtility.IsPersistent(Selection.activeObject)
					)
				{
					behaviourTreeAsset = behaviourTreeFound;
				}
				// If an object in hierarchy with AIBehaviorTreeComponent is selected
				else if (Selection.activeGameObject && Selection.activeGameObject.TryGetComponent(out AIBehaviorTreeComponent comp))
				{
					if (comp.BehaviourTreeAsset.IsNotNull())
					{
						behaviourTreeAsset = comp.BehaviourTreeAsset;
					}
				}
			}

			if (behaviourTreeAsset.IsNotNull())
			{
				IBlackboardView blackboardView = EditorApplication.isPlayingOrWillChangePlaymode ? m_BlackboardEntriesInspectorView : m_BlackboardKeysInspectorView;
				m_BehaviourTreeView?.PopulateView(behaviourTreeAsset, m_BehaviourTreeInspectorView, blackboardView, treeInstanceData);
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
