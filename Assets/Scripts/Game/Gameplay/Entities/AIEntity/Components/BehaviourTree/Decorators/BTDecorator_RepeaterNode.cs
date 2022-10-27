using System.Collections;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	/// <summary>
	/// Repeater composite node. <br/>
	/// Repeater Nodes repeats their own child execution n-times ignoring the result. <br/>
	/// </summary>
	[BTNodeDetails("Repeater", "Repeats its child execution n-times ignoring the result")]
	public partial class BTDecorator_RepeaterNode : BTDecoratorNode
	{
		public class RuntimeData : RuntimeDataBase
		{
			public uint CurrentRepeatCount = 0u;
		}

		[SerializeField, Min(0u), Tooltip("[0-4294967295u], 0 (Zero) Means infinite"), ToNodeInspector]
		protected uint m_RepeatCount = 1u;


		//////////////////////////////////////////////////////////////////////////
		protected override RuntimeDataBase CreateRuntimeDataInstance(in BTNodeInstanceData InThisNodeInstanceData)
		{
			if (m_RepeatCount > 0)
			{
				return new RuntimeData();
			}

			return base.CreateRuntimeDataInstance(InThisNodeInstanceData);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			EBTNodeState OutState = base.OnActivation(InThisNodeInstanceData);
			if (OutState == EBTNodeState.RUNNING)
			{
				if (m_RepeatCount > 0)
				{
					RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
					nodeData.CurrentRepeatCount = 0u;
				}

			//	BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, Child);
			//	Child.ResetNode(childInstanceData);
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;

			BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, Child);

			if (m_RepeatCount > 0)
			{
				RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
				if (nodeData.CurrentRepeatCount >= m_RepeatCount)
				{
					OutState = EBTNodeState.SUCCEEDED;
				}
				else
				{
					if (BTNode.IsFinished(Child.UpdateNode(childInstanceData, InDeltaTime)))
					{
						nodeData.CurrentRepeatCount++;
					}
				}
			}
			// No repeat limit
			else
			{
				Child.UpdateNode(childInstanceData, InDeltaTime);
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeReset(in BTNodeInstanceData InThisNodeInstanceData)
		{
			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
			nodeData.CurrentRepeatCount = 0u;

			BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, Child);
			Child.ResetNode(childInstanceData);
		}
	}
}
