
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public interface IResourceComposite {

	bool		NeedToBeLoaded();
	void		Reinit();
	string[]	GetChildPaths();
	void		AddChild( Object child );
}

public class ResourceManager : MonoBehaviour {

	/// <summary>
	/// Class that contains the loaded asset, if load succeded
	/// </summary>
	/// <typeparam name="T">Asset type</typeparam>
	public class LoadedData<T> {
		public	T Asset;
	}

	private	static		ResourceManager				m_Instance			= null;

	private	static		bool						m_IsInitialized		= false;

	private	static	new	System.Action<string>		print				= delegate { };

	private	static		bool						m_ShowDebugInfo		= false;

	private	static		List<string>				m_CurrentLoading	= new List<string>();

	private	static		Dictionary<string, Texture>	m_Textures			= new Dictionary<string, Texture>();

	// 
	private void Awake()
	{	
		// Singleton
		if ( m_Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		DontDestroyOnLoad( this );
		m_Instance = this;
		m_IsInitialized = true;

		Database.Section debugInfosSection = null;
		if ( GlobalManager.Configs.bGetSection( "DebugInfos", ref debugInfosSection ) )
		{
			m_ShowDebugInfo = debugInfosSection.AsBool( "ResourceManager", false);
			if ( m_ShowDebugInfo )
			{
				print = Debug.Log;
			}
		}
	}

	
	///////////////////////////////////////////////////
	public	static	void	LoadTextureAsync( string resourcePath )
	{
		Texture tex = null;
		if ( m_Textures.TryGetValue( resourcePath, out tex ) == true )
		{
			Debug.LogError( "ResourceManager: Trying to load and already loaded texture:!!" + resourcePath );
			return; // Already loaded
		}

		if ( m_CurrentLoading.Contains( resourcePath ) == true )
		{
			return; // already loading, skip
		}

		m_CurrentLoading.Add( resourcePath );

		LoadedData<Texture> loadedResource = new LoadedData<Texture>();
		System.Action<Texture> onTextureLoaded = delegate( Texture t )
		{
			m_Textures.Add( resourcePath, t );
			m_CurrentLoading.Remove( resourcePath );
		};
		LoadResourceAsync( resourcePath, loadedResource, onTextureLoaded );
	}

	///////////////////////////////////////////////////
	public	static	Texture	LoadTextureSync( string resourcePath )
	{
		Object obj = Resources.Load( resourcePath );
		return obj as Texture;
	}

	///////////////////////////////////////////////////
	public	static	bool	RequireTexture( string resourcePath, Texture tex )
	{
		return m_Textures.TryGetValue( resourcePath, out tex );
	}



	///////////////////////////////////////////////////
	private	static	void	Initialize()
	{
		if ( m_IsInitialized == false )
		{
			GameObject resourceManagerGO = new GameObject();
			resourceManagerGO.hideFlags = HideFlags.HideAndDontSave;
			resourceManagerGO.AddComponent<ResourceManager>();

			m_IsInitialized = true;
		}
	}



	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////		SYNC		/////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////

	/// <summary> Synchronously load resource with given path, load recursively if composite type </summary>
	public	static	bool	LoadResourceSync<T>( string ResourcePath, LoadedData<T> loadedResource ) where T : Object
	{
		Initialize();

		bool result = true;

		print( "SYNC Loading: " + ResourcePath );
		
		result &= InternalLoadResourceSync( ResourcePath, loadedResource, ref result );
		
		print( "SYNC Loaded: " + ResourcePath );

		return result;
	}


	/////////////////////////////////////////////////////////////////
	private	static  bool	InternalLoadResourceSync<T>( string ResourcePath, LoadedData<T> loadedResource, ref bool bResult ) where T : Object
	{
		if ( bResult == false )
		{
			return bResult;
		}

		System.Diagnostics.Stopwatch m_StopWatch = null;
		print( "INTERNAL SYNC Loading: " + ResourcePath );	
		if ( m_ShowDebugInfo )
		{
			m_StopWatch = new System.Diagnostics.Stopwatch();
			m_StopWatch.Start();
		}
			
		loadedResource.Asset = Resources.Load<T>( ResourcePath );

		if ( m_ShowDebugInfo )
		{
			m_StopWatch.Stop();
			print( "INTERNAL SYNC Loaded: " + ResourcePath + " in " + m_StopWatch.Elapsed.Milliseconds + "ms" );
			m_StopWatch.Reset();
		}

		bResult = loadedResource.Asset.IsNotNull();

		// Coposite parse
		if ( loadedResource.Asset is IResourceComposite )
		{
			IResourceComposite composite = loadedResource.Asset as IResourceComposite;
			if ( composite.NeedToBeLoaded() == true )
			{
				composite.Reinit();
				foreach( string childPath in composite.GetChildPaths() )
				{
					LoadedData<Object> childloadedResource = new LoadedData<Object>();
					if ( bResult = InternalLoadResourceSync( childPath, childloadedResource, ref bResult ) )
					{
						composite.AddChild( childloadedResource.Asset );
					}
				}
			}
		}

		if ( bResult )
		{
			print( "INTERNAL SYNC Loaded: " + ResourcePath );
		}

		return bResult;
	}



	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////		ASYNC		/////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////

	/// <summary> Asynchronously load resource with given path, load recursively if composite type. Onload completed with success callbeck is called </summary>
	public static void LoadResourceAsync<T>( string ResourcePath, LoadedData<T> loadedResource, System.Action<T> OnResourceLoaded ) where T : Object
	{
		Initialize();

		CoroutinesManager.Start( LoadResourceAsyncCoroutine( ResourcePath, loadedResource, OnResourceLoaded ),
			"ResourceManger::LoadResourceAsync: Loading " + ResourcePath );
	}


	/////////////////////////////////////////////////////////////////
	/// <summary> Asynchronously load resource with given path, load recursively if composite type. Onload completed with success callbeck is called
	/// return iterator of the MAIN load coroutine </summary>

	public	static  	IEnumerator LoadResourceAsyncCoroutine<T>( string ResourcePath, LoadedData<T> loadedData, System.Action<T> OnResourceLoaded, System.Action<string> OnFailure = null) where T : Object
	{
		Initialize();

		if ( loadedData == null )
		{
			loadedData = new LoadedData<T>();
		}

		print( "COROUTINE ASYNC Loading: " + ResourcePath );
		
		yield return CoroutinesManager.Start( InternalLoadResourceAsync( ResourcePath, loadedData ),
			"ResourceManger::LoadResourceAsyncCoroutine: Loading " + ResourcePath );
		
		print( "COROUTINE ASYNC Loaded: " + ResourcePath );

		bool bHasValidAsset = loadedData.Asset != null;
		if ( bHasValidAsset == false )
		{
			Debug.LogError( "ResourceManager::LoadResourceAsyncCoroutine: Cannot load resource " + ResourcePath );
			if ( OnFailure.IsNotNull() )
			{
				OnFailure( ResourcePath );
			}
		}

		if ( OnResourceLoaded.IsNotNull() && bHasValidAsset == true )
		{
			OnResourceLoaded( loadedData.Asset as T );
		}
	
	}

	
	/////////////////////////////////////////////////////////////////
	private	static  IEnumerator InternalLoadResourceAsync<T>( string ResourcePath, LoadedData<T> loadedResource ) where T : Object
	{
		System.Diagnostics.Stopwatch m_StopWatch = null;
		print( "INTERNAL ASYNC Loading: " + ResourcePath );
		if ( m_ShowDebugInfo )
		{
			m_StopWatch = new System.Diagnostics.Stopwatch();
			m_StopWatch.Start();
		}
		
		ResourceRequest request = Resources.LoadAsync( ResourcePath );
		yield return request;
		loadedResource.Asset = request.asset as T;

		if ( m_ShowDebugInfo )
		{
			m_StopWatch.Stop();
			print( "INTERNAL ASYNC Loaded: " + ResourcePath + " in " + m_StopWatch.Elapsed.Milliseconds + "ms" );
			m_StopWatch.Reset();
		}

		// Composite parse
		if ( loadedResource.Asset is IResourceComposite )
		{
			IResourceComposite composite = loadedResource.Asset as IResourceComposite;
			if ( composite.NeedToBeLoaded() == true )
			{
				composite.Reinit();
				foreach( string childPath in composite.GetChildPaths() )
				{
					LoadedData<Object> childloadedResource = new LoadedData<Object>();
					yield return CoroutinesManager.Start( InternalLoadResourceAsync( childPath, childloadedResource ), 
						"ResourceManger::InternalLoadResourceAsync: Loading " + childPath
					);
					composite.AddChild( childloadedResource.Asset );
				}
			}
		}

		print( "INTERNAL ASYNC Loaded: " + ResourcePath );
	}


}