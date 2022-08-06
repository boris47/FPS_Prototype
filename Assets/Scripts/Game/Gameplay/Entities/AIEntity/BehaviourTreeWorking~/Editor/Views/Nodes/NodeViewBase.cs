using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Entities.AI.Components.Behaviours
{
	internal abstract class NodeViewBase : Node
	{
		protected readonly BTNode m_BehaviourTreeNode = null;
		private readonly EdgeConnectorListener m_EdgeConnectorListener = null;
		protected readonly bool m_IsBehaviourTreeInstance = false;

		protected readonly VisualElement m_Aux = null;
		protected readonly Label m_NodeIndexLabel = null;
		protected readonly Port m_Input = null;
		protected readonly List<Port> m_Outputs = null;

		internal Port InputPort => m_Input;

		internal BTNode BehaviourTreeNode => m_BehaviourTreeNode;

		public System.Type InsideNodeEditorType { get; } = typeof(BTNode.BTNodeEditor);
		private Editor m_InsideNodeEditor = null;

		public NodeViewBase(in BTNode InNode, in EdgeConnectorListener InEdgeConnectorListener, in bool bIsBehaviourTreeInstance)
		: base("Assets/Scripts/Game/Entities/AIEntity/BehaviourTree/Editor/Data/BehaviourTreeNodeView.uxml")
		{
			m_BehaviourTreeNode = InNode;
			m_EdgeConnectorListener = InEdgeConnectorListener;
			m_IsBehaviourTreeInstance = bIsBehaviourTreeInstance;

			title = InNode.NodeName;
			tooltip = InNode.NodeInfo;
			viewDataKey = InNode.AsEditorInterface.Guid;

			this.capabilities &= ~Capabilities.Collapsible;
			m_Aux = this.Q("aux");
			m_NodeIndexLabel = this.Q<Label>("nodeIndex");

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
				UnityEngine.Object.DestroyImmediate(m_InsideNodeEditor);
				m_InsideNodeEditor = Editor.CreateEditor(InNode, InsideNodeEditorType);
				IMGUIContainer container = new IMGUIContainer(RegisterOnInspectorGUI);
				m_Aux.Add(container);
			}

			{
				var p = GetPosition();
				p.xMin = InNode.AsEditorInterface.Position.x;
				p.yMin = InNode.AsEditorInterface.Position.y;
				SetPosition(p);
			}

			m_Input = CreateInputPort();
			m_Outputs = CreateOutputPorts();
			RefreshPorts();

			if (m_BehaviourTreeNode.AsEditorInterface.HasBreakpoint)
			{
				AddToClassList("breakpointActive");
			}
			else
			{
				RemoveFromClassList("breakpointActive");
			}
			EditorUtility.SetDirty(m_BehaviourTreeNode);

			m_BehaviourTreeNode.AsEditorInterface.UpdateView = UpdateView;
			UpdateView();
		}

		protected abstract Port CreateInputPort();
		protected abstract List<Port> CreateOutputPorts();

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

			evt.menu.AppendAction((bHasBreakPoint ? "Remove" : "Set") + " Breakpoint", _ => ToggleBreakpoint(m_BehaviourTreeNode.AsEditorInterface));
			evt.menu.AppendSeparator();
			evt.menu.AppendAction("EditScript", _ => AssetDatabase.OpenAsset(MonoScript.FromScriptableObject(m_BehaviourTreeNode)));
			evt.menu.AppendSeparator();
		}

		protected NodeViewPort CreatePort(in Orientation InOrientation, in Direction InDirection, in Port.Capacity InCapacity, in BTNode InNode, in uint InPortIndex = 0u, in System.Type[] InPortTypeSelectors = null)
		{
			return NodeViewPort.Create(InOrientation, InDirection, InCapacity, m_EdgeConnectorListener, InNode, InPortIndex, InPortTypeSelectors);
		}

		// On edge creation
		public Edge OnConnectionWith(in Edge InEdge, in NodeViewBase InChild, in uint? InPortIndex = null)
		{
			NodeViewBase parentView = InEdge.output.node.GetAsBTNodeView();
			NodeViewBase childView = InEdge.input.node.GetAsBTNodeView();
			CustomAssertions.IsTrue(parentView.GetBTNode() == m_BehaviourTreeNode);
			CustomAssertions.IsTrue(InChild.GetBTNode() == childView.BehaviourTreeNode);

			// Add node to parent
			m_BehaviourTreeNode.BehaviourTree.AsEditorInterface.AddChildTo(parentView.BehaviourTreeNode, childView.BehaviourTreeNode);

			// Set parent output port to child parent port index
			InChild.BehaviourTreeNode.AsEditorInterface.ParentPortIndex = InPortIndex ?? InEdge.output.GetBTNodePort().PortIndex;

			// Notify this node of the edge creation
			OnChildConnected(childView.BehaviourTreeNode, InEdge);
			return InEdge;
		}

		// On view population
		public Edge ConnectTo(in NodeViewBase InChild, in uint InPortIndex)
		{
			if (m_Outputs.IsValidIndex(InPortIndex))
			{
				Edge edgeCreated = m_Outputs.At(InPortIndex).ConnectTo(InChild.InputPort);
				OnChildConnected(InChild.GetBTNode(), edgeCreated);
				return edgeCreated;
			}
			throw new System.ArgumentException($"Invalid index: {InPortIndex}", nameof(InPortIndex));
		}

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

		protected virtual void OnChildDisconnected(in BTNode InChild) { }

		protected virtual void OnChildConnected(in BTNode InChild, in Edge InEdgeCreated) { }

		private void UpdateView()
		{
			m_NodeIndexLabel.text = m_BehaviourTreeNode.NodeIndex.ToString("D2");
		}

		public sealed override void SetPosition(Rect newPos)
		{
			base.SetPosition(newPos);
			m_BehaviourTreeNode.AsEditorInterface.Position = new Vector2(newPos.xMin, newPos.yMin);
			EditorUtility.SetDirty(m_BehaviourTreeNode);
		}

		public sealed override void OnSelected()
		{
			base.OnSelected();
			BehaviourTreeView.OnNodeSelected(this);
		}

		public sealed override void OnUnselected()
		{
			BehaviourTreeView.OnNodeUnSelected(this);
			base.OnUnselected();
		}

		protected override void OnPortRemoved(Port port)
		{
			base.OnPortRemoved(port);
		}

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

		public static NodeViewBase CreateNodeView(in BTNode InNode, in EdgeConnectorListener InEdgeConnectorListener, in bool bIsBehaviourTreeInstance)
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
			if (CustomAssertions.IsNotNull(ctorType, $"Cannot create view for node of type {InNode?.GetType().Name??"null"}"))
			{
				var args = new object[] { InNode, InEdgeConnectorListener, bIsBehaviourTreeInstance };
				return (NodeViewBase)System.Activator.CreateInstance(ctorType, args);
			}
			return null;
		}
	}

	internal sealed class SearchWindowProvider : ScriptableObject, ISearchWindowProvider
	{
		private readonly BehaviourTreeView m_TreeView = null;
		private readonly NodeViewPort m_StartPort = null;

		private static IEnumerable<SearchTreeEntry> m_Eligibles = TypeCache.GetTypesDerivedFrom<BTNode>()
				.Except(Enumerable.Repeat(typeof(BTRootNode), 1))
				.Where(t => !t.IsAbstract)
				.Select(t =>
				{
					string name = BehaviourTreeEditorUtils.GetInstancePropertyValue<string>(t, "NodeName") ?? t.Name;
					return new SearchTreeEntry(new GUIContent(name)) { level = 1, userData = t };
				});

		internal SearchWindowProvider(in BehaviourTreeView InTreeView, in NodeViewPort InStartPort)
		{
			m_TreeView = InTreeView;
			m_StartPort = InStartPort;
			//m_StartPort.PortTypeSelectors
		}

		List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
		{
			var result = new List<SearchTreeEntry>()
			{
				// First item in the tree is the title of the window.
				new SearchTreeGroupEntry(new GUIContent("Create Node"), 0)
			};
			result.AddRange(m_Eligibles.Where(t => m_StartPort.PortTypeSelectors?.Any(s => ReflectionHelper.IsInerithedFrom(s, t.userData as System.Type)) ?? true));
			return result;
		}

		bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
		{
			bool bResult = false;
			if (bResult = BehaviourTreeEditorWindow.TryTransformInBTViewSpace(context.screenMousePosition, out Vector2 graphMousePosition))
			{
				// Create node view
				NodeViewBase childNodeView = m_TreeView.CreateNode(searchTreeEntry.userData as System.Type, graphMousePosition);
				if (childNodeView.IsNotNull())
				{
					// Create and add the edge
					Edge edge = m_StartPort.ConnectTo(childNodeView.InputPort);	// Create the edge
					m_TreeView.Add(edge);										// Add the edge to tree graph view
					edge.input.Connect(edge);									// Connect input port to the edge
					edge.output.Connect(edge);									// Connect output port to the edge

					// Actually add the child node to the parent node
					m_StartPort.GetNodeViewBase().OnConnectionWith(edge, childNodeView, m_StartPort.PortIndex);
				}

				// Update Nodes Indexes
				BehaviourTreeEditorUtils.AssignChildrenIndexes(m_TreeView.m_BehaviourTree.AsEditorInterface.RootNode);
			}
			return bResult;
		}
	}

	internal sealed class NodeViewPort : Port
	{
		public BTNode Node { get; private set; }

		public uint PortIndex { get; private set; }

		public System.Type[] PortTypeSelectors { get; private set; } = null;

		NodeViewPort(in Orientation InOrientation, in Direction InDirection, in Capacity InCapacity, in BTNode InNode, in uint InPortIndex, in System.Type[] InPortTypeSelectors)
			: base(InOrientation, InDirection, InCapacity, typeof(bool))
		{
			Node = InNode;
			PortIndex = InPortIndex;
			PortTypeSelectors = InPortTypeSelectors;
		}

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

	internal sealed class EdgeConnectorListener : IEdgeConnectorListener
	{
		private readonly BehaviourTreeView m_TreeView = null;

		private GraphViewChange m_GraphViewChange;
		private List<Edge> m_EdgesToCreate;
		private List<GraphElement> m_EdgesToDelete;

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
			if (CustomAssertions.IsNotNull(draggedPort) && draggedPort.direction == Direction.Output)
			{
				var startPort = draggedPort.GetBTNodePort();
				if (CustomAssertions.IsNotNull(startPort))
				{
					SearchWindowProvider searchWindowProvider = new SearchWindowProvider(m_TreeView, startPort);
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

			BehaviourTreeEditorUtils.AssignChildrenIndexes(m_TreeView.m_BehaviourTree.AsEditorInterface.RootNode);
		}
	}
}
