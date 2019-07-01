
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
		if ( GlobalManager.bIsChangingScene == true )
			return;

		Player player = other.GetComponent<Player>();
		if ( player == null )
			return;

		CustomSceneManager.LoadSceneData loadedResource = new CustomSceneManager.LoadSceneData()
		{
			iSceneIdx		= m_NextSceneIdx,
			sSaveToLoad		= "",
			bMustLoadSave	= false
		};
		CustomSceneManager.LoadSceneSync( loadedResource );
	}

}
