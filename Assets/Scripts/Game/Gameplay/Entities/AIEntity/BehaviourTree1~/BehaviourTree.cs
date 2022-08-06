
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	// Runtime
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

		[SerializeField, HideInInspector]
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
		private			Stack<BTNode>			m_VisitedNodes			= new Stack<BTNode>();

		[SerializeField, ReadOnly]
		private			List<BTCompositeNode>	m_ConditionedComposites = new List<BTCompositeNode>();
		// Every node waiting for lower priority abort is listed here and
		// whenever some of its conditional evaluate true than trigger the conditional
		// abort, checking priority

		//---------------------
		public			bool					IsInstance				=> m_IsInstance;
		public			AIController			Owner					=> m_Controller;



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
		//////////////////////////////////////////////////////////////////////////
		public void OnAwake(in AIController InController)
		{
			m_Controller = InController;
			m_RootNode?.OnAwake(this);
			m_TreeState = EBehaviourTreeState.STOPPED;
		}

		//////////////////////////////////////////////////////////////////////////
		public void StartTree()
		{
			if (m_TreeState == EBehaviourTreeState.STOPPED)
			{
				if (CustomAssertions.IsNotNull(GameManager.CyclesEvents))
				{
					m_VisitedNodes.Clear();
					m_RootNode.ResetNode();
					if (m_RootNode.TryActivation())
					{
						m_TreeState = EBehaviourTreeState.RUNNING;

						GameManager.CyclesEvents.OnFrame += OnFrame;
					}
				}
			}
			else
			{
				Debug.LogError($"Is being trying to start a behaviour tree with state {m_TreeState}", this);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnFrame(float InDeltaTime)
		{
			if (m_VisitedNodes.Count > 0)
			{
				m_VisitedNodes.Peek().OnUpdate();
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

		//////////////////////////////////////////////////////////////////////////
		public Blackboard GetBlackboard() => m_Controller.Blackboard;

		//////////////////////////////////////////////////////////////////////////
		public void ConditionalFlowAbort(in BTCompositeNode InCompositeRequestingLowPriorityAbort)
		{
			Debug.Log($"Requested ConditionalFlowAbort");
			int compositeToActivateIndex = m_ConditionedComposites.IndexOf(InCompositeRequestingLowPriorityAbort);
			bool bValidIndex = CustomAssertions.IsTrue(m_ConditionedComposites.IsValidIndex(compositeToActivateIndex));
			bool bContained = CustomAssertions.IsTrue(m_VisitedNodes.Contains(InCompositeRequestingLowPriorityAbort));
			bool bValidAbortType = CustomAssertions.IsTrue(InCompositeRequestingLowPriorityAbort.AbortType == EAbortType.LowerPriority);

			if (bValidIndex && bContained && bValidAbortType)
			{
				// Remove all the composites with lower priority of the selected one, and the selected one
				// because no longer need to have check in update
				for (int i = m_ConditionedComposites.Count - 1; i >= compositeToActivateIndex; i--)
				{
					BTCompositeNode composite = m_ConditionedComposites[i];
					{
						composite.Conditional.OnRemoveObserver();
					}
					m_ConditionedComposites.RemoveAt(i);
				}

				// Abort current running node (Instant)
				m_VisitedNodes.Pop().AbortNode();

				// Dequeue and reset all stored nodes till the requesting composite is found
				while (m_VisitedNodes.Peek() != InCompositeRequestingLowPriorityAbort)
				{
					m_VisitedNodes.Pop().ResetNode();
				}

				// Notify the composite that is going to be re-activated by this operation so
				// a new evaluation of it's conditional can be avoided
				InCompositeRequestingLowPriorityAbort.ActivateByConditionalAbort();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public BTNode LastRunningNode() => m_VisitedNodes.Peek();
		//////////////////////////////////////////////////////////////////////////
		public bool IsNodeVisited(in BTNode InNode) => m_VisitedNodes.Contains(InNode);
		//////////////////////////////////////////////////////////////////////////
		public void AddConditionalComposite(in BTCompositeNode InComposite) => m_ConditionedComposites.Add(InComposite);
		//////////////////////////////////////////////////////////////////////////
		public bool IsNodeRunning(in BTNode InNode) => m_VisitedNodes.Contains(InNode);
		//////////////////////////////////////////////////////////////////////////
		public void AddVisitedNode(in BTNode InNode)
		{
			CustomAssertions.IsTrue(!m_VisitedNodes.Contains(InNode));
			m_VisitedNodes.Push(InNode);
		}
		//////////////////////////////////////////////////////////////////////////
		public void RemoveVisitedNode(in BTNode InNode)
		{
			// Verify that the last visited node is actually the node passing it's result to the parent
			bool cond = m_VisitedNodes.Pop() == InNode;
			CustomAssertions.IsTrue(cond);

			// This node inclusion in the list is not mandatory because is only expected to be there if has LowerPriority abort type
			if (InNode is BTCompositeNode composite)
			{
				m_ConditionedComposites.Remove(composite);
			}
		}
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
