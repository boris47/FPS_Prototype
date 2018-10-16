
using UnityEngine;

public class Arrow : Weapon {

	private			uint							BURSTSIZE					= 2;


	[Header("Arrow Properties")]

	[SerializeField]
	private			Bullet							m_Bullet					= null;

	[SerializeField]
	private			float							m_ShotsDelay				= 0.1f;

	private			GameObjectsPool<Bullet>			m_PoolBullets				= null;

	private			uint							m_ShotsCount				= 0;

	private			float							m_ShotsTimer				= 0f;


	protected override string OtherInfo
	{
		get {
			return "";
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	void			Awake()
	{

		if ( m_Bullet == null )
		{
			print( "Weapon " + name + " need a defined bullet to use " );
			enabled = false;
		}

		base.Awake();

		// BULLETS POOL CREATION
		{
			if ( m_Bullet != null )
			{
				GameObject bulletGO = m_Bullet.gameObject;
				m_PoolBullets = new GameObjectsPool<Bullet>
				(
					model			: m_Bullet.gameObject,
					size			: m_MagazineCapacity,
					containerName	: "ArrowBulletsPool",
					permanent		: true,
					actionOnObject	: ( Bullet o ) =>
					{
						o.SetActive( false );
						o.Setup
						(
							canPenetrate: false,
							whoRef: Player.Instance,
							weaponRef: this,
							damageMin: -1.0f,
							damageMax: m_MainDamage
						);

						Player.Instance.DisableCollisionsWith( o.Collider );
					}
				);
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	public		override	bool			CanChangeWeapon()
	{
		return base.CanChangeWeapon();// if ( m_InTransition == true ) return false; if ( m_LockTimer > 0 ) return false;
	}


	//////////////////////////////////////////////////////////////////////////
	// FireSingleMode
	public		override	void			OnWeaponChange()
	{
		base.OnWeaponChange();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEndReload
	private					void			OnEndReload()
	{
		m_Magazine = m_MagazineCapacity;
		UI.Instance.InGame.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	StreamUnit	OnSave( StreamData streamData )
	{
		StreamUnit streamUnit		= base.OnSave( streamData );

		// MAGAZINE
		streamUnit.AddInternal( "Magazine", m_Magazine );

		// FLASHLIGHT
		if ( m_FlashLight != null )
			streamUnit.AddInternal( "FlashLightActive", m_FlashLight.Activated );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	StreamUnit	OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = base.OnLoad( streamData );
		if ( streamUnit == null )
			return null;

		// MAGAZINE
		m_Magazine = ( uint ) streamUnit.GetAsInt( "Magazine" );

		// FLASHLIGHT
		if ( m_FlashLight != null )
			m_FlashLight.SetActive( streamUnit.GetAsBool( "FlashLightActive") );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// Update ( override )
	protected	override	void			Update()
	{
		// Burst
		{
			// Continue burst
			if ( m_ShotsCount > 0 && m_ShotsCount < BURSTSIZE && m_ShotsTimer < 0f )
			{
				m_ShotsTimer = m_ShotsDelay;
				m_ShotsCount ++;
				FireShot( m_ShotsCount == BURSTSIZE );
				return;
			}

			// End of burst
			if ( m_ShotsCount == BURSTSIZE )
			{
				m_ShotsCount = 0;
				return;
			}
		}

		// Lock Timer
		if ( m_LockTimer > 0f )
		{
			m_LockTimer -= Time.deltaTime;

			if ( m_ShotsCount == 0 )
				return;
		}

		// burst shots delay
		m_ShotsTimer -= Time.deltaTime;

		// Shoot delay
		m_FireTimer -= Time.deltaTime;

		if ( m_WeaponState == WeaponState.STASHED )
			return;
		
		if ( Player.Instance.ChosingDodgeRotation == true )
			return;
		
		// Zoom
		if ( InputManager.Inputs.Fire2 && m_InTransition == false && m_IsRecharging == false )
		{
			ToggleZoom();
			return;
		}

		// Just after reload
		if ( m_LockTimer < 0f )
		{
//			anim.speed = m_AnimatorStdSpeed;
			m_LockTimer = 0f;
			if ( m_IsRecharging == true )
			{
				m_IsRecharging = false;
				OnEndReload();
			}
			m_NeedRecharge = false;
		}
		

		// Reload
		if ( m_Magazine == 0 || ( InputManager.Inputs.Reload && m_Magazine < m_MagazineCapacity ) || m_NeedRecharge )
		{
//			m_AnimatorStdSpeed = anim.speed;
//			anim.speed = 2f;

			if ( WeaponManager.Instance.IsZoomed )
			{
				if ( m_InTransition == false )
				{
					WeaponManager.Instance.ZoomOut();
					m_NeedRecharge = true;
				}
				return;
			}

			m_Animator.Play( m_ReloadAnim.name, -1, 0f );
			m_LockTimer = m_ReloadAnim.length; // / 2f;
			m_IsRecharging = true;
			return;
		}

		// Check
		if ( Player.Instance.IsRunning && WeaponManager.Instance.IsZoomed && m_InTransition == false )
		{
			WeaponManager.Instance.ZoomOut();
		}

		// Bullet delay
		if ( m_FireTimer > 0 )
			return;


		// Fire
		m_IsFiring = false;
		if ( InputManager.Inputs.Fire1 && m_Magazine > 0 && m_InTransition == false && m_NeedRecharge == false )
		{
			if ( m_ShotsCount == 0 )
			{
				m_ShotsTimer = m_ShotsDelay;
				m_ShotsCount ++;
			}
			FireShot( false );

		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSecondaryFire
	private					void			ToggleZoom()
	{
		if ( WeaponManager.Instance.IsZoomed == false )
			WeaponManager.Instance.ZoomIn();
		else
			WeaponManager.Instance.ZoomOut();
	}


	//////////////////////////////////////////////////////////////////////////
	// FireSingleMode
	private					void			FireShot( bool applyDeviation )
	{
		m_Magazine --;
		Shoot( applyDeviation );
		m_IsFiring = true;
	}
	

	//////////////////////////////////////////////////////////////////////////
	// ConfigureShot
	private					void			Shoot( bool applyDeviation )
	{
		m_FireTimer = m_ShotDelay;

		m_Animator.Play( m_FireAnim.name, -1, 0f );

		// BULLET
		IBullet bullet = m_PoolBullets.GetComponent();

		// CAM DISPERSION
		float finalDispersion = m_FireDispersion * bullet.RecoilMult;
		finalDispersion	*= Player.Instance.IsCrouched			? 0.50f : 1.00f;
		finalDispersion	*= Player.Instance.IsMoving				? 1.50f : 1.00f;
		finalDispersion	*= Player.Instance.IsRunning			? 2.00f : 1.00f;
		finalDispersion	*= WeaponManager.Instance.IsZoomed		? 0.80f : 1.00f;

		// SHOOT
		bullet.Shoot( position: m_FirePoint.position, direction: m_FirePoint.forward );
		m_AudioSourceFire.Play();
		if ( applyDeviation == true )
			CameraControl.Instance.ApplyDeviation( m_CamDeviation );
		CameraControl.Instance.ApplyDispersion( finalDispersion );
		UI.Instance.InGame.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDestroy
	private void OnDestroy()
	{
		m_PoolBullets.Destroy();
	}
}
