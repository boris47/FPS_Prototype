﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


[System.Serializable]
public	enum SceneEnumeration {
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
public	enum SceneLoadStep {
	BEFORE_SCENE_ACTIOVATION,
	AFTER_SCENE_ACTIVATION,
	AFTER_SAVE_LOAD
}



public class CustomSceneManager : SingletonMonoBehaviour<CustomSceneManager> {

	public	static int	CurrentSceneIndex
	{
		get { return SceneManager.GetActiveScene().buildIndex; }
	}

	public class LoadSceneData {
		public	SceneEnumeration	eScene				= SceneEnumeration.NONE;
		public	SceneEnumeration	iPrevSceneIdx			= SceneEnumeration.PREVIOUS;
		public	bool				bMustLoadSave			= false;
		public	string				sSaveToLoad				= "";
		public	System.Action		pOnPreLoadCompleted		= delegate { };
		public	System.Action		pOnLoadCompleted		= delegate { };
	}


	private	static	bool				m_IsCurrentlyPreloading		= false;
	private	static	bool				m_HasPreloadedScene			= false;


	private	static	Dictionary< SceneLoadStep, List<System.Action<SceneEnumeration>> > Delegates = new Dictionary<SceneLoadStep, List<System.Action<SceneEnumeration>>>()
	{
		{ SceneLoadStep.BEFORE_SCENE_ACTIOVATION, new List<System.Action<SceneEnumeration>>() },
		{ SceneLoadStep.AFTER_SCENE_ACTIVATION, new List<System.Action<SceneEnumeration>>() },
		{ SceneLoadStep.AFTER_SAVE_LOAD, new List<System.Action<SceneEnumeration>>() }
	};

	//	private	static	List< System.Action<KeyValuePair< SceneLoadStep, Scene>> > Delegates = new List<System.Action<KeyValuePair<SceneLoadStep, Scene>>>();


	/////////////////////////////////////////////////////////////////
	protected override void OnBeforeSplashScreen()
	{
		Delegates[SceneLoadStep.BEFORE_SCENE_ACTIOVATION].Clear();
		Delegates[SceneLoadStep.AFTER_SCENE_ACTIVATION].Clear();
		Delegates[SceneLoadStep.AFTER_SAVE_LOAD].Clear();

	}



	/////////////////////////////////////////////////////////////////
	private	static	bool	HasGotValidLoadScenData( LoadSceneData loadSceneData )
	{
		if ( loadSceneData == null )
			return false;

		if ( loadSceneData.eScene == SceneEnumeration.NONE )
			return false;

		// Check for save existance
		if ( loadSceneData.bMustLoadSave && System.IO.File.Exists( loadSceneData.sSaveToLoad ) == false )
		{
			return false;
		}

		int currentSceneIdx = CurrentSceneIndex;

		// Requesting to go to next scene
		if ( loadSceneData.eScene == SceneEnumeration.NEXT )
		{
			loadSceneData.eScene = (SceneEnumeration)( currentSceneIdx + 1 );
			if ( (int)loadSceneData.eScene >= SceneManager.sceneCountInBuildSettings )
				return false;
		}

		// Requesting to go to previous scene
		if ( loadSceneData.eScene == SceneEnumeration.PREVIOUS )
		{
			loadSceneData.eScene = (SceneEnumeration)( currentSceneIdx - 1);
			if ( loadSceneData.eScene < 0 )
				return false;
		}

		if ( (int)loadSceneData.eScene == currentSceneIdx )
			return false;

		loadSceneData.iPrevSceneIdx = (SceneEnumeration)( currentSceneIdx );

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

		// Pre load callback
		loadSceneData.pOnPreLoadCompleted();

		SoundManager.Instance.OnSceneLoaded();

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

	
	public class LoadCondition {

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
			while( bHasToWait )
				yield return null;

			if ( pCoroutineToWait.IsNotNull() )
				yield return pCoroutineToWait;

		}
	}



	public class PreloadSceneData {
		public	AsyncOperation		asyncOperation	= null;
		public	SceneEnumeration	eScene			= SceneEnumeration.NONE;
	}




	/// <summary> EXPERIMENTAL: Preload a scene and return into the second argument the AsyncOperation that manage that load </summary>
	public	static	IEnumerator Preload ( SceneEnumeration SceneIdx, PreloadSceneData preloadSceneData )
	{
		Debug.Log( "Preloading of scene " + SceneIdx );

		m_IsCurrentlyPreloading = true;

		IEnumerator enumerator = Instance.PreloadCO( SceneIdx, preloadSceneData );
		CoroutinesManager.Start( enumerator, "CustomSceneManager::Preload: Preloading scene " + SceneIdx );
		return enumerator;
	}



	/// <summary> Complete the load of a èrevious preloaded scene </summary>
	public	static	IEnumerator	CompleteSceneAsyncLoad( PreloadSceneData preloadSceneData )
	{
		IEnumerator enumerator = Instance.CompleteSceneAsyncLoadCO( preloadSceneData );
		CoroutinesManager.Start( enumerator, "CustomSceneManager::CompleteSceneAsyncLoad: Completing load of " + preloadSceneData.eScene );
		return enumerator;
	}



	/// <summary> Launch load of a scene asynchronously </summary>
	public	static IEnumerator	LoadSceneAsync( LoadSceneData loadSceneData, LoadCondition loadCondition = null )
	{
		if ( HasGotValidLoadScenData( loadSceneData ) == false )
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
	private	IEnumerator PreloadCO( SceneEnumeration SceneIdx, PreloadSceneData preloadSceneData )
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

		print("Preload comleted");
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
		while ( preloadSceneData.asyncOperation.isDone == false )
		{
			yield return null;
		}

		m_HasPreloadedScene = false;

		SoundManager.Instance.OnSceneLoaded();

		GlobalManager.bIsLoadingScene = false;

		UIManager.Indicators.enabled = true;
		UIManager.Minimap.enabled = true;

		GlobalManager.InputMgr.EnableCategory( InputCategory.ALL );

		if ( GameManager.Instance )
			GameManager.SetInGameAs( true );

		if ( CameraControl.Instance.IsNotNull() )
			CameraControl.Instance.CanParseInput = true;

		// Leave to UIManager the decision on which UI menu must be shown
		UIManager.Instance.EnableMenuByScene( preloadSceneData.eScene );
	}



	/// <summary> Internal coroutine that load a scene asynchronously </summary>
	private	IEnumerator	LoadSceneAsyncCO( LoadSceneData loadSceneData, LoadCondition loadCondition = null )
	{
		// Wait for every launched coroutine in awake of scripts
		yield return CoroutinesManager.WaitPendingCoroutines();

		Loading.SetLoadingSceneName( loadSceneData.eScene );
		Loading.SetProgress( 0.0f );

		GlobalManager.bIsLoadingScene = true;

		// Disable all input categories
		GlobalManager.InputMgr.DisableCategory( InputCategory.ALL );

		// Enable Loading UI
		Loading.Show();
		
		Loading.SetSubTask( "Loading 'Loading' Level" );

		// Load Synchronously Loading Scene syncronously
		LoadSceneData loadingLoadSceneData = new LoadSceneData()
		{
			eScene = SceneEnumeration.LOADING,
		};
		LoadSceneSync( loadingLoadSceneData );
		
		yield return null;

		// Set global state as ChangingScene state
		GlobalManager.bIsChangingScene = true;
		
		Loading.SetSubTask( "Start async Loading" );

		// Start async load of scene
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync( (int)loadSceneData.eScene, LoadSceneMode.Single );

		// We want this operation to impact performance less than possible
//		asyncOperation.priority = 0;

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
			Delegates[SceneLoadStep.BEFORE_SCENE_ACTIOVATION].ForEach( d => d(loadSceneData.eScene) );
		}

		Loading.SetSubTask( "Waiting for loading condition" );

		// Wait for load condition if defined
		Loading.SetProgress( 0.60f );
		if ( loadCondition.IsNotNull() )
		{
			yield return loadCondition.WaitForPendingOperations();
		}

		Loading.SetSubTask( "1. Waiting for pending coroutines" );

		// Wait for every launched coroutine
		yield return CoroutinesManager.WaitPendingCoroutines();

		// Setting the time scale to Zero because in order to freeze everything but continue to receive unity messages
		Time.timeScale = 0F;

		Loading.SetSubTask( "Activation of the scene" );

		// Proceed with scene activation and Awake Calls
		asyncOperation.allowSceneActivation = true;

		Loading.SetSubTask( "Calling 'OnAfterSceneActivation' on receivers" );

		// Call on every registered
		Delegates[SceneLoadStep.AFTER_SCENE_ACTIVATION].ForEach( d => d(loadSceneData.eScene) );

		// Wait for start completion
		Loading.SetProgress( 0.70f );
		while ( asyncOperation.isDone == false )
		{
			yield return null;
		}

		Loading.SetSubTask( "2. Waiting for pending coroutines" );

		// Wait for every launched coroutine
		yield return CoroutinesManager.WaitPendingCoroutines();
		
		Loading.SetSubTask( "Calling 'pOnPreLoadCompleted' callback" );

		// Pre load callback
		Loading.SetProgress( 0.80f );
		loadSceneData.pOnPreLoadCompleted();

		yield return null;

		Loading.SetSubTask( "3. Waiting for pending coroutines" );

		// Wait for every launched coroutine in awake of scripts
		yield return CoroutinesManager.WaitPendingCoroutines();

		Loading.SetSubTask( "Calling 'SoundManager.OnSceneLoaded'" );

		SoundManager.Instance.OnSceneLoaded();
		
		yield return null;

		// LOAD DATA
		if ( loadSceneData.bMustLoadSave == true )
		{
			Loading.SetSubTask( "Going to load save " + loadSceneData.sSaveToLoad );
			Loading.SetProgress( 0.95f );
			GameManager.StreamEvents.Load( loadSceneData.sSaveToLoad );
		}

		Loading.SetSubTask( "4. Waiting for pending coroutines" );

		// Wait for every coroutines started from load
		yield return CoroutinesManager.WaitPendingCoroutines();


		Loading.SetSubTask( "Calling 'OnAfterLoadedData' on receivers" );

		// Call on every registered
		Delegates[SceneLoadStep.AFTER_SAVE_LOAD].ForEach( d => d(loadSceneData.eScene) );

		Loading.SetSubTask( "Waitning for the unload of unused assets" );

		// Unload unused asset in order to free same memory
//		yield return Resources.UnloadUnusedAssets();

		Loading.SetSubTask( "GC..." );

		System.GC.Collect();

		Loading.SetSubTask( "Calling 'pOnLoadCompleted' callback" );

		// Post load callback
		Loading.SetProgress( 0.95f );
		loadSceneData.pOnLoadCompleted();

		yield return null;

		Loading.SetSubTask( "Enabling components" );

		GlobalManager.bIsLoadingScene = false;
		Loading.SetProgress( 1.00f );
		UIManager.Indicators.enabled = true;
		UIManager.Minimap.enabled = true;

		yield return null;

		GlobalManager.InputMgr.EnableCategory( InputCategory.ALL );

		if ( GameManager.Instance )
			GameManager.SetInGameAs( true );

		if ( CameraControl.Instance.IsNotNull() )
			CameraControl.Instance.CanParseInput = true;

		// Wait for every launched coroutine
		yield return CoroutinesManager.WaitPendingCoroutines();

//		yield return new WaitForSecondsRealtime( 3.00f );

		Loading.Hide();

		// Leave to UIManager the decision on which UI menu must be shown
		UIManager.Instance.EnableMenuByScene( loadSceneData.eScene );

		yield return null;

		Loading.SetSubTask( "Completed" );

		Time.timeScale = 1.0F;
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
	public	static	void	RegisterOnLoad( System.Action<SceneEnumeration> onSceneLoad, SceneLoadStep step )
	{
		Delegates[step].Add( onSceneLoad );
	}



	//
	public	static	void	UnregisterOnLoad( System.Action<SceneEnumeration> onSceneLoad, SceneLoadStep step )
	{
		Delegates[step].Remove( onSceneLoad );
	}
	
}
