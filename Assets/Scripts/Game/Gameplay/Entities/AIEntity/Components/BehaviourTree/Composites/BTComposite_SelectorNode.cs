using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	/// <summary>
	/// Selector composite node.<br/>
	/// Selector Nodes execute their children from left to right, and will stop executing its children when one of their children succeeds.<br/>
	/// If a Selector's child succeeds, the Selector succeeds. If all the Selector's children fail, the Selector fails.<br/>
	/// </summary>
	[BTNodeDetails("Selector", "Executes its children from left to right, and will stop on child success")]
	public partial class BTComposite_SelectorNode : BTCompositeNode
	{
		//////////////////////////////////////////////////////////////////////////
		/// <summary> OnNodeUpdate gets called once a child terminates its execution </summary>
		protected override EBTNodeState OnNodeUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;

			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
			if (nodeData.CurrentIndex >= Children.Length)
			{
				if (MustRepeat)
				{
					ResetNode(InThisNodeInstanceData);
				}
				else
				{
					OutState = EBTNodeState.FAILED;
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
							OutState = EBTNodeState.SUCCEEDED;
						}
						else
						{
							++nodeData.CurrentIndex;
						}
					}
				}
			}
			return OutState;
		}
	}
}
