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
					OutState = EBTNodeState.FAILED;
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
					case EBTNodeState.FAILED:
					{
						++nodeData.CurrentIndex;
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
			}
			return OutState;
		}
	}
}
