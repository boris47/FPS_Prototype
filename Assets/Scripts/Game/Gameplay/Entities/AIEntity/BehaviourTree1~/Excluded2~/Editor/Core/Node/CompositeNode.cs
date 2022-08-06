using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Entities.AI.Components.Behaviours.Editor
{
	public class CompositeNode : BehaviourTreeNode
	{
		public readonly List<Port> ChildPorts = new List<Port>();

		private List<BehaviourTreeNode> cache = new List<BehaviourTreeNode>();

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.MenuItems().Add(new BehaviourTreeDropdownMenuAction("Change Behaviour", (a) =>
			{
				var provider = new CompositeSearchWindowProvider(this);
				SearchWindow.Open(new SearchWindowContext(evt.localMousePosition), provider);
			}));
			evt.menu.MenuItems().Add(new BehaviourTreeDropdownMenuAction("Add Child", (a) => AddChild()));
			evt.menu.MenuItems().Add(new BehaviourTreeDropdownMenuAction("Remove Unnecessary Children", (a) => RemoveUnnecessaryChildren()));
		}

		public CompositeNode()
		{
			AddChild();
		}

		public void AddChild()
		{
			var child = CreateChildPort();
			ChildPorts.Add(child);
			outputContainer.Add(child);
		}

		private void RemoveUnnecessaryChildren()
		{
			var unnecessary = ChildPorts.Where(p => !p.connected).ToList();
			unnecessary.ForEach(e =>
			{
				ChildPorts.Remove(e);
				outputContainer.Remove(e);
			});
		}

		protected override bool OnValidate(Stack<BehaviourTreeNode> stack)
		{
			if (ChildPorts.Count <= 0) return false;

			foreach (var port in ChildPorts)
			{
				if (!port.connected)
				{
					style.backgroundColor = Color.red;
					return false;
				}
				stack.Push(port.connections.First().input.node as BehaviourTreeNode);
			}
			style.backgroundColor = new StyleColor(StyleKeyword.Null);
			return true;
		}

		protected override void OnCommit(Stack<BehaviourTreeNode> stack)
		{
			cache.Clear();
			foreach (var port in ChildPorts)
			{
				var child = port.connections.First().input.node as BehaviourTreeNode;
				(NodeBehaviour as Composite).AddChild(child.ReplaceBehaviour());
				stack.Push(child);
				cache.Add(child);
			}
		}

		protected override void OnClearStyle()
		{
			foreach (var node in cache)
			{
				node.ClearStyle();
			}
		}
	}
}
