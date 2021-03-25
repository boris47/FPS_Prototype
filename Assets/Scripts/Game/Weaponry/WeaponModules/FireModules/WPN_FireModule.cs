
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
	void					ChangeFireMode					(string FireMode);
	void					ChangeFireMode				<T> () where T : WPN_BaseFireMode, new();
}

/// <summary> Abstract base class for fire modules </summary>
public abstract class WPN_FireModule : WPN_BaseModule, IWPN_FireModule
{
	private	const		string						FIRE_SOUND_COLLECTION_PATH	= "Scriptables/WeaponsFireSound";
	private	static		AudioCollection				m_ModuleSounds				= null;

	[SerializeField]
	protected			Transform					m_FirePoint					= null;
	[SerializeField]
	protected			uint						m_Magazine					= 0;
	[SerializeField]
	protected			uint						m_MagazineCapacity			= 1;
	[SerializeField]
	protected			uint						m_BaseBulletsPerShot			= 1;
	[SerializeField]
	protected			float						m_ShotDelay					= 0.5f;
	[SerializeField]
	protected			float						m_CamDeviation				= 0.02f;
	[SerializeField]
	protected			float						m_FireDispersion			= 0.05f;
	[SerializeField]
	protected			float						m_Recoil					= 0.3f;
	[SerializeField]
	protected			WPN_BaseFireMode			m_WpnFireMode				= null;

	protected			GameObjectsPool<Bullet>		m_PoolBullets				= null;
	protected			CustomAudioSource			m_AudioSourceFire			= null;
	protected			UI_BaseCrosshair			m_UI_Crosshair				= null;
		
	public abstract		EFireMode					FireMode					{ get; }
	public				Vector3						FirePointPosition			=> m_FirePoint.position; 
	public				Quaternion					FirePointRotation			=> m_FirePoint.rotation;
	public				uint						Magazine					=> m_Magazine;
	public				uint						MagazineCapacity			=> m_MagazineCapacity;
	public				uint						BaseBulletsPerShot			=> m_BaseBulletsPerShot;
	public				float						CamDeviation				=> m_CamDeviation;
	public				float						FireDispersion				=> m_FireDispersion;



	//////////////////////////////////////////////////////////////////////////
	protected virtual void Awake()
	{
		// Load fire sounds collection
		if (!m_ModuleSounds)
		{
			CustomAssertions.IsTrue(ResourceManager.LoadResourceSync(FIRE_SOUND_COLLECTION_PATH, out m_ModuleSounds));
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public override bool OnAttach(IWeapon wpn, EWeaponSlots slot)
	{
		m_WeaponRef = wpn;
		m_ModuleSlot = slot;

		CustomAssertions.IsNotNull(m_FirePoint = wpn.Transform.Find("FirePoint"));

		// MODULE CONTAINER
		string containerID = Weapon.GetModuleSlotName(slot);
		m_FireModeContainer = transform.Find(containerID)?.gameObject;
		if (m_FireModeContainer.IsNotNull())
		{
			Destroy(m_FireModeContainer);
		}
		m_FireModeContainer = new GameObject(containerID);
		m_FireModeContainer.transform.SetParent(transform);
		m_FireModeContainer.transform.localPosition = Vector3.zero;
		m_FireModeContainer.transform.localRotation = Quaternion.identity;

		string moduleSectionName = GetType().FullName;

		// RECOVER MODULE SECTION
		CustomAssertions.IsTrue(GlobalManager.Configs.TryGetSection(moduleSectionName, out m_ModuleSection));

		// APPLY FIRE MODE
		if (CustomAssertions.IsTrue(m_ModuleSection.TryAsString("FireMode", out string weaponFireModeSectionName)))
		{
			ChangeFireMode(weaponFireModeSectionName);
		}

		// ASSIGN INTERNALS
		m_ShotDelay				= m_ModuleSection.AsFloat( "BaseShotDelay", m_ShotDelay );
		m_MagazineCapacity		= m_ModuleSection.AsUInt( "BaseMagazineCapacity", m_MagazineCapacity );
		m_BaseBulletsPerShot		= m_ModuleSection.AsUInt( "BaseBulletsPerShot", m_BaseBulletsPerShot );
		m_CamDeviation			= m_ModuleSection.AsFloat( "BaseCamDeviation", m_CamDeviation );
		m_FireDispersion		= m_ModuleSection.AsFloat( "BaseFireDispersion", m_FireDispersion );
		m_Recoil				= m_ModuleSection.AsFloat( "BaseRecoil", m_Recoil);
		m_Magazine				= m_MagazineCapacity;

		// CREATE FIRE MODE
		m_WpnFireMode.Setup(this, m_ShotDelay, Shoot);

		CustomAssertions.IsTrue(InternalSetup(m_ModuleSection));

		// AUDIO
		if (CustomAssertions.IsTrue(m_ModuleSection.TryAsString("FireSound", out string fireSound)))
		{
			AudioSource source = m_FireModeContainer.GetOrAddIfNotFound<AudioSource>();
			source.playOnAwake = false;
			if (source.clip = System.Array.Find(m_ModuleSounds.AudioClips, s => s.name == fireSound))
			{
				DynamicCustomAudioSource audioSource = m_FireModeContainer.GetOrAddIfNotFound<DynamicCustomAudioSource>();
				{
					audioSource.Setup(source);
					audioSource.enabled = true;
				}
				m_AudioSourceFire = audioSource;
			}
		}

		// BULLET POOL
		{
			// Remove previous if exists
			if (m_PoolBullets.IsNotNull())
			{
				m_PoolBullets.Destroy();
				m_PoolBullets = null;
			}

			// Get Bullet section
			string bulletSectionName = m_ModuleSection.AsString("Bullet", "InvalidBulletResource");

			// Get bullet model
			Bullet.GetBulletModel(bulletSectionName, out GameObject loadedResource);

			// Create pool
			GameObjectsPoolConstructorData<Bullet> data = new GameObjectsPoolConstructorData<Bullet>(loadedResource, m_MagazineCapacity * m_BaseBulletsPerShot)
			{
				ContainerName = $"{moduleSectionName}_BulletsPool_{wpn.Transform.name}",
				ActionOnObject = ActionOnBullet,
				IsAsyncBuild = true,
			};
			m_PoolBullets = new GameObjectsPool<Bullet>(data);
		}

		if (!m_UI_Crosshair)
		{
			if (m_ModuleSection.TryAsString("Crosshair", out string crosshairSection))
			{
				m_UI_Crosshair = UIManager.InGame.EnableCrosshair(System.Type.GetType(crosshairSection));
				m_UI_Crosshair.SetMin(m_FireDispersion * 2f);
			}
			CustomAssertions.IsNotNull(m_UI_Crosshair, $"Crosshair is invalid");
		}

		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public override void OnDetach()
	{
		UIManager.InGame.RemoveCrosshair( m_UI_Crosshair );
	}


	//////////////////////////////////////////////////////////////////////////
	protected void ActionOnBullet(Bullet bullet)
	{
		bullet.Setup
		(
			whoRef: m_WeaponRef.Owner,
			weaponRef: m_WeaponRef as Weapon
		);
	//	bullet.gameObject.layer = LayerMask.NameToLayer("PlayerBullets");
	}


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
		foreach (Database.Section mod in Modifiers)
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
	public override void ApplyModifier(Database.Section modifier)
	{
		m_Modifiers.Add(modifier);

		Database.Section Configuration = GetCurrentConfiguration(m_ModuleSection, m_Modifiers);

		ApplyConfiguration(Configuration);
	}


	//////////////////////////////////////////////////////////////////////////
	public override void ResetBaseConfiguration()
	{
		// Reset everything of this module
		m_Modifiers.Clear();
		OnDetach();
		OnAttach(m_WeaponRef, m_ModuleSlot);
	}


	//////////////////////////////////////////////////////////////////////////
	public override void RemoveModifier(Database.Section modifier)
	{
		int indexOfModifier = m_Modifiers.IndexOf(modifier);
		if (indexOfModifier < 0)
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
		m_PoolBullets.Resize(m_Magazine		= m_MagazineCapacity);

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
			Bullet.GetBulletModel(bulletSectionName, out GameObject model);
			m_PoolBullets.Convert(model, ActionOnBullet);
		}
		m_UI_Crosshair.SetMin(m_FireDispersion * 2f);
	}


	//////////////////////////////////////////////////////////////////////////
	public void ChangeFireMode(string weaponFireModeSectionName)
	{
		weaponFireModeSectionName = $"WPN_FireMode_{weaponFireModeSectionName}";
		System.Type type = System.Type.GetType(weaponFireModeSectionName.Trim());
		CustomAssertions.IsNotNull(type, $"Setting invalid weapon fire mode \"{weaponFireModeSectionName}\"");
		LoadFireMode(m_FireModeContainer, type, ref m_WpnFireMode);
	}


	//////////////////////////////////////////////////////////////////////////
	public void ChangeFireMode<T>() where T : WPN_BaseFireMode, new()
	{
		LoadFireMode(m_FireModeContainer, typeof(T), ref m_WpnFireMode);
	}


	//////////////////////////////////////////////////////////////////////////
	private static void LoadFireMode(in GameObject container, in System.Type type, ref WPN_BaseFireMode fireMode)
	{
		if (fireMode.IsNotNull())
		{
			if (fireMode.GetType() != type)
			{
				Object.Destroy(fireMode);
			}
		}

		string weaponFireModeSectionName = type.Name;

		// Check module type as child of WPN_BaseModule
		CustomAssertions.IsTrue(type.IsSubclassOf(typeof(WPN_BaseFireMode)), $"Class Requested is not a supported weapon fire mode, \"{weaponFireModeSectionName}\"");
		
		// Check and get fire mode section
		CustomAssertions.IsTrue(GlobalManager.Configs.TryGetSection(weaponFireModeSectionName, out Database.Section section), $"Cannot find section for fire mode \"{weaponFireModeSectionName}\"");

		// Assign new fire mode
		fireMode = container.AddComponent(type) as WPN_BaseFireMode;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual float GetFireDispersion()
	{
		return m_FireDispersion;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual float GetCamDeviation()
	{
		return m_CamDeviation;
	}


	//////////////////////////////////////////////////////////////////////////
	protected abstract void Shoot(float moduleFireDispersion, float moduleCamDeviation);


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
