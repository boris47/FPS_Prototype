
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
	[BTNodeDetails("Two Parallel", "Allows for running two children: one which must be a single task node, and the other of which can be a composite")]
	public sealed partial class BTComposite_TwoParallelNode : BTCompositeNode
	{		
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
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override EBTNodeState OnActivation()
		{
			EBTNodeState OutState = base.OnActivation();
			if (OutState == EBTNodeState.RUNNING)
			{
				m_Main = null;
				m_Background = null;

				if (Children.IsValidIndex(1))
				{
					if (!(Children.At(0) is BTTaskNode) || !(Children.At(1) is BTCompositeNode))
					{
						OutState = EBTNodeState.FAILED;
						Debug.LogError("TwoParallelNode: "
						+ $"Expected first child of type {nameof(BTTaskNode)}, got {Children.At(0).GetType().Name};"
						+ ' '
						+ $"Expected second child of type {nameof(BTCompositeNode)}, got {Children.At(1).GetType().Name};"
						);
					}
					else
					{
						m_Main = Children[0] as BTTaskNode;
						m_Background = Children[1] as BTCompositeNode;
						BehaviourTree.LockRunningNode(this);
					}
				}
			}
			else
			{
				Debug.LogError($"Cannot activare node {nameof(BTComposite_TwoParallelNode)} because not enough children assigned");
				OutState = EBTNodeState.FAILED;
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
							m_Background.RequestAbortNode(bAbortImmediately: false);
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
						m_Main.RequestAbortNode(bAbortImmediately: true);
						m_Background.RequestAbortNode(bAbortImmediately: true);
						OutState = EBTNodeState.FAILED;
						Utils.CustomAssertions.IsTrue(false, this, $"Unsupported ParallelMode {m_ParallelMode}");
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

			BehaviourTree.UnLockRunningNode(this);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnAbortNodeRequested(in bool bAbortImmediately)
		{
			if (!BTNode.IsFinished(m_Main))
			{
				m_Main.RequestAbortNode(bAbortImmediately: false);
			}

			if (!BTNode.IsFinished(m_Background))
			{
				m_Background.RequestAbortNode(bAbortImmediately: false);
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
