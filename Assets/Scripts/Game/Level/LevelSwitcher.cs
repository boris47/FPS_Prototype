
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent( typeof( Collider ) )]
public class LevelSwitcher : MonoBehaviour {

	[SerializeField]
	private		int					m_NextSceneIdx				= 0;


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		if ( GameManager.IsChangingScene == true )
			return;

		Player player = other.GetComponent<Player>();
		if ( player == null )
			return;

		CustomSceneManager.LoadSceneData loadData = new CustomSceneManager.LoadSceneData()
		{
			iSceneIdx = m_NextSceneIdx,
			sSaveToLoad = "",
			bMustLoadSave = false
		};
		CustomSceneManager.LoadSceneSync( loadData );
	}

}
