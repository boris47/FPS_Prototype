using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_Confirmation : UI_Base, IStateDefiner
{

	private				System.Action		m_OnConfirmAction					= null;
	private				System.Action		m_OnCancelAction					= null;
	private				Text				m_LabelText							= null;

	private				bool				m_IsInitialized						= false;
						bool				IStateDefiner.IsInitialized			=> m_IsInitialized;


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.PreInit()
	{

	}


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.Initialize()
	{
		if (!m_IsInitialized)
		{
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Panel", out Transform panel)))
			{
				CustomAssertions.IsTrue(panel.TrySearchComponentByChildIndex(0, out m_LabelText));

				if (CustomAssertions.IsTrue(panel.TrySearchComponentByChildIndex(1, out Button onConfirmButton)))
				{
					void OnConfirm()
					{
						m_OnConfirmAction();
						gameObject.SetActive(false);
					}
					onConfirmButton.onClick.AddListener(OnConfirm);
				}

				if (CustomAssertions.IsTrue(panel.TrySearchComponentByChildIndex(2, out Button onCancelButton)))
				{
					void OnCancel()
					{
						m_OnCancelAction();
						gameObject.SetActive(false);
					}
					onCancelButton.onClick.AddListener(OnCancel);
				}
			}

			// disable navigation for everything
			Navigation noNavigationMode = new Navigation() { mode = Navigation.Mode.None };
			foreach (Selectable s in GetComponentsInChildren<Selectable>())
			{
				s.navigation = noNavigationMode;
			}

			gameObject.SetActive(false);
			m_IsInitialized = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.ReInit()
	{

	}


	//////////////////////////////////////////////////////////////////////////
	bool IStateDefiner.Finalize()
	{
		return m_IsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		CustomAssertions.IsTrue(m_IsInitialized);
	}


	//////////////////////////////////////////////////////////////////////////
	public void Show(string LabelMsg, System.Action OnConfirm, System.Action OnCancel = null)
	{
		CustomAssertions.IsTrue(m_IsInitialized);
		CustomAssertions.IsNotNull(OnConfirm);

		m_LabelText.text	= LabelMsg;
		m_OnConfirmAction	= OnConfirm;
		m_OnCancelAction	= OnCancel ?? (() => { });
		gameObject.SetActive(true);
	}

}
