using UnityEngine;

public class Intro : MonoBehaviour
{
	private void Update()
	{
		if ( Input.anyKeyDown == true )
		{
			enabled = false;

			CustomSceneManager.LoadSceneData loadSceneData = new CustomSceneManager.LoadSceneData()
			{
				eScene = ESceneEnumeration.MAIN_MENU
			};
			CustomSceneManager.LoadSceneAsync( loadSceneData );
		}
	}

}
