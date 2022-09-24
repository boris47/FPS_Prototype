using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Entities.Relations
{
	internal sealed class RelationsEditor : GuardedEditorWindow<RelationsEditor, RelationsData>
	{
		private static readonly Vector2									s_MinSize								= new Vector2(500f, 200f);
		private static readonly Vector2									s_MaxSize								= new Vector2(1200f, 900f);

		private				EntityFaction[]								m_EntityFactions						= null;
		private				string[]									m_EntityFactionsNames					= null;

		private				int											m_CurrentTabIndex						= 0;


		//////////////////////////////////////////////////////////////////////////
		[MenuItem("Window/Relations Editor")]
		internal static void OnMenuItem()
		{
			OpenWindow("Relations Editor", RelationsData.ResourcePath, s_MinSize, s_MaxSize);
		}
		
		//////////////////////////////////////////////////////////////////////////
		public static void OpenWindow(RelationsData InRelationsData)
		{
			OpenWindow("Relations Editor", RelationsData.ResourcePath, InRelationsData, s_MinSize, s_MaxSize);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnBeforeShow()
		{
			base.OnBeforeShow();

			RelationsData.Editor.Sync(Data);

			RefreshViewData();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();

			AssetDatabase.SaveAssetIfDirty(Data);
		}

		//////////////////////////////////////////////////////////////////////////
		private void RefreshViewData()
		{
			m_EntityFactions = Data.Factions;
			m_EntityFactionsNames = RelationsData.Editor.GetFactionsNames(Data);
		}

		//////////////////////////////////////////////////////////////////////////
		private bool OnCreateEntityFactionRequest(string InFactionName)
		{
			if (RelationsData.Editor.Contains(Data, InFactionName))
			{
				EditorUtility.DisplayDialog("Key already exists", $"The Faaction '{InFactionName}' already registered", "OK");
				return false;
			}

			using (new Utils.Editor.MarkAsDirty(Data))
			{
				EntityFaction newFaction = RelationsData.Editor.CreateFaction(Data, InFactionName);
				AssetDatabase.AddObjectToAsset(newFaction, Data);
			}
			RefreshViewData();
			return true;
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnEntityFactionRenameRequest(EntityFaction entityFaction)
		{
			bool onRename(string newName)
			{
				if (RelationsData.Editor.Contains(Data, newName))
				{
					EditorUtility.DisplayDialog("Entity faction already exists", $"The entity faction '{newName}' already registered", "OK");
					return false;
				}
				using (new Utils.Editor.MarkAsDirty(Data))
				{
					using (new Utils.Editor.MarkAsDirty(entityFaction))
					{
						RelationsData.Editor.Rename(Data, entityFaction, newName);
					}
				}
				Utils.Editor.ProjectBrowserResetter.Execute();
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Data));
				RefreshViewData();
				return true;
			}

			EditorUtils.InputValueWindow.OpenStringInput(onRename, null, entityFaction.FactionName);
		}

		//////////////////////////////////////////////////////////////////////////
		private bool OnEntityFactionRemoveRequest(in EntityFaction entityFaction)
		{
			bool bResult = false;
			if (EditorUtility.DisplayDialog("Entity factiondeletion", $"Are you sure you want to delete entity faction '{entityFaction.FactionName}' ", "Yes", "No"))
			{
				RelationsData.Editor.DeleteFaction(Data, entityFaction.FactionName);
				AssetDatabase.RemoveObjectFromAsset(entityFaction);
				RefreshViewData();
				bResult = true;
			}
			return bResult;
		}


		//////////////////////////////////////////////////////////////////////////
		private void OnGUI()
		{
			using (new EditorGUILayout.VerticalScope())
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					if (GUILayout.Button("Create Faction", GUILayout.MaxWidth(110f)))
					{
						EditorUtils.InputValueWindow.OpenStringInput(OnCreateEntityFactionRequest, null);
					}

					m_CurrentTabIndex = GUILayout.Toolbar(m_CurrentTabIndex, m_EntityFactionsNames);
					
					GUILayout.FlexibleSpace();
				}

				EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider); // ---------------------------------------------------------------------------

				if (m_EntityFactions.TryGetByIndex(m_CurrentTabIndex + 1, out EntityFaction entityFaction)) // +1 To avoid Default faction
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						if (GUILayout.Button("Rename", GUILayout.Width(100f)))
						{
							OnEntityFactionRenameRequest(entityFaction);
						}
						if (GUILayout.Button("Delete", GUILayout.Width(100f)))
						{
							OnEntityFactionRemoveRequest(entityFaction);
						}

						using (new EditorGUILayout.VerticalScope())
						{
							foreach (EntityFaction otherEntityFaction in Data.Factions)
							{
								if (!entityFaction.IsEqual(otherEntityFaction) && !EntityFaction.Editor.IsDefault(entityFaction))
								{
									using (new EditorGUILayout.VerticalScope(GUILayout.Width(200f)))
									{
										using (new EditorGUILayout.HorizontalScope())
										{
											GUILayout.Label($"{entityFaction.FactionName} -> {otherEntityFaction.FactionName}");

											if (GUILayout.Button("Reset", GUILayout.Width(70f)))
											{
												Data.OverrideRelations(entityFaction, otherEntityFaction, RelationsData.NeutralRelationValue);
											}
										}

										using (new Utils.Editor.CustomGUIBackgroundColor())
										{
											float value = Data.GetRelationValue(entityFaction, otherEntityFaction);
											GUI.backgroundColor = Color.Lerp(Color.red, Color.green, value);
											float newValue = GUILayout.HorizontalSlider(value, 0f, 1f, GUILayout.Height(40f));
											if (value != newValue)
											{
												Data.OverrideRelations(entityFaction, otherEntityFaction, newValue);
											}
										}
									}	
								}
							}
						}
					}
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////

		[CustomEditor(typeof(RelationsData))]
		internal class RelationsDataCustomEditor : Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();

				if (GUILayout.Button("Edit Relations"))
				{
					RelationsEditor.OpenWindow(serializedObject.targetObject as RelationsData);
				}
			}
		}
	}

}
