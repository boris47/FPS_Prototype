
using UnityEngine;
using UnityEngine.UI;

public class Blaster : Weapon
{

	[Header("Blaster Properties")]
	
	[SerializeField]
	private		Bullet							m_Bullet					= null;

	[SerializeField, Range( 0.01f, 10f )]
	private		float							m_ChargeSpeed				= 1f;

	[SerializeField]
	private		Renderer						m_Renderer					= null;

	private		GameObjectsPool<Bullet>			m_PoolBullets				= null;

	private		Color							m_StartEmissiveColor		= Color.clear;

	private		Canvas							m_Canvas					= null;
	private		Image							m_Panel						= null;
	private		Slider							m_ChargeSlider				= null;

	private		float							m_Charge					= 0f;
	private		bool							m_HasCharged				= false;

	private		float							m_BulletMaxDamage			= 0f;
	private		float							m_BulletMinDamage			= 0f;
	private	 float						   m_BulletEffectIntensity		= 0f;

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

		m_StartEmissiveColor = Color.clear;

		m_Canvas			= GetComponentInChildren<Canvas>();
		m_Panel				= m_Canvas.transform.GetChild(0).GetComponent<Image>();
		m_ChargeSlider		= m_Panel.transform.GetChild(0).GetComponent<Slider>();
		m_ChargeSlider.value = 0f;

		// BULLETS POOL CREATION
		{
			if ( m_Bullet != null )
			{
				GameObject bulletGO = m_Bullet.gameObject;
				m_PoolBullets = new GameObjectsPool<Bullet>
				(
					model			: m_Bullet.gameObject,
					size			: m_MagazineCapacity,
					containerName	: "BlasterBulletsPool",
					permanent		: true,
					actionOnObject	: ( Bullet o ) =>
					{
						o.SetActive( false );
						o.Setup( damage : m_MainDamage, canPenetrate : false, whoRef : Player.Instance, weapon : this );
						Player.Instance.DisableCollisionsWith( o.Collider );
					}
				);
				IBullet bullet = m_PoolBullets.GetComponent();
				m_BulletMaxDamage = bullet.DamageMax;
				m_BulletMinDamage = bullet.DamageMin;
				Light bulletLight =	bullet.Effect as Light;
				m_BulletEffectIntensity = bulletLight.intensity;
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

		// RESET INTERNALS
		m_Charge					= 0f;
		m_HasCharged				= false;
		m_ChargeSlider.value		= 0f;
		m_Renderer.material.SetColor( "_EmissionColor", m_StartEmissiveColor );
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

		// FLASHLIGHT
		if ( m_FlashLight != null )
			m_FlashLight.SetActive( streamingUnit.GetAsBool( "FlashLightActive") );

		// MAGAZINE
		m_Magazine = ( uint ) streamingUnit.GetAsInt( "Magazine" );

		// RESET INTERNALS
		m_Charge					= 0f;
		m_HasCharged				= false;
		m_ChargeSlider.value		= 0f;
		m_Renderer.material.SetColor( "_EmissionColor", m_StartEmissiveColor );

		return streamingUnit;
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
			m_NeedRecharge = false;
		}
		

		// Bullet delay
		if ( m_FireTimer > 0 )
			return;

		// Charging
		m_IsFiring = false;
		if ( InputManager.Inputs.Fire1Loop == true )
		{
			if ( m_HasCharged == false )
			{
				m_Charge += Time.deltaTime * m_ChargeSpeed;
				if ( m_Charge > 1f )
				{
					m_HasCharged = true;
					m_Charge = 1f;
				}
				Color current = Color.Lerp( m_StartEmissiveColor, Color.blue, m_ChargeSlider.value );
				m_Renderer.material.SetColor( "_EmissionColor", current );
				m_ChargeSlider.value = m_Charge;
			}
			float finalDispersion = m_FireDispersion * Time.deltaTime;
			finalDispersion	*= Player.Instance.IsCrouched			? 0.50f : 1.00f;
			finalDispersion	*= Player.Instance.IsMoving				? 1.50f : 1.00f;
			finalDispersion	*= Player.Instance.IsRunning			? 2.00f : 1.00f;
			finalDispersion	*= WeaponManager.Instance.IsZoomed		? 0.80f : 1.00f;
			CameraControl.Instance.ApplyDispersion( finalDispersion );
			// deviation scaled with time delta
			float deviation = Random.Range( -m_CamDeviation, m_CamDeviation ) * ( 2f - m_Charge );
			CameraControl.Instance.ApplyDeviation( deviation * Time.deltaTime, 1f, 0.5f );
			m_IsFiring = true;
		}

		// Fire
		if ( InputManager.Inputs.Fire1Released == true )
		{
			if ( m_Magazine > 0 && m_InTransition == false && m_NeedRecharge == false )
			{
				FireBlast();
			}
			m_Renderer.material.SetColor( "_EmissionColor", m_StartEmissiveColor );
			m_ChargeSlider.value = 0f;
			m_Charge		= 0f;
			m_HasCharged	= false;
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
	private					void			ToggleZoom()
	{
		if ( WeaponManager.Instance.IsZoomed == false )
			WeaponManager.Instance.ZoomIn();
		else
			WeaponManager.Instance.ZoomOut();
	}


	//////////////////////////////////////////////////////////////////////////
	// FireSingleMode
	private					void			FireBlast()
	{
		m_Magazine --;
		Shoot( m_Charge );
		m_IsFiring = true;
	}
	

	//////////////////////////////////////////////////////////////////////////
	// ConfigureShot
	private					void			Shoot( float power )
	{
		float clampedPower = Mathf.Max( 0.2f, power );

		m_FireTimer = m_ShotDelay * clampedPower;

		m_Animator.Play( m_FireAnim.name, -1, 0f );

		// BULLET
		IBullet bullet = m_PoolBullets.GetComponent();

		// BULLET DAMAGE
		bullet.DamageMax = bullet.DamageMin = m_MainDamage * clampedPower * 10f;

		// BULLET EEFFECT
		Light bulletLight =	bullet.Effect as Light;
		bulletLight.intensity = m_BulletEffectIntensity * clampedPower;

		// SHOOT
		bullet.Shoot( position: m_FirePoint.position, direction: m_FirePoint.forward, velocity: bullet.Velocity * clampedPower );
		m_AudioSourceFire.Play();
		CameraControl.Instance.ApplyDeviation( m_CamDeviation * 0.2f * clampedPower );
		UI.Instance.InGame.UpdateUI();
	}

	
	//////////////////////////////////////////////////////////////////////////
	// OnDestroy
	private void OnDestroy()
	{
		m_PoolBullets.Destroy();
	}
}
