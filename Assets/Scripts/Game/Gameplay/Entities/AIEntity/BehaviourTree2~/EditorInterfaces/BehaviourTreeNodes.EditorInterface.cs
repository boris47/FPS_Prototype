#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

namespace Entities.AI.Components.Behaviours
{
	public interface IBTNodeEditorInterface
	{
		System.Action UpdateView { get; set; }
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

		[SerializeField, /*ReadOnly*/HideInInspector]
		private			string				m_Guid									= System.Guid.NewGuid().ToString();

		[SerializeField, /*ReadOnly*/HideInInspector]
		private			Vector2				m_Position								= Vector2.zero;

		[SerializeField, /*ReadOnly*/HideInInspector]
		private			bool				m_HasBreakpoint							= false;

		public			System.Action		m_UpdateView							= null;

		/*IBehaviourTreeNodeEditor*/
		System.Action						IBTNodeEditorInterface.UpdateView		{ get => m_UpdateView;			set => m_UpdateView = value; }
		string								IBTNodeEditorInterface.Guid				=> m_Guid;
		EBTNodeState						IBTNodeEditorInterface.NodeState		=> m_NodeState;
		Vector2								IBTNodeEditorInterface.Position			{ get => m_Position;			set => m_Position = value; }
		uint								IBTNodeEditorInterface.NodeIndex		{ get => m_NodeIndex;			set => m_NodeIndex = value; }
		bool								IBTNodeEditorInterface.HasBreakpoint	{ get => m_HasBreakpoint;		set => m_HasBreakpoint = value; }
		BTNode								IBTNodeEditorInterface.Parent			{ get => m_Parent;				set => m_Parent = value; }
		BehaviourTree						IBTNodeEditorInterface.BehaviourTree	{ get => m_BehaviourTree as BehaviourTree; set => m_BehaviourTree = value; }
		/*IBehaviourTreeNodeEditor*/


		protected class ToNodeInspectorAttribute : System.Attribute
		{
			public readonly bool bShowDefaultLabel = false;
			public readonly string Label = null;

			public ToNodeInspectorAttribute(bool bShowLabel = true)
			{
				this.bShowDefaultLabel = bShowLabel;
			}

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
									EditorGUILayout.PropertyField(iterator, GUIContent.none, true, GUILayout.MaxWidth(150f));
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


	public interface IBTCompositeNodeEditorInterface
	{
		void SortChildren();
	}

	public abstract partial class BTCompositeNode : IBTCompositeNodeEditorInterface
	{
		void IBTCompositeNodeEditorInterface.SortChildren()
		{
			static int SortByHorizontalPosition(IBTNodeEditorInterface left, IBTNodeEditorInterface right) => left.Position.x < right.Position.x ? -1 : 1;

			if (!m_BehaviourTree.IsInstance)
			{
				EditorUtility.SetDirty(this);
				m_Children.ForEach(c => EditorUtility.SetDirty(c));
			}
			{
				m_Children.Sort(SortByHorizontalPosition);
			}
			if (!m_BehaviourTree.IsInstance)
			{
				AssetDatabase.SaveAssetIfDirty(this);
				m_Children.ForEach(c => AssetDatabase.SaveAssetIfDirty(c));
			}
		}
	}


	public interface IBTConditionalEditorInterface
	{
		
	}

	public partial class BTConditional : IBTConditionalEditorInterface
	{
		
	}

	public static class Extensions
	{
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
