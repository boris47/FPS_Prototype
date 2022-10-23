
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public enum EBTParallelMode
	{
		/// <summary> When main task finishes, immediately abort background tree. </summary>
		AbortBackground,
		/// <summary> When main task finishes, wait for background tree to finish. </summary>
		WaitForBackground,
	}

	/// <summary>
	/// Two Parallel composite node. <br/>
	/// Allows for running two children: one which must be a single action node, and the other of which can be a composite. <br/>
	/// </summary>
	public sealed partial class BTComposite_TwoParallelNode : BTCompositeNode
	{
		public override string NodeName => "Two Parallel";
		public override string NodeInfo => "Allows for running two children: one which must be a single task node, and the other of which can be a composite";
		
		[SerializeField, ToNodeInspector(Label: "Mode")]
		private				EBTParallelMode			m_ParallelMode			= EBTParallelMode.AbortBackground;

		[SerializeField, ReadOnly, ToNodeInspector]
		private				BTTaskNode				m_Main					= null;

		[SerializeField, ReadOnly, ToNodeInspector]
		private				BTCompositeNode			m_Background			= null;


		//---------------------
		protected override	int						MinimumChildrenCount	=> 2;


		//////////////////////////////////////////////////////////////////////////
		protected sealed override void CopyDataToInstance(in BTNode InNewInstance)
		{
			base.CopyDataToInstance(InNewInstance);
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override void OnAwakeInternal(in BehaviourTree InBehaviourTree)
		{
			base.OnAwakeInternal(InBehaviourTree);

			if (CustomAssertions.IsTrue(Children.IsValidIndex(1)))
			{
				if (Children.At(0) is BTTaskNode task)
				{
					m_Main = task;
				}

				if (Children.At(1) is BTCompositeNode composite)
				{
					m_Background = Children.At(1) as BTCompositeNode;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override EBTNodeState OnActivation()
		{
			EBTNodeState OutState = base.OnActivation();
			if (OutState == EBTNodeState.RUNNING)
			{
				BehaviourTree.LockRunningNode();
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override EBTNodeState OnUpdate()
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;

			if (!BTNode.IsFinished(m_Main))
			{
				m_Main.Update();

				if (!BTNode.IsFinished(m_Background))
				{
					m_Background.Update();
				}
			}
			else
			{
				switch (m_ParallelMode)
				{
					case EBTParallelMode.AbortBackground:
					{
						if (m_Background.NodeState != EBTNodeState.ABORTING && m_Background.NodeState != EBTNodeState.ABORTED)
						{
							m_Background.BeginAbortNode();
						}
						else
						{
							if (!BTNode.IsFinished(m_Background))
							{
								m_Background.Update();
							}
							else
							{
								if (m_MustRepeat)
								{
									ResetNode();
								}
								else
								{
									OutState = m_Main.NodeState == EBTNodeState.SUCCEEDED && m_Background.NodeState == EBTNodeState.ABORTED ? EBTNodeState.SUCCEEDED : EBTNodeState.FAILED;
								}
							}
						}
						break;
					}
					case EBTParallelMode.WaitForBackground:
					{
						if (!BTNode.IsFinished(m_Background))
						{
							m_Background.Update();
						}
						else
						{
							if (m_MustRepeat)
							{
								ResetNode();
							}
							else
							{
								OutState = m_Main.NodeState == EBTNodeState.SUCCEEDED && m_Background.NodeState == EBTNodeState.SUCCEEDED ? EBTNodeState.SUCCEEDED : EBTNodeState.FAILED;
							}
						}
						break;
					}
					default:
					{
						m_Main.BeginAbortNode();
						m_Background.BeginAbortNode();
						OutState = EBTNodeState.FAILED;
						CustomAssertions.IsTrue(false, $"Unsupported ParallelMode {m_ParallelMode}", this);
						break;
					}
				}
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override void OnTerminate()
		{
			base.OnTerminate();

			BehaviourTree.UnLockRunningNode();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnBeginAbortNode()
		{
			if (!BTNode.IsFinished(m_Main))
			{
				m_Main.BeginAbortNode();
			}

			if (!BTNode.IsFinished(m_Background))
			{
				m_Background.BeginAbortNode();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdateAborting()
		{
			if (!BTNode.IsFinished(m_Main))
			{
				m_Main.Update();
			}

			if (!BTNode.IsFinished(m_Background))
			{
				m_Background.Update();
			}

			return BTNode.IsFinished(m_Main) && BTNode.IsFinished(m_Background) ? EBTNodeState.ABORTED : EBTNodeState.ABORTING;
		}
	}
}
