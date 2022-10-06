#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Entities.AI.Components
{
	public partial class AIMemoryComponent // Editor
	{
		[CustomEditor(typeof(AIMemoryComponent))]
		public class AIMemoryComponentEditor : Editor
		{
			private SerializedProperty m_EntryTable = null;
			private ReorderableList m_ReorderableList = null;


			//////////////////////////////////////////////////////////////////////////
			private void OnEnable()
			{
				m_EntryTable = serializedObject.FindProperty(nameof(m_Memories));

				m_ReorderableList = new ReorderableList(serializedObject, elements: m_EntryTable, draggable: false, displayHeader: false, displayAddButton: false, displayRemoveButton: false)
				{
					elementHeightCallback = _ => EditorGUIUtility.singleLineHeight,
					drawElementCallback = DrawElement
				};
			}

			//////////////////////////////////////////////////////////////////////////
			private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
			{
				Rect[] areas = SplitHorizontally(rect, 50f, 50f);
				DrawKey(areas[0], index);
				DrawValue(areas[1], index);
			}

			//////////////////////////////////////////////////////////////////////////
			private void DrawKey(Rect rect, int index)
			{
				SerializedProperty property = m_EntryTable.GetArrayElementAtIndex(index);
				SerializedProperty identifier = property.FindPropertyRelative("m_Identifier");
				MemoryIdentifier reference = (MemoryIdentifier)identifier.objectReferenceValue;
				string label = string.IsNullOrEmpty(reference.DebugName) ? reference.name : reference.DebugName;
				EditorGUI.LabelField(rect, label);
			}

			//////////////////////////////////////////////////////////////////////////
			private void DrawValue(Rect rect, int index)
			{
				SerializedProperty property = m_EntryTable.GetArrayElementAtIndex(index);
				if (property.IsNotNull())
				{
					SerializedProperty value = property.FindPropertyRelative("m_Value");
					if (value.IsNotNull())
					{
						EditorGUI.PropertyField(rect, value, GUIContent.none);
					}
				}
			}

			//////////////////////////////////////////////////////////////////////////
			public override void OnInspectorGUI()
			{
				serializedObject.Update(); // Update the array property's representation in the inspector

				m_ReorderableList.DoLayoutList(); // Have the ReorderableList do its work

				// We need to call this so that changes on the Inspector are saved by Unity.
				serializedObject.ApplyModifiedProperties();
			}

			//////////////////////////////////////////////////////////////////////////
			//Static Utility
			private static Rect[] SplitHorizontally(Rect source, params float[] cuts)
			{
				Rect[] OutRects = new Rect[cuts.Length];

				float x = 0f;
				for (int i = 0; i < cuts.Length; i++)
				{
					var rect = new Rect(source);
					{
						rect.x += x;
						rect.width *= cuts[i] / 100f;
						x += rect.width;
					}
					OutRects[i] = rect;
				}
				return OutRects;
			}
		}
	}
}
#endif