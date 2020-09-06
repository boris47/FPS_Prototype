using System.Collections;
using System.Collections.Generic;
using Database;
using UnityEngine;


//////////////////////////////////////////////////////////////////////////
// WPN_BaseModuleEmpty
/// <summary> Concrete class for empty weapon modules </summary>
[System.Serializable]
public class WPN_BaseModuleEmpty : WPN_BaseModule {

	public override Section ModuleSection
	{
		get {
			return new Database.Section( "WPN_BaseModuleEmpty", "Unassigned" );
		}
	}
	public		override	bool	Setup				( IWeapon w, EWeaponSlots slot ) { return true; }
	protected	override	bool	InternalSetup		( Database.Section moduleSection ) { return true; }

	public		override	bool	OnSave				( StreamUnit streamUnit ) { return true; }
	public		override	bool	OnLoad				( StreamUnit streamUnit ) {	return true; }

	public		override	bool	CanChangeWeapon		() {  return true; }
	public		override	bool	CanBeUsed			() {  return true; }
	public		override	void	OnWeaponChange		() { }
	public		override	void	InternalUpdate		( float DeltaTime ) { }
	public		override	bool	NeedReload			() { return false; }
	public		override	void	OnAfterReload		() { }

	//
	public		override	void	OnStart				()	{ }
	public		override	void	OnUpdate			()	{ }
	public		override	void	OnEnd				()	{ }

}