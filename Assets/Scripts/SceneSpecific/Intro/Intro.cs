using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour {

	private void Update()
	{
		if ( Input.anyKeyDown == true )
		{
			enabled = false;

			CustomSceneManager.LoadSceneData loadSceneData = new CustomSceneManager.LoadSceneData()
			{
				eScene = SceneEnumeration.MAIN_MENU
			};
			CustomSceneManager.LoadSceneAsync( loadSceneData );
		}
	}

}
