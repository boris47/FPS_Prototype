
namespace Entities.AI.Components
{
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

	[CustomEditor(typeof(Blackboard))]
	public class BlackboardEditor : Editor
	{
		private Blackboard m_Blackboard = null;
		private SerializedProperty m_KeyList = null;
		private SerializedProperty m_EntryTable = null;
		private ReorderableList m_ReorderableKeysList = null;
		private ReorderableList m_ReorderableEntriesList = null;


		//////////////////////////////////////////////////////////////////////////
		private void OnEnable()
		{
			m_Blackboard = (Blackboard)serializedObject.targetObject;
			m_KeyList = serializedObject.FindProperty("m_Keys");
			m_EntryTable = serializedObject.FindProperty("m_Entries");

			m_ReorderableEntriesList = new ReorderableList(serializedObject, elements: m_KeyList, draggable: false, displayHeader: false, displayAddButton: /*true*/false, displayRemoveButton: /*true*/false)
			{
				elementHeightCallback = _ => EditorGUIUtility.singleLineHeight,
			//	drawElementCallback = DrawElement,
			//	onAddDropdownCallback = OnAddDropdownCallback,
			//	onRemoveCallback = OnRemoveCallback
			};
		}

		System.Type[] m_SupportedValuesType = new System.Type[]
		{
			typeof(bool), typeof(int), typeof(float), typeof(string),
			typeof(Vector3), typeof(Quaternion), typeof(Entity)
		};

		//////////////////////////////////////////////////////////////////////////
		private void OnAddDropdownCallback(Rect InRect, ReorderableList InList)
		{
			string name = string.Empty;
			System.Type supportedType = null;

			bool TryAcceptType(System.Type InType)
			{
				supportedType = InType;

				m_Blackboard.AddKey(name, supportedType);
				return true;
			}

			bool TryAcceptValue(string InValue)
			{
				if (string.IsNullOrEmpty(InValue))
				{
					EditorUtility.DisplayDialog("Invalid value", $"Key name accepts only a non zero length string", "OK");
					return false;
				}

				if (m_Blackboard.HasKeyRegistered(InValue))
				{
					EditorUtility.DisplayDialog("Already exists", $"Key {InValue} already defined in blackboard", "OK");
					return false;
				}

				name = InValue;
				EditorUtils.InputValueWindow.OpenSystemTypeType(TryAcceptType, null, m_SupportedValuesType);
				return true;
			}
			EditorUtils.InputValueWindow.OpenStringInput(TryAcceptValue, null);
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnRemoveCallback(ReorderableList InList)
		{
			/*
			SerializedProperty p = InList.serializedProperty.GetArrayElementAtIndex(InList.index);

			ReorderableList.defaultBehaviours.DoRemoveButton(InList);
			InList.index
			m_Blackboard.RemoveEntry()
			*/
		}

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
		public override void OnInspectorGUI()
		{
			serializedObject.Update(); // Update the array property's representation in the inspector

			m_ReorderableEntriesList.DoLayoutList(); // Have the ReorderableList do its work

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