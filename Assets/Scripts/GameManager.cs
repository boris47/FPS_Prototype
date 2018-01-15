using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameEvent      : UnityEngine.Events.UnityEvent { }

public class GameManager : MonoBehaviour {

	public static GameManager Instance = null;

	public		Transform	m_Players = null;

	// Use this for initialization
	void Awake ()
	{
		if ( Instance == null )
			Instance = this;
		else {
			Destroy( gameObject );
			return;
		}

		DontDestroyOnLoad( this );

		GLOBALS.Settings = new Reader();
#if UNITY_EDITOR
		GLOBALS.Settings.LoadFile( "Assets/Resources/Settings.txt" );
#else
		GLOBALS.Settings.LoadFile( "Settings" );
#endif

		GLOBALS.Configs = new Reader();
#if UNITY_EDITOR
		GLOBALS.Configs.LoadFile( "Assets/Resources/Configs/All.txt" );
#else
		GLOBALS.Configs.LoadFile( "Configs\\All" );
#endif

		Application.targetFrameRate = 60;
	}


	private void Start()
	{	
		if ( m_Players == null ) {
			Debug.Log( "Cannot Find players" );
			return;
		}

		GLOBALS.Player1 = m_Players.GetChild( 0 ).GetComponent<Player>();
		GLOBALS.Player2 = m_Players.GetChild( 1 ).GetComponent<Player>();
		GLOBALS.Player3 = m_Players.GetChild( 2 ).GetComponent<Player>();
		GLOBALS.Player4 = m_Players.GetChild( 3 ).GetComponent<Player>();

		GLOBALS.Player1.IsActive = true;
		Player.CurrentActivePlayer = GLOBALS.Player1;

//		CameraControl.Instance.SwitchToTarget( GLOBALS.Player1.gameObject );
	}


	// Update is called once per frame
	private void Update()
	{
		// APPLICATION EXIT
		if ( Input.GetKeyDown( KeyCode.Escape ) )
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}

		GLOBALS.InputMgr.Update();

		if ( GLOBALS.Player1 != null && Input.GetKeyDown( KeyCode.F1 ) ) CameraControl.Instance.SwitchToTarget( GLOBALS.Player1.gameObject );
		if ( GLOBALS.Player2 != null && Input.GetKeyDown( KeyCode.F2 ) ) CameraControl.Instance.SwitchToTarget( GLOBALS.Player2.gameObject );
		if ( GLOBALS.Player3 != null && Input.GetKeyDown( KeyCode.F3 ) ) CameraControl.Instance.SwitchToTarget( GLOBALS.Player3.gameObject );
		if ( GLOBALS.Player4 != null && Input.GetKeyDown( KeyCode.F4 ) ) CameraControl.Instance.SwitchToTarget( GLOBALS.Player4.gameObject );

	}


}
