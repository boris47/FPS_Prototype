
using System.Collections.Generic;
using UnityEngine;


public interface IWPN_FireModule
{
	EFireMode				FireMode						{ get; }

	uint					Magazine						{ get; }
	uint					MagazineCapacity				{ get; }

	float					CamDeviation					{ get; }
	float					FireDispersion					{ get; }

	bool					NeedReload						();
	bool					ChangeFireMode					( string FireMode );
	bool					ChangeFireMode				<T>	() where T : WPN_BaseFireMode, new();
}

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
	[SerializeField]	protected	WPN_BaseFireMode			m_WpnFireMode				= null;

	// INTERFACE START
	public abstract	EFireMode									FireMode					{ get; }
	public			Vector3										FirePointPosition			=> m_FirePoint.position; 
	public			Quaternion									FirePointRotation			=> m_FirePoint.rotation;

	public			uint										Magazine					=> m_Magazine;
	public			uint										MagazineCapacity			=> m_MagazineCapacity;

	public			float										CamDeviation				=> m_CamDeviation;
	public			float										FireDispersion				=> m_FireDispersion;
	// INTERFACE END

	protected		GameObjectsPool<Bullet>						m_PoolBullets				= null;
	protected		bool										m_Initialized				= false;
	protected		CustomAudioSource							m_AudioSourceFire			= null;

	protected		UI_BaseCrosshair							m_UI_Crosshair				= null;
	//		SETUP
	//////////////////////////////////////////////////////////////////////////
	public		override	bool	OnAttach( IWeapon wpn, EWeaponSlots slot )
	{
		string moduleSectionName = GetType().FullName;

		m_WeaponRef = wpn;
		m_ModuleSlot = slot;

		m_FirePoint = wpn.Transform.Find( "FirePoint" );

		// MODULE CONTAINER
		string containerID = Weapon.GetModuleSlotName( slot );
		m_FireModeContainer = transform.Find(containerID)?.gameObject;
		if (m_FireModeContainer != null )
		{
			Destroy(m_FireModeContainer );
		}
		m_FireModeContainer = new GameObject( containerID );
		m_FireModeContainer.transform.SetParent(transform );
		m_FireModeContainer.transform.localPosition = Vector3.zero;
		m_FireModeContainer.transform.localRotation = Quaternion.identity;

		// TRY RECOVER MODULE SECTION
		if ( !GlobalManager.Configs.TryGetSection( moduleSectionName, out m_ModuleSection ) )			// Get Module Section
			return false;

		// GET FIRE MODE SECTION NAME
		string weaponFireModeSectionName = null;
		if ( !m_ModuleSection.TryAsString( "FireMode", out weaponFireModeSectionName ) ) 
			return false;

		// LOAD FIRE MODE
		if ( !ChangeFireMode( weaponFireModeSectionName ) ) 
			return false;

		//		m_WpnFireMode.transform.SetParent( m_FireModeContainer.transform );

		// ASSIGN INTERNALS
		m_ShotDelay				= m_ModuleSection.AsFloat( "BaseShotDelay", m_ShotDelay );
		m_MagazineCapacity		= m_ModuleSection.AsUInt( "BaseMagazineCapacity", m_MagazineCapacity );
		m_CamDeviation			= m_ModuleSection.AsFloat( "BaseCamDeviation", m_CamDeviation );
		m_FireDispersion		= m_ModuleSection.AsFloat( "BaseFireDispersion", m_FireDispersion );
		m_Recoil				= m_ModuleSection.AsFloat( "BaseRecoil", m_Recoil);
		m_Magazine				= m_MagazineCapacity;

		// CREATE FIRE MODE
		m_WpnFireMode.Setup( this, m_ShotDelay, Shoot );

		if (InternalSetup(m_ModuleSection ) == false )
			return false;

		// AUDIO
		{
			// Load fire sounds collection
			if ( m_ModuleSounds == null )
			{
				const string fireSoundCollectionPath = "Scriptables/WeaponsFireSound";
				ResourceManager.LoadResourceSync( fireSoundCollectionPath, out m_ModuleSounds );
			}

			if (m_ModuleSection.TryAsString( "FireSound", out string fireSound ) )
			{
				AudioSource source = m_FireModeContainer.GetOrAddIfNotFound<AudioSource>();
				source.playOnAwake = false;
				if ( source.clip = System.Array.Find( m_ModuleSounds.AudioClips, s => s.name == fireSound ) )
				{
					DynamicCustomAudioSource audioSource = m_FireModeContainer.GetOrAddIfNotFound<DynamicCustomAudioSource>();
					audioSource.Setup( source );
					if (!audioSource.enabled) audioSource.enabled = true;

					m_AudioSourceFire = audioSource;
				}
				
			}
		}

		// BULLET POOL
		{
			// Remove previous if exists
			if (m_PoolBullets != null )
			{
				m_PoolBullets.Destroy();
				m_PoolBullets = null;
			}

			// Get Bullet section
			string bulletSectionName = m_ModuleSection.AsString( "Bullet", "InvalidBulletResource" );

			// Load bullet model
			CustomAssertions.IsTrue
			(
				Bullet.TryGetBulletModel(bulletSectionName, out GameObject loadedResource),
				$"WPN_FireModule::Setup: Cannot load bullet with name {bulletSectionName} for weapon {wpn.Section.GetSectionName()}"
			);

			// Create pool
			GameObjectsPoolConstructorData<Bullet> data = new GameObjectsPoolConstructorData<Bullet>(loadedResource, m_MagazineCapacity)
			{
				ContainerName	= $"{moduleSectionName}_BulletsPool_{wpn.Transform.name}",
				ActionOnObject	= ActionOnBullet,
				IsAsyncBuild	= true,
			};
			m_PoolBullets = new GameObjectsPool<Bullet>( data );
		}

		if (!m_UI_Crosshair)
		{
			if (m_ModuleSection.TryAsString("Crosshair", out string crosshairSection))
			{
				m_UI_Crosshair = UIManager.InGame.EnableCrosshair( System.Type.GetType( crosshairSection ) );
				m_UI_Crosshair.SetMin(m_FireDispersion*2f);
			}
			Debug.Assert( m_UI_Crosshair.IsNotNull(), $"Crosshair is invalid" );
		}

		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public override void OnDetach()
	{
		UIManager.InGame.RemoveCrosshair( m_UI_Crosshair );
	}


	//////////////////////////////////////////////////////////////////////////
	protected				void	ActionOnBullet( Bullet bullet )
	{
		bullet.SetActive( false );
		bullet.Setup
		(
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
		m_Modifiers.Add(modifier);

		Database.Section Configuration = GetCurrentConfiguration(m_ModuleSection, m_Modifiers);

		ApplyConfiguration(Configuration);
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void	ResetBaseConfiguration()
	{
		// Reset everything of this module
		m_Modifiers.Clear();
		OnDetach();
		OnAttach(m_WeaponRef, m_ModuleSlot);
	}
	

	//////////////////////////////////////////////////////////////////////////
	public		override	void	RemoveModifier( Database.Section modifier )
	{
		int indexOfModifier = m_Modifiers.IndexOf( modifier );
		if ( indexOfModifier < 0 )
		{
			return;
		}

		m_Modifiers.RemoveAt(indexOfModifier);

		Database.Section Configuration = GetCurrentConfiguration(m_ModuleSection, m_Modifiers);

		ApplyConfiguration(Configuration);
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	ApplyConfiguration( Database.Section Configuration )
	{
		// MAGAZINE
		m_MagazineCapacity					= Configuration.AsUInt("MagazineCapacity", m_MagazineCapacity);
		m_PoolBullets.Resize(m_Magazine = m_MagazineCapacity);

		// DAMAGE
		m_PoolBullets.ExecuteActionOnObjects(ActionOnBullet);

		// DEVIATION AND DISPERSION
		m_CamDeviation						= Configuration.AsFloat("CamDeviation");
		m_FireDispersion					= Configuration.AsFloat("FireDispersion");
		m_Recoil							= Configuration.AsFloat("Recoil");

		// FIRE MODE
		string newFireModeSecName			= Configuration.AsString("FireMode");
		ChangeFireMode(newFireModeSecName);
		m_ShotDelay							= Configuration.AsFloat("ShotDelay");
		m_WpnFireMode.Setup(this, m_ShotDelay, Shoot);

		// BULLET
		string bulletSectionName			= Configuration.AsString("Bullet");
		if (bulletSectionName != m_PoolBullets.PeekComponent().GetType().Name)
		{
			if (Bullet.TryGetBulletModel(bulletSectionName, out GameObject model))
			{
				m_PoolBullets.Convert(model, ActionOnBullet);
			}
		}
		m_UI_Crosshair.SetMin(m_FireDispersion*2f);
	}


	//		FIREMODE
	//////////////////////////////////////////////////////////////////////////
	public					bool	ChangeFireMode( string weaponFireModeSectionName )
	{
		weaponFireModeSectionName = $"WPN_FireMode_{weaponFireModeSectionName}";
		System.Type type = System.Type.GetType(weaponFireModeSectionName.Trim());
		if (type == null)
		{
			Debug.Log($"WPN_FireModule:ChangeFireMode:Setting invalid weapon fire mode \"{weaponFireModeSectionName}\"");
			return false;
		}
		return TryLoadFireMode(m_FireModeContainer, type, ref m_WpnFireMode);
	}


	//////////////////////////////////////////////////////////////////////////
	public					bool	ChangeFireMode<T>() where T : WPN_BaseFireMode, new()
	{
		return TryLoadFireMode(m_FireModeContainer, typeof(T), ref m_WpnFireMode);
	}


	//////////////////////////////////////////////////////////////////////////
	private	static			bool	TryLoadFireMode( GameObject container, System.Type type, ref WPN_BaseFireMode fireMode )
	{
		if (fireMode.IsNotNull())
		{
			if (fireMode.GetType() == type)
			{
				return true; // same firemode, change masked as success
			}
			else
			{
				Object.Destroy(fireMode);
			}
		}

		string weaponFireModeSectionName = type.Name;

		// Check module type as child of WPN_BaseModule
		if (!type.IsSubclassOf(typeof(WPN_BaseFireMode)))
		{
			Debug.Log($"WPN_FireModule:Class Requested is not a supported weapon fire mode, \"{weaponFireModeSectionName}\"");
			return false;
		}

		if (!GlobalManager.Configs.TryGetSection(weaponFireModeSectionName, out Database.Section section))
		{
			Debug.Log($"WPN_FireModule: Cannot find section for fire mode \"{weaponFireModeSectionName}\"");
			return false;
		}

		fireMode = container.AddComponent(type) as WPN_BaseFireMode;
		return true;
	}


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


	//////////////////////////////////////////////////////////////////////////
	protected	abstract	void	Shoot( float moduleFireDispersion, float moduleCamDeviation );


	//////////////////////////////////////////////////////////////////////////
	protected void OnDestroy()
	{
		if (m_PoolBullets.IsNotNull())
		{
			m_PoolBullets.Destroy();
		}

		if (m_FireModeContainer)
		{
			Destroy(m_FireModeContainer);
		}
	}
}
