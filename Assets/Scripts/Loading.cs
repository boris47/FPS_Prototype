using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour {


	[SerializeField]
	private	SceneEnumeration	m_SceneToLoad	= SceneEnumeration.NONE;

	[SerializeField]
	private	string				m_SaveToLoad	= string.Empty;




	//////////////////////////////////////////////////////////////////////////
	private IEnumerator Start()
	{
		yield return null;

		if ( m_SceneToLoad != SceneEnumeration.NONE && GlobalManager.bIsLoadingScene == false )
		{
			yield return CoroutinesManager.WaitPendingCoroutines();

			DontDestroyOnLoad( gameObject );

			CustomSceneManager.LoadSceneData m_DataForNextScene = new CustomSceneManager.LoadSceneData();
			m_DataForNextScene.iSceneIdx = m_SceneToLoad;

			if ( m_SaveToLoad.Length > 0 )
			{
				m_DataForNextScene.bMustLoadSave = true;
				m_DataForNextScene.sSaveToLoad = m_SaveToLoad;
			}

			yield return CustomSceneManager.LoadSceneAsync( m_DataForNextScene );

			Destroy( gameObject );
		}
	}
}
