using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : InGameSingleton<Loading>
{
	private		Slider				m_LoadingBar						= null;
	private		Text				m_LoadingLevelNameText				= null;
	private		Text				m_LoadingSubTask					= null;
	private		float				m_CurrentProgressValue				= 0.0f;

	private		bool				m_IsInitializedInternal				= false;

	//////////////////////////////////////////////////////////////////////////
	private	void	Awake()
	{
		gameObject.SetActive(false);

		m_IsInitializedInternal = transform.SearchComponent( ref m_LoadingBar, ESearchContext.CHILDREN );
		m_IsInitializedInternal &= transform.SearchChildWithName( "LoadingSceneName", ref m_LoadingLevelNameText );
		m_IsInitializedInternal &= transform.SearchChildWithName( "LoadingSubTask", ref m_LoadingSubTask );

		if ( !m_IsInitializedInternal )
		{
			Debug.LogError( "Loading Singleton has initialization issues" );
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = UnityEditor.EditorApplication.isPaused = false;
#endif
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	static	void	Show()
	{
		ResetBar();

		Instance.gameObject.SetActive(true);
	}



	//////////////////////////////////////////////////////////////////////////
	public	static	void	Hide()
	{
		ResetBar();

		Instance.gameObject.SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	private static	void ResetBar()
	{
		Instance.m_CurrentProgressValue = 0.0f;
		Instance.m_LoadingBar.value = 0.0f;
	}



	//////////////////////////////////////////////////////////////////////////
	public	static	void	SetLoadingSceneName( ESceneEnumeration scene )
	{
		Instance.m_LoadingLevelNameText.text = "Loading: " + scene.ToString();
	}

	private static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
	//////////////////////////////////////////////////////////////////////////
	public	static	void	EndSubTask()
	{
		stopWatch.Stop();
		Debug.LogFormat( "Step '{0}' required {1}ms.", Instance.m_LoadingSubTask.text, stopWatch.ElapsedMilliseconds);
	}


	//////////////////////////////////////////////////////////////////////////
	public	static	void	SetSubTask( string subTaskName )
	{
		stopWatch.Reset(); stopWatch.Start();
		Instance.m_LoadingSubTask.text = subTaskName;
		Debug.Log( subTaskName );
	}



	//////////////////////////////////////////////////////////////////////////
	public	static	void	SetProgress( float CurrentProgress )
	{
		Instance.m_CurrentProgressValue = Mathf.Clamp01( CurrentProgress );
	}



	//////////////////////////////////////////////////////////////////////////
	private void LateUpdate()
	{
		Instance.m_LoadingBar.value = Mathf.MoveTowards( Instance.m_LoadingBar.value, Instance.m_CurrentProgressValue, Time.unscaledDeltaTime );
	}

}
