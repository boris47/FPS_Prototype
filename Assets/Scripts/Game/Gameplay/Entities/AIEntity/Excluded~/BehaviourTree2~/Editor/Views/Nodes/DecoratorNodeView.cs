using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System;

namespace Entities.AI.Components.Behaviours
{
	internal class DecoratorNodeView : NodeViewBase
	{
		public DecoratorNodeView(in BTNode InNode, in EdgeConnectorListener InEdgeConnectorListener, in bool bIsBehaviourTreeInstance)
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
			Port output = base.CreatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, m_BehaviourTreeNode);
			outputContainer.Add(output);
			return new List<Port>() { output };
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			base.BuildContextualMenu(evt);
		}
	}
	/*
	internal class ConditionalNodeView : DecoratorNodeView
	{
	//	public override Type InsideNodeEditorType => typeof(BTConditional.BTConditionalEditor);

		public ConditionalNodeView(in BTNode InNode, in EdgeConnectorListener InEdgeConnectorListener, in bool bIsBehaviourTreeInstance)
		: base(InNode, InEdgeConnectorListener, bIsBehaviourTreeInstance)
		{
			m_Aux.style.alignContent = Align.Stretch;
			m_Aux.style.alignItems = Align.Stretch;
		}
	}
	*/
}
