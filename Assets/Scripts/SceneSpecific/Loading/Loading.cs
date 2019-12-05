using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour {

	private		const	string		RESOURCE_PATH						= "Prefabs/Loading";

	private		static	Loading		m_Instance							= null;
	private		static	bool		m_IsInitialized						= false;


	private		Slider				m_LoadingBar						= null;
	private		Text				m_LoadingLevelNameText				= null;
	private		Text				m_LoadingSubTask					= null;
	private		float				m_CurrentProgressValue				= 0.0f;

	private		bool				m_IsInitializedInternal				= false;



	///////////////////////////////////////////////////
	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
	private	static	void	Initialize()
	{
		if ( m_IsInitialized == false )
		{
			Loading original = Resources.Load<Loading>( RESOURCE_PATH );
			UnityEngine.Assertions.Assert.IsNotNull
			(
				original,
				RESOURCE_PATH + " cannot be loaded !!!"
			);

			m_Instance = Object.Instantiate( original );

			DontDestroyOnLoad( m_Instance );

			m_IsInitialized = true;
		}
	}

	
	//////////////////////////////////////////////////////////////////////////
	private	void	Awake()
	{
		// Singleton
		if ( m_Instance != null )
		{
			Destroy(gameObject);
			return;
		}
		
		gameObject.SetActive(false);

		m_IsInitializedInternal = transform.SearchComponent( ref m_LoadingBar, SearchContext.CHILDREN );
		m_IsInitializedInternal &= transform.SearchComponent( ref m_LoadingLevelNameText, SearchContext.CHILDREN, c => c.name == "LoadingSceneName" );
		m_IsInitializedInternal &= transform.SearchComponent( ref m_LoadingSubTask, SearchContext.CHILDREN, c => c.name == "LoadingSubTask" );
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		if ( m_Instance != this )
			return;

		m_IsInitialized = false;
		m_Instance = null;
	}


	//////////////////////////////////////////////////////////////////////////
	public	static	void	Show()
	{
		ResetBar();

		m_Instance.gameObject.SetActive(true);
	}



	//////////////////////////////////////////////////////////////////////////
	public	static	void	Hide()
	{
		ResetBar();

		m_Instance.gameObject.SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	private static	void ResetBar()
	{
		m_Instance.m_CurrentProgressValue = 0.0f;
		m_Instance.m_LoadingBar.value = 0.0f;
	}



	//////////////////////////////////////////////////////////////////////////
	public	static	void	SetLoadingSceneName( SceneEnumeration scene )
	{
		m_Instance.m_LoadingLevelNameText.text = "Loading: " + scene.ToString();
	}

	private static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
	//////////////////////////////////////////////////////////////////////////
	public	static	void	EndSubTask()
	{
		stopWatch.Stop();
		Debug.Log("Step \"" + m_Instance.m_LoadingSubTask.text + "\" required " + stopWatch.ElapsedMilliseconds + "ms" );
	}


	//////////////////////////////////////////////////////////////////////////
	public	static	void	SetSubTask( string subTaskName )
	{
		stopWatch.Reset(); stopWatch.Start();
		m_Instance.m_LoadingSubTask.text = subTaskName;
		Debug.Log( subTaskName );
	}



	//////////////////////////////////////////////////////////////////////////
	public	static	void	SetProgress( float CurrentProgress )
	{
		m_Instance.m_CurrentProgressValue = Mathf.Clamp01( CurrentProgress );
	}



	//////////////////////////////////////////////////////////////////////////
	private void LateUpdate()
	{
		m_Instance.m_LoadingBar.value = Mathf.MoveTowards( m_Instance.m_LoadingBar.value, m_Instance.m_CurrentProgressValue, Time.unscaledDeltaTime );
	}
}
