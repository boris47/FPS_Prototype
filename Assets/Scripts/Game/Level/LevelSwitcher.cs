
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSwitcher : MonoBehaviour {

	[SerializeField]
	private		int					m_NextSceneIdx				= 0;

	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		Player player = other.GetComponent<Player>();
		if ( player == null )
			return;

//		print("entrato");

		int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
		UI.Instance.LoadSceneByIdx( currentSceneIndex + 1 );

		gameObject.SetActive( false );
	}

}
