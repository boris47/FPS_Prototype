using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Entities.AI.Components.Behaviours
{
	internal class CompositeNodeView : NodeViewBase
	{
		public CompositeNodeView(in BTCompositeNode InNode, in EdgeConnectorListener InEdgeConnectorListener, in bool bIsBehaviourTreeInstance)
		: base(InNode, InEdgeConnectorListener, bIsBehaviourTreeInstance)
		{
			
		}

		//////////////////////////////////////////////////////////////////////////
		protected override Port CreateInputPort()
		{
			Port input = base.CreatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, m_BehaviourTreeNode);
			inputContainer.Add(input);
			return input;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override List<Port> CreateOutputPorts()
		{
			Port output = base.CreatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, m_BehaviourTreeNode);
			outputContainer.Add(output);
			return new List<Port>() { output };
		}

		//////////////////////////////////////////////////////////////////////////
		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			base.BuildContextualMenu(evt);
		}
	}
}
