using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour {

	private IEnumerator Start()
	{
		UnityEngine.Assertions.Assert.raiseExceptions = true;

		Debug.developerConsoleVisible = true;

		Physics.queriesHitBackfaces = false;

		Application.backgroundLoadingPriority = ThreadPriority.Low;

		// Preload MainMenu scene
		CustomSceneManager.LoadSceneData loadSceneData = new CustomSceneManager.LoadSceneData()
		{
			iSceneIdx = SceneEnumeration.MAIN_MENU
		};
		CustomSceneManager.PreloadSceneData preloadData = new CustomSceneManager.PreloadSceneData();

		CustomSceneManager.Preload( SceneEnumeration.MAIN_MENU, preloadData );

		yield return null;

		// Add here your intro splash screens



		yield return null;

		yield return new WaitUntil( () => Input.anyKeyDown == true );

		yield return CustomSceneManager.CompleteSceneAsyncLoad( preloadData );

		CustomSceneManager.UnLoadSceneAsync( (int)SceneEnumeration.INTRO );
	}


	private void OnDestroy()
	{
		print("Intro::OnDestroy: Intro object Destroyied");
	}

}
