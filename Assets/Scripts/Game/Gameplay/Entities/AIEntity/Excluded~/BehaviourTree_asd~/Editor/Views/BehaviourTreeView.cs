using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System.Collections.Generic;

namespace Entities.AI.Components.Behaviours
{
	internal interface IBlackboardView
	{
		void UpdateSelection(in Blackboard InBlackboardAsset, in BehaviourTreeInstanceData InBehaviourTreeInstanceData);
		void ClearSelection();
	}

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

		public BehaviourTreeNodeInspectorView InspectorView { get; private set; } = null;
		private IBlackboardView m_BlackboardInterfaceInspectorView = null;
		public BehaviourTree BehaviourTreeAsset { get; private set; } = null;


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
		private bool TryFindNodeView(in BTNode InNode, out NodeViewBase OutViewBase)
		{
			string guid = BTNode.Editor.GetGuid(InNode);
			OutViewBase = GetNodeByGuid(guid) as NodeViewBase;
			return OutViewBase.IsNotNull();
		}

		//////////////////////////////////////////////////////////////////////////
		public void PopulateView(in BehaviourTree InBehaviourTreeAsset, in BehaviourTreeNodeInspectorView InInspectorView, in IBlackboardView InBlackboardInspectorView, in BehaviourTreeInstanceData InBehaviourTreeInstanceData)
		{
			BehaviourTreeAsset = InBehaviourTreeAsset;
			InspectorView = InInspectorView;
			m_BlackboardInterfaceInspectorView = InBlackboardInspectorView;

			m_BlackboardInterfaceInspectorView.UpdateSelection(InBehaviourTreeAsset.BlackboardAsset, InBehaviourTreeInstanceData);

			viewTransformChanged -= OnGraphViewTransformChanged;
			graphViewChanged -= OnGraphViewChanged;
			{
				DeleteElements(graphElements.ToList());

				// Restore previous view position
				viewTransform.position = BehaviourTree.Editor.GetEditorGraphPosition(InBehaviourTreeAsset);
				viewTransform.scale = BehaviourTree.Editor.GetEditorGraphScale(InBehaviourTreeAsset);
			}
			graphViewChanged += OnGraphViewChanged;
			viewTransformChanged += OnGraphViewTransformChanged;

			// Ensure blackboard is initialzied
			BehaviourTree.Editor.EnsureBlackboard(BehaviourTreeAsset);

			// Ensure we have a root node
			BehaviourTree.Editor.EnsureRootNode(BehaviourTreeAsset);
			
			// Creates nodes views
			foreach (BTNode node in BehaviourTreeAsset.AsEditorInterface.Nodes)
			{
				CreateNodeView(node);
			}

			// Create edges
			foreach (BTNode node in BehaviourTreeAsset.AsEditorInterface.Nodes)
			{
				if (node is IParentNode parentNode)
				{
					if (Utils.CustomAssertions.IsTrue(TryFindNodeView(node, out NodeViewBase parentView), null, $"Cannot retrieve view for node {parent?.name ?? "Null"}"))
					{
						foreach (BTNode childNode in parentNode.Children)
						{
							if (Utils.CustomAssertions.IsTrue(TryFindNodeView(childNode, out NodeViewBase childView), null, $"Cannot retrieve view for node {childNode?.name ?? "Null"}"))
							{
								uint portIndex = BTNode.Editor.GetNodeParentPortIndex(childView.BehaviourTreeNode);
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

			BehaviourTreeAsset = null;
			m_BlackboardInterfaceInspectorView?.ClearSelection();
			InspectorView?.ClearSelection();
			DeleteElements(graphElements.ToList());
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnGraphViewTransformChanged(GraphView graphView)
		{
			BehaviourTree.Editor.SetEditorGraphPosition(BehaviourTreeAsset, viewTransform.position);
			BehaviourTree.Editor.SetEditorGraphScale(BehaviourTreeAsset, viewTransform.scale);
		}
		
		//////////////////////////////////////////////////////////////////////////
		private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
		{
			bool bHasElementsToRemove = graphViewChange.elementsToRemove.IsNotNull() && graphViewChange.elementsToRemove.Count > 0;
			bool bHasEdgeToCreate = graphViewChange.edgesToCreate.IsNotNull() && graphViewChange.edgesToCreate.Count > 0;
		//	bool bHasElementsMoved = graphViewChange.movedElements.IsNotNull() && graphViewChange.movedElements.Count > 0;
			if ((bHasElementsToRemove || bHasEdgeToCreate))
			{
				EditorUtility.SetDirty(BehaviourTreeAsset);
			}

			if (bHasElementsToRemove)
			{
				foreach (GraphElement element in graphViewChange.elementsToRemove)
				{
					if (element is NodeViewBase nodeView)
					{
						BehaviourTree.Editor.DeleteNodeAsset(BehaviourTreeAsset, nodeView.BehaviourTreeNode);
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

			if ((bHasElementsToRemove || bHasEdgeToCreate /*|| bHasElementsMoved*/))
			{
				BehaviourTreeEditorUtils.AssignChildrenIndexes(BehaviourTreeAsset.AsEditorInterface.RootNode);
				AssetDatabase.SaveAssetIfDirty(BehaviourTreeAsset);
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
			BTNode node = BehaviourTree.Editor.CreateNodeAsset(BehaviourTreeAsset, nodeType);
			
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
				string guid = BTNode.Editor.GetGuid(BehaviourTreeAsset.AsEditorInterface.RootNode);
				Node rootNodeView = GetNodeByGuid(guid);
				if (Utils.CustomAssertions.IsNotNull(rootNodeView))
				{
					BTAutoArrangeHelpers.AutoArrange(rootNodeView);
				}
			}

			if (evt.target is BehaviourTreeView && BehaviourTreeAsset.IsNotNull())
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
		//	if (node.AsEditorInterface.HasBreakpoint)
		//	{
		//		if (Utils.CustomAssertions.IsTrue(TryFindNodeView(node, out NodeViewBase nodeView), $"Cannot retrieve view for node {node?.name ?? "Null"}"))
		//		{
		//			EditorApplication.isPaused = true;
		//		}
		//	}
		}

		//////////////////////////////////////////////////////////////////////////
		private NodeViewBase CreateNodeView(in BTNode InNode, Vector2? OptPosition = null)
		{
			if (OptPosition.HasValue)
			{
				BTNode.Editor.SetEditorGraphPosition(InNode, OptPosition.Value);
			}

			if (EditorApplication.isPlaying)
			{
		//		InNode.OnNodeActivation -= BreakpointCheck;
		//		InNode.OnNodeActivation += BreakpointCheck;
		//
		//		InNode.OnNodeTermination -= BreakpointCheck;
		//		InNode.OnNodeTermination += BreakpointCheck;
			}

			NodeViewBase nodeView = NodeViewBase.CreateNodeView(InNode, m_EdgeConnectorListener);
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