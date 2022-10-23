using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Entities.AI.Components.Behaviours.Editor
{
	public class ActionNode : BehaviourTreeNode
	{
		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.MenuItems().Add(new BehaviourTreeDropdownMenuAction("Change Behaviour", (a) =>
			{
				var provider = new ActionSearchWindowProvider(this);
				SearchWindow.Open(new SearchWindowContext(evt.localMousePosition), provider);
			}));
		}

		protected override bool OnValidate(Stack<BehaviourTreeNode> stack) => true;

		protected override void OnCommit(Stack<BehaviourTreeNode> stack)
		{
		}

		protected override void OnClearStyle()
		{
		}
	}
}
