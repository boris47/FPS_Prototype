using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////////////////////////////////
// WeaponZoomToogle
public class WPN_WeaponModule_Zoom : WPN_BaseModule {

	public	override	bool	CanChangeWeapon	() {  return true; }
	public	override	bool	CanBeUsed		() {  return Player.Instance.IsRunning == false; }
	public	override	void	OnWeaponChange	() { }
	public	override	void	OnAfterReload	() { }
	public	override	void	InternalUpdate	() { }

	public override void Setup( IWeapon w )
	{	}

	/// <summary> Zoom toggle </summary>
	public override		void	OnStart()
	{
		if ( WeaponManager.Instance.IsZoomed == false )
			WeaponManager.Instance.ZoomIn();
		else
			WeaponManager.Instance.ZoomOut();
	}

}
