
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public partial class BehaviourTree : ScriptableObject
	{
		[System.Serializable]
		public enum EBehaviourTreeState
		{
			STOPPED,
			RUNNING,
			PAUSED,
			INVALID
		}

		[SerializeField, ReadOnly]
		private			BTRootNode				m_RootNode				= null;

		[SerializeField/*, HideInInspector*/]
		private			List<BTNode>			m_Nodes					= new List<BTNode>();

		[SerializeField, ReadOnly]
		private			AIController			m_Controller			= null;

		[SerializeField, ReadOnly]
		private			EBehaviourTreeState		m_TreeState				= EBehaviourTreeState.INVALID;

		[SerializeField, ReadOnly]
		private			bool					m_IsInstance			= false;

		[SerializeField, ReadOnly]
		private			BTNode					m_CurrentRunningNode	= null;

		[SerializeField, ReadOnly]
		private			bool					m_RunningNodeLocked		= false;

		//---------------------
		public			bool					IsInstance				=> m_IsInstance;
		public			AIController			Owner					=> m_Controller;


		//---------------------
		private			AbortOperation			m_AbortOperation		= null;


		//////////////////////////////////////////////////////////////////////////
		public static BehaviourTree CreateBehaviourTreeInstance(in BehaviourTree InBehaviourTreeSource)
		{
			static void Traverse(in BTNode node, System.Action<BTNode> visiter)
			{
				visiter(node);
				if (node is IParentNode parent)
				{
					parent.Children.ForEach(n => Traverse(n, visiter));
				}
			}

			BehaviourTree newTreeInstance = Instantiate(InBehaviourTreeSource);
#if UNITY_EDITOR
			newTreeInstance.m_Source = InBehaviourTreeSource;
#endif
			newTreeInstance.m_IsInstance = true;

			// Root node
			newTreeInstance.m_RootNode = InBehaviourTreeSource.m_RootNode?.CloneInstance() as BTRootNode;

			// Other Nodes
			newTreeInstance.m_Nodes.Clear();

			if (newTreeInstance.m_RootNode)
			{
				Traverse(newTreeInstance.m_RootNode, n =>
				{
					newTreeInstance.m_Nodes.Add(n);
				});
			}

			// Return the instance
			return newTreeInstance;
		}

		//////////////////////////////////////////////////////////////////////////
		public void OnAwake(in AIController InController)
		{
			if (!m_IsInstance)
			{
				Debug.LogError($"Cannot awake a not tree instance", this);
				return;
			}

			m_Controller = InController;

			m_Nodes.ForEach(n => n.OnAwake(this));
			m_RootNode?.OnAwake(this);
			m_TreeState = EBehaviourTreeState.STOPPED;
		}

		//////////////////////////////////////////////////////////////////////////
		public void StartTree()
		{
			if (!m_IsInstance)
			{
				Debug.LogError($"Cannot start a not tree instance", this);
				return;
			}

			if (m_TreeState == EBehaviourTreeState.STOPPED)
			{
				if (CustomAssertions.IsNotNull(GameManager.CyclesEvents) && CustomAssertions.IsTrue(m_Nodes.At(0) == m_RootNode))
				{
					m_TreeState = EBehaviourTreeState.RUNNING;
					m_RootNode.ResetNode();
					SetRunningNode(m_Nodes.At(0));
					GameManager.CyclesEvents.OnFrame += UpdateTree;
				}
			}
			else
			{
				Debug.LogError($"Is being trying to start a behaviour tree with state {m_TreeState}", this);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void LockRunningNode() => m_RunningNodeLocked = true;

		//////////////////////////////////////////////////////////////////////////
		public void UnLockRunningNode() => m_RunningNodeLocked = false;

		//////////////////////////////////////////////////////////////////////////
		public void SetRunningNode(in BTNode InNode)
		{
			if (!m_RunningNodeLocked)
			{
				if (InNode.IsNotNull() && InNode.Parent is BTNode parent && parent.NodeState != EBTNodeState.RUNNING)
				{
					CustomAssertions.IsTrue(false);
					return;
				}

				m_CurrentRunningNode = InNode;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void UpdateTree(float InDeltaTime)
		{
			if (m_AbortOperation.IsNotNull())
			{
				if (m_AbortOperation.Completed())
				{
					m_AbortOperation = null;
				}
			}
			else
			{
				m_CurrentRunningNode?.Update();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void RequestAbort(in BTNode InNodeAborter, in BTNode InNodeToAbort, in System.Action InPostAbortOp)
		{
			if (m_AbortOperation == null)
			{
				m_AbortOperation = new AbortOperation(InNodeAborter, InNodeToAbort, InPostAbortOp);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void AbortLowPriorityNodesAndRunConditional(BTConditional InConditionalRequestingLowPriorityAbort, in System.Action InPostAbort)
		{
			bool bValidAbortType = CustomAssertions.IsTrue(InConditionalRequestingLowPriorityAbort.AbortType == EAbortType.LowerPriority || InConditionalRequestingLowPriorityAbort.AbortType == EAbortType.Both);
			uint conditionalToActivateIndex = InConditionalRequestingLowPriorityAbort.NodeIndex;
			uint currentRunningNodeIndex = m_CurrentRunningNode.NodeIndex;
			bool bValidIndex = CustomAssertions.IsTrue(conditionalToActivateIndex < currentRunningNodeIndex);

			if (bValidIndex && bValidAbortType)
			{
				RequestAbort(InConditionalRequestingLowPriorityAbort, m_CurrentRunningNode, delegate
				{
					for (uint i = currentRunningNodeIndex; i > conditionalToActivateIndex; i--)
					{
						m_Nodes.At(i).ResetNode();
					}

					m_CurrentRunningNode = InConditionalRequestingLowPriorityAbort;
				} + InPostAbort);
			}
		}

		//---------------------
		private class AbortOperation
		{
			public readonly BTNode m_NodeAborter = null;
			public readonly BTNode m_NodeToAbort = null;
			public readonly System.Action m_PostAbortOp = null;

			//////////////////////////////////////////////////////////////////////////
			public AbortOperation(in BTNode InNodeAborter, in BTNode InNodeToAbort, in System.Action InPostAbortOp)
			{
				CustomAssertions.IsNotNull(InNodeAborter);
				CustomAssertions.IsNotNull(InNodeToAbort);
				CustomAssertions.IsNotNull(InPostAbortOp);

				m_NodeAborter = InNodeAborter;
				m_NodeToAbort = InNodeToAbort;
				m_PostAbortOp = InPostAbortOp;

				m_NodeToAbort.BeginAbortNode();
				CustomAssertions.IsTrue(m_NodeToAbort.NodeState == EBTNodeState.ABORTING || m_NodeToAbort.NodeState == EBTNodeState.ABORTED);
			}

			//////////////////////////////////////////////////////////////////////////
			public bool Completed()
			{
				bool OutResult = m_NodeToAbort.Update() == EBTNodeState.ABORTED;
				if (OutResult)
				{
					m_PostAbortOp();
				}
				return OutResult;
			}
		}
	}


	/*
	// 
	// 		//////////////////////////////////////////////////////////////////////////
	// 		public void PauseTree()
	// 		{
	// 			if (m_TreeState == EBehaviourTreeState.RUNNING)
	// 			{
	// 				m_TreeState = EBehaviourTreeState.PAUSED;
	// 
	// 				GameManager.CyclesEvents.OnThink -= OnThink;
	// 
	// 				m_VisitedNodes.Peek().OnTreePause();
	// 			}
	// 		}
	// 
	// 		//////////////////////////////////////////////////////////////////////////
	// 		public void ResumeTree()
	// 		{
	// 			if (m_TreeState == EBehaviourTreeState.PAUSED)
	// 			{
	// 				m_TreeState = EBehaviourTreeState.RUNNING;
	// 
	// 				GameManager.CyclesEvents.OnThink += OnThink;
	// 
	// 				m_VisitedNodes.Peek().OnTreeResume();
	// 			}
	// 		}
	// 
	// 		//////////////////////////////////////////////////////////////////////////
	// 		public void StopTree()
	// 		{
	// 			if (m_TreeState == EBehaviourTreeState.RUNNING || m_TreeState == EBehaviourTreeState.PAUSED)
	// 			{
	// 				m_TreeState = EBehaviourTreeState.STOPPED;
	// 
	// 				m_VisitedNodes.Peek().OnTreeStop();
	// 				m_VisitedNodes.Clear();
	// 
	// 				m_RootNode?.ResetNode();
	// 
	// 				ResetTree();
	// 			}
	// 		}
	// 
	// 		//////////////////////////////////////////////////////////////////////////
	// 		public void ResetTree()
	// 		{
	// 			if (m_TreeState == EBehaviourTreeState.RUNNING || m_TreeState == EBehaviourTreeState.PAUSED)
	// 			{
	// 				StopTree();
	// 			}
	// 			m_RootNode?.ResetNode();
	// 			m_VisitedNodes.Clear();
	// 		}
	// 
	// 		//////////////////////////////////////////////////////////////////////////
	// 		public void RestartTree()
	// 		{
	// 			ResetTree();
	// 			StartTree();
	// 		}
	*/
}



/* https://www.researchgate.net/publication/340870872_An_event-driven_behavior_trees_extension_to_facilitate_non-player_multi-agent_coordination_in_video_games
Consider an NPC exploring the terrain until it ﬁnds a nearby target to follow.
Suppose that the blackboard is initially empty and that there are no targets near the NPC.
The EDBT’s execution starts by ticking its root, which immediately ticks the service node below.
This node repeatedly and concurrently calls the method findTarget, which checks if there is a target near the NPC and (if any is found)
updates blackboard[“target”] with a reference to the target.

Then, the service node ticks the selector and the selector ticks the 'blackboard observer decorator', which is composed of the blackboard key “target”, a condition that checks
if blackboard[“target”] has a value, and the abort rule lower-priority.

Given that blackboard[“target”] has no value, the condition is not met and the completion status failure is returned to the selector.
However, the abort rule lower-priority makes the 'blackboard observer decorator' start observing the key “target”.
The selector then ticks the task node explore.

Suppose that, while the NPC is exploring, findTarget stores a reference to a target in blackboard[“target”], causing the 'blackboard observer decorator' to be notiﬁed by the blackboard.
Now that the node’s condition is met, the abort rule lower-priority has two eﬀects:
 - 1. the 'blackboard observer decorator' stops observing the blackboard key “target”, and
 - 2. the 'blackboard observer decorator'’s ﬁrst composite ancestor — in this case, the selector — will check if it has any descendant nodes placed to the 'blackboard observer decorator'’s right (i.e., nodes with a lower priority)
      that are in the running status; if so, the selector will abort them all and will tick the 'blackboard observer decorator'; otherwise, the EDBT’s execution continues normally.

Therefore, the task node explore is aborted and the 'blackboard observer decorator' is ticked, which immediately ticks its child.
Observe that the task node follow uses the reference stored in blackboard[“target”] as a parameter.
In addition note that, due to (1), nothing will occur if blackboard[“target”] is updated again while the task node follow is in the running status
*/
