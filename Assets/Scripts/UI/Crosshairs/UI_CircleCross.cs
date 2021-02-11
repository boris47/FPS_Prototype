using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_CircleCross : UI_BaseCrosshair
{
	[SerializeField]
	private		Image		m_Circle		= null;

	public override IEnumerator Initialize()
	{
		IsInitialized = transform.TrySearchComponentByChildName( "Circle", out m_Circle	);
		yield return base.Initialize();
		
	}

	protected override void InternalUpdate()
	{
		Vector3 newScale = m_Circle.transform.localScale;
		newScale.x = CurrentValue * m_EffectMult;
		newScale.y = CurrentValue * m_EffectMult;
		m_Circle.transform.localScale = newScale;
	}
}
