using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Entities.AI.Components.Behaviours.Editor
{
	public class BehaviourTreeView: GraphView
	{
		private readonly struct EdgePair
		{
			public readonly NodeBehaviour NodeBehaviour;
			public readonly Port ParentOutputPort;

			public EdgePair(NodeBehaviour nodeBehaviour, Port parentOutputPort)
			{
				NodeBehaviour = nodeBehaviour;
				ParentOutputPort = parentOutputPort;
			}
		}

		private readonly BehaviourTree BehaviourTree;

		private RootNode root;

		private readonly NodeResolver nodeResolver = new NodeResolver();

		public BehaviourTreeView(BehaviourTree bt, EditorWindow editor)
		{
			BehaviourTree = bt;
			style.flexGrow = 1;
			style.flexShrink = 1;

			SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
			Insert(0, new GridBackground());

			var contentDragger = new ContentDragger();
			//default activator requires Alt key. Alt key is tired.
			contentDragger.activators.Add(new ManipulatorActivationFilter()
			{
				button = MouseButton.LeftMouse,
			});
			// item dragging is prior to view dragging.
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(contentDragger);

			var provider = ScriptableObject.CreateInstance<NodeSearchWindowProvider>();
			provider.Initialize(this, editor);

			nodeCreationRequest += context =>
			{
				SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), provider);
			};
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			base.BuildContextualMenu(evt);
			var remainTargets = evt.menu.MenuItems().FindAll(e =>
			{
				switch (e)
				{
					case BehaviourTreeDropdownMenuAction _ : return true;
					case DropdownMenuAction a: return a.name == "Create Node" || a.name == "Delete";
					default: return false;
				}
			});
			//Remove needless default actions .
			evt.menu.MenuItems().Clear();
			remainTargets.ForEach(evt.menu.MenuItems().Add);
		}

		public override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter) {
			var compatiblePorts = new List<Port>();
			foreach (var port in ports.ToList())
			{
				if (startAnchor.node == port.node ||
					startAnchor.direction == port.direction ||
					startAnchor.portType != port.portType)
				{
					continue;
				}

				compatiblePorts.Add(port);
			}
			return compatiblePorts;
		}

		public void Restore()
		{
			var stack = new Stack<EdgePair>();
			stack.Push(new EdgePair(BehaviourTree.Root, null));
			while (stack.Count > 0)
			{
				// create node
				var edgePair = stack.Pop();
				if (edgePair.NodeBehaviour == null)
				{
					continue;
				}
				var node = nodeResolver.CreateNodeInstance(edgePair.NodeBehaviour.GetType());
				node.Restore(edgePair.NodeBehaviour);
				AddElement(node);
				node.SetPosition( edgePair.NodeBehaviour.graphPosition);

				// connect parent
				if (edgePair.ParentOutputPort != null)
				{
					var edge = ConnectPorts(edgePair.ParentOutputPort, node.Parent);
					AddElement(edge);
				}

				// seek child
				switch (edgePair.NodeBehaviour)
				{
					case Composite nb:
					{
						var compositeNode = node as CompositeNode;
						var addible = nb.Children.Count - compositeNode.ChildPorts.Count;
						for (var i = 0; i < addible; i++)
						{
							compositeNode.AddChild();
						}

						for (var i = 0; i < nb.Children.Count; i++)
						{
							stack.Push(new EdgePair(nb.Children[i], compositeNode.ChildPorts[i]));
						}
						break;
					}
					case Conditional nb:
					{
						var decoratorNode = node as ConditionalNode;
						stack.Push(new EdgePair(nb.Child, decoratorNode.Child));
						break;
					}
					case Root nb:
					{
						root = node as RootNode;
						if (nb.Child != null)
						{
							stack.Push(new EdgePair(nb.Child, root.Child));
						}
						break;
					}
				}
			}
		}


		public bool Save()
		{
			if (Validate())
			{
				Commit();
				return true;
			}

			return false;
		}

		private bool Validate()
		{
			//validate nodes by DFS.
			var stack = new Stack<BehaviourTreeNode>();
			stack.Push(root);
			while (stack.Count > 0)
			{
				var node = stack.Pop();
				if (!node.Validate(stack))
				{
					return false;
				}
			}
			return true;
		}

		private void Commit()
		{
			var stack = new Stack<BehaviourTreeNode>();
			stack.Push(root);

			// save new components
			while (stack.Count > 0)
			{
				var node = stack.Pop();
				node.Commit(stack);
			}

			root.PostCommit(BehaviourTree);

			// notify to unity editor that the tree is changed.
			EditorUtility.SetDirty(BehaviourTree);
		}

		private static Edge ConnectPorts(Port output, Port input)
		{
			var tempEdge = new Edge
			{
				output = output,
				input = input
			};
			tempEdge.input.Connect(tempEdge);
			tempEdge.output.Connect(tempEdge);
			return tempEdge;
		}

	}
}
