
using System.Collections;
using UnityEngine;


public interface IResourceComposite
{
	bool		NeedToBeLoaded		();
	void		Reinit				();
	string[]	GetChildPaths		();
	void		AddChild			(Object child);
}

public sealed class ResourceManager : MonoBehaviourSingleton<ResourceManager>
{
	/// <summary> Class that contains the loaded asset, if load succeded </summary>
	public class AsyncLoadedData<T>
	{
		public		T		Asset		= default(T);
		public		bool	IsLoaded	= false;
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


	#region SYNC LOAD

	/// <summary> Synchronously load resource with given path, load recursively if composite type </summary>
	public static bool LoadResourceSync<T>(string resourcePath, out T loadedResource) where T : Object
	{
		return InternalLoadResourceSync(resourcePath, out loadedResource);
	}


	/////////////////////////////////////////////////////////////////
	private static bool InternalLoadResourceSync<T>(string resourcePath, out T loadedResource) where T : Object
	{
		loadedResource = default;

		print( $"INTERNAL SYNC Loading: {resourcePath}" );
		System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
		stopWatch.Start();
		{
			loadedResource = Resources.Load<T>(resourcePath);
		}
		stopWatch.Stop();

		bool bResult = loadedResource.IsNotNull();

		if (loadedResource is IResourceComposite composite && composite.NeedToBeLoaded())
		{
			composite.Reinit();

			foreach (string childPath in composite.GetChildPaths())
			{
				if (bResult = InternalLoadResourceSync(childPath, out Object childloadedResource))
				{
					composite.AddChild(childloadedResource);
				}
			}
		}

		print(bResult ? $"INTERNAL SYNC loaded: {resourcePath} in {stopWatch.Elapsed.Milliseconds}ms" : $"INTERNAL SYNC not loaded!!");
		return bResult;
	}

	#endregion

	/////////////////////////////////////////////////////////////////

	#region A-SYNC LOAD

	/// <summary> Asynchronously load resource with given path, load recursively if composite type. Onload completed with success callbeck is called </summary>
	public static void LoadResourceAsync<T>(in string resourcePath, in AsyncLoadedData<T> loadedResource, in System.Action<T> onResourceLoaded, in System.Action<string> onFailure = null) where T : Object
	{
		CoroutinesManager.Start(LoadResourceAsyncCoroutine(resourcePath, loadedResource, onResourceLoaded, onFailure), $"ResourceManger::LoadResourceAsync: Loading ${resourcePath}");
	}

	/// <summary> Asynchronously load resource with given path, load recursively if composite type. Onload completed with success callbeck is called </summary>
	public static void LoadResourceAsync<T>(in string resourcePath, in System.Action<T> onResourceLoaded, in System.Action<string> onFailure = null) where T : Object
	{
		CoroutinesManager.Start(LoadResourceAsyncCoroutine(resourcePath, null, onResourceLoaded, onFailure), $"ResourceManger::LoadResourceAsync: Loading ${resourcePath}");
	}


	/////////////////////////////////////////////////////////////////
	/// <summary> Asynchronously load resource with given path, load recursively if composite type. Onload completed with success callback is called </summary>
	private static IEnumerator LoadResourceAsyncCoroutine<T>(string ResourcePath, AsyncLoadedData<T> loadedResource, System.Action<T> OnResourceLoaded = null, System.Action<string> OnFailure = null) where T : Object
	{
		loadedResource = loadedResource ?? new AsyncLoadedData<T>();

		yield return CoroutinesManager.Start(InternalLoadResourceAsync(ResourcePath, loadedResource), $"ResourceManger::LoadResourceAsyncCoroutine: Loading {ResourcePath}");

		if (loadedResource.IsLoaded)
		{
			if (OnResourceLoaded.IsNotNull())
			{
				OnResourceLoaded(loadedResource.Asset as T);
			}
		}
		else
		{
			Debug.LogError($"Cannot load resource {ResourcePath}");
			if (OnFailure.IsNotNull())
			{
				OnFailure(ResourcePath);
			}
		}
	}


	/////////////////////////////////////////////////////////////////
	private static IEnumerator InternalLoadResourceAsync<T>(string resourcePath, AsyncLoadedData<T> loadedResource) where T : Object
	{
		print($"Loading: {resourcePath}");
		System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
		stopWatch.Start();
		{
			ResourceRequest request = Resources.LoadAsync(resourcePath);
			request.priority = 0;

			yield return new WaitUntil(() => request.isDone);

			loadedResource.Asset = request.asset as T;
			loadedResource.IsLoaded = loadedResource.Asset.IsNotNull();
		}
		stopWatch.Stop();

		if (loadedResource.IsLoaded && loadedResource.Asset is IResourceComposite composite && composite.NeedToBeLoaded())
		{
			composite.Reinit();

			foreach (string childPath in composite.GetChildPaths())
			{
				AsyncLoadedData<Object> childloadedResource = new AsyncLoadedData<Object>();
				yield return InternalLoadResourceAsync(childPath, childloadedResource);
				yield return null;
				composite.AddChild(childloadedResource.Asset);
			}
		}

		print(loadedResource.IsLoaded ? $"Loaded: {resourcePath} in {stopWatch.Elapsed.Milliseconds}ms" : $"Not loaded!!");
	}

	#endregion
}