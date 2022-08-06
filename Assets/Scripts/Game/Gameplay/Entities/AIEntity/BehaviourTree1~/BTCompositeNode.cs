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
	}

	public abstract partial class BTCompositeNode : BTNode, IParentNode
	{
		[SerializeField, ReadOnly/*, HideInInspector*/]
		private			List<BTNode>			m_Children					= new List<BTNode>();

		[SerializeField, ReadOnly]
		private			BTConditional			m_Conditional				= null;

		[SerializeField, ReadOnly]
		protected		uint					m_CurrentIndex				= 0;

		/// <summary>
		/// <para><b><see cref="EAbortType.None"/></b>: The call at start evaluate once</para>
		/// <para><b><see cref="EAbortType.Self"/></b>: The call at start evaluate and on failure register re-evaluation every tick and whenever the conditions is meet again<br/>
		/// notify behaviour tree to abort children of this composite</para>
		/// <para><b><see cref="EAbortType.LowerPriority"/></b>: The call at start evaluate and of failure register re-evaluation every tick and whenever the conditions is meet again<br/>
		/// notify behaviour tree to abort every node in the queue until this composite</para>
		/// </summary>
		[SerializeField]
		protected		EAbortType				m_AbortType					= EAbortType.None;

		//---------------------
		public			List<BTNode>			Children					=> m_Children;
		public			BTConditional			Conditional					=> m_Conditional;
		public			uint					CurrentIndex				=> m_CurrentIndex;
		public			EAbortType				AbortType					=> m_AbortType;

		//---------------------
//		private			bool					bIsLowerPriorityActivation	= false;

		//////////////////////////////////////////////////////////////////////////
		/// <summary> This method is called just before this composite is re-activated after a conditional abort </summary>
		public void ActivateByConditionalAbort()
		{
			// Overrides the parent current active node index with this node index
			if (Parent is BTCompositeNode parentAsComposite)
			{
				// Get index at parent level of this node
				uint compositeIndex = ((uint)parentAsComposite.Children.IndexOf(this));

				// And set this index as the current active node
				parentAsComposite.OverrideActiveChildIndex(compositeIndex);
			}

			// Reset the node so when restarting its state is clean.
			ResetNode();

			// Skipping the OnInitialize re-check by setting this node as running directly
			m_NodeState = EBTNodeState.RUNNING;

			// Instead of adding this from base class we do it here
			m_BehaviourTree.AddVisitedNode(this);
		}

		//////////////////////////////////////////////////////////////////////////
		public void OverrideActiveChildIndex(in uint InChildIndex) => m_CurrentIndex = InChildIndex;

		//////////////////////////////////////////////////////////////////////////
	//	public sealed override List<BTNode> GetChildren() => new List<BTNode>(m_Children);

		//////////////////////////////////////////////////////////////////////////
		public override void ResetNode()
		{
			m_Children.ForEach(c => c.ResetNode());

			base.ResetNode();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void CopyDataToInstance(in BTNode InNewInstance)
		{
			var node = InNewInstance as BTCompositeNode;
			node.m_Children = m_Children.ConvertAll(c => c.CloneInstance(node));
			node.m_CurrentIndex = 0;
			node.m_AbortType = m_AbortType;
			if (m_Conditional.IsNotNull())
			{
				node.m_Conditional = m_Conditional.CloneInstance();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnAwakeInternal(in BehaviourTree InBehaviourTree)
		{
			m_Conditional?.OnAwake(this, InBehaviourTree);
			foreach(var child in m_Children)
			{
				child.OnAwake(InBehaviourTree);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override bool OnTryActivation()
		{
			m_CurrentIndex = 0u;

			bool bCanBeActivated = true;

			// If conditional is assigned
			if (m_Conditional.IsNotNull())
			{
				// Evaluate the conditional
				bCanBeActivated = m_Conditional.GetEvaluation();

				// At this point, because we are not entering the node
				// we'll setup a listener for lower priority conditional abort check
				if (!bCanBeActivated)
				{
					// If abort type is 'priority' and IF our parent is not the root node,
					// We register a re-evaluation callback to be repeated in time
					if (m_AbortType == EAbortType.LowerPriority && !(Parent is BTRootNode))
					{
						m_Conditional.OnEnableObserver();
						BehaviourTree.AddConditionalComposite(this);
					}
					bCanBeActivated = false;
				}
			}

			if (bCanBeActivated)
			{
				bCanBeActivated = m_Children.At(m_CurrentIndex).TryActivation();
			}
			return bCanBeActivated;
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override EBTNodeState PreUpdate()
		{
			CustomAssertions.IsTrue(m_Children.IsValidIndex(m_CurrentIndex));
			CustomAssertions.IsTrue(NodeState == EBTNodeState.RUNNING && BehaviourTree.IsNodeRunning(this));

			EBTNodeState outState = NodeState;

			// If conditional is assigned and abort type is 'Self' then
			if (m_Conditional.IsNotNull() && m_AbortType == EAbortType.Self)
			{
				// Evaluate the conditional
				bool bConditionalEvaluationResult = m_Conditional.GetEvaluation();
				if (!bConditionalEvaluationResult)
				{
					// At this point we no longer are allowed to update current child
					// 1) We need to abort the current running node and
					m_Children.At(m_CurrentIndex).AbortNode();

					// 2) after complete reset this node
					ResetNode();

					// 3) return failure state so the next child can be executed
					outState = EBTNodeState.FAILED;
				}
			}
			return outState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override EBTNodeState Update()
		{
			if (m_NodeState == EBTNodeState.RUNNING)
			{
				BTNode child = Children.At(m_CurrentIndex);
				if (child.NodeState == EBTNodeState.INACTIVE)
				{
					m_NodeState = child.TryActivation() ? EBTNodeState.RUNNING : EBTNodeState.FAILED;
				}
			}
			return m_NodeState;
		}

		//////////////////////////////////////////////////////////////////////////
		public abstract void OnChildFinished(in BTNode InNode, in EBTNodeState InChildState);

		//////////////////////////////////////////////////////////////////////////
		protected override void OnTerminate(in bool bIsAbort)
		{
			base.OnTerminate(bIsAbort);

			if (bIsAbort)
			{
				m_Children.At(m_CurrentIndex).AbortNode();
			}
			else
			{

			}
		}
	}
}
