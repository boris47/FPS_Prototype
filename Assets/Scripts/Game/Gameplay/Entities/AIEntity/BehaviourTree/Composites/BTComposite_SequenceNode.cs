
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
		protected sealed override EBTNodeState OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			return base.OnActivation(InThisNodeInstanceData);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;

			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);

			BTNode child = Children.At(nodeData.CurrentIndex);
			BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, child);

			switch (child.UpdateNode(childInstanceData, InDeltaTime))
			{
				case EBTNodeState.INACTIVE:
				{
					Utils.CustomAssertions.IsTrue(false);
					break;
				}
				case EBTNodeState.SUCCEEDED:
				{
					if (Children.IsValidIndex(nodeData.CurrentIndex + 1))
					{
						++nodeData.CurrentIndex;

						child = Children.At(nodeData.CurrentIndex);
						BTNodeInstanceData nextChildInstanceData = GetChildInstanceData(InThisNodeInstanceData, child);
						BehaviourTreeAsset.SetRunningNode(nextChildInstanceData);
					}
					else
					{
						if (m_MustRepeat)
						{
							ResetNode(InThisNodeInstanceData);
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
