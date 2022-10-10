using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Entities.AI.Components.Behaviours
{
	internal sealed class TwoParallelNodeView : CompositeNodeView
	{
		private BTComposite_TwoParallelNode m_TwoParallelNode = null;

		public TwoParallelNodeView(in BTComposite_TwoParallelNode InNode, in EdgeConnectorListener InEdgeConnectorListener)
		: base(InNode, InEdgeConnectorListener)
		{
			m_TwoParallelNode = InNode;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override List<Port> CreateOutputPorts()
		{
			List<Port> OutPorts = new List<Port>();
			{
				for (uint index = 0; index < BTComposite_TwoParallelNode.kMaxParallelChildrenCount; index++)
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
			uint nodeIndex = InChild.AsEditorInterface.ParentPortIndex;
			if (m_TwoParallelNode.Children.TryGetByIndex(nodeIndex, out BTNode _))
			{
				m_TwoParallelNode.Children[(int)nodeIndex] = InChild;
			}
			else
			{
				m_TwoParallelNode.AddChild(InChild, nodeIndex);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnChildDisconnected(in BTNode InChild)
		{
			uint nodeIndex = InChild.AsEditorInterface.ParentPortIndex;
			if (m_TwoParallelNode.Children.TryGetByIndex(nodeIndex, out BTNode _))
			{
				m_TwoParallelNode.Children[(int)nodeIndex] = null;
			}
		}
	}
}
