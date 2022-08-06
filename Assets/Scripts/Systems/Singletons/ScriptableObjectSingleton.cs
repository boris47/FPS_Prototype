
using System.Collections.Generic;
using UnityEngine;


[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
public class ScriptableObjectResourcePath : System.Attribute
{
	public readonly string ResourcePath = null;

	public ScriptableObjectResourcePath(string InResourcePath)
	{
		if(Utils.CustomAssertions.IsTrue(Utils.String.IsResourcesPath(InResourcePath)))
		{
			ResourcePath = InResourcePath;
		}
	}
}

/// <summary> Live coupled with <see cref="ScriptableObjectResourcePath"/> </summary>
public abstract class GlobalScriptableObjectSingleton<T> : ScriptableObject
	, SingletonInitializer.IAfterAssembliesLoaded
#if UNITY_EDITOR
	, SingletonInitializer.IEditorInitializeOnLoadMethod
#endif
	where T : ScriptableObject, new()
{
	private		static			T									m_InstanceInternal		= null;
	private		static			GlobalScriptableObjectSingleton<T>	m_SingletonInstance		= null;

	protected	static			T									m_Instance              => m_InstanceInternal;
	protected	static			bool								ShowDebugInfo			{ get; private set; } = false;

	public		static			T									Instance
	{
		get {
			LoadInstance();
			Utils.CustomAssertions.IsNotNull(m_InstanceInternal, $"{typeof(T).Name} has been not initialized correctly!!");
			return m_InstanceInternal;
		}
	}

#if UNITY_EDITOR
	private static void AfterAssembliesLoaded() => LoadInstance();
	private static void InitializeOnLoadMethod() => LoadInstance();
	protected static void RecreateInstance() => LoadInstance();
#endif

	private static void LoadInstance()
	{
		if (m_InstanceInternal == null)
		{
			if (Utils.CustomAssertions.IsTrue(ReflectionHelper.GetAttributeValue(typeof(T), (ScriptableObjectResourcePath a) => a.ResourcePath, out string OutResourcePath),
				$"Cannot retrieve attribute {nameof(ScriptableObjectResourcePath)} for class {typeof(T).Name}"))
			{
				T newInstance = Resources.Load<T>(OutResourcePath);
				
#if UNITY_EDITOR
				if (newInstance == null)
				{
					if (Utils.CustomAssertions.IsTrue(Utils.String.TryConvertFromResourcePathToAssetPath(OutResourcePath, out string AssetPath)))
					{
						newInstance = CreateInstance<T>();
						System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(AssetPath));
						UnityEditor.AssetDatabase.CreateAsset(newInstance, AssetPath);
						UnityEditor.AssetDatabase.SaveAssets();
					}

				}

				// Changing the preloaded assets is only effective if the editor is not in play mode
				if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
				{
					List<Object> preloadedAssets = new List<Object>(UnityEditor.PlayerSettings.GetPreloadedAssets());
					if (!preloadedAssets.Exists(asset => asset.IsNotNull() && asset.GetType() == typeof(T)))
					{
						preloadedAssets.Add(newInstance);
						UnityEditor.PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
						UnityEditor.AssetDatabase.SaveAssets();
					}
				}
#endif
				m_InstanceInternal = newInstance as T;
				m_SingletonInstance = (m_InstanceInternal as GlobalScriptableObjectSingleton<T>);
				if (Utils.CustomAssertions.IsNotNull(m_SingletonInstance))
				{
					m_SingletonInstance.OnInitialize();
				}
			}
		}
	}

	private void OnEnable()
	{
		if (m_InstanceInternal.IsNotNull() && m_InstanceInternal != this)
		{
			Debug.LogWarning("There are two instances of the GameplaySettings loaded. This should never happen.");
			Debug.LogWarning("Main instance ID: " + m_InstanceInternal.GetInstanceID() + ", other instance ID: " + GetInstanceID() + ".");
		}
	}

	protected virtual void OnInitialize() { }
}