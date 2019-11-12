﻿
using UnityEngine;


public interface IFlashLight : IWeaponAttachment {
	
}


public class Flashlight : WeaponAttachment, IFlashLight {
	
	protected		Light	m_SpotLight			= null;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	protected	void	Awake()
	{
		m_IsUsable = transform.SearchComponent( ref m_SpotLight, SearchContext.CHILDREN );
		if ( m_IsUsable )
		{
			m_SpotLight.type = LightType.Spot;
			m_SpotLight.intensity = 0.001f;
		}
	}
	

	//////////////////////////////////////////////////////////////////////////
	public override void OnActivate()
	{
		if ( m_IsUsable == false || m_IsAttached == false )
			return;

		m_IsActive = true;

		m_SpotLight.intensity = 1.0f;
	}


	//////////////////////////////////////////////////////////////////////////
	public override void OnDeactivated()
	{
		if ( m_IsUsable == false || m_IsAttached == false )
			return;

		m_IsActive = false;

		m_SpotLight.intensity = 0.001f;
	}


	//////////////////////////////////////////////////////////////////////////
	public	override void	SetActive( bool state )
	{
		if ( m_IsUsable == false || m_IsAttached == false )
			return;

		m_IsActive = state;

		if ( m_IsActive == true )
		{
			OnActivate();
		}
		else
		{
			OnDeactivated();
		}
	}

}
