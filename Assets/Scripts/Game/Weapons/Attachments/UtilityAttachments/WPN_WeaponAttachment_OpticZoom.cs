using UnityEngine;
using System.Collections;

public class WPN_WeaponAttachment_OpticZoom : WPN_WeaponAttachment_Zoom
{
	protected override void OnActivateInternal()
	{
		WeaponManager.Instance.ZoomIn(this.m_ZoomOffset, this.m_ZoomFactor, this.m_ZoomingTime, this.m_ZoomSensitivityMultiplier, this.m_ZoomFrame );
	}
}
