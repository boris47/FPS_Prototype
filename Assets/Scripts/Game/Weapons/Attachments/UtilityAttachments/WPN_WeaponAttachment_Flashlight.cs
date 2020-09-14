
using UnityEngine;


public class WPN_WeaponAttachment_Flashlight : WPN_BaseWeaponAttachment
{	
	protected		Light	m_SpotLight			= null;

	protected		float	m_Intensity			= 1f;
	protected		float	m_Range				= 30f;
	protected		float	m_SpotAngle			= 45f;

	//////////////////////////////////////////////////////////////////////////
	protected	void	Awake()
	{
		this.m_IsUsable = this.transform.SearchComponent( ref this.m_SpotLight, ESearchContext.CHILDREN );
		if (this.m_IsUsable )
		{
			this.m_SpotLight.type = LightType.Spot;
			this.m_SpotLight.intensity = 0.001f;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public override bool Configure(in Database.Section attachmentSection)
	{
		this.m_Intensity	= attachmentSection.AsFloat( "Intensity", this.m_Intensity );
		this.m_Range		= attachmentSection.AsFloat( "Range", this.m_Range );
		this.m_SpotAngle	= attachmentSection.AsFloat( "SpotAngle", this.m_SpotAngle );


		this.m_SpotLight.intensity = 0.001f;
		this.m_SpotLight.range = this.m_Range;
		this.m_SpotLight.spotAngle = this.m_SpotAngle;
		return true;
	}
	

	//////////////////////////////////////////////////////////////////////////
	protected override void OnActivate()
	{
		if (this.m_IsUsable == false || this.m_IsAttached == false )
			return;

		this.m_IsActive = true;

		this.m_SpotLight.intensity = this.m_Intensity;
//		this.m_SpotLight.range = this.m_Range;
//		this.m_SpotLight.spotAngle = this.m_SpotAngle;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDeactivated()
	{
		if (this.m_IsUsable == false || this.m_IsAttached == false )
			return;

		this.m_IsActive = false;

		this.m_SpotLight.intensity = 0.001f;
	}
}
