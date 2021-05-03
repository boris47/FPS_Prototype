
using UnityEngine;


public class WPN_WeaponAttachment_Flashlight : WPN_WeaponAttachmentBase
{	
	protected		Light	m_SpotLight			= null;

	protected		float	m_Intensity			= 1f;
	protected		float	m_Range				= 30f;
	protected		float	m_SpotAngle			= 45f;

	//////////////////////////////////////////////////////////////////////////
	protected void Awake()
	{
		m_IsUsable = transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_SpotLight);
		if (m_IsUsable)
		{
			m_SpotLight.type = LightType.Spot;
			m_SpotLight.intensity = 0.001f;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected override bool ConfigureInternal(in Database.Section attachmentSection)
	{
		CustomAssertions.IsTrue(attachmentSection.TryAsFloat("Intensity", out m_Intensity));
		CustomAssertions.IsTrue(attachmentSection.TryAsFloat("Range", out m_Range));
		CustomAssertions.IsTrue(attachmentSection.TryAsFloat("SpotAngle", out m_SpotAngle));

		m_SpotLight.intensity = 0.001f;
		m_SpotLight.range = m_Range;
		m_SpotLight.spotAngle = m_SpotAngle;
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnActivate()
	{
		if (m_IsUsable == false || m_IsAttached == false)
			return;

		m_IsActive = true;

		m_SpotLight.intensity = m_Intensity;
		//this.m_SpotLight.range = this.m_Range;
		//this.m_SpotLight.spotAngle = this.m_SpotAngle;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDeactivated()
	{
		if (m_IsUsable == false || m_IsAttached == false)
			return;

		m_IsActive = false;

		m_SpotLight.intensity = 0.001f;
	}
}
