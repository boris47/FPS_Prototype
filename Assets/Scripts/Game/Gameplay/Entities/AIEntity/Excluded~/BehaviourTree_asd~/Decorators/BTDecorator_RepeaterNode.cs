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
			[ReadOnly]
			public uint CurrentRepeatCount = 0u;
		}

		[SerializeField, Min(0u), Tooltip("[0-4294967295u], 0 (Zero) Means infinite"), ToNodeInspector]
		protected uint m_RepeatCount = 1u;


		//////////////////////////////////////////////////////////////////////////
		protected override RuntimeDataBase CreateRuntimeDataInstance(in BTNodeInstanceData InThisNodeInstanceData) => new RuntimeData();

		//////////////////////////////////////////////////////////////////////////
		protected override void OnAwakeInternal(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnAwakeInternal(InThisNodeInstanceData);

			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);

			nodeData.CurrentRepeatCount = 0u;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);

			nodeData.CurrentRepeatCount = 0u;

			return EBTNodeState.RUNNING;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;

			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
			BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, Child);
			
			switch (Child.UpdateNode(childInstanceData, InDeltaTime))
			{
				case EBTNodeState.FAILED:
				case EBTNodeState.SUCCEEDED:
				{
					if (m_RepeatCount > 0)
					{
						nodeData.CurrentRepeatCount++;
						if (nodeData.CurrentRepeatCount >= m_RepeatCount)
						{

							OutState = EBTNodeState.SUCCEEDED;
						}
						else
						{
							Child.ResetNode(childInstanceData);
						}
					}
					else
					{
						Child.ResetNode(childInstanceData);
					}
					break;
				}
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeReset(in BTNodeInstanceData InThisNodeInstanceData)
		{
			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
			BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, Child);

			nodeData.CurrentRepeatCount = 0u;

			Child.ResetNode(childInstanceData);
		}
	}
}
