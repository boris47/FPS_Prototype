
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	//////////////////////////////////////////////////////////////////////////
	public abstract class RuntimeDataBase
	{
		protected static BlackboardInstanceData GetBlackboardInstance( in AIController InController)
		{
			return InController.BehaviorTreeComponent.BlackboardInstanceData;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public class BTNodeDummySpecificInstanceData : RuntimeDataBase { }


	//////////////////////////////////////////////////////////////////////////
	public abstract partial class BTNode : ScriptableObject
	{
		[SerializeField, ReadOnly]
		private			BehaviourTree					m_BehaviourTreeAsset			= null;

		[SerializeField, ReadOnly/*HideInInspector*/]
		private			uint							m_NodeIndex						= 0u;

		[SerializeField, ReadOnly]
		private			BTNode							m_ParentAsset					= null;

		//---------------------
		public BehaviourTree							BehaviourTreeAsset				=> m_BehaviourTreeAsset;
		public uint										NodeIndex						=> m_NodeIndex;
		public BTNode									ParentAsset						=> m_ParentAsset;


		//////////////////////////////////////////////////////////////////////////
		public static bool IsFinished(in BTNodeInstanceData InInstanceData) => InInstanceData.NodeState >= EBTNodeState.FAILED;
		public static bool IsFinished(in EBTNodeState InState) => InState >= EBTNodeState.FAILED;


		//////////////////////////////////////////////////////////////////////////
		public static BTNodeInstanceData CreateInstanceData(in BTNode InNodeAsset, in uint InCurrentIndex, in BehaviourTreeInstanceData InTreeInstanceData, in BlackboardInstanceData InBlackboardInstanceData, in BTNodeInstanceData[] InNodesInstancesData, in RuntimeDataBase[] InNodesRuntimeData, in BTNodeInstanceData InParentInstance = null)
		{
			BTNode nodeInstance = ScriptableObject.CreateInstance(InNodeAsset.GetType()) as BTNode;
			nodeInstance.m_NodeIndex = InCurrentIndex;

			BTNodeInstanceData nodeInstanceData = new BTNodeInstanceData(InTreeInstanceData, InBlackboardInstanceData, InNodeAsset, nodeInstance, InParentInstance);
			InNodesInstancesData[InCurrentIndex] = nodeInstanceData;
			InNodesRuntimeData[InCurrentIndex] = InNodeAsset.CreateRuntimeDataInstance(nodeInstanceData);

			return nodeInstanceData;
		}

		//////////////////////////////////////////////////////////////////////////
		protected static BTNodeInstanceData GetChildInstanceData(in BTNodeInstanceData InNodeInstanceData, in BTNode InChild)
		{
			return InNodeInstanceData.BehaviourTreeInstanceData.NodesInstanceData.At(InChild.NodeIndex);
		}

		//////////////////////////////////////////////////////////////////////////
		protected static T GetRuntimeData<T>(in BTNodeInstanceData InNodeInstanceData) where T : RuntimeDataBase
		{
			uint nodeIndex = InNodeInstanceData.NodeAsset.NodeIndex;
			return InNodeInstanceData.BehaviourTreeInstanceData.GetRuntimeData<T>(nodeIndex);
		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual RuntimeDataBase CreateRuntimeDataInstance(in BTNodeInstanceData InThisNodeInstanceData) => new BTNodeDummySpecificInstanceData();

		//////////////////////////////////////////////////////////////////////////
		public void OnAwake(in BTNodeInstanceData InThisNodeInstanceData)
		{
			OnAwakeInternal(InThisNodeInstanceData);
		}

		//////////////////////////////////////////////////////////////////////////
		public EBTNodeState UpdateNode(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			// Activation
			if (InThisNodeInstanceData.NodeState == EBTNodeState.INACTIVE)
			{
				var activationResult = OnActivation(InThisNodeInstanceData);
				if (!BTNode.IsFinished(activationResult)) // Check termination on activation
				{
					InThisNodeInstanceData.BehaviourTreeInstanceData.SetRunningNode(InThisNodeInstanceData);
				}
				InThisNodeInstanceData.SetNodeState(activationResult);
				Utils.CustomAssertions.IsTrue(activationResult != EBTNodeState.INACTIVE);
			}

			// Update
			if (InThisNodeInstanceData.NodeState == EBTNodeState.RUNNING)
			{
				InThisNodeInstanceData.SetNodeState(OnUpdate(InThisNodeInstanceData, InDeltaTime));
			}

			// Finished
			if (BTNode.IsFinished(InThisNodeInstanceData))
			{
				if (InThisNodeInstanceData.NodeState == EBTNodeState.SUCCEEDED)
				{
					OnTerminateSuccess(InThisNodeInstanceData);
				}

				if (InThisNodeInstanceData.NodeState == EBTNodeState.FAILED)
				{
					OnTerminateFailure(InThisNodeInstanceData);
				}
				InThisNodeInstanceData.BehaviourTreeInstanceData.SetRunningNode(InThisNodeInstanceData.ParentInstanceData);
			}
			return InThisNodeInstanceData.NodeState;
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Abort this node </summary>
		public void AbortAndResetNode(in BTNodeInstanceData InThisNodeInstanceData)
		{
			OnNodeAbort(InThisNodeInstanceData);

			ResetNode(InThisNodeInstanceData);
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Reset this node </summary>
		public void ResetNode(in BTNodeInstanceData InThisNodeInstanceData)
		{
			InThisNodeInstanceData.SetNodeState(EBTNodeState.INACTIVE);

			OnNodeReset(InThisNodeInstanceData);
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Called tree awake, before start </summary>
		protected virtual void OnAwakeInternal(in BTNodeInstanceData InThisNodeInstanceData) { }
		/// <summary> Called on node activation, can decide if node can be run or not </summary>
		protected virtual EBTNodeState OnActivation(in BTNodeInstanceData InThisNodeInstanceData) => EBTNodeState.RUNNING;
		/// <summary> Called immediately after succeeded activation and every tree loop till node is finished </summary>
		protected virtual EBTNodeState OnUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime) => EBTNodeState.SUCCEEDED;
		/// <summary> Called when node terminate execution with success </summary>
		protected virtual void OnTerminateSuccess(in BTNodeInstanceData InThisNodeInstanceData) { }
		/// <summary> Called when node terminate execution with failure </summary>
		protected virtual void OnTerminateFailure(in BTNodeInstanceData InThisNodeInstanceData) { }
		/// <summary> Called when node is aborted followed by a reset, no other calls should come to this node </summary>
		protected virtual void OnNodeAbort(in BTNodeInstanceData InThisNodeInstanceData) { }
		/// <summary> The node is resetting its internal state </summary>
		protected virtual void OnNodeReset(in BTNodeInstanceData InThisNodeInstanceData) { }
	}


	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////


	public class ToNodeInspectorAttribute : System.Attribute
	{
		public readonly bool bShowDefaultLabel = false;
		public readonly string Label = null;
		public readonly float ValueWidth = 150f;

		//////////////////////////////////////////////////////////////////////////
		public ToNodeInspectorAttribute(bool bInShowDefaultLabel = true, string InLabel = null, float InValueWidth = 0f)
		{
			bShowDefaultLabel = bInShowDefaultLabel;
			Label = InLabel;
			if (InValueWidth > 0f)
			{
				ValueWidth = Mathf.Max(InValueWidth, 50f);
			}
		}
	}
}

#if UNITY_EDITOR
namespace Entities.AI.Components.Behaviours
{
	using UnityEditor;

	///////////////////////////////////////////////////
	//////////////////// All Nodes ////////////////////
	///////////////////////////////////////////////////
	public abstract partial class BTNode
	{
		[SerializeField, HideInInspector]
		private string m_Guid = System.Guid.NewGuid().ToString();

		[SerializeField, HideInInspector]
		private Vector2 m_EditorGraphPositionPosition = Vector2.zero;

		[SerializeField, HideInInspector]
		private uint m_ParentPortIndex = 0u;

		[SerializeField, HideInInspector]
		private bool m_HasBreakpoint = false;

		public static class Editor
		{
			public static string GetGuid(in BTNode InNode) => InNode.m_Guid;
			public static Vector2 GetEditorGraphPosition(in BTNode InNode) => InNode.m_EditorGraphPositionPosition;
			public static void SetEditorGraphPosition(in BTNode InNode, in Vector2 InNewPosition) => InNode.m_EditorGraphPositionPosition = InNewPosition;
			public static bool HasBreakpoint(in BTNode InNode) => InNode.m_HasBreakpoint;
			public static bool SetBreakpoint(in BTNode InNode, in bool InEnabled) => InNode.m_HasBreakpoint = InEnabled;
			public static uint GetNodeIndex(in BTNode InNode) => InNode.m_NodeIndex;
			public static void SetNodeIndex(in BTNode InNode, in uint InNewNodeIndex) => InNode.m_NodeIndex = InNewNodeIndex;
			public static uint GetNodeParentPortIndex(in BTNode InNode) => InNode.m_ParentPortIndex;
			public static void SetNodeParentPortIndex(in BTNode InNode, in uint InNewParentPortIndex) => InNode.m_ParentPortIndex = InNewParentPortIndex;
			public static BTNode GetParentAsset(in BTNode InNode) => InNode.m_ParentAsset;
			public static void SetParentAsset(in BTNode InNode, in BTNode InParentNodeAsset) => InNode.m_ParentAsset = InParentNodeAsset;
			public static BehaviourTree GetBehaviourTreeAsset(in BTNode InNode) => InNode.m_BehaviourTreeAsset;
			public static void SetBehaviourTreeAsset(in BTNode InNode, in BehaviourTree InBehaviourTreeAsset) => InNode.m_BehaviourTreeAsset = InBehaviourTreeAsset;
		}


		//////////////////////////////////////////////////////////////////////////
		public class BTNodeInBTViewEditor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				EditorGUI.BeginChangeCheck();
				{
					serializedObject.UpdateIfRequiredOrScript();
					SerializedProperty thisProperty = serializedObject.GetIterator();
					while (thisProperty.NextVisible(true))
					{
						if (thisProperty.TryGetAttribute(out ToNodeInspectorAttribute attribute, true))
						{
							if (attribute.bShowDefaultLabel || !string.IsNullOrEmpty(attribute.Label))
							{
								EditorGUILayout.BeginHorizontal(/*"Button"*/);
								{
									GUILayout.Label(attribute.Label ?? thisProperty.displayName);

									bool bIsReadOnly = thisProperty.TryGetAttribute(out ReadOnlyAttribute _, false);
									using (new EditorGUI.DisabledScope(bIsReadOnly))
									{
										EditorGUILayout.PropertyField(thisProperty, GUIContent.none, false, GUILayout.MaxWidth(attribute.ValueWidth));
									}
								}
								EditorGUILayout.EndHorizontal();
							}
							else
							{
								EditorGUILayout.PropertyField(thisProperty, GUIContent.none, true);
							}
						}
					}
				}
				if (EditorGUI.EndChangeCheck())
				{
					serializedObject.ApplyModifiedProperties();
				}
			}
		}
	}
}
#endif