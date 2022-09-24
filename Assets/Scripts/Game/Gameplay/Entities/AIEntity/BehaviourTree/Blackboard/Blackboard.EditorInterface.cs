
#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Entities.AI.Components
{
	public sealed partial class Blackboard // Editor
	{
		[CustomEditor(typeof(Blackboard))]
		public class BlackboardEditor : Editor
		{
			private Blackboard m_Blackboard = null;
			private SerializedProperty m_EntryTable = null;
			private ReorderableList m_ReorderableList = null;


			//////////////////////////////////////////////////////////////////////////
			private void OnEnable()
			{
				m_Blackboard = (Blackboard)serializedObject.targetObject;
				m_EntryTable = serializedObject.FindProperty(nameof(m_Entries));

				m_ReorderableList = new ReorderableList(serializedObject, elements: m_EntryTable, draggable: false, displayHeader: false, displayAddButton: true, displayRemoveButton: true)
				{
					elementHeightCallback = _ => EditorGUIUtility.singleLineHeight,
					drawElementCallback = DrawElement,
					onAddDropdownCallback = (Rect buttonRect, ReorderableList _) => PopupWindow.Show(buttonRect, new ChooseEntryTypePopup(m_Blackboard))
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
				SerializedProperty identifier = property.FindPropertyRelative("m_BlackboardEntryKey");
				BlackboardEntryKey reference = (BlackboardEntryKey)identifier.objectReferenceValue;
				string label = string.IsNullOrEmpty(reference.DebugName) ? reference.name : reference.DebugName;
				EditorGUI.LabelField(rect, label);
			}

			//////////////////////////////////////////////////////////////////////////
			private void DrawValue(Rect rect, int index)
			{
				SerializedProperty property = m_EntryTable.GetArrayElementAtIndex(index);
				SerializedProperty bbEEntryValue = property.FindPropertyRelative("m_Value");
				EditorGUI.PropertyField(rect, bbEEntryValue, GUIContent.none);
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

		//////////////////////////////////////////////////////////////////////////
		public class ChooseEntryTypePopup : PopupWindowContent
		{
			private IEnumerable<System.Type> m_AllowedTypes = TypeCache.GetTypesDerivedFrom<BlackboardEntryBase>().Where(t => !t.IsAbstract);
			private IEnumerable<string> m_ValidKeys = AssetDatabase.FindAssets($"t:{typeof(BlackboardEntryKey).Name}").Select(AssetDatabase.GUIDToAssetPath);
			private Blackboard m_Blackboard = null;

			private string m_KeyAssetPath = null;

			//////////////////////////////////////////////////////////////////////////
			public ChooseEntryTypePopup(in Blackboard InBlackboard)
			{
				m_Blackboard = InBlackboard;
			}

			//////////////////////////////////////////////////////////////////////////
			public override void OnGUI(Rect rect)
			{
				if (string.IsNullOrEmpty(m_KeyAssetPath))
				{
					GUILayout.Label("Choose an entry blackboard key");
					foreach (string assetPath in m_ValidKeys)
					{
						if (!m_Blackboard.m_Entries.Select(e => AssetDatabase.GetAssetPath(e.BlackboardEntryKey)).Any(s => s.Equals(assetPath)))
						{
							string label = System.IO.Path.GetFileNameWithoutExtension(assetPath);
							if (GUILayout.Button(label))
							{
								m_KeyAssetPath = assetPath;
								editorWindow.Repaint();
							}
						}
					}
				}
				else
				{
					GUILayout.Label("Choose an entry type");
					foreach (System.Type entryType in m_AllowedTypes)
					{
						if (!m_Blackboard.m_Entries.Select(e => e.GetType()).Contains(entryType))
						{
							if (GUILayout.Button(entryType.Name))
							{
								BlackboardEntryKey key = AssetDatabase.LoadAssetAtPath<BlackboardEntryKey>(m_KeyAssetPath);
								BlackboardEntryBase newEntry = m_Blackboard.GetOrCreateEntry(entryType, key);
								editorWindow.Close();
							}
						}
					}
				}
			}
		}
	}
}
#endif
