﻿#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

namespace Entities.AI.Components.Behaviours
{
	public interface IBTNodeEditorInterface
	{
		string Guid { get; }
		EBTNodeState NodeState { get; }
		System.Action UpdateView { get; set; }
		Vector2 Position { get; set; }
		uint NodeIndex { get; set; }
		uint ParentPortIndex { get; set; }
		BTNode Parent { get; set; }
		BehaviourTree BehaviourTree { get; set; }
		bool HasBreakpoint { get; set; }
	}

	///////////////////////////////////////////////////
	//////////////////// All Nodes ////////////////////
	///////////////////////////////////////////////////

	public abstract partial class BTNode : IBTNodeEditorInterface
	{
		public		IBTNodeEditorInterface	AsEditorInterface						=> this;

		[SerializeField, /*ReadOnly*/HideInInspector]
		private			string				m_Guid									= System.Guid.NewGuid().ToString();

		[SerializeField, /*ReadOnly*/HideInInspector]
		private			Vector2				m_Position								= Vector2.zero;

		[SerializeField, /*ReadOnly*/HideInInspector]
		private			bool				m_HasBreakpoint							= false;

		[SerializeField, ReadOnly]
		private			uint				m_ParentPortIndex						= 0u;

		//---------------------
		public			System.Action		m_UpdateView							= null;

		/*IBehaviourTreeNodeEditor*/
		string								IBTNodeEditorInterface.Guid				=> m_Guid;
		EBTNodeState						IBTNodeEditorInterface.NodeState		=> m_NodeState;
		System.Action						IBTNodeEditorInterface.UpdateView		{ get => m_UpdateView;			set => m_UpdateView = value; }
		Vector2								IBTNodeEditorInterface.Position			{ get => m_Position;			set => m_Position = value; }
		uint								IBTNodeEditorInterface.NodeIndex		{ get => m_NodeIndex;			set => m_NodeIndex = value; }
		uint								IBTNodeEditorInterface.ParentPortIndex	{ get => m_ParentPortIndex;		set => m_ParentPortIndex = value; }
		bool								IBTNodeEditorInterface.HasBreakpoint	{ get => m_HasBreakpoint;		set => m_HasBreakpoint = value; }
		BTNode								IBTNodeEditorInterface.Parent			{ get => m_Parent;				set => m_Parent = value; }
		BehaviourTree						IBTNodeEditorInterface.BehaviourTree	{ get => m_BehaviourTree;		set => m_BehaviourTree = value; }
		/*IBehaviourTreeNodeEditor*/


		protected class ToNodeInspectorAttribute : System.Attribute
		{
			public readonly bool bShowDefaultLabel = false;
			public readonly string Label = null;

			//////////////////////////////////////////////////////////////////////////
			public ToNodeInspectorAttribute(bool bShowLabel = true)
			{
				this.bShowDefaultLabel = bShowLabel;
			}

			//////////////////////////////////////////////////////////////////////////
			public ToNodeInspectorAttribute(string Label)
			{
				this.Label = Label;
			}
		}

		[CustomEditor(typeof(BTNode))]
		public class BTNodeEditor : Editor
		{
			public override void OnInspectorGUI()
			{
				EditorGUI.BeginChangeCheck();
				{
					serializedObject.UpdateIfRequiredOrScript();
					SerializedProperty iterator = serializedObject.GetIterator();
					while (iterator.NextVisible(true))
					{
						ToNodeInspectorAttribute attribute = iterator.GetAttribute<ToNodeInspectorAttribute>(true);
						if (attribute.IsNotNull())
						{
							if (attribute.bShowDefaultLabel || !string.IsNullOrEmpty(attribute.Label))
							{
								EditorGUILayout.BeginHorizontal("Button");
								{
									GUILayout.Label(attribute.Label ?? iterator.displayName);

									using (new EditorGUI.DisabledScope(disabled: iterator.GetAttribute<ReadOnlyAttribute>(false).IsNotNull()))
									{
										EditorGUILayout.PropertyField(iterator, GUIContent.none, true, GUILayout.MaxWidth(150f));
									}
								}
								EditorGUILayout.EndHorizontal();

							//	EditorGUILayout.PropertyField(iterator, new GUIContent(attribute.Label ?? iterator.displayName), true);
							}
							else
							{
								EditorGUILayout.PropertyField(iterator, GUIContent.none, true);
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

	///////////////////////////////////////////////////
	//////////////////// Composites ///////////////////
	///////////////////////////////////////////////////

	public interface IBTCompositeNodeEditorInterface
	{
		void SortChildren();
	}

	public abstract partial class BTCompositeNode : IBTCompositeNodeEditorInterface
	{
		//////////////////////////////////////////////////////////////////////////
		void IBTCompositeNodeEditorInterface.SortChildren()
		{
			static int SortByHorizontalPosition(IBTNodeEditorInterface left, IBTNodeEditorInterface right) => left.Position.x < right.Position.x ? -1 : 1;

			if (!m_BehaviourTree.IsInstance)
			{
				EditorUtility.SetDirty(this);
				m_Children.ForEach(c => EditorUtility.SetDirty(c));
			}
			if (ShouldSortChildren())
			{
				m_Children.Sort(SortByHorizontalPosition);
			}
			if (!m_BehaviourTree.IsInstance)
			{
				AssetDatabase.SaveAssetIfDirty(this);
				m_Children.ForEach(c => AssetDatabase.SaveAssetIfDirty(c));
			}
		}

		protected virtual bool ShouldSortChildren() => true;
	}

	public interface IBTCompositeNode_TwoParallelNodeEditorInterface
	{
		BTTaskNode MainNode { get; set; }
		BTCompositeNode BackgroundNode { get; set; }
	}

	public partial class BTComposite_TwoParallelNode : IBTCompositeNode_TwoParallelNodeEditorInterface
	{
		public IBTCompositeNode_TwoParallelNodeEditorInterface AsTwoParallelNodeEditorInterface => this;

		BTTaskNode						IBTCompositeNode_TwoParallelNodeEditorInterface.MainNode		{ get => m_Main;			set => m_Main = value; }
		BTCompositeNode					IBTCompositeNode_TwoParallelNodeEditorInterface.BackgroundNode	{ get => m_Background;		set => m_Background = value; }

		//////////////////////////////////////////////////////////////////////////
		protected override bool ShouldSortChildren() => false;
	}

	///////////////////////////////////////////////////
	////////////////// Conditionals ///////////////////
	///////////////////////////////////////////////////
	///
	public interface IBTConditionalEditorInterface
	{
		
	}

	public partial class BTConditional : IBTConditionalEditorInterface
	{
		
	}


	///////////////////////////////////////////////////
	//////////////////// Extensions ///////////////////
	///////////////////////////////////////////////////
	public static class Extensions
	{
		//////////////////////////////////////////////////////////////////////////
		public static T GetAttribute<T>(this SerializedProperty prop, bool inherit) where T : System.Attribute
		{
			if (prop == null) { return null; }

			System.Type t = prop.serializedObject.targetObject.GetType();
			foreach (string name in prop.propertyPath.Split('.'))
			{
				FieldInfo fieldInfo = t.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				if (fieldInfo.IsNotNull())
				{
					object[] result = fieldInfo.GetCustomAttributes(typeof(T), inherit);
					return result.IsNotNull() && result.IsValidIndex(0) ? result[0] as T : null;
				}
			}
			return null;
		}
	}
}

#endif
