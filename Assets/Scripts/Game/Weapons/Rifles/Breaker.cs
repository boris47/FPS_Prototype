
using UnityEngine;

public class Breaker : Weapon
{
	[Header("Breaker Properties")]
	[SerializeField]
	protected		Bullet							m_Bullet					= null;


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
					containerName	: "RifleBulletsPool1",
					permanent		: true,
					actionOnObject	: ( Bullet o ) =>
					{
						o.SetActive( false );
//						o.Setup( damageMin : m_MainDamage, damageMax : m_MainDamage, canPenetrate : false, whoRef : Player.Instance, weapon : null );
						o.Setup( whoRef: Player.Instance, weapon: null );
						Physics.IgnoreCollision( o.Collider, Player.Entity.PhysicCollider, ignore: true );
						Physics.IgnoreCollision( o.Collider, Player.Instance.PlayerNearAreaTrigger );
						Physics.IgnoreCollision( o.Collider, Player.Instance.PlayerFarAreaTrigger );

						/*
						Physics.IgnoreCollision( o.Collider, Player.Entity.PhysicCollider, ignore : true );
						Physics.IgnoreCollision( o.Collider, Player.Entity.Brain.FieldOfView., ignore : true );
						Physics.IgnoreCollision( o.Collider, Player.Entity.PhysicCollider, ignore : true );
						*/
					}
				);
				/*
				for ( int i = 0; i < m_MagazineCapacity; i++ )
				{
					var bullet1 = m_PoolBullets.GetComponent();
					for ( int j = 0; j < m_MagazineCapacity; j++ )
					{
						var bullet2 = m_PoolBullets.GetComponent();
						Physics.IgnoreCollision( bullet1.Collider, bullet2.Collider, ignore : true );
					}
				}
				*/
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	public		override	bool			CanChangeWeapon()
	{
		return base.CanChangeWeapon();
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
	protected	override	StreamingUnit	OnLoad( StreamingData streamingData )
	{
		return base.OnLoad( streamingData );
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	StreamingUnit	OnSave( StreamingData streamingData )
	{
		return base.OnSave( streamingData );
	}

	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	void			Update()
	{
		m_FireTimer -= Time.deltaTime;
		
		if ( Player.Instance.ChosingDodgeRotation == true )
			return;
		
		// Zoom
		if ( InputManager.Inputs.Fire2 && m_InTransition == false && m_IsRecharging == false )
		{
			OnSecondaryFire();
			return;
		}

		// Reloading
		if ( m_LockTimer > 0f )
		{
			m_LockTimer -= Time.deltaTime;
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
			m_BrustCount = 0;
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
		if ( Player.Instance.IsRunning && WeaponManager.Instance.Zoomed && m_InTransition == false )
		{
			WeaponManager.Instance.ZoomOut();
		}

		if ( m_Magazine <= 0 || ( InputManager.Inputs.Reload && m_Magazine < m_MagazineCapacity ) || m_NeedRecharge )
		{
//			m_AnimatorStdSpeed = anim.speed;
//			anim.speed = 2f;

			if ( WeaponManager.Instance.Zoomed )
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
		if ( WeaponManager.Instance.Zoomed == false )
			WeaponManager.Instance.ZoomIn( this, m_ZoomOffset, m_ZoomingTime );
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

		// POSITION
		Vector3 position = m_FirePoint.position;

		// DIRECTION
		Vector3 direction = m_FirePoint.forward;

		// AUDIOSOURCE
		ICustomAudioSource audioSource = m_AudioSourceFire;

		// CAM DISPERSION
		float finalDispersion = m_CamDeviation * bullet.RecoilMult;
		finalDispersion	*= Player.Instance.IsCrouched			? 0.50f : 1.00f;
		finalDispersion	*= Player.Instance.IsMoving				? 1.50f : 1.00f;
		finalDispersion	*= Player.Instance.IsRunning			? 2.00f : 1.00f;
		finalDispersion	*= WeaponManager.Instance.Zoomed		? 0.80f : 1.00f;

		// SHOOT
		bullet.Shoot( position: position, direction: direction );
		m_AudioSourceFire.Play();
		CameraControl.Instance.ApplyDeviation( m_CamDeviation );
		UI.Instance.InGame.UpdateUI();
	}

}
