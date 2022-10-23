using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System.Threading.Tasks;

namespace Entities.AI.Components.Behaviours
{
	internal sealed class BehaviourTreeEditorWindow : EditorWindow
	{
		private static BehaviourTreeEditorWindow m_Window = null;
		private static BehaviourTreeView m_BehaviourTreeView = null;
		private static BehaviourTreeInspectorView m_InspectorView = null;
		private static BehaviourTree m_BehaviourTree = null;

		public static async Task OpenWindow(BehaviourTree InBehaviourTree)
		{
			if (m_Window)
			{ 
				m_Window.Close();
				await Task.Delay(100);
			}

			m_Window = GetWindow<BehaviourTreeEditorWindow>();
			m_Window.titleContent = new GUIContent("BehaviorTreeEditor");
			m_BehaviourTree = InBehaviourTree;
		}

		private void OnDestroy()
		{
			m_Window = null;
			m_BehaviourTreeView = null;
			m_InspectorView = null;
		}

		public static bool TryTransformInBTViewSpace(in Vector2 InScreenMousePosition, out Vector2 OutGraphMousePosition)
		{
			OutGraphMousePosition = default;
			if (CustomAssertions.IsNotNull(m_Window) && CustomAssertions.IsNotNull(m_BehaviourTreeView))
			{
				Vector2 worldMousePosition = m_Window.rootVisualElement.ChangeCoordinatesTo(m_Window.rootVisualElement.parent, InScreenMousePosition - m_Window.position.position);
				OutGraphMousePosition = m_BehaviourTreeView.contentViewContainer.WorldToLocal(worldMousePosition);
				return true;
			}
			return false;
		}

		private void OnEnable()
		{
			m_Window = GetWindow<BehaviourTreeEditorWindow>();
			EditorApplication.playModeStateChanged -= OnPaymodeStateChanged;
			EditorApplication.playModeStateChanged += OnPaymodeStateChanged;
		}

		private void OnDisable()
		{
			EditorApplication.playModeStateChanged -= OnPaymodeStateChanged;
		}

		public void CreateGUI()
		{
			// Each editor window contains a root VisualElement object
			VisualElement root = rootVisualElement;

			m_InspectorView = new BehaviourTreeInspectorView();
			{

			}
			m_BehaviourTreeView = new BehaviourTreeView() { m_InspectorView = m_InspectorView };
			{
				m_BehaviourTreeView.focusable = true;
				m_BehaviourTreeView.style.flexGrow = 1f;
			}

			TwoPaneSplitView splitView = new TwoPaneSplitView();
			{
				VisualElement leftElement = new VisualElement() { name = "left-panel" };
				{
					leftElement.Add(m_InspectorView);
				}
				splitView.Add(leftElement);


				VisualElement rightElement = new VisualElement() { name = "right-panel" };
				{
					rightElement.Add(m_BehaviourTreeView);
				}
				splitView.Add(rightElement);

				splitView.fixedPaneInitialDimension = 300;
			}
			root.Add(splitView);
			OnSelectionChange();
		}

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
			else if (Selection.activeGameObject && Selection.activeGameObject.TryGetComponent(out AIBehaviorTreeComponent comp))
			{
				behaviourTree = comp.BehaviourTree;
			}
			if (behaviourTree.IsNotNull())
			{
				m_BehaviourTreeView.PopulateView(behaviourTree);
			}
		}

		private void OnInspectorUpdate()
		{
			if (m_BehaviourTreeView.IsNotNull())
			{
				m_BehaviourTreeView.UpdateNodeState();
			}
		}

		private async void DelayedOnSelectionChange()
		{
			await System.Threading.Tasks.Task.Delay(200);
			OnSelectionChange();
		}

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
