using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Confirmation : MonoBehaviour, IStateDefiner {

	private	System.Action	m_OnConfirmAction	= null;
	private	System.Action	m_OnCancelAction	= null;

	private	Text			m_LabelText			= null;

	private	bool			m_bIsInitialized	= false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
	} 


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	bool IStateDefiner.Initialize()
	{
		if ( m_bIsInitialized == true )
		{
			return true;
		}

		m_bIsInitialized = true;
		{
			Navigation noNavigationMode = new Navigation() { mode = Navigation.Mode.None };

			m_bIsInitialized &= transform.childCount > 0;

			Transform panel = null;
			if ( m_bIsInitialized )
			{
				panel = transform.GetChild( 0 );
			}
				
			// Label
			if ( m_bIsInitialized )
			{
				m_bIsInitialized &= panel.SearchComponentInChild( 0, ref m_LabelText );
			}

			// Confirm button
			Button onConfirmButton = null;
			if ( m_bIsInitialized && ( m_bIsInitialized &= panel.SearchComponentInChild( 1, ref onConfirmButton ) ) )
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
			Button onCancelButton = null;
			if ( m_bIsInitialized && ( m_bIsInitialized &= panel.SearchComponentInChild( 2, ref onCancelButton ) ) )
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
		return m_bIsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	// ReInit
	bool IStateDefiner.ReInit()
	{
		return m_bIsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	// Finalize
	bool IStateDefiner.Finalize()
	{
		return m_bIsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	// Show
	public	void	Show( string LabelMsg, System.Action OnConfirm , System.Action OnCancel = null )
	{
		if ( m_bIsInitialized == false )
			return;

		m_LabelText.text	= LabelMsg;
		m_OnConfirmAction	= OnConfirm != null ? OnConfirm : () => { };
		m_OnCancelAction	= OnCancel  != null ? OnCancel  : () => { };
		gameObject.SetActive( true );
	}

}
