using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;

namespace Entities.AI.Components.Behaviours.Editor
{
	public sealed class RootNode : BehaviourTreeNode
	{
		public readonly Port Child;

		private BehaviourTreeNode cache;

		public RootNode()
		{
			SetBehaviour(typeof(Root));
			title = "Root";
			Child = CreateChildPort();
			outputContainer.Add(Child);
		}

		protected override void AddParent()
		{
		}

		protected override void AddDescription()
		{
		}

		protected override void OnRestore()
		{
			(NodeBehaviour as Root).UpdateEditor = ClearStyle;
		}

		protected override bool OnValidate(Stack<BehaviourTreeNode> stack)
		{
			if (!Child.connected)
			{
				return false;
			}
			stack.Push(Child.connections.First().input.node as BehaviourTreeNode);
			return true;
		}
		protected override void OnCommit(Stack<BehaviourTreeNode> stack)
		{
			var child = Child.connections.First().input.node as BehaviourTreeNode;
			var newRoot = new Root();
			newRoot.Child = child.ReplaceBehaviour();
			newRoot.UpdateEditor = ClearStyle;
			NodeBehaviour = newRoot;
			stack.Push(child);
			cache = child;
		}

		public void PostCommit(BehaviourTree tree)
		{
			tree.Root = (NodeBehaviour as Root);
		}

		protected override void OnClearStyle()
		{
			cache?.ClearStyle();
		}
	}
}
