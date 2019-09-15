using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour {

	private void Start()
	{
		CoroutinesManager.Start( OnStart() );
	}

	private IEnumerator OnStart()
	{
		UnityEngine.Assertions.Assert.raiseExceptions = true;
		Debug.developerConsoleVisible = true;
		Physics.queriesHitBackfaces = false;
		Application.backgroundLoadingPriority = ThreadPriority.Low;

		// Preload MainMenu scene
//		CustomSceneManager.PreloadSceneData preloadData = new CustomSceneManager.PreloadSceneData();
//		yield return CustomSceneManager.Preload( SceneEnumeration.MAIN_MENU, preloadData );

		yield return null;

		// Add here your intro splash screens



		yield return null;

		yield return new WaitUntil( () => Input.anyKeyDown == true );

//		yield return CustomSceneManager.CompleteSceneAsyncLoad( preloadData );

		CustomSceneManager.LoadSceneData loadSceneData = new CustomSceneManager.LoadSceneData()
		{
			eScene = SceneEnumeration.MAIN_MENU
		};
//		CustomSceneManager.LoadSceneSync( loadSceneData );

		CustomSceneManager.LoadSceneAsync( loadSceneData );

//		CustomSceneManager.UnLoadSceneAsync( (int)SceneEnumeration.INTRO );
	}

	/*
	private void Update()
	{
		if ( Input.GetKeyUp( KeyCode.Return ) )
		{
			enabled = false;
			CustomSceneManager.LoadSceneData loadSceneData = new CustomSceneManager.LoadSceneData()
			{
				eScene = SceneEnumeration.MAIN_MENU
			};
			CustomSceneManager.LoadSceneAsync( loadSceneData );
		}
	}
	*/

}
