
using UnityEngine;

public class Breaker : Weapon
{
	[Header("Breaker Properties")]

	[SerializeField]
	private			Bullet							m_Bullet					= null;

	private			GameObjectsPool<Bullet>			m_PoolBullets				= null;

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
					containerName	: "BreackerBulletsPool",
					permanent		: true,
					actionOnObject	: ( Bullet o ) =>
					{
						o.SetActive( false );
//						o.Setup( damageMin : m_MainDamage, damageMax : m_MainDamage, canPenetrate : false, whoRef : Player.Instance, weapon : null );
						o.Setup( whoRef: Player.Instance, weapon: null );
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
	protected	override	StreamingUnit	OnSave( StreamingData streamingData )
	{
		StreamingUnit streamingUnit		= base.OnSave( streamingData );

		streamingUnit.AddInternal( "Magazine", m_Magazine );

		if ( m_FlashLight != null )
			streamingUnit.AddInternal( "FlashLightActive", m_FlashLight.Activated );

		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	StreamingUnit	OnLoad( StreamingData streamingData )
	{
		StreamingUnit streamingUnit = base.OnLoad( streamingData );
		if ( streamingUnit == null )
			return null;

		// MAGAZINE
		m_Magazine = ( uint ) streamingUnit.GetAsInt( "Magazine" );

		// FLASHLIGHT
		if ( m_FlashLight != null )
			m_FlashLight.SetActive( streamingUnit.GetAsBool( "FlashLightActive") );

		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	void			Update()
	{
		// Lock Timer
		if ( m_LockTimer > 0f )
		{
			m_LockTimer -= Time.deltaTime;
			return;
		}

		// Shoot delay
		m_FireTimer -= Time.deltaTime;

		if ( m_WeaponState == WeaponState.STASHED )
			return;
		
		if ( Player.Instance.ChosingDodgeRotation == true )
			return;
		
		// Zoom
		if ( InputManager.Inputs.Fire2 && m_InTransition == false && m_IsRecharging == false )
		{
			OnSecondaryFire();
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
		
		// Bullet delay
		if ( m_FireTimer > 0 )
			return;

		// Fire
		m_IsFiring = false;
		if ( InputManager.Inputs.Fire1 && m_Magazine > 0 && m_InTransition == false && m_NeedRecharge == false )
		{
			FireShots();
			m_IsFiring = true;
		}

		// Check
		if ( Player.Instance.IsRunning && WeaponManager.Instance.IsZoomed && m_InTransition == false )
		{
			WeaponManager.Instance.ZoomOut();
		}

		if ( m_Magazine <= 0 || ( InputManager.Inputs.Reload && m_Magazine < m_MagazineCapacity ) || m_NeedRecharge )
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
		}	
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSecondaryFire
	private					void			OnSecondaryFire()
	{
		if ( WeaponManager.Instance.IsZoomed == false )
			WeaponManager.Instance.ZoomIn();
		else
			WeaponManager.Instance.ZoomOut();
	}


	//////////////////////////////////////////////////////////////////////////
	// FireSingleMode
	private					void			FireShots()
	{
		m_Magazine --;
		ConfigureShot();
	}


	//////////////////////////////////////////////////////////////////////////
	// ConfigureShot
	private					void			ConfigureShot()
	{
		m_FireTimer = m_ShotDelay;

		m_Animator.Play( m_FireAnim.name, -1, 0f );

		// BULLET
		IBullet bullet = m_PoolBullets.GetComponent();

		// CAM DISPERSION
		float finalDispersion = m_CamDeviation * bullet.RecoilMult;
		finalDispersion	*= Player.Instance.IsCrouched			? 0.50f : 1.00f;
		finalDispersion	*= Player.Instance.IsMoving				? 1.50f : 1.00f;
		finalDispersion	*= Player.Instance.IsRunning			? 2.00f : 1.00f;
		finalDispersion	*= WeaponManager.Instance.IsZoomed		? 0.80f : 1.00f;

		// SHOOT
		bullet.Shoot( position: m_FirePoint.position, direction: m_FirePoint.forward );
		m_AudioSourceFire.Play();
		CameraControl.Instance.ApplyDeviation( m_CamDeviation );
		UI.Instance.InGame.UpdateUI();
	}

	
	//////////////////////////////////////////////////////////////////////////
	// OnDestroy
	private void OnDestroy()
	{
		m_PoolBullets.Destroy();
	}
}
