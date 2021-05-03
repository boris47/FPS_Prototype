using UnityEngine;

public class WPN_FireModule_Syphon : WPN_FireModule
{
	[SerializeField]
	protected			float							m_BeamLength				= 50f;

	[SerializeField]	
	protected			float							m_AmmoUnitRechargeDelay		= 0.1f;

	[SerializeField]	
	protected			Renderer						m_Renderer					= null;

	protected			Color							m_StartEmissiveColor		= Color.clear;
	protected			float							m_BaseAmmoRestoreCounter	= 0f;

	protected			WPN_ModuleAttachment_LaserPointer m_Laser						= null;


	public	override	EFireMode		FireMode => EFireMode.NONE;


	//////////////////////////////////////////////////////////////////////////
	protected override bool InternalSetup(Database.Section moduleSection)
	{
		CustomAssertions.IsTrue(moduleSection.TryAsFloat("BaseAmmoRestoreCounter", out m_BaseAmmoRestoreCounter));

		if (CustomAssertions.IsTrue(moduleSection.TryAsString("Module_Prefab", out string modulePrefabPath)))
		{
			GameObject modulePrefab = Resources.Load(modulePrefabPath) as GameObject;
			if (modulePrefab)
			{
				GameObject modulePrefabInstance = Instantiate<GameObject>(modulePrefab, m_FirePoint);
				modulePrefabInstance.transform.localPosition = Vector3.zero;
				modulePrefabInstance.transform.localRotation = Quaternion.identity;

				if (modulePrefabInstance.transform.TryGetComponent(out m_Laser))
				{
					m_Laser.LaserLength = m_BeamLength;
					m_Laser.enabled = false;
				}

				if (CustomAssertions.IsTrue(m_WeaponRef.Transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_Renderer, s => s.name == "Graphics")))
				{
					m_StartEmissiveColor = m_Renderer.material.GetColor("_EmissionColor");
				}
			}
		}
		return true;
	}

	//////////////////////////////////////////////////////////////////////////
	protected void Start()
	{
		m_Laser.enabled = false;
	}

	//////////////////////////////////////////////////////////////////////////
	public override bool OnSave(StreamUnit streamUnit)
	{
		streamUnit.SetInternal(name, m_Magazine);
		return true;
	}

	//////////////////////////////////////////////////////////////////////////
	public override bool OnLoad(StreamUnit streamUnit)
	{
		m_Magazine = (uint)streamUnit.GetAsInt(name);
		return true;
	}

	//////////////////////////////////////////////////////////////////////////
	public override bool NeedReload()
	{
		return m_Magazine < m_FireModuleData.MagazineCapacity;
	}

	//////////////////////////////////////////////////////////////////////////
	public override void OnAfterReload()
	{
		m_Magazine = m_FireModuleData.MagazineCapacity;
	}

	//////////////////////////////////////////////////////////////////////////
	public virtual void OnLoad(uint magazine)
	{
		m_Magazine = magazine;
	}

	//////////////////////////////////////////////////////////////////////////
	public override bool CanBeUsed()
	{
		return m_Magazine > 0;
	}

	//////////////////////////////////////////////////////////////////////////
	protected override void Shoot(float moduleFireDispersion, float moduleCamDeviation)
	{
		m_Magazine = (uint)Mathf.Max(--m_Magazine, 1);

		if (m_Magazine == 0)
		{
			m_Laser.enabled = false;
			m_AudioSourceFire.Stop();
		}

		// TODO muzzle flash
		//EffectManager.Instance.PlayEffect( EffectType.MUZZLE, m_FirePoint.position, m_FirePoint.forward, 1 );
		//EffectManager.Instance.PlayEffect( EffectType.SMOKE, m_FirePoint.position, m_FirePoint.forward, 1 );

		moduleFireDispersion *= m_WeaponRef.Owner.Motion.MotionStrategy.States.IsCrouched ? 0.50f : 1.00f;
		moduleFireDispersion *= m_WeaponRef.Owner.Motion.MotionStrategy.States.IsMoving ? 1.50f : 1.00f;
		moduleFireDispersion *= m_WeaponRef.Owner.Motion.MotionStrategy.States.IsRunning ? 2.00f : 1.00f;
		// TODO Convert for usage by other entities
		moduleFireDispersion *= WeaponManager.Instance.IsZoomed ? 0.80f : 1.00f;

		//m_AudioSourceFire.Play();

		// WEAPON DEVIATION
		WeaponPivot.Instance.ApplyDeviation(moduleCamDeviation);

		// CAM DISPERSION
		WeaponPivot.Instance.ApplyDispersion(moduleFireDispersion);

		// UI ELEMENTS
		UIManager.InGame.UpdateUI();
	}

	//////////////////////////////////////////////////////////////////////////
	public override bool CanChangeWeapon()
	{
		return true;
	}

	//////////////////////////////////////////////////////////////////////////
	public override void OnWeaponChange()
	{
		m_Laser.enabled = false;
	}

	//////////////////////////////////////////////////////////////////////////
	protected void FixedUpdate()
	{
		if (m_Laser.enabled && m_Laser.HasHit)
		{
			IBullet bullet = m_PoolBullets.PeekComponent();
			if (Utils.Base.TrySearchComponent(m_Laser.RayCastHit.transform.gameObject, ESearchContext.LOCAL, out Entity entity))
			{
				// Do damage scaled with time scale
				entity.OnHittedDetails(transform.position, m_WeaponRef.Owner, bullet.DamageType, bullet.Damage * Time.timeScale, false);

				EffectsManager.Instance.PlayEffect(EffectsManager.EEffecs.PLASMA, m_Laser.RayCastHit.point, m_Laser.RayCastHit.normal, 1);
			}
			m_Laser.RayCastHit.rigidbody?.AddForceAtPosition(m_Laser.transform.forward * bullet.Velocity, m_Laser.RayCastHit.point, ForceMode.Impulse);
			EffectsManager.Instance.PlayEffect(EffectsManager.EEffecs.ENTITY_ON_HIT, m_Laser.RayCastHit.point, m_Laser.RayCastHit.normal, 1);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	protected override void OnFrame(float DeltaTime)
	{
		m_WpnFireMode.InternalUpdate(DeltaTime, m_Magazine);

		if (!m_Laser.enabled && m_Magazine < m_FireModuleData.MagazineCapacity)
		{
			m_BaseAmmoRestoreCounter -= Time.deltaTime;
			if (m_BaseAmmoRestoreCounter < 0f)
			{
				m_Magazine++;
				m_BaseAmmoRestoreCounter = m_AmmoUnitRechargeDelay;
			}
		}

		float value = ((float)m_Magazine / (float)m_FireModuleData.MagazineCapacity);
		Color current = Color.Lerp(Color.black, m_StartEmissiveColor, value);
		m_Renderer.material.SetColor("_EmissionColor", current);
	}

	//////////////////////////////////////////////////////////////////////////
	public override void OnStart()
	{
		if (CanBeUsed())
		{
			m_WpnFireMode.OnStart(GetFireDispersion(), GetCamDeviation());
			m_AudioSourceFire.Play();
			m_Laser.enabled = true;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public override void OnUpdate()
	{
		if (CanBeUsed())
		{
			m_WpnFireMode.OnUpdate(GetFireDispersion(), GetCamDeviation());
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public override void OnEnd()
	{
		if (CanBeUsed())
		{
			m_WpnFireMode.OnEnd(GetFireDispersion(), GetCamDeviation());
			m_AudioSourceFire.Stop();
			m_Laser.enabled = false;
		}
	}
}
