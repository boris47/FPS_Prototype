using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GlobalManager : MonoBehaviour {

	private	static			GlobalManager	m_Instance				= null;
	public	static			GlobalManager	Instance
	{
		get { return m_Instance; }
	}

	// Load Settings and Configs
	private	const string settingspath		= "Settings";
	private	const string configsPath		= "Configs\\All";

	public	static			bool			bIsChangingScene			= false;
	public	static			bool			bIsLoadingScene				= false;
	public	static			bool			bCanSave					= true;


	private	static			SectionMap		m_Settings				= null;
	public	static			SectionMap		Settings
	{
		get { return m_Settings; }
	}


	private	static			SectionMap		m_Configs				= null;
	public	static			SectionMap		Configs
	{
		get { return m_Configs; }
	}

	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void OnBeforeSceneLoad ()
	{
		print( "GlobalManager::OnBeforeSceneLoad" );
	}

	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.AfterSceneLoad)]
	static void AfterSceneLoad ()
	{
		print( "GlobalManager::AfterSceneLoad" );
	}

	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		print("Globalmanager:Awake");
		// SINGLETON
		if ( m_Instance != null )
		{
			Destroy( gameObject );
//			gameObject.SetActive( false );
			return;
		}
		DontDestroyOnLoad( this );

		m_Settings	= new SectionMap();
		m_Configs	= new SectionMap();

		Settings.LoadFile( settingspath );
		Configs.LoadFile( configsPath );

		m_Instance		= this;
	}


	//////////////////////////////////////////////////////////////////////////
	public		static		void		SetCursorVisibility( bool newState )
	{
		Cursor.visible = newState;
		Cursor.lockState = newState ? CursorLockMode.None : CursorLockMode.Locked;
	}


	//////////////////////////////////////////////////////////////////////////
	public		static		void		SetTimeScale( float value )
	{
		SoundManager.Instance.Pitch = Time.timeScale = value;
	}


	//////////////////////////////////////////////////////////////////////////
	public		static		void		QuitInstanly()
	{
		Debug.Log( "GlobalManager: Exiting" );
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;		
#else
		Application.Quit();
#endif
	}

}
