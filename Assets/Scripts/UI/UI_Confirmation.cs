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
		get { return this.m_IsInitialized; }
	}

	string IStateDefiner.StateName
	{
		get { return this.name; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if (this.m_IsInitialized == true )
			yield break;

		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		this.m_IsInitialized = true;
		{
			Navigation noNavigationMode = new Navigation() { mode = Navigation.Mode.None };

			this.m_IsInitialized &= this.transform.childCount > 0;

			Transform panel = null;
			if (this.m_IsInitialized )
			{
				panel = this.transform.GetChild( 0 );
			}
				
			// Label
			if (this.m_IsInitialized )
			{
				this.m_IsInitialized &= panel.SearchComponentInChild( 0, ref this.m_LabelText );
			}

			yield return null;

			// Confirm button
			Button onConfirmButton = null;
			if (this.m_IsInitialized && (this.m_IsInitialized &= panel.SearchComponentInChild( 1, ref onConfirmButton ) ) )
			{
				onConfirmButton.navigation = noNavigationMode;
				onConfirmButton.onClick.AddListener( 
					() => {
						this.m_OnConfirmAction();
						this.gameObject.SetActive( false );
					}
				);
			}

			yield return null;

			// Cancel button
			Button onCancelButton = null;
			if (this.m_IsInitialized && (this.m_IsInitialized &= panel.SearchComponentInChild( 2, ref onCancelButton ) ) )
			{
				onCancelButton.navigation = noNavigationMode;
				onCancelButton.onClick.AddListener( 
					() => {
						this.m_OnCancelAction();
						this.gameObject.SetActive( false );
					}
				);
			}

			this.gameObject.SetActive( false );

			yield return null;

			if (this.m_IsInitialized )
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
		return this.m_IsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	// Show
	public	void	Show( string LabelMsg, System.Action OnConfirm , System.Action OnCancel = null )
	{
		if (this.m_IsInitialized == false )
			return;

		this.m_LabelText.text	= LabelMsg;
		this.m_OnConfirmAction	= OnConfirm != null ? OnConfirm : () => { };
		this.m_OnCancelAction	= OnCancel  != null ? OnCancel  : () => { };
		this.gameObject.SetActive( true );
	}

}
