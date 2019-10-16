
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public interface IResourceComposite {

	bool		NeedToBeLoaded();
	void		Reinit();
	string[]	GetChildPaths();
	void		AddChild( Object child );
}

public partial class ResourceManager : SingletonMonoBehaviour<ResourceManager> {

	public enum AsyncLoadStrategy {
		CONTINUOS,
		PAUSED
	};

	/// <summary> Class that contains the loaded asset, if load succeded </summary>
	public class LoadedData<T> {
		public		T					Asset			= default(T);
	//	public		AsynLoadStrategy	Strategy		= AsyncLoadStrategy.CONTINUOS;
	//	public		float				TimeToWait		= 1.0f;
	}

//	private	static		ResourceManager				m_Instance			= null;

//	private	static		bool						m_IsInitialized		= false;

	private	static	new	System.Action<string>		print				= delegate { };

	/*
	///////////////////////////////////////////////////
	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
	private	static	void	Initialize()
	{
		if ( m_IsInitialized == false )
		{
			m_Instance = FindObjectOfType<ResourceManager>();
			if ( m_Instance == null )
			{
				m_Instance = new GameObject("ResourceManager").AddComponent<ResourceManager>();
			}
			m_Instance.hideFlags = HideFlags.DontSave;
			m_IsInitialized = true;

			DontDestroyOnLoad( m_Instance );
		}
	}
	*/


	///////////////////////////////////////////////////
	protected override void OnFirstGetCall()
	{}


	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////		SYNC		/////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////

	/// <summary> Synchronously load resource with given path, load recursively if composite type </summary>
	public	static	bool	LoadResourceSync<T>( string ResourcePath, LoadedData<T> loadedResource ) where T : Object
	{
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
		CoroutinesManager.Start( LoadResourceAsyncCoroutine( ResourcePath, loadedResource, OnResourceLoaded ),
			"ResourceManger::LoadResourceAsync: Loading " + ResourcePath );
	}


	/////////////////////////////////////////////////////////////////
	/// <summary> Asynchronously load resource with given path, load recursively if composite type. Onload completed with success callbeck is called
	/// return iterator of the MAIN load coroutine </summary>

	public	static  	IEnumerator LoadResourceAsyncCoroutine<T>( string ResourcePath, LoadedData<T> loadedResource, System.Action<T> OnResourceLoaded, System.Action<string> OnFailure = null) where T : Object
	{
		if ( loadedResource == null )
		{
			loadedResource = new LoadedData<T>();
		}

		print( "COROUTINE ASYNC Loading: " + ResourcePath );
		
		yield return CoroutinesManager.Start( InternalLoadResourceAsync( ResourcePath, loadedResource ),
			"ResourceManger::LoadResourceAsyncCoroutine: Loading " + ResourcePath );
		
		print( "COROUTINE ASYNC Loaded: " + ResourcePath );

		bool bHasValidAsset = loadedResource.Asset != null;
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
			OnResourceLoaded( loadedResource.Asset as T );
		}
	}

	
	/////////////////////////////////////////////////////////////////
	private	static  IEnumerator InternalLoadResourceAsync<T>( string ResourcePath, LoadedData<T> loadedResource ) where T : Object
	{
		System.Diagnostics.Stopwatch m_StopWatch = null;
		print( "ResourceManger::InternalLoadResourceAsync: INTERNAL ASYNC Loading: " + ResourcePath );

		if ( m_ShowDebugInfo )
		{
			m_StopWatch = new System.Diagnostics.Stopwatch();
			m_StopWatch.Start();
		}
		
		ResourceRequest request = Resources.LoadAsync( ResourcePath );
//		request.priority = 0;
		while( request.isDone == false )
		{
			yield return null;
		}

		loadedResource.Asset = request.asset as T;

		if ( m_ShowDebugInfo )
		{
			m_StopWatch.Stop();
			print( "ResourceManger::InternalLoadResourceAsync: INTERNAL ASYNC Loaded: " + ResourcePath + " in " + m_StopWatch.Elapsed.Milliseconds + "ms" );
			m_StopWatch.Reset();
		}

		// If composite, load children
		if ( loadedResource.Asset is IResourceComposite )
		{
			IResourceComposite composite = loadedResource.Asset as IResourceComposite;
			if ( composite.NeedToBeLoaded() == true )
			{
				composite.Reinit();

				string[] compositeFilePaths = composite.GetChildPaths();
				int arraySize = compositeFilePaths.Length;

				for ( int i = 0; i < arraySize; i++ )
				{
					string compositeFilePath = compositeFilePaths[i];
					LoadedData<Object> childloadedResource = new LoadedData<Object>();
					yield return InternalLoadResourceAsync( compositeFilePath, childloadedResource );
/*					yield return CoroutinesManager.Start
					(
						InternalLoadResourceAsync( compositeFilePath, childloadedResource ),
						"ResourceManger::InternalLoadResourceAsync: Loading " + compositeFilePath
					);
*/
					yield return null;
					composite.AddChild( childloadedResource.Asset );
				}
			}
		}

		print( "ResourceManger::InternalLoadResourceAsync: INTERNAL ASYNC Loaded: " + ResourcePath );
	}


}



public partial class ResourceManager {

	[System.Serializable]
	public	class WeakRefResource {

		[SerializeField]
		private		Object				m_Asset					= null;

		[SerializeField]
		private		string				m_AssetPath				= string.Empty;

//		private		bool				m_bIsAssetPathValid		= false;

		/*
		public		bool				FileExists
		{
			get { return System.IO.File.Exists( m_AssetPath ); }
		}
		*/

		///////////////////////////////////////////////////
		public WeakRefResource( string path )
		{
			if ( Utils.String.IsAssetsPath( path ) )
			{
				Utils.String.ConvertFromAssetPathToResourcePath( ref path );
				m_AssetPath = path;
			}

			if ( Utils.String.IsResourcesPath( path ) )
			{
				m_AssetPath = path;
			}

			// TODO System.WeakReference
		}


		///////////////////////////////////////////////////
		public	Object	TryLoad()
		{
			return Resources.Load( m_AssetPath );
		}


		///////////////////////////////////////////////////
		public	bool	TryLoad( ref Object objRef )
		{
			objRef = Resources.Load( m_AssetPath );
			return objRef != null;
		}


		///////////////////////////////////////////////////
		public	void	Unload()
		{
			Resources.UnloadAsset( m_Asset );
		}


		///////////////////////////////////////////////////
		public	void	Reset()
		{
			m_Asset		= null;
			m_AssetPath	= string.Empty;
		}

	}

}