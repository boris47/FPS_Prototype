using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//////////////////////////////////////////////////////////////////////////
// WPN_FireModule_Shotgun
public class WPN_FireModule_Shotgun : WPN_FireModule {

	protected	uint	m_BuckshotSize	= 3;
	protected	float	m_BasePerShotFireDispersion = 0.2f;

	public override EFireMode FireMode
	{
		get {
			return EFireMode.NONE;
		}
	}

	protected	override		bool	InternalSetup( Database.Section moduleSection )
	{
		this.m_BuckshotSize				 = moduleSection.AsUInt( "BaseBucketSize", this.m_BuckshotSize );
		this.m_BasePerShotFireDispersion = moduleSection.AsFloat( "BasePerShotFireDispersion", this.m_BasePerShotFireDispersion );
		return true;
	}

	public		override		void	ApplyModifier( Database.Section modifier )
	{
		base.ApplyModifier( modifier );

		float MultPerShotFireDispersion		= modifier.AsFloat( "MultPerShotFireDispersion",	1.0f );
		float MultBucketSize				= modifier.AsFloat( "MultBucketSize",				1.0f );

		this.m_BasePerShotFireDispersion	= this.m_BasePerShotFireDispersion * MultPerShotFireDispersion;
		this.m_BuckshotSize					= (uint)( (float)this.m_BuckshotSize * MultBucketSize);
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
		return this.m_Magazine == 0 || this.m_Magazine < this.m_MagazineCapacity;
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

		this.m_Magazine --;
		for ( int i = 0; i < this.m_BuckshotSize; i++ )
		{
			this.InternalShoot( moduleFireDispersion, moduleCamDeviation );
		}

		// TODO muzzle flash
		//		EffectManager.Instance.PlayEffect( EffectType.MUZZLE, m_FirePoint.position, m_FirePoint.forward, 1 );
		//		EffectManager.Instance.PlayEffect( EffectType.SMOKE, m_FirePoint.position, m_FirePoint.forward, 1 );

		this.m_AudioSourceFire.Play();

		// CAM DEVIATION
		CameraControl.Instance.ApplyDeviation( moduleCamDeviation );

		// CAM DISPERSION
		CameraControl.Instance.ApplyDispersion( moduleFireDispersion );

		// CAM RECOIL
		CameraControl.Instance.AddRecoil(this.m_Recoil );

		// UI ELEMENTS
		UIManager.InGame.UpdateUI();
	}

	//
	protected	void	InternalShoot( float moduleFireDispersion, float moduleCamDeviation )
	{
		// BULLET
		IBullet bullet = this.m_PoolBullets.GetNextComponent();

		// POSITION
		Vector3 position = this.m_FirePoint.position;

		// DIRECTION
		Vector3 dispersionVector = new Vector3
		(
			Random.Range( -this.m_BasePerShotFireDispersion, this.m_BasePerShotFireDispersion ),
			Random.Range( -this.m_BasePerShotFireDispersion, this.m_BasePerShotFireDispersion ),
			Random.Range( -this.m_BasePerShotFireDispersion, this.m_BasePerShotFireDispersion )
		);

		Vector3 direction = (this.m_FirePoint.forward + dispersionVector ).normalized;

		// SHOOT
		bullet.Shoot( position: position, direction: direction );
	}

	public override bool CanChangeWeapon()
	{
		return true;
	}

	public override void OnWeaponChange()
	{
		
	}

	protected override void InternalUpdate( float DeltaTime )
	{
		this.m_WpnFireMode.InternalUpdate( DeltaTime, this.m_Magazine );
	}

	//    START
	public override        void    OnStart()
	{
		if (this.CanBeUsed() )
		{
			this.m_WpnFireMode.OnStart(this.GetFireDispersion(), this.GetCamDeviation() );
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
		}
	}

}
