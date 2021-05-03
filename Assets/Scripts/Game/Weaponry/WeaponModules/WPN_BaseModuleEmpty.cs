


/// <summary> Concrete class for empty weapon modules </summary>
[System.Serializable]
public class WPN_BaseModuleEmpty : WPN_BaseModule
{
	public override Database.Section ModuleSection		=> new Database.Section( "WPN_BaseModuleEmpty" );

	public		override	void	OnAttach			( IWeapon w, EWeaponSlots slot ) { }
	public		override	void	OnDetach			()	{ }
	protected	override	bool	InternalSetup		( Database.Section moduleSection ) => true;

	public		override	bool	OnSave				( StreamUnit streamUnit ) => true;
	public		override	bool	OnLoad				( StreamUnit streamUnit ) => true;

	public		override	bool	CanChangeWeapon		() => true;
	public		override	bool	CanBeUsed			() => true;
	public		override	void	OnWeaponChange		() { }
	protected	override	void	OnFrame		( float DeltaTime ) { }
	public		override	bool	NeedReload			() => false;
	public		override	void	OnAfterReload		() { }

	//
	public		override	void	OnStart				()	{ }
	public		override	void	OnUpdate			()	{ }
	public		override	void	OnEnd				()	{ }

}