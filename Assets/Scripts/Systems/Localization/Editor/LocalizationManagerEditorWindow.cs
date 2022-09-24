using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Localization
{
	internal sealed class LocalizationManagerEditorWindow : GuardedEditorWindow<LocalizationManagerEditorWindow, LocalizationData>
	{
		private static readonly Vector2									s_MinSize								= new Vector2(1200f, 200f);
		private static readonly Vector2									s_MaxSize								= new Vector2(1200f, 900f);

		private				SystemLanguage[]							m_Languages								= null;
		private				string[]									m_LanguagesNames						= null;
		private				string										m_KeySearchFilter						= string.Empty;
		private				AudioSource									m_AudioSource							= null;

		private				int											m_CurrentTabIndex						= 0;

		//////////////////////////////////////////////////////////////////////////
		[MenuItem("Window/Localizations")]
		internal static void OnMenuItem()
		{
			OpenWindow("Localizations", LocalizationData.ResourcePath, s_MinSize, s_MaxSize);
		}

		//////////////////////////////////////////////////////////////////////////
		public static void OpenWindow(LocalizationData InLocalizationData)
		{
			OpenWindow("Localizations", LocalizationData.ResourcePath, InLocalizationData, s_MinSize, s_MaxSize);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnBeforeShow()
		{
			base.OnBeforeShow();

			GameObject go = new GameObject();
			go.hideFlags = HideFlags.HideAndDontSave;
			m_AudioSource = go.AddComponent<AudioSource>();

			LocalizationData.Editor.Sync(Data);
			RefreshViewData();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();

			AssetDatabase.SaveAssetIfDirty(Data);

			m_AudioSource.Destroy();
		}

		//////////////////////////////////////////////////////////////////////////
		private void RefreshViewData()
		{
			m_Languages = Data.AvailableLanguages;
			m_LanguagesNames = m_Languages.Select(l => l.ToString()).ToArray();
		}


		//////////////////////////////////////////////////////////////////////////
		private bool OnKeyAddRequest(string InKeyName)
		{
			if (LocalizationKey.Editor.Contains(InKeyName))
			{
				EditorUtility.DisplayDialog("Key already exists", $"The key '{InKeyName}' already registered", "OK");
				return false;
			}

			using (new Utils.Editor.MarkAsDirty(Data))
			{
				LocalizationKey newKey = LocalizationKey.Editor.CreateAsset(InKeyName);
				LocalizationData.Editor.AddKey(Data, newKey);
				AssetDatabase.AddObjectToAsset(newKey, Data);
			}
			RefreshViewData();
			return true;
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnKeyRenameRequest(LocalizationKey key)
		{
			bool onRename(string newName)
			{
				if (LocalizationKey.Editor.Contains(key.name))
				{
					EditorUtility.DisplayDialog("Key already exists", $"The key '{key.name}' already registered", "OK");
					return false;
				}

				var path = AssetDatabase.GetAssetPath(key);
				AssetDatabase.RenameAsset(path, path.Replace(key.name, newName));
				return true;
			}

			EditorUtils.InputValueWindow.OpenStringInput(onRename, null);
		}

		//////////////////////////////////////////////////////////////////////////
		private bool OnKeyRemoveRequest(in LocalizationKey key)
		{
			bool bResult = false;
			if (EditorUtility.DisplayDialog("Key Deletion", $"Are you sure you want to delete key '{key.name}' ", "Yes", "No"))
			{
				LocalizationData.Editor.RemoveKey(Data, key);
				AssetDatabase.RemoveObjectFromAsset(key);
				RefreshViewData();
				bResult = true;
			}
			return bResult;
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnGUI()
		{
			GUILayout.BeginVertical();
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Filter:", GUILayout.Width(50f));
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

				EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider); // ---------------------------------------------------------------------------

				if (m_Languages.TryGetByIndex(m_CurrentTabIndex, out SystemLanguage language) && Data.TryGetValue(language, out LocalizationTable localizationTable))
				{
					LocalizationKey[] allKeys = LocalizationTable.Editor.GetKeys(localizationTable);
					for (int i = 0, length = allKeys.Length; i < length; i++)
					{
						LocalizationKey key = allKeys[i];
						if (key.name.Contains(m_KeySearchFilter))
						{
							GUILayout.BeginHorizontal();
							{
								// Key
								GUILayout.Label(key.name, GUILayout.Width(150f));
								if (GUILayout.Button("R", GUILayout.Width(18f)))
								{
									OnKeyRenameRequest(key);
								}
								if (GUILayout.Button("x", GUILayout.Width(18f)))
								{
									if (OnKeyRemoveRequest(key))
									{
										length -= 1;
										i -= 1;
									}
								}

								// Value
								if (LocalizationTable.Editor.TryGetValue(localizationTable, key, out LocalizationValue value))
								{
									if (LocalizationValue.Editor.DrawGUI(value, m_AudioSource))
									{
										EditorUtility.SetDirty(Data);
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
					if (Data.AvailableLanguages.Length == 1)
					{
						const string msg = "Must be selected at least one language";
						EditorUtility.DisplayDialog("Language deletion", msg, "Yes");
						return;
					}
					else
					{
						string msg = $"Language {language} is going to be removed from tables losing all data about it, operation cannot be reverted. Do you want to proceed?";
						if (EditorUtility.DisplayDialog("Language deletion", msg, "Yes", "No"))
						{
							LocalizationData.Editor.RemoveLanguage(Data, language);
						}
					}
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
	}
}