using System;

namespace Entities.AI.Components.Behaviours.Editor
{
	public class NodeResolver
	{
		public BehaviourTreeNode CreateNodeInstance(Type type)
		{
			BehaviourTreeNode node;
			if (type.IsSubclassOf(typeof(Composite)))
			{
				node = new CompositeNode();
			} else if (type.IsSubclassOf(typeof(Conditional)))
			{
				node = new ConditionalNode();
			} else if (type == typeof(Root))
			{
				node = new RootNode();
			}
			else
			{
				node = new ActionNode();
			}
			node.SetBehaviour(type);
			return node;
		}
	}
}
