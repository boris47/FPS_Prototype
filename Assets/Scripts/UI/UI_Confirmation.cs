using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_Confirmation : MonoBehaviour, IStateDefiner {

	private	System.Action	m_OnConfirmAction	= null;
	private	System.Action	m_OnCancelAction	= null;

	private	Text			m_LabelText			= null;

	private	bool			m_IsInitialized	= false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_IsInitialized; }
	}

	string IStateDefiner.StateName
	{
		get { return name; }
	}


	//////////////////////////////////////////////////////////////////////////
	public void PreInit() { }

	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if (m_IsInitialized == true )
			yield break;

		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		m_IsInitialized = true;
		{
			Navigation noNavigationMode = new Navigation() { mode = Navigation.Mode.None };

			m_IsInitialized &= transform.childCount > 0;

			Transform panel = null;
			if (m_IsInitialized )
			{
				panel = transform.GetChild( 0 );
			}
				
			// Label
			if (m_IsInitialized )
			{
				m_IsInitialized &= panel.TrySearchComponentByChildIndex( 0, out m_LabelText );
			}

			yield return null;

			// Confirm button
			Button onConfirmButton = null;
			if (m_IsInitialized && (m_IsInitialized &= panel.TrySearchComponentByChildIndex( 1, out onConfirmButton ) ) )
			{
				onConfirmButton.navigation = noNavigationMode;
				onConfirmButton.onClick.AddListener( 
					() => {
						m_OnConfirmAction();
						gameObject.SetActive( false );
					}
				);
			}

			yield return null;

			// Cancel button
			Button onCancelButton = null;
			if (m_IsInitialized && (m_IsInitialized &= panel.TrySearchComponentByChildIndex( 2, out onCancelButton ) ) )
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

			yield return null;

			if (m_IsInitialized )
			{
				CoroutinesManager.RemoveCoroutineFromPendingCount( 1 );
			}
			else
			{
				Debug.LogError( "UI_Confirmation: Bad initialization!!!" );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ReInit
	IEnumerator IStateDefiner.ReInit()
	{
		yield return null;
	}


	//////////////////////////////////////////////////////////////////////////
	// Finalize
	bool IStateDefiner.Finalize()
	{
		return m_IsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	// Show
	public	void	Show( string LabelMsg, System.Action OnConfirm , System.Action OnCancel = null )
	{
		if (m_IsInitialized == false )
			return;

		m_LabelText.text	= LabelMsg;
		m_OnConfirmAction	= OnConfirm != null ? OnConfirm : () => { };
		m_OnCancelAction	= OnCancel  != null ? OnCancel  : () => { };
		gameObject.SetActive( true );
	}

}
