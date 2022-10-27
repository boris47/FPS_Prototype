using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

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

		public EditorWindow EditorWindow { get; private set; } = null;
		public BehaviourTreeNodeInspectorView InspectorView { get; private set; } = null;
		private IBlackboardView m_BlackboardInterfaceInspectorView = null;
		public BehaviourTree BehaviourTreeAsset { get; private set; } = null;
		private BehaviourTreeInstanceData m_BehaviourTreeInstanceData = null;


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
				miniMap.SetPosition(new Rect(10f, 30f, 200f, 200f));
			}
			Add(miniMap);

			this.AddManipulator(new ContentZoomer());
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
		}


		//////////////////////////////////////////////////////////////////////////
		public bool TryFindNodeView(in BTNode InNode, out NodeViewBase OutViewBase)
		{
			string guid = BTNode.Editor.GetGuid(InNode);
			OutViewBase = nodes.FirstOrDefault(n => n.viewDataKey == guid) as NodeViewBase;
			return OutViewBase.IsNotNull();
		}

		//////////////////////////////////////////////////////////////////////////
		public void PopulateView(in EditorWindow InEditorWindow, in BehaviourTree InBehaviourTreeAsset, in BehaviourTreeNodeInspectorView InInspectorView, in IBlackboardView InBlackboardInspectorView, in BehaviourTreeInstanceData InBehaviourTreeInstanceData)
		{
			EditorWindow = InEditorWindow;
			BehaviourTreeAsset = InBehaviourTreeAsset;
			m_BehaviourTreeInstanceData = InBehaviourTreeInstanceData;
			InspectorView = InInspectorView;
			
			viewTransformChanged -= OnGraphViewTransformChanged;
			graphViewChanged -= OnGraphViewChanged;
			{
				DeleteElements(graphElements.ToList());

				// Ensure blackboard is initialzied
				BehaviourTree.Editor.EnsureBlackboard(InBehaviourTreeAsset);

				// Ensure we have a root node
				BehaviourTree.Editor.EnsureRootNode(InBehaviourTreeAsset);

				bool bIsDirty = BehaviourTree.Editor.RemoveInvalidNodes(InBehaviourTreeAsset);

				m_BlackboardInterfaceInspectorView = InBlackboardInspectorView;
				m_BlackboardInterfaceInspectorView.UpdateSelection(InBehaviourTreeAsset.BlackboardAsset, InBehaviourTreeInstanceData);

				BTNode[] nodes = BehaviourTree.Editor.GetAllNodes(InBehaviourTreeAsset);

				for (int i = nodes.Length - 1; i >= 0; i--)
				{
					BTNode node = nodes[i];
					if (node.IsNotNull())
					{
						// Creates node view
						CreateNodeView(node);

						// Create edges
						if (node is IParentNode parentNode)
						{
							if (Utils.CustomAssertions.IsTrue(TryFindNodeView(node, out NodeViewBase parentView), null, $"Cannot retrieve view for node {parent?.name ?? "Null"}"))
							{
								IReadOnlyList<BTNode> children = parentNode.Children;
								for (int ii = children.Count - 1; ii >= 0; --ii)
								{
									BTNode childNode = children[ii];
									if (childNode.IsNotNull())
									{
										if (Utils.CustomAssertions.IsTrue(TryFindNodeView(childNode, out NodeViewBase childView), null, $"Cannot retrieve view for node {childNode.name}"))
										{
											uint portIndex = BTNode.Editor.GetNodeParentPortIndex(childView.BehaviourTreeNode);
											AddElement(parentView.ConnectTo(childView, portIndex));
										}
									}
									else
									{
										if (parentNode is BTCompositeNode asComposite)
										{
											BTCompositeNode.Editor.RemoveInvalidChildAt(asComposite, ii);
											Debug.LogError($"Removing child node at {ii} of {node.name} because is null");
											bIsDirty |= true;
										}
										else
										{
											throw new System.InvalidOperationException($"Impossible to remove child node at {ii} of {node.name} because is null");
										}
									}
								}
							}
						}
					}
				}
				if (bIsDirty)
				{
					BehaviourTreeEditorUtils.UpdateNodeIndexes(this);
				}

				// Restore previous view position and scale
				viewTransform.position = BehaviourTree.Editor.GetEditorGraphPosition(InBehaviourTreeAsset);
				viewTransform.scale = BehaviourTree.Editor.GetEditorGraphScale(InBehaviourTreeAsset);
			}
			graphViewChanged += OnGraphViewChanged;
			viewTransformChanged += OnGraphViewTransformChanged;
		}

		//////////////////////////////////////////////////////////////////////////
		internal void InvalidateView()
		{
			viewTransformChanged -= OnGraphViewTransformChanged;
			graphViewChanged -= OnGraphViewChanged;

			BehaviourTreeAsset = null;
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
			bool bHasElementsMoved = graphViewChange.movedElements.IsNotNull() && graphViewChange.movedElements.Count > 0;
			if ((bHasElementsToRemove || bHasEdgeToCreate || bHasElementsMoved))
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

			if ((bHasElementsToRemove || bHasEdgeToCreate || bHasElementsMoved))
			{
				BehaviourTreeEditorUtils.UpdateNodeIndexes(this);
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
				searchWindowProvider.Configure(EditorWindow, this);
				SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(actionInfo.eventInfo.mousePosition)), searchWindowProvider);
			}

			void AutoArrangeNodes(DropdownMenuAction actionInfo)
			{
				Node rootNodeView = GetNodeByGuid(BTNode.Editor.GetGuid(BehaviourTreeAsset.RootNode));
				if (Utils.CustomAssertions.IsNotNull(rootNodeView))
				{
					BTAutoArrangeHelpers.AutoArrange(rootNodeView);
				}
			}

			if (evt.target is BehaviourTreeView && BehaviourTreeAsset.IsNotNull())
			{
				evt.menu.AppendAction($"Auto Arrange", AutoArrangeNodes);
				evt.menu.AppendSeparator();
				evt.menu.AppendAction("Create Node", CreateNodeAtPosition);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void BreakpointCheck(BTNode node)
		{
			if (BTNode.Editor.HasBreakpoint(node))
			{
				if (Utils.CustomAssertions.IsTrue(TryFindNodeView(node, out NodeViewBase nodeView), $"Cannot retrieve view for node {node.name}"))
				{
					EditorApplication.isPaused = true;
				}
			}
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

			NodeViewBase nodeView = NodeViewBase.CreateNodeView(InNode, this, m_EdgeConnectorListener);
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
				if (m_BehaviourTreeInstanceData.IsNotNull())
				{
					foreach (Node graphNode in nodes)
					{
						NodeViewBase view = graphNode.GetAsBTNodeView();
						BTNodeInstanceData nodeInstanceData = System.Array.Find(m_BehaviourTreeInstanceData.NodesInstanceData, n => n.NodeAsset == view.BehaviourTreeNode);
						if (nodeInstanceData.IsNotNull())
						{
							view.UpdateState(nodeInstanceData);
						}
					}
				}
			}
		}
	}
}
