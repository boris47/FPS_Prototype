
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

		GlobalManager.bIsChangingScene = true;

		bool bIsPlayer = other.transform.HasComponent<Player>();
		if ( bIsPlayer == false )
			return;

		CustomSceneManager.LoadSceneData loadSceneData = new CustomSceneManager.LoadSceneData()
		{
			eScene		= m_NextSceneIdx
		};
		CustomSceneManager.LoadSceneAsync( loadSceneData );
	}

}
