
using System.Linq;
using System.Collections;
using System.Collections.Generic;
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

	/*
	public enum EBTParallelMode
	{
		/// <summary> When main task finishes, immediately abort background tree. </summary>
		AbortBackground,
		/// <summary> When main task finishes, wait for background tree to finish. </summary>
		WaitForBackground,
	}
	*/

	/// <summary>
	/// Two Parallel composite node. <br/>
	/// Allows for running two children: one which must be a single action node, and the other of which can be a composite. <br/>
	/// </summary>
	[BTNodeDetails("Two Parallel", "Allows for running two children: one which must be a single task node, and the other of which can be a composite")]
	public sealed partial class BTComposite_TwoParallelNode : BTCompositeNode
	{
		public const uint kMaxParallelChildrenCount = 7;
		/*
		[SerializeField, ToNodeInspector(Label: "Mode")]
		private				EBTParallelMode			m_ParallelMode			= EBTParallelMode.AbortBackground;

		[SerializeField, ReadOnly, ToNodeInspector]
		private				BTTaskNode				m_Main					= null;

		[SerializeField, ReadOnly, ToNodeInspector]
		private				BTCompositeNode			m_Background			= null;


		//---------------------
		protected override	int						MinimumChildrenCount	=> 2;
		*/

		[SerializeField, ToNodeInspector]
		private ESuccessMode m_SuccessMode = ESuccessMode.All;

		[SerializeField, ToNodeInspector]
		private EFailureMode m_FailureMode = EFailureMode.Any;

		class ParallelNodeData : RuntimeData
		{
			public BitArray RunningChildren = new BitArray((int)kMaxParallelChildrenCount);  // Each bit is a child
			
			[ReadOnly]
			public int SuccessCount = 0;
			
			[ReadOnly]
			public int FailureCount = 0;
		}



		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			EBTNodeState OutState = base.OnActivation(InThisNodeInstanceData);
			if (OutState == EBTNodeState.RUNNING)
			{
				ParallelNodeData nodeData = GetRuntimeData<ParallelNodeData>(InThisNodeInstanceData);

				nodeData.RunningChildren.SetAll(false);
				nodeData.SuccessCount = 0;
				nodeData.FailureCount = 0;

				for (int i = 0, count = Children.Count; i < count; ++i)
				{
					nodeData.RunningChildren.Set(i, true); // On actiovation set as started
				}

			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnTerminate(in BTNodeInstanceData InThisNodeInstanceData)
		{
			ParallelNodeData nodeData = GetRuntimeData<ParallelNodeData>(InThisNodeInstanceData);
			for (int i = 0, count = Children.Count; i < count; ++i)
			{
				if (nodeData.RunningChildren.Get(i))
				{
					BTNode child = Children.At(i);

					BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, child);
					child.ResetNode(childInstanceData);
				}
			}
			base.OnTerminate(InThisNodeInstanceData);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnAbortNodeRequested(in BTNodeInstanceData InThisNodeInstanceData)
		{
			ParallelNodeData nodeData = GetRuntimeData<ParallelNodeData>(InThisNodeInstanceData);
			for (int i = 0, count = Children.Count; i < count; ++i)
			{
				if (nodeData.RunningChildren.Get(i))
				{
					BTNode child = Children.At(i);

					BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, child);

					child.RequestAbortNode(childInstanceData);
					child.ResetNode(childInstanceData);
				}
			}
			base.OnAbortNodeRequested(InThisNodeInstanceData);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;
			{
				ParallelNodeData nodeData = GetRuntimeData<ParallelNodeData>(InThisNodeInstanceData);
				for (int index = 0, count = Children.Count; index < count; ++index)
				{
					bool childIsRunning = nodeData.RunningChildren.Get(index);
					if (childIsRunning)
					{
						BTNode child = Children.At(index);

						BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, child);
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

				if ((m_SuccessMode == ESuccessMode.All && nodeData.SuccessCount == Children.Count)
					||
					(m_SuccessMode == ESuccessMode.Any && nodeData.SuccessCount > 0)
				)
				{
					OutState = EBTNodeState.SUCCEEDED;
				}

				if ((m_FailureMode == EFailureMode.All && nodeData.FailureCount == Children.Count)
					||
					(m_FailureMode == EFailureMode.Any && nodeData.FailureCount > 0)
				)
				{
					OutState = EBTNodeState.FAILED;
				}
			}
			return OutState;
		}
	}
}
