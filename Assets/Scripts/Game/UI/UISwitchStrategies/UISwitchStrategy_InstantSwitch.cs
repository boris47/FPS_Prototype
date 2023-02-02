using System.Collections;
using UnityEngine;

public class UISwitchStrategy_InstantSwitch : UISwitchStrategy
{

	//////////////////////////////////////////////////////////////////////////
	public override IEnumerator ExecuteUISwitch(UI_Base InCurrentUI, UI_Base InUIToShow)
	{
		yield return DefaultInstant(InCurrentUI, InUIToShow);
	}
}