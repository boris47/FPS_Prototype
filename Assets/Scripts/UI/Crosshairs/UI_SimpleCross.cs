
using UnityEngine;

public class UI_SimpleCross : UI_BaseCrosshair
{
	[SerializeField, ReadOnly]
	private		RectTransform		m_Horizontal_Left		= null;

	[SerializeField, ReadOnly]
	private		RectTransform		m_Horizontal_Right		= null;

	[SerializeField, ReadOnly]
	private		RectTransform		m_Vertical_Up			= null;

	[SerializeField, ReadOnly]
	private		RectTransform		m_Vertical_Down			= null;


	//////////////////////////////////////////////////////////////////////////
	public override void Initialize()
	{
		base.Initialize();

		if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Center", out Transform center)))
		{
			if (CustomAssertions.IsTrue(center.TrySearchComponentByChildName("AxisX", out Transform axisX)))
			{
				CustomAssertions.IsTrue(axisX.TrySearchComponentByChildName("HCrosshair1", out m_Horizontal_Left));
				CustomAssertions.IsTrue(axisX.TrySearchComponentByChildName("HCrosshair2", out m_Horizontal_Right));
			}

			if (CustomAssertions.IsTrue(center.TrySearchComponentByChildName("AxisY", out Transform axisY)))
			{
				CustomAssertions.IsTrue(axisY.TrySearchComponentByChildName("VCrosshair1", out m_Vertical_Up));
				CustomAssertions.IsTrue(axisY.TrySearchComponentByChildName("VCrosshair2", out m_Vertical_Down));
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void InternalUpdate()
	{
		float currentValue = CurrentValue * m_EffectMult;
		{
			Vector3 newPosition = m_Horizontal_Left.localPosition;
			newPosition.x = -currentValue;
			m_Horizontal_Left.localPosition = newPosition;
		}

		{
			Vector3 newPosition = m_Horizontal_Right.localPosition;
			newPosition.x = currentValue;
			m_Horizontal_Right.localPosition = newPosition;
		}

		{
			Vector3 newPosition = m_Vertical_Up.localPosition;
			newPosition.y = currentValue;
			m_Vertical_Up.localPosition = newPosition;
		}

		{
			Vector3 newPosition = m_Vertical_Down.localPosition;
			newPosition.y = -currentValue;
			m_Vertical_Down.localPosition = newPosition;
		}
	}
}
