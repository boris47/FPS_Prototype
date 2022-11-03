
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
		//////////////////////////////////////////////////////////////////////////
		/// <summary> OnNodeUpdate gets called once a child terminates its execution </summary>
		protected override EBTNodeState OnNodeUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;

			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
			if (nodeData.CurrentIndex >= Children.Length)
			{
				ConditionalLog($"Sequence completed", InThisNodeInstanceData);
				if (MustRepeat)
				{
					ResetNode(InThisNodeInstanceData);
				}
				else
				{
					OutState = EBTNodeState.SUCCEEDED;
				}
			}
			else
			{
				BTNode childAsset = Children.At(nodeData.CurrentIndex);
				BTNodeInstanceData childInstanceData = GetNodeInstanceData(InThisNodeInstanceData, childAsset);

				if (Utils.CustomAssertions.IsTrue(childInstanceData.NodeState != EBTNodeState.RUNNING))
				{
					if (childInstanceData.NodeState == EBTNodeState.INACTIVE)
					{
						ConditionalLog($"Starting setting as running node child {childAsset.name}(Index:{nodeData.CurrentIndex})", InThisNodeInstanceData);
						childInstanceData.SetAsRunningNode();
					}
					else
					{
						ConditionalLog($"Child {childAsset.name}(Index:{nodeData.CurrentIndex}) finished with {childInstanceData.NodeState}", InThisNodeInstanceData);
						if (childInstanceData.NodeState == EBTNodeState.SUCCEEDED)
						{
							++nodeData.CurrentIndex;
						}
						else
						{
							OutState = EBTNodeState.FAILED;
						}
					}
				}
			}
			return OutState;
		}
	}
}
