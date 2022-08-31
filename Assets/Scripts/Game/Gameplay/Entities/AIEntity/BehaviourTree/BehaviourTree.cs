
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public partial class BehaviourTree : ScriptableObject
	{
		[SerializeField, ReadOnly]
		private			BTRootNode				m_RootNode				= null;

		[SerializeField, /*HideInInspector*/]
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
		public			BTRootNode				RootNode				=> m_RootNode;
		public			AIController			Owner					=> m_Controller;
		public			EBehaviourTreeState		TreeState				=> m_TreeState;
		public			bool					IsInstance				=> m_IsInstance;


		//---------------------
		private			AbortOperation			m_AbortOperation		= null;
		private			List<IBTNodeTickable>	m_Tickables				= new List<IBTNodeTickable>();


		//////////////////////////////////////////////////////////////////////////
		public static BehaviourTree CreateBehaviourTreeInstance(in BehaviourTree InBehaviourTreeAsset)
		{
			static void Traverse(in BTNode node, System.Action<BTNode> actionOnNode)
			{
				actionOnNode(node);
				if (node is IParentNode parent)
				{
					parent.Children.ForEach(delegate (BTNode n) { Traverse(n, actionOnNode); });
				}
			}

			BehaviourTree newTreeInstance = Instantiate(InBehaviourTreeAsset);
#if UNITY_EDITOR
			newTreeInstance.m_Source = InBehaviourTreeAsset;
#endif
			newTreeInstance.m_IsInstance = true;

			// Root node
			newTreeInstance.m_RootNode = InBehaviourTreeAsset.m_RootNode?.CloneInstance() as BTRootNode;

			// Other Nodes
			newTreeInstance.m_Nodes.Clear();

			if (newTreeInstance.m_RootNode)
			{
				Traverse(newTreeInstance.m_RootNode, delegate(BTNode n)
				{
					newTreeInstance.m_Nodes.Add(n);
				});
			}

			// Return the instance
			return newTreeInstance;
		}

		private static Dictionary<BehaviourTree, BehaviourTree> m_Instances = new Dictionary<BehaviourTree, BehaviourTree>();

		//////////////////////////////////////////////////////////////////////////
		public static BehaviourTree GetInstanceOf(in BehaviourTree InBehaviourTreeSource)
		{
			if (InBehaviourTreeSource.IsInstance)
			{
				return InBehaviourTreeSource;
			}

			BehaviourTree OutTree = null;
			if (!m_Instances.TryGetValue(InBehaviourTreeSource, out OutTree))
			{
				OutTree = CreateBehaviourTreeInstance(InBehaviourTreeSource);
				m_Instances.Add(InBehaviourTreeSource, OutTree);
			}
			return OutTree;
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
		public bool StartTree()
		{
			if (!m_IsInstance)
			{
				Debug.LogError($"Cannot start a not tree instance", this);
				return false;
			}

			if (m_TreeState == EBehaviourTreeState.STOPPED)
			{
				if (Utils.CustomAssertions.IsTrue(m_Nodes.At(0) == m_RootNode))
				{
					m_TreeState = EBehaviourTreeState.RUNNING;
					m_RootNode.ResetNode();
					SetRunningNode(m_Nodes.At(0));
				}
			}
			else
			{
				Debug.LogError($"Is being trying to start a behaviour tree with state {m_TreeState}", this);
				return false;
			}
			return true;
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Only callable if called from current running node </summary>
		public void LockRunningNode(in BTNode InRequester)
		{
			if (Utils.CustomAssertions.IsTrue(InRequester == m_CurrentRunningNode))
			{
				m_RunningNodeLocked = true;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Only callable if called from current running node </summary>
		public void UnLockRunningNode(in BTNode InRequester)
		{
			if (Utils.CustomAssertions.IsTrue(InRequester == m_CurrentRunningNode))
			{
				m_RunningNodeLocked = false;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetRunningNode(in BTNode InNode)
		{
			if (!m_RunningNodeLocked)
			{
				if (InNode.IsNotNull() && InNode.Parent is BTNode parent && parent.NodeState != EBTNodeState.RUNNING)
				{
					Utils.CustomAssertions.IsTrue(false, this, $"Trying to set a {InNode.name} node as active when parent {parent.name} is not running");
					return;
				}

				m_CurrentRunningNode = InNode;
			}
			else
			{
			//	Debug.Log($"Bypassed {nameof(SetRunningNode)} execution");
			}
		}


		//////////////////////////////////////////////////////////////////////////
		public void AddTickableNode(in BTNode InTickable)
		{
			m_Tickables.Add(InTickable);
		}

		//////////////////////////////////////////////////////////////////////////
		public void RemoveTickableNode(in BTNode InTickable)
		{
			m_Tickables.Remove(InTickable);
		}

		//////////////////////////////////////////////////////////////////////////
		public void UpdateFrame(in float InDeltaTime)
		{
			foreach(IBTNodeTickable node in m_Tickables)
			{
				node.UpdateFrame(InDeltaTime);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void UpdateFixed()
		{
			foreach (IBTNodeTickable node in m_Tickables)
			{
				node.UpdateFixed();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public EBTNodeState Update()
		{
			if (m_TreeState == EBehaviourTreeState.RUNNING)
			{
				if (m_AbortOperation.IsNotNull())
				{
					if (m_AbortOperation.Update())
					{
						m_AbortOperation = null;

						// Once abort op is finished unlock the current running node
						UnLockRunningNode(m_CurrentRunningNode);
					}
				}
				else
				{
					m_CurrentRunningNode.Update();
				}
			}

			switch (m_RootNode.NodeState)
			{
				case EBTNodeState.INACTIVE:
					Utils.CustomAssertions.IsTrue(false);
					break;
				case EBTNodeState.SUCCEEDED:
				case EBTNodeState.FAILED:
				case EBTNodeState.ABORTED:
					m_TreeState = EBehaviourTreeState.STOPPED;
					break;
				case EBTNodeState.ABORTING:
				case EBTNodeState.RUNNING:
					m_TreeState = EBehaviourTreeState.RUNNING;
					break;
				default:
					break;
			}
			return m_RootNode.NodeState;
		}

		//////////////////////////////////////////////////////////////////////////
		public void RequestAbort(in BTNode InNodeAborter, in BTNode InNodeToAbort, in System.Action InPostAbortOp)
		{
			if (m_AbortOperation == null)
			{
				if (Utils.CustomAssertions.IsTrue(InNodeAborter != m_CurrentRunningNode))
				{
					// Prevent other nodes to set as running node a different of the current running one
					LockRunningNode(m_CurrentRunningNode);
					m_AbortOperation = new AbortOperation(this, InNodeAborter, InNodeToAbort, InPostAbortOp);
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void AbortLowPriorityNodesAndRunConditional(BTConditional InConditionalRequestingLowPriorityAbort, in System.Action InPostAbort)
		{
			bool bValidAbortType = Utils.CustomAssertions.IsTrue(InConditionalRequestingLowPriorityAbort.AbortType == EAbortType.LowerPriority || InConditionalRequestingLowPriorityAbort.AbortType == EAbortType.Both);
			uint conditionalToActivateIndex = InConditionalRequestingLowPriorityAbort.NodeIndex;
			uint currentRunningNodeIndex = m_CurrentRunningNode.NodeIndex;
			bool bValidIndex = Utils.CustomAssertions.IsTrue(conditionalToActivateIndex < currentRunningNodeIndex);

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
			private readonly BehaviourTree m_BehaviourTree = null;

			//////////////////////////////////////////////////////////////////////////
			public AbortOperation(in BehaviourTree InBehaviourTree, in BTNode InNodeAborter, in BTNode InNodeToAbort, in System.Action InPostAbortOp)
			{
				Utils.CustomAssertions.IsNotNull(InBehaviourTree);
				Utils.CustomAssertions.IsNotNull(InNodeAborter);
				Utils.CustomAssertions.IsNotNull(InNodeToAbort);
				Utils.CustomAssertions.IsNotNull(InPostAbortOp);

				m_BehaviourTree = InBehaviourTree;
				m_NodeAborter = InNodeAborter;
				m_NodeToAbort = InNodeToAbort;
				m_PostAbortOp = InPostAbortOp;

				m_NodeToAbort.RequestAbortNode(bAbortImmediately: false);
				Utils.CustomAssertions.IsTrue(m_NodeToAbort.NodeState == EBTNodeState.ABORTING || m_NodeToAbort.NodeState == EBTNodeState.ABORTED);
			}

			//////////////////////////////////////////////////////////////////////////
			public bool Update()
			{
				bool OutResult = m_NodeToAbort.NodeState == EBTNodeState.ABORTED || m_NodeToAbort.Update() == EBTNodeState.ABORTED;
				if (OutResult)
				{
					// Current running node is fully aborted, so we can unlock the running node assignment
					// m_PostAbortOp?.Invoke will set a new running node so we need to unlock
					m_BehaviourTree.UnLockRunningNode(m_BehaviourTree.m_CurrentRunningNode);
					m_PostAbortOp?.Invoke();
				}
				return OutResult;
			}
		}
	


	
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
		//////////////////////////////////////////////////////////////////////////
	 	public void StopTree()
	 	{
	 		if (m_TreeState == EBehaviourTreeState.RUNNING || m_TreeState == EBehaviourTreeState.PAUSED)
	 		{
	 			m_TreeState = EBehaviourTreeState.STOPPED;

	 			m_RootNode.RequestAbortNode(bAbortImmediately: true);

				m_RootNode.ResetNode();
	 		}
	 	}
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
	}
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
