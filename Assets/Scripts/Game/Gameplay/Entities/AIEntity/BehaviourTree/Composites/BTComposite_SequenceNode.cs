﻿
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	/// <summary>
	/// Sequence composite node. <br/>
	/// Sequence Nodes execute their children from left to right, and will stop executing its children when one of their children fails. <br/>
	/// If a child fails, then the Sequence fails. If all the Sequence's children succeed, then the Sequence succeeds. <br/>
	/// </summary>
	[BTNodeDetails("Sequence", "Execute its children from left to right, and will stop executing its children when one of their children fails")]
	public sealed partial class BTComposite_SequenceNode : BTCompositeNode
	{
		protected sealed override EBTNodeState OnActivation() => base.OnActivation();

		protected override EBTNodeState OnUpdate(in float InDeltaTime)
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;
			switch (Children.At(m_CurrentIndex).UpdateNode(InDeltaTime))
			{
				case EBTNodeState.INACTIVE:
				{
					Utils.CustomAssertions.IsTrue(false);
					break;
				}
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
						}
						else
						{
							OutState = EBTNodeState.SUCCEEDED;
						}
					}
					break;
				}
				case EBTNodeState.FAILED:
				{
					OutState = EBTNodeState.FAILED;
					break;
				}
				case EBTNodeState.RUNNING: break;
				default:
				{
					Utils.CustomAssertions.IsTrue(false);
					break;
				}
			}
			return OutState;
		}
	}
}
