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
		protected readonly Port m_Input = null;
		protected readonly List<Port> m_Outputs = null;

		internal Port InputPort => m_Input;

		internal BTNode BehaviourTreeNode => m_BehaviourTreeNode;

		public NodeViewBase(in BTNode InNode, in EdgeConnectorListener InEdgeConnectorListener, in bool bIsBehaviourTreeInstance)
		: base("Assets/Scripts/Game/Entities/AIEntity/BehaviourTree/Editor/Data/BehaviourTreeNodeView.uxml")
		{
			m_BehaviourTreeNode = InNode;
			m_EdgeConnectorListener = InEdgeConnectorListener;
			m_IsBehaviourTreeInstance = bIsBehaviourTreeInstance;

			title = InNode.NodeName;
			tooltip = InNode.NodeInfo;
			viewDataKey = InNode.AsEditorInterface.Guid;

			{
				var p = GetPosition();
				p.xMin = InNode.AsEditorInterface.Position.x;
				p.yMin = InNode.AsEditorInterface.Position.y;
				SetPosition(p);
			}

			this.capabilities &= ~Capabilities.Collapsible;

			m_Aux = this.Q("aux");
			m_Input = CreateInputPort();
			m_Outputs = CreateOutputPorts();
			RefreshPorts();
			/*
			static async void EnsureCreationPosition(NodeViewBase nodeView)
			{
				await System.Threading.Tasks.Task.Delay(50);
				var p = nodeView.GetPosition();
				p.x += 1f; nodeView.SetPosition(p);
				p.x -= 1f; nodeView.SetPosition(p);
			}
			if (!m_IsBehaviourTreeInstance)
			{
				EnsureCreationPosition(this);
			}
			*/
			if (m_BehaviourTreeNode.AsEditorInterface.HasBreakpoint)
			{
				AddToClassList("breakpointActive");
			}
			else
			{
				RemoveFromClassList("breakpointActive");
			}
			EditorUtility.SetDirty(m_BehaviourTreeNode);
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

			evt.menu.AppendAction((bHasBreakPoint ? "Remove" : "Set") + " Breakpoint", action => ToggleBreakpoint(m_BehaviourTreeNode.AsEditorInterface));
			evt.menu.AppendSeparator();
		}


		protected Port CreatePort(in Orientation InOrientation, in Direction InDirection, in Port.Capacity InCapacity, in BTNode InNode, in int InPortIndex = 0)
		{
			return NodeViewPort.Create(InOrientation, InDirection, InCapacity, m_EdgeConnectorListener, InNode, InPortIndex);
		}
		
		public Edge ConnectTo(in NodeViewBase other)
		{
			return m_Outputs[0].ConnectTo(other.InputPort);
		//	if (m_Output.IsValidIndex(index))
		//	{
		//		return m_Output[index].ConnectTo(other.m_Input);
		//	}
		//	throw new System.ArgumentException($"Invalid index: {index}", nameof(index));
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

		public void SortChildren()
		{
			static int SortByHorizontalPosition(IBTNodeEditorInterface left, IBTNodeEditorInterface right) => left.Position.x < right.Position.x ? -1 : 1;

			if (m_BehaviourTreeNode is BTCompositeNode compositeNode)
			{
				if (!m_IsBehaviourTreeInstance)
				{
					EditorUtility.SetDirty(compositeNode);
				}
				compositeNode.Children.Sort(SortByHorizontalPosition);
			}
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
	//			case BTTwoParallelNode _: ctorType = typeof(TwoParallelNodeView); break;
				// Base types
				case BTRootNode _: ctorType = typeof(RootNodeView); break;
				case BTTaskNode _: ctorType = typeof(ActionNodeView); break;
				case BTCompositeNode _: ctorType = typeof(CompositeNodeView); break;
				case BTDecoratorNode _: ctorType = typeof(DecoratorNodeView); break;
			}
			if (CustomAssertions.IsNotNull(ctorType))
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
		}

		List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
		{
			var result = new List<SearchTreeEntry>()
			{
				// First item in the tree is the title of the window.
				new SearchTreeGroupEntry(new GUIContent("Create Node"), 0)
			};
			result.AddRange(m_Eligibles);
			return result;
		}

		bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
		{
			bool bResult = false;
			if (bResult = BehaviourTreeEditorWindow.TryTransformInBTViewSpace(context.screenMousePosition, out Vector2 graphMousePosition))
			{
				// Create node view
				var nodeView = m_TreeView.CreateNode(searchTreeEntry.userData as System.Type, graphMousePosition);

				// Create and add the edge
				{
					Edge edge = m_StartPort.ConnectTo(nodeView.InputPort);	// Create the edge
					m_TreeView.Add(edge);									// Add the edge to tree graph view
					edge.input.Connect(edge);								// Connect input port to the edge
					edge.output.Connect(edge);								// Connect output port to the edge
				}

				// Actually add the child node to the parent node
				{
					BTNode parent = m_StartPort.GetBTNode();
					BTNode child = nodeView.GetBTNode();
					m_TreeView.m_BehaviourTree.AsEditorInterface.AddChildTo(parent, child);
				}
			}
			return bResult;
		}
	}

	internal sealed class NodeViewPort : Port
	{
		public BTNode Node { get; private set; }

		public int PortIndex { get; private set; } = 0;

		NodeViewPort(in Orientation InOrientation, in Direction InDirection, in Capacity InCapacity, in BTNode InNode, in int InPortIndex = 0)
			: base(InOrientation, InDirection, InCapacity, typeof(bool))
		{
			PortIndex = InPortIndex;
			Node = InNode;
		}

		public static NodeViewPort Create(in Orientation InOrientation, in Direction InDirection, in Capacity InCapacity, in EdgeConnectorListener InEdgeConnectorListener, in BTNode InNode, in int InPortIndex = 0)
		{
			NodeViewPort port = new NodeViewPort(InOrientation, InDirection, InCapacity, InNode, InPortIndex)
			{
				m_EdgeConnector = new EdgeConnector<Edge>(InEdgeConnectorListener)
			};

			port.PortIndex = InPortIndex;
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
		}
	}
}
