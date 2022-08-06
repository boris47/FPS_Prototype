#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Entities.AI.Components.Behaviours
{
	public interface IBTNodeEditorInterface
	{
		string Guid { get; }
		EBTNodeState NodeState { get; }
		Vector2 Position { get; set; }
		uint NodeIndex { get; set; }
		BTNode Parent { get; set; }
		BehaviourTree BehaviourTree { get; set; }
		bool HasBreakpoint { get; set; }
	}

	public abstract partial class BTNode : IBTNodeEditorInterface
	{
		public		IBTNodeEditorInterface	AsEditorInterface						=> this;

		[SerializeField, HideInInspector]
		private			string				m_Guid									= System.Guid.NewGuid().ToString();

		[SerializeField, ReadOnly]
		private			Vector2				m_Position								= Vector2.zero;

		[SerializeField, ReadOnly]
		private			bool				m_HasBreakpoint							= false;

		/*IBehaviourTreeNodeEditor*/
		string								IBTNodeEditorInterface.Guid				=> m_Guid;
		EBTNodeState						IBTNodeEditorInterface.NodeState		=> m_NodeState;
		Vector2								IBTNodeEditorInterface.Position			{ get => m_Position;			set => m_Position = value; }
		uint								IBTNodeEditorInterface.NodeIndex		{ get => m_NodeIndex;			set => m_NodeIndex = value; }
		bool								IBTNodeEditorInterface.HasBreakpoint	{ get => m_HasBreakpoint;		set => m_HasBreakpoint = value; }
		BTNode								IBTNodeEditorInterface.Parent			{ get => m_Parent;				set => m_Parent = value; }
		BehaviourTree						IBTNodeEditorInterface.BehaviourTree	{ get => m_BehaviourTree as BehaviourTree; set => m_BehaviourTree = value; }
		/*IBehaviourTreeNodeEditor*/
	}


	public interface IBTCompositeNodeEditorInterface
	{
		BTConditional GetConditional();
		void SetConditioanl(in BTConditional InConditional);
	}

	public abstract partial class BTCompositeNode : IBTCompositeNodeEditorInterface
	{
		BTConditional IBTCompositeNodeEditorInterface.GetConditional() => m_Conditional;
		void IBTCompositeNodeEditorInterface.SetConditioanl(in BTConditional InConditional)
		{
			m_Conditional = InConditional;
		}
	}


	public interface IBTConditionalEditorInterface
	{
		BTCompositeNode NodeConditionalized { get; set; }
	}

	public partial class BTConditional : IBTConditionalEditorInterface
	{
		BTCompositeNode IBTConditionalEditorInterface.NodeConditionalized { get => m_NodeConditionalized; set => m_NodeConditionalized = value; }


		[CustomEditor(typeof(BTConditional))]
		public class BTConditionalEditor : Editor
		{
			public override void OnInspectorGUI()
			{
				EditorGUI.BeginChangeCheck();
				serializedObject.UpdateIfRequiredOrScript();
				SerializedProperty iterator = serializedObject.GetIterator();
				while (iterator.NextVisible(true))
				{
					if (iterator.propertyPath != "m_Script")
					{
						EditorGUILayout.PropertyField(iterator, true);
					}
				}
				serializedObject.ApplyModifiedProperties();
				EditorGUI.EndChangeCheck();
			}
		}
	}
}

#endif
