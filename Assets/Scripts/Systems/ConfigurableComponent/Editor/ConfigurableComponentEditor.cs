using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public sealed class ConfigurableComponentEditor
{
	//////////////////////////////////////////////////////////////////////////
	/// Ref: https://forum.unity.com/threads/how-can-i-callback-after-the-scripts-compilation.819492/#post-5429361
	[UnityEditor.Callbacks.DidReloadScripts]
	private static void CreateAssetWhenReady()
	{
		if (EditorApplication.isCompiling || EditorApplication.isUpdating)
		{
			EditorApplication.delayCall += CreateAssetWhenReady;
			return;
		}

		static void CreateAssetsNow()
		{
			EditorApplication.delayCall -= CreateAssetsNow;
			if (!EditorApplication.isPlayingOrWillChangePlaymode)
			{
				CreateAssetsNowInternal();
			}
		}

		EditorApplication.delayCall += CreateAssetsNow;
	}

	//////////////////////////////////////////////////////////////////////////
	private static string[] CollectConfigurationAssetPaths()
	{
		static void RecursiveFindFolders(string folder, List<string> allFolders)
		{
			string[] subFolders = AssetDatabase.GetSubFolders(folder);
			allFolders.AddRange(subFolders);
			foreach (string subFolder in subFolders)
			{
				RecursiveFindFolders(subFolder, allFolders);
			}
		}

		string[] subFolders = AssetDatabase.GetSubFolders("Assets/Resources");
		List<string> allFolders = new List<string>(subFolders);
		foreach (string subFolder in subFolders)
		{
			RecursiveFindFolders(subFolder, allFolders);
		}

		string[] configurablesGUIDS = AssetDatabase.FindAssets($"t: {typeof(ConfigurationBase).FullName}", allFolders.ToArray());
		string[] configurablesAssetPath = configurablesGUIDS
			.Select(guid => AssetDatabase.GUIDToAssetPath(guid))
			.Where(p => Utils.Paths.IsAsset(p))
			.ToArray();

		return configurablesAssetPath;
	}

	//////////////////////////////////////////////////////////////////////////
	private static bool TryGetConfigurationType(in System.Type InConfigurableComponentType, out System.Type OutConfigurationType)
	{
		OutConfigurationType = null;
		if (ReflectionHelper.TryGetAttributeValue(InConfigurableComponentType, (Configurable configurable) => configurable, out Configurable configurable))
		{
			// Get and use, if valid, field name to retrieve the config type
			string fieldName = configurable.FieldName;
			if (!string.IsNullOrEmpty(fieldName))
			{
				FieldInfo field = InConfigurableComponentType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
				if (field.IsNotNull())
				{
					OutConfigurationType = field.FieldType;
				}
				else
				{
					Debug.LogError($"ConfigurableComponent: Used field name \"${fieldName}\" cannot be retrieved from type {InConfigurableComponentType.Name}");
				}
			}
			// Or get it from Configurable
			else
			{
				OutConfigurationType = configurable.ConfigurationType;
			}
		}
		return OutConfigurationType.IsNotNull();
	}

	//////////////////////////////////////////////////////////////////////////
	private static bool IsValidConfigurationType(in System.Type InConfigurationType, out string OutError)
	{
		OutError = string.Empty;
		if (!ReflectionHelper.IsInerithedFrom(typeof(ConfigurationBase), InConfigurationType))
		{
			OutError = $"[Config:{InConfigurationType.Name}]: Configuration must inerith from {nameof(ConfigurationBase)}";
		}

		if (InConfigurationType.IsAbstract)
		{
			OutError = $"[Config:{InConfigurationType.Name}]: Configuration class is abstract";
		}
		return string.IsNullOrEmpty(OutError);
	}

	//////////////////////////////////////////////////////////////////////////
	private static bool TryGetResourcePath(in System.Type InConfigurableComponentType, out string OutResourcePath)
	{
		OutResourcePath = string.Empty;
		if (ReflectionHelper.TryGetAttributeValue(InConfigurableComponentType, (Configurable configurable) => configurable, out Configurable configurable) && configurable.IsNotNull())
		{
			if (Utils.Paths.IsResourcesPath(configurable.ResourcePath))
			{
				OutResourcePath = configurable.ResourcePath;
			}
		}
		return !string.IsNullOrEmpty(OutResourcePath);
	}

	//////////////////////////////////////////////////////////////////////////
	private static void CreateAssetsNowInternal()
	{
		// Before everything remove invalid assets
		foreach (string assetPath in CollectConfigurationAssetPaths())
		{
			ConfigurationBase config = AssetDatabase.LoadAssetAtPath<ConfigurationBase>(assetPath);
			if (config == null)
			{
				// Invalid configuration
				AssetDatabase.DeleteAsset(assetPath);
			}
			else
			{
				Resources.UnloadAsset(config);
			}
		}

		foreach (System.Type configurableComponentType in TypeCache.GetTypesWithAttribute<Configurable>())
		{
			if (TryGetConfigurationType(configurableComponentType, out System.Type ConfigurationType))
			{
				if (!IsValidConfigurationType(ConfigurationType, out string OutErrorMsg))
				{
					Debug.LogError($"ConfigurableComponent: Type {configurableComponentType.Name} {OutErrorMsg}");
				}
				else
				{
					if (TryGetResourcePath(configurableComponentType, out string ResourcePath))
					{
						if (Utils.Paths.TryConvertFromResourcePathToAssetPath(ResourcePath, out string AssetPath))
						{
							// Compare ConfigType and existing one
							ConfigurationBase existingConfig = Resources.Load<ConfigurationBase>(ResourcePath);
							if (existingConfig == null || existingConfig.GetType() != ConfigurationType)
							{
								Debug.LogWarning($"ConfigurableComponent: Recreating configuration {ConfigurationType} as {AssetPath} because not existing or of different type");
								Resources.UnloadAsset(existingConfig);
								AssetDatabase.DeleteAsset(AssetPath);
							}
							else
							{
								Resources.UnloadAsset(existingConfig);
							}

							// Create asset if not exists
							if (!System.IO.File.Exists(AssetPath))
							{
								System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(AssetPath));

								AssetDatabase.CreateAsset(ScriptableObject.CreateInstance(ConfigurationType), AssetPath);
							}
						}
						else
						{
							Debug.LogError($"ConfigurableComponent: Type {configurableComponentType.Name} [Config:{ConfigurationType.Name}]: Unable to convert resource path to asset path");
						}
					}
					else
					{
						Debug.LogError($"ConfigurableComponent: Type {configurableComponentType.Name}: Invalid ${nameof(ResourcePath)}");
					}
				}
			}
			else
			{
				Debug.LogError($"ConfigurableComponent: Invalid configuration type for component {configurableComponentType.Name}");
			}
		}
		EditorUtility.UnloadUnusedAssetsImmediate();
	}


	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////

	[CustomEditor(typeof(Component), editorForChildClasses: true)]
	private class ComponentEditor : Editor
	{
		private class ConfigurableData
		{
			public bool bIsOpened = false;
			public string HoldingTypeName = string.Empty;
			public ConfigurationBase TheConfiguration = null;
			public SerializedObject SerializedObject = null;
		}

		private List<ConfigurableData> m_Data = new List<ConfigurableData>();


		//////////////////////////////////////////////////////////////////////////
		private void OnEnable()
		{
			Component componentInstance = serializedObject.targetObject as Component;

			System.Type currentType = componentInstance.GetType();
			while (currentType.BaseType.IsNotNull())
			{
				if (ReflectionHelper.TryGetAttributeValue(currentType, (Configurable configurable) => configurable.ResourcePath, out string ResourcePath))
				{
					ConfigurationBase loadedConfiguration = Resources.Load<ConfigurationBase>(ResourcePath);
					m_Data.Add( new ConfigurableData()
					{
						HoldingTypeName = currentType.Name,
						TheConfiguration = loadedConfiguration,
						SerializedObject = new SerializedObject(loadedConfiguration)
					});
				}
				currentType = currentType.BaseType;
			}

			m_Data.Reverse();
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnDisable()
		{
			foreach (ConfigurableData data in m_Data)
			{
				data.SerializedObject.Dispose();
				AssetDatabase.SaveAssetIfDirty(data.TheConfiguration);
				Resources.UnloadAsset(data.TheConfiguration);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public override void OnInspectorGUI()
		{
			// Draw script as usual
			SerializedProperty prop = serializedObject.FindProperty("m_Script");
			if (prop.IsNotNull())
			{
				using (new EditorGUI.DisabledScope(disabled: true))
				{
					EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true);
				}
			}

			if (m_Data.Count > 0)
			{
				GUILayout.Space(10f);

				GUILayout.Label("-------------------------------");
				
				// Draw configurations
				foreach (ConfigurableData data in m_Data)
				{
					
					GUILayout.BeginHorizontal();
					{
						string label = $"{ObjectNames.NicifyVariableName(data.HoldingTypeName)} Configs";
						data.bIsOpened = EditorGUILayout.Foldout(data.bIsOpened, label, toggleOnLabelClick: true);

						if (GUILayout.Button("Locate"))
						{
							EditorGUIUtility.PingObject(data.SerializedObject.targetObject);
						}
					}
					GUILayout.EndHorizontal();

					if (data.bIsOpened)
					{
						data.SerializedObject.DoLayoutWithoutScriptProperty();
					}
				}

				GUILayout.Label("-------------------------------");

				GUILayout.Space(10f);
			}

			// Normal component draw
			serializedObject.DoLayoutWithoutScriptProperty();
		}
	}
}
