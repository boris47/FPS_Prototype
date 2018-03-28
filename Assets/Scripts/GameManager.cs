using System.Collections;
using System.Collections.Generic;
using CFG_Reader;
using UnityEngine;

[System.Serializable]
public class GameEvent      : UnityEngine.Events.UnityEvent { }

public class GameManager : MonoBehaviour {

	public	static Reader			Settings	= null;

	public	static Reader			Configs		= null;

	public	static InputManager		InputMgr	= new InputManager();

	public	static GameManager		Instance	= null;

	public	bool					HideCursor	= true;

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

		Settings = new Reader();
#if UNITY_EDITOR
		Settings.LoadFile( "Assets/Resources/Settings.txt" );
#else
		Settings.LoadFile( "Settings" );
#endif

		Configs = new Reader();
#if UNITY_EDITOR
		Configs.LoadFile( "Assets/Resources/Configs/All.txt" );
#else
		Configs.LoadFile( "Configs\\All" );
#endif

		if ( HideCursor )
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}

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

		InputMgr.Update();

	}


}
