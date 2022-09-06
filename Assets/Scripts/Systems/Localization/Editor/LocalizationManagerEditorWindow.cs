using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Localization
{
	internal sealed class LocalizationManagerEditorWindow : GuardedEditorWindow<LocalizationManagerEditorWindow, LocalizationData>
	{
	//	private				List<Rect>									m_RowElements							= new List<Rect>();
	//	private				List<Rect>									m_ListElements							= new List<Rect>();
	//	private				List<List<Rect>>							m_TextFieldsElements					= new List<List<Rect>>();
	//	private				bool										m_IsInterfaceCreated					= false;
		private				SystemLanguage[]							m_Languages								= null;
		private				string[]									m_LanguagesNames						= null;
		private				int											m_LanguagesCount						= 0;
		private				int											m_KeysCount								= 0;
		private				string										m_KeySearchFilter						= string.Empty;

		private				int											m_CurrentTabIndex						= 0;

		//////////////////////////////////////////////////////////////////////////
		[MenuItem("Window/Localizations")]
		internal static void OnMenuItem()
		{
			OpenWindow("Localizations", LocalizationData.ResourcePath, new Vector2(400f, 200f), new Vector2(1200, 900f));
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OpenWindow(LocalizationData InLocalizationData)
		{
			OpenWindow("Localizations", LocalizationData.ResourcePath, InLocalizationData, new Vector2(400f, 200f), new Vector2(1200, 900f));
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnBeforeShow()
		{
			base.OnBeforeShow();

			RefreshViewData();
			LocalizationData.Editor.Sync(Data);
		}

		//////////////////////////////////////////////////////////////////////////
		private void RefreshViewData()
		{
			m_Languages = Data.AvailableLanguages;
			m_LanguagesNames = m_Languages.Select(l => l.ToString()).ToArray();
			m_LanguagesCount = m_Languages.Length;
			m_KeysCount = LocalizationData.Editor.GetKeysCount(Data);
		}


		//////////////////////////////////////////////////////////////////////////
		private void OnKeyAddRequest(string newKey)
		{
			if (LocalizationData.Editor.KeyExists(Data, newKey))
			{
				EditorUtility.DisplayDialog("Key already exists", $"The key '{newKey}' already registered", "OK");
			}
			else
			{
				LocalizationData.Editor.AddKey(Data, newKey);
				RefreshViewData();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnKeyRemoveRequest(string key)
		{
			LocalizationData.Editor.RemoveKey(Data, key);
			RefreshViewData();
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnGUI()
		{
			GUILayout.BeginVertical();
			{
				GUILayout.BeginHorizontal();
				{
					m_KeySearchFilter = GUILayout.TextField(m_KeySearchFilter, GUILayout.Width(190f));
					if (GUILayout.Button("Add Key", GUILayout.MaxWidth(90f)))
					{
						EditorUtils.InputValueWindow.OpenStringInput(OnKeyAddRequest, null);
					}
					if (GUILayout.Button("Languages", GUILayout.MaxWidth(90f)))
					{
						ShowLanguageSelectionMenu();
					}
					m_CurrentTabIndex = GUILayout.Toolbar(m_CurrentTabIndex, m_LanguagesNames);
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndHorizontal();

				for (int i = 0; i < m_KeysCount; i++)
				{
					if (LocalizationData.Editor.TryGetKeyAt(Data, i, out string key))
					{
						if (key.Contains(m_KeySearchFilter))
						{
							GUILayout.BeginHorizontal();
							{
								// Key
								GUILayout.Label(key, GUILayout.Width(100f));
								if (GUILayout.Button("X", GUILayout.Width(15f)))
								{
									OnKeyRemoveRequest(key);
								}

								// Value
								if (m_Languages.TryGetByIndex(m_CurrentTabIndex, out SystemLanguage language) && Data.TryGetValue(language, out LocalizationTable localizationTable))
								{
									string value = LocalizationTable.Editor.GetValueAt(localizationTable, i);
									string newValue = GUILayout.TextField(value, GUILayout.Width(300f));
									if (newValue != value)
									{
										LocalizationTable.Editor.SetKeyValue(localizationTable, key, newValue);
									}
								}
							}
							GUILayout.EndHorizontal();
						}
					}
				}
			}
			GUILayout.EndVertical();
		}

		//////////////////////////////////////////////////////////////////////////
		private void ShowLanguageSelectionMenu()
		{
			void OnLanguageSelected(object lang)
			{
				SystemLanguage language = (SystemLanguage)lang;
				if (LocalizationData.Editor.HasLanguage(Data, language))
				{
					LocalizationData.Editor.RemoveLanguage(Data, language);
				}
				else
				{
					LocalizationData.Editor.AddLanguage(Data, language);
				}
				RefreshViewData();
			}

			// create the menu and add items to it
			GenericMenu menu = new GenericMenu();

			string[] names = System.Enum.GetNames(typeof(SystemLanguage));
			for (int i = 0, length = names.Length; i < length; i++)
			{
				string name = names[i];
				SystemLanguage language = (SystemLanguage)System.Enum.Parse(typeof(SystemLanguage), name);
				menu.AddItem(new GUIContent(name), m_Languages.Contains(language), OnLanguageSelected, language);
			}

			// display the menu
			menu.ShowAsContext();
		}


		/*

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();
			
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();

			m_RowElements.Clear();
			m_ListElements.Clear();
			m_TextFieldsElements.Clear();
		}


		//////////////////////////////////////////////////////////////////////////
		private void CreateInterface()
		{
			m_LanguagesNames = Data.AvailableLanguages.Select(l => l.ToString()).ToArray();
			Vector2 biggestLabelSize = m_LanguagesNames.Select(s => EditorStyles.label.CalcSize(new GUIContent(s))).MaxBy(v => v.sqrMagnitude);

			const float textFieldWidth = 100f;
			const float leftPanelStart = 10f;
			const float upRowPanelStart = 10f;
			const float columnSeparator = 15f;
			float rowLineHeight = EditorGUIUtility.singleLineHeight * 1.3f;
			float labelVsSliderWidth = Mathf.Max(biggestLabelSize.x, textFieldWidth);
			float rightPanelStart = leftPanelStart + labelVsSliderWidth + 10f;
			float upTableStart = upRowPanelStart + rowLineHeight;

			Vector2 position = Vector2.zero;

			// Add top row labels (languages)
			position.Set(rightPanelStart, upRowPanelStart);
			{
				for (int i = 0, length = m_LanguagesNames.Length; i < length; i++)
				{
					string name = m_LanguagesNames[i];
					Vector2 nameSize = EditorStyles.label.CalcSize(new GUIContent(name));
					m_RowElements.Add(new Rect(position.x, position.y, nameSize.x, nameSize.y));
					position.x += labelVsSliderWidth + columnSeparator;
				}
			}

			// Left names list item (keys)
			position.Set(leftPanelStart, upTableStart);
			{
				for (int i = 0, length = LocalizationData.Editor.GetKeysCount(Data); i < length; i++)
				{
					if (LocalizationData.Editor.TryGetKeyAt(Data, i, out string key))
					{
						Vector2 nameSize = EditorStyles.label.CalcSize(new GUIContent(key));
						m_ListElements.Add(new Rect(position.x, position.y, nameSize.x, nameSize.y));
						position.y += rowLineHeight;
					}
				}
			}

			// Text fields (Values)
			position.Set(rightPanelStart, upTableStart);
			{
				for (int i = 0, length = m_LanguagesNames.Length; i < length; i++)
				{
					List<Rect> textFieldsForThisRow = m_TextFieldsElements.AddRef(new List<Rect>());
					for (int k = 0, kLength = LocalizationData.Editor.GetKeysCount(Data); k < kLength; k++)
					{
						textFieldsForThisRow.Add(new Rect(position.x - (labelVsSliderWidth * 0.25f) + (biggestLabelSize.x * 0.5f), position.y, labelVsSliderWidth * 0.5f, rowLineHeight));
						position.x += labelVsSliderWidth + columnSeparator;
					}
					position.x = rightPanelStart;
					position.y += rowLineHeight;
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		private void OnGUI()
		{
			if (!m_IsInterfaceCreated)
			{
				m_IsInterfaceCreated = true;
				CreateInterface();
			}

			// Languages
			for (int i = 0, length = m_LanguagesNames.Length; i < length; i++)
			{
				string name = m_LanguagesNames[i];
				Rect rect = m_RowElements[i];
				GUI.Label(rect, name, EditorStyles.label);
			}

			// Key list
			for (int i = 0, length = LocalizationData.Editor.GetKeysCount(Data); i < length; i++)
			{
				if (LocalizationData.Editor.TryGetKeyAt(Data, i, out string key))
				{
					Rect rect = m_ListElements[i];
					GUI.Label(rect, name, EditorStyles.label);
				}
			}

			// Text fields
			for (int i = 0, length = m_LanguagesNames.Length; i < length; i++)
			{
				List<Rect> textFieldsForThisRow = m_TextFieldsElements[i];
				for (int k = 0, kLength = LocalizationData.Editor.GetKeysCount(Data); k < kLength; k++)
				{
					// EditorGUILayout.TextField
					Rect rect = textFieldsForThisRow[k];
					if (LocalizationData.Editor.TryGetTableAt(Data, i, out LocalizationTable table))
					{
						if (LocalizationTable.Editor.TryGetKeyAndValueAt(table, k, out string key, out string currentValue))
						{
							string newValue = GUI.TextField(rect, currentValue);
							if (currentValue != newValue)
							{
								LocalizationTable.Editor.SetKeyValue(table, key, newValue);
							}
						}
					}
				}
			}
		}
		*/
	}
}