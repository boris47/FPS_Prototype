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

		// Label
		m_LabelText = panel.GetChild( 0 ).GetComponent<Text>();

		Navigation noNavigationMode = new Navigation() { mode = Navigation.Mode.None };

		// Confirm button
		Button onConfirmButton = panel.GetChild( 1 ).GetComponent<Button>();
		{
			onConfirmButton.navigation = noNavigationMode;
			onConfirmButton.onClick.AddListener( 
				() => {
					m_OnConfirmAction();
					gameObject.SetActive( false );
				}
			);
		}

		// Cancel button
		Button onCancelButton = panel.GetChild( 2 ).GetComponent<Button>();
		{
			onCancelButton.navigation = noNavigationMode;
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
		m_LabelText.text	= LabelMsg;
		m_OnConfirmAction	= OnConfirm != null ? OnConfirm : () => { };
		m_OnCancelAction	= OnCancel  != null ? OnCancel  : () => { };
		gameObject.SetActive( true );
	}

}
