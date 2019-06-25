
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

	public class LoadData<T> {
		public	T Asset;
	}

	private	static	ResourceManager				m_Instance			= null;

	private	static	bool						m_IsInitialized		= false;

	private	static	bool						m_ShowDebugInfo		= false;

	private	static	List<string>				m_CurrentLoading	= new List<string>();

	private	static	Dictionary<string, Texture>	m_Textures			= new Dictionary<string, Texture>();

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
		}
	}

	
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

		LoadData<Texture> loadData = new LoadData<Texture>();
		System.Action<Texture> onTextureLoaded = delegate( Texture t )
		{
			m_Textures.Add( resourcePath, t );
			m_CurrentLoading.Remove( resourcePath );
		};
		LoadResourceAsync( resourcePath, loadData, onTextureLoaded );
	}

	public	static	Texture	LoadTextureSync( string resourcePath )
	{
		Object obj = Resources.Load( resourcePath );
		return obj as Texture;
	}

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
	public	static	bool	LoadResourceSync<T>( string ResourcePath, LoadData<T> loadData ) where T : Object
	{
		Initialize();

		bool result = true;

		if ( m_ShowDebugInfo )				print( "SYNC Loading: " + ResourcePath );
		{
			result &= InternalLoadResourceSync( ResourcePath, loadData, ref result );
		}
		if ( m_ShowDebugInfo && result )	print( "SYNC Loaded: " + ResourcePath );

		return result;
	}


	/////////////////////////////////////////////////////////////////
	private	static  bool	InternalLoadResourceSync<T>( string ResourcePath, LoadData<T> loadData, ref bool bResult ) where T : Object
	{
		if ( bResult == true )
		{
			if ( m_ShowDebugInfo )				print( "INTERNAL SYNC Loading: " + ResourcePath );
			{
				System.Diagnostics.Stopwatch m_StopWatch = new System.Diagnostics.Stopwatch();
				m_StopWatch.Start();
				{
					loadData.Asset = Resources.Load<T>( ResourcePath );
				}
				m_StopWatch.Stop();

				if ( m_ShowDebugInfo )				print( "INTERNAL SYNC Loaded: " + ResourcePath + " in " + m_StopWatch.Elapsed.Milliseconds + "ms" );

				bResult = loadData.Asset.IsNotNull();

				if ( loadData.Asset is IResourceComposite )
				{
					IResourceComposite composite = loadData.Asset as IResourceComposite;
					if ( composite.NeedToBeLoaded() == true )
					{
						composite.Reinit();
						foreach( string childPath in composite.GetChildPaths() )
						{
							LoadData<Object> childLoadData = new LoadData<Object>();
							bResult = InternalLoadResourceSync( childPath, childLoadData, ref bResult );
							if ( bResult )
							{
								composite.AddChild( childLoadData.Asset );
							}
						}
					}
				}
			}
			if ( m_ShowDebugInfo && bResult )	print( "INTERNAL SYNC Loaded: " + ResourcePath );
		}

		return bResult;
	}



	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////		ASYNC		/////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////

	/// <summary> Asynchronously load resource with given path, load recursively if composite type. Onload completed with success callbeck is called </summary>
	public static void LoadResourceAsync<T>( string ResourcePath, LoadData<T> loadData, System.Action<T> OnResourceLoaded ) where T : Object
	{
		Initialize();

		m_Instance.StartCoroutine( LoadResourceAsyncCoroutine( ResourcePath, loadData, OnResourceLoaded ) );
	}


	/////////////////////////////////////////////////////////////////
	/// <summary> Asynchronously load resource with given path, load recursively if composite type. Onload completed with success callbeck is called
	/// return iterator of the MAIN load coroutine </summary>

	public	static  	IEnumerator LoadResourceAsyncCoroutine<T>( string ResourcePath, LoadData<T> loadData, System.Action<T> OnResourceLoaded, System.Action<string> OnFailure = null) where T : Object
	{
		Initialize();

		if ( loadData == null )
		{
			loadData = new LoadData<T>();
		}

		if ( m_ShowDebugInfo )				print( "COROUTINE ASYNC Loading: " + ResourcePath );
		{
			yield return m_Instance.StartCoroutine( InternalLoadResourceAsync( ResourcePath, loadData ) );
		}
		if ( m_ShowDebugInfo )				print( "COROUTINE ASYNC Loaded: " + ResourcePath );

		bool bHasValidAsset = loadData.Asset != null;
		if ( bHasValidAsset == false )
		{
			Debug.LogError( "ResourceManager:Cannot load resource " + ResourcePath );
			if ( OnFailure.IsNotNull() )
			{
				OnFailure( ResourcePath );
			}
		}

		if ( OnResourceLoaded.IsNotNull() && bHasValidAsset == true )
		{
			OnResourceLoaded( loadData.Asset as T );
		}
	
	}

	
	/////////////////////////////////////////////////////////////////
	private	static  IEnumerator InternalLoadResourceAsync<T>( string ResourcePath, LoadData<T> loadData ) where T : Object
	{
		if ( m_ShowDebugInfo )			print( "INTERNAL ASYNC Loading: " + ResourcePath );
		{
			System.Diagnostics.Stopwatch m_StopWatch = new System.Diagnostics.Stopwatch();
			m_StopWatch.Start();
			{
				ResourceRequest request = Resources.LoadAsync( ResourcePath );
				yield return request;
				loadData.Asset = request.asset as T;
			}
			m_StopWatch.Stop();

			if ( m_ShowDebugInfo )				print( "INTERNAL ASYNC Loaded: " + ResourcePath + " in " + m_StopWatch.Elapsed.Milliseconds + "ms" );
			m_StopWatch.Reset();

			if ( loadData.Asset is IResourceComposite )
			{
				IResourceComposite composite = loadData.Asset as IResourceComposite;
				if ( composite.NeedToBeLoaded() == true )
				{
					composite.Reinit();
					foreach( string childPath in composite.GetChildPaths() )
					{
						LoadData<Object> childLoadData = new LoadData<Object>();
						yield return m_Instance.StartCoroutine( InternalLoadResourceAsync( childPath, childLoadData ) );
						composite.AddChild( childLoadData.Asset );
					}
				}
			}
		}
		if ( m_ShowDebugInfo )			print( "INTERNAL ASYNC Loaded: " + ResourcePath );
	}


}