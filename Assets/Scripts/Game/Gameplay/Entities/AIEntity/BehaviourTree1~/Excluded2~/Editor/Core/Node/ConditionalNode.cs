using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Entities.AI.Components.Behaviours.Editor
{
	public class ConditionalNode : BehaviourTreeNode
	{
		private Port childPort;

		public Port Child => childPort;

		private BehaviourTreeNode cache;

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.MenuItems().Add(new BehaviourTreeDropdownMenuAction("Change Behaviour", (a) =>
			{
				var provider = new ConditionalSearchWindowProvider(this);
				SearchWindow.Open(new SearchWindowContext(evt.localMousePosition), provider);
			}));
		}

		public ConditionalNode()
		{
			childPort = CreateChildPort();
			outputContainer.Add(childPort);
		}

		protected override bool OnValidate(Stack<BehaviourTreeNode> stack)
		{
			if (!childPort.connected)
			{
				return true;
			}
			stack.Push(childPort.connections.First().input.node as BehaviourTreeNode);
			return true;
		}

		protected override void OnCommit(Stack<BehaviourTreeNode> stack)
		{
			if (!childPort.connected)
			{
				(NodeBehaviour as Conditional).Child = null;
				cache = null;
				return;
			}
			var child = childPort.connections.First().input.node as BehaviourTreeNode;
			(NodeBehaviour as Conditional).Child = child.ReplaceBehaviour();
			stack.Push(child);
			cache = child;
		}

		protected override void OnClearStyle()
		{
			cache?.ClearStyle();
		}
	}
}
