
using UnityEngine;

public class Blackjack : Weapon {

	[Header("Blackjack Properties")]

	[SerializeField]
	private			Bullet							m_Bullet					= null;

	[SerializeField]
	private			FireModes						m_FireMode					= FireModes.AUTO;

	[SerializeField, Range( 1, 4 )]
	private			uint							m_BrustSize					= 3;

	protected		uint							m_BrustCount				= 0;
	private			FireFunction					m_FireFunction				= null;
	private			GameObjectsPool<Bullet>			m_PoolBullets				= null;

	protected override string OtherInfo
	{
		get {
			return m_FireMode.ToString();
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
					containerName	: "BlackjackBulletsPool",
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
		m_FireMode = FireModes.AUTO;
		SelectFireFunction();
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	public		override	bool			CanChangeWeapon()
	{
		return base.CanChangeWeapon(); // if ( m_InTransition == true ) return false; if ( m_LockTimer > 0 ) return false;
	}


	//////////////////////////////////////////////////////////////////////////
	// FireSingleMode
	public		override	void			OnWeaponChange()
	{
		base.OnWeaponChange();

		// RESET INTERNALS
		m_BrustCount = 0;
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

		// FIREMODE
		streamUnit.AddInternal( "Firemode", m_FireMode );

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

		// FIREMODE
		{
			m_FireMode = streamUnit.GetAsEnum<FireModes>( "Firemode" );
			SelectFireFunction();
		}

		// FLASHLIGHT
		if ( m_FlashLight != null )
			m_FlashLight.SetActive( streamUnit.GetAsBool( "FlashLightActive") );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	private					void			SelectFireFunction()
	{
		switch( m_FireMode )
		{
			case FireModes.AUTO:	m_FireFunction = FireAutoMode;	break;
			case FireModes.BURST:	m_FireFunction = FireBrustMode;	break;
			case FireModes.SINGLE:	m_FireFunction = FireSingleMode;	break;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Update ( override )
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
			m_BrustCount = 0;
			m_NeedRecharge = false;
		}

		
		// Fire mode cycle
		if ( InputManager.Inputs.Gadget1 )
		{
			if ( m_FireMode == FireModes.AUTO )
				m_FireMode = FireModes.SINGLE;
			else
				m_FireMode ++;

			UI.Instance.InGame.UpdateUI();
			SelectFireFunction();
		}
		

		// Bullet delay
		if ( m_FireTimer > 0 )
			return;

		m_IsFiring = false;
		if ( m_Magazine > 0 && m_InTransition == false && m_NeedRecharge == false )
		{
			m_FireFunction();
		}

		// End of brust
		if ( InputManager.Inputs.Fire1Released && m_BrustCount > 0 )
		{
			m_BrustCount = 0;
		}


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
	private					void			ToggleZoom()
	{
		if ( WeaponManager.Instance.IsZoomed == false )
			WeaponManager.Instance.ZoomIn();
		else
			WeaponManager.Instance.ZoomOut();
	}


	//////////////////////////////////////////////////////////////////////////
	// FireSingleMode
	private					void			FireSingleMode()
	{
		if ( InputManager.Inputs.Fire1 )
		{
			Shoot();
			m_IsFiring = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FireBrustMode
	private					void			FireBrustMode()
	{
		// Start of brust
		if ( InputManager.Inputs.Fire1Loop && m_BrustCount < m_BrustSize )
		{
			m_BrustCount ++;

			Shoot();
			m_IsFiring = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FireAutoMode
	private					void			FireAutoMode()
	{
		if ( ( InputManager.Inputs.Fire1Loop ) )
		{
			Shoot();
			m_IsFiring = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ConfigureShot
	private					void			Shoot()
	{
		m_FireTimer = m_ShotDelay;

		m_Animator.Play( m_FireAnim.name, -1, 0f );
			
		m_Magazine --;

		// TODO muzzle flash
//		EffectManager.Instance.PlayEffect( EffectType.MUZZLE, m_FirePoint.position, m_FirePoint.forward, 1 );
//		EffectManager.Instance.PlayEffect( EffectType.SMOKE, m_FirePoint.position, m_FirePoint.forward, 1 );

//		Time.timeScale = 0.0001f;

//		UnityEditor.EditorApplication.isPaused = true;

		// BULLET
		IBullet bullet = m_PoolBullets.GetComponent();

		// CAM DISPERSION
		float finalDispersion = m_FireDispersion * bullet.RecoilMult;
		finalDispersion	*= Player.Instance.IsCrouched			? 0.50f : 1.00f;
		finalDispersion	*= Player.Instance.IsMoving				? 1.50f : 1.00f;
		finalDispersion	*= Player.Instance.IsRunning			? 2.00f : 1.00f;
		finalDispersion *= ( m_FireMode == FireModes.SINGLE )	? 0.50f : 1.00f;
		finalDispersion *= ( m_FireMode == FireModes.BURST )	? 0.80f : 1.00f;
		finalDispersion *= ( m_FireMode == FireModes.AUTO )		? 1.10f : 1.00f;
		finalDispersion	*= WeaponManager.Instance.IsZoomed		? 0.80f : 1.00f;

		// SHOOT
		bullet.Shoot( position: m_FirePoint.position, direction: m_FirePoint.forward );
		m_AudioSourceFire.Play();
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
