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
		private		static readonly		Dictionary<string, System.Type>			m_MappedTypes			= new Dictionary<string, System.Type>();
		public		static				System.Action<NodeViewBase>				OnNodeSelected			{get; private set;}
		public		static				System.Action<NodeViewBase>				OnNodeUnSelected		{get; private set;}

		//////////////////////////////////////////////////////////////////////////
		static BehaviourTreeView()
		{
			foreach(System.Type derivedType in TypeCache.GetTypesDerivedFrom<BTTaskNode>().Where(t => !t.IsAbstract))
			{
				string name = BehaviourTreeEditorUtils.TryGetNameAndDescription(derivedType, out string OutName, out string OutDescription) ? OutName : derivedType.Name;
				m_MappedTypes.Add($"Actions/{name}", derivedType);
			}

			foreach (System.Type derivedType in TypeCache.GetTypesDerivedFrom<BTCompositeNode>().Where(t => !t.IsAbstract))
			{
				string name = BehaviourTreeEditorUtils.TryGetNameAndDescription(derivedType, out string OutName, out string OutDescription) ? OutName : derivedType.Name;
				m_MappedTypes.Add($"Composites/{name}", derivedType);
			}

			foreach (System.Type derivedType in TypeCache.GetTypesDerivedFrom<BTDecoratorNode>().Where(t => !t.IsAbstract))
			{
				string name = BehaviourTreeEditorUtils.TryGetNameAndDescription(derivedType, out string OutName, out string OutDescription) ? OutName : derivedType.Name;
				m_MappedTypes.Add($"Decorators/{name}", derivedType);
			}
		}

		public new class UxmlFactory : UxmlFactory<BehaviourTreeView, UxmlTraits> { }

		private readonly EdgeConnectorListener m_EdgeConnectorListener = null;

		public BehaviourTreeInspectorView InspectorView { get; private set; } = null;
		public BehaviourTree BehaviourTree { get; private set; } = null;


		//////////////////////////////////////////////////////////////////////////
		public BehaviourTreeView()
		{
			OnNodeSelected = delegate (NodeViewBase node) { InspectorView?.UpdateSelection(node); };
			OnNodeUnSelected = delegate { InspectorView?.ClearSelection(); };

			if (BehaviourTreeEditorUtils.TryGetStyleAssetPath(nameof(BehaviourTreeView), out string path))
			{
				StyleSheet asset = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{path}/GridBackground.uss");
				if (Utils.CustomAssertions.IsNotNull(asset))
				{
					styleSheets.Add(asset);
				}
			}

			m_EdgeConnectorListener = new EdgeConnectorListener(this);

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

		//	Button button = new Button(SaveSource) { text = "Save source" };
		//	{
		//		button.transform.position = new Vector3(0f, 0f);
		//	}
		//	Add(button);

			this.AddManipulator(new ContentZoomer());
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
		}


		//////////////////////////////////////////////////////////////////////////
		private bool TryFindNodeView(in IBTNodeEditorInterface InNode, out NodeViewBase OutViewBase)
		{
			OutViewBase = GetNodeByGuid(InNode.Guid) as NodeViewBase;
			return OutViewBase.IsNotNull();
		}

		//////////////////////////////////////////////////////////////////////////
		private void SaveSource()
		{
			BehaviourTree?.AsEditorInterface.UpdateSource();
		}

		//////////////////////////////////////////////////////////////////////////
		public void PopulateView(in BehaviourTree InBehaviourTree, in BehaviourTreeInspectorView InInspectorView)
		{
			InspectorView = InInspectorView;
			BehaviourTree = InBehaviourTree;

			viewTransformChanged -= OnGraphViewTransformChanged;
			graphViewChanged -= OnGraphViewChanged;
			{
				DeleteElements(graphElements.ToList());

				// Restore previous view position
				viewTransform.position = InBehaviourTree.AsEditorInterface.Position;
				viewTransform.scale = InBehaviourTree.AsEditorInterface.Scale;
			}
			graphViewChanged += OnGraphViewChanged;
			viewTransformChanged += OnGraphViewTransformChanged;

			// Ensure we have a root node
			BehaviourTree.AsEditorInterface.EnsureRootNode();
			
			// Creates nodes views
			foreach (BTNode node in BehaviourTree.AsEditorInterface.Nodes)
			{
				CreateNodeView(node);
			}

			// Create edges
			foreach (BTNode node in BehaviourTree.AsEditorInterface.Nodes)
			{
				if (node is IParentNode parentNode)
				{
					if (Utils.CustomAssertions.IsTrue(TryFindNodeView(node, out NodeViewBase parentView), $"Cannot retrieve view for node {parent?.name ?? "Null"}"))
					{
						foreach (BTNode childNode in parentNode.Children)
						{
							if (Utils.CustomAssertions.IsTrue(TryFindNodeView(childNode, out NodeViewBase childView), $"Cannot retrieve view for node {childNode?.name ?? "Null"}"))
							{
								uint portIndex = childView.BehaviourTreeNode.AsEditorInterface.ParentPortIndex;
								AddElement(parentView.ConnectTo(childView, portIndex));
							}
						}
					}
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		internal void InvalidateView()
		{
			graphViewChanged -= OnGraphViewChanged;
			viewTransformChanged -= OnGraphViewTransformChanged;

			BehaviourTree = null;
			DeleteElements(graphElements.ToList());
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnGraphViewTransformChanged(GraphView graphView)
		{
			BehaviourTree.AsEditorInterface.Position = viewTransform.position;
			BehaviourTree.AsEditorInterface.Scale = viewTransform.scale;
		}
		
		//////////////////////////////////////////////////////////////////////////
		private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
		{
			bool IsBTInstance = BehaviourTree.IsInstance;
			bool bHasElementsToRemove = graphViewChange.elementsToRemove.IsNotNull() && graphViewChange.elementsToRemove.Count > 0;
			bool bHasEdgeToCreate = graphViewChange.edgesToCreate.IsNotNull() && graphViewChange.edgesToCreate.Count > 0;
		//	bool bHasElementsMoved = graphViewChange.movedElements.IsNotNull() && graphViewChange.movedElements.Count > 0;
			if (!IsBTInstance && (bHasElementsToRemove || bHasEdgeToCreate))
			{
				EditorUtility.SetDirty(BehaviourTree);
			}

			if (bHasElementsToRemove)
			{
				foreach (GraphElement element in graphViewChange.elementsToRemove)
				{
					if (element is NodeViewBase nodeView)
					{
						BehaviourTree.AsEditorInterface.DeleteNode(nodeView.BehaviourTreeNode);
					}

					if (element is Edge edge)
					{
						NodeViewBase parentView = edge.output.node.GetAsBTNodeView();
						NodeViewBase childView = edge.input.node.GetAsBTNodeView();
						parentView.DisconnectFrom(childView);
					}
				}
			}

			if (bHasEdgeToCreate)
			{
				foreach (Edge edge in graphViewChange.edgesToCreate)
				{
					NodeViewBase parentView = edge.output.node.GetAsBTNodeView();
					NodeViewBase childView = edge.input.node.GetAsBTNodeView();
					uint portIndex = edge.output.GetBTNodePort().PortIndex;
					parentView.OnEdgeCreation(edge, childView, portIndex);
				};
			}

			//	if (bHasElementsMoved)
			//	{
			//		BehaviourTreeEditorUtils.AssignChildrenIndexes(BehaviourTree.AsEditorInterface.RootNode);
			//	}

			if (!IsBTInstance && (bHasElementsToRemove || bHasEdgeToCreate /*|| bHasElementsMoved*/))
			{
				BehaviourTreeEditorUtils.AssignChildrenIndexes(BehaviourTree.AsEditorInterface.RootNode);
				AssetDatabase.SaveAssetIfDirty(BehaviourTree);
			}
			return graphViewChange;
		}


		//////////////////////////////////////////////////////////////////////////
		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			List<Port> availablePorts = new List<Port>();
			ports.ToList(availablePorts);

			// Filter out not valid ports
			for (int i = availablePorts.Count - 1; i >= 0; i--)
			{
				Port candidatePort = availablePorts[i];
				bool bHasSameDirection = candidatePort.direction == startPort.direction;
				bool bBelongSameNode = candidatePort.node.viewDataKey == startPort.node.viewDataKey;
				bool bCapacityReached = (candidatePort.capacity == Port.Capacity.Single && candidatePort.connected) || (candidatePort.capacity == Port.Capacity.Multi && false);
				if (bHasSameDirection || bBelongSameNode || bCapacityReached)
				{
					availablePorts.RemoveAt(i);
				}
			}

			List<Port> OutPorts = new List<Port>();

			// Verify for allowed node types
			System.Type[] allowedTypes = startPort.GetBTNodePort().PortTypeSelectors;
			if (allowedTypes.IsNotNull())
			{
				for (int i = availablePorts.Count - 1; i >= 0; i--)
				{
					Port eligiblePort = availablePorts[i];
					System.Type candidatePortNodeType = eligiblePort.GetBTNode().GetType();
					foreach (System.Type allowedType in allowedTypes)
					{
						if (ReflectionHelper.IsInerithedFrom(allowedType, candidatePortNodeType))
						{
							OutPorts.Add(eligiblePort);
							break;
						}
					}
				}
			}
			else
			{
				OutPorts.AddRange(availablePorts);
			}

			return OutPorts;
		}


		//////////////////////////////////////////////////////////////////////////
		internal NodeViewBase CreateNode(System.Type nodeType, Vector2 position)
		{
			BTNode node = BehaviourTree.AsEditorInterface.CreateNode(nodeType);
			
			// Create and add NodeView
			return CreateNodeView(node, position);
		}

		//////////////////////////////////////////////////////////////////////////
		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			void CreateNodeAtPosition(DropdownMenuAction actionInfo)
			{
				SearchWindowProvider searchWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
				searchWindowProvider.hideFlags = HideFlags.HideAndDontSave;
				searchWindowProvider.Configure(this);
				SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(actionInfo.eventInfo.mousePosition)), searchWindowProvider);
			}

			void AutoArrangeNodes(DropdownMenuAction actionInfo)
			{
				Node rootNodeView = GetNodeByGuid(BehaviourTree.AsEditorInterface.RootNode.AsEditorInterface.Guid);
				if (Utils.CustomAssertions.IsNotNull(rootNodeView))
				{
					BTAutoArrangeHelpers.AutoArrange(rootNodeView);
				}
			}

			if (evt.target is BehaviourTreeView && BehaviourTree.IsNotNull())
			{
			//	Vector2 worldMousePosition = (((Vector3)evt.localMousePosition) - contentViewContainer.transform.position) * 1f / contentViewContainer.transform.scale.x;
				
				evt.menu.AppendAction($"Auto Arrange", AutoArrangeNodes);
				evt.menu.AppendSeparator();
				evt.menu.AppendAction("Create Node", CreateNodeAtPosition);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void BreakpointCheck(BTNode node)
		{
			if (node.AsEditorInterface.HasBreakpoint)
			{
				if (Utils.CustomAssertions.IsTrue(TryFindNodeView(node, out NodeViewBase nodeView), $"Cannot retrieve view for node {node?.name ?? "Null"}"))
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

			NodeViewBase nodeView = NodeViewBase.CreateNodeView(InNode, m_EdgeConnectorListener, BehaviourTree.IsInstance);
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
