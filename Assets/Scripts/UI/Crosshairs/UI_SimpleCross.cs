
using UnityEngine;
using System.Collections;

public class UI_SimpleCross : UI_BaseCrosshair
{
	[SerializeField]
	private		RectTransform		m_Horizontal_Left		= null;

	[SerializeField]
	private		RectTransform		m_Horizontal_Right		= null;

	[SerializeField]
	private		RectTransform		m_Vertical_Up			= null;

	[SerializeField]
	private		RectTransform		m_Vertical_Down			= null;


	public override IEnumerator Initialize()
	{
		IsInitialized = transform.SearchChildWithName( "HCrosshair1", ref m_Horizontal_Left	);	yield return null;
		IsInitialized &= IsInitialized && transform.SearchChildWithName( "HCrosshair2", ref m_Horizontal_Right	);	yield return null;
		IsInitialized &= IsInitialized && transform.SearchChildWithName( "VCrosshair1", ref m_Vertical_Up		);	yield return null;
		IsInitialized &= IsInitialized && transform.SearchChildWithName( "VCrosshair2", ref m_Vertical_Down		);	yield return null;
		yield return base.Initialize();
	}

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
