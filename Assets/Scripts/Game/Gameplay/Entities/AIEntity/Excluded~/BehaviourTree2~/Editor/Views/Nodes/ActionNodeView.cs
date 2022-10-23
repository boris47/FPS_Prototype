using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Entities.AI.Components.Behaviours
{
	internal class ActionNodeView : NodeViewBase
	{
		public ActionNodeView(in BTNode InNode, in EdgeConnectorListener InEdgeConnectorListener, in bool bIsBehaviourTreeInstance)
		: base(InNode, InEdgeConnectorListener, bIsBehaviourTreeInstance)
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
