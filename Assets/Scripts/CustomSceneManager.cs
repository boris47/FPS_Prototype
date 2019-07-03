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
//	LOADING		= 1,
	MAIN_MENU	= 1,
	OPENWORLD1	= 2,
	OPENWORLD2	= 3,
	OPENWORLD3	= 4,
	ENDING		= 5,
	COUNT
}

public class CustomSceneManager : MonoBehaviour {

	public class LoadSceneData {
		public	SceneEnumeration	iSceneIdx				= SceneEnumeration.NONE;
		public	LoadSceneMode		eLoadMode				= LoadSceneMode.Single;
		public	bool				bMustLoadSave			= false;
		public	string				sSaveToLoad				= "";
		public	System.Action		pOnPreLoadCompleted		= null;
		public	System.Action		pOnLoadCompleted		= null;
	}



	private	static	Dictionary<string, UnityAction<Scene, LoadSceneMode> > m_RegisteredDelegates = new Dictionary<string, UnityAction<Scene, LoadSceneMode>>();

	private	static	CustomSceneManager		m_Instance = null;

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


	private	static	bool	HasGotValidLoadScenData( LoadSceneData loadSceneData )
	{
		if ( loadSceneData == null )
			return false;

		if ( loadSceneData.iSceneIdx == SceneEnumeration.NONE )
			return false;

		if ( loadSceneData.iSceneIdx == SceneEnumeration.NEXT )
		{
			loadSceneData.iSceneIdx = (SceneEnumeration)SceneManager.GetActiveScene().buildIndex + 1;
		}

		if ( loadSceneData.iSceneIdx == SceneEnumeration.PREVIOUS )
		{
			loadSceneData.iSceneIdx = (SceneEnumeration)SceneManager.GetActiveScene().buildIndex - 1;
		}

		if ( (int)loadSceneData.iSceneIdx == SceneManager.GetActiveScene().buildIndex )
			return false;

		if ( loadSceneData.iSceneIdx < 0 || (int)loadSceneData.iSceneIdx >= SceneManager.sceneCountInBuildSettings )
			return false;

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

		SceneManager.LoadScene( (int)loadSceneData.iSceneIdx, loadSceneData.eLoadMode );

		// Remove global state as ChangingScene state
		GlobalManager.bIsChangingScene = false;

		// Pre load callback
		if ( loadSceneData.pOnPreLoadCompleted != null )
		{
			loadSceneData.pOnPreLoadCompleted();
		}

		// LOAD DATA
		{
			SoundManager.Instance.OnSceneLoaded();
			if ( loadSceneData.bMustLoadSave == true )
			{
				GameManager.StreamEvents.Load( loadSceneData.sSaveToLoad );
			}
		}

		// Post load callback
		if ( loadSceneData.pOnLoadCompleted != null )
		{
			loadSceneData.pOnLoadCompleted();
		}
	}



	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////		ASYNC		/////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////


	public class LoadConditionVerified {
		public	bool	bHasToWait = false;
	}


	/// <summary> Launch load of a scene asynchronously </summary>
	public	static void	LoadSceneAsync( LoadSceneData loadSceneData, LoadConditionVerified loadCondition = null )
	{
		if ( HasGotValidLoadScenData( loadSceneData ) == false )
			return;

		m_Instance.StartCoroutine( m_Instance.LoadSceneAsyncCO( loadSceneData, loadCondition ) );
	}



	/// <summary> Internla coroutine that load a scene asynchronously </summary>
	private	IEnumerator	LoadSceneAsyncCO( LoadSceneData loadSceneData, LoadConditionVerified loadCondition = null )
	{
		yield return new WaitForEndOfFrame();

		// Set global state as ChangingScene state
		GlobalManager.bIsChangingScene = true;
		
		// Start async load of scene
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync( (int)loadSceneData.iSceneIdx, loadSceneData.eLoadMode );
		asyncOperation.allowSceneActivation = false;

		// Wait for load completion
		while ( asyncOperation.progress < 0.9f )
		{
//			print( "Process: " + asyncOperation.progress );
			yield return null;
		}

//		print("before isdone");

		yield return new WaitUntil( () => loadCondition != null ? (loadCondition.bHasToWait == false) : true );

		asyncOperation.allowSceneActivation = true;

		// Wait for start completion
		while ( asyncOperation.isDone == false )
		{
			yield return null;
		}

//		print("after isdone");
		
		// Remove global state as ChangingScene state
		GlobalManager.bIsChangingScene = false;
		GlobalManager.bIsLoadingScene = true;
		
		// Pre load callback
		if ( loadSceneData.pOnPreLoadCompleted != null )
		{
			loadSceneData.pOnPreLoadCompleted();
		}
		yield return new WaitForEndOfFrame();

		// LOAD DATA
		{
			SoundManager.Instance.OnSceneLoaded();
			if ( loadSceneData.bMustLoadSave == true )
			{
				GameManager.StreamEvents.Load( loadSceneData.sSaveToLoad );
			}
		}

		// Wait for every launched coroutine in awake of scripts
		yield return CoroutinesManager.WaitPendingCoroutines();

		// Post load callback
		if ( loadSceneData.pOnLoadCompleted != null )
		{
			loadSceneData.pOnLoadCompleted();
		}
		yield return new WaitForEndOfFrame();

		GlobalManager.bIsLoadingScene = false;
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
