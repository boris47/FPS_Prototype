
using UnityEngine;

public class UI_PauseMenu : MonoBehaviour {

	public	void	ReturnToMenu()
	{
		// Only if paused can return to main menu
		if ( GameManager.IsPaused == false )
			return;

		// Exit pause state
		GameManager.PauseEvents.SetPauseState( false );

		// Force curso to be visible
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		// Effect frame reset
		UI.Instance.EffectFrame.color = Color.black;

		// Hide Pause menu UI
		gameObject.SetActive( false );

		// Hide In-Game UI
		UI.Instance.InGame.gameObject.SetActive( false );

		// Show MainMenu object
		UI.Instance.MainMenu.gameObject.SetActive( true );

		// update current active transform
		UI.Instance.SwitchTo( UI.Instance.MainMenu.transform );

		// Load menu
		UnityEngine.SceneManagement.SceneManager.LoadScene( 0 );
	}

}
