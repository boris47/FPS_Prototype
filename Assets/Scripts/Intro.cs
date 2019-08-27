using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour {


	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnBeforeSceneLoad ()
	{
		UnityEngine.Assertions.Assert.raiseExceptions = true;

		Debug.developerConsoleVisible = true;

		Physics.queriesHitBackfaces = false;

		Application.backgroundLoadingPriority = ThreadPriority.Low;

//		UnityEditor.EditorBuildSettingsScene
//		UnityEditor.BuildOptions.ForceEnableAssertions
//		print( "EntryPoint::OnBeforeSceneLoad" );
	}


	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void AfterSceneLoad ()
	{
//		print( "EntryPoint::AfterSceneLoad" );
	}


	private IEnumerator Start()
	{
		yield return null;

		// Add here your intro splash screens

		yield return null;

		CustomSceneManager.LoadSceneData data = new CustomSceneManager.LoadSceneData()
		{
			iSceneIdx = SceneEnumeration.MAIN_MENU,
			eLoadMode = LoadSceneMode.Single
		};

		CustomSceneManager.LoadConditionVerified loadConditon = new CustomSceneManager.LoadConditionVerified()
		{
			bHasToWait = true
		};
		CustomSceneManager.LoadSceneAsync( data, loadConditon );

		yield return new WaitUntil( () => Input.anyKeyDown == true );


		loadConditon.bHasToWait = false;
		
		// Unload this scene to save memory
//		SceneManager.UnloadSceneAsync( gameObject.scene );
	}


	private void OnDestroy()
	{
		print("Intro::OnDestroy: Intro object Destroyied");
	}

}
