using System.Collections;
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



public class CustomSceneManager : MonoBehaviour {

	public	static int	CurrentSceneIndex
	{
		get { return SceneManager.GetActiveScene().buildIndex; }
	}

	public class LoadSceneData {
		public	SceneEnumeration	iSceneIdx				= SceneEnumeration.NONE;
		public	SceneEnumeration	iPrevSceneIdx			= SceneEnumeration.PREVIOUS;
		public	bool				bMustLoadSave			= false;
		public	string				sSaveToLoad				= "";
		public	System.Action		pOnPreLoadCompleted		= delegate { };
		public	System.Action		pOnLoadCompleted		= delegate { };
	}


	private	static	CustomSceneManager		m_Instance = null;
	private	static	Dictionary<string, UnityAction<Scene, LoadSceneMode> > m_RegisteredDelegates = new Dictionary<string, UnityAction<Scene, LoadSceneMode>>();
	private	static	List< UnityAction<Scene, LoadSceneMode> > Delegates = new List<UnityAction<Scene, LoadSceneMode>>();
	

	/////////////////////////////////////////////////////////////////
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

		Delegates.ForEach( d => SceneManager.sceneLoaded -= d );
	}



	/////////////////////////////////////////////////////////////////
	private	static	bool	HasGotValidLoadScenData( LoadSceneData loadSceneData )
	{
		if ( loadSceneData == null )
			return false;

		if ( loadSceneData.iSceneIdx == SceneEnumeration.NONE )
			return false;

		// Check for save existance
		if ( loadSceneData.bMustLoadSave && System.IO.File.Exists( loadSceneData.sSaveToLoad ) == false )
		{
			return false;
		}

		int currentSceneIdx = CurrentSceneIndex;

		// Requesting to go to next scene
		if ( loadSceneData.iSceneIdx == SceneEnumeration.NEXT )
		{
			loadSceneData.iSceneIdx = (SceneEnumeration)( currentSceneIdx + 1 );
			if ( (int)loadSceneData.iSceneIdx >= SceneManager.sceneCountInBuildSettings )
				return false;
		}

		// Requesting to go to previous scene
		if ( loadSceneData.iSceneIdx == SceneEnumeration.PREVIOUS )
		{
			loadSceneData.iSceneIdx = (SceneEnumeration)( currentSceneIdx - 1);
			if ( loadSceneData.iSceneIdx < 0 )
				return false;
		}

		if ( (int)loadSceneData.iSceneIdx == currentSceneIdx )
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

		SceneManager.LoadScene( (int)loadSceneData.iSceneIdx, LoadSceneMode.Single );

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
		public AsyncOperation		asyncOperation	= null;
		public SceneEnumeration		sceneIdx		= SceneEnumeration.NONE;
	}




	/// <summary> EXPERIMENTAL: Preload a scene and return into the second argument the AsyncOperation that manage that load </summary>
	public	static	IEnumerator Preload ( SceneEnumeration SceneIdx, PreloadSceneData preloadSceneData )
	{
		Debug.Log( "Preloading of scene " + SceneIdx );

		IEnumerator enumerator = m_Instance.PreloadCO( SceneIdx, preloadSceneData );
		CoroutinesManager.Start( enumerator, "CustomSceneManager::Preload: Preloading scene " + SceneIdx );
		return enumerator;
	}



	/// <summary> Complete the load of a èrevious preloaded scene </summary>
	public	static	IEnumerator	CompleteSceneAsyncLoad( PreloadSceneData preloadSceneData )
	{
		IEnumerator enumerator = m_Instance.CompleteSceneAsyncLoadCO( preloadSceneData.asyncOperation );
		CoroutinesManager.Start( enumerator, "CustomSceneManager::CompleteSceneAsyncLoad: Completing load of " + preloadSceneData.sceneIdx );
		return enumerator;
	}



	/// <summary> Launch load of a scene asynchronously </summary>
	public	static IEnumerator	LoadSceneAsync( LoadSceneData loadSceneData, LoadCondition loadCondition = null )
	{
		if ( HasGotValidLoadScenData( loadSceneData ) == false )
			return null;

		IEnumerator enumerator = m_Instance.LoadSceneAsyncCO( loadSceneData, loadCondition );
		CoroutinesManager.Start( enumerator, "CustomSceneManager::LoadSceneAsync: Loading " + loadSceneData.iSceneIdx );
		return enumerator;
	}



	/// <summary> Start the unload of a scene, return the coroutine managing the async operation </summary>
	public	static	IEnumerator	UnLoadSceneAsync( int SceneIdx )
	{
		Scene scene =  SceneManager.GetSceneByBuildIndex( SceneIdx );
		if ( scene.isLoaded == false )
			return null;

	//	Debug.Log( "Unloading Scene " + scene.name );
		IEnumerator enumerator = m_Instance.UnLoadSceneAsyncCO( scene );
		CoroutinesManager.Start( enumerator, "CustomSceneManager::UnLoadSceneAsync:Async unload of " + scene.name );
		return enumerator;
	}



	/////////////////////////////////////////////////////////////////
	private	IEnumerator PreloadCO( SceneEnumeration SceneIdx, PreloadSceneData preloadSceneData )
	{
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync( (int)SceneIdx, LoadSceneMode.Additive );

		// We want this operation to impact performance less than possible
		asyncOperation.priority = 0;

		asyncOperation.allowSceneActivation = false;

		preloadSceneData.asyncOperation = asyncOperation;
		preloadSceneData.sceneIdx = SceneIdx;

		// Wait for load completion
		while ( asyncOperation.progress < 0.9f )
		{
			yield return null;
		}

//		while ( asyncOperation.isDone == false )
		{
			yield return null;
		}
	}



	/// <summary> EXPERIMENTAL: Internal coroutine that complete the load a preloaded scene </summary>
	private	IEnumerator	CompleteSceneAsyncLoadCO( AsyncOperation asyncOperation )
	{
		yield return new WaitForEndOfFrame();
		asyncOperation.allowSceneActivation = true;

		// Wait for start completion
		while ( asyncOperation.isDone == false )
		{
			yield return null;
		}

		yield return null;

		SoundManager.Instance.OnSceneLoaded();

		GlobalManager.bIsLoadingScene = false;
	}



	/// <summary> Internal coroutine that load a scene asynchronously </summary>
	private	IEnumerator	LoadSceneAsyncCO( LoadSceneData loadSceneData, LoadCondition loadCondition = null )
	{
		// Wait for every launched coroutine in awake of scripts
		yield return CoroutinesManager.WaitPendingCoroutines();

		GlobalManager.bIsLoadingScene = true;

		// Disable all input categories
		GlobalManager.Instance.InputMgr.DisableCategory( InputCategory.ALL );

		// Enable Loading Menu
		UIManager.Instance.GoToMenu( UIManager.Loading );

		// Load Loading Scene syncronously
		{
			LoadSceneData loadingLoadSceneData = new LoadSceneData()
			{
				iSceneIdx			= SceneEnumeration.LOADING,
			};
			LoadSceneSync( loadingLoadSceneData );
		}

		UIManager.Loading.SetProgress( 0.0f );

		yield return new WaitForEndOfFrame();

		// Set global state as ChangingScene state
		GlobalManager.bIsChangingScene = true;
		
		// Start async load of scene
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync( (int)loadSceneData.iSceneIdx, LoadSceneMode.Single );

		// We want this operation to impact performance less than possible
		asyncOperation.priority = 0;

		asyncOperation.allowSceneActivation = false;

		// Wait for load completion
		while ( asyncOperation.progress < 0.9f )
		{
			UIManager.Loading.SetProgress( asyncOperation.progress * 0.5f );
			yield return null;
		}

		UIManager.Loading.SetProgress( 0.60f );
		if ( loadCondition.IsNotNull() )
		{
			yield return loadCondition.WaitForPendingOperations();
		}

		Time.timeScale = 0F;

		asyncOperation.allowSceneActivation = true;

		UIManager.Loading.SetProgress( 0.70f );

		// Wait for start completion
		while ( asyncOperation.isDone == false )
		{
			yield return null;
		}
		
		// Pre load callback
		UIManager.Loading.SetProgress( 0.80f );
		loadSceneData.pOnPreLoadCompleted();

		// Wait for every launched coroutine in awake of scripts
		yield return CoroutinesManager.WaitPendingCoroutines();

		SoundManager.Instance.OnSceneLoaded();
		
		// LOAD DATA
		if ( loadSceneData.bMustLoadSave == true )
		{
			UIManager.Loading.SetProgress( 0.95f );
			GameManager.StreamEvents.Load( loadSceneData.sSaveToLoad );
		}

		// Wait for every launched coroutine in awake of scripts
		yield return CoroutinesManager.WaitPendingCoroutines();

		// Post load callback
		UIManager.Loading.SetProgress( 0.95f );

		loadSceneData.pOnLoadCompleted();

		yield return new WaitForEndOfFrame();

		GlobalManager.bIsLoadingScene = false;

		UIManager.Loading.SetProgress( 1.00f );

		// Wait for every launched coroutine in awake of scripts
		yield return CoroutinesManager.WaitPendingCoroutines();

		yield return new WaitForSecondsRealtime( 3.00f );

		UIManager.Instance.GoToMenu( UIManager.InGame );
		UIManager.Indicators.enabled = true;
		UIManager.Minimap.enabled = true;
		GlobalManager.Instance.InputMgr.EnableCategory( InputCategory.ALL );
		GameManager.Instance.SetInGameAs( true );
		CameraControl.Instance.CanParseInput = true;

		Time.timeScale = 1F;
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
	public	static	void	RegisterOnLoad( UnityAction<Scene, LoadSceneMode> activeSceneChanged )
	{
		SceneManager.sceneLoaded += activeSceneChanged;

		Delegates.Add( activeSceneChanged );
	}



	//
	public	static	void	UnregisterOnLoad( UnityAction<Scene, LoadSceneMode> activeSceneChanged )
	{
		SceneManager.sceneLoaded -= activeSceneChanged;

		Delegates.Remove( activeSceneChanged );
	}

	
}
