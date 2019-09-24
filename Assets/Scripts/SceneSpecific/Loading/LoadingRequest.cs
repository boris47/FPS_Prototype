using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingRequest : MonoBehaviour {

	/*
	[SerializeField]
	private	SceneEnumeration	m_SceneToLoad	= SceneEnumeration.NONE;

	[SerializeField]
	private	string				m_SaveToLoad	= string.Empty;
	*/


	/*
	//////////////////////////////////////////////////////////////////////////
	private void Start()
	{
		if ( m_SceneToLoad != SceneEnumeration.NONE && GlobalManager.bIsLoadingScene == false )
		{
			CustomSceneManager.LoadSceneData m_DataForNextScene = new CustomSceneManager.LoadSceneData();
			m_DataForNextScene.eScene = m_SceneToLoad;

			if ( m_SaveToLoad.Length > 0 )
			{
				m_DataForNextScene.bMustLoadSave = true;
				m_DataForNextScene.sSaveToLoad = m_SaveToLoad;
			}

			CoroutinesManager.CreateSequence( CoroutinesManager.WaitPendingCoroutines() )
				.AddStep( CustomSceneManager.LoadSceneAsync( m_DataForNextScene ) )
				.ExecuteSequence();
		}
	}
	*/
}
