using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Entities.AI.Components.Behaviours
{
	internal sealed class ParallelNodeView : CompositeNodeView
	{
		private BTComposite_ParallelNode m_ParallelNode = null;

		public ParallelNodeView(in BTComposite_ParallelNode InNode, in BehaviourTreeView InBehaviourTreeView, in EdgeConnectorListener InEdgeConnectorListener)
		: base(InNode, InBehaviourTreeView, InEdgeConnectorListener)
		{
			m_ParallelNode = InNode;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override List<Port> CreateOutputPorts()
		{
			List<Port> OutPorts = new List<Port>();
			{
				for (uint index = 0; index < BTComposite_ParallelNode.kMaxParallelChildrenCount; ++index)
				{
					NodeViewPort viewPort = base.CreatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, m_BehaviourTreeNode, index);
					outputContainer.Add(viewPort);
					OutPorts.Add(viewPort);
				}
			}
			return OutPorts;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnChildConnected(in BTNode InChild, in Edge InEdgeCreated)
		{
			uint nodeIndex = BTNode.Editor.GetNodeParentPortIndex(InChild);
			if (m_ParallelNode.Children.TryGetByIndex(nodeIndex, out BTNode _))
			{
				m_ParallelNode.Children[(int)nodeIndex] = InChild;
			}
			else
			{
				BTComposite_ParallelNode.Editor.AddChild(m_ParallelNode, InChild, nodeIndex);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnChildDisconnected(in BTNode InChild)
		{
			uint nodeIndex = BTNode.Editor.GetNodeParentPortIndex(InChild);
			if (m_ParallelNode.Children.TryGetByIndex(nodeIndex, out BTNode _))
			{
				m_ParallelNode.Children[(int)nodeIndex] = null;
			}
		}
	}
}
