
using UnityEngine;

internal class SingletonInitializer
{
	/// <summary> The implementation of this interface ensure the calls from unity engine of static methods decorated by RuntimeInitializeOnLoadMethod attribute </summary>
	public interface IRuntimeInitializeLoad {}

	// ref https://docs.microsoft.com/it-it/dotnet/api/system.reflection.bindingflags?view=netframework-4.7.1
	static SingletonInitializer() { }

	/** We use reflection to get all types including interface 'IRuntimeInitializeLoad' and initialize them on phase 'SubsystemRegistration' */
//	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
//	private static void SubsystemRegistration()
//	{
//		var singletons = ReflectionHelper.FindInerithedFromInterface<IRuntimeInitializeLoad>(bInlcludeAbstracts: false);
//		ReflectionHelper.CallMethodOnTypes(singletons, "SubsystemRegistration", IsBaseMethod: true);
//	}
//
//	/** We use reflection to get all types including interface 'IRuntimeInitializeLoad' and initialize them on phase 'AfterAssembliesLoaded' */
//	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
//	private static void AfterAssembliesLoaded()
//	{
//		var singletons = ReflectionHelper.FindInerithedFromInterface<IRuntimeInitializeLoad>(bInlcludeAbstracts: false);
//		ReflectionHelper.CallMethodOnTypes(singletons, "AfterAssembliesLoaded", IsBaseMethod: true);
//	}
//
//	/** We use reflection to get all types including interface 'IRuntimeInitializeLoad' and initialize them on phase 'BeforeSplashScreen'
//	 * If the splash screen is turned off, functions using this load type are invoked when the splash screen would have been displayed
//	 */
//	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
//	private static void BeforeSplashScreen()
//	{
//		var singletons = ReflectionHelper.FindInerithedFromInterface<IRuntimeInitializeLoad>(bInlcludeAbstracts: false);
//		ReflectionHelper.CallMethodOnTypes(singletons, "BeforeSplashScreen", IsBaseMethod: true);
//	}

	/** We use reflection to get all types including interface 'IRuntimeInitializeLoad' and initialize them on phase 'BeforeSceneLoad' */
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void BeforeSceneLoad()
	{
		var singletons = ReflectionHelper.FindInerithedFromInterface<IRuntimeInitializeLoad>(bInlcludeAbstracts: false);
		ReflectionHelper.CallMethodOnTypes(singletons, "BeforeSceneLoad", IsBaseMethod: true);
	}

	/** We use reflection to get all types including interface 'IRuntimeInitializeLoad' and initialize them on phase 'AfterSceneLoad' */
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void AfterSceneLoad()
	{
		var singletons = ReflectionHelper.FindInerithedFromInterface<IRuntimeInitializeLoad>(bInlcludeAbstracts: false);
		ReflectionHelper.CallMethodOnTypes(singletons, "AfterSceneLoad", IsBaseMethod: true);
	}
}

public interface ISingleton { }
/*
public interface IOnDemandSingleton { }
*/
public interface IDeclaredSingleton { }

public abstract class MonoBehaviourSingleton<T> : MonoBehaviour, ISingleton, SingletonInitializer.IRuntimeInitializeLoad where T : MonoBehaviour, new()
{
	private		const			string							SINGLETONS_FOLDER_PATH	= "Prefabs/Essentials/Singletons_Global/";
	private		static			bool							m_IsInitialized			= false;
	private		static			T								m_InstanceInternal		= null;
	private		static			MonoBehaviourSingleton<T>		m_SingletonInstance		= null;

	protected	static			T								m_Instance              => m_InstanceInternal;
	protected	static			bool							ShowDebugInfo			{ get; private set; } = false;

	public		static			T								Instance
	{
		get {
			CustomAssertions.IsNotNull(m_InstanceInternal, $"{typeof(T).Name} has been not initialized correctly!!");
			return m_InstanceInternal;
		}
	}
	/*
	//////////////////////////////////////////////////////////////////////////
	private	static	void	SubsystemRegistration()
	{
		InitializeIfNeeded();
	//	m_SingletonInstance.OnSubsystemRegistration();
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
	public static bool TryInitializeSingleton<K>(out K instance) where K : MonoBehaviour, new()
	{
		// The name of the singleton to instantiate
		string typeName = typeof(K).Name;
		instance = null;

		//UnityEngine.Debug.Log($"Creation of Singleton {typeName}");

		// Debug Info enable if requested by section
		if (GlobalManager.Configs.TryGetSection("DebugInfos", out Database.Section debugInfosSection))
		{
			ShowDebugInfo = debugInfosSection.AsBool(typeName);
		}

		K[] others = Object.FindObjectsOfType<K>();
		if (others.Length > 0)
		{
			if (others.Length > 1)
			{
				UnityEngine.Debug.LogError($"Found multiple instances of singleton {typeName}");
				return false;
			}

			// We save the founded one
			instance = others[0];
		}

		// If need to be instantiated
		if (!instance)
		{
			string resourcePath = $"{SINGLETONS_FOLDER_PATH}{typeName}";
			K source = Resources.Load<K>(resourcePath);
			if (CustomAssertions.IsNotNull(source, $"Cannot load Singleton at {resourcePath}"))
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
		}

		CustomAssertions.IsNotNull(instance);
		DontDestroyOnLoad(instance);
		return true;
	}

	//////////////////////////////////////////////////////////////////////////
	private	static	void	InitializeIfNeeded()
	{
		if (m_IsInitialized)
		{
			CustomAssertions.IsNotNull(m_InstanceInternal);
			return;
		}
		m_IsInitialized = true;

		CustomAssertions.IsTrue(TryInitializeSingleton(out m_InstanceInternal));

		m_SingletonInstance = (m_InstanceInternal as MonoBehaviourSingleton<T>);
		m_SingletonInstance.OnInitialize();
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

public abstract class OnDemandSingleton<T> : MonoBehaviour, IDeclaredSingleton where T : MonoBehaviour, new()
{
	private		const			string							SINGLETONS_FOLDER_PATH	= "Prefabs/Essentials/Singletons_InGame/";
	private		static			bool							m_IsInitialized			= false;
	private		static			bool							m_IsShuttingDown		= false;
	protected	static			T								m_Instance				{ get; private set; }

	public		static			bool							HasInstance				=> m_IsInitialized;
	public		static			T								Instance
	{
		get
		{
			// Singleton initialization forbidden on application quit
			//if (GlobalManager.IsQuittings)
			if (m_IsShuttingDown)
			{
				return null;
			}

			if (!m_IsInitialized)
			{
				m_IsInitialized = true;
				Initialize();
			}

			CustomAssertions.IsNotNull(m_Instance, $"{typeof(T).Name} has been not initialized correctly!!");

			return m_Instance as T;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private static void Initialize()
	{
		string typeName = typeof(T).Name;

		T[] others = Object.FindObjectsOfType<T>();
		if (others.Length > 0)
		{
			if (others.Length > 1)
			{
				UnityEngine.Debug.LogError($"Found multiple instances of singleton {typeName}");
				return;
			}

			// We use the founded one
			m_Instance = others[0];
		}

		// If need to be instantiated
		if (!m_Instance)
		{
			string resourcePath = $"{SINGLETONS_FOLDER_PATH}{typeName}";
			T source = Resources.Load<T>(resourcePath);
			if (source)
			{
				bool prevState = source.enabled;
				source.enabled = false; // This prevent OnEnabled called just after the awake
				m_Instance = Instantiate(source);
				m_Instance.name = m_Instance.name.Replace("Clone", "OnDemandSingleton");
				source.enabled = prevState;

				m_Instance.enabled = prevState;
			}
			else
			{
				m_Instance = new GameObject($"{typeName}_GeneratedOnDemandSingleton").AddComponent<T>();
				UnityEngine.Debug.Log($"Singleton {typeName} has been generated");
			}
		}
		CustomAssertions.IsNotNull(m_Instance);
		DontDestroyOnLoad(m_Instance);
	}

	protected virtual void Awake()
	{
		if (m_Instance && (object)m_Instance != this)
		{
			Destroy(gameObject);
			return;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnDestroy()
	{
		if ((object)m_Instance != this)
		{
			return;
		}

		m_Instance = null;
		m_IsInitialized = false;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnApplicationQuit()
	{
		m_IsShuttingDown = true;
	}
}



/*
public abstract class OnDemandSingleton<T>: MonoBehaviour, IOnDemandSingleton where T : MonoBehaviour, new()
{
	private		const			string							SINGLETONS_FOLDER_PATH	= "Prefabs/Essentials/Singletons_InGame/";
	private		static			bool							m_IsInitialized			= false;
	private		static			bool							m_IsShuttingDown		= false;
	protected	static			T								m_Instance				{ get; private set; }

	protected	static			bool							ShowDebugInfo			{ get; private set; } = false;
	protected	static			bool							IsDestroying			{ get; private set; } = false;

	public		static			T								Instance
	{
		get {
			// Singleton initialization forbidden on application quit
		//	if (GlobalManager.IsQuittings)
			if (m_IsShuttingDown)
			{
				return null;
			}

			if (!m_IsInitialized)
			{
				Initialize();
				m_IsInitialized = true;
			}

			CustomAssertions.IsNotNull(m_Instance, $"{typeof(T).Name} has been not initialized correctly!!");

			return m_Instance as T;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private static		void	Initialize()
	{	
		// The name of the singleton to instantiate
		string typeName = typeof(T).Name;

		UnityEngine.Debug.Log($"Creation of InGame Singleton {typeName}");

		// Debug Info enable if requested by section
		if (GlobalManager.Configs.TryGetSection("DebugInfos", out Database.Section debugInfosSection))
		{
			ShowDebugInfo = debugInfosSection.AsBool(typeName);
		}

		var others = Object.FindObjectsOfType<T>();
		if (others.Length > 0)
		{
			if (others.Length > 1)
			{
				UnityEngine.Debug.LogError($"Found multiple instances of singleton {typeName}");
				return;
			}

			// We save the founded one
			m_Instance = others[0];
		}

		// If need to be instantiated
		if (!m_Instance)
		{
			string resourcePath = $"{SINGLETONS_FOLDER_PATH}{typeName}";
			var source = Resources.Load<T>(resourcePath);
			CustomAssertions.IsNotNull(source, $"Cannot load Singleton at {resourcePath}");
			if (source)
			{
				m_Instance = Instantiate(source);
				m_Instance.name = Instance.name.Replace("Clone", "OnDemandSingleton");
			}
			else
			{
				UnityEngine.Debug.LogError($"Instantiation of singleton {typeName} failed");
				return;
			}
		}

		DontDestroyOnLoad(m_Instance);
		(m_Instance as OnDemandSingleton<T>).OnInitialize();
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Called on initialization </summary>
	protected virtual	void	OnInitialize() { }


	//////////////////////////////////////////////////////////////////////////
	protected virtual	void	Awake()
	{
		// SINGLETON
	//	if (m_Instance.IsNotNull())
		{
	//		Destroy(gameObject);
			return;
		}
	}
	

	//////////////////////////////////////////////////////////////////////////
	protected virtual	void	OnDestroy()
	{
		// Avoid the destruction of the singleton instance reference
		if (m_Instance != this)
		{
			return;
		}

		IsDestroying = true;
		m_IsInitialized = false;
		m_Instance = null;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnApplicationQuit()
	{
		m_IsShuttingDown = true;
	}
}
*/
  