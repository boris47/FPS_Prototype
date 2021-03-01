using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WPN_FireModule_Syphon : WPN_FireModule
{
	[SerializeField]
	protected		float							m_BeamLength				= 50f;

	[SerializeField]
	protected		float							m_AmmoUnitRechargeDelay		= 0.1f;

	[SerializeField]
	protected		Renderer						m_Renderer					= null;

	protected		Color							m_StartEmissiveColor		= Color.clear;
	protected		float							m_BaseAmmoRestoreCounter	= 0f;

	protected		WPN_WeaponAttachment_LaserPointer m_Laser						= null;


	public override EFireMode FireMode => EFireMode.NONE;
	

	protected	override	bool	InternalSetup( Database.Section moduleSection )
	{	
		if ( moduleSection.TryAsString( "Module_Prefab", out string modulePrefabPath ) )
		{
			m_BaseAmmoRestoreCounter = moduleSection.AsFloat( "BaseAmmoRestoreCounter", m_BaseAmmoRestoreCounter );

			GameObject modulePrefab = Resources.Load( modulePrefabPath ) as GameObject;
			if ( modulePrefab )
			{	
				GameObject modulePrefabInstance = Instantiate<GameObject>( modulePrefab, m_FirePoint );
				modulePrefabInstance.transform.localPosition = Vector3.zero;
				modulePrefabInstance.transform.localRotation = Quaternion.identity;
			
				if ( modulePrefabInstance.transform.TrySearchComponent(ESearchContext.LOCAL, out m_Laser ) )
				{
					m_Laser.LaserLength = m_BeamLength;
					m_Laser.enabled = false;
				}

				if (m_WeaponRef.Transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_Renderer, s => s.name == "Graphics" ) )
				{
					m_StartEmissiveColor = m_Renderer.material.GetColor( "_EmissionColor" );
				}
			}
		}
		return true;
	}

	public override void ApplyModifier( Database.Section modifier )
	{
		base.ApplyModifier( modifier );

		// Do actions here
	}


	public	override	void	ResetBaseConfiguration()
	{
		base.ResetBaseConfiguration();

		// Do actions here
	}

	public	override	void	RemoveModifier( Database.Section modifier )
	{
		base.RemoveModifier( modifier );

		// Do Actions here
	}


	protected void Start()
	{
		m_Laser.enabled = false;
	}


	public	override	bool	OnSave			( StreamUnit streamUnit )
	{
		streamUnit.SetInternal(name, m_Magazine );
		return true;
	}

	//
	public	override	bool	OnLoad			( StreamUnit streamUnit )
	{
		m_Magazine = (uint)streamUnit.GetAsInt(name );
		return true;
	}


	//
	public	override	bool	NeedReload()
	{
		return m_Magazine < m_MagazineCapacity;
	}

	//
	public		override	void	OnAfterReload()
	{
		m_Magazine = m_MagazineCapacity;
	}

	// ON LOAD
	public		virtual		void	OnLoad( uint magazine )
	{
		m_Magazine = magazine;
	}

	// CAN SHOOT
	public	override		bool	CanBeUsed()
	{
		return m_Magazine > 0;
	}

	
	// SHOOT
	protected	override		void	Shoot( float moduleFireDispersion, float moduleCamDeviation )
	{
		//		m_FireDelay = m_BaseShotDelay;
		m_Magazine = (uint)Mathf.Max( --m_Magazine, 1 );
//		this.m_Magazine --;

		if (m_Magazine == 0 )
		{
			m_Laser.enabled = false;
			m_AudioSourceFire.Stop();
		}

		// TODO muzzle flash
//		EffectManager.Instance.PlayEffect( EffectType.MUZZLE, m_FirePoint.position, m_FirePoint.forward, 1 );
//		EffectManager.Instance.PlayEffect( EffectType.SMOKE, m_FirePoint.position, m_FirePoint.forward, 1 );

		// BULLET
//		IBullet bullet = m_PoolBullets.GetComponent();

		moduleFireDispersion	*= Player.Instance.IsCrouched			? 0.50f : 1.00f;
		moduleFireDispersion	*= Player.Instance.IsMoving				? 1.50f : 1.00f;
		moduleFireDispersion	*= Player.Instance.IsRunning			? 2.00f : 1.00f;
		moduleFireDispersion	*= WeaponManager.Instance.IsZoomed		? 0.80f : 1.00f;
//		moduleFireDispersion *= bullet.RecoilMult;

		// SHOOT
//		bullet.Shoot( position: m_FirePoint.position, direction: m_FirePoint.forward );

//		m_AudioSourceFire.Play();

		// CAM DEVIATION
		m_WeaponRef.ApplyDeviation( moduleCamDeviation );

		// CAM DISPERSION
		m_WeaponRef.ApplyDispersion( moduleFireDispersion );

		// UI ELEMENTS
		UIManager.InGame.UpdateUI();
	}


	public override bool CanChangeWeapon()
	{
		return true;
	}


	public override void OnWeaponChange()
	{
		m_Laser.enabled = false;
	}

	// FIXED UPDATE
	protected void FixedUpdate()
	{
		// Hit target(s)
		if (m_Laser.enabled == true && m_Laser.HasHit == true )
		{
			IBullet bullet = m_PoolBullets.PeekComponent();
			if ( Utils.Base.TrySearchComponent( m_Laser.RayCastHit.transform.gameObject, ESearchContext.LOCAL, out IEntity entity ) )
			{
				// Do damage scaled with time scale
				entity.Events.OnHittedDetails( transform.position, Player.Instance, bullet.DamageType, bullet.Damage * Time.timeScale, false );

				EffectsManager.Instance.PlayEffect( EffectsManager.EEffecs.PLASMA, m_Laser.RayCastHit.point, m_Laser.RayCastHit.normal, 1 );
			}
			m_Laser.RayCastHit.rigidbody?.AddForceAtPosition( m_Laser.transform.forward * bullet.Velocity , m_Laser.RayCastHit.point, ForceMode.Impulse );
			EffectsManager.Instance.PlayEffect( EffectsManager.EEffecs.ENTITY_ON_HIT, m_Laser.RayCastHit.point, m_Laser.RayCastHit.normal, 1 );
		}
	}

	// INTERNAL UPDATE
	protected override void InternalUpdate( float DeltaTime )
	{
		m_WpnFireMode.InternalUpdate( DeltaTime, m_Magazine );

		if (m_Laser.enabled == false && m_Magazine < m_MagazineCapacity )
		{
			m_BaseAmmoRestoreCounter -= Time.deltaTime;
			if (m_BaseAmmoRestoreCounter < 0f )
			{
				m_Magazine ++;
				m_BaseAmmoRestoreCounter = m_AmmoUnitRechargeDelay;
			}
		}

		float value = ( ( float )m_Magazine / ( float )m_MagazineCapacity );
		Color current = Color.Lerp( Color.black, m_StartEmissiveColor, value );
		m_Renderer.material.SetColor( "_EmissionColor", current );
	}

	//    START
	public override        void    OnStart()
	{
		if (CanBeUsed() )
		{
			m_WpnFireMode.OnStart(GetFireDispersion(), GetCamDeviation() );
			m_AudioSourceFire.Play();
			m_Laser.enabled = true;
		}
	}

	//    INTERNAL UPDATE
	public    override    void    OnUpdate()
	{
		if (CanBeUsed() )
		{
			m_WpnFireMode.OnUpdate(GetFireDispersion(), GetCamDeviation() );
		}
	}

	//    END
	public override        void    OnEnd()
	{
		if (CanBeUsed() )
		{
			m_WpnFireMode.OnEnd(GetFireDispersion(), GetCamDeviation() );
			m_AudioSourceFire.Stop();
			m_Laser.enabled = false;
		}
	}
	
}
