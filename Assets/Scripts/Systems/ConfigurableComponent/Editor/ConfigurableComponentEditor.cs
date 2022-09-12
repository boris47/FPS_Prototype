using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public sealed class ConfigurableComponentEditor
{
	/*
	[System.Serializable]
	private class ConfigurableData : System.IEquatable<ConfigurableData>
	{
		[SerializeField]
		private				TypeIdentifier				m_ConfigurableComponentIdentifier				= null;
		[SerializeField]
		private				string						m_AssetLocation									= string.Empty;
		[SerializeField]
		private				TypeIdentifier				m_ConfigurableTypeIdentifier					= null;

		private				System.Type					m_CachedComponentType							= null;
		private				System.Type					m_CachedConfigurableType						= null;


		public				TypeIdentifier				ConfigurableComponentIdentifier					=> m_ConfigurableComponentIdentifier;
		public				string						AssetLocation									=> m_AssetLocation;
		public				TypeIdentifier				ConfigurableTypeIdentifier						=> m_ConfigurableTypeIdentifier;

		public System.Type ConfigurableComponentType
		{
			get
			{
				if (m_CachedComponentType == null)
				{
					ConfigurableComponentIdentifier.TryGetType(out m_CachedComponentType);
				}
				return m_CachedComponentType;
			}
		}
		public System.Type ConfigurableType
		{
			get
			{
				if (m_CachedConfigurableType == null)
				{
					ConfigurableTypeIdentifier.TryGetType(out m_CachedConfigurableType);
				}
				return m_CachedConfigurableType;
			}
		}

		public ConfigurableData(in System.Type InConfigurableComponentType, in string InAssetLocation, in System.Type InConfigurableType)
		{
			m_ConfigurableComponentIdentifier		= new TypeIdentifier(InConfigurableComponentType);
			m_AssetLocation							= InAssetLocation;
			m_ConfigurableTypeIdentifier			= new TypeIdentifier(InConfigurableType);
		}

		public override bool Equals(object obj) => Equals(obj as ConfigurableData);
		

		public bool Equals(ConfigurableData other)
		{
			return other.IsNotNull() &&
				   EqualityComparer<TypeIdentifier>.Default.Equals(m_ConfigurableComponentIdentifier, other.m_ConfigurableComponentIdentifier) &&
				   m_AssetLocation == other.m_AssetLocation &&
				   EqualityComparer<TypeIdentifier>.Default.Equals(m_ConfigurableTypeIdentifier, other.m_ConfigurableTypeIdentifier);
		}

		public static bool operator ==(ConfigurableData left, ConfigurableData right) => EqualityComparer<ConfigurableData>.Default.Equals(left, right);

		public static bool operator !=(ConfigurableData left, ConfigurableData right) => !(left == right);
	}

	[SerializeField]
	private List<ConfigurableData> m_ConfigurablesData = new List<ConfigurableData>();

	*/

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

		string[] subFolders = AssetDatabase.GetSubFolders($"Assets/Resources/{Configurable.MainFolderName}");
		List<string> allFolders = new List<string>(subFolders);
		foreach (string subFolder in subFolders)
		{
			RecursiveFindFolders(subFolder, allFolders);
		}

		string[] configurablesGUIDS = AssetDatabase.FindAssets(string.Empty, allFolders.ToArray());
		string[] configurablesAssetPath = configurablesGUIDS
			.Select(guid => AssetDatabase.GUIDToAssetPath(guid))
			.Where(p => Utils.Paths.IsAsset(p))
			.ToArray();

		return configurablesAssetPath;
	}

	//////////////////////////////////////////////////////////////////////////
	private static bool TryGetConfigurationType(in System.Type configurableComponentType, out System.Type OutConfigurationType)
	{
		OutConfigurationType = null;
		if (ReflectionHelper.TryGetAttributeValue(configurableComponentType, (Configurable configurable) => configurable, out Configurable configurable))
		{
			// Get and use, if valid, field name to retrieve the config type
			string fieldName = configurable.FieldName;
			if (!string.IsNullOrEmpty(fieldName))
			{
				FieldInfo field = configurableComponentType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
				if (field.IsNotNull())
				{
					OutConfigurationType = field.FieldType;
				}
				else
				{
					Debug.LogError($"ConfigurableComponent: Used field name \"${fieldName}\" cannot be retrieved from type {configurableComponentType.Name}");
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
	private static bool TryGetResourcePath(in System.Type configurableComponentType, out string OutResourcePath)
	{
		OutResourcePath = string.Empty;
		if (ReflectionHelper.TryGetAttributeValue(configurableComponentType, (Configurable configurable) => configurable, out Configurable configurable))
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
		}

		foreach (System.Type configurableComponentType in TypeCache.GetTypesWithAttribute<Configurable>())
		{
			if (TryGetConfigurationType(configurableComponentType, out System.Type ConfigurationType))
			{
				string OutErrorMsg = null;
				if (!IsValidConfigurationType(ConfigurationType, out OutErrorMsg))
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
		private static bool s_IsOpen = true;
		private ConfigurationBase m_Configuration = null;
		private SerializedObject m_SerializedConfigObject = null;
		private string m_Label = string.Empty;

		//////////////////////////////////////////////////////////////////////////
		private void OnEnable()
		{
			Component componentInstance = serializedObject.targetObject as Component;
			if (componentInstance.IsNotNull() && componentInstance.TryGetConfiguration(out m_Configuration))
			{
				m_SerializedConfigObject = new SerializedObject(m_Configuration);
				m_Label = $"{ObjectNames.NicifyVariableName(componentInstance.GetType().Name)} Configs";
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnDisable()
		{
			if (m_SerializedConfigObject.IsNotNull())
			{
				m_SerializedConfigObject.Dispose();

			}
			if (m_Configuration.IsNotNull())
			{
				AssetDatabase.SaveAssetIfDirty(m_Configuration);
				Resources.UnloadAsset(m_Configuration);
			}
			m_SerializedConfigObject = null;
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

			// Draw configuration
			if (m_SerializedConfigObject.IsNotNull())
			{
				GUILayout.BeginHorizontal();
				{
					s_IsOpen = EditorGUILayout.Foldout(s_IsOpen, m_Label, toggleOnLabelClick: true);

					if (s_IsOpen && GUILayout.Button("Locate"))
					{
						EditorGUIUtility.PingObject(m_SerializedConfigObject.targetObject);
					}
				}
				GUILayout.EndHorizontal();

				if (s_IsOpen)
				{
					m_SerializedConfigObject.DoLayoutWithoutScriptProperty();

					GUILayout.Label("-------------------------------");

					GUILayout.Space(10f);
				}
			}

			// Normal component draw
			serializedObject.DoLayoutWithoutScriptProperty();
		}
	}
}
