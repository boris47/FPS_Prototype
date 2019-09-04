
using System.Collections;
using System.Collections.Generic;
using Database;
using UnityEngine;

public interface IWPN_FireModule {

	FireModes				FireMode						{ get; }

	float					Damage							{ get; }
	uint					Magazine						{ get; }
	uint					MagazineCapacity				{ get; }

	float					CamDeviation					{ get; }
	float					FireDispersion					{ get; }

	bool					NeedReload						();
	bool					ChangeFireMode					( string FireMode );
	bool					ChangeFireMode				<T>	();
}

public interface IWPN_UtilityModule {

}


//////////////////////////////////////////////////////////////////////////
// WPN_BaseModule ( Abstract )
/// <summary> Abstract base class for weapon modules </summary>
[System.Serializable]
public abstract class WPN_BaseModule : MonoBehaviour, IModifiable {

	protected		Database.Section			m_ModuleSection				= new Database.Section( "Empty", "Unassigned" );
	protected		IWeapon						m_WeaponRef					= null;
	protected		WeaponSlots					m_ModuleSlot				= WeaponSlots.NONE;
	protected		List<Database.Section>		m_Modifiers					= new List<Database.Section>();
	protected		GameObject					m_FireModeContainer					= null;


	public virtual		Database.Section			ModuleSection
	{
		get { return m_ModuleSection; }
	}

	public		abstract	bool	Setup			( IWeapon w, WeaponSlots slot );

	//////////////////////////////////////////////////////////////////////////
	protected	abstract	bool	InternalSetup( Database.Section moduleSection );


	//////////////////////////////////////////////////////////////////////////
	public	static	bool	GetRules( Database.Section moduleSection, ref string[] allowedBullets )
	{
		bool result = true;

		string[] localAllowedBullets = null;
		if ( result &= moduleSection.bGetMultiAsArray<string>( "AllowedBullets", ref localAllowedBullets ) )
		{
			allowedBullets = localAllowedBullets;
		}

		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	public		bool		CanAssignBullet( string bulletName )
	{
		bool result = true;

		string[] allowedBullets = null;
		if ( result &= GetRules( m_ModuleSection, ref allowedBullets ) )
		{
			result &= System.Array.IndexOf( allowedBullets, bulletName ) > -1;
		}

		return result;
	}

	//		MODIFIERS
	//////////////////////////////////////////////////////////////////////////


	public		virtual		void	ApplyModifier( Database.Section modifier )	{ }
	public		virtual		void	ResetBaseConfiguration()	{ }
	public		virtual		void	RemoveModifier( Database.Section modifier )	{ }


	protected	virtual	void OnEnable()
	{
		GameManager.UpdateEvents.OnFrame += InternalUpdate;
	}

	protected	virtual void OnDisable()
	{
		if ( GameManager.UpdateEvents.IsNotNull() )
			GameManager.UpdateEvents.OnFrame -= InternalUpdate;
	}

	public		abstract	bool	OnSave			( StreamUnit streamUnit );
	public		abstract	bool	OnLoad			( StreamUnit streamUnit );

	public		abstract	bool	CanChangeWeapon	();
	public		abstract	bool	CanBeUsed		();
	public		abstract	void	OnWeaponChange	();

	public		abstract	bool	NeedReload		();
	public		abstract	void	OnAfterReload	();

	public		abstract	void	InternalUpdate( float DeltaTime );

	//
	public		virtual		void	OnStart		()	{ }
	public		virtual		void	OnUpdate	()	{ }
	public		virtual		void	OnEnd		()	{ }

}


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
	public		override	bool	Setup				( IWeapon w, WeaponSlots slot ) { return true; }
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


//////////////////////////////////////////////////////////////////////////
// WPN_FireModule  ( Abstract )
/// <summary> Abstract base class for fire modules </summary>
[System.Serializable]
public abstract class WPN_FireModule : WPN_BaseModule, IWPN_FireModule {

	private	static		AudioCollection							m_ModuleSounds				= null;

	[SerializeField]	protected	Transform					m_FirePoint					= null;
	[SerializeField]	protected	uint						m_Magazine					= 0;
	[SerializeField]	protected	uint						m_MagazineCapacity			= 1;
	[SerializeField]	protected	bool						m_CanPenetrate				= false;
	[SerializeField]	protected	float						m_Damage					= 0.0f;
	[SerializeField]	protected	float						m_ShotDelay					= 0.5f;
	[SerializeField]	protected	float						m_CamDeviation				= 0.02f;
	[SerializeField]	protected	float						m_FireDispersion			= 0.05f;
	[SerializeField]	protected	float						m_Recoil					= 0.3f;
	[SerializeField]	protected	WPN_FireMode_Base			m_WpnFireMode				= null;

	// INTERFACE START
	public abstract	FireModes									FireMode					{ get; }
	public			Vector3										FirePointPosition			{ get { return m_FirePoint.position; } } // TODO Assign m_FirePoint
	public			Quaternion									FirePointRotation			{ get { return m_FirePoint.rotation; } }
	public			uint										Magazine					{ get { return m_Magazine; } }
	public			uint										MagazineCapacity			{ get { return m_MagazineCapacity; } }
	public			float										Damage						{ get { return m_Damage; } }

	public			float										CamDeviation				{ get { return m_CamDeviation; } }
	public			float										FireDispersion				{ get { return m_FireDispersion; } }
	// INTERFACE END

	protected		GameObjectsPool<Bullet>						m_PoolBullets				= null;
	protected		bool										m_Initialized				= false;
	protected		CustomAudioSource							m_AudioSourceFire			= null; // TODO Create audio


	//		SETUP
	//////////////////////////////////////////////////////////////////////////
	public		override	bool	Setup( IWeapon wpn, WeaponSlots slot )
	{
		string moduleSectionName = this.GetType().FullName;

		m_WeaponRef = wpn;
		m_ModuleSlot = slot;

		m_FirePoint					= m_WeaponRef.Transform.Find( "FirePoint" );

		// MODULE CONTAINER
		string containerID = Weapon.GetModuleSlotName( slot );
		m_FireModeContainer = transform.Find(containerID) != null ? transform.Find(containerID).gameObject : null;
		if ( m_FireModeContainer != null )
		{
			Destroy( m_FireModeContainer );
		}
		m_FireModeContainer = new GameObject( containerID );
		m_FireModeContainer.transform.SetParent( transform );
		m_FireModeContainer.transform.localPosition = Vector3.zero;
		m_FireModeContainer.transform.localRotation = Quaternion.identity;
		

		// TRY RECOVER MODULE SECTION
		if ( GlobalManager.Configs.bGetSection( moduleSectionName, ref m_ModuleSection ) == false )			// Get Module Section
			return false;

		// GET FIRE MODE SECTION NAME
		string weaponFireModeSectionName = null;
		if ( m_ModuleSection.bAsString( "FireMode", ref weaponFireModeSectionName ) == false )
			return false;

		// LOAD FIRE MODE
		weaponFireModeSectionName = "WPN_FireMode_" + weaponFireModeSectionName;
		if ( TryLoadFireMode( m_FireModeContainer, weaponFireModeSectionName, ref m_WpnFireMode ) == false )
			return false;

//		m_WpnFireMode.transform.SetParent( m_FireModeContainer.transform );

		// ASSIGN INTERNALS
		m_ShotDelay				= m_ModuleSection.AsFloat( "BaseShotDelay", m_ShotDelay );
		m_MagazineCapacity		= m_ModuleSection.AsUInt( "BaseMagazineCapacity", m_MagazineCapacity );
		m_Damage				= m_ModuleSection.AsFloat( "BaseDamage", m_Damage );
		m_CanPenetrate			= m_ModuleSection.AsBool( "bCanPenetrate" );
		m_CamDeviation			= m_ModuleSection.AsFloat( "BaseCamDeviation", m_CamDeviation );
		m_FireDispersion		= m_ModuleSection.AsFloat( "BaseFireDispersion", m_FireDispersion );
		m_Recoil				= m_ModuleSection.AsFloat( "BaseRecoil", m_Recoil);
		m_CanPenetrate			= m_ModuleSection.AsBool( "bCanPenetrate" );
		m_Magazine				= m_MagazineCapacity;

		// CREATE FIRE MODE
		m_WpnFireMode.Setup( this, m_ShotDelay, Shoot );


		if ( InternalSetup( m_ModuleSection ) == false )
			return false;


		// AUDIO
		{
			// Load fire sounds collection
			if ( m_ModuleSounds == null )
			{
				const string fireSoundCollectionPath = "Scriptables/WeaponsFireSound";
				m_ModuleSounds = Resources.Load<AudioCollection>( fireSoundCollectionPath );
			}

			string fireSound = null;
			if ( m_ModuleSection.bAsString( "FireSound", ref fireSound ) )
			{
				AudioSource source = m_FireModeContainer.GetOrAddIfNotFound<AudioSource>();
				{
					source.playOnAwake = false;
					if ( source.clip = System.Array.Find( m_ModuleSounds.AudioClips, s => s.name == fireSound ) )
					{
						DynamicCustomAudioSource audioSource = m_FireModeContainer.GetOrAddIfNotFound<DynamicCustomAudioSource>();
						audioSource.enabled = true;

						audioSource.Setup( source );
						m_AudioSourceFire = audioSource;
					}
				}
			}
		}

		// BULLET POOL
		{
			// Remove previous if exists
			if ( m_PoolBullets != null )
			{
				m_PoolBullets.Destroy();
				m_PoolBullets = null;
			}

			// Create new pool
			string bulletObjectName = m_ModuleSection.AsString( "Bullet", "InvalidBulletResource" );

			ResourceManager.LoadedData<GameObject> loadedResource = new ResourceManager.LoadedData<GameObject>();
			bool bIsBulletLoaded = ResourceManager.LoadResourceSync( "Prefabs/Bullets/" + bulletObjectName, loadedResource );
			if ( bIsBulletLoaded == false )
			{
				Debug.Log( "WPN_FireModule::Setup: Cannot load bullet with name " + bulletObjectName + " for weapon " + wpn.Section.GetName() );
				Debug.Assert(false);
			}

			const bool bIsAsyncLoaded = true;

			GameObjectsPoolConstructorData<Bullet> data = new GameObjectsPoolConstructorData<Bullet>()
			{
				Model			= loadedResource.Asset,
				Size			= m_MagazineCapacity,
				ContainerName	= moduleSectionName + "_BulletsPool_" + wpn.Transform.name,
				ActionOnObject	= ActionOnBullet,
				IsAsyncBuild	= bIsAsyncLoaded,
			};

			m_PoolBullets = new GameObjectsPool<Bullet>( data );
		}

		return true;
	}

	
	//////////////////////////////////////////////////////////////////////////
	protected				void	ActionOnBullet( Bullet bullet )
	{
		bullet.SetActive( false );
		bullet.Setup
		(
			canPenetrate: m_CanPenetrate,
			whoRef: Player.Instance,
			weaponRef: m_WeaponRef as Weapon
		);
		bullet.gameObject.layer = LayerMask.NameToLayer("PlayerBullets");
//		Player.Instance.DisableCollisionsWith( bullet.Collider );
//		Physics.IgnoreCollision( Player.Entity.PhysicCollider, bullet.Collider, ignore: true );
	}


	//		MODIFIERS
	//////////////////////////////////////////////////////////////////////////
	protected	static	Database.Section GetCurrentConfiguration( Database.Section ModuleSection, List<Database.Section> Modifiers )
	{
		// Module Section values
		uint	MagazineCapacity		= ModuleSection.AsUInt( "BaseMagazineCapacity", 0u );
		float	Damage					= ModuleSection.AsFloat( "BaseDamage" );
		float	CamDeviation			= ModuleSection.AsFloat( "BaseCamDeviation" );
		float	FireDispersion			= ModuleSection.AsFloat( "BaseFireDispersion" );
		float	Recoil					= ModuleSection.AsFloat( "BaseRecoil");
		float	ShotDelay				= ModuleSection.AsFloat( "BaseShotDelay" );
		string	FireMode				= ModuleSection.AsString( "FireMode", "Single" );
		string	Bullet					= ModuleSection.AsString( "Bullet", "RifleBullet");
		bool	CanPenetrate			= ModuleSection.AsBool( "bCanPenetrate" );

		// Apply modifiers
		foreach( Database.Section mod in Modifiers )
		{
			float multCapacity		= mod.AsFloat( "MultMagazineCapacity", 1.0f );
			MagazineCapacity		= (uint)((float)MagazineCapacity * multCapacity );
			Damage					*= mod.AsFloat( "MultDamage", Damage );
			CamDeviation			*= mod.AsFloat( "MultCamDeviation", CamDeviation );
			FireDispersion			*= mod.AsFloat( "MultFireDispersion", FireDispersion );
			Recoil					*= mod.AsFloat( "MultRecoil", Recoil );
			ShotDelay				*= mod.AsFloat( "MultShotDelay", ShotDelay );

			FireMode				= mod.AsString( "FireMode", FireMode );
			Bullet					= mod.AsString( "Bullet", Bullet );
			CanPenetrate			= mod.AsUInt( "bCanPenetrate", 0u ) > 0 ? true : false;
		}

		// return the current configuration
		Database.Section config = new Database.Section( "LastConfigSection", ModuleSection.GetName() );
		config.Set( "MagazineCapacity",				MagazineCapacity	);
		config.Set( "Damage",						Damage				);
		config.Set( "CamDeviation",					CamDeviation		);
		config.Set( "FireDispersion",				FireDispersion		);
		config.Set( "Recoil",						Recoil				);
		config.Set( "ShotDelay",					ShotDelay			);
		config.Set( "FireMode",						FireMode			);
		config.Set( "Bullet",						Bullet				);
		config.Set( "bCanPenetrate",				CanPenetrate		);
		return config;
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void	ApplyModifier( Database.Section modifier )
	{
		m_Modifiers.Add( modifier );

		Database.Section Configuration = GetCurrentConfiguration( m_ModuleSection, m_Modifiers );

		ApplyConfiguration( Configuration );
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void	ResetBaseConfiguration()
	{
		// Reset everything of this module
		m_Modifiers.Clear();
		Setup( m_WeaponRef, m_ModuleSlot );
	}
	

	//////////////////////////////////////////////////////////////////////////
	public		override	void	RemoveModifier( Database.Section modifier )
	{
		int indexOfModifier = m_Modifiers.IndexOf( modifier );
		if ( indexOfModifier < 0 )
		{
			return;
		}
		
		m_Modifiers.RemoveAt( indexOfModifier );
		
		Database.Section Configuration = GetCurrentConfiguration( m_ModuleSection, m_Modifiers );

		ApplyConfiguration( Configuration );
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	ApplyConfiguration( Database.Section Configuration )
	{
		// MAGAZINE
		m_MagazineCapacity					= Configuration.AsUInt( "MagazineCapacity", m_MagazineCapacity );
		m_PoolBullets.Resize( m_Magazine = m_MagazineCapacity );

		// DAMAGE
		int CanPenetrate					= Configuration.As<int>( "bCanPenetrate" );		// 0 no change, 1 disable, 2 enable
		m_CanPenetrate						= CanPenetrate == 0 ? m_CanPenetrate : CanPenetrate == 0 ? false : true;
		m_PoolBullets.ExecuteActionOnObjectr( ActionOnBullet );
		m_Damage							= Configuration.AsFloat( "Damage" );


		// DEVIATION AND DISPERSION
		m_CamDeviation						= Configuration.AsFloat( "CamDeviation" );
		m_FireDispersion					= Configuration.AsFloat( "FireDispersion");
		m_Recoil							= Configuration.AsFloat( "Recoil");


		// FIRE MODE
		string newFireModeSecName			= Configuration.AsString( "FireMode" );
		ChangeFireMode( newFireModeSecName );
		m_ShotDelay							= Configuration.AsFloat( "ShotDelay");
		m_WpnFireMode.Setup( this, m_ShotDelay, Shoot );

		// BULLET
		string bulletObjectName				= Configuration.AsString( "Bullet" );
		GameObject bulletGO = null;
		if ( ( bulletGO = Resources.Load<GameObject>( "Prefabs/Bullets/" + bulletObjectName ) ) != null )
		{
//			m_PoolBullets.Convert( bulletGO, ActionOnBullet );
		}
	}


	//		FIREMODE
	//////////////////////////////////////////////////////////////////////////
	public					bool	ChangeFireMode( string FireMode )
	{
		FireMode = "WPN_FireMode_" + FireMode;
		return TryLoadFireMode( m_FireModeContainer, FireMode, ref m_WpnFireMode );
	}


	//////////////////////////////////////////////////////////////////////////
	public					bool	ChangeFireMode<T>()
	{
		return TryLoadFireMode( m_FireModeContainer, typeof(T).Name, ref m_WpnFireMode );
	}


	//////////////////////////////////////////////////////////////////////////
	private	static			bool	TryLoadFireMode( GameObject container, string weaponFireModeSectionName, ref WPN_FireMode_Base fireMode )
	{
		System.Type type = System.Type.GetType( weaponFireModeSectionName.Trim() );
		if ( type == null )
		{
			Debug.Log( "WPN_FireModule:Setting invalid weapon fire mode \"" + weaponFireModeSectionName + "\"" );
			return false;
		}
		
		if ( fireMode != null )
		{
			if ( fireMode.GetType() == type )
			{
				return true; // same firemode, change masked as success
			}
			else
			{
				Object.Destroy( fireMode );
			}
		}
			
		// Check module type as child of WPN_BaseModule
		if ( type.IsSubclassOf( typeof( WPN_FireMode_Base ) ) == false )
		{
			Debug.Log( "WPN_FireModule:Class Requested is not a supported weapon fire mode, \"" + weaponFireModeSectionName + "\"" );
			return false;
		}

		Database.Section section = null;
		if ( GlobalManager.Configs.bGetSection( weaponFireModeSectionName, ref section ) == false )
		{
			Debug.Log( "WPN_FireModule: CAnnot find section for fire mode \"" + weaponFireModeSectionName + "\"" );
			return false;
		}

		fireMode = container.AddComponent( type ) as WPN_FireMode_Base;
		return true;
	}


	//		DISPERSION
	//////////////////////////////////////////////////////////////////////////
	protected	virtual		float	GetFireDispersion()
	{
		return m_FireDispersion;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		float	GetCamDeviation()
	{
		return m_CamDeviation;
	}


	//		SHOOT ACTION
	//////////////////////////////////////////////////////////////////////////
	protected	abstract	void	Shoot( float moduleFireDispersion, float moduleCamDeviation );


	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		if ( m_PoolBullets != null )
		{
			m_PoolBullets.Destroy();
		}

		if ( m_FireModeContainer )
		{
			Destroy( m_FireModeContainer );
		}
	}

}
