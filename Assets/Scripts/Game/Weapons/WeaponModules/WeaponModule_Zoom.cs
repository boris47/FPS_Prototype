using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public	struct ZoomOverrideData {
	public	float				ZoomingTime;
	public	float				ZoomSensitivity;
	public	float				ZoomFactor;
	public	Image				ZoomFrame;
}

//////////////////////////////////////////////////////////////////////////
// WeaponZoomToogle
public class WPN_WeaponModule_Zoom : WPN_BaseModule {

	protected	float				m_ZoomingTime			= 1.0f;
	protected	float				m_ZoomSensitivity		= 1.0f;
	protected	float				m_ZoomFactor			= 2.0f;
	protected	Image				m_ZoomFrame				= null;


	public	virtual	float			ZoomSensitivity
	{
		get { return m_ZoomSensitivity; }
	}



	public	override	bool	Setup			( IWeapon w )
	{
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	bool	InternalSetup( Database.Section moduleSection )
	{
		float zoomingTime		= moduleSection.AsFloat( "ZoomingTime",		m_ZoomFactor );
		float zoomSensitivity	= moduleSection.AsFloat( "ZoomSensitivity",	m_ZoomFactor );
		float zoomFactor		= moduleSection.AsFloat( "ZoomFactor",		m_ZoomFactor );

		m_ZoomingTime			= zoomingTime;
		m_ZoomSensitivity		= zoomSensitivity;
		m_ZoomFactor			= zoomFactor;

		return true;
	}


	//		MODIFIERS
	//////////////////////////////////////////////////////////////////////////
	public		override		void	ApplyModifier( Database.Section modifier )
	{
		base.ApplyModifier( modifier );


	}



	//////////////////////////////////////////////////////////////////////////
	public	override	bool	OnSave			( StreamUnit streamUnit )
	{
		streamUnit.SetInternal( "ZoomingTime",					m_ZoomingTime );
		streamUnit.SetInternal( "zoomSensitivity",				m_ZoomSensitivity );
		streamUnit.SetInternal( "zoomFactor",					m_ZoomFactor );
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public	override	bool	OnLoad			( StreamUnit streamUnit )
	{
		m_ZoomingTime				= streamUnit.GetAsFloat( "ZoomingTime" );
		m_ZoomSensitivity			= streamUnit.GetAsFloat( "ZoomSensitivity" );
		m_ZoomFactor				= streamUnit.GetAsFloat( "ZoomFactor" );
		return true;
	}

	public	override	bool	CanChangeWeapon	() {  return true; }
	public	override	bool	CanBeUsed		() {  return Player.Instance.IsRunning == false; }
	public	override	void	OnWeaponChange	() { }
	public	override	bool	NeedReload		() { return false; }
	public	override	void	OnAfterReload	() { }
	public	override	void	InternalUpdate	( float DeltaTime ) { }

	

	/// <summary> Zoom toggle </summary>
	public override		void	OnStart()
	{
		ZoomOverrideData overrideData = new ZoomOverrideData()
		{
			ZoomingTime			= m_ZoomingTime,
			ZoomSensitivity		= m_ZoomSensitivity,
			ZoomFactor			= m_ZoomFactor,
			ZoomFrame			= m_ZoomFrame
		};

		
		if ( WeaponManager.Instance.IsZoomed )
			WeaponManager.Instance.ZoomOut();
		else
			WeaponManager.Instance.ZoomIn();
			
	}

}
