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
	private		float				m_CurrentProgressValue				= 0.0f;

	private		bool				m_IsInitializedInternal				= true;



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

			Object.Instantiate( original ).Awake();

			m_IsInitialized = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		// Singleton
		if ( m_Instance != null && m_Instance != this )
		{
			Destroy( gameObject );
			return;
		}
		m_Instance = this;
		DontDestroyOnLoad(this);

		m_IsInitializedInternal &= transform.SearchComponent( ref m_LoadingBar, SearchContext.CHILDREN );
		m_IsInitializedInternal &= transform.SearchComponent( ref m_LoadingLevelNameText, SearchContext.CHILDREN, c => c.name == "LoadingSceneNameText" );
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		if ( m_Instance != this )
			return;

		m_IsInitialized = false;
	}


	/*
	//////////////////////////////////////////////////////////////////////////
	private	void	Awake()
	{
		// Singleton
		if ( m_Instance != null )
		{
			Destroy(gameObject);
			return;
		}

		m_IsInitializedInternal = transform.SearchComponent( ref m_LoadingBar, SearchContext.CHILDREN );
		m_IsInitializedInternal &= transform.SearchComponent( ref m_LoadingLevelNameText, SearchContext.CHILDREN, c => c.name == "LoadingSceneNameText" );
	}
	*/

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



	//////////////////////////////////////////////////////////////////////////
	public	static	void	SetProgress( float CurrentProgress )
	{
//		m_LoadingBar.value = CurrentProgress;
		m_Instance.m_CurrentProgressValue = CurrentProgress;
	}



	//////////////////////////////////////////////////////////////////////////
	private void LateUpdate()
	{
		m_Instance.m_LoadingBar.value = Mathf.MoveTowards( m_Instance.m_LoadingBar.value, m_Instance.m_CurrentProgressValue, Time.unscaledDeltaTime );
	}
}
