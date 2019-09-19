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
		public	SceneEnumeration	eScene				= SceneEnumeration.NONE;
		public	SceneEnumeration	iPrevSceneIdx			= SceneEnumeration.PREVIOUS;
		public	bool				bMustLoadSave			= false;
		public	string				sSaveToLoad				= "";
		public	System.Action		pOnPreLoadCompleted		= delegate { };
		public	System.Action		pOnLoadCompleted		= delegate { };
	}


	private	static	CustomSceneManager	m_Instance					= null;
	private	static	bool				m_IsInitialized				= false;


	private	static	List< UnityAction<Scene, LoadSceneMode> > Delegates = new List<UnityAction<Scene, LoadSceneMode>>();
	

	/////////////////////////////////////////////////////////////////
	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
	private	static	void	Initialize()
	{
		if ( m_IsInitialized == false )
		{
			m_Instance = FindObjectOfType<CustomSceneManager>();
			if ( m_Instance == null )
			{
				m_Instance = new GameObject("CustomSceneManager").AddComponent<CustomSceneManager>();
			}
			m_Instance.hideFlags = HideFlags.DontSave;
			m_IsInitialized = true;

			DontDestroyOnLoad( m_Instance );

			Delegates.ForEach( d => SceneManager.sceneLoaded -= d );

			m_IsInitialized = true;
		}
	}



	/////////////////////////////////////////////////////////////////
	private void Awake()
	{
		// Singleton
		if ( m_Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		m_Instance = this;
		m_IsInitialized = true;

		Delegates.ForEach( d => SceneManager.sceneLoaded -= d );
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		if ( m_Instance != this )
			return;

		m_IsInitialized = false;
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

		IEnumerator enumerator = m_Instance.PreloadCO( SceneIdx, preloadSceneData );
		CoroutinesManager.Start( enumerator, "CustomSceneManager::Preload: Preloading scene " + SceneIdx );
		return enumerator;
	}



	/// <summary> Complete the load of a èrevious preloaded scene </summary>
	public	static	IEnumerator	CompleteSceneAsyncLoad( PreloadSceneData preloadSceneData )
	{
		IEnumerator enumerator = m_Instance.CompleteSceneAsyncLoadCO( preloadSceneData );
		CoroutinesManager.Start( enumerator, "CustomSceneManager::CompleteSceneAsyncLoad: Completing load of " + preloadSceneData.eScene );
		return enumerator;
	}



	/// <summary> Launch load of a scene asynchronously </summary>
	public	static IEnumerator	LoadSceneAsync( LoadSceneData loadSceneData, LoadCondition loadCondition = null )
	{
		if ( HasGotValidLoadScenData( loadSceneData ) == false )
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
		preloadSceneData.eScene = SceneIdx;

		// Wait for load completion
		while ( asyncOperation.progress < 0.9f )
		{
			yield return null;
		}

		print("Preload comleted");

	}



	/// <summary> EXPERIMENTAL: Internal coroutine that complete the load a preloaded scene </summary>
	private	IEnumerator	CompleteSceneAsyncLoadCO( PreloadSceneData preloadSceneData )
	{
		yield return new WaitForEndOfFrame();
		preloadSceneData.asyncOperation.allowSceneActivation = true;

		// Wait for start completion
		while ( preloadSceneData.asyncOperation.isDone == false )
		{
			yield return null;
		}

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

		// Enable Loading Menu
//		UIManager.Instance.GoToMenu( Loading );
		Loading.Show();

		// Load Loading Scene syncronously
		{
			LoadSceneData loadingLoadSceneData = new LoadSceneData()
			{
				eScene			= SceneEnumeration.LOADING,
			};
			LoadSceneSync( loadingLoadSceneData );
		}

		yield return null;

		// Set global state as ChangingScene state
		GlobalManager.bIsChangingScene = true;
		
		// Start async load of scene
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync( (int)loadSceneData.eScene, LoadSceneMode.Single );

		// We want this operation to impact performance less than possible
		asyncOperation.priority = 0;

		asyncOperation.allowSceneActivation = false;

		// Wait for load completion
		while ( asyncOperation.progress < 0.90f )
		{
			Loading.SetProgress( asyncOperation.progress * 0.5f );
			yield return null;
		}

		Loading.SetProgress( 0.60f );
		if ( loadCondition.IsNotNull() )
		{
			yield return loadCondition.WaitForPendingOperations();
		}

		Scene scene  = SceneManager.GetSceneByBuildIndex( (int)loadSceneData.eScene );

//		GameObject[] roots = scene.GetRootGameObjects();

//		System.Array.ForEach( roots, ( GameObject go ) => {
//			go.BroadcastMessage( "OnBeforeSceneActivation", SendMessageOptions.DontRequireReceiver );
//		} );


		// Wait for every launched coroutine in awake of scripts
		yield return CoroutinesManager.WaitPendingCoroutines();

		Time.timeScale = 0F;

		asyncOperation.allowSceneActivation = true;

		Loading.SetProgress( 0.70f );

		// Wait for start completion
		while ( asyncOperation.isDone == false )
		{
			yield return null;
		}
		
		// Pre load callback
		Loading.SetProgress( 0.80f );
		loadSceneData.pOnPreLoadCompleted();

		yield return null;

		// Wait for every launched coroutine in awake of scripts
		yield return CoroutinesManager.WaitPendingCoroutines();

		SoundManager.Instance.OnSceneLoaded();
		
		yield return null;

		// LOAD DATA
		if ( loadSceneData.bMustLoadSave == true )
		{
			Loading.SetProgress( 0.95f );
			GameManager.StreamEvents.Load( loadSceneData.sSaveToLoad );
		}

		// Wait for every launched coroutine in awake of scripts
		yield return CoroutinesManager.WaitPendingCoroutines();


//		yield return Resources.UnloadUnusedAssets();
//		System.GC.Collect();

		// Post load callback
		Loading.SetProgress( 0.95f );
		loadSceneData.pOnLoadCompleted();

		yield return null;

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

		// Wait for every launched coroutine in awake of scripts
		yield return CoroutinesManager.WaitPendingCoroutines();

//		yield return new WaitForSecondsRealtime( 3.00f );

		Loading.Hide();

		// Leave to UIManager the decision on which UI menu must be shown
		UIManager.Instance.EnableMenuByScene( loadSceneData.eScene );

		yield return null;

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
