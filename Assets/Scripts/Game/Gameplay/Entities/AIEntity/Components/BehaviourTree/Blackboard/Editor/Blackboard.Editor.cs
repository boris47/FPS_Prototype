﻿
namespace Entities.AI.Components
{
	using System.Linq;
	using UnityEngine;
	using UnityEditor;
	using UnityEditorInternal;

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
			m_KeyList = serializedObject.FindProperty(Blackboard.Editor.KeyListPropertyName);

			if (Utils.CustomAssertions.IsNotNull(m_KeyList))
			{
				m_ReorderableKeysList = new ReorderableList(serializedObject, elements: m_KeyList, draggable: false, displayHeader: false, displayAddButton: false, displayRemoveButton: false)
				{
					elementHeightCallback = _ => EditorGUIUtility.singleLineHeight,
					drawElementCallback = KeyList_DrawElement,
					multiSelect = true
				};
			}
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

		private static int legth = uint.MaxValue.ToString().Length;

		//////////////////////////////////////////////////////////////////////////
		private void KeyList_DrawKey(Rect rect, int index)
		{
			if (Blackboard.Editor.TryGetKeyAt(m_Blackboard, index, out BlackboardKeySpecifier specifier))
			{
				BlackboardEntryKey entryKey = specifier.Key;
				string paddedUniqueId = entryKey.UniqueId.ToString().PadLeft(legth, ' ');
				EditorGUI.LabelField(rect, string.IsNullOrEmpty(entryKey) ? "N/A" : $"({paddedUniqueId}) {entryKey.Name}");
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
			if (m_ReorderableKeysList.IsNotNull())
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