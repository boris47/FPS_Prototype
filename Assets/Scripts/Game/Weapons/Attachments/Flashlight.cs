
using UnityEngine;


public interface IFlashLight : IWeaponAttachment {
	
}


public class Flashlight : WeaponAttachment, IFlashLight {
	
	protected		Light	m_SpotLight			= null;


	//////////////////////////////////////////////////////////////////////////
	// Awake
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
	protected override void OnActivate()
	{
		if (this.m_IsUsable == false || this.m_IsAttached == false )
			return;

		this.m_IsActive = true;

		this.m_SpotLight.intensity = 1.0f;
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
