using System.Collections;
using System.Collections.Generic;
using CFG_Reader;
using UnityEngine;

[System.Serializable]
public class GameEvent      : UnityEngine.Events.UnityEvent { }

public class GameManager : MonoBehaviour {

	public static GameManager Instance = null;

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

	}


}
