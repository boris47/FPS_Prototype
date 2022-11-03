
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public partial class BehaviourTree : ScriptableObject
	{
		private static	List<BehaviourTreeInstanceData>		s_TreeInstances				= new List<BehaviourTreeInstanceData>();
		public static bool bLogEnabled { get; private set; } = true;

		[Header("Asset Data")]
		[SerializeField, ReadOnly]
		private			BTRootNode							m_RootNode					= null;

		[SerializeField, ReadOnly]
		private			List<BTNode>						m_Nodes						= new List<BTNode>();

		[SerializeField, ReadOnly]
		private			Blackboard							m_BlackboardAsset			= null;


		public			BTRootNode							RootNode					=> m_RootNode;
		public			Blackboard							BlackboardAsset				=> m_BlackboardAsset;

		//////////////////////////////////////////////////////////////////////////
		public static BTNode[] GetNodesUsed(in BehaviourTree InBehaviourTreeAsset)
		{
			static void CollectNodes(in BTNode node, in List<BTNode> InNodesCollection)
			{
				InNodesCollection.Add(node);
				if (node is IParentNode asParentNode)
				{
					foreach (BTNode child in asParentNode.Children)
					{
						CollectNodes(child, InNodesCollection);
					}
				}
			}

			List<BTNode> nodes = new List<BTNode>();
			CollectNodes(InBehaviourTreeAsset.m_RootNode, nodes);
			return nodes.ToArray();
		}

		//////////////////////////////////////////////////////////////////////////
		public static BehaviourTreeInstanceData CreateInstanceFrom(in BehaviourTree InBehaviourTreeAsset, in AIController InController)
		{
			static void IterateTree(in BTNode nodeAsset, ref uint InCurrentNodeIndex, in BehaviourTreeInstanceData treeInstanceData, in BlackboardInstanceData InBlackboardInstanceData, in BTNodeInstanceData[] nodesInstancesData, in RuntimeDataBase[] nodesRuntimeData, in BTNodeInstanceData parentNodeInstanceData)
			{
				var nodeInstanceData = BTNode.CreateInstanceData(nodeAsset, InCurrentNodeIndex, treeInstanceData, InBlackboardInstanceData, nodesInstancesData, nodesRuntimeData, parentNodeInstanceData);
				++InCurrentNodeIndex;

				if (nodeAsset is IParentNode asParentNode)
				{
					foreach (BTNode child in asParentNode.Children)
					{
						IterateTree(child, ref InCurrentNodeIndex, treeInstanceData, InBlackboardInstanceData, nodesInstancesData, nodesRuntimeData, nodeInstanceData);
					}
				}
			}

			// Create tree instance
			BehaviourTree newTreeInstance = CreateInstance<BehaviourTree>();

			// Pre-allocate node data
			BTNode[] nodeToUse = GetNodesUsed(InBehaviourTreeAsset);
			BTNodeInstanceData[] nodesInstancesData = new BTNodeInstanceData[nodeToUse.Length];
			RuntimeDataBase[] nodesRuntimeData = new RuntimeDataBase[nodeToUse.Length];

			// Create instance of blackboard if needed
			if (InBehaviourTreeAsset.m_BlackboardAsset == null)
			{
				InBehaviourTreeAsset.m_BlackboardAsset = CreateInstance<Blackboard>();
			}

			//Create tree instance data
			BehaviourTreeInstanceData treeInstanceData = new BehaviourTreeInstanceData(InBehaviourTreeAsset, newTreeInstance, InController, nodesInstancesData, nodesRuntimeData, out BlackboardInstanceData OutBlackboardInstanceData);

			// Create and collect instances data and runtime data
			uint reachedNodeIndex = 0u;
			IterateTree(InBehaviourTreeAsset.RootNode, ref reachedNodeIndex, treeInstanceData, OutBlackboardInstanceData, nodesInstancesData, nodesRuntimeData, null);

			ConditionalLog($"Create instance of {InBehaviourTreeAsset.name} with {reachedNodeIndex}", InController);
			
			// Finally create tree instance data and store in global tree instances list
			return s_TreeInstances.AddRef(treeInstanceData);
		}

		//////////////////////////////////////////////////////////////////////////
		public static bool TryGetInstance(uint InTreeId, out BehaviourTree OutBehaviourTree)
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
		protected static void ConditionalLog(in string InMessage, in Object InContext)
		{
			if (BehaviourTree.bLogEnabled) Debug.Log($"{InContext.name}: {InMessage}", InContext);
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
				if (Utils.CustomAssertions.IsTrue(InTreeInstanceData.NodesInstanceData.At(0) == InTreeInstanceData.RootNode))
				{
					InTreeInstanceData.SetTreeState(EBehaviourTreeState.RUNNING);
				//	m_RootNode.ResetNode(InTreeInstanceData.RootNode);
					InTreeInstanceData.SetRunningNode(InTreeInstanceData.RootNode);
					outValue = true;

					ConditionalLog($"Started tree {name}", InTreeInstanceData.Controller);
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
			ConditionalLog($"Restart of tree {name}", InTreeInstanceData.Controller);
			ResetTree(InTreeInstanceData);
			return StartTree(InTreeInstanceData);
		}

		//////////////////////////////////////////////////////////////////////////
		public EBTNodeState UpdateTree(in BehaviourTreeInstanceData InTreeInstanceData, in float InDeltaTime)
		{
			// Update nodes state
			if (InTreeInstanceData.TreeState == EBehaviourTreeState.RUNNING)
			{
				// Update tickables
				InTreeInstanceData.UpdateTickables(InDeltaTime);

				if (Utils.CustomAssertions.IsNotNull(InTreeInstanceData.CurrentRunningNode))
				{
					// Accept the abort candidate request
					if (InTreeInstanceData.CandidateForAbort.IsNotNull())
					{
						uint requiringNodeIndex = InTreeInstanceData.CandidateForAbort.Candidate.NodeInstance.NodeIndex;
						uint currentRunningNodeIndex = InTreeInstanceData.CurrentRunningNode.NodeInstance.NodeIndex;
						for (uint i = currentRunningNodeIndex; i > requiringNodeIndex; --i)
						{
							BTNodeInstanceData nodeInstanceData = InTreeInstanceData.NodesInstanceData[i];
							nodeInstanceData.NodeAsset.AbortAndResetNode(nodeInstanceData);
						}
						InTreeInstanceData.CandidateForAbort.AcceptRequest();
						InTreeInstanceData.RemoveCandidateForAbort();
					}

					BTNode nodeAsset = InTreeInstanceData.CurrentRunningNode.NodeAsset;
					BTNodeInstanceData instanceData = InTreeInstanceData.CurrentRunningNode;
					nodeAsset.UpdateNode(instanceData, InDeltaTime);
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
				m_RootNode.AbortAndResetNode(InTreeInstanceData.RootNode);

				ConditionalLog($"Stop of tree {name}", InTreeInstanceData.Controller);
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
		public void RequestExecution(in BTNodeInstanceData InRequestedBy, in System.Action<BTNodeInstanceData> InOnRequestAccepted)
		{
			/*
			static BTCompositeNode GetCompositeParent(in BTNodeInstanceData InCurrentNode)
			{
				BTCompositeNode parent = null;
				{
					BTNodeInstanceData currentNode = InCurrentNode;
					while (currentNode.IsNotNull())
					{
						if (currentNode.ParentInstanceData.NodeInstance is BTCompositeNode composite)
						{
							parent = composite;
							break;
						}
						else
						{
							currentNode = currentNode.ParentInstanceData;
						}
					}
				}
				return parent;
			}
			*/

			BehaviourTreeInstanceData treeInstanceData = InRequestedBy.BehaviourTreeInstanceData;
			uint requiringNodeIndex = InRequestedBy.NodeInstance.NodeIndex;
			uint currentRunningNodeIndex = treeInstanceData.CurrentRunningNode.NodeInstance.NodeIndex;

		//	BTCompositeNode requiringNodeCompositeAnchestor = GetCompositeParent(InRequestedBy);
		//	BTCompositeNode currentRunningNodeAnchestor = GetCompositeParent(treeInstanceData.CurrentRunningNode);
		//	if (requiringNodeCompositeAnchestor == currentRunningNodeAnchestor)

			// Low Priority Aborts Only
			if (requiringNodeIndex < currentRunningNodeIndex)
			{
				if (treeInstanceData.CandidateForAbort.IsNotNull())
				{
					uint candidateIndex = treeInstanceData.CandidateForAbort.Candidate.NodeInstance.NodeIndex;
					if (requiringNodeIndex < candidateIndex)
					{
						treeInstanceData.SetCandidateForAbort(InRequestedBy, InOnRequestAccepted);
					}
				}
				else
				{
					treeInstanceData.SetCandidateForAbort(InRequestedBy, InOnRequestAccepted);
				}
			}
		}
	}
}


#if UNITY_EDITOR
namespace Entities.AI.Components.Behaviours
{
	using UnityEditor;
	using UnityEngine;
	// Editor
	[CreateAssetMenu()]
	public partial class BehaviourTree
	{
		[SerializeField, ReadOnly]
		private Vector2 m_EditorGraphPosition = Vector2.zero;

		[SerializeField, ReadOnly]
		private Vector3 m_EditorGraphScale = Vector3.one;


		public static class Editor
		{
			//////////////////////////////////////////////////////////////////////////
			public static Vector2 GetEditorGraphPosition(in BehaviourTree InBehaviourTreeAsset) => InBehaviourTreeAsset.m_EditorGraphPosition;
			public static void SetEditorGraphPosition(in BehaviourTree InBehaviourTreeAsset, in Vector2 InNewPosition) => InBehaviourTreeAsset.m_EditorGraphPosition = InNewPosition;

			//////////////////////////////////////////////////////////////////////////
			public static Vector3 GetEditorGraphScale(in BehaviourTree InBehaviourTreeAsset) => InBehaviourTreeAsset.m_EditorGraphScale;
			public static void SetEditorGraphScale(in BehaviourTree InBehaviourTreeAsset, in Vector3 InNewScale) => InBehaviourTreeAsset.m_EditorGraphScale = InNewScale;

			//////////////////////////////////////////////////////////////////////////
		//	public static List<BTNode> GetNodes(in BehaviourTree InBehaviourTreeAsset) => InBehaviourTreeAsset.m_Nodes;
			public static void SortNodes(in BehaviourTree InBehaviourTreeAsset, in System.Comparison<BTNode> InComparer) => InBehaviourTreeAsset.m_Nodes.Sort(InComparer);
			public static BTNode[] GetAllNodes(in BehaviourTree InBehaviourTreeAsset) => InBehaviourTreeAsset.m_Nodes.ToArray();
			//////////////////////////////////////////////////////////////////////////
			public static BTNode[] GetNodesUsed(in BehaviourTree InBehaviourTreeAsset)
			{
				static void CollectNodes(in BTNode node, in List<BTNode> InNodesCollection)
				{
					InNodesCollection.Add(node);
					if (node is IParentNode asParentNode)
					{
						// Sort child if composite
						if (node is BTCompositeNode asComposite)
						{
							BTCompositeNode.Editor.SortChildren(asComposite);
						}

						foreach (BTNode child in asParentNode.Children)
						{
							CollectNodes(child, InNodesCollection);
						}
					}
				}

				List<BTNode> nodes = new List<BTNode>();
				CollectNodes(InBehaviourTreeAsset.m_RootNode, nodes);
				return nodes.ToArray();
			}
			public static BTRootNode GetRootNode(in BehaviourTree InBehaviourTreeAsset) => InBehaviourTreeAsset.m_RootNode;

			//////////////////////////////////////////////////////////////////////////
			public static void EnsureBlackboard(in BehaviourTree InBehaviourTreeAsset)
			{
				if (InBehaviourTreeAsset.m_BlackboardAsset == null)
				{
					if (InBehaviourTreeAsset.TryGetSubObjectsOfType(out Blackboard[] bbs))
					{
						InBehaviourTreeAsset.m_BlackboardAsset = bbs[0];
					}
					else
					{
						using (new Utils.Editor.MarkAsDirty(InBehaviourTreeAsset))
						{
							InBehaviourTreeAsset.m_BlackboardAsset = CreateInstance<Blackboard>();
							InBehaviourTreeAsset.m_BlackboardAsset.name = nameof(BlackboardAsset);
							AssetDatabase.AddObjectToAsset(InBehaviourTreeAsset.m_BlackboardAsset, InBehaviourTreeAsset);
						}
					}
				}
			}

			//////////////////////////////////////////////////////////////////////////
			public static BTRootNode EnsureRootNode(in BehaviourTree InBehaviourTreeAsset)
			{
				if (!InBehaviourTreeAsset.m_RootNode)
				{
					InBehaviourTreeAsset.m_RootNode = (BTRootNode)CreateNodeAsset(InBehaviourTreeAsset, typeof(BTRootNode), "Root Node");
				}
				return InBehaviourTreeAsset.m_RootNode;
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool RemoveInvalidNodes(in BehaviourTree InBehaviourTreeAsset)
			{
				bool bOutResult = false;
				var nodes = InBehaviourTreeAsset.m_Nodes;
				for (int i = nodes.Count  - 1; i >= 0; i--)
				{
					if (!nodes[i].IsNotNull())
					{
						InBehaviourTreeAsset.m_Nodes.RemoveAt(i);
						bOutResult = true;
					}
				}
				return bOutResult;
			}

			//////////////////////////////////////////////////////////////////////////
			public static BTNode CreateNodeAsset(in BehaviourTree InBehaviourTreeAsset, in System.Type InNodeType, in string InNodeName = null)
			{
				BTNode newNodeInstance = ScriptableObject.CreateInstance(InNodeType) as BTNode;
				newNodeInstance.name = InNodeName ?? InNodeType.Name;

				BTNode.Editor.SetBehaviourTreeAsset(newNodeInstance, InBehaviourTreeAsset);
				InBehaviourTreeAsset.m_Nodes.Add(newNodeInstance);

			//	using (new Utils.Editor.MarkAsDirty(InBehaviourTreeAsset))
				{
					AssetDatabase.AddObjectToAsset(newNodeInstance, InBehaviourTreeAsset);
				}
				return newNodeInstance;
			}

			//////////////////////////////////////////////////////////////////////////
			public static void DeleteNodeAsset(in BehaviourTree InBehaviourTreeAsset, in BTNode InNodeToDelete)
			{
				InBehaviourTreeAsset.m_Nodes.Remove(InNodeToDelete);
				using (new Utils.Editor.MarkAsDirty(InBehaviourTreeAsset))
				{
					AssetDatabase.RemoveObjectFromAsset(InNodeToDelete);
				}
				InNodeToDelete.Destroy();
			}

			//////////////////////////////////////////////////////////////////////////
			public static void AddChildTo(in BTNode InParent, in BTNode InChild, in uint? InPortIndex)
			{
				if (Utils.CustomAssertions.IsTrue(InParent is IParentNode))
				{
					using (new Utils.Editor.MarkAsDirty(InParent))
					{
						switch (InParent)
						{
							case BTRootNode node: BTRootNode.SetChild(node, InChild); break;
							case BTDecoratorNode node: BTDecoratorNode.Editor.SetChild(node, InChild); break;
							case BTCompositeNode node: BTCompositeNode.Editor.AddChild(node, InChild, InPortIndex); break;
						}
						using (new Utils.Editor.MarkAsDirty(InChild))
						{
							BTNode.Editor.SetParentAsset(InChild, InParent);
						}
					}
				}
			}

			//////////////////////////////////////////////////////////////////////////
			public static void RemoveChildFrom(in BTNode InParent, in BTNode InChild)
			{
				if (Utils.CustomAssertions.IsTrue(InParent is IParentNode))
				{
					using (new Utils.Editor.MarkAsDirty(InParent))
					{
						switch (InParent)
						{
							case BTRootNode node: BTRootNode.SetChild(node, null); break;
							case BTDecoratorNode node: BTDecoratorNode.Editor.SetChild(node, null); break;
							case BTCompositeNode node: BTCompositeNode.Editor.RemoveChild(node, InChild); break;
						}

						using (new Utils.Editor.MarkAsDirty(InChild))
						{
							BTNode.Editor.SetParentAsset(InChild, null);
						}
					}
				}
			}
		}
	}
}
#endif

/*
facilitate_non-player_multi-agent_coordination_in_video_games
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
