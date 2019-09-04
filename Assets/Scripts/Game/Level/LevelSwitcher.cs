
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent( typeof( Collider ) )]
public class LevelSwitcher : MonoBehaviour {

	[SerializeField]
	private		SceneEnumeration				m_NextSceneIdx				= SceneEnumeration.NONE;


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		if ( GlobalManager.bIsChangingScene == true )
			return;

		bool bIsPlayer = other.transform.HasComponent<Player>();
		if ( bIsPlayer == false )
			return;

		CustomSceneManager.LoadSceneData loadSceneData = new CustomSceneManager.LoadSceneData()
		{
			iSceneIdx		= m_NextSceneIdx,
			sSaveToLoad		= "",
			bMustLoadSave	= false
		};
		CustomSceneManager.LoadSceneSync( loadSceneData );
	}

}
