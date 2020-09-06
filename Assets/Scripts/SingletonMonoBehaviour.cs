using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


internal class SingletonInitializer : MonoBehaviour {

	public interface ITopSingleton {}

	// ref https://docs.microsoft.com/it-it/dotnet/api/system.reflection.bindingflags?view=netframework-4.7.1

	/** We use reflection to get all types including interface 'ITopSingleton' and initialize them on phase 'SubsystemRegistration' */
	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.SubsystemRegistration)]
	protected	static	void	SubsystemRegistration()
	{
		List<System.Type> singletons = ReflectionHelper.FindInerithedFromInterface<ITopSingleton>(bInlcludeAbstracts:false);
		ReflectionHelper.CallMethodOnTypes( singletons, "SubsystemRegistration", IsBaseMethod: true );
	}

	/** We use reflection to get all types including interface 'ITopSingleton' and initialize them on phase 'AfterAssembliesLoaded' */
	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	protected	static	void	AfterAssembliesLoaded()
	{
		List<System.Type> singletons = ReflectionHelper.FindInerithedFromInterface<ITopSingleton>(bInlcludeAbstracts: false);
		ReflectionHelper.CallMethodOnTypes( singletons, "AfterAssembliesLoaded", IsBaseMethod: true );
	}

	/** We use reflection to get all types including interface 'ITopSingleton' and initialize them on phase 'BeforeSplashScreen'
	 * If the splash screen is turned off, functions using this load type are invoked when the splash screen would have been displayed
	 */
	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSplashScreen)]
	protected	static	void	BeforeSplashScreen()
	{
		List<System.Type> singletons = ReflectionHelper.FindInerithedFromInterface<ITopSingleton>(bInlcludeAbstracts: false);
		ReflectionHelper.CallMethodOnTypes( singletons, "BeforeSplashScreen", IsBaseMethod: true );
	}


	/** We use reflection to get all types including interface 'ITopSingleton' and initialize them on phase 'BeforeSceneLoad' */
	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
	protected	static	void	BeforeSceneLoad()
	{
		List<System.Type> singletons = ReflectionHelper.FindInerithedFromInterface<ITopSingleton>(bInlcludeAbstracts: false);
		ReflectionHelper.CallMethodOnTypes( singletons, "BeforeSceneLoad", IsBaseMethod: true );
	}
}



public abstract class SingletonMonoBehaviour<T> : MonoBehaviour, SingletonInitializer.ITopSingleton where T : MonoBehaviour {

	private		static			SingletonMonoBehaviour<T>				m_Instance				= null;
	public		static			T										Instance
	{
		get {
			UnityEngine.Assertions.Assert.IsTrue
			(
				!!m_Instance || !m_IsInitialized,
				typeof(T).Name + " has been not initialized correctly!!"
			);
			return m_Instance as T;
		}
	}

	private		static			bool			m_IsInitialized			= false;

	protected	static			bool			ShowDebugInfo
	{
		get; private set;
	}


	//////////////////////////////////////////////////////////////////////////
	private	static	void	Initialize()
	{
		if ( m_IsInitialized == false )
		{
			m_Instance = FindObjectOfType<SingletonMonoBehaviour<T>>();
			if ( m_Instance == null )
			{
				m_Instance = new GameObject( typeof(T).Name ).AddComponent<T>() as SingletonMonoBehaviour<T>;
				print( "Singleton created " + typeof(T).Name );
			}
//			m_Instance.hideFlags = HideFlags.DontSave;
			m_IsInitialized = true;

			DontDestroyOnLoad( m_Instance );

			Database.Section debugInfosSection = null;
			if ( GlobalManager.Configs.GetSection( "DebugInfos", ref debugInfosSection ) )
			{
				ShowDebugInfo = debugInfosSection.AsBool( typeof(T).Name, false );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Called by unity in type SingletonInitializer
	private	static	void	SubsystemRegistration()
	{
		Initialize();

		m_Instance.OnSubsystemRegistration();
	}


	//////////////////////////////////////////////////////////////////////////
	// Called by unity in type SingletonInitializer
	private	static	void	AfterAssembliesLoaded()
	{
		Initialize();

		m_Instance.OnAfterAssembliesLoaded();
	}


	//////////////////////////////////////////////////////////////////////////
	// Called by unity in type SingletonInitializer
	private	static	void	BeforeSplashScreen()
	{
		Initialize();

		m_Instance.OnBeforeSplashScreen();
	}

	
	//////////////////////////////////////////////////////////////////////////
	// Called by unity in type SingletonInitializer
	private	static	void	BeforeSceneLoad()
	{
		Initialize();

		m_Instance.OnBeforeSceneLoad();
	}


	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////

		
	//////////////////////////////////////////////////////////////////////////
	/// <summary> Callback used for registration of subsystems </summary>
	protected virtual	void	OnSubsystemRegistration() {}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Callback when all assemblies are loaded and preloaded assets are initialized. </summary>
	protected virtual	void	OnAfterAssembliesLoaded() {}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Immediately before the splash screen is shown. </summary>
	protected virtual	void	OnBeforeSplashScreen() { }


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Before Scene is loaded. </summary>
	protected virtual	void	OnBeforeSceneLoad() { }


	//////////////////////////////////////////////////////////////////////////
	protected virtual	void	OnDestroy()
	{
		if ( m_Instance != this )
			return;

		m_IsInitialized = false;
		m_Instance = null;
	}

}