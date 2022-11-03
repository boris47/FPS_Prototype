
using System.Collections;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public enum ESuccessMode
	{
		Any,
		All,
	};

	public enum EFailureMode
	{
		Any,
		All,
	};

	/// <summary>
	/// Parallel composite node. <br/>
	/// Composite node which executes it's children in parallel
	/// </summary>
	[BTNodeDetails("Parallel Node", "Composite node which executes it's children in parallel")]
	public sealed partial class BTComposite_ParallelNode : BTCompositeNode
	{
		public const uint kMaxParallelChildrenCount = 7;

		[SerializeField, ToNodeInspector]
		private				ESuccessMode			m_SuccessMode			= ESuccessMode.All;

		[SerializeField, ToNodeInspector]
		private				EFailureMode			m_FailureMode			= EFailureMode.Any;

		class ParallelNodeData : RuntimeData
		{
			public BitArray RunningChildren = new BitArray((int)kMaxParallelChildrenCount);  // Each bit is a child
			
			[ReadOnly]
			public int SuccessCount = 0;
			
			[ReadOnly]
			public int FailureCount = 0;
		}


		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeInitializationResult OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			EBTNodeInitializationResult OutState = base.OnActivation(InThisNodeInstanceData);
			if (OutState == EBTNodeInitializationResult.RUNNING)
			{
				ParallelNodeData nodeData = GetRuntimeData<ParallelNodeData>(InThisNodeInstanceData);

				nodeData.RunningChildren.SetAll(false);
				nodeData.SuccessCount = 0;
				nodeData.FailureCount = 0;

				for (int i = 0, count = Children.Length; i < count; ++i)
				{
					nodeData.RunningChildren.Set(i, true); // On actiovation set as started
				}

			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnNodeUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;
			{
				ParallelNodeData nodeData = GetRuntimeData<ParallelNodeData>(InThisNodeInstanceData);
				for (int index = 0, count = Children.Length; index < count; ++index)
				{
					bool childIsRunning = nodeData.RunningChildren.Get(index);
					if (childIsRunning)
					{
						BTNode child = Children[index];
						BTNodeInstanceData childInstanceData = GetNodeInstanceData(InThisNodeInstanceData, child);
						EBTNodeState childState = child.UpdateNode(childInstanceData, InDeltaTime);
						if (BTNode.IsFinished(childState))
						{
							if (childState == EBTNodeState.SUCCEEDED)
							{
								++nodeData.SuccessCount;
							}
							else
							{
								++nodeData.FailureCount;
							}
							nodeData.RunningChildren.Set(index, false);
						}
					}
				}

				// Success or Restart
				if ((m_SuccessMode == ESuccessMode.All && nodeData.SuccessCount == Children.Length)
					||
					(m_SuccessMode == ESuccessMode.Any && nodeData.SuccessCount > 0)
				)
				{
					if (MustRepeat)
					{
						ResetNode(InThisNodeInstanceData);
					}
					else
					{
						OutState = EBTNodeState.SUCCEEDED;
					}
				}

				// Failure or Restart
				if ((m_FailureMode == EFailureMode.All && nodeData.FailureCount == Children.Length)
					||
					(m_FailureMode == EFailureMode.Any && nodeData.FailureCount > 0)
				)
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
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeAbort(in BTNodeInstanceData InThisNodeInstanceData)
		{
			ParallelNodeData nodeData = GetRuntimeData<ParallelNodeData>(InThisNodeInstanceData);
			for (int i = 0, count = Children.Length; i < count; ++i)
			{
				if (nodeData.RunningChildren.Get(i))
				{
					BTNode child = Children[i];
					BTNodeInstanceData childInstanceData = GetNodeInstanceData(InThisNodeInstanceData, child);
					child.AbortAndResetNode(childInstanceData);
				}
			}
			base.OnNodeAbort(InThisNodeInstanceData);
		}
	}
}

#if UNITY_EDITOR
namespace Entities.AI.Components.Behaviours
{
	using UnityEditor;

	public partial class BTComposite_ParallelNode
	{
		//////////////////////////////////////////////////////////////////////////
		protected override bool ShouldSortChildren() => false;
	}
}
#endif
