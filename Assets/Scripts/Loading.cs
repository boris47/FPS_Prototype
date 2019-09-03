using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour {

	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnBeforeSceneLoad ()
	{
//		GlobalManager.bIsLoadingScene = true;

//		UnityEditor.EditorBuildSettingsScene
//		UnityEditor.BuildOptions.ForceEnableAssertions
		print( "Loading::OnBeforeSceneLoad" );
	}



	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void AfterSceneLoad ()
	{
		print( "Loading::AfterSceneLoad" );
	}
	
	public	static	CustomSceneManager.LoadSceneData m_DataForNextScene = new CustomSceneManager.LoadSceneData();



	public static void SetLoadSceneData( CustomSceneManager.LoadSceneData loadSceneData )
	{
		if ( loadSceneData.IsNotNull() )
		{
			m_DataForNextScene = loadSceneData;
		}
		else
		{
			m_DataForNextScene = default( CustomSceneManager.LoadSceneData );
		}
	}



	/// <summary> Start a coroutine that First load Loading Scene, than load asyncronously the required scene  </summary>
	public static IEnumerator Switch()
	{
		IEnumerator enumerator = SwitchCO();
		CoroutinesManager.Start( enumerator );
		return enumerator;
	}



	private	static	IEnumerator SwitchCO()
	{
		int currentActiveScene = SceneManager.GetActiveScene().buildIndex;
		
		// Load Loading Scene syncronously
		{
			CustomSceneManager.LoadSceneData loadSceneData = new CustomSceneManager.LoadSceneData()
			{
				iSceneIdx			= SceneEnumeration.LOADING,
			};
			CustomSceneManager.LoadSceneSync( loadSceneData );
		}

		// TODO: show an ui element, pheraps an image, that can prevent the player to see what is happening

		// Load Desired Scene 
		{
			yield return CustomSceneManager.LoadSceneAsync( m_DataForNextScene );
		}
	}




	[SerializeField]
	private	SceneEnumeration m_SceneToLoad = SceneEnumeration.NONE;

	private void Start()
	{
		if ( m_SceneToLoad != SceneEnumeration.NONE )
		{
			m_DataForNextScene = new CustomSceneManager.LoadSceneData();
			m_DataForNextScene.iSceneIdx = m_SceneToLoad;
			CustomSceneManager.LoadSceneAsync( m_DataForNextScene );
		}
	}
}
