using UnityEngine;
using System.Collections;


public class UI_MainMenu : MonoBehaviour {

	

	//////////////////////////////////////////////////////////////////////////
	// OnNewGame
	public	void	OnNewGame()
	{
		UI.Instance.LoadSceneByIdx( 1 );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnNewGame
	public	void	OnResume()
	{
//		int savedSceneIdx = 2;
//		StartCoroutine( LoadSceneByIdx( savedSceneIdx ) );
	}

}
