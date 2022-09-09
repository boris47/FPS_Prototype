
using System.Linq;
using UnityEngine;

public class SingletonInitializer
{
	/// <summary> We use reflection to get all types implementing this interface and initialize them on phase <see cref="RuntimeInitializeLoadType.SubsystemRegistration"/> </summary>
	public interface ISubsystemRegistration { }

	/// <summary> We use reflection to get all types implementing this interface and initialize them on phase <see cref="RuntimeInitializeLoadType.AfterAssembliesLoaded"/> </summary>
	public interface IAfterAssembliesLoaded { }

	/// <summary>
	/// We use reflection to get all types implementing this interface and initialize them on phase <see cref="RuntimeInitializeLoadType.BeforeSplashScreen"/>
	/// <para><b>NOTE:</b> If the splash screen is turned off, functions using this load type are invoked when the splash screen would have been displayed</para> 
	/// </summary>
	public interface IBeforeSplashScreen { }

	/// <summary> We use reflection to get all types implementing this interface and initialize them on phase <see cref="RuntimeInitializeLoadType.BeforeSceneLoad"/> </summary>
	public interface IBeforeSceneLoad { }

	/// <summary> We use reflection to get all types implementing this interface and initialize them on phase <see cref="RuntimeInitializeLoadType.AfterSceneLoad"/> </summary>
	public interface IAfterSceneLoad { }

#if UNITY_EDITOR
	/// <summary>
	/// We use reflection to get all types implementing this interface and initialize them on phase <see cref="UnityEditor.InitializeOnLoadMethod"/>
	/// <para><b>NOTE: EDITOR ONLY</b></para> 
	/// </summary>
	public interface IEditorInitializeOnLoadMethod { }
#endif

	// ref https://docs.microsoft.com/it-it/dotnet/api/system.reflection.bindingflags?view=netframework-4.7.1
	static SingletonInitializer() { }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void SubsystemRegistration()
	{
		var singletons = SerializedMonoBehaviourSingletonTypes.Instance.Get(nameof(ISubsystemRegistration));
		ReflectionHelper.CallStaticMethodOnTypes(singletons, "SubsystemRegistration", IsBaseMethod: true);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void AfterAssembliesLoaded()
	{
		var singletons = SerializedMonoBehaviourSingletonTypes.Instance.Get(nameof(IAfterAssembliesLoaded));
		ReflectionHelper.CallStaticMethodOnTypes(singletons, "AfterAssembliesLoaded", IsBaseMethod: true);
	}

	//If the splash screen is turned off, functions using this load type are invoked when the splash screen would have been displayed
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	private static void BeforeSplashScreen()
	{
		var singletons = SerializedMonoBehaviourSingletonTypes.Instance.Get(nameof(IBeforeSplashScreen));
		ReflectionHelper.CallStaticMethodOnTypes(singletons, "BeforeSplashScreen", IsBaseMethod: true);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void BeforeSceneLoad()
	{
		var singletons = SerializedMonoBehaviourSingletonTypes.Instance.Get(nameof(IBeforeSceneLoad));
		ReflectionHelper.CallStaticMethodOnTypes(singletons, "BeforeSceneLoad", IsBaseMethod: true);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void AfterSceneLoad()
	{
		var singletons = SerializedMonoBehaviourSingletonTypes.Instance.Get(nameof(IAfterSceneLoad));
		ReflectionHelper.CallStaticMethodOnTypes(singletons, "AfterSceneLoad", IsBaseMethod: true);
	}

#if UNITY_EDITOR
	[UnityEditor.InitializeOnLoadMethod]
	private static void Editor_InitializeOnLoadMethod()
	{
		SerializedMonoBehaviourSingletonTypes.Instance.AddRange(nameof(ISubsystemRegistration), UnityEditor.TypeCache.GetTypesDerivedFrom<ISubsystemRegistration>().Where(t => !t.IsAbstract).ToArray());
		SerializedMonoBehaviourSingletonTypes.Instance.AddRange(nameof(IAfterAssembliesLoaded), UnityEditor.TypeCache.GetTypesDerivedFrom<IAfterAssembliesLoaded>().Where(t => !t.IsAbstract).ToArray());
		SerializedMonoBehaviourSingletonTypes.Instance.AddRange(nameof(IBeforeSplashScreen), UnityEditor.TypeCache.GetTypesDerivedFrom<IBeforeSplashScreen>().Where(t => !t.IsAbstract).ToArray());
		SerializedMonoBehaviourSingletonTypes.Instance.AddRange(nameof(IBeforeSceneLoad), UnityEditor.TypeCache.GetTypesDerivedFrom<IBeforeSceneLoad>().Where(t => !t.IsAbstract).ToArray());
		SerializedMonoBehaviourSingletonTypes.Instance.AddRange(nameof(IAfterSceneLoad), UnityEditor.TypeCache.GetTypesDerivedFrom<IAfterSceneLoad>().Where(t => !t.IsAbstract).ToArray());

		SerializedMonoBehaviourSingletonTypes.Instance.AddRange(nameof(IEditorInitializeOnLoadMethod), UnityEditor.TypeCache.GetTypesDerivedFrom<IEditorInitializeOnLoadMethod>().Where(t => !t.IsAbstract).ToArray());
		
		var singletons = SerializedMonoBehaviourSingletonTypes.Instance.Get(nameof(IEditorInitializeOnLoadMethod));
		ReflectionHelper.CallStaticMethodOnTypes(singletons, "InitializeOnLoadMethod", IsBaseMethod: true);
	}
#endif
}

/// <summary> By default initialized at <see cref="RuntimeInitializeLoadType.BeforeSceneLoad"/> </summary>
public abstract class GlobalMonoBehaviourSingleton<T> : MonoBehaviour, SingletonInitializer.IBeforeSceneLoad, SingletonInitializer.IAfterSceneLoad where T : MonoBehaviour, new()
{
	private		static			bool								m_IsInitialized			= false;
	private		static			T									m_InstanceInternal		= null;
	private		static			GlobalMonoBehaviourSingleton<T>		m_SingletonInstance		= null;

	protected	static			T									m_Instance              => m_InstanceInternal;
	protected	static			bool								ShowDebugInfo			{ get; private set; } = false;

	public		static			T									Instance
	{
		get {
			Utils.CustomAssertions.IsNotNull(m_InstanceInternal, $"{typeof(T).Name} has been not initialized correctly!!");
			return m_InstanceInternal;
		}
	}

	/*
	//////////////////////////////////////////////////////////////////////////
	private	static	void	SubsystemRegistration()
	{
		InitializeIfNeeded();
		m_SingletonInstance.OnSubsystemRegistration();
	}

	//////////////////////////////////////////////////////////////////////////
	private	static	void	AfterAssembliesLoaded()
	{
		InitializeIfNeeded();
		m_SingletonInstance.OnAfterAssembliesLoaded();
	}

	//////////////////////////////////////////////////////////////////////////
	private	static	void	BeforeSplashScreen()
	{
		InitializeIfNeeded();
		m_SingletonInstance.OnBeforeSplashScreen();
	}
	*/
	//////////////////////////////////////////////////////////////////////////
	private	static	void	BeforeSceneLoad()
	{
		InitializeIfNeeded();
		m_SingletonInstance.OnBeforeSceneLoad();
	}

	//////////////////////////////////////////////////////////////////////////
	private	static	void	AfterSceneLoad()
	{
		InitializeIfNeeded();
		m_SingletonInstance.OnAfterSceneLoad();
		// Here we re-enable the component so OnEnabled is called
		m_SingletonInstance.enabled = true;
	}

	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////
	private static bool TryInitializeSingleton(in System.Type InSingletonType, out MonoBehaviour OutInstance)
	{
		OutInstance = null;

		if (ReflectionHelper.IsInerithedFrom(typeof(MonoBehaviour), InSingletonType))
		{
			// The name of the singleton to instantiate
			string typeName = InSingletonType.Name;

			// Search in Loaded game objects
			Object[] others = Object.FindObjectsOfType(InSingletonType);
			if (others.Length > 0)
			{
				if (others.Length > 1)
				{
					UnityEngine.Debug.LogError($"Object: Found multiple instances of singleton {typeName}");
					return false;
				}

				// We save the founded one
				OutInstance = others[0] as MonoBehaviour;
			}
			else
			{
				// Search in Resources folders
				others = Resources.LoadAll(string.Empty, InSingletonType);
				if (others.Length > 0)
				{
					if (others.Length > 1)
					{
						UnityEngine.Debug.LogError($"Resources: Found multiple instances of singleton {typeName}");
						return false;
					}

					OutInstance = Instantiate(others[0] as MonoBehaviour);
				}
			}


			// If need to be instantiated
			if (!OutInstance)
			{
				OutInstance = new GameObject(typeName).AddComponent(InSingletonType) as MonoBehaviour;
			}

			DontDestroyOnLoad(OutInstance);
		}
		return Utils.CustomAssertions.IsNotNull(OutInstance);
	}

	//////////////////////////////////////////////////////////////////////////
	private	static	void	InitializeIfNeeded()
	{
		if (m_IsInitialized)
		{
			Utils.CustomAssertions.IsNotNull(m_InstanceInternal);
		}
		else
		{
			if (Utils.CustomAssertions.IsTrue(TryInitializeSingleton(typeof(T), out MonoBehaviour newInstance)))
			{
				m_IsInitialized = true;

				m_InstanceInternal = newInstance as T;

				m_SingletonInstance = (m_InstanceInternal as GlobalMonoBehaviourSingleton<T>);
				m_SingletonInstance.OnInitialize();
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Called on initialization </summary>
	protected virtual void OnInitialize() { }
	
	/*
	//////////////////////////////////////////////////////////////////////////
	/// <summary> Callback used for registration of subsystems </summary>
	protected virtual void OnSubsystemRegistration() { }


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Callback when all assemblies are loaded and preloaded assets are initialized. </summary>
	protected virtual void OnAfterAssembliesLoaded() { }


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Immediately before the splash screen is shown. </summary>
	protected virtual void OnBeforeSplashScreen() { }
	*/

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Before Scene is loaded. </summary>
	protected virtual void OnBeforeSceneLoad() { }
	
	
	//////////////////////////////////////////////////////////////////////////
	/// <summary> After Scene is loaded. </summary>
	protected virtual void OnAfterSceneLoad() { }
}

/// <summary> Sort of on-demand singleton for gameplay </summary>
public abstract class GameMonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour, new()
{
	private		static			bool								m_IsInitialized			= false;
	private		static			T									m_InstanceInternal		= null;
	private		static			GameMonoBehaviourSingleton<T>		m_SingletonInstance		= null;

	protected	static			T									m_Instance              => m_InstanceInternal;
	protected	static			bool								ShowDebugInfo			{ get; private set; } = false;

	public		static			T									Instance
	{
		get {
			InitializeIfNeeded();
			Utils.CustomAssertions.IsNotNull(m_InstanceInternal, $"{typeof(T).Name} has been not initialized correctly!!");
			return m_InstanceInternal;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private	static	void	InitializeIfNeeded()
	{
		if (m_IsInitialized)
		{
			Utils.CustomAssertions.IsNotNull(m_InstanceInternal);
		}
		else
		{
			if (Utils.CustomAssertions.IsTrue(TryInitializeSingleton(typeof(T), out MonoBehaviour newInstance)))
			{
				m_IsInitialized = true;

				m_InstanceInternal = newInstance as T;
				DontDestroyOnLoad(m_InstanceInternal);

				m_SingletonInstance = (m_InstanceInternal as GameMonoBehaviourSingleton<T>);
				m_SingletonInstance.OnInitialize();
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private static bool TryInitializeSingleton(System.Type InSingletonType, out MonoBehaviour OutInstance)
	{
		OutInstance = null;

		if (ReflectionHelper.IsInerithedFrom(typeof(MonoBehaviour), InSingletonType))
		{
			// The name of the singleton to instantiate
			string typeName = InSingletonType.Name;
			/*
			// Debug Info enable if requested by section
			if (Database.Global.TryGetSection("DebugInfos", out DatabaseCore.Section debugInfosSection))
			{
				ShowDebugInfo = debugInfosSection.AsBool(typeName);
			}
			*/
			MonoBehaviour[] others = Object.FindObjectsOfType(InSingletonType) as MonoBehaviour[];
			if (others.Length > 0)
			{
				if (others.Length > 1)
				{
					UnityEngine.Debug.LogError($"Found multiple instances of singleton {typeName}");
					return false;
				}

				// We save the founded one
				OutInstance = others[0];
			}

			// If need to be instantiated
			if (!OutInstance)
			{
				OutInstance = new GameObject(typeName).AddComponent(InSingletonType) as MonoBehaviour;
				/*
				string resourcePath = $"{SINGLETONS_FOLDER_PATH}{typeName}";
				MonoBehaviour source = Resources.Load<MonoBehaviour>(resourcePath);
				if (Utils.CustomAssertions.IsNotNull(source, $"Cannot load Singleton at {resourcePath}"))
				{
					bool prevState = source.enabled;
					source.enabled = false; // This prevent OnEnabled called just after the awake
					instance = Instantiate(source);
					instance.name = instance.name.Replace("Clone", "Singleton");
					source.enabled = prevState;
				}
				else
				{
					UnityEngine.Debug.LogError($"Instantiation of singleton {typeName} failed");
					return false;
				}
				*/
			}
		}
		return Utils.CustomAssertions.IsNotNull(OutInstance);
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Called on initialization </summary>
	protected virtual void OnInitialize() { }
}
