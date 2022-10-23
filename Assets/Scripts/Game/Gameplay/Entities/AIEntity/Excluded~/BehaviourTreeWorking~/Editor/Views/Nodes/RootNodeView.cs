using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Entities.AI.Components.Behaviours
{
	internal class RootNodeView : NodeViewBase
	{
		public RootNodeView(in BTRootNode InNode, in EdgeConnectorListener InEdgeConnectorListener, in bool bIsBehaviourTreeInstance)
		: base(InNode, InEdgeConnectorListener, bIsBehaviourTreeInstance)
		{
			this.capabilities = Capabilities.Selectable | Capabilities.Movable | Capabilities.Snappable;
		}

		protected override Port CreateInputPort()
		{
			// No input ports for root nodes
			return null;
		}

		protected override List<Port> CreateOutputPorts()
		{
			Port output = base.CreatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, m_BehaviourTreeNode);
			outputContainer.Add(output);
			return new List<Port>() { output };
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			base.BuildContextualMenu(evt);
		}
	}
}
