
namespace Entities.AI.Components
{
	using System.Linq;
	using UnityEngine;
	using UnityEditor;
	using UnityEditorInternal;

	[CustomPropertyDrawer(typeof(Blackboard))]
	public class BlackboardPropertyDrawer : PropertyDrawer
	{
		private Blackboard m_Blackboard = null;
		private SerializedProperty m_EntryTable = null;
		private ReorderableList m_ReorderableList = null;

		//////////////////////////////////////////////////////////////////////////
		private ReorderableList GetList(SerializedProperty property)
		{
			if (m_ReorderableList == null)
			{
				SerializedProperty entriesProperty = property.FindPropertyRelative("m_Entries");
				m_Blackboard = entriesProperty.serializedObject.targetObject as Blackboard;
				m_ReorderableList = new ReorderableList(entriesProperty.serializedObject, elements: m_EntryTable, draggable: false, displayHeader: false, displayAddButton: false, displayRemoveButton: false)
				{
					elementHeightCallback = _ => EditorGUIUtility.singleLineHeight,
					drawElementCallback = DrawElement,
					//	onAddDropdownCallback = (Rect buttonRect, ReorderableList _) => PopupWindow.Show(buttonRect, new ChooseEntryTypePopup(m_Blackboard))
				};
			}
			return m_ReorderableList;
		}

		//////////////////////////////////////////////////////////////////////////
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => GetList(property).GetHeight();

		//////////////////////////////////////////////////////////////////////////
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			GetList(property).DoList(position);
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
			BlackboardEntryKey reference = identifier.stringValue;
			EditorGUI.LabelField(rect, reference ?? string.Empty);
		}

		//////////////////////////////////////////////////////////////////////////
		private void DrawValue(Rect rect, int index)
		{
			SerializedProperty property = m_EntryTable.GetArrayElementAtIndex(index);
			SerializedProperty bbEEntryValue = property.FindPropertyRelative("m_Value");
			EditorGUI.PropertyField(rect, bbEEntryValue, GUIContent.none);
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
	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////
	

	[CustomEditor(typeof(Blackboard))]
	public class BlackboardEditor : Editor
	{
		private static string[] kSupportedValuesTypeNames = null;
		private static System.Type[] kEntryValueTypes = null;

		private Blackboard m_Blackboard = null;
		private SerializedProperty m_KeyList = null;
		private ReorderableList m_ReorderableKeysList = null;


		//////////////////////////////////////////////////////////////////////////
		private void OnEnable()
		{
			if (kSupportedValuesTypeNames == null)
			{
				kEntryValueTypes = BlackboardEntryBase.Editor.GetEntryValueTypes();
				int length = kEntryValueTypes.Length;
				kSupportedValuesTypeNames = new string[length];
				for (int i = 0; i < length; i++)
				{
					kSupportedValuesTypeNames[i] = kEntryValueTypes[i].Name;
				}
			}

			m_Blackboard = (Blackboard)serializedObject.targetObject;
			Blackboard.Editor.EnsureKeysLoaded(m_Blackboard);
			m_KeyList = serializedObject.FindProperty("m_Keys");

			m_ReorderableKeysList = new ReorderableList(serializedObject, elements: m_KeyList, draggable: false, displayHeader: false, displayAddButton: false, displayRemoveButton: false)
			{
				elementHeightCallback = _ => EditorGUIUtility.singleLineHeight,
				drawElementCallback = KeyList_DrawElement,
				multiSelect = true
			};
		}

		#region KeyList

		//////////////////////////////////////////////////////////////////////////
		private void KeyList_OnAddDropdownCallback()
		{
			string keyName = string.Empty;

			bool TryAcceptOption(string InValue)
			{
				System.Type type = kEntryValueTypes.FirstOrDefault(t => t.Name == InValue);
				if (Utils.CustomAssertions.IsNotNull(type))
				{
					using (new Utils.Editor.MarkAsDirty(m_Blackboard))
					{
						Blackboard.Editor.AddKey(m_Blackboard, keyName, type);
						Blackboard.Editor.SortKeys(m_Blackboard);
					}
				}
				return true;
			}

			bool TryAcceptValue(string InValue)
			{
				if (string.IsNullOrEmpty(InValue))
				{
					EditorUtility.DisplayDialog("Invalid value", $"Key name accepts only a non zero length string", "OK");
					return false;
				}

				if (Blackboard.Editor.HasKey(m_Blackboard, InValue))
				{
					EditorUtility.DisplayDialog("Already exists", $"Key {InValue} already defined in blackboard", "OK");
					return false;
				}

				keyName = InValue;
				EditorUtils.InputValueWindow.OpenDropdown(TryAcceptOption, null, kSupportedValuesTypeNames);
				return true;
			}
			EditorUtils.InputValueWindow.OpenStringInput(TryAcceptValue, null);
		}

		//////////////////////////////////////////////////////////////////////////
		private void KeyList_OnRemoveCallback()
		{
			using (new Utils.Editor.MarkAsDirty(m_Blackboard))
			{
				for (int i = m_ReorderableKeysList.selectedIndices.Count - 1; i >= 0; i--)
				{
					int selectedIndex = m_ReorderableKeysList.selectedIndices[i];
					if (Blackboard.Editor.TryGetKeyAt(m_Blackboard, selectedIndex, out BlackboardKeySpecifier specifier))
					{
						BlackboardEntryKey entryKey = specifier.Key;
						Blackboard.Editor.RemoveKey(m_Blackboard, entryKey);
					}
				}
				m_ReorderableKeysList.ClearSelection();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void KeyList_DrawElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			Rect[] areas = SplitHorizontally(rect, 50f, 50f);
			KeyList_DrawKey(areas[0], index);
			KeyList_DrawValue(areas[1], index);
		}

		//////////////////////////////////////////////////////////////////////////
		private void KeyList_DrawKey(Rect rect, int index)
		{
			if (Blackboard.Editor.TryGetKeyAt(m_Blackboard, index, out BlackboardKeySpecifier specifier))
			{
				BlackboardEntryKey entryKey = specifier.Key;
				EditorGUI.LabelField(rect, string.IsNullOrEmpty(entryKey) ? "N/A" : entryKey);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void KeyList_DrawValue(Rect rect, int index)
		{
			if (Blackboard.Editor.TryGetKeyAt(m_Blackboard, index, out BlackboardKeySpecifier specifier))
			{
				System.Type specifierType = specifier.Type;
				EditorGUI.LabelField(rect, specifierType.Name);
			}
		}

		#endregion

		//////////////////////////////////////////////////////////////////////////
		public override void OnInspectorGUI()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Add key"))
				{
					KeyList_OnAddDropdownCallback();
				}
				if (m_ReorderableKeysList.selectedIndices.Count > 0 && GUILayout.Button("Remove Selected"))
				{
					KeyList_OnRemoveCallback();
				}
			}

			serializedObject.Update(); // Update the array property's representation in the inspector

			m_ReorderableKeysList.DoLayoutList();

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