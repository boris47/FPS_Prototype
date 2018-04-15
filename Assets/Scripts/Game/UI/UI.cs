using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public interface IUI {

	UI_MainMenu		MainMenu			{ get; }
	UI_InGame		InGame				{ get; }

	void			LoadSceneByIdx		( int sceneIdx );

}


public class UI : MonoBehaviour, IUI {
	
	public	static	IUI				Instance				= null;

	private			UI_MainMenu		m_MainMenu				= null;
	private			UI_InGame		m_InGame				= null;


	// INTERFACE START
					UI_MainMenu		IUI.MainMenu			{ get { return m_MainMenu; } }
					UI_InGame		IUI.InGame				{ get { return m_InGame; } }
	// INTERFACE END


	private			AsyncOperation	m_AsyncOperation		= null;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		if ( Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		Instance = this;
		DontDestroyOnLoad( this );

		m_MainMenu	= GetComponentInChildren<UI_MainMenu>( includeInactive : true );
		m_InGame	= GetComponentInChildren<UI_InGame>(  includeInactive : true  );
	}


	//////////////////////////////////////////////////////////////////////////
	// LoadSceneByIdx
	public	void	LoadSceneByIdx( int sceneIdx )
	{
		UI.Instance.MainMenu.gameObject.SetActive( false );

		StartCoroutine( LoadSceneByIdxCO( sceneIdx ) );
	}


	//////////////////////////////////////////////////////////////////////////
	// LoadSceneByIdx ( Coroutine )
	private	IEnumerator	LoadSceneByIdxCO( int sceneIdx )
	{
		GameManager.IsChangingScene = true;

		m_AsyncOperation = SceneManager.LoadSceneAsync( sceneIdx );
		m_AsyncOperation.allowSceneActivation = false;

		while ( m_AsyncOperation.progress < 0.9f )
		{
			yield return null;
		}

		m_AsyncOperation.allowSceneActivation = true;

		UI.Instance.InGame.gameObject.SetActive( true );
		UI.Instance.InGame.Hide();

		GameManager.IsChangingScene = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnQuit
	public	void	OnQuit()
	{

#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif

	}

}
