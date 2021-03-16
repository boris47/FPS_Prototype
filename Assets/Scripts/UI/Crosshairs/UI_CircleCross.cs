using UnityEngine;
using UnityEngine.UI;

public class UI_CircleCross : UI_BaseCrosshair
{
	[SerializeField]
	private		Image		m_Circle		= null;

	public override void Initialize()
	{
		base.Initialize();

		CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Circle", out m_Circle));
	}

	protected override void InternalUpdate()
	{
		Vector3 newScale = m_Circle.transform.localScale;
		newScale.x = CurrentValue * m_EffectMult;
		newScale.y = CurrentValue * m_EffectMult;
		m_Circle.transform.localScale = newScale;
	}
}
