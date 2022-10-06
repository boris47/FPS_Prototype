using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Entities.AI.Components.Behaviours
{
	internal abstract class NodeViewBase : Node
	{
		protected readonly		BTNode						m_BehaviourTreeNode				= null;
		protected readonly		VisualElement				m_Aux							= null;
		protected readonly		Label						m_NodeIndexLabel				= null;
		protected readonly		Port						m_Input							= null;
		protected readonly		List<Port>					m_Outputs						= null;

		private readonly		EdgeConnectorListener		m_EdgeConnectorListener			= null;
		private readonly		Editor						m_InsideNodeEditor				= null;


		public					Port						InputPort						=> m_Input;
		public					BTNode						BehaviourTreeNode				=> m_BehaviourTreeNode;
		public					System.Type					InsideNodeEditorType			{ get; } = typeof(BTNode.BTNodeEditor);
		
		private static readonly string k_BehaviourTreeNodeViewPath = "Assets/Resources/Editor/BehaviourTree/BehaviourTreeNodeView.uxml";

		static NodeViewBase()
		{
			if (BehaviourTreeEditorUtils.TryGetStyleAssetPath(nameof(NodeViewBase), out string path))
			{
				k_BehaviourTreeNodeViewPath = $"{path}/BehaviourTreeNodeView.uxml";
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public NodeViewBase(in BTNode InNode, in EdgeConnectorListener InEdgeConnectorListener) : base(k_BehaviourTreeNodeViewPath)
		{
			m_BehaviourTreeNode = InNode;
			m_EdgeConnectorListener = InEdgeConnectorListener;

			if (BehaviourTreeEditorUtils.TryGetNameAndDescription(InNode.GetType(), out string OutName, out string OutDescription))
			{
				title = OutName;
				tooltip = OutDescription;
			}
			else
			{
				title = InNode.GetType().Name;
			//	tooltip = "No Description";
			}
			viewDataKey = InNode.AsEditorInterface.Guid;

			// Remove collapsible capability
			this.capabilities &= ~Capabilities.Collapsible;

			// Search for node elements
			m_Aux = this.Q("aux");
			Utils.CustomAssertions.IsNotNull(m_Aux);
			m_NodeIndexLabel = this.Q<Label>("nodeIndex");
			Utils.CustomAssertions.IsNotNull(m_NodeIndexLabel);

			// Create and add node editor
			if (InsideNodeEditorType.IsNotNull())
			{
				void RegisterOnInspectorGUI()
				{
					if (m_InsideNodeEditor.serializedObject?.targetObject.IsNotNull() ?? false)
					{
						m_InsideNodeEditor.OnInspectorGUI();
					}
					else
					{
						if (m_InsideNodeEditor.IsNotNull())
						{
							Clear();

							UnityEngine.Object.DestroyImmediate(m_InsideNodeEditor);
						}
					}
				}

				m_Aux.Clear();
				if (m_InsideNodeEditor.IsNotNull())
				{
					UnityEngine.Object.DestroyImmediate(m_InsideNodeEditor);
				}
				m_InsideNodeEditor = Editor.CreateEditor(m_BehaviourTreeNode, InsideNodeEditorType);
				IMGUIContainer container = new IMGUIContainer(RegisterOnInspectorGUI);
				m_Aux.Add(container);
			}

			// Set node position 
			{
				Rect p = GetPosition();
				p.xMin = InNode.AsEditorInterface.Position.x;
				p.yMin = InNode.AsEditorInterface.Position.y;
				SetPosition(p);
			}

			// Creates node ports
			m_Input = CreateInputPort();
			m_Outputs = CreateOutputPorts();
			RefreshPorts();

			// If input node has breakpoint set, add class
			if (m_BehaviourTreeNode.AsEditorInterface.HasBreakpoint)
			{
				AddToClassList("breakpointActive");
			}
			EditorUtility.SetDirty(m_BehaviourTreeNode);

			// Register callback for update the node view (BehaviourTreeEditorUtils.AssignChildrenIndexes, ...)
			m_BehaviourTreeNode.AsEditorInterface.UpdateView = UpdateView;

			// Immediately get data and update node view
			UpdateView();
		}

		//////////////////////////////////////////////////////////////////////////
		protected abstract Port CreateInputPort();
		protected abstract List<Port> CreateOutputPorts();

		//////////////////////////////////////////////////////////////////////////
		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			bool bHasBreakPoint = m_BehaviourTreeNode.AsEditorInterface.HasBreakpoint;
			void ToggleBreakpoint(IBTNodeEditorInterface node)
			{
				node.HasBreakpoint = !bHasBreakPoint;
				if (m_BehaviourTreeNode.AsEditorInterface.HasBreakpoint)
				{
					AddToClassList("breakpointActive");
				}
				else
				{
					RemoveFromClassList("breakpointActive");
				}
			}

			evt.menu.AppendAction( $"{(bHasBreakPoint ? "Remove" : "Set")} Breakpoint", _ => ToggleBreakpoint(m_BehaviourTreeNode.AsEditorInterface));
			evt.menu.AppendSeparator();
			evt.menu.AppendAction("Edit Script", _ => AssetDatabase.OpenAsset(MonoScript.FromScriptableObject(m_BehaviourTreeNode)));
			evt.menu.AppendSeparator();
		}

		//////////////////////////////////////////////////////////////////////////
		protected NodeViewPort CreatePort(in Orientation InOrientation, in Direction InDirection, in Port.Capacity InCapacity, in BTNode InNode, in uint InPortIndex = 0u, in System.Type[] InPortTypeSelectors = null)
		{
			return NodeViewPort.Create(InOrientation, InDirection, InCapacity, m_EdgeConnectorListener, InNode, InPortIndex, InPortTypeSelectors);
		}

		//////////////////////////////////////////////////////////////////////////
		// On edge creation
		public void OnEdgeCreation(in Edge InEdge, in NodeViewBase InChild, in uint? InPortIndex = null)
		{
			NodeViewBase parentView = InEdge.output.node.GetAsBTNodeView();
			NodeViewBase childView = InEdge.input.node.GetAsBTNodeView();
			Utils.CustomAssertions.IsTrue(parentView.GetBTNode() == m_BehaviourTreeNode);
			Utils.CustomAssertions.IsTrue(InChild.GetBTNode() == childView.BehaviourTreeNode);

			// Add node to parent
			m_BehaviourTreeNode.BehaviourTree.AsEditorInterface.AddChildTo(parentView.BehaviourTreeNode, childView.BehaviourTreeNode, InPortIndex);

			// Set parent output port to child parent port index
			InChild.BehaviourTreeNode.AsEditorInterface.ParentPortIndex = InPortIndex ?? InEdge.output.GetBTNodePort().PortIndex;

			// Notify this node of the edge creation
			OnChildConnected(childView.BehaviourTreeNode, InEdge);
		}

		//////////////////////////////////////////////////////////////////////////
		// On view population
		public Edge ConnectTo(in NodeViewBase InChild, in uint InPortIndex)
		{
			if (m_Outputs.IsValidIndex(InPortIndex))
			{
				Edge edgeCreated = m_Outputs.At(InPortIndex).ConnectTo(InChild.InputPort);
			//	OnChildConnected(InChild.GetBTNode(), edgeCreated);
				return edgeCreated;
			}
			throw new System.ArgumentException($"Invalid index: {InPortIndex}", nameof(InPortIndex));
		}

		//////////////////////////////////////////////////////////////////////////
		public void DisconnectFrom(in NodeViewBase InChild)
		{
			BTNode parentNode = m_BehaviourTreeNode;
			BTNode childNode = InChild.BehaviourTreeNode;

			// Remove this node from parent children
			m_BehaviourTreeNode.BehaviourTree.AsEditorInterface.RemoveChildFrom(parentNode, childNode);

			// Disconnect edge from port
			m_Outputs.At(InChild.BehaviourTreeNode.AsEditorInterface.ParentPortIndex).Disconnect(InChild.InputPort.connections.First());

			// Notify this node of the edge deletion
			OnChildDisconnected(childNode);
			childNode.AsEditorInterface.ParentPortIndex = 0u;
		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnChildConnected(in BTNode InChild, in Edge InEdgeCreated) { }
		protected virtual void OnChildDisconnected(in BTNode InChild) { }

		//////////////////////////////////////////////////////////////////////////
		private void UpdateView()
		{
			m_NodeIndexLabel.text = m_BehaviourTreeNode.NodeIndex.ToString("D2");
		}

		//////////////////////////////////////////////////////////////////////////
		public sealed override void SetPosition(Rect newPos)
		{
			base.SetPosition(newPos);
			m_BehaviourTreeNode.AsEditorInterface.Position = new Vector2(newPos.xMin, newPos.yMin);
			EditorUtility.SetDirty(m_BehaviourTreeNode);
		}

		//////////////////////////////////////////////////////////////////////////
		public sealed override void OnSelected()
		{
			base.OnSelected();
			BehaviourTreeView.OnNodeSelected(this);
		}

		//////////////////////////////////////////////////////////////////////////
		public sealed override void OnUnselected()
		{
			BehaviourTreeView.OnNodeUnSelected(this);
			base.OnUnselected();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnPortRemoved(Port port)
		{
			base.OnPortRemoved(port);
		}

		//////////////////////////////////////////////////////////////////////////
		public void UpdateState()
		{
			const string inactiveState = "inactive";
			const string succeededState = "succeeded";
			const string failedState = "failed";
			const string runningState = "running";

			RemoveFromClassList(inactiveState);
			RemoveFromClassList(succeededState);
			RemoveFromClassList(failedState);
			RemoveFromClassList(runningState);

			if (EditorApplication.isPlaying || EditorApplication.isPaused)
			{
				switch (m_BehaviourTreeNode.NodeState)
				{
					case EBTNodeState.INACTIVE:
					{
						AddToClassList(inactiveState);
						break;
					}
					case EBTNodeState.SUCCEEDED:
					{
						AddToClassList(succeededState);
						break;
					}
					case EBTNodeState.FAILED:
					{
						AddToClassList(failedState);
						break;
					}
					case EBTNodeState.RUNNING:
					{
						AddToClassList(runningState);
						break;
					}
				}

				if (m_BehaviourTreeNode.AsEditorInterface.HasBreakpoint)
				{
					AddToClassList("breakpointActive");
				}
				else
				{
					RemoveFromClassList("breakpointActive");
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public static NodeViewBase CreateNodeView(in BTNode InNode, in EdgeConnectorListener InEdgeConnectorListener)
		{
			System.Type ctorType = null;
			switch (InNode)
			{
				// Specific Types
				case BTComposite_TwoParallelNode _: ctorType = typeof(TwoParallelNodeView); break;

				// Base types
				case BTRootNode _: ctorType = typeof(RootNodeView); break;
				case BTTaskNode _: ctorType = typeof(ActionNodeView); break;
				case BTCompositeNode _: ctorType = typeof(CompositeNodeView); break;
				case BTDecoratorNode _: ctorType = typeof(DecoratorNodeView); break;
			}
			if (Utils.CustomAssertions.IsNotNull(ctorType, $"Cannot create view for node of type {InNode?.GetType().Name??"null"}"))
			{
				object[] args = new object[] { InNode, InEdgeConnectorListener };
				return (NodeViewBase)System.Activator.CreateInstance(ctorType, args);
			}
			return null;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	internal sealed class SearchWindowProvider : ScriptableObject, ISearchWindowProvider
	{
		private static readonly			Dictionary<System.Type, List<System.Type>>		m_MappedTypes		= new Dictionary<System.Type, List<System.Type>>();
		private							BehaviourTreeView								m_TreeView			= null;
		private							NodeViewPort									m_StartPort			= null;


		//////////////////////////////////////////////////////////////////////////
		static SearchWindowProvider() 
		{
			foreach (System.Type concreteNodeType in TypeCache.GetTypesDerivedFrom<BTNode>())
			{
				if (concreteNodeType != typeof(BTRootNode) && !concreteNodeType.IsAbstract)
				{
					List<System.Type> types = ReflectionHelper.GetBaseTypesOf(typeof(BTNode), concreteNodeType, true);

					List<System.Type> derivedTypes = null;
					if (!m_MappedTypes.TryGetValue(types.First(), out derivedTypes))
					{
						derivedTypes = new List<System.Type>() {};
						m_MappedTypes.Add(types.First(), derivedTypes);
					}
					derivedTypes.Add(concreteNodeType);
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void Configure(in BehaviourTreeView InTreeView, in NodeViewPort InStartPort = null)
		{
			m_TreeView = InTreeView;
			m_StartPort = InStartPort;
		}

		//////////////////////////////////////////////////////////////////////////
		List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
		{
			List<SearchTreeEntry> OutResult = new List<SearchTreeEntry>()
			{
				// First item in the tree is the title of the window.
				new SearchTreeGroupEntry(new GUIContent("Create Node"), 0)
			};

			List<SearchTreeEntry> eligibles = new List<SearchTreeEntry>();
			foreach (KeyValuePair<System.Type, List<System.Type>> keyValue in m_MappedTypes)
			{
				// Base type entry for sub-menu
				{
					string baseName = BehaviourTreeEditorUtils.TryGetNameAndDescription(keyValue.Key, out string OutName, out string OutDescription) ? OutName : keyValue.Key.Name;
					eligibles.Add(new SearchTreeGroupEntry(new GUIContent(baseName), 1));
				}

				// All sub types
				foreach (System.Type concreteNodeType in keyValue.Value)
				{
					string name = BehaviourTreeEditorUtils.TryGetNameAndDescription(concreteNodeType, out string OutName, out string OutDescription) ? OutName : concreteNodeType.Name;
					eligibles.Add(new SearchTreeEntry(new GUIContent(name)) { level = 2, userData = concreteNodeType });
				}
			}

			foreach (SearchTreeEntry entry in eligibles)
			{
				if (m_StartPort.IsNotNull() && m_StartPort.PortTypeSelectors.IsNotNull() && entry.userData.IsNotNull())
				{
					System.Type entryType = entry.userData as System.Type;
					foreach (System.Type eligibleType in m_StartPort.PortTypeSelectors)
					{
						if (ReflectionHelper.IsInerithedFrom(eligibleType, entryType))
						{
							entry.level = 1;
							OutResult.Add(entry);
						}
					}
				}
				else
				{
					OutResult.Add(entry);
				}
			}

			return OutResult;
		}

		//////////////////////////////////////////////////////////////////////////
		bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
		{
			bool bResult = false;
			if (bResult = BehaviourTreeEditorWindow.TryTransformInBTViewSpace(context.screenMousePosition, out Vector2 graphMousePosition))
			{
				// Create node view
				NodeViewBase childNodeView = m_TreeView.CreateNode(searchTreeEntry.userData as System.Type, graphMousePosition);
				if (childNodeView.IsNotNull() && m_StartPort.IsNotNull())
				{
					if (m_StartPort.capacity == Port.Capacity.Single && m_StartPort.connected)
					{
						m_TreeView.DeleteElements(m_StartPort.connections);
					}

					// Create and add the edge
					Edge edge = m_StartPort.ConnectTo(childNodeView.InputPort);	// Create the edge
					m_TreeView.Add(edge);										// Add the edge to tree graph view
					edge.input.Connect(edge);									// Connect input port to the edge
					edge.output.Connect(edge);									// Connect output port to the edge

					// Actually add the child node to the parent node
					m_StartPort.GetNodeViewBase().OnEdgeCreation(edge, childNodeView, m_StartPort.PortIndex);
				}

				// Update Nodes Indexes
				BehaviourTreeEditorUtils.AssignChildrenIndexes(m_TreeView.BehaviourTree.AsEditorInterface.RootNode);
			}
			return bResult;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	internal sealed class NodeViewPort : Port
	{
		public readonly		BTNode				Node							= null;
		public readonly		uint				PortIndex						= 0u;
		public readonly		System.Type[]		PortTypeSelectors				= null;


		//////////////////////////////////////////////////////////////////////////
		NodeViewPort(in Orientation InOrientation, in Direction InDirection, in Capacity InCapacity, in BTNode InNode, in uint InPortIndex, in System.Type[] InPortTypeSelectors)
			: base(InOrientation, InDirection, InCapacity, typeof(bool))
		{
			Node = InNode;
			PortIndex = InPortIndex;
			PortTypeSelectors = InPortTypeSelectors;
		}


		//////////////////////////////////////////////////////////////////////////
		public static NodeViewPort Create(in Orientation InOrientation, in Direction InDirection, in Capacity InCapacity, in EdgeConnectorListener InEdgeConnectorListener, in BTNode InNode, in uint InPortIndex, in System.Type[] InPortTypeSelectors)
		{
			NodeViewPort port = new NodeViewPort(InOrientation, InDirection, InCapacity, InNode, InPortIndex, InPortTypeSelectors)
			{
				m_EdgeConnector = new EdgeConnector<Edge>(InEdgeConnectorListener)
			};

			port.style.flexDirection = InDirection == Direction.Input ? FlexDirection.Column : FlexDirection.ColumnReverse;
			port.portName = string.Empty;
			port.AddManipulator(port.m_EdgeConnector);
			return port;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	internal sealed class EdgeConnectorListener : IEdgeConnectorListener
	{
		private readonly BehaviourTreeView		m_TreeView				= null;

		private readonly GraphViewChange		m_GraphViewChange		= default;
		private readonly List<Edge>				m_EdgesToCreate			= null;
		private readonly List<GraphElement>		m_EdgesToDelete			= null;


		//////////////////////////////////////////////////////////////////////////
		public EdgeConnectorListener(BehaviourTreeView InTreeView)
		{
			m_TreeView = InTreeView;

			m_EdgesToCreate = new List<Edge>();
			m_EdgesToDelete = new List<GraphElement>();
			m_GraphViewChange.edgesToCreate = m_EdgesToCreate;
		}

		//////////////////////////////////////////////////////////////////////////
		public void OnDropOutsidePort(Edge edge, Vector2 position)
		{
			Port draggedPort = (edge.output?.edgeConnector.edgeDragHelper.draggedPort) ?? (edge.input?.edgeConnector.edgeDragHelper.draggedPort);
			if (Utils.CustomAssertions.IsNotNull(draggedPort) && draggedPort.direction == Direction.Output)
			{
				NodeViewPort startPort = draggedPort.GetBTNodePort();
				if (Utils.CustomAssertions.IsNotNull(startPort))
				{
					SearchWindowProvider searchWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
					searchWindowProvider.hideFlags = HideFlags.HideAndDontSave;
					searchWindowProvider.Configure(m_TreeView, startPort);
					SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), searchWindowProvider);
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void OnDrop(GraphView graphView, Edge edge)
		{
			// Vanilla code
			m_EdgesToCreate.Clear();
			m_EdgesToCreate.Add(edge);
			m_EdgesToDelete.Clear();
			if (edge.input.capacity == Port.Capacity.Single)
			{
				foreach (Edge connection in edge.input.connections)
				{
					if (connection != edge)
					{
						m_EdgesToDelete.Add(connection);
					}
				}
			}

			if (edge.output.capacity == Port.Capacity.Single)
			{
				foreach (Edge connection2 in edge.output.connections)
				{
					if (connection2 != edge)
					{
						m_EdgesToDelete.Add(connection2);
					}
				}
			}

			if (m_EdgesToDelete.Count > 0)
			{
				graphView.DeleteElements(m_EdgesToDelete);
			}

			List<Edge> edgesToCreate = m_EdgesToCreate;
			if (graphView.graphViewChanged != null)
			{
				edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;
			}

			foreach (Edge item in edgesToCreate)
			{
				graphView.AddElement(item);
				edge.input.Connect(item);
				edge.output.Connect(item);
			}

			BehaviourTreeEditorUtils.AssignChildrenIndexes(m_TreeView.BehaviourTree.AsEditorInterface.RootNode);
		}
	}
}
