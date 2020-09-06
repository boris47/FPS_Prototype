using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


[System.Serializable]
public	enum ESceneEnumeration {
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
public	enum ESceneLoadStep {
	BEFORE_SCENE_ACTIVATION,
	AFTER_SCENE_ACTIVATION,
	AFTER_SAVE_LOAD
}



public class CustomSceneManager : SingletonMonoBehaviour<CustomSceneManager>
{
	public	static int	CurrentSceneIndex
	{
		get { return SceneManager.GetActiveScene().buildIndex; }
	}

	public class LoadSceneData
	{
		public	ESceneEnumeration	eScene					= ESceneEnumeration.NONE;
		public	ESceneEnumeration	iPrevSceneIdx			= ESceneEnumeration.PREVIOUS;
		public	bool				bMustLoadSave			= false;
		public	string				sSaveToLoad				= "";
		public	System.Action		pOnPreLoadCompleted		= delegate { };
		public	System.Action		pOnLoadCompleted		= delegate { };
	}

	private	static	bool				m_IsCurrentlyPreloading		= false;
	private	static	bool				m_HasPreloadedScene			= false;


	private	static readonly Dictionary< ESceneLoadStep, List<System.Action<ESceneEnumeration>> > Delegates = new Dictionary<ESceneLoadStep, List<System.Action<ESceneEnumeration>>>()
	{
		{ ESceneLoadStep.BEFORE_SCENE_ACTIVATION, new List<System.Action<ESceneEnumeration>>() },
		{ ESceneLoadStep.AFTER_SCENE_ACTIVATION, new List<System.Action<ESceneEnumeration>>() },
		{ ESceneLoadStep.AFTER_SAVE_LOAD, new List<System.Action<ESceneEnumeration>>() }
	};

	//	private	static	List< System.Action<KeyValuePair< SceneLoadStep, Scene>> > Delegates = new List<System.Action<KeyValuePair<SceneLoadStep, Scene>>>();

	/*
	/////////////////////////////////////////////////////////////////
	protected override void OnBeforeSceneLoad()
	{
		Delegates[SceneLoadStep.BEFORE_SCENE_ACTIVATION].Clear();
		Delegates[SceneLoadStep.AFTER_SCENE_ACTIVATION].Clear();
		Delegates[SceneLoadStep.AFTER_SAVE_LOAD].Clear();
	}
	*/


	/////////////////////////////////////////////////////////////////
	private	static	bool	HasGotValidLoadScenData( LoadSceneData loadSceneData )
	{
		if ( loadSceneData == null )
			return false;

		if ( loadSceneData.eScene == ESceneEnumeration.NONE )
			return false;

		// Check for save existence
		if ( loadSceneData.bMustLoadSave && System.IO.File.Exists( loadSceneData.sSaveToLoad ) == false )
		{
			return false;
		}

		int currentSceneIdx = CurrentSceneIndex;

		// Requesting to go to next scene
		if ( loadSceneData.eScene == ESceneEnumeration.NEXT )
		{
			loadSceneData.eScene = (ESceneEnumeration)( currentSceneIdx + 1 );
			if ( (int)loadSceneData.eScene >= SceneManager.sceneCountInBuildSettings )
				return false;
		}

		// Requesting to go to previous scene
		if ( loadSceneData.eScene == ESceneEnumeration.PREVIOUS )
		{
			loadSceneData.eScene = (ESceneEnumeration)( currentSceneIdx - 1);
			if ( loadSceneData.eScene < 0 )
				return false;
		}

		if ( (int)loadSceneData.eScene == currentSceneIdx )
			return false;

		loadSceneData.iPrevSceneIdx = (ESceneEnumeration)( currentSceneIdx );
		return true;
	}


	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////		SYNC		/////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////

	/// <summary> Parse the load request synchronously </summary>
	public	static void	LoadSceneSync( LoadSceneData loadSceneData )
	{
		if ( HasGotValidLoadScenData( loadSceneData ) == false )
			return;

		InternalLoadSceneSync( loadSceneData );
	}


	/// <summary> Internally load the scene synchronously </summary>
	private	static void InternalLoadSceneSync( LoadSceneData loadSceneData )
	{
		// Set global state as ChangingScene state
		GlobalManager.bIsChangingScene = true;

		SceneManager.LoadScene( (int)loadSceneData.eScene, LoadSceneMode.Single );

		// Remove global state as ChangingScene state
		GlobalManager.bIsChangingScene = false;

		// Preload callback
		loadSceneData.pOnPreLoadCompleted();

		SoundManager.OnSceneLoaded();

		// LOAD DATA
		{
			if ( loadSceneData.bMustLoadSave == true )
			{
				GameManager.StreamEvents.Load( loadSceneData.sSaveToLoad );
			}
		}

		// Post load callback
		loadSceneData.pOnLoadCompleted();
	}


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
			this.bHasToWait = waiter;
		}

		public		void	AssigPendingOperations( IEnumerator enumerator )
		{
			this.pCoroutineToWait = enumerator;
		}

		public IEnumerator WaitForPendingOperations()
		{
			yield return new WaitWhile(() => this.bHasToWait);

			if (this.pCoroutineToWait.IsNotNull())
			{
				yield return this.pCoroutineToWait;
			}
		}
	}

	public class PreloadSceneData
	{
		public	AsyncOperation		asyncOperation	= null;
		public	ESceneEnumeration	eScene			= ESceneEnumeration.NONE;
	}


	/// <summary> EXPERIMENTAL: Preload a scene and return into the second argument the AsyncOperation that manage that load </summary>
	public	static	IEnumerator Preload ( ESceneEnumeration SceneIdx, PreloadSceneData preloadSceneData )
	{
		Debug.Log( "Preloading of scene " + SceneIdx );

		if ( m_IsCurrentlyPreloading == true || m_HasPreloadedScene == true )
		{
			Debug.LogError( "CoroutinesManager::Preload: a preload operation is currently in progress, preload denied for scene " + preloadSceneData.eScene.ToString() );
			return new WaitForSecondsRealtime(1.0f);
		}

		m_IsCurrentlyPreloading = true;

		IEnumerator enumerator = Instance.PreloadCO( SceneIdx, preloadSceneData );
		CoroutinesManager.Start( enumerator, "CustomSceneManager::Preload: Preloading scene " + SceneIdx );
		return enumerator;
	}


	/// <summary> Complete the load of a previous preloaded scene </summary>
	public	static	IEnumerator	CompleteSceneAsyncLoad( PreloadSceneData preloadSceneData )
	{
		IEnumerator enumerator = Instance.CompleteSceneAsyncLoadCO( preloadSceneData );
		CoroutinesManager.Start( enumerator, "CustomSceneManager::CompleteSceneAsyncLoad: Completing load of " + preloadSceneData.eScene );
		return enumerator;
	}


	/// <summary> Launch load of a scene asynchronously </summary>
	public	static IEnumerator	LoadSceneAsync( LoadSceneData loadSceneData, LoadCondition loadCondition = null )
	{
		if ( HasGotValidLoadScenData( loadSceneData ) == false || m_HasPreloadedScene || m_IsCurrentlyPreloading )
			return null;

		IEnumerator enumerator = Instance.LoadSceneAsyncCO( loadSceneData, loadCondition );
		CoroutinesManager.Start( enumerator, "CustomSceneManager::LoadSceneAsync: Loading " + loadSceneData.eScene );
		return enumerator;
	}


	/// <summary> Start the unload of a scene, return the coroutine managing the async operation </summary>
	public	static	IEnumerator	UnLoadSceneAsync( int SceneIdx )
	{
		Scene scene =  SceneManager.GetSceneByBuildIndex( SceneIdx );
		if ( scene.isLoaded == false )
			return null;

		IEnumerator enumerator = Instance.UnLoadSceneAsyncCO( scene );
		CoroutinesManager.Start( enumerator, "CustomSceneManager::UnLoadSceneAsync:Async unload of " + scene.name );
		return enumerator;
	}


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

		print("Preload completed");
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
		yield return new WaitUntil( () => preloadSceneData.asyncOperation.isDone);

		m_HasPreloadedScene = false;

		SoundManager.OnSceneLoaded();

		GlobalManager.bIsLoadingScene = false;

		UIManager.Indicators.enabled = true;
		UIManager.Minimap.enabled = true;

		GlobalManager.InputMgr.EnableCategory( EInputCategory.ALL );

		if ( GameManager.Instance )
			GameManager.SetInGameAs( true );

		GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, true);

		// Leave to UIManager the decision on which UI menu must be shown
		yield return UIManager.Instance.EnableMenuByScene( preloadSceneData.eScene );
	}



	/// <summary> Internal coroutine that load a scene asynchronously </summary>
	private	IEnumerator	LoadSceneAsyncCO( LoadSceneData loadSceneData, LoadCondition loadCondition = null )
	{
		System.Diagnostics.Stopwatch loadWatch = new System.Diagnostics.Stopwatch();
		loadWatch.Start();

		// Disable all input categories
		GlobalManager.InputMgr.DisableCategory( EInputCategory.ALL );

		// Wait for every coroutine
		yield return CoroutinesManager.WaitPendingCoroutines();

		GlobalManager.bIsLoadingScene = true;

		// Enable Loading UI
		Loading.SetLoadingSceneName( loadSceneData.eScene );
		Loading.Show();

		Loading.SetSubTask( "Loading 'Loading' Level" );
		{
			// Load Synchronously Loading Scene synchronously
			LoadSceneData loadingLoadSceneData = new LoadSceneData()
			{
				eScene = ESceneEnumeration.LOADING,
			};
			LoadSceneSync( loadingLoadSceneData );
			yield return null;

			// Set global state as ChangingScene state
			GlobalManager.bIsChangingScene = true;
		}
		Loading.EndSubTask();

		AsyncOperation asyncOperation = null;
		Loading.SetSubTask( "Start async Loading" );
		{
			// Start async load of scene
			asyncOperation = SceneManager.LoadSceneAsync( (int)loadSceneData.eScene, LoadSceneMode.Single );

			// We want this operation to impact performance less than possible
			asyncOperation.priority = 0;

			asyncOperation.allowSceneActivation = false;

			// Wait for load completion
			while ( asyncOperation.progress < 0.90f )
			{
				Loading.SetProgress( asyncOperation.progress * 0.5f );
				yield return null;
			}

			// Send message "OnBeforeSceneActivation"
			{
				Loading.SetSubTask( "Calling 'OnBeforeSceneActivation' on receivers" );

				// Call on every registered
				Delegates[ESceneLoadStep.BEFORE_SCENE_ACTIVATION].ForEach( d => d(loadSceneData.eScene) );
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

			// Setting the time scale to Zero because in order to freeze everything but continue to receive unity messages
			Time.timeScale = 0F;
		}
		Loading.EndSubTask();

		Loading.SetSubTask( "Fake activation of the scene" );
		{
			// Proceed with scene activation and Awake and OnEnable Calls
			asyncOperation.allowSceneActivation = true;
			yield return null;
			asyncOperation.allowSceneActivation = false;

			// Call on every registered
			Delegates[ESceneLoadStep.AFTER_SCENE_ACTIVATION].ForEach( d => d(loadSceneData.eScene) );

			// Wait for every launched coroutine
			yield return CoroutinesManager.WaitPendingCoroutines();
		}
		Loading.EndSubTask();

		Loading.SetSubTask( "Real activation of the scene" );
		{
			asyncOperation.allowSceneActivation = true;
		}
		Loading.EndSubTask();

		Loading.SetSubTask( "Calling 'OnAfterSceneActivation' on receivers" );
		{
			// Wait for start completion
			Loading.SetProgress( 0.70f );
			yield return new WaitUntil(() => asyncOperation.isDone);
		}
		Loading.EndSubTask();

		Loading.SetSubTask( "2. Waiting for pending coroutines" );
		{
			// Wait for every launched coroutine
			yield return CoroutinesManager.WaitPendingCoroutines();
		}
		Loading.EndSubTask();
		
		Loading.SetSubTask( "Calling 'pOnPreLoadCompleted' callback" );
		{
			// Pre load callback
			Loading.SetProgress( 0.80f );
			loadSceneData.pOnPreLoadCompleted();
			yield return null;
		}
		Loading.EndSubTask();

		Loading.SetSubTask( "3. Waiting for pending coroutines" );
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
			Loading.SetSubTask( "Going to load save " + loadSceneData.sSaveToLoad );
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
			Delegates[ESceneLoadStep.AFTER_SAVE_LOAD].ForEach( d => d(loadSceneData.eScene) );
		}
		Loading.EndSubTask();

		Loading.SetSubTask( "Waiting for the unload of unused assets" );
		{
	// Unload unused asset in order to free same memory
	//		yield return Resources.UnloadUnusedAssets();
		}
		Loading.EndSubTask();

		Loading.SetSubTask( "GC..." );
		{
			System.GC.Collect();
		}
		Loading.EndSubTask();

		Loading.SetSubTask( "Calling 'pOnLoadCompleted' callback" );
		{
			// Post load callback
			Loading.SetProgress( 0.95f );
			loadSceneData.pOnLoadCompleted();
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
			UIManager.Indicators.enabled = true;
			UIManager.Minimap.enabled = true;
			yield return null;

			GlobalManager.InputMgr.EnableCategory( EInputCategory.ALL );

			if ( GameManager.Instance )
			{
				GameManager.SetInGameAs( true );
			}

			GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, true);

			// Wait for every launched coroutine
			yield return CoroutinesManager.WaitPendingCoroutines();
		}
		Loading.EndSubTask();
		Loading.Hide();

		// Leave to UIManager the decision on which UI menu must be shown
		yield return UIManager.Instance.EnableMenuByScene( loadSceneData.eScene );

		yield return null;

		Loading.SetSubTask( "Completed" );
		
		Time.fixedDeltaTime = currentFixedDeltaTime;
		Physics.autoSimulation = true;
		Time.timeScale = 1.0F;

		loadWatch.Stop();
		Debug.LogFormat("Loading {0} took {1}ms.", loadSceneData.eScene, loadWatch.ElapsedMilliseconds);
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
	public	static	void	RegisterOnLoad( System.Action<ESceneEnumeration> onSceneLoad, ESceneLoadStep step )
	{
		Delegates[step].Add( onSceneLoad );
	}



	//
	public	static	void	UnregisterOnLoad( System.Action<ESceneEnumeration> onSceneLoad, ESceneLoadStep step )
	{
		Delegates[step].Remove( onSceneLoad );
	}
	
}
