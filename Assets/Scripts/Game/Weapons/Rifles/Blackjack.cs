
using UnityEngine;

public class Blackjack : Weapon {

	public enum FireModes {
		SINGLE, BURST, AUTO
	}

	[Header("Blackjack Properties")]

	[SerializeField]
	protected		Bullet							m_Bullet					= null;

	[SerializeField]
	private			FireModes						m_FireMode					= FireModes.AUTO;

	[SerializeField, Range( 1, 4 )]
	protected		uint							m_BrustSize					= 3;

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
					containerName	: name + "BulletsPool",
					permanent		: true,
					actionOnObject	: ( Bullet o ) =>
					{
						o.SetActive( false );
						o.Setup( damage : m_MainDamage, canPenetrate : false, whoRef : Player.Instance, weapon : this );
						Physics.IgnoreCollision( o.Collider, Player.Entity.PhysicCollider, ignore : true );
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
	protected	override	StreamingUnit	OnSave( StreamingData streamingData )
	{

		StreamingUnit streamingUnit		= base.OnSave( streamingData );

		streamingUnit.AddInternal( "Magazine = " + m_Magazine );
		streamingUnit.AddInternal( "Firemode = " + m_FireMode.ToString() );

		if ( m_FlashLight != null )
			streamingUnit.AddInternal( "FlashLightActive = " + m_FlashLight.Activated );

		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	StreamingUnit	OnLoad( StreamingData streamingData )
	{
		StreamingUnit streamingUnit = base.OnLoad( streamingData );

		KeyValue[] internals = Utils.Base.GetKeyValues( streamingUnit.Internals );

		// MAGAZINE
		{
			uint magazine = m_MagazineCapacity;
			if ( uint.TryParse( internals[0].Value, out magazine ) )
			{
				m_Magazine = magazine;
			}
		}

		// FIREMODE
		{
			m_FireMode = ( FireModes ) System.Enum.Parse( typeof( FireModes ), internals[1].Value );
			SelectFireFunction();
		}

		// FLASHLIGHT
		if ( m_FlashLight != null )
		{
			bool state = internals[2].Value.ToLower() == "true" ? true : false;
			m_FlashLight.SetActive( state );
		}

		return streamingUnit;
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
		m_FireTimer -= Time.deltaTime;
		
		if ( Player.Instance.ChosingDodgeRotation == true )
			return;
		
		// Zoom
		if ( InputManager.Inputs.Fire2 && m_InTransition == false && m_IsRecharging == false )
		{
			ToggleZoom();
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

		
		// Fire mode cycle
		if ( InputManager.Inputs.ItemAction2 )
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
	private					void			ToggleZoom()
	{
		if ( WeaponManager.Instance.Zoomed == false )
			WeaponManager.Instance.ZoomIn( this, m_ZoomOffset, m_ZoomingTime );
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

		// BULLET
		IBullet bullet = m_PoolBullets.GetComponent();

		// POSITION
		Vector3 position = m_FirePoint.position;

		// DIRECTION
		m_DispersionVector.Set( Random.Range( -1f, 1f ), Random.Range( -1f, 1f ), Random.Range( -1f, 1f ) );
		m_DispersionVector /= WeaponManager.Instance.Zoomed ? m_ZoomFactor : 1f;

		Vector3 direction = m_FirePoint.forward;

		// CAM DISPERSION
		float finalDispersion = m_FireDispersion * bullet.RecoilMult;
		finalDispersion	*= Player.Instance.IsCrouched			? 0.50f : 1.00f;
		finalDispersion	*= Player.Instance.IsMoving				? 1.50f : 1.00f;
		finalDispersion	*= Player.Instance.IsRunning			? 2.00f : 1.00f;
		finalDispersion *= ( m_FireMode == FireModes.SINGLE )	? 0.50f : 1.00f;
		finalDispersion *= ( m_FireMode == FireModes.BURST )	? 0.80f : 1.00f;
		finalDispersion *= ( m_FireMode == FireModes.AUTO )		? 1.10f : 1.00f;
		finalDispersion	*= WeaponManager.Instance.Zoomed		? 0.80f : 1.00f;

		// SHOOT
		bullet.Shoot( position: position, direction: direction );
		m_AudioSourceFire.Play();
		CameraControl.Instance.ApplyDeviation( m_CamDeviation );
		CameraControl.Instance.ApplyDispersion( finalDispersion );
		UI.Instance.InGame.UpdateUI();
	}

}
