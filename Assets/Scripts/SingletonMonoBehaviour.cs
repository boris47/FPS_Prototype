using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


internal class SingletonInitializer : MonoBehaviour {

	public interface ITopSingleton {}

	// ref https://docs.microsoft.com/it-it/dotnet/api/system.reflection.bindingflags?view=netframework-4.7.1

	/**
	 * We use reflection to get all types including interface 'ITopSingleton' and initialize them on phase 'SubsystemRegistration'
	 */
	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.SubsystemRegistration)] // Callback used for registration of subsystems
	protected	static	void	SubsystemRegistration()
	{
		IList<System.Type> singletons = ReflectionHelper.FindInerithed<ITopSingleton>();
		foreach( System.Type singleton in singletons )
		{
			MethodInfo initializeMethod = singleton.BaseType.GetMethod( "SubsystemRegistration", BindingFlags.NonPublic | BindingFlags.Static );
			if ( initializeMethod != null )
			{
				initializeMethod.Invoke( null, null );
			}
		}
	}

	/**
	 * We use reflection to get all types including interface 'ITopSingleton' and initialize them on phase 'AfterAssembliesLoaded'
	 */
	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.AfterAssembliesLoaded)] // Callback when all assemblies are loaded and preloaded assets are initialized.
	protected	static	void	AfterAssembliesLoaded()
	{
		IList<System.Type> singletons = ReflectionHelper.FindInerithed<ITopSingleton>();
		foreach( System.Type singleton in singletons )
		{
			MethodInfo initializeMethod = singleton.BaseType.GetMethod( "AfterAssembliesLoaded", BindingFlags.NonPublic | BindingFlags.Static );
			if ( initializeMethod != null )
			{
				initializeMethod.Invoke( null, null );
			}
		}
	}

	/**
	 * We use reflection to get all types including interface 'ITopSingleton' and initialize them on phase 'BeforeSplashScreen'
	 * If the splash screen is turned off, functions using this load type are invoked when the splash screen would have been displayed
	 */
	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSplashScreen)] // Immediately before the splash screen is shown.
	protected	static	void	BeforeSplashScreen()
	{
		IList<System.Type> singletons = ReflectionHelper.FindInerithed<ITopSingleton>();
		foreach( System.Type singleton in singletons )
		{
			MethodInfo initializeMethod = singleton.BaseType.GetMethod( "BeforeSplashScreen", BindingFlags.NonPublic | BindingFlags.Static );
			if ( initializeMethod != null )
			{
				initializeMethod.Invoke( null, null );
			}
		}
	}


	/**
	 * We use reflection to get all types including interface 'ITopSingleton' and initialize them on phase 'BeforeSceneLoad'
	 */
	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)] // Before Scene is loaded.
	protected	static	void	BeforeSceneLoad()
	{
		IList<System.Type> singletons = ReflectionHelper.FindInerithed<ITopSingleton>();
		foreach( System.Type singleton in singletons )
		{
			MethodInfo initializeMethod = singleton.BaseType.GetMethod( "BeforeSceneLoad", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.ExactBinding );
			if ( initializeMethod != null )
			{
				initializeMethod.Invoke( null, null );
			}
		}
	}
}



public abstract class SingletonMonoBehaviour<T> : MonoBehaviour, SingletonInitializer.ITopSingleton where T : MonoBehaviour {

	private		static			SingletonMonoBehaviour<T>				m_Instance				= null;
	public		static			T										Instance
	{
		get {
			UnityEngine.Assertions.Assert.IsNotNull
			(
				m_Instance,
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
	private static void BeforeSceneLoad()
	{
		if ( m_IsInitialized == false )
		{
			m_Instance = FindObjectOfType<SingletonMonoBehaviour<T>>();
			if ( m_Instance == null )
			{
				m_Instance = new GameObject( typeof(T).Name ).AddComponent<T>() as SingletonMonoBehaviour<T>;
			}
//			m_Instance.hideFlags = HideFlags.DontSave;
			m_IsInitialized = true;

			DontDestroyOnLoad( m_Instance );

			Database.Section debugInfosSection = null;
			if ( GlobalManager.Configs.bGetSection( "DebugInfos", ref debugInfosSection ) )
			{
				ShowDebugInfo = debugInfosSection.AsBool( typeof(T).Name, false );
			}

			m_Instance.OnBeforeSceneLoad();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnBeforeSceneLoad()
	{ }


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnDestroy()
	{
		if ( m_Instance != this )
			return;

		m_IsInitialized = false;
		m_Instance = null;
	}

}