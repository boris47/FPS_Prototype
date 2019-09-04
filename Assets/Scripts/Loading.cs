using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour {

	private	static	CustomSceneManager.LoadSceneData m_DataForNextScene = null;


	/// <summary>  </summary>
	public static void SetLoadSceneData( CustomSceneManager.LoadSceneData loadSceneData )
	{
		if ( loadSceneData.IsNotNull() )
		{
			m_DataForNextScene = loadSceneData;
			m_DataForNextScene.fProgress = 0f;
			m_DataForNextScene.bIsCompleted = false;
		}
		else
		{
			m_DataForNextScene = null;
		}
	}



	/// <summary> Start a coroutine that First load Loading Scene, than load asyncronously the required scene  </summary>
	public static IEnumerator Switch()
	{
		IEnumerator enumerator = LoadSceneAsync();
		CoroutinesManager.Start( enumerator, "Loading::Swicth: Switching" );
		return enumerator;
	}



	private	static	IEnumerator LoadSceneAsync()
	{
		yield return null;

		GlobalManager.Instance.InputMgr.DisableCategory( InputCategory.ALL );

		// Load Loading Scene syncronously
		{
			CustomSceneManager.LoadSceneData loadSceneData = new CustomSceneManager.LoadSceneData()
			{
				iSceneIdx			= SceneEnumeration.LOADING,
			};
			CustomSceneManager.LoadSceneSync( loadSceneData );
		}

		// Show Loading screen
		UIManager.Loading.Show();

		// Load Desired Scene 
		{
			CustomSceneManager.LoadSceneAsync( m_DataForNextScene );
			while( m_DataForNextScene.bIsCompleted == false )
			{
				UIManager.Loading.SetProgress( m_DataForNextScene.fProgress );
				yield return null;
			}
		}

		UIManager.Loading.Hide();
		GlobalManager.Instance.InputMgr.EnableCategory( InputCategory.ALL );
	}




	[SerializeField]
	private	SceneEnumeration m_SceneToLoad = SceneEnumeration.NONE;

	private IEnumerator Start()
	{
		yield return null;
		if ( m_SceneToLoad != SceneEnumeration.NONE && m_DataForNextScene == null )
		{
			DontDestroyOnLoad(gameObject);
				m_DataForNextScene = new CustomSceneManager.LoadSceneData();
				m_DataForNextScene.iSceneIdx = m_SceneToLoad;
				yield return LoadSceneAsync();
	//	yield return	CustomSceneManager.LoadSceneAsync( m_DataForNextScene );
			Destroy( gameObject );
		}
	}
}
