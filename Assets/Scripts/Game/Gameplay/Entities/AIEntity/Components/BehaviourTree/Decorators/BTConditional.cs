using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[System.Serializable]
	public enum EAbortType
	{
		/// <summary> The Conditional task will not be reevaluated and no aborts will be issued. </summary>
		None,

		/// <summary>
		/// The current conditional is re-evaluated constantly and as soon the condition is not met anymore it will abort the current branch<br/>
		///	Moving execution to next parent child branch
		/// </summary>
		Self,

		/// <summary>
		/// The current conditional is re-evaluated constantly even when the current active node is on another branch with lower priority<br/>
		/// When the conditional is satisfied again then it will abort other lower priority nodes tasks
		/// </summary>
		LowerPriority,

		/// <summary>
		/// When activating if the condition is met child is executed, whenever is not met and child is running abort this child. <br/>
		/// Otherwise on activation if condition is not met then return failure but await for condition to met again.<br/>
		/// Once is met again trigger a conditional flow abort
		/// </summary>
		Both
	}

	[System.Serializable]
	[BTNodeDetails("Conditional")]
	public abstract partial class BTConditional : BTDecoratorNode
	{
		/// <summary>
		/// <para><b><see cref="EAbortType.None"/></b>: No abort logic</para>
		/// <para><b><see cref="EAbortType.Self"/></b>: While this node is running whenever the condition is not met running child is aborted and control returned to the parent</para>
		/// <para><b><see cref="EAbortType.LowerPriority"/></b>: On activation failed listen for condition and whenever is met request request a low priority abort in favor of this node</para>
		/// </summary>
		[SerializeField, ToNodeInspector]
		protected		EAbortType				m_AbortType					= EAbortType.None;

		public			EAbortType				AbortType					=> m_AbortType;


		//////////////////////////////////////////////////////////////////////////
		protected sealed override EBTNodeState OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;
			bool bCanBeActivated = GetEvaluation(InThisNodeInstanceData);
			if (bCanBeActivated)
			{
				if (Child.IsNotNull())
				{
					if (m_AbortType == EAbortType.Self || m_AbortType == EAbortType.Both)
					{
						StartObserve(InThisNodeInstanceData);
					}
					// Setting this node as running because SetRunningNode require running parent
					InThisNodeInstanceData.SetNodeState(EBTNodeState.RUNNING);
					InThisNodeInstanceData.BehaviourTreeInstanceData.SetRunningNode(InThisNodeInstanceData);
				}
				else
				{
					// In this way the node can be used as a checkpoint where to restart when condition is met
					OutState = EBTNodeState.SUCCEEDED;
				}
			}
			else
			{
				if (m_AbortType == EAbortType.LowerPriority || m_AbortType == EAbortType.Both)
				{
					StartObserve(InThisNodeInstanceData);
				}
				OutState = EBTNodeState.FAILED;
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;
			{
				if (Child.IsNotNull())
				{
					BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, Child);
					EBTNodeState childState = Child.UpdateNode(childInstanceData, InDeltaTime);
					if (BTNode.IsFinished(childState))
					{
						// Reflect the child result to the parent of this node as it is
						OutState = childState;

						// And stop being relevant
						if (m_AbortType == EAbortType.Self)
						{
							StopObserve(InThisNodeInstanceData);
						}
					}
				}
				// No child, success as result by design
				else
				{
					OutState = EBTNodeState.SUCCEEDED;
				}
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnTerminateSuccess(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnTerminateSuccess(InThisNodeInstanceData);

			// Event if this node is terminated with success, only if abort type is Self we stop observing
			// So that, if abort type is low prioritythe abort request request can be sent anyway
			if (m_AbortType == EAbortType.Self)
			{
				StopObserve(InThisNodeInstanceData);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeReset(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnNodeReset(InThisNodeInstanceData);

			// On this node reset we want to stop observing whatever is the abort condition
			StopObserve(InThisNodeInstanceData);
		}

		//////////////////////////////////////////////////////////////////////////
		protected EOnChangeDelExecutionResult AbortSelf(in BTNodeInstanceData InThisNodeInstanceData)
		{
			if (m_AbortType == EAbortType.Self || m_AbortType == EAbortType.Both)
			{
				// We  expect this node to be running to abort itself
				if (Utils.CustomAssertions.IsTrue(InThisNodeInstanceData.NodeState == EBTNodeState.RUNNING))
				{
					// Abort child node if there is one
					if (Child.IsNotNull())
					{
						BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, Child);
						Child.AbortAndResetNode(childInstanceData);
					}

					// Reset this node
				//	ResetNode(InThisNodeInstanceData);

					// Declare this node as failed
					InThisNodeInstanceData.SetNodeState(EBTNodeState.FAILED);

					// Notify the tree to run parent of this node
					InThisNodeInstanceData.BehaviourTreeInstanceData.SetRunningNode(InThisNodeInstanceData.ParentInstanceData);
				}
			}
			return m_AbortType == EAbortType.Self ? EOnChangeDelExecutionResult.REMOVE : EOnChangeDelExecutionResult.LEAVE;
		}

		//////////////////////////////////////////////////////////////////////////
		protected void RequestLowPriorityAbort(BTNodeInstanceData InThisNodeInstanceData)
		{
			if (m_AbortType == EAbortType.LowerPriority || m_AbortType == EAbortType.Both)
			{
				// Request the tree to abort current running node and move to this node, setting this node as running
				// The request will be processed in the next tree update
				BehaviourTreeAsset.RequestExecution(InThisNodeInstanceData, OnLowPriorityAbortRequestAccepted);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnLowPriorityAbortRequestAccepted(BTNodeInstanceData InThisNodeInstanceData)
		{
			if (Utils.CustomAssertions.IsTrue(m_AbortType == EAbortType.LowerPriority || m_AbortType == EAbortType.Both))
			{
				// Reset this node in order to start from a clean state
			//	ResetNode(InThisNodeInstanceData);

				// Overrides the parent current active node index with this node index
				if (InThisNodeInstanceData.ParentInstanceData.NodeAsset is BTCompositeNode parentAsComposite)
				{
					// Get index at parent level of this node
					int compositeIndex = parentAsComposite.IndexOf(this);

					if (Utils.CustomAssertions.IsTrue(compositeIndex >= 0))
					{
						// And set this index as the current active node
						parentAsComposite.OverrideActiveChildIndex(InThisNodeInstanceData.ParentInstanceData, (uint)compositeIndex);
					}
				}

				// Wherever there is a child
				if (Child.IsNotNull())
				{
					BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, Child);

					// Skipping the OnInitialize re-check by setting this node as running directly
					InThisNodeInstanceData.SetNodeState(EBTNodeState.RUNNING);

					// Instead of adding this from base class we do it here
					InThisNodeInstanceData.BehaviourTreeInstanceData.SetRunningNode(childInstanceData);
				}
				else
				{
					// By design this node is set as succeeded
					InThisNodeInstanceData.SetNodeState(EBTNodeState.SUCCEEDED);

					// Notify the tree to run parent of this node
					InThisNodeInstanceData.BehaviourTreeInstanceData.SetRunningNode(InThisNodeInstanceData.ParentInstanceData);
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual bool GetEvaluation(in BTNodeInstanceData InInstanceData) => false;

		//////////////////////////////////////////////////////////////////////////
		protected virtual void StartObserve(in BTNodeInstanceData InInstanceData) { }

		//////////////////////////////////////////////////////////////////////////
		protected virtual void StopObserve(in BTNodeInstanceData InInstanceData) { }

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnTick(BTNodeInstanceData InThisNodeInstanceData, float InDeltaTime) { }
	}
}

#if UNITY_EDITOR
namespace Entities.AI.Components.Behaviours
{
	using UnityEditor;

	public abstract partial class BTConditional // Editor
	{

	}
}
#endif
