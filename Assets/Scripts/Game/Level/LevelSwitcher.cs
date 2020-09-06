
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent( typeof( Collider ) )]
public class LevelSwitcher : MonoBehaviour {

	[SerializeField]
	private		ESceneEnumeration				m_NextSceneIdx				= ESceneEnumeration.NONE;


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
			eScene		= m_NextSceneIdx
		};
		CustomSceneManager.LoadSceneAsync( loadSceneData );
	}

}
