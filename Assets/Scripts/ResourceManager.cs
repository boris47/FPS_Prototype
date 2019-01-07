
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

	private	static	ResourceManager		m_Instance = null;

	private	static	bool				m_IsInitialized = false;

	private	static	bool				m_ShowDebugInfo	= false;

	// 
	private void Awake()
	{	// Singleton
		if ( m_Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		DontDestroyOnLoad( this );
		m_Instance = this;
		m_IsInitialized = true;
	}

	public class LoadData<T> {
		public	T Asset;
	}

	///////////////////////////////////////////////////

	
	private	static	string RESOURCES_PATH = "Assets/Resources/";
	/// <summary> Concatenate ResourcesPath, resourcePath and extension </summary>
	private static string GetFullPath( string ResourcePath )
	{
		return RESOURCES_PATH + ResourcePath + ".asset";
	}

	// 
	private	static	void	Initialize()
	{
		if ( m_IsInitialized == false )
		{
			GameObject go = new GameObject();
			go.hideFlags = HideFlags.HideAndDontSave;
			m_Instance = go.AddComponent<ResourceManager>();
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

		string fullPath = GetFullPath( ResourcePath );
		if ( System.IO.File.Exists( fullPath ) == false )
		{
			print( "Cannot find resource: " + fullPath );
			return false;
		}

		if ( m_ShowDebugInfo )				print( "SYNC Loading: " + fullPath );
		{
			result &= InternalLoadResourceSync( ResourcePath, loadData, ref result );
		}
		if ( m_ShowDebugInfo && result )	print( "SYNC Loaded: " + fullPath );

		return result;
	}


	/////////////////////////////////////////////////////////////////
	private	static  bool InternalLoadResourceSync<T>( string ResourcePath, LoadData<T> loadData, ref bool bResult ) where T : Object
	{
		if ( bResult == true )
		{
			string fullPath = GetFullPath( ResourcePath );

			if ( m_ShowDebugInfo )				print( "INTERNAL SYNC Loading: " + fullPath );
			{
				T asset = Resources.Load<T>( ResourcePath );
				loadData.Asset = asset;

				if ( asset is IResourceComposite )
				{
					IResourceComposite composite = asset as IResourceComposite;
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
			if ( m_ShowDebugInfo && bResult )	print( "INTERNAL SYNC Loaded: " + RESOURCES_PATH + fullPath );
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

	public	static  	IEnumerator LoadResourceAsyncCoroutine<T>( string ResourcePath, LoadData<T> loadData, System.Action<T> OnResourceLoaded = null ) where T : Object
	{
		Initialize();

		string fullPath = GetFullPath( ResourcePath );
		if ( System.IO.File.Exists( fullPath ) == false )
		{
			print( "Cannot find resource: " + fullPath );
			yield break;
		}

		if ( m_ShowDebugInfo )					print( "COROUTINE ASYNC Loading: " + fullPath );
		{
			yield return m_Instance.StartCoroutine( InternalLoadResourceAsync( ResourcePath, loadData ) );
		}
		if ( m_ShowDebugInfo && loadData.Asset != null )	print( "COROUTINE ASYNC Loaded: " + fullPath );

		if ( OnResourceLoaded != null )
			OnResourceLoaded( loadData.Asset as T );
	}

	
	/////////////////////////////////////////////////////////////////
	private	static  IEnumerator InternalLoadResourceAsync<T>( string ResourcePath, LoadData<T> loadData ) where T : Object
	{
		string fullPath = GetFullPath( ResourcePath );

		T asset = null;
		if ( m_ShowDebugInfo )			print( "INTERNAL ASYNC Loading: " + fullPath );
		{
			ResourceRequest request = Resources.LoadAsync( ResourcePath );
			yield return request;

			asset =  request.asset as T;
			loadData.Asset = asset;

			if ( asset is IResourceComposite )
			{
				IResourceComposite composite = request.asset as IResourceComposite;
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
		if ( m_ShowDebugInfo && asset != null )		print( "INTERNAL ASYNC Loaded: " + RESOURCES_PATH + fullPath );
	}


}