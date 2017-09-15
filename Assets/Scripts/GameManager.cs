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

	}
	
	// Update is called once per frame
	void Update () {
		
	}


}
