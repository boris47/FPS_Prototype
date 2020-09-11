
using System.Collections.Generic;
using UnityEngine;


/// <summary> Abstract base class for fire modules </summary>
[System.Serializable]
public abstract class WPN_FireModule : WPN_BaseModule, IWPN_FireModule
{
	private	static		AudioCollection							m_ModuleSounds				= null;

	[SerializeField]	protected	Transform					m_FirePoint					= null;
	[SerializeField]	protected	uint						m_Magazine					= 0;
	[SerializeField]	protected	uint						m_MagazineCapacity			= 1;
	[SerializeField]	protected	float						m_ShotDelay					= 0.5f;
	[SerializeField]	protected	float						m_CamDeviation				= 0.02f;
	[SerializeField]	protected	float						m_FireDispersion			= 0.05f;
	[SerializeField]	protected	float						m_Recoil					= 0.3f;
	[SerializeField]	protected	WPN_FireMode_Base			m_WpnFireMode				= null;

	// INTERFACE START
	public abstract	EFireMode									FireMode					{ get; }
	public			Vector3										FirePointPosition			=> this.m_FirePoint.position; 
	public			Quaternion									FirePointRotation			=> this.m_FirePoint.rotation;

	public			uint										Magazine					=> this.m_Magazine;
	public			uint										MagazineCapacity			=> this.m_MagazineCapacity;

	public			float										CamDeviation				=> this.m_CamDeviation;
	public			float										FireDispersion				=> this.m_FireDispersion;
	// INTERFACE END

	protected		GameObjectsPool<Bullet>						m_PoolBullets				= null;
	protected		bool										m_Initialized				= false;
	protected		CustomAudioSource							m_AudioSourceFire			= null;


	//		SETUP
	//////////////////////////////////////////////////////////////////////////
	public		override	bool	OnAttach( IWeapon wpn, EWeaponSlots slot )
	{
		string moduleSectionName = this.GetType().FullName;

		this.m_WeaponRef = wpn;
		this.m_ModuleSlot = slot;

		this.m_FirePoint = wpn.Transform.Find( "FirePoint" );

		// MODULE CONTAINER
		string containerID = Weapon.GetModuleSlotName( slot );
		this.m_FireModeContainer = this.transform.Find(containerID)?.gameObject;
		if (this.m_FireModeContainer != null )
		{
			Destroy(this.m_FireModeContainer );
		}
		this.m_FireModeContainer = new GameObject( containerID );
		this.m_FireModeContainer.transform.SetParent(this.transform );
		this.m_FireModeContainer.transform.localPosition = Vector3.zero;
		this.m_FireModeContainer.transform.localRotation = Quaternion.identity;
		

		// TRY RECOVER MODULE SECTION
		if ( !GlobalManager.Configs.GetSection( moduleSectionName, ref this.m_ModuleSection ) )			// Get Module Section
			return false;

		// GET FIRE MODE SECTION NAME
		string weaponFireModeSectionName = null;
		if ( !this.m_ModuleSection.bAsString( "FireMode", ref weaponFireModeSectionName ) ) 
			return false;

		// LOAD FIRE MODE
		if ( !this.ChangeFireMode( weaponFireModeSectionName ) ) 
			return false;

		//		m_WpnFireMode.transform.SetParent( m_FireModeContainer.transform );

		// ASSIGN INTERNALS
		this.m_ShotDelay			= this.m_ModuleSection.AsFloat( "BaseShotDelay", this.m_ShotDelay );
		this.m_MagazineCapacity		= this.m_ModuleSection.AsUInt( "BaseMagazineCapacity", this.m_MagazineCapacity );
		this.m_CamDeviation			= this.m_ModuleSection.AsFloat( "BaseCamDeviation", this.m_CamDeviation );
		this.m_FireDispersion		= this.m_ModuleSection.AsFloat( "BaseFireDispersion", this.m_FireDispersion );
		this.m_Recoil				= this.m_ModuleSection.AsFloat( "BaseRecoil", this.m_Recoil);
		this.m_Magazine				= this.m_MagazineCapacity;

		// CREATE FIRE MODE
		this.m_WpnFireMode.Setup( this, this.m_ShotDelay, this.Shoot );

		if (this.InternalSetup(this.m_ModuleSection ) == false )
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
			if (this.m_ModuleSection.bAsString( "FireSound", ref fireSound ) )
			{
				AudioSource source = this.m_FireModeContainer.GetOrAddIfNotFound<AudioSource>();
				{
					source.playOnAwake = false;
					if ( source.clip = System.Array.Find( m_ModuleSounds.AudioClips, s => s.name == fireSound ) )
					{
						DynamicCustomAudioSource audioSource = this.m_FireModeContainer.GetOrAddIfNotFound<DynamicCustomAudioSource>();
						audioSource.Setup( source );
						if (!audioSource.enabled) audioSource.enabled = true;

						this.m_AudioSourceFire = audioSource;
					}
				}
			}
		}

		// BULLET POOL
		{
			// Remove previous if exists
			if (this.m_PoolBullets != null )
			{
				this.m_PoolBullets.Destroy();
				this.m_PoolBullets = null;
			}

			// Create new pool
			string bulletObjectName = this.m_ModuleSection.AsString( "Bullet", "InvalidBulletResource" );

			ResourceManager.LoadedData<GameObject> loadedResource = new ResourceManager.LoadedData<GameObject>();
			bool bIsBulletLoaded = ResourceManager.LoadResourceSync( "Prefabs/Bullets/" + bulletObjectName, loadedResource );

			UnityEngine.Assertions.Assert.IsTrue
			(
				bIsBulletLoaded,
				"WPN_FireModule::Setup: Cannot load bullet with name " + bulletObjectName + " for weapon " + wpn.Section.GetSectionName()
			);

			const bool bIsAsyncLoaded = true;

			GameObjectsPoolConstructorData<Bullet> data = new GameObjectsPoolConstructorData<Bullet>()
			{
				Model			= loadedResource.Asset,
				Size			= m_MagazineCapacity,
				ContainerName	= moduleSectionName + "_BulletsPool_" + wpn.Transform.name,
				ActionOnObject	= ActionOnBullet,
				IsAsyncBuild	= bIsAsyncLoaded,
			};

			this.m_PoolBullets = new GameObjectsPool<Bullet>( data );
		}

		foreach(IWeaponAttachment attachment in this.m_WeaponRef.Transform.GetComponentsInChildren<IWeaponAttachment>())
		{
			attachment.OnAttached();
		}

		return true;
	}

	public override void OnDetach()
	{
		foreach(IWeaponAttachment attachment in this.m_WeaponRef.Transform.GetComponentsInChildren<IWeaponAttachment>())
		{
			attachment.OnRemoved();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected				void	ActionOnBullet( Bullet bullet )
	{
		bullet.SetActive( false );
		bullet.Setup
		(
			whoRef: Player.Instance,
			weaponRef: this.m_WeaponRef as Weapon
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
		Database.Section config = new Database.Section( "LastConfigSection", ModuleSection.GetSectionName() );
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
		this.m_Modifiers.Add( modifier );

		Database.Section Configuration = GetCurrentConfiguration(this.m_ModuleSection, this.m_Modifiers );

		this.ApplyConfiguration( Configuration );
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void	ResetBaseConfiguration()
	{
		// Reset everything of this module
		this.m_Modifiers.Clear();
		this.OnAttach(this.m_WeaponRef, this.m_ModuleSlot );
	}
	

	//////////////////////////////////////////////////////////////////////////
	public		override	void	RemoveModifier( Database.Section modifier )
	{
		int indexOfModifier = this.m_Modifiers.IndexOf( modifier );
		if ( indexOfModifier < 0 )
		{
			return;
		}

		this.m_Modifiers.RemoveAt( indexOfModifier );
		
		Database.Section Configuration = GetCurrentConfiguration(this.m_ModuleSection, this.m_Modifiers );

		this.ApplyConfiguration( Configuration );
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	ApplyConfiguration( Database.Section Configuration )
	{
		// MAGAZINE
		this.m_MagazineCapacity					= Configuration.AsUInt( "MagazineCapacity", this.m_MagazineCapacity );
		this.m_PoolBullets.Resize(this.m_Magazine = this.m_MagazineCapacity );

		// DAMAGE
		this.m_PoolBullets.ExecuteActionOnObjects(this.ActionOnBullet );

		// DEVIATION AND DISPERSION
		this.m_CamDeviation						= Configuration.AsFloat( "CamDeviation" );
		this.m_FireDispersion					= Configuration.AsFloat( "FireDispersion");
		this.m_Recoil							= Configuration.AsFloat( "Recoil");

		// FIRE MODE
		string newFireModeSecName				= Configuration.AsString( "FireMode" );
		this.ChangeFireMode( newFireModeSecName );
		this.m_ShotDelay						= Configuration.AsFloat( "ShotDelay");
		this.m_WpnFireMode.Setup( this, this.m_ShotDelay, this.Shoot );

		// BULLET
		string bulletObjectName				= Configuration.AsString( "Bullet" );
		if ( bulletObjectName != this.m_PoolBullets.PeekComponent().GetType().Name )
		{
			ResourceManager.LoadedData<GameObject> loadedData = new ResourceManager.LoadedData<GameObject>();
			if ( ResourceManager.LoadResourceSync( "Prefabs/Bullets/" + bulletObjectName, loadedData ) )
			{
				this.m_PoolBullets.Convert( loadedData.Asset, this.ActionOnBullet );
			}
		}
	}


	//		FIREMODE
	//////////////////////////////////////////////////////////////////////////
	public					bool	ChangeFireMode( string weaponFireModeSectionName )
	{
		weaponFireModeSectionName = "WPN_FireMode_" + weaponFireModeSectionName;
		System.Type type = System.Type.GetType( weaponFireModeSectionName.Trim() );
		if ( type == null )
		{
			Debug.Log( "WPN_FireModule:ChangeFireMode:Setting invalid weapon fire mode \"" + weaponFireModeSectionName + "\"" );
			return false;
		}
		return TryLoadFireMode(this.m_FireModeContainer, type, ref this.m_WpnFireMode );
	}


	//////////////////////////////////////////////////////////////////////////
	public					bool	ChangeFireMode<T>() where T : WPN_FireMode_Base
	{
		return TryLoadFireMode(this.m_FireModeContainer, typeof(T), ref this.m_WpnFireMode );
	}


	//////////////////////////////////////////////////////////////////////////
	private	static			bool	TryLoadFireMode( GameObject container, System.Type type, ref WPN_FireMode_Base fireMode )
	{
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

		string weaponFireModeSectionName = type.Name;
			
		// Check module type as child of WPN_BaseModule
		if ( type.IsSubclassOf( typeof( WPN_FireMode_Base ) ) == false )
		{
			Debug.Log( "WPN_FireModule:Class Requested is not a supported weapon fire mode, \"" + weaponFireModeSectionName + "\"" );
			return false;
		}

		Database.Section section = null;
		if ( GlobalManager.Configs.GetSection( weaponFireModeSectionName, ref section ) == false )
		{
			Debug.Log( "WPN_FireModule: Cannot find section for fire mode \"" + weaponFireModeSectionName + "\"" );
			return false;
		}

		fireMode = container.AddComponent( type ) as WPN_FireMode_Base;
		return true;
	}


	//		DISPERSION
	//////////////////////////////////////////////////////////////////////////
	protected	virtual		float	GetFireDispersion()
	{
		return this.m_FireDispersion;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		float	GetCamDeviation()
	{
		return this.m_CamDeviation;
	}


	//		SHOOT ACTION
	//////////////////////////////////////////////////////////////////////////
	protected	abstract	void	Shoot( float moduleFireDispersion, float moduleCamDeviation );


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDestroy()
	{
		base.OnDestroy();

		if (this.m_PoolBullets != null )
		{
			this.m_PoolBullets.Destroy();
		}

		if (this.m_FireModeContainer )
		{
			Destroy( this.m_FireModeContainer );
		}
	}

}
