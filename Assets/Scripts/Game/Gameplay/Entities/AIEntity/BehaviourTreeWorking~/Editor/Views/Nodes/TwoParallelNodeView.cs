using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Entities.AI.Components.Behaviours
{
	internal sealed class TwoParallelNodeView : CompositeNodeView
	{
		private const uint MainNodeIndex = 0u;
		private const uint BackgroundNodeIndex = 1u;
		private BTComposite_TwoParallelNode m_TwoParallelNode = null;

		public TwoParallelNodeView(in BTComposite_TwoParallelNode InNode, in EdgeConnectorListener InEdgeConnectorListener, in bool bIsBehaviourTreeInstance)
		: base(InNode, InEdgeConnectorListener, bIsBehaviourTreeInstance)
		{
			m_TwoParallelNode = InNode;
		}

		protected override List<Port> CreateOutputPorts()
		{
			List<Port> OutPorts = new List<Port>();
			{
				{
					System.Type[] portTypeSelectors = new System.Type[] { typeof(BTTaskNode) };
					NodeViewPort main = base.CreatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, m_BehaviourTreeNode, MainNodeIndex, portTypeSelectors);
					outputContainer.Add(main);
					OutPorts.Add(main);
				}

				{
					System.Type[] portTypeSelectors = new System.Type[] { typeof(BTCompositeNode) };
					NodeViewPort background = base.CreatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, m_BehaviourTreeNode, BackgroundNodeIndex, portTypeSelectors);
					outputContainer.Add(background);
					OutPorts.Add(background);
				}
			}
			return OutPorts;
		}

		protected override void OnChildConnected(in BTNode InChild, in Edge InEdgeCreated)
		{
			if (InChild.AsEditorInterface.ParentPortIndex == MainNodeIndex)
			{
				m_TwoParallelNode.AsTwoParallelNodeEditorInterface.MainNode = InChild as BTTaskNode;
			}

			if (InChild.AsEditorInterface.ParentPortIndex == BackgroundNodeIndex)
			{
				m_TwoParallelNode.AsTwoParallelNodeEditorInterface.BackgroundNode = InChild as BTCompositeNode;
			}
		}

		protected override void OnChildDisconnected(in BTNode InChild)
		{
			if (InChild.AsEditorInterface.ParentPortIndex == MainNodeIndex)
			{
				m_TwoParallelNode.AsTwoParallelNodeEditorInterface.MainNode = null;
			}

			if (InChild.AsEditorInterface.ParentPortIndex == BackgroundNodeIndex)
			{
				m_TwoParallelNode.AsTwoParallelNodeEditorInterface.BackgroundNode = null;
			}
		}


		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			base.BuildContextualMenu(evt);
		}
	}
}
