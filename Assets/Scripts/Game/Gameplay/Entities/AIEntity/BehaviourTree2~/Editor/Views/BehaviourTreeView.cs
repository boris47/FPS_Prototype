using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System.Collections.Generic;

namespace Entities.AI.Components.Behaviours
{
	internal class BehaviourTreeView : GraphView
	{
		public static System.Action<NodeViewBase> OnNodeSelected = delegate { };
		public static System.Action<NodeViewBase> OnNodeUnSelected = delegate { };
		private static readonly Dictionary<string, System.Type> m_MappedTypes = new Dictionary<string, System.Type>();
		 
		static BehaviourTreeView()
		{
			foreach(System.Type derivedType in TypeCache.GetTypesDerivedFrom<BTTaskNode>().Where(t => !t.IsAbstract))
			{
				string name = BehaviourTreeEditorUtils.GetInstancePropertyValue<string>(derivedType, "NodeName") ?? derivedType.Name;
				m_MappedTypes.Add($"Actions/{name}", derivedType);
			}

			foreach (System.Type derivedType in TypeCache.GetTypesDerivedFrom<BTCompositeNode>().Where(t => !t.IsAbstract))
			{
				string name = BehaviourTreeEditorUtils.GetInstancePropertyValue<string>(derivedType, "NodeName") ?? derivedType.Name;
				m_MappedTypes.Add($"Composites/{name}", derivedType);
			}

			foreach (System.Type derivedType in TypeCache.GetTypesDerivedFrom<BTDecoratorNode>().Where(t => !t.IsAbstract))
			{
				string name = BehaviourTreeEditorUtils.GetInstancePropertyValue<string>(derivedType, "NodeName") ?? derivedType.Name;
				m_MappedTypes.Add($"Decorators/{name}", derivedType);
			}
		}

		public new class UxmlFactory : UxmlFactory<BehaviourTreeView, UxmlTraits> { }

		public BehaviourTreeInspectorView m_InspectorView = null;
		public BehaviourTree m_BehaviourTree { get; private set; } = null;
		private readonly EdgeConnectorListener m_EdgeConnectorListener = null;

		public BehaviourTreeView()
		{
			OnNodeSelected = delegate (NodeViewBase node) { m_InspectorView?.UpdateSelection(node); };
			OnNodeUnSelected = delegate { m_InspectorView?.ClearSelection(); };

			m_EdgeConnectorListener = new EdgeConnectorListener(this);
			styleSheets.Add(Resources.Load<StyleSheet>("GridBackground"));

			GridBackground grid = new GridBackground();
			{
				grid.StretchToParentSize();
			}
			Insert(0, grid);

			MiniMap miniMap = new MiniMap { anchored = true };
			{
				miniMap.SetPosition(new Rect(10f, 30f, 150f, 100f));
			}
			Add(miniMap);

			Button button = new Button(SaveSource) { text = "Save source" };
			{
				button.transform.position = new Vector3(0f, 0f);
			}
			Add(button);

			this.AddManipulator(new ContentZoomer());
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
		}


		//////////////////////////////////////////////////////////////////////////
		private NodeViewBase FindNodeView(IBTNodeEditorInterface node)
		{
			if (CustomAssertions.IsNotNull(node, $"Cannot retrieve view for node {(node as BTNode)?.name??"Null"}"))
			{
				return GetNodeByGuid(node.Guid) as NodeViewBase;
			}
			return null;
		}

		//////////////////////////////////////////////////////////////////////////
		private void SaveSource()
		{
			m_BehaviourTree?.AsEditorInterface.UpdateSource();
		}

		//////////////////////////////////////////////////////////////////////////
		public void PopulateView(BehaviourTree InBehaviourTree)
		{
			m_BehaviourTree = InBehaviourTree;

			viewTransformChanged -= OnGraphViewTransformChanged;
			graphViewChanged -= OnGraphViewChanged;
			{
				DeleteElements(graphElements.ToList());

				// Restore previous view position
				viewTransform.position = InBehaviourTree.AsEditorInterface.Position;
			}
			graphViewChanged += OnGraphViewChanged;
			viewTransformChanged += OnGraphViewTransformChanged;

			// Ensure we have a root node
			m_BehaviourTree.AsEditorInterface.EnsureRootNode();
			
			// Creates nodes views
			m_BehaviourTree.AsEditorInterface.Nodes.ForEach(n => CreateNodeView(n));

			// Create edges
			m_BehaviourTree.AsEditorInterface.Nodes.ForEach(node =>
			{
				if (node is IParentNode parentNode)
				{
					NodeViewBase parentView = FindNodeView(node);
					if (parentView.IsNotNull())
					{
						parentNode.Children.ForEach(childNode =>
						{
							NodeViewBase childView = FindNodeView(childNode);
							if (childView.IsNotNull())
							{
								AddElement(parentView.ConnectTo(childView));
							}
						});
					}
				}
			});
		}


		//////////////////////////////////////////////////////////////////////////
		internal void InvalidateView()
		{
			graphViewChanged -= OnGraphViewChanged;
			viewTransformChanged -= OnGraphViewTransformChanged;

			m_BehaviourTree = null;
			DeleteElements(graphElements.ToList());
		}


		//////////////////////////////////////////////////////////////////////////
		private void OnGraphViewTransformChanged(GraphView graphView)
		{
			m_BehaviourTree.AsEditorInterface.Position = viewTransform.position;
		}

		
		//////////////////////////////////////////////////////////////////////////
		private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
		{
			bool IsBTInstance = m_BehaviourTree.IsInstance;
			if (!IsBTInstance)
			{
				EditorUtility.SetDirty(m_BehaviourTree);
			}

			if (graphViewChange.elementsToRemove.IsNotNull())
			{
				graphViewChange.elementsToRemove.ForEach(element =>
				{
					if (element is NodeViewBase nodeView)
					{
						m_BehaviourTree.AsEditorInterface.DeleteNode(nodeView.BehaviourTreeNode);
					}

					if (element is Edge edge)
					{
						BTNode parent = edge.output.node.GetBTNode();
						BTNode child = edge.input.node.GetBTNode();
						m_BehaviourTree.AsEditorInterface.RemoveChildFrom(parent, child);
					}
				});
				BehaviourTreeEditorUtils.AssignChildrenIndexes(m_BehaviourTree.AsEditorInterface.RootNode);
			}

			if (graphViewChange.edgesToCreate.IsNotNull())
			{
				graphViewChange.edgesToCreate.ForEach(edge =>
				{
					BTNode parent = edge.output.node.GetBTNode();
					BTNode child = edge.input.node.GetBTNode();
					m_BehaviourTree.AsEditorInterface.AddChildTo(parent, child);
				});
				BehaviourTreeEditorUtils.AssignChildrenIndexes(m_BehaviourTree.AsEditorInterface.RootNode);
			}

			if (graphViewChange.movedElements.IsNotNull())
			{
				BehaviourTreeEditorUtils.AssignChildrenIndexes(m_BehaviourTree.AsEditorInterface.RootNode);
			}
			if (!IsBTInstance)
			{
				AssetDatabase.SaveAssetIfDirty(m_BehaviourTree);
			}

			return graphViewChange;
		}


		//////////////////////////////////////////////////////////////////////////
		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			return ports.ToList()
			.Where(candidatePort => candidatePort.direction != startPort.direction && candidatePort.node != startPort.node)
			.ToList();
		}


		//////////////////////////////////////////////////////////////////////////
		internal NodeViewBase CreateNode(System.Type nodeType, Vector2 position)
		{
			BTNode node = m_BehaviourTree.AsEditorInterface.CreateNode(nodeType);
			
			// Create and add NodeView
			return CreateNodeView(node, position);
		}


		//////////////////////////////////////////////////////////////////////////
		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			if (evt.target is BehaviourTreeView && m_BehaviourTree.IsNotNull())
			{
				Vector2 worldMousePosition = (((Vector3)evt.localMousePosition) - contentViewContainer.transform.position) * 1f / contentViewContainer.transform.scale.x;
				
				evt.menu.AppendAction($"Auto Arrange", _ => AutoArrangeNodes());
				evt.menu.AppendSeparator();

				foreach(KeyValuePair<string, System.Type> keyValues in m_MappedTypes)
				{
					evt.menu.AppendAction(keyValues.Key, action => CreateNode(keyValues.Value, worldMousePosition));
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		private void AutoArrangeNodes()
		{
			Node rootNodeView = GetNodeByGuid(m_BehaviourTree.AsEditorInterface.RootNode.AsEditorInterface.Guid);
			if (CustomAssertions.IsNotNull(rootNodeView))
			{
				BTAutoArrangeHelpers.AutoArrange(rootNodeView);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void BreakpointCheck(BTNode node)
		{
			if (node.AsEditorInterface.HasBreakpoint)
			{
				NodeViewBase nodeView = FindNodeView(node.AsEditorInterface);
				if (CustomAssertions.IsNotNull(nodeView))
				{
					EditorApplication.isPaused = true;
				//	ClearSelection();
				//	AddToSelection(nodeView);
				//	FrameSelection();
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private NodeViewBase CreateNodeView(in BTNode InNode, Vector2? OptPosition = null)
		{
			if (OptPosition.HasValue)
			{
				InNode.AsEditorInterface.Position = OptPosition.Value;
			}

			if (EditorApplication.isPlaying)
			{
				InNode.OnNodeActivation -= BreakpointCheck;
				InNode.OnNodeActivation += BreakpointCheck;

				InNode.OnNodeTermination -= BreakpointCheck;
				InNode.OnNodeTermination += BreakpointCheck;
			}

			NodeViewBase nodeView = NodeViewBase.CreateNodeView(InNode, m_EdgeConnectorListener, m_BehaviourTree.IsInstance);
			if (nodeView.IsNotNull())
			{
				AddElement(nodeView);
			}
			return nodeView;
		}


		//////////////////////////////////////////////////////////////////////////
		public void UpdateNodeState()
		{
			if (EditorApplication.isPlaying || EditorApplication.isPaused)
			{
				nodes.ForEach(n => n.GetAsBTNodeView().UpdateState());
			}
		}
	}
}
