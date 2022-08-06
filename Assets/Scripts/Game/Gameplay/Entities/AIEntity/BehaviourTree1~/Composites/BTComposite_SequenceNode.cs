
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	/// <summary>
	/// Sequence composite node. <br/>
	/// Sequence Nodes execute their children from left to right, and will stop executing its children when one of their children fails. <br/>
	/// If a child fails, then the Sequence fails. If all the Sequence's children succeed, then the Sequence succeeds. <br/>
	/// </summary>
	public sealed partial class BTComposite_SequenceNode : BTCompositeNode
	{
		public override string NodeName => "Sequence";
		public override string NodeInfo => "Execute its children from left to right, and will stop executing its children when one of their children fails";

		//////////////////////////////////////////////////////////////////////////
		public override void OnChildFinished(in BTNode InNode, in EBTNodeState InChildState)
		{
			if (CustomAssertions.IsTrue(m_CurrentIndex == Children.IndexOf(InNode)))
			{
				if (InChildState == EBTNodeState.FAILED)
				{
					m_NodeState = EBTNodeState.FAILED;
				}

				if (InChildState == EBTNodeState.SUCCEEDED)
				{
					if (m_CurrentIndex + 1 >= Children.Count)
					{
						m_NodeState = EBTNodeState.SUCCEEDED;
					}
					else
					{
						if (!Children.At(++m_CurrentIndex).TryActivation())
						{
							m_NodeState = EBTNodeState.FAILED;
						}
					}
				}
			}
		}
	}
}
