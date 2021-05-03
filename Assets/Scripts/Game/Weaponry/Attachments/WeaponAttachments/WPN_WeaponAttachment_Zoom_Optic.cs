public class WPN_WeaponAttachment_Zoom_Optic : WPN_WeaponAttachment_Zoom_IronSight
{
	//////////////////////////////////////////////////////////////////////////
	protected override void OnActivateInternal()
	{
		WeaponManager.Instance.ZoomIn(m_ZoomOffset, m_ZoomFactor, m_ZoomingTime, m_ZoomSensitivityMultiplier, m_ZoomFrame);
	}
}
