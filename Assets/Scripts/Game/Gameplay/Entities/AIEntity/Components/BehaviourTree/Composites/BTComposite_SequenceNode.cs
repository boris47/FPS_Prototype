
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
		protected override EBTNodeState OnUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;

			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
			if (nodeData.CurrentIndex >= Children.Count)
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
			else
			{
				BTNode child = Children[(int)nodeData.CurrentIndex];
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
						++nodeData.CurrentIndex;
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
			}
			return OutState;
		}
	}
}
