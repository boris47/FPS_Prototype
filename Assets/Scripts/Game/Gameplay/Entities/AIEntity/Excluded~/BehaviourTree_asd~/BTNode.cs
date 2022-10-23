
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	//////////////////////////////////////////////////////////////////////////
	public class BTNodeInstanceData
	{
		[SerializeField, /*ReadOnly*/ HideInInspector]
		private			BehaviourTreeInstanceData		m_InBehaviourTreeInstanceData	= null;

		[SerializeField, ReadOnly]
		private			BTNode							m_NodeAsset						= null;

		[SerializeField, /*ReadOnly*/ HideInInspector]
		private			BTNode							m_NodeInstance					= null;
			
		[SerializeField, /*ReadOnly*/ HideInInspector]
		private			BTNodeInstanceData				m_ParentInstanceData			= null;


		//---------------------
		[ReadOnly]
		public			EBTNodeState					NodeState						= EBTNodeState.INACTIVE;

		[HideInInspector]
		public			bool							bCanTerminate					= true;

		public			System.UIntPtr					RuntimeData						= System.UIntPtr.Zero;

		//---------------------
		public BehaviourTreeInstanceData				BehaviourTreeInstanceData		=> m_InBehaviourTreeInstanceData;
		public BTNode									NodeAsset						=> m_NodeAsset;
		public BTNode									NodeInstance					=> m_NodeInstance;
		public BTNodeInstanceData						ParentInstanceData				=> m_ParentInstanceData;


		//////////////////////////////////////////////////////////////////////////
		public BTNodeInstanceData(in BehaviourTreeInstanceData InTreeInstanceData, in BTNode InNodeAsset, in BTNode InNodeInstance, in BTNodeInstanceData InParentInstanceData = null)
		{
			m_InBehaviourTreeInstanceData = InTreeInstanceData;
			m_NodeAsset = InNodeAsset;
			m_NodeInstance = InNodeInstance;
			m_ParentInstanceData = InParentInstanceData;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public abstract class RuntimeDataBase { }


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
		protected BTNode								ThisNodeAsset					=> this;


		//////////////////////////////////////////////////////////////////////////
		public static bool IsFinished(in BTNodeInstanceData InInstanceData) => InInstanceData.NodeState >= EBTNodeState.FAILED;
		public static bool IsFinished(in EBTNodeState InState) => InState >= EBTNodeState.FAILED;


		//////////////////////////////////////////////////////////////////////////
		public static void CreateInstanceData(in BTNode InNodeAsset, ref uint InCurrentIndex, in BehaviourTreeInstanceData InTreeInstanceData, in BTNodeInstanceData[] InNodesInstancesData, in RuntimeDataBase[] InNodesRuntimeData, in BTNodeInstanceData InParentInstance = null)
		{
			BTNode nodeInstance = ScriptableObject.CreateInstance(InNodeAsset.GetType()) as BTNode;
			BTNodeInstanceData nodeInstanceData = new BTNodeInstanceData(InTreeInstanceData, InNodeAsset, nodeInstance, InParentInstance);
			InNodesInstancesData[InCurrentIndex] = nodeInstanceData;
			InNodesRuntimeData[InCurrentIndex] = InNodeAsset.CreateRuntimeDataInstance(nodeInstanceData);
			InCurrentIndex++;

			if (InNodeAsset is IParentNode asParentNode)
			{
				foreach (BTNode child in asParentNode.Children)
				{
					BTNode.CreateInstanceData(child, ref InCurrentIndex, InTreeInstanceData, InNodesInstancesData, InNodesRuntimeData, nodeInstanceData);
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public static BTNodeInstanceData GetChildInstanceData(in BTNodeInstanceData InNodeInstanceData, in BTNode InChild)
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
		public void SetNodeState(in BTNodeInstanceData InThisNodeInstanceData, in EBTNodeState InNewState)
		{
			InThisNodeInstanceData.NodeState = InNewState;
		}

		//////////////////////////////////////////////////////////////////////////
		public EBTNodeState UpdateNode(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			if (InThisNodeInstanceData.NodeState == EBTNodeState.INACTIVE)
			{
				InThisNodeInstanceData.BehaviourTreeInstanceData.SetRunningNode(InThisNodeInstanceData);
				InThisNodeInstanceData.NodeState = OnActivation(InThisNodeInstanceData);
				InThisNodeInstanceData.bCanTerminate = InThisNodeInstanceData.NodeState != EBTNodeState.FAILED;
				Utils.CustomAssertions.IsTrue(InThisNodeInstanceData.NodeState != EBTNodeState.INACTIVE);
			}

			if (InThisNodeInstanceData.NodeState == EBTNodeState.RUNNING)
			{
				InThisNodeInstanceData.NodeState = OnUpdate(InThisNodeInstanceData, InDeltaTime);
			}

			if (BTNode.IsFinished(InThisNodeInstanceData))
			{
				if (InThisNodeInstanceData.bCanTerminate)
				{
					OnTerminate(InThisNodeInstanceData);
				}

				InThisNodeInstanceData.BehaviourTreeInstanceData.SetRunningNode(InThisNodeInstanceData.ParentInstanceData);
			}
			return InThisNodeInstanceData.NodeState;
		}

		//////////////////////////////////////////////////////////////////////////
		public void RequestAbortNode(in BTNodeInstanceData InThisNodeInstanceData)
		{
			OnAbortNodeRequested(InThisNodeInstanceData);
		}

		//////////////////////////////////////////////////////////////////////////
		public void ResetNode(in BTNodeInstanceData InThisNodeInstanceData)
		{
			InThisNodeInstanceData.NodeState = EBTNodeState.INACTIVE;

			OnNodeReset(InThisNodeInstanceData);
		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnAwakeInternal(in BTNodeInstanceData InThisNodeInstanceData) { }
		protected virtual EBTNodeState OnActivation(in BTNodeInstanceData InThisNodeInstanceData) => EBTNodeState.RUNNING;
		protected virtual EBTNodeState OnUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime) => EBTNodeState.SUCCEEDED;
		protected virtual void OnTerminate(in BTNodeInstanceData InThisNodeInstanceData) { }
		protected virtual void OnAbortNodeRequested(in BTNodeInstanceData InThisNodeInstanceData) { }
		protected virtual void OnNodeReset(in BTNodeInstanceData InThisNodeInstanceData) { }
	}


	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////


	internal class ToNodeInspectorAttribute : System.Attribute
	{
		public readonly bool bShowDefaultLabel = false;
		public readonly string Label = null;

		//////////////////////////////////////////////////////////////////////////
		public ToNodeInspectorAttribute(bool bShowLabel = true)
		{
			bShowDefaultLabel = bShowLabel;
		}

		//////////////////////////////////////////////////////////////////////////
		public ToNodeInspectorAttribute(string InLabel)
		{
			Label = InLabel;
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

		//---------------------
		public event System.Action m_UpdateView = delegate { };

		public static class Editor
		{
			public static string GetGuid(in BTNode InNode) => InNode.m_Guid;
			public static void RequireViewUpdate(in BTNode InNode) => InNode.m_UpdateView();
			public static Vector2 GetEditorGraphPosition(in BTNode InNode) => InNode.m_EditorGraphPositionPosition;
			public static void SetEditorGraphPosition(in BTNode InNode, in Vector2 InNewPosition) => InNode.m_EditorGraphPositionPosition = InNewPosition;
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
										EditorGUILayout.PropertyField(thisProperty, GUIContent.none, false, GUILayout.MaxWidth(150f));
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