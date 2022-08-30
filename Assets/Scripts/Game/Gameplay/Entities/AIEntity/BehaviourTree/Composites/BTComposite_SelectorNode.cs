﻿
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	/// <summary>
	/// Selector composite node.<br/>
	/// Selector Nodes execute their children from left to right, and will stop executing its children when one of their children succeeds.<br/>
	/// If a Selector's child succeeds, the Selector succeeds. If all the Selector's children fail, the Selector fails.<br/>
	/// </summary>
	[BTNodeDetails("Selector", "Executes its children from left to right, and will stop executing its children when one of their children succeeds")]
	public partial class BTComposite_SelectorNode : BTCompositeNode
	{
		protected sealed override EBTNodeState OnActivation() => base.OnActivation();

		protected override EBTNodeState OnUpdate()
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;
			switch (Children.At(m_CurrentIndex).Update())
			{
				case EBTNodeState.INACTIVE:
				{
					Utils.CustomAssertions.IsTrue(false);
					break;
				}
				case EBTNodeState.FAILED:
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
							OutState = EBTNodeState.FAILED;
						}
					}
					break;
				}
				case EBTNodeState.SUCCEEDED:
				{
					OutState = EBTNodeState.SUCCEEDED;
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