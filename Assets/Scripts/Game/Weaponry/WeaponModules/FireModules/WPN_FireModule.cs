
using System.Collections.Generic;
using UnityEngine;


public interface IWPN_FireModule
{
	EFireMode				FireMode						{ get; }

	uint					Magazine						{ get; }
	uint					MagazineCapacity				{ get; }

	bool					NeedReload						();
	bool					TryChangeFireMode				(string FireMode);
	bool					TryChangeFireMode				<T> () where T : WPN_FireModeBase, new();
}

/// <summary> Abstract base class for fire modules </summary>
public abstract partial class WPN_FireModule : WPN_BaseModule, IWPN_FireModule
{
	private	const		string								FIRE_SOUND_COLLECTION_PATH		= "Scriptables/WeaponsFireSound";
	private	static		AudioCollection						m_ModuleSounds					= null;

	[System.Serializable]
	public class FireModuleData
	{
		[SerializeField]
		public			uint								MagazineCapacity				= 1u;
		[SerializeField]
		public			uint								BulletsPerShot					= 1u;
		[SerializeField]
		public			float								BulletDamage					= 10f;
		[SerializeField]
		public			float								BulletVelocity					= 60f;
		[SerializeField]
		public			float								ImpactForceMultiplier			= 0.0f;
		[SerializeField]
		public			float								ShotDelay						= 0.5f;
		[SerializeField]
		public			float								CamDeviation					= 0.02f;
		[SerializeField]
		public			float								FireDispersion					= 0.05f;
		[SerializeField]
		public			float								Recoil							= 0.3f;
		[SerializeField]
		public			string								FireMode						= "None";
		[SerializeField]
		public			string								BulletSection					= "None";
		
		public void AssignFrom(FireModuleData other)
		{
			MagazineCapacity = other.MagazineCapacity;
			BulletsPerShot = other.BulletsPerShot;
			BulletDamage = other.BulletDamage;
			BulletVelocity = other.BulletVelocity;
			ImpactForceMultiplier = other.ImpactForceMultiplier;
			ShotDelay = other.ShotDelay;
			CamDeviation = other.CamDeviation;

			FireDispersion = other.FireDispersion;
			Recoil = other.Recoil;
			FireMode = !other.FireMode.IsNone() ? other.FireMode : FireMode;
			BulletSection = !other.BulletSection.IsNone() ?  other.BulletSection : BulletSection;
		}
	}

	[SerializeField]
	public				uint								m_Magazine						= 0u;

	[SerializeField]
	protected			FireModuleData						m_FireModuleData				= new FireModuleData();

	[SerializeField]
	protected			Transform							m_FirePoint						= null;
	[SerializeField, ReadOnly]
	protected			WPN_FireModeBase					m_WpnFireMode					= null;

	protected			GameObjectsPool<Bullet>				m_PoolBullets					= null;
	protected			CustomAudioSource					m_AudioSourceFire				= null;
	protected			UI_BaseCrosshair					m_UI_Crosshair					= null;
		
	public abstract		EFireMode							FireMode						{ get; }
	public				Vector3								FirePointPosition				=> m_FirePoint.position; 
	public				Quaternion							FirePointRotation				=> m_FirePoint.rotation;
	public				uint								Magazine						=> Magazine;
	public				uint								MagazineCapacity				=> m_FireModuleData.MagazineCapacity;
	public				uint								BaseBulletsPerShot				=> m_FireModuleData.BulletsPerShot;



	//////////////////////////////////////////////////////////////////////////
	protected override void Awake()
	{
		base.Awake();

		// Load fire sounds collection
		if (!m_ModuleSounds)
		{
			CustomAssertions.IsTrue(ResourceManager.LoadResourceSync(FIRE_SOUND_COLLECTION_PATH, out m_ModuleSounds));
		}

		CustomAssertions.IsNotNull(m_FirePoint);
	}


	//////////////////////////////////////////////////////////////////////////
	public override void OnAttach(IWeapon wpn, EWeaponSlots slot)
	{
		string moduleSectionName = GetType().FullName;
		m_WeaponRef = wpn;
		m_ModuleSlot = slot;

		// MODULE CONTAINER
		string containerID = WeaponBase.GetModuleSlotName(slot);
		m_FireModeContainer = transform.Find(containerID)?.gameObject;
		if (m_FireModeContainer.IsNotNull())
		{
			Destroy(m_FireModeContainer);
		}
		m_FireModeContainer = new GameObject(containerID);
		m_FireModeContainer.transform.SetParent(transform);
		m_FireModeContainer.transform.localPosition = Vector3.zero;
		m_FireModeContainer.transform.localRotation = Quaternion.identity;

		CustomAssertions.IsTrue(GlobalManager.Configs.TrySectionToOuter(m_ModuleSection, m_FireModuleData));

		TryChangeFireMode(m_FireModuleData.FireMode);

		// ASSIGN FIRE MODULE DATA
		m_Magazine = m_FireModuleData.MagazineCapacity;

		// CREATE FIRE MODE
		m_WpnFireMode.Setup(this, m_FireModuleData, Shoot);

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

			// Get bullet model
			Bullet.GetBulletModel(m_FireModuleData.BulletSection, out GameObject loadedResource);

			// Create pool
			uint size = m_FireModuleData.MagazineCapacity * m_FireModuleData.BulletsPerShot;
			var data = new GameObjectsPoolConstructorData<Bullet>(loadedResource, size)
			{
				ContainerName = $"{moduleSectionName}_BulletsPool_{wpn.Transform.name}",
				ActionOnObject = ActionOnBullet,
				IsAsyncBuild = false,
			};
			m_PoolBullets = new GameObjectsPool<Bullet>(data);
		}

		if (!m_UI_Crosshair)
		{
			if (m_ModuleSection.TryAsString("Crosshair", out string crosshairSection))
			{
				m_UI_Crosshair = UIManager.InGame.EnableCrosshair(System.Type.GetType(crosshairSection));
				m_UI_Crosshair.SetMin(m_FireModuleData.FireDispersion);
			}
			CustomAssertions.IsNotNull(m_UI_Crosshair, $"Crosshair is invalid");
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public override void OnDetach()
	{
		UIManager.InGame.RemoveCrosshair(m_UI_Crosshair);
	}


	//////////////////////////////////////////////////////////////////////////
	protected void ActionOnBullet(Bullet bullet)
	{
		bullet.Setup
		(
			whoRef: m_WeaponRef.Owner,
			weaponRef: m_WeaponRef as WeaponBase
		);
	//	bullet.gameObject.layer = LayerMask.NameToLayer("PlayerBullets");
	}


	//////////////////////////////////////////////////////////////////////////
	public bool TryChangeFireMode(string weaponFireModeSectionName)
	{
		return TryChangeFireMode(m_FireModeContainer, System.Type.GetType($"WPN_FireMode_{weaponFireModeSectionName}"), ref m_WpnFireMode);
	}


	//////////////////////////////////////////////////////////////////////////
	public bool TryChangeFireMode<T>() where T : WPN_FireModeBase, new()
	{
		return TryChangeFireMode(m_FireModeContainer, typeof(T), ref m_WpnFireMode);
	}


	//////////////////////////////////////////////////////////////////////////
	private static bool TryChangeFireMode(in GameObject container, in System.Type type, ref WPN_FireModeBase currentFireMode)
	{
		CustomAssertions.IsNotNull(type);

		if (currentFireMode.IsNotNull())
		{
			if (currentFireMode.GetType() != type)
			{
				Object.Destroy(currentFireMode);
			}
			else
			{
				// Same type, nothing change
				return false;
			}
		}

		string weaponFireModeSectionName = type.Name;

		// Check module type as child of WPN_BaseModule
		CustomAssertions.IsTrue(type.IsSubclassOf(typeof(WPN_FireModeBase)), $"Class Requested is not a supported weapon fire mode, \"{weaponFireModeSectionName}\"");
		
		// Check and get fire mode section
		CustomAssertions.IsTrue(GlobalManager.Configs.TryGetSection(weaponFireModeSectionName, out Database.Section section), $"Cannot find section for fire mode \"{weaponFireModeSectionName}\"");

		// Assign new fire mode
		currentFireMode = container.AddComponent(type) as WPN_FireModeBase;
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual float GetFireDispersion()
	{
		return m_FireModuleData.FireDispersion;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual float GetCamDeviation()
	{
		return m_FireModuleData.CamDeviation;
	}


	//////////////////////////////////////////////////////////////////////////
	protected abstract void Shoot(float moduleFireDispersion, float moduleCamDeviation);


	//////////////////////////////////////////////////////////////////////////
	protected void OnDestroy()
	{
		m_PoolBullets.Destroy();

		if (m_FireModeContainer)
		{
			Destroy(m_FireModeContainer);
		}
	}
}
