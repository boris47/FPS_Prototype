using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static GameManager Instance = null;


	// Use this for initialization
	void Awake () {
		
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


	private void Start() {
		
		GameObject Players = GameObject.Find( "Players" );
		GLOBALS.Player1 = Players.transform.GetChild( 0 ).GetComponent<Player>();
		GLOBALS.Player2 = Players.transform.GetChild( 1 ).GetComponent<Player>();
		GLOBALS.Player3 = Players.transform.GetChild( 2 ).GetComponent<Player>();
		GLOBALS.Player4 = Players.transform.GetChild( 3 ).GetComponent<Player>();

		GLOBALS.Player1.IsActive = true;

	}

	// Update is called once per frame
	void Update () {
		
		GLOBALS.InputMgr.Update();

	}


}
