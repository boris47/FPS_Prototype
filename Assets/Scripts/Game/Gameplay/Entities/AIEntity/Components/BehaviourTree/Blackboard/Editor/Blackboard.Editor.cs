
namespace Entities.AI.Components
{
	using System.Linq;
	using UnityEngine;
	using UnityEditor;
	using UnityEditorInternal;

	public class BlackboardEditor : Editor
	{
		private static GUIStyle LeftAllighmentStyle = null;
		private static string[] kSupportedValuesTypeNames = null;
		private static System.Type[] kEntryValueTypes = null;

		private Blackboard m_Blackboard = null;
		private SerializedProperty m_KeyList = null;
		private ReorderableList m_ReorderableKeysList = null;

		protected virtual bool bIsReadonly { get; } = false;


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
				m_ReorderableKeysList = new ReorderableList(serializedObject, elements: m_KeyList, draggable: false, displayHeader: true, displayAddButton: false, displayRemoveButton: false)
				{
					elementHeightCallback = _ => EditorGUIUtility.singleLineHeight,
					drawElementCallback = KeyList_DrawElement,
					drawHeaderCallback = KeyList_DrawHeader,
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
		private void KeyList_OnRenameCallback()
		{
			int selectedIndex = m_ReorderableKeysList.selectedIndices[0];
			if (Blackboard.Editor.TryGetKeyAt(m_Blackboard, selectedIndex, out BlackboardKeySpecifier specifier))
			{
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

					Blackboard.Editor.RenameKey(specifier, InValue);
					return true;
				}

				EditorUtils.InputValueWindow.OpenStringInput(TryAcceptValue, null);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void KeyList_OnChangeTypeCallback()
		{
			int selectedIndex = m_ReorderableKeysList.selectedIndices[0];
			if (Blackboard.Editor.TryGetKeyAt(m_Blackboard, selectedIndex, out BlackboardKeySpecifier specifier))
			{
				bool TryAcceptOption(string InValue)
				{
					System.Type type = kEntryValueTypes.FirstOrDefault(t => t.Name == InValue);
					if (Utils.CustomAssertions.IsNotNull(type))
					{
						using (new Utils.Editor.MarkAsDirty(m_Blackboard))
						{
							Blackboard.Editor.ChangeTypeForKey(specifier, type);
						}
					}
					return true;
				}

				EditorUtils.InputValueWindow.OpenDropdown(TryAcceptOption, null, kSupportedValuesTypeNames);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void KeyList_DrawHeader(Rect InRect)
		{
			if (LeftAllighmentStyle == null)
			{
				LeftAllighmentStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
			}

			Rect[] areas = SplitHorizontally(InRect, 30f, 43f, 27f);
			EditorGUI.LabelField(areas[0], "Unique ID");
			EditorGUI.LabelField(areas[1], "Name");
			EditorGUI.LabelField(areas[2], "Value Type", LeftAllighmentStyle);
		}

		//////////////////////////////////////////////////////////////////////////
		private void KeyList_DrawElement(Rect InRect, int InIndex, bool isActive, bool isFocused)
		{
			if (LeftAllighmentStyle == null)
			{
				LeftAllighmentStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
			}

			if (Blackboard.Editor.TryGetKeyAt(m_Blackboard, InIndex, out BlackboardKeySpecifier specifier))
			{
				Rect[] areas = SplitHorizontally(InRect, 30f, 33f, 37f);
				KeyList_DrawKeyId(areas[0], specifier);
				KeyList_DrawKeyName(areas[1], specifier);
				KeyList_DrawValue(areas[2], specifier);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void KeyList_DrawKeyId(in Rect InRect, in BlackboardKeySpecifier InSpecifier)
		{
			BlackboardEntryKey entryKey = InSpecifier.Key;
			EditorGUI.LabelField(InRect, $"({entryKey.UniqueId})");
		}

		//////////////////////////////////////////////////////////////////////////
		private void KeyList_DrawKeyName(in Rect InRect, in BlackboardKeySpecifier InSpecifier)
		{
			BlackboardEntryKey entryKey = InSpecifier.Key;
			EditorGUI.LabelField(InRect, entryKey.Name);
		}

		//////////////////////////////////////////////////////////////////////////
		
		private void KeyList_DrawValue(in Rect InRect, in BlackboardKeySpecifier InSpecifier)
		{
			System.Type specifierType = InSpecifier.Type;
			EditorGUI.LabelField(InRect, specifierType.Name, LeftAllighmentStyle);
		}

		#endregion

		//////////////////////////////////////////////////////////////////////////
		public override void OnInspectorGUI()
		{
			if (m_ReorderableKeysList.IsNotNull())
			{
				if (!bIsReadonly)
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
						if (m_ReorderableKeysList.selectedIndices.Count > 0 && GUILayout.Button("Rename"))
						{
							KeyList_OnRenameCallback();
						}
						if (m_ReorderableKeysList.selectedIndices.Count > 0 && GUILayout.Button("Change Type"))
						{
							KeyList_OnChangeTypeCallback();
						}
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

	[CustomEditor(typeof(Blackboard))]
	public class BlackboardEditorReadOnly : BlackboardEditor
	{
		protected override bool bIsReadonly => true;
	}
}