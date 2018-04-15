
using UnityEngine;
using UnityEngine.SceneManagement;

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

		UI.Instance.LoadSceneByIdx( m_NextSceneIdx );
	}

}
