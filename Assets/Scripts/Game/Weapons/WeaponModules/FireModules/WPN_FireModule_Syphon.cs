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

	protected		Laser							m_Laser						= null;


	public override EFireMode FireMode
	{
		get {
			return EFireMode.NONE;
		}
	}

	protected	override	bool	InternalSetup( Database.Section moduleSection )
	{	
		string modulePrefabPath = null;
		if ( moduleSection.bAsString( "Module_Prefab", ref modulePrefabPath ) )
		{
			this.m_BaseAmmoRestoreCounter = moduleSection.AsFloat( "BaseAmmoRestoreCounter", this.m_BaseAmmoRestoreCounter );

			GameObject modulePrefab = Resources.Load( modulePrefabPath ) as GameObject;
			if ( modulePrefab )
			{	
				GameObject modulePrefabInstance = Instantiate<GameObject>( modulePrefab, this.m_FirePoint );
				modulePrefabInstance.transform.localPosition = Vector3.zero;
				modulePrefabInstance.transform.localRotation = Quaternion.identity;
			
				if ( modulePrefabInstance.transform.SearchComponent( ref this.m_Laser, ESearchContext.LOCAL ) )
				{
					this.m_Laser.LaserLength = this.m_BeamLength;
					this.m_Laser.enabled = false;
					this.m_Laser.OnAttached();
				}

				if (this.m_WeaponRef.Transform.SearchComponent( ref this.m_Renderer, ESearchContext.CHILDREN, s => s.name == "Graphics" ) )
				{
					this.m_StartEmissiveColor = this.m_Renderer.material.GetColor( "_EmissionColor" );
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
		this.m_Laser.enabled = false;
	}


	public	override	bool	OnSave			( StreamUnit streamUnit )
	{
		streamUnit.SetInternal(this.name, this.m_Magazine );
		return true;
	}

	//
	public	override	bool	OnLoad			( StreamUnit streamUnit )
	{
		this.m_Magazine = (uint)streamUnit.GetAsInt(this.name );
		return true;
	}


	//
	public	override	bool	NeedReload()
	{
		return this.m_Magazine < this.m_MagazineCapacity;
	}

	//
	public		override	void	OnAfterReload()
	{
		this.m_Magazine = this.m_MagazineCapacity;
	}

	// ON LOAD
	public		virtual		void	OnLoad( uint magazine )
	{
		this.m_Magazine = magazine;
	}

	// CAN SHOOT
	public	override		bool	CanBeUsed()
	{
		return this.m_Magazine > 0;
	}

	
	// SHOOT
	protected	override		void	Shoot( float moduleFireDispersion, float moduleCamDeviation )
	{
		//		m_FireDelay = m_BaseShotDelay;
		this.m_Magazine = (uint)Mathf.Max( --this.m_Magazine, 1 );
//		this.m_Magazine --;

		if (this.m_Magazine == 0 )
		{
			this.m_Laser.enabled = false;
			this.m_AudioSourceFire.Stop();
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
		CameraControl.Instance.ApplyDeviation( moduleCamDeviation );

		// CAM DISPERSION
		CameraControl.Instance.ApplyDispersion( moduleFireDispersion );

		// UI ELEMENTS
		UIManager.InGame.UpdateUI();
	}


	public override bool CanChangeWeapon()
	{
		return true;
	}


	public override void OnWeaponChange()
	{
		this.m_Laser.enabled = false;
	}

	// FIXED UPDATE
	protected void FixedUpdate()
	{
		// Hit target(s)
		if (this.m_Laser.enabled == true && this.m_Laser.HasHit == true )
		{
			IBullet bullet = this.m_PoolBullets.PeekComponent();
			IEntity entity = null;
			if ( Utils.Base.SearchComponent(this.m_Laser.RayCastHit.transform.gameObject, ref entity, ESearchContext.LOCAL ) )
			{

				// Do damage scaled with time scale
				entity.Events.OnHittedDetails(this.transform.position, Player.Instance, bullet.DamageType, bullet.Damage * Time.timeScale, false );

				EffectsManager.Instance.PlayEffect( EffectsManager.EEffecs.PLASMA, this.m_Laser.RayCastHit.point, this.m_Laser.RayCastHit.normal, 1 );
			}
			this.m_Laser.RayCastHit.rigidbody?.AddForceAtPosition( this.m_Laser.transform.forward * bullet.Velocity , this.m_Laser.RayCastHit.point, ForceMode.Impulse );
			EffectsManager.Instance.PlayEffect( EffectsManager.EEffecs.ENTITY_ON_HIT, this.m_Laser.RayCastHit.point, this.m_Laser.RayCastHit.normal, 1 );
		}
	}

	// INTERNAL UPDATE
	public override void InternalUpdate( float DeltaTime )
	{
		this.m_WpnFireMode.InternalUpdate( DeltaTime, this.m_Magazine );

		if (this.m_Laser.enabled == false && this.m_Magazine < this.m_MagazineCapacity )
		{
			this.m_BaseAmmoRestoreCounter -= Time.deltaTime;
			if (this.m_BaseAmmoRestoreCounter < 0f )
			{
				this.m_Magazine ++;
				this.m_BaseAmmoRestoreCounter = this.m_AmmoUnitRechargeDelay;
			}
		}

		float value = ( ( float )this.m_Magazine / ( float )this.m_MagazineCapacity );
		Color current = Color.Lerp( Color.black, this.m_StartEmissiveColor, value );
		this.m_Renderer.material.SetColor( "_EmissionColor", current );
	}

	//    START
	public override        void    OnStart()
	{
		if (this.CanBeUsed() )
		{
			this.m_WpnFireMode.OnStart(this.GetFireDispersion(), this.GetCamDeviation() );
			this.m_AudioSourceFire.Play();
			this.m_Laser.enabled = true;
		}
	}

	//    INTERNAL UPDATE
	public    override    void    OnUpdate()
	{
		if (this.CanBeUsed() )
		{
			this.m_WpnFireMode.OnUpdate(this.GetFireDispersion(), this.GetCamDeviation() );
		}
	}

	//    END
	public override        void    OnEnd()
	{
		if (this.CanBeUsed() )
		{
			this.m_WpnFireMode.OnEnd(this.GetFireDispersion(), this.GetCamDeviation() );
			this.m_AudioSourceFire.Stop();
			this.m_Laser.enabled = false;
		}
	}
	
}
