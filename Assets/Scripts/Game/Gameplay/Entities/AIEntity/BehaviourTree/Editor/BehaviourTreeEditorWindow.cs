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
		private static BehaviourTreeNodeInspectorView m_InspectorView = null;
		private static BehaviourTreeBBInspectorView m_BlackboardInspectorView = null;
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
			m_InspectorView = null;
			m_BlackboardInspectorView = null;
		}

		//////////////////////////////////////////////////////////////////////////
		public static bool TryTransformInBTViewSpace(in Vector2 InScreenMousePosition, out Vector2 OutGraphMousePosition)
		{
			OutGraphMousePosition = default;
			if (Utils.CustomAssertions.IsNotNull(m_Window = m_Window ?? GetWindow<BehaviourTreeEditorWindow>()) && Utils.CustomAssertions.IsNotNull(m_BehaviourTreeView))
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
			// Each editor window contains a root VisualElement object
			VisualElement root = rootVisualElement;

			m_InspectorView = new BehaviourTreeNodeInspectorView()
			{
				name = "NodeInstpectorPanel"
			};
			m_BlackboardInspectorView = new BehaviourTreeBBInspectorView()
			{
				name = "BBInstpectorPanel"
			};
			m_BehaviourTreeView = new BehaviourTreeView();
			{
				m_BehaviourTreeView.focusable = true;
				m_BehaviourTreeView.style.flexGrow = 1f;
			}

			TwoPaneSplitView splitView = new TwoPaneSplitView();
			{
				VisualElement leftElement = new VisualElement() { name = "left-panel" };
				{
					TwoPaneSplitView splitView2 = new TwoPaneSplitView();
					{
						splitView2.Add(m_InspectorView);
						splitView2.Add(m_BlackboardInspectorView);
					}
					leftElement.Add(splitView2);

					splitView2.orientation = TwoPaneSplitViewOrientation.Vertical;
					splitView2.fixedPaneInitialDimension = 200f;
				}
				splitView.Add(leftElement);

				 
				VisualElement rightElement = new VisualElement() { name = "right-panel" };
				{
					rightElement.Add(m_BehaviourTreeView);
				}
				splitView.Add(rightElement);

				splitView.orientation = TwoPaneSplitViewOrientation.Horizontal;
				splitView.fixedPaneInitialDimension = 300f;
			}
			root.Add(splitView);
			OnSelectionChange();
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnSelectionChange()
		{
			m_BehaviourTreeView.InvalidateView();

			BehaviourTree behaviourTree = null;

			// If is passed from outside
			if (m_BehaviourTree.IsNotNull())
			{
				behaviourTree = m_BehaviourTree;
			}
			// If a behaviour tree is selected
			else if (Selection.activeObject is BehaviourTree behaviourTreeFound
			//	&& EditorUtility.IsPersistent(Selection.activeObject)
				)
			{
				behaviourTree = behaviourTreeFound;
			}
			// If an object in hierarchy with AIBehaviorTreeComponent is selected
			else if (Selection.activeGameObject && Selection.activeGameObject.TryGetComponent(out AIBehaviorTreeComponent comp))
			{
				if (comp.BehaviourTreeAsset.IsNotNull())
				{
					behaviourTree = comp.BehaviourTreeAsset;
				}
			}

			if (behaviourTree.IsNotNull())
			{
				m_BehaviourTreeView.PopulateView(behaviourTree, m_InspectorView, m_BlackboardInspectorView);
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
		private async void DelayedOnSelectionChange()
		{
			await System.Threading.Tasks.Task.Delay(200);
			OnSelectionChange();
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnPaymodeStateChanged(PlayModeStateChange obj)
		{
			switch (obj)
			{
				case PlayModeStateChange.EnteredPlayMode:
				case PlayModeStateChange.EnteredEditMode:
				{
					DelayedOnSelectionChange();
					break;
				}
				case PlayModeStateChange.ExitingEditMode:
				{
					break;
				}
				case PlayModeStateChange.ExitingPlayMode:
				{
					break;
				}
			}
		}
	}
}
