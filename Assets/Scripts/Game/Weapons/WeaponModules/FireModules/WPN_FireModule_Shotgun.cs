using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//////////////////////////////////////////////////////////////////////////
// WPN_FireModule_Shotgun
public class WPN_FireModule_Shotgun : WPN_FireModule {

	protected	uint	m_BuckshotSize	= 3;
	protected	float	m_BasePerShotFireDispersion = 0.2f;

	public override FireModes FireMode
	{
		get {
			return FireModes.NONE;
		}
	}

	protected	override		bool	InternalSetup( Database.Section moduleSection )
	{
		m_BuckshotSize				= moduleSection.AsInt( "BaseBucketSize", m_BuckshotSize );
		m_BasePerShotFireDispersion = moduleSection.AsFloat( "BasePerShotFireDispersion", m_BasePerShotFireDispersion );
		return true;
	}

	public		override		void	ApplyModifier( Database.Section modifier )
	{
		base.ApplyModifier( modifier );

		float MultPerShotFireDispersion		= modifier.AsFloat( "MultPerShotFireDispersion",	1.0f );
		float MultBucketSize				= modifier.AsFloat( "MultBucketSize",				1.0f );

		m_BasePerShotFireDispersion			= m_BasePerShotFireDispersion * MultPerShotFireDispersion;
		m_BuckshotSize						= (uint)( (float)m_BuckshotSize * MultBucketSize);
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
		streamUnit.SetInternal( name, m_Magazine );
		return true;
	}

	//
	public	override	bool	OnLoad			( StreamUnit streamUnit )
	{
		m_Magazine = (uint)streamUnit.GetAsInt( name );
		return true;
	}


	//
	public	override	bool	NeedReload()
	{
		return m_Magazine == 0 || m_Magazine < m_MagazineCapacity;
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

		m_Magazine --;
		for ( int i = 0; i < m_BuckshotSize; i++ )
		{
			InternalShoot( moduleFireDispersion, moduleCamDeviation );
		}

		// TODO muzzle flash
//		EffectManager.Instance.PlayEffect( EffectType.MUZZLE, m_FirePoint.position, m_FirePoint.forward, 1 );
//		EffectManager.Instance.PlayEffect( EffectType.SMOKE, m_FirePoint.position, m_FirePoint.forward, 1 );
		
		m_AudioSourceFire.Play();

		// CAM DEVIATION
		CameraControl.Instance.ApplyDeviation( moduleCamDeviation );

		// CAM DISPERSION
		CameraControl.Instance.ApplyDispersion( moduleFireDispersion );

		// CAM RECOIL
		CameraControl.Instance.AddRecoil( m_Recoil );

		// UI ELEMENTS
		UI.Instance.InGame.UpdateUI();
	}

	//
	protected	void	InternalShoot( float moduleFireDispersion, float moduleCamDeviation )
	{
		// BULLET
		IBullet bullet = m_PoolBullets.GetNextComponent();

		// POSITION
		Vector3 position = m_FirePoint.position;

		// DIRECTION
		Vector3 dispersionVector = new Vector3
		(
			Random.Range( -m_BasePerShotFireDispersion, m_BasePerShotFireDispersion ),
			Random.Range( -m_BasePerShotFireDispersion, m_BasePerShotFireDispersion ),
			Random.Range( -m_BasePerShotFireDispersion, m_BasePerShotFireDispersion )
		);

		Vector3 direction = ( m_FirePoint.forward + dispersionVector ).normalized;

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

	public override void InternalUpdate( float DeltaTime )
	{
		m_WpnFireMode.InternalUpdate( DeltaTime, m_Magazine );
	}

	//    START
	public override        void    OnStart()
	{
		if ( CanBeUsed() )
		{
			m_WpnFireMode.OnStart( GetFireDispersion(), GetCamDeviation() );
		}
	}

	//    INTERNAL UPDATE
	public    override    void    OnUpdate()
	{
		if ( CanBeUsed() )
		{
			m_WpnFireMode.OnUpdate( GetFireDispersion(), GetCamDeviation() );
		}
	}

	//    END
	public override        void    OnEnd()
	{
		if ( CanBeUsed() )
		{
			m_WpnFireMode.OnEnd( GetFireDispersion(), GetCamDeviation() );
		}
	}

}
