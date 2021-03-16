using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[System.Serializable]
public	enum ESceneEnumeration
{
	NEXT		= -256,
	PREVIOUS	= -255,
	NONE		= -1,
	INTRO		= 0,
	LOADING		= INTRO + 1,
	MAIN_MENU	= LOADING + 1,
	OPENWORLD1	= MAIN_MENU + 1,
	OPENWORLD2	= OPENWORLD1 + 1,
	OPENWORLD3	= OPENWORLD2 + 1,
	ENDING		= OPENWORLD3 + 1,
	COUNT
}

[System.Serializable]
public	enum ESceneLoadStep
{
	BEFORE_SCENE_ACTIVATION,
	AFTER_SCENE_ACTIVATION,
	AFTER_SAVE_LOAD,
	/// <summary> Called by Unity Scene Manager </summary>
	SCENE_LOADED
}


public class CustomSceneManager : MonoBehaviourSingleton<CustomSceneManager>
{
	public class LoadSceneData
	{
		public	ESceneEnumeration	eScene					= ESceneEnumeration.NONE;
		public	ESceneEnumeration	ePrevScene				= ESceneEnumeration.PREVIOUS;
		public	LoadSceneMode		eMode					= LoadSceneMode.Single;
		public	bool				bMustLoadSave			= false;
		public	string				sSaveToLoad				= string.Empty;
		public	System.Action		pOnPreLoadCompleted		= null;
		public	System.Action		pOnLoadCompleted		= null;
	}

	private	static	bool				m_IsCurrentlyPreloading		= false;
	private	static	bool				m_HasPreloadedScene			= false;


	private	static readonly Dictionary<ESceneLoadStep, List<System.Action<ESceneEnumeration, ESceneEnumeration>>> Delegates = new Dictionary<ESceneLoadStep, List<System.Action<ESceneEnumeration, ESceneEnumeration>>>()
	{
		{ESceneLoadStep.BEFORE_SCENE_ACTIVATION,	new List<System.Action<ESceneEnumeration, ESceneEnumeration>>()},
		{ESceneLoadStep.AFTER_SCENE_ACTIVATION,		new List<System.Action<ESceneEnumeration, ESceneEnumeration>>()},
		{ESceneLoadStep.AFTER_SAVE_LOAD,			new List<System.Action<ESceneEnumeration, ESceneEnumeration>>()},
		{ESceneLoadStep.SCENE_LOADED,				new List<System.Action<ESceneEnumeration, ESceneEnumeration>>()}
	};

	private static int m_CurrentSceneIndex = 0;
	public static int CurrentSceneIndex => m_CurrentSceneIndex;
	public static ESceneEnumeration CurrentSceneEnumeration => (ESceneEnumeration)m_CurrentSceneIndex;


	protected override void OnInitialize()
	{
		base.OnInitialize();

		m_CurrentSceneIndex = SceneManager.GetActiveScene().buildIndex;

		SceneManager.sceneLoaded += OnSceneLoaded;
	}


	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}


	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex != (int)ESceneEnumeration.LOADING) // Loading scene is not to consider
		{
			Delegates[ESceneLoadStep.SCENE_LOADED].ForEach(d => d?.Invoke((ESceneEnumeration)m_CurrentSceneIndex, (ESceneEnumeration)scene.buildIndex));
			m_CurrentSceneIndex = scene.buildIndex;
		}
	}

	/////////////////////////////////////////////////////////////////
	/// <summary> Return true if the current scene index is a play scene index </summary>
	public static bool IsGameScene()
	{
		return IsGameScene(m_CurrentSceneIndex);
	}


	/////////////////////////////////////////////////////////////////
	/// <summary> Return true if the given scene index is a play scene index </summary>
	public static bool IsGameScene(int sceneIndex)
	{
		return sceneIndex > (int)ESceneEnumeration.MAIN_MENU && sceneIndex < (int)ESceneEnumeration.COUNT;
	}


	/////////////////////////////////////////////////////////////////
	/// <summary> Return true if the given scene index is a play scene index </summary>
	public static bool IsGameScene(ESceneEnumeration sceneEnumeration)
	{
		return IsGameScene((int)sceneEnumeration);
	}


	/////////////////////////////////////////////////////////////////
	private	static	bool	HasGotValidLoadScenData( LoadSceneData loadSceneData )
	{
		if (loadSceneData == null)
			return false;

		if (loadSceneData.eScene == ESceneEnumeration.NONE)
			return false;

		// Requested the already loaded scene
		if ((int)loadSceneData.eScene == m_CurrentSceneIndex)
		{
			return false;
		}

		// Check for save existence
		if (loadSceneData.bMustLoadSave && System.IO.File.Exists(loadSceneData.sSaveToLoad) == false)
		{
			return false;
		}

		// Requesting to go to next scene
		if (loadSceneData.eScene == ESceneEnumeration.NEXT)
		{
			ESceneEnumeration nextScene = (ESceneEnumeration)(m_CurrentSceneIndex + 1);
			if ((int)nextScene >= SceneManager.sceneCountInBuildSettings)
			{
				return false;
			}

			loadSceneData.eScene = nextScene;
		}

		// Requesting to go to previous scene
		if (loadSceneData.eScene == ESceneEnumeration.PREVIOUS)
		{
			ESceneEnumeration previousScene = (ESceneEnumeration)(m_CurrentSceneIndex + 1);
			if (previousScene < 0)
			{
				return false;
			}

			loadSceneData.eScene = previousScene;
		}

		loadSceneData.ePrevScene = (ESceneEnumeration)(m_CurrentSceneIndex);
		return true;
	}

	/*
	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////		SYNC		/////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////

	/// <summary> Parse the load request synchronously </summary>
	public	static void	LoadSceneSync( LoadSceneData loadSceneData )
	{
		if (HasGotValidLoadScenData(loadSceneData))
		{
			InternalLoadSceneSync(loadSceneData);
		}
	}


	/// <summary> Internally load the scene synchronously </summary>
	private static void InternalLoadSceneSync(LoadSceneData loadSceneData)
	{
		// Set global state as ChangingScene state
		GlobalManager.bIsChangingScene = true;

		GlobalManager.InputMgr.DisableCategory(EInputCategory.ALL);

		CoroutinesManager.StopAll();

		GlobalManager.bIsLoadingScene = true;
		SceneManager.LoadScene((int)loadSceneData.eScene, loadSceneData.eMode);

		// Ensure that loaded scene is also the active one
		SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex((int)loadSceneData.eScene));

		Delegates[ESceneLoadStep.BEFORE_SCENE_ACTIVATION].ForEach(d => d?.Invoke(CurrentSceneEnumeration, loadSceneData.eScene));
		Delegates[ESceneLoadStep.AFTER_SCENE_ACTIVATION].ForEach(d => d?.Invoke(CurrentSceneEnumeration, loadSceneData.eScene));

		// Preload callback
		loadSceneData.pOnPreLoadCompleted?.Invoke();

		SoundManager.OnSceneLoaded();

		// LOAD DATA
		if (loadSceneData.bMustLoadSave)
		{
			GameManager.StreamEvents.Load(loadSceneData.sSaveToLoad);
		}

		// Call on every registered
		Delegates[ESceneLoadStep.AFTER_SAVE_LOAD].ForEach(d => d?.Invoke(CurrentSceneEnumeration, loadSceneData.eScene));

		System.GC.Collect();

		loadSceneData.pOnLoadCompleted?.Invoke();

		GlobalManager.bIsLoadingScene = false;

		GlobalManager.InputMgr.EnableCategory(EInputCategory.ALL);

		// Remove global state as ChangingScene state
		GlobalManager.bIsChangingScene = false;

		UIManager.Indicators.enabled = true;
		UIManager.Minimap.enabled = true;

		SoundManager.OnSceneLoaded();

		// Leave to UIManager the decision on which UI menu must be shown
		UIManager.Instance.EnableMenuByScene( loadSceneData.eScene );
	}
	*/

	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////		ASYNC		/////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////

	
	public class LoadCondition
	{
		private	bool			bHasToWait			= false;
		private	IEnumerator		pCoroutineToWait	= null;

		public		void	AssigPendingOperations( ref bool waiter )
		{
			bHasToWait = waiter;
		}

		public		void	AssigPendingOperations( IEnumerator enumerator )
		{
			pCoroutineToWait = enumerator;
		}

		public IEnumerator WaitForPendingOperations()
		{
			yield return new WaitWhile(() => bHasToWait);

			if (pCoroutineToWait.IsNotNull())
			{
				yield return pCoroutineToWait;
			}
		}
	}

	public class PreloadSceneData
	{
		public	AsyncOperation		asyncOperation	= null;
		public	ESceneEnumeration	eScene			= ESceneEnumeration.NONE;
	}

	/*
	/// <summary> EXPERIMENTAL: Preload a scene and return into the second argument the AsyncOperation that manage that load </summary>
	public	static	IEnumerator Preload ( ESceneEnumeration SceneIdx, PreloadSceneData preloadSceneData )
	{
		Debug.Log( "Preloading of scene " + SceneIdx );

		if (m_IsCurrentlyPreloading || m_HasPreloadedScene)
		{
			Debug.LogError( "CoroutinesManager::Preload: a preload operation is currently in progress, preload denied for scene " + preloadSceneData.eScene.ToString() );
			return new WaitForSecondsRealtime(1.0f);
		}

		m_IsCurrentlyPreloading = true;

		IEnumerator enumerator = m_Instance.PreloadCO( SceneIdx, preloadSceneData );
		CoroutinesManager.Start( enumerator, "CustomSceneManager::Preload: Preloading scene " + SceneIdx );
		return enumerator;
	}


	/// <summary> Complete the load of a previous preloaded scene </summary>
	public	static	IEnumerator	CompleteSceneAsyncLoad( PreloadSceneData preloadSceneData )
	{
		IEnumerator enumerator = m_Instance.CompleteSceneAsyncLoadCO( preloadSceneData );
		CoroutinesManager.Start( enumerator, "CustomSceneManager::CompleteSceneAsyncLoad: Completing load of " + preloadSceneData.eScene );
		return enumerator;
	}
	*/


	/// <summary> Launch load of a scene asynchronously </summary>
	public	static IEnumerator	LoadSceneAsync( LoadSceneData loadSceneData, LoadCondition loadCondition = null )
	{
		if ( HasGotValidLoadScenData( loadSceneData ) == false || m_HasPreloadedScene || m_IsCurrentlyPreloading )
			return null;

		IEnumerator enumerator = m_Instance.LoadSceneAsyncCO( loadSceneData, loadCondition );
		CoroutinesManager.Start( enumerator, "CustomSceneManager::LoadSceneAsync: Loading " + loadSceneData.eScene );
		return enumerator;
	}


	/// <summary> Start the unload of a scene, return the coroutine managing the async operation </summary>
	public	static	IEnumerator	UnLoadSceneAsync( int SceneIdx )
	{
		Scene scene =  SceneManager.GetSceneByBuildIndex( SceneIdx );
		if ( scene.isLoaded == false )
			return null;

		IEnumerator enumerator = m_Instance.UnLoadSceneAsyncCO( scene );
		CoroutinesManager.Start( enumerator, "CustomSceneManager::UnLoadSceneAsync:Async unload of " + scene.name );
		return enumerator;
	}

	/*
	/////////////////////////////////////////////////////////////////
	private	IEnumerator PreloadCO( ESceneEnumeration SceneIdx, PreloadSceneData preloadSceneData )
	{
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync( (int)SceneIdx, LoadSceneMode.Additive );

		if ( asyncOperation == null )
		{
			m_IsCurrentlyPreloading = false;
			Debug.LogError( "Cannot preload " + preloadSceneData.eScene.ToString() );
			yield break;
		}

		// We want this operation to impact performance less than possible
		asyncOperation.priority = 0;

		asyncOperation.allowSceneActivation = false;

		preloadSceneData.asyncOperation = asyncOperation;
		preloadSceneData.eScene = SceneIdx;

		// Wait for load completion
		while ( asyncOperation.progress < 0.9f )
		{
			yield return null;
		}

		print( "Preload completed" );
		m_IsCurrentlyPreloading = false;
		m_HasPreloadedScene = true;
	}


	/// <summary> EXPERIMENTAL: Internal coroutine that complete the load a preloaded scene </summary>
	private	IEnumerator	CompleteSceneAsyncLoadCO( PreloadSceneData preloadSceneData )
	{
		if ( m_HasPreloadedScene == false )
		{
			Debug.LogError( "Cannot complete preload before preload start itself" );
			yield break;
		}

		m_IsCurrentlyPreloading = false;

		yield return new WaitForEndOfFrame();
		preloadSceneData.asyncOperation.allowSceneActivation = true;

		// Wait for start completion
		yield return new WaitUntil( () => preloadSceneData.asyncOperation.isDone );

		m_HasPreloadedScene = false;

		SoundManager.OnSceneLoaded();

		GlobalManager.bIsLoadingScene = false;

		UIManager.Indicators.enabled = true;
		UIManager.Minimap.enabled = true;

		GlobalManager.InputMgr.EnableCategory(EInputCategory.ALL);

		GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, true);

		// Leave to UIManager the decision on which UI menu must be shown
		UIManager.Instance.EnableMenuByScene( preloadSceneData.eScene );
	}
	*/


	/// <summary> Internal coroutine that load a scene asynchronously </summary>
	private	IEnumerator	LoadSceneAsyncCO( LoadSceneData loadSceneData, LoadCondition loadCondition = null )
	{
		System.Diagnostics.Stopwatch loadWatch = new System.Diagnostics.Stopwatch();
		loadWatch.Start();

		// Disable all input categories
		GlobalManager.InputMgr.DisableCategory( EInputCategory.ALL );

		GlobalManager.bIsLoadingScene = true;
		/*
		MonoBehaviour[] singletons = FindObjectsOfType<MonoBehaviour>().Where(m => m is ISingleton).ToArray();
		foreach(var singleton in singletons)
		{
			singleton.enabled = false;
		}
		*/
		// Wait for every coroutine
		yield return CoroutinesManager.WaitPendingCoroutines();

		// Enable Loading UI 
		Loading.SetLoadingSceneName( loadSceneData.eScene );
		Loading.Show();

		ESceneEnumeration prevSceneEnumeration = CurrentSceneEnumeration;
		Loading.SetSubTask( "Showing Scene 'Loading'" );
		{
			// Set Loading Scene as currently active one
			SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex((int)ESceneEnumeration.LOADING));

			// Set global state as ChangingScene state
			GlobalManager.bIsChangingScene = true;
		}
		Loading.EndSubTask();

		Loading.SetSubTask( $"Unloading scene '{(ESceneEnumeration)m_CurrentSceneIndex}'" );
		{
			// Load Synchronously Loading Scene synchronously
			AsyncOperation asyncOp = SceneManager.UnloadSceneAsync(m_CurrentSceneIndex);
			asyncOp.priority = 0;
			yield return asyncOp;

		//	asyncOp = Resources.UnloadUnusedAssets();
		//	asyncOp.priority = 0;
		//	yield return asyncOp;

		//	System.GC.Collect();
		}
		Loading.EndSubTask();

		yield return null; yield return null;

		AsyncOperation asyncOperation = null;
		Loading.SetSubTask( $"Start async load of {loadSceneData.eScene}" );
		{
			// Start async load of scene
			asyncOperation = SceneManager.LoadSceneAsync((int)loadSceneData.eScene, LoadSceneMode.Additive); // Forcing additive because the loading scene

			// We want this operation to impact performance less than possible
			asyncOperation.priority = 0;

			asyncOperation.allowSceneActivation = false;

			// Wait for load completion
			while ( asyncOperation.progress < 0.90f )
			{
				Loading.SetProgress(asyncOperation.progress * 0.5f);
				yield return null;
			}

			// Send message "OnBeforeSceneActivation"
			{
				Loading.SetSubTask( "Calling 'OnBeforeSceneActivation' on receivers" );

				// Call on every registered
				Delegates[ESceneLoadStep.BEFORE_SCENE_ACTIVATION].ForEach(d => d?.Invoke(prevSceneEnumeration, loadSceneData.eScene));
			}
		}
		Loading.EndSubTask();

		Loading.SetSubTask( "Waiting for loading condition" );
		{
			// Wait for load condition if defined
			Loading.SetProgress( 0.60f );
			yield return loadCondition?.WaitForPendingOperations();
		}
		Loading.EndSubTask();

		float currentFixedDeltaTime = 0f;
		Loading.SetSubTask( "1. Waiting for pending coroutines" );
		{
			// Wait for every launched coroutine
			yield return CoroutinesManager.WaitPendingCoroutines();

			Physics.autoSimulation = false;
			currentFixedDeltaTime = Time.fixedDeltaTime;
			Time.fixedDeltaTime = 0.0f;

			// Setting the time scale to Zero in order to freeze everything but continue to receive unity messages
			Time.timeScale = 0f;
		}
		Loading.EndSubTask();

		Loading.SetSubTask( "Activation of the scene" );
		{
			// Proceed with scene activation and Awake and OnEnable Calls
			asyncOperation.allowSceneActivation = true;
			yield return null;

			// Ensure that loaded scene is also the active one
			SceneManager.SetActiveScene( SceneManager.GetSceneByBuildIndex( (int) loadSceneData.eScene ) );

			// Call on every registered
			Delegates[ESceneLoadStep.AFTER_SCENE_ACTIVATION].ForEach( d => d?.Invoke(prevSceneEnumeration, loadSceneData.eScene) );

			// Wait for every launched coroutine
			yield return CoroutinesManager.WaitPendingCoroutines();
		}
		Loading.EndSubTask();

		Loading.SetSubTask( "Calling 'OnAfterSceneActivation' on receivers" );
		{
			// Wait for start completion
			Loading.SetProgress( 0.70f );
			yield return new WaitUntil( () => asyncOperation.isDone );
		}
		Loading.EndSubTask();

		
		Loading.SetSubTask( "Calling 'pOnPreLoadCompleted' callback" );
		{
			// Pre load callback
			Loading.SetProgress( 0.80f );
			loadSceneData.pOnPreLoadCompleted?.Invoke();
			yield return null;
		}
		Loading.EndSubTask();

		Loading.SetSubTask( "2. Waiting for pending coroutines" );
		{
			// Wait for every launched coroutine in awake of scripts
			yield return CoroutinesManager.WaitPendingCoroutines();
		}
		Loading.EndSubTask();

		Loading.SetSubTask( "Calling 'SoundManager.OnSceneLoaded'" );
		{
			SoundManager.OnSceneLoaded();
			yield return null;
		}
		Loading.EndSubTask();

		// LOAD DATA
		if ( loadSceneData.bMustLoadSave == true )
		{
			Loading.SetSubTask( $"Going to load save '{loadSceneData.sSaveToLoad}'" );
			{
				Loading.SetProgress( 0.95f );
				GameManager.StreamEvents.Load( loadSceneData.sSaveToLoad );
			}
			Loading.EndSubTask();
		}
		
		Loading.SetSubTask( "4. Waiting for pending coroutines" );
		{
			// Wait for every coroutines started from load
			yield return CoroutinesManager.WaitPendingCoroutines();
		}
		Loading.EndSubTask();

		Loading.SetSubTask( "Calling 'OnAfterLoadedData' on receivers" );
		{
			// Call on every registered
			Delegates[ESceneLoadStep.AFTER_SAVE_LOAD].ForEach(d => d?.Invoke(prevSceneEnumeration, loadSceneData.eScene));
		}
		Loading.EndSubTask();

	//	Loading.SetSubTask( "Waiting for the unload of unused assets" );
		{
	// Unload unused asset in order to free same memory
	//		yield return Resources.UnloadUnusedAssets();
		}
	//	Loading.EndSubTask();

		Loading.SetSubTask( "GC..." );
		{
			System.GC.Collect();
		}
		Loading.EndSubTask();

		Loading.SetSubTask( "Calling 'pOnLoadCompleted' callback" );
		{
			// Post load callback
			Loading.SetProgress( 0.95f );
			loadSceneData.pOnLoadCompleted?.Invoke();
			yield return null;
		}
		Loading.EndSubTask();

		Loading.SetSubTask("5. Waiting for pending coroutines");
		{
			// Wait for every launched coroutine
			yield return CoroutinesManager.WaitPendingCoroutines();
		}
		Loading.EndSubTask();

		Loading.SetSubTask( "Enabling components" );
		{
			GlobalManager.bIsLoadingScene = false;
			Loading.SetProgress( 1.00f );
			if (UIManager.Indicators) UIManager.Indicators.enabled = true;
			if (UIManager.Minimap) UIManager.Minimap.enabled = true;
			yield return null;

			GlobalManager.InputMgr.EnableCategory(EInputCategory.ALL);

//			GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, true);

			// Wait for every launched coroutine
			yield return CoroutinesManager.WaitPendingCoroutines();
		}
		Loading.EndSubTask();
		Loading.Hide();

		// Leave to UIManager the decision on which UI menu must be shown
		UIManager.Instance.EnableMenuByScene(loadSceneData.eScene);

		yield return null;
		/*
		foreach (var singleton in singletons)
		{
			singleton.enabled = true;
		}
		*/
		Time.fixedDeltaTime = currentFixedDeltaTime;
		Physics.autoSimulation = true;
		Time.timeScale = 1f;

		loadWatch.Stop();
		Debug.LogFormat( "Loading {0} took {1}ms.", loadSceneData.eScene, loadWatch.ElapsedMilliseconds );
	}


	/////////////////////////////////////////////////////////////////
	private IEnumerator UnLoadSceneAsyncCO( Scene scene )
	{
		AsyncOperation operation = SceneManager.UnloadSceneAsync( scene );
		
		// We want this operation to impact performance less than possible
		operation.priority = 0;
		
		yield return operation;
	}




	// 
	public static void RegisterOnLoad(ESceneLoadStep step, System.Action<ESceneEnumeration, ESceneEnumeration> onSceneLoad)
	{
		Delegates[step].Add(onSceneLoad);
	}



	//
	public static void UnregisterOnLoad(ESceneLoadStep step, System.Action<ESceneEnumeration, ESceneEnumeration> onSceneLoad)
	{
		Delegates[step].Remove(onSceneLoad);
	}

}
