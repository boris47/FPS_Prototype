
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif


[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
public class Configurable : System.Attribute
{
	public readonly string FieldName = null;
	public readonly System.Type ConfigType = null;
	public readonly string ResourcePath = null;

	private Configurable(string InResourcePath)
	{
		ResourcePath = InResourcePath;
	}

	public Configurable(System.Type InConfigType, string InResourcePath) : this(InResourcePath)
	{
		ConfigType = InConfigType;
	}

	public Configurable(string InFieldName, string InResourcePath) : this(InResourcePath)
	{
		FieldName = InFieldName;
	}
}


public static class ConfigurableComponent_Extension
{

	//////////////////////////////////////////////////////////////////////////
	public static bool TryGetConfiguration<T>(this Component component, out T OutConfiguration) where T : ConfigurationBase
	{
		OutConfiguration = null;
		if (ReflectionHelper.GetAttributeValue(component.GetType(), (Configurable configurable) => configurable.ResourcePath, out string ResourcePath))
		{
			OutConfiguration = Resources.Load<T>(ResourcePath);
		}
		return OutConfiguration.IsNotNull();
	}


#if UNITY_EDITOR
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

		EditorApplication.delayCall += CreateAssetsNow;
	}


	//////////////////////////////////////////////////////////////////////////
	private static void CreateAssetsNow()
	{
		EditorApplication.delayCall -= CreateAssetsNow;
		// TODO Search for orphan configurations

		if (!EditorApplication.isPlayingOrWillChangePlaymode)
		{
			foreach (System.Type configurableType in TypeCache.GetTypesWithAttribute<Configurable>())
			{
				// Extract attribute data
				if (Utils.CustomAssertions.IsTrue(ReflectionHelper.GetAttributeValue(configurableType, (Configurable configurable) => configurable, out Configurable configurable)))
				{
					// Get and verify ResourcePath
					string ResourcePath = configurable.ResourcePath;
					if (Utils.CustomAssertions.IsTrue(Utils.String.IsResourcesPath(ResourcePath), $"ConfigurableComponent: Type {configurableType.Name}: Invalid ${nameof(ResourcePath)}"))
					{
						System.Type ConfigType = null;

						// Get and use, if valid, field name to retrieve the config type
						string FieldName = configurable.FieldName;
						if (!string.IsNullOrEmpty(FieldName))
						{
							FieldInfo field = configurableType.GetField(FieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
							if (Utils.CustomAssertions.IsNotNull(field, $"ConfigurableComponent: Used field name \"${FieldName}\" cannot be retrieved from type {configurableType.Name}"))
							{
								ConfigType = field.FieldType;
							}
						}

						// Or get it from Configurable
						else
						{
							ConfigType = configurable.ConfigType;
						}

						// Validate configType
						if (Utils.CustomAssertions.IsNotNull(ConfigType, $"ConfigurableComponent: Invalid {nameof(ConfigType)} for type {configurableType.Name}"))
						{
							// Verify config type
							string err3 = $"ConfigurableComponent: Type {configurableType.Name} [Config:{ConfigType.Name}]: Configuration must inerith from {nameof(ConfigurationBase)}";
							string err4 = $"ConfigurableComponent: Type {configurableType.Name} [Config:{ConfigType.Name}]: Configuration class is abstract";
							if (Utils.CustomAssertions.IsTrue(ReflectionHelper.IsInerithedFrom(typeof(ConfigurationBase), ConfigType), err3)
							 && Utils.CustomAssertions.IsTrue(!ConfigType.IsAbstract, err4))
							{
								string err5 = $"ConfigurableComponent: Type {configurableType.Name} [Config:{ConfigType.Name}]: Unable to convert resource path to asset path";
								if (Utils.CustomAssertions.IsTrue(Utils.String.TryConvertFromResourcePathToAssetPath(ResourcePath, out string AssetPath), err5))
								{
									// Compare ConfigType and existing one
									ConfigurationBase existingConfig = AssetDatabase.LoadAssetAtPath<ConfigurationBase>(AssetPath);
									if (existingConfig.IsNotNull() && existingConfig.GetType() != ConfigType)
									{
										AssetDatabase.DeleteAsset(AssetPath);
									}

									// Create asset if not exists
									if (!System.IO.File.Exists(AssetPath))
									{
										System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(AssetPath));

										AssetDatabase.CreateAsset(ScriptableObject.CreateInstance(ConfigType), AssetPath);
									}
								}
							}
						}
					}
				}
			}
		}
		EditorUtility.UnloadUnusedAssetsImmediate();
	}


	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////

	[CustomEditor(typeof(Component), editorForChildClasses: true)]
	private class ComponentEditor : UnityEditor.Editor
	{
		private SerializedObject m_SerializedConfigObject = null;
		private Component m_Instance = null;


		//////////////////////////////////////////////////////////////////////////
		private void OnEnable()
		{
			m_Instance = serializedObject.targetObject as Component;
			if (m_Instance.IsNotNull() && m_Instance.TryGetConfiguration(out ConfigurationBase config))
			{
				m_SerializedConfigObject = new SerializedObject(config);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnDisable()
		{
			m_Instance = null;
			if (m_SerializedConfigObject.IsNotNull())
			{
				m_SerializedConfigObject.Dispose();
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
					GUILayout.Label("------------CONFIGS------------");
					if (GUILayout.Button("Locate"))
					{
						EditorGUIUtility.PingObject(m_SerializedConfigObject.targetObject);
					}
				}
				GUILayout.EndHorizontal();

				m_SerializedConfigObject.DoLayoutWithoutScrptProperty();

				GUILayout.Label("-------------------------------");

				GUILayout.Space(10f);
			}

			// Normal component draw
			{
				using(new LocalizationGroup(serializedObject))
				{
					EditorGUI.BeginChangeCheck();
					{
						serializedObject.UpdateIfRequiredOrScript();

						SerializedProperty iterator = serializedObject.GetIterator();
						bool enterChildren = true;
						while (iterator.NextVisible(enterChildren))
						{
							if (iterator.propertyPath != "m_Script")
							{
								EditorGUILayout.PropertyField(iterator, true);
							}
							enterChildren = false;
						}
					}
					if (EditorGUI.EndChangeCheck())
					{
						serializedObject.ApplyModifiedProperties();
					}
				}
			}
		}
	}
#endif
}