using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour
{
/*	
	private void Awake()
	{
		CustomSceneManager.LoadSceneData loadSceneData = new CustomSceneManager.LoadSceneData()
		{
			eScene = ESceneEnumeration.LOADING,
			eMode = LoadSceneMode.Additive
		};
		CustomSceneManager.LoadSceneSync( loadSceneData );
	//	SceneManager.LoadScene( (int) ESceneEnumeration.LOADING, LoadSceneMode.Additive );
	}
*/

	private void Update()
	{
		if ( Input.anyKeyDown == true )
		{
			enabled = false;

			CustomSceneManager.LoadSceneData loadSceneData = new CustomSceneManager.LoadSceneData()
			{
				eScene = ESceneEnumeration.MAIN_MENU
			};
			CustomSceneManager.LoadSceneAsync( loadSceneData );
		}
	}

}
