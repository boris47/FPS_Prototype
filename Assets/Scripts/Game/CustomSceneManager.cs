using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class CustomSceneManager : MonoBehaviour {

	private	static Dictionary<string, UnityAction<Scene, LoadSceneMode> > map
		= new Dictionary<string, UnityAction<Scene, LoadSceneMode>>();

	private	static	CustomSceneManager		m_Instance = null;

	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void OnBeforeSceneLoad ()
	{
		print( "CustomSceneManager::OnBeforeSceneLoad" );

	}

	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.AfterSceneLoad)]
	static void AfterSceneLoad ()
	{
		print( "CustomSceneManager::AfterSceneLoad" );
	}

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
	}

	public class LoadSceneData {
		public	int				iSceneIdx				= -1;
		public	LoadSceneMode	eLoadMode				= LoadSceneMode.Single;
		public	bool			bMustLoadSave			= false;
		public	string			sSaveToLoad				= "";
		public	System.Action	pOnPreLoadCompleted		= null;
		public	System.Action	pOnLoadCompleted		= null;
	}


	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////		SYNC		/////////////////////////
	/////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////

	/// <summary> Parse the load request synchronously </summary>
	public	static void	LoadSceneSync( LoadSceneData loadSceneData )
	{
		if ( loadSceneData == null )
			return;

		if ( loadSceneData.iSceneIdx == -1 )
			return;

		if ( loadSceneData.iSceneIdx == SceneManager.GetActiveScene().buildIndex )
			return;

		InternalLoadSceneSync( loadSceneData );
	}

	/// <summary> Internally load the scene synchronously </summary>
	private	static void InternalLoadSceneSync( LoadSceneData loadSceneData )
	{
		// Set global state as ChangingScene state
		GlobalManager.bIsChangingScene = true;

		SceneManager.LoadScene( loadSceneData.iSceneIdx, loadSceneData.eLoadMode );

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


	/// <summary> Launch load of a scene asynchronously </summary>
	public	static void	LoadSceneAsync( LoadSceneData loadSceneData )
	{
		if ( loadSceneData == null )
			return;

		if ( loadSceneData.iSceneIdx == 0 )
			return;

		if ( loadSceneData.iSceneIdx == SceneManager.GetActiveScene().buildIndex )
			return;

		m_Instance.StartCoroutine( m_Instance.LoadSceneAsyncCO( loadSceneData ) );
	}


	/// <summary> Internla coroutine that load a scene asynchronously </summary>
	private	IEnumerator	LoadSceneAsyncCO( LoadSceneData loadSceneData )
	{
		yield return new WaitForEndOfFrame();

		// Set global state as ChangingScene state
		GlobalManager.bIsChangingScene = true;
		
		// Start async load of scene
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync( loadSceneData.iSceneIdx, loadSceneData.eLoadMode );
		asyncOperation.allowSceneActivation = false;

		// Wait for load completion
		while ( asyncOperation.progress < 0.9f )
		{
//			print( "Process: " + asyncOperation.progress );
			yield return null;
		}

//		print("before isdone");

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
	public	static	void	RegisterOnLoad( System.Action<int> activeSceneChanged )
	{
		UnityAction<Scene, LoadSceneMode> OnLoad = delegate( Scene scene, LoadSceneMode mode )
		{
			activeSceneChanged( scene.buildIndex );
		};

		string id = activeSceneChanged.Method.Name;
		map.Add( id, OnLoad );
		SceneManager.sceneLoaded += OnLoad;
	}


	//
	public	static	void	UnregisterOnLoad( System.Action activeSceneChanged )
	{
		string id = activeSceneChanged.Method.Name;

		UnityAction<Scene, LoadSceneMode> OnLoad = null;
		if ( map.TryGetValue( id, out OnLoad ) )
		{
			SceneManager.sceneLoaded -= OnLoad;
		}
	}

	
}
