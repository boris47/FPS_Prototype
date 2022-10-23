using System.Collections;
using System.Collections.Generic;
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
	public abstract partial class BTConditional : BTDecoratorNode
	{
		/// <summary>
		/// <para><b><see cref="EAbortType.None"/></b>: No abort logic</para>
		/// <para><b><see cref="EAbortType.Self"/></b>: While this node is running whenever the condition is not met running child is aborted and control returned to the parent</para>
		/// <para><b><see cref="EAbortType.LowerPriority"/></b>: On activation failed listen for condition and whenever is met request abort of running node and reset nodes till this node running this</para>
		/// </summary>
		[SerializeField, ToNodeInspector]
		protected		EAbortType				m_AbortType					= EAbortType.None;

		public			EAbortType				AbortType					=> m_AbortType;


		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnActivation()
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;
			bool bCanBeActivated = GetEvaluation();
			if (bCanBeActivated)
			{
				if (Child.IsNotNull())
				{
					if (m_AbortType == EAbortType.Self || m_AbortType == EAbortType.Both)
					{
						OnBecomeRelevant();
					}
					BehaviourTree.SetRunningNode(Child);
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
					OnBecomeRelevant();
				}
				OutState = EBTNodeState.FAILED;
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate()
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;

			EBTNodeState childState = Child.Update();
			switch (childState)
			{
				case EBTNodeState.SUCCEEDED:
				case EBTNodeState.FAILED:
				{
					// Reflect the child result to the parent of this node as it is
					OutState = childState;

				//	// And stop being relevant
					if (m_AbortType == EAbortType.Self)
					{
						OnCeaseRelevant();
					}
					break;
				}
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected EOnChangeDelExecutionResult AbortSelf()
		{
			if (m_AbortType == EAbortType.Self || m_AbortType == EAbortType.Both)
			{
				BehaviourTree.RequestAbort(this, Child, delegate
				{
					// Reset this node
					ResetNode();

					// Declare this node as failed
					SetNodeState(EBTNodeState.FAILED);

					// Notify the tree to run parent of this node
					BehaviourTree.SetRunningNode(Parent as BTNode);
				});
			}
			return m_AbortType == EAbortType.Self ? EOnChangeDelExecutionResult.REMOVE : EOnChangeDelExecutionResult.LEAVE;
		}

		//////////////////////////////////////////////////////////////////////////
		protected void AbortLowPriority()
		{
			if (m_AbortType == EAbortType.LowerPriority || m_AbortType == EAbortType.Both)
			{
				// Request the tree to abort current running node and move to this node, setting this node as running
				BehaviourTree.AbortLowPriorityNodesAndRunConditional(this, delegate
				{
					// Reset this node in order to start from a clean state
					ResetNode();

					// Overrides the parent current active node index with this node index
					if (Parent is BTCompositeNode parentAsComposite)
					{
						// Get index at parent level of this node
						int compositeIndex = parentAsComposite.Children.IndexOf(this);

						if (CustomAssertions.IsTrue(compositeIndex >= 0))
						{
							// And set this index as the current active node
							parentAsComposite.OverrideActiveChildIndex((uint)compositeIndex);
						}
					}

					// Wherever there is a child
					if (Child.IsNotNull())
					{
						// Skipping the OnInitialize re-check by setting this node as running directly
						SetNodeState(EBTNodeState.RUNNING);

						// Instead of adding this from base class we do it here
						BehaviourTree.SetRunningNode(this);
					}
					else
					{
						// By design this node is set as succeeded
						SetNodeState(EBTNodeState.SUCCEEDED);

						// Notify the tree to run parent of this node
						BehaviourTree.SetRunningNode(Parent as BTNode);
					}
				});
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected abstract bool GetEvaluation();

		//////////////////////////////////////////////////////////////////////////
		protected abstract void OnBecomeRelevant();

		//////////////////////////////////////////////////////////////////////////
		protected abstract void OnCeaseRelevant();
	}
}
