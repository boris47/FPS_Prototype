
using UnityEngine;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Entities.AI.Components.Behaviours
{
	internal class ToNodeInspectorAttribute : System.Attribute
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

#if UNITY_EDITOR
	public interface IBTNodeEditorInterface
	{
		string Guid { get; }
		System.Action UpdateView { get; set; }
		Vector2 Position { get; set; }
		uint NodeIndex { get; set; }
		uint ParentPortIndex { get; set; }
		BTNode Parent { get; set; }
		BehaviourTree BehaviourTree { get; set; }
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
		private			uint				m_ParentPortIndex						= 0u;

		//---------------------
		public			System.Action		m_UpdateView							= null;

		/*IBehaviourTreeNodeEditor*/
		string								IBTNodeEditorInterface.Guid				=> m_Guid;
		System.Action						IBTNodeEditorInterface.UpdateView		{ get => m_UpdateView;			set => m_UpdateView = value; }
		Vector2								IBTNodeEditorInterface.Position			{ get => m_Position;			set => m_Position = value; }
		uint								IBTNodeEditorInterface.NodeIndex		{ get => m_NodeIndex;			set => m_NodeIndex = value; }
		uint								IBTNodeEditorInterface.ParentPortIndex	{ get => m_ParentPortIndex;		set => m_ParentPortIndex = value; }
	//	bool								IBTNodeEditorInterface.HasBreakpoint	{ get => m_HasBreakpoint;		set => m_HasBreakpoint = value; }
		BTNode								IBTNodeEditorInterface.Parent			{ get => m_ParentAsset;				set => m_ParentAsset = value; }
		BehaviourTree						IBTNodeEditorInterface.BehaviourTree	{ get => m_BehaviourTreeAsset;		set => m_BehaviourTreeAsset = value; }
		/*IBehaviourTreeNodeEditor*/

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
								EditorGUILayout.BeginHorizontal(/*"Button"*/);
								{
									GUILayout.Label(attribute.Label ?? iterator.displayName);

									using (new EditorGUI.DisabledScope(disabled: iterator.GetAttribute<ReadOnlyAttribute>(false).IsNotNull()))
									{
										EditorGUILayout.PropertyField(iterator, GUIContent.none, true, GUILayout.MaxWidth(150f));
									}
								}
								EditorGUILayout.EndHorizontal();
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

			if (m_Children.Count > 1)
			{
				if (ShouldSortChildren())
				{
					using (new Utils.Editor.MarkAsDirty(this))
					{
						m_Children.ForEach(c => EditorUtility.SetDirty(c));
						m_Children.Sort(SortByHorizontalPosition);
						m_Children.ForEach(c => AssetDatabase.SaveAssetIfDirty(c));
					}
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual bool ShouldSortChildren() => true;
	}

	public interface IBTCompositeNode_TwoParallelNodeEditorInterface
	{

	}

	public partial class BTComposite_TwoParallelNode : IBTCompositeNode_TwoParallelNodeEditorInterface
	{
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

#endif

}
