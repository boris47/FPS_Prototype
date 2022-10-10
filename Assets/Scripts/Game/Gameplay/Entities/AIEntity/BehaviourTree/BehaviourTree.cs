
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public partial class BehaviourTree : ScriptableObject
	{
		private static	List<BehaviourTreeInstanceData>		s_TreeInstances					= new List<BehaviourTreeInstanceData>();

		[Header("Asset Data")]
		[SerializeField, ReadOnly]
		private			BTRootNode							m_RootNode					= null;

		[SerializeField, ReadOnly]
		private			List<BTNode>						m_Nodes						= new List<BTNode>();

		[SerializeField, ReadOnly]
		private			Blackboard							m_Blackboard				= null;


		public			BTRootNode							RootNode					=> m_RootNode;

		//////////////////////////////////////////////////////////////////////////
		public static BehaviourTreeInstanceData CreateInstanceFrom(in BehaviourTree InBehaviourTreeAsset, in AIController InController)
		{
			// Create tree instance
			BehaviourTree newTreeInstance = CreateInstance<BehaviourTree>();

			//Create tree instance data
			BehaviourTreeInstanceData treeInstanceData = new BehaviourTreeInstanceData(InBehaviourTreeAsset, newTreeInstance, InController);

			// Pre-allocate node data
			BTNodeInstanceData[] nodesInstancesData = new BTNodeInstanceData[InBehaviourTreeAsset.m_Nodes.Count];
			RuntimeDataBase[] nodesRuntimeData = new RuntimeDataBase[InBehaviourTreeAsset.m_Nodes.Count];

			// Recursively create and collect instances data and runtime data
			BTNode.CreateInstanceData(InBehaviourTreeAsset.RootNode, treeInstanceData, nodesInstancesData, nodesRuntimeData);

			// Create instance off blackboard
			if (InBehaviourTreeAsset.m_Blackboard == null)
			{
				InBehaviourTreeAsset.m_Blackboard = CreateInstance<Blackboard>();
			}
			BlackboardInstanceData blackboardInstance = Blackboard.CreateInstanceData(InBehaviourTreeAsset.m_Blackboard, treeInstanceData);

			// Finally create tree instance data and store in global tree instances list
			treeInstanceData.SetBlackboardInstance(blackboardInstance);
			treeInstanceData.SetNodesInstancesData(nodesInstancesData);
			treeInstanceData.SetNodesRuntimeData(nodesRuntimeData);
			return s_TreeInstances.AddRef(treeInstanceData);
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetInstance(uint InTreeId, out BehaviourTree OutBehaviourTree)
		{
			bool OutValue = false;
			OutBehaviourTree = null;
			if (s_TreeInstances.TryFind(out BehaviourTreeInstanceData outInstanceData, out int _, instance => instance.UniqueId == InTreeId))
			{
				OutBehaviourTree = outInstanceData.TreeInstance;
			}
			return OutValue;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool DestroyInstance(uint InTreeId)
		{
			bool OutValue = false;
			if (s_TreeInstances.TryFind(out BehaviourTreeInstanceData _, out int index, instance => instance.UniqueId == InTreeId))
			{
				s_TreeInstances.RemoveAt(index);
				OutValue = true;
			}
			return OutValue;
		}

		//////////////////////////////////////////////////////////////////////////
		public void OnAwake(in BehaviourTreeInstanceData InTreeInstanceData)
		{
			// Initialize nodes
			System.Array.ForEach(InTreeInstanceData.NodesInstanceData, nodeInstanceData => nodeInstanceData.NodeAsset.OnAwake(nodeInstanceData));

			//Set starting state of the tree
			InTreeInstanceData.SetTreeState(EBehaviourTreeState.STOPPED);
		}

		//////////////////////////////////////////////////////////////////////////
		public bool StartTree(in BehaviourTreeInstanceData InTreeInstanceData)
		{
			bool outValue = false;
			if (InTreeInstanceData.TreeState == EBehaviourTreeState.STOPPED)
			{
				if (m_RootNode.IsNotNull() && Utils.CustomAssertions.IsTrue(InTreeInstanceData.NodesInstanceData.At(0) == InTreeInstanceData.RootNode))
				{
					InTreeInstanceData.SetTreeState(EBehaviourTreeState.RUNNING);
					m_RootNode.ResetNode(InTreeInstanceData.RootNode);
					InTreeInstanceData.SetRunningNode(InTreeInstanceData.RootNode);
					outValue = true;
				}
			}
			else
			{
				Debug.LogError($"Is being trying to start a behaviour tree with state {InTreeInstanceData.TreeState}", InTreeInstanceData.Controller);
			}
			return outValue;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool RestartTree(in BehaviourTreeInstanceData InTreeInstanceData)
		{
			ResetTree(InTreeInstanceData);
			return StartTree(InTreeInstanceData);
		}

		//////////////////////////////////////////////////////////////////////////
		public EBTNodeState UpdateTree(in BehaviourTreeInstanceData InTreeInstanceData, in float InDeltaTime)
		{
			// Update nodes state
			if (InTreeInstanceData.TreeState == EBehaviourTreeState.RUNNING)
			{
				if (Utils.CustomAssertions.IsNotNull(InTreeInstanceData.CurrentRunningNode))
				{
					InTreeInstanceData.CurrentRunningNode.NodeAsset.UpdateNode(InTreeInstanceData.CurrentRunningNode, InDeltaTime);
				}
			}

			switch (InTreeInstanceData.RootNode.NodeState)
			{
				case EBTNodeState.INACTIVE:
				Utils.CustomAssertions.IsTrue(false);
				break;
				case EBTNodeState.SUCCEEDED:
				case EBTNodeState.FAILED:
				//case EBTNodeState.ABORTED:
				InTreeInstanceData.SetTreeState(EBehaviourTreeState.STOPPED);
				break;
				//case EBTNodeState.ABORTING:
				case EBTNodeState.RUNNING:
				InTreeInstanceData.SetTreeState(EBehaviourTreeState.RUNNING);
				break;
				default:
				break;
			}
			return InTreeInstanceData.RootNode.NodeState;
		}

		//////////////////////////////////////////////////////////////////////////
		public void StopTree(in BehaviourTreeInstanceData InTreeInstanceData)
		{
			if (InTreeInstanceData.TreeState == EBehaviourTreeState.RUNNING || InTreeInstanceData.TreeState == EBehaviourTreeState.PAUSED)
			{
				InTreeInstanceData.SetTreeState(EBehaviourTreeState.STOPPED);
				m_RootNode.ResetNode(InTreeInstanceData.RootNode);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void ResetTree(in BehaviourTreeInstanceData InTreeInstanceData)
		{
			if (InTreeInstanceData.TreeState == EBehaviourTreeState.RUNNING || InTreeInstanceData.TreeState == EBehaviourTreeState.PAUSED)
			{
				StopTree(InTreeInstanceData);
			}

			m_RootNode.ResetNode(InTreeInstanceData.RootNode);

			InTreeInstanceData.SetTreeState(EBehaviourTreeState.PAUSED);
			InTreeInstanceData.SetRunningNode(null);
			InTreeInstanceData.SetRunningNodeLocked(false);
		}

		//////////////////////////////////////////////////////////////////////////
		public void LockRunningNode(in BehaviourTreeInstanceData InTreeInstanceData, in BTNode InRequester)
		{
			if (Utils.CustomAssertions.IsTrue(InRequester == InTreeInstanceData.CurrentRunningNode.NodeAsset))
			{
				InTreeInstanceData.SetRunningNodeLocked(true);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Only callable if called from current running node </summary>
		public void UnLockRunningNode(in BehaviourTreeInstanceData InTreeInstanceData, in BTNodeInstanceData InRequester)
		{
			if (Utils.CustomAssertions.IsTrue(InRequester == InTreeInstanceData.CurrentRunningNode))
			{
				InTreeInstanceData.SetRunningNodeLocked(false);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetRunningNode(in BTNodeInstanceData InNode)
		{
			if (!InNode.BehaviourTreeInstanceData.IsRunningNodeLocked)
			{
				if (InNode.IsNotNull() && InNode.ParentInstanceData.IsNotNull() && InNode.ParentInstanceData.NodeState != EBTNodeState.RUNNING)
				{
					Utils.CustomAssertions.IsTrue(false, this, $"Trying to set a {InNode.NodeAsset.name} node as active when parent {InNode.ParentInstanceData.NodeAsset.name} is not running");
					return;
				}
				InNode.BehaviourTreeInstanceData.SetRunningNode(InNode);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void RequestExecution(in BTNodeInstanceData InRequestedBy, in System.Action InOnPostAbortAction)
		{

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

https://cs.uns.edu.ar/~ragis/Agis%20et%20al.%20(2020)%20-%20An%20event-driven%20behavior%20trees%20extension%20to%20facilitate%20non-player%20multi-agent%20coordination%20in%20video%20games.pdf
Consider an EDBT which models a simple behavior that makes the NPC explore the terrain until it finds a nearby target to follow.
Suppose that the blackboard is initially empty and that there are no targets near the NPC.
The EDBT’s execution starts by ticking its root, which immediately ticks the service node below.
This node repeatedly and concurrently calls the method ғɪɴᴅTᴀʀɢᴇᴛ, which checks if there is a target near the NPC and (if any is found) updates blackboard[“target”] with a reference to the target.
Then, the service node ticks the selector and the selector ticks the BOD, which is composed of the blackboard key “target”, a condition that checks if blackboard[“target”] has a value, and the abort rule lower-priority.
Given that blackboard[“target”] has no value, the condition is not met and the completion status failure is returned to the selector.
However, the abort rule lower-priority makes the BOD start observing the key “target”.
The selector then ticks the task node ᴇxᴘʟᴏʀᴇ.
Suppose that, while the NPC is exploring, ғɪɴᴅTᴀʀɢᴇᴛ stores a reference to a target in blackboard[“target”], causing the BOD to be notified by the blackboard.
Now that the node’s condition is met, the abort rule lower-priority has two effects:
1. the BOD stops observing the blackboard key “target”, and
2. the BOD’s first composite ancestor—in this case, the selector— will check if it has any descendant nodes placed to the BOD’s right (i.e., nodes with a lower priority) that are in the running status;
if so, the selector will abort them all and will tick the BOD;
otherwise, the EDBT’s execution continues normally.
Therefore, the task node ᴇxᴘʟᴏʀᴇ is aborted and the BOD is ticked, which immediately ticks its child.
Observe that the task node ғᴏʟʟᴏᴡ uses the reference stored in blackboard[“target”] as a parameter.
In addition note that, due to (1), nothing will occur if blackboard[“target”] is updated again while the task node ғᴏʟʟᴏᴡ is in the running status.
*/
