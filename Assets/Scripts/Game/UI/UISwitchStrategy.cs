using System.Collections;
using UnityEngine;

public abstract class UISwitchStrategy : MonoBehaviour
{
	[SerializeField]
	private bool m_Inverted = false;

	protected bool Inverted => m_Inverted;


	//////////////////////////////////////////////////////////////////////////
	public abstract IEnumerator ExecuteUISwitch(UI_Base InCurrentUI, UI_Base InUIToShow);


	//////////////////////////////////////////////////////////////////////////
	protected IEnumerator DefaultInstant(UI_Base InCurrentUI, UI_Base InUIToShow)
	{
		// Disable current active menu gameObject
		InCurrentUI.gameObject.SetActive(false);

		// Enable current active menu gameObject
		InUIToShow.gameObject.SetActive(true);

		yield return null;
	}
}