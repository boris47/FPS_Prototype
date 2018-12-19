using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Confirmation : MonoBehaviour {

	private	System.Action		m_OnConfirmAction	= null;
	private	System.Action		m_OnCancelAction	= null;

	private	Text				m_LabelText			= null;

	// Initial setup
	public void Initialize()
	{
		Transform panel = transform.GetChild( 0 );

		m_LabelText = panel.GetChild( 0 ).GetComponent<Text>();

		Button onConfirmButton = panel.GetChild( 1 ).GetComponent<Button>();
		{
			onConfirmButton.navigation = new Navigation() { mode = Navigation.Mode.None };
			onConfirmButton.onClick.RemoveAllListeners();
			onConfirmButton.onClick.AddListener( 
				() => {
					m_OnConfirmAction();
					gameObject.SetActive( false );
				}
			);
		}

		Button onCancelButton = panel.GetChild( 2 ).GetComponent<Button>();
		{
			onCancelButton.navigation = new Navigation() { mode = Navigation.Mode.None };
			onCancelButton.onClick.RemoveAllListeners();
			onCancelButton.onClick.AddListener( 
				() => {
					m_OnCancelAction();
					gameObject.SetActive( false );
				}
			);
		}

		gameObject.SetActive( false );
	}

	public	void	Show( string LabelMsg, System.Action OnConfirm , System.Action OnCancel = null )
	{
		Initialize();
		m_LabelText.text	= LabelMsg;
		m_OnConfirmAction	= OnConfirm != null ? OnConfirm : () => { };
		m_OnCancelAction	= OnCancel  != null ? OnCancel  : () => { };
		gameObject.SetActive( true );
	}

}
