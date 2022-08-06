
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

		protected override EBTNodeState OnUpdate()
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;
			switch (Children.At(m_CurrentIndex).Update())
			{
				case EBTNodeState.INACTIVE:
					CustomAssertions.IsTrue(false);
					break;
				case EBTNodeState.SUCCEEDED:
				{
					if (Children.IsValidIndex(m_CurrentIndex + 1))
					{
						BehaviourTree.SetRunningNode(Children.At(++m_CurrentIndex));
					}
					else
					{
						if (m_MustRepeat)
						{
							ResetNode();
							SetNodeState(EBTNodeState.RUNNING);
						}
						else
						{
							OutState = EBTNodeState.SUCCEEDED;
						}
					}
					break;
				}
				case EBTNodeState.FAILED:
					OutState = EBTNodeState.FAILED;
					break;
				case EBTNodeState.RUNNING:
					break;
				default:
					CustomAssertions.IsTrue(false);
					break;
			}
			return OutState;
		}
	}
}
