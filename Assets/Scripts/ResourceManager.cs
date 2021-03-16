
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public interface IResourceComposite
{
	bool		NeedToBeLoaded		();
	void		Reinit				();
	string[]	GetChildPaths		();
	void		AddChild			(Object child);
}

public partial class ResourceManager : MonoBehaviourSingleton<ResourceManager>
{
	/// <summary> Class that contains the loaded asset, if load succeded </summary>
	public class AsyncLoadedData<T>
	{
		public T Asset = default(T);
	}

	private static new System.Action<string> print = delegate { };

	/////////////////////////////////////////////////////////////////
	protected override void OnInitialize()
	{
		base.OnInitialize();

		if (ShowDebugInfo)
		{
			print = Debug.Log;
		}
	}


	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////		SYNC		/////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////

	/// <summary> Synchronously load resource with given path, load recursively if composite type </summary>
	public static bool LoadResourceSync<T>(string ResourcePath, out T loadedResource) where T : Object
	{
		bool result = true;
		{
			loadedResource = default(T);
			print( $"SYNC Loading: {ResourcePath}" );
			result &= InternalLoadResourceSync( ResourcePath, ref loadedResource, ref result );
			print( $"SYNC {(!result?"not":"")} loaded: {ResourcePath}" );
		}
		return result;
	}


	/////////////////////////////////////////////////////////////////
	private static bool InternalLoadResourceSync<T>(string ResourcePath, ref T loadedResource, ref bool bResult) where T : Object
	{
		if (bResult == false)
		{
			return bResult;
		}

		System.Diagnostics.Stopwatch m_StopWatch = null;
		print( $"INTERNAL SYNC Loading: {ResourcePath}" );

		m_StopWatch = new System.Diagnostics.Stopwatch();
		m_StopWatch.Start();

		loadedResource = Resources.Load<T>( ResourcePath );

		m_StopWatch.Stop();
		print( $"INTERNAL SYNC Loaded: {ResourcePath} in {m_StopWatch.Elapsed.Milliseconds}ms" );
		m_StopWatch.Reset();

		bResult = loadedResource.IsNotNull();

		// Coposite parse
		if (loadedResource is IResourceComposite)
		{
			IResourceComposite composite = loadedResource as IResourceComposite;
			if (composite.NeedToBeLoaded() == true)
			{
				composite.Reinit();
				foreach (string childPath in composite.GetChildPaths())
				{
					Object childloadedResource = null;
					if (bResult = InternalLoadResourceSync(childPath, ref childloadedResource, ref bResult))
					{
						composite.AddChild(childloadedResource);
					}
				}
			}
		}

		if (bResult)
		{
			print( $"INTERNAL SYNC Loaded: {ResourcePath}" );
		}

		return bResult;
	}



	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////		ASYNC		/////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////

	/// <summary> Asynchronously load resource with given path, load recursively if composite type. Onload completed with success callbeck is called </summary>
	public static void LoadResourceAsync<T>(string ResourcePath, AsyncLoadedData<T> LoadedResource, System.Action<T> OnResourceLoaded) where T : Object
	{
		CoroutinesManager.Start( LoadResourceAsyncCoroutine( ResourcePath, LoadedResource, OnResourceLoaded ), $"ResourceManger::LoadResourceAsync: Loading ${ResourcePath}" );
	}


	/////////////////////////////////////////////////////////////////
	/// <summary> Asynchronously load resource with given path, load recursively if composite type. Onload completed with success callbeck is called
	/// return iterator of the MAIN load coroutine </summary>

	public static IEnumerator LoadResourceAsyncCoroutine<T>(string ResourcePath, AsyncLoadedData<T> loadedResource, System.Action<T> OnResourceLoaded = null, System.Action<string> OnFailure = null) where T : Object
	{
		loadedResource = loadedResource ?? new AsyncLoadedData<T>();
		
		print( $"COROUTINE ASYNC Loading: {ResourcePath}" );

		yield return CoroutinesManager.Start( InternalLoadResourceAsync( ResourcePath, loadedResource ), $"ResourceManger::LoadResourceAsyncCoroutine: Loading {ResourcePath}" );

		print( $"COROUTINE ASYNC Loaded: {ResourcePath}" );

		bool bHasValidAsset = loadedResource.Asset != null;
		if (bHasValidAsset == false)
		{
			Debug.LogError( $"ResourceManager::LoadResourceAsyncCoroutine: Cannot load resource {ResourcePath}" );
			if (OnFailure.IsNotNull())
			{
				OnFailure( ResourcePath );
			}
		}

		if (OnResourceLoaded.IsNotNull() && bHasValidAsset == true)
		{
			OnResourceLoaded(loadedResource.Asset as T);
		}
	}


	/////////////////////////////////////////////////////////////////
	private static IEnumerator InternalLoadResourceAsync<T>(string ResourcePath, AsyncLoadedData<T> loadedResource) where T : Object
	{
		System.Diagnostics.Stopwatch m_StopWatch = null;
		print( $"ResourceManger::InternalLoadResourceAsync: INTERNAL ASYNC Loading: {ResourcePath}" );

		m_StopWatch = new System.Diagnostics.Stopwatch();
		m_StopWatch.Start();

		ResourceRequest request = Resources.LoadAsync(ResourcePath);
		request.priority = 0;

		yield return new WaitUntil( () => request.isDone );

		loadedResource.Asset = request.asset as T;

		m_StopWatch.Stop();
		print( $"ResourceManger::InternalLoadResourceAsync: INTERNAL ASYNC Loaded: {ResourcePath} in {m_StopWatch.Elapsed.Milliseconds}ms" );
		m_StopWatch.Reset();

		// If composite, load children
		if (loadedResource?.Asset is IResourceComposite composite && composite.NeedToBeLoaded() == true)
		{
			composite.Reinit();

			string[] compositeFilePaths = composite.GetChildPaths();
			int arraySize = compositeFilePaths.Length;

			for (int i = 0; i < arraySize; i++)
			{
				string compositeFilePath = compositeFilePaths[i];
				AsyncLoadedData<Object> childloadedResource = new AsyncLoadedData<Object>();
				yield return InternalLoadResourceAsync(compositeFilePath, childloadedResource);
				yield return null;
				composite.AddChild(childloadedResource.Asset);
			}
		}

		print( $"ResourceManger::InternalLoadResourceAsync: INTERNAL ASYNC Loaded: {ResourcePath}" );
	}


}



public partial class ResourceManager
{
	[System.Serializable]
	public class WeakRefResource
	{
		[System.NonSerialized]
		private Object m_Asset = null;

		[SerializeField]
		private string m_AssetPath = string.Empty;

		///////////////////////////////////////////////////
		public WeakRefResource(string path)
		{
			if (Utils.String.IsAssetsPath(path))
			{
				Utils.String.ConvertFromAssetPathToResourcePath(ref path);
				m_AssetPath = path;
			}

			if (Utils.String.IsResourcesPath(path))
			{
				m_AssetPath = path;
			}

			// TODO System.WeakReference
		}


		///////////////////////////////////////////////////
		public Object TryLoad()
		{
			return Resources.Load(m_AssetPath);
		}


		///////////////////////////////////////////////////
		public bool TryLoad(ref Object objRef)
		{
			objRef = Resources.Load(m_AssetPath);
			return objRef != null;
		}


		///////////////////////////////////////////////////
		public void Unload()
		{
			Resources.UnloadAsset(m_Asset);
		}


		///////////////////////////////////////////////////
		public void Reset()
		{
			m_Asset = null;
			m_AssetPath = string.Empty;
		}

	}

}