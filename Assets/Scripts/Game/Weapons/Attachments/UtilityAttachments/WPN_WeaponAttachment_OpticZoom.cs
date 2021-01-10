using UnityEngine;
using System.Collections;

public class WPN_WeaponAttachment_OpticZoom : WPN_WeaponAttachment_Zoom
{
	protected override void OnActivateInternal()
	{
		WeaponManager.Instance.ZoomIn(m_ZoomOffset, m_ZoomFactor, m_ZoomingTime, m_ZoomSensitivityMultiplier, m_ZoomFrame );
	}
}
