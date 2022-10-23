using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Entities.AI.Components.Behaviours
{
	internal class ActionNodeView : NodeViewBase
	{
		public ActionNodeView(in BTTaskNode InNode, in BehaviourTreeView InBehaviourTreeView, in EdgeConnectorListener InEdgeConnectorListener)
		: base(InNode, InBehaviourTreeView, InEdgeConnectorListener)
		{

		}

		protected override Port CreateInputPort()
		{
			Port input = base.CreatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, m_BehaviourTreeNode);
			inputContainer.Add(input);
			return input;
		}

		protected override List<Port> CreateOutputPorts()
		{
			// No output ports for actions
			return new List<Port>();
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			base.BuildContextualMenu(evt);
		}
	}
}
