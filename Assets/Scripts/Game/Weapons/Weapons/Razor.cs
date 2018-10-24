
using UnityEngine;
using UnityEngine.UI;

public class Razor : Weapon
{
	[Header("Razor Properties")]

	[SerializeField]
	private		float							m_BeamLength				= 50f;

	[SerializeField]
	private		float							m_AmmoUnitRechargeDelay		= 0.1f;

	[SerializeField]
	private		Renderer						m_Renderer					= null;

	private		Color							m_StartEmissiveColor		= Color.clear;

	private		float							m_AmmoRestoreCounter		= 0f;

	private		Canvas							m_Canvas					= null;
	private		Image							m_Panel						= null;
	private		Slider							m_MagazineSlider			= null;

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
		base.Awake();

		if ( m_Laser == null )
		{
			enabled = false;
			return;
		}

		m_StartEmissiveColor = m_Renderer.material.GetColor( "_EmissionColor" );

		m_Canvas			= GetComponentInChildren<Canvas>();
		m_Panel				= m_Canvas.transform.GetChild(0).GetComponent<Image>();
		m_MagazineSlider	= m_Panel.transform.GetChild(0).GetComponent<Slider>();
		m_MagazineSlider.value = 1f;

		m_Laser.LaserLength = m_BeamLength;
	}


	private void Start()
	{
		m_Laser.enabled = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// CanChangeWeapon ( Override )
	public		override	bool			CanChangeWeapon()
	{
		return base.CanChangeWeapon();// if ( m_InTransition == true ) return false; if ( m_LockTimer > 0 ) return false;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnWeaponChange ( override )
	public		override	void			OnWeaponChange()
	{
		base.OnWeaponChange();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave ( Override )
	protected	override	StreamUnit	OnSave( StreamData streamData )
	{
		StreamUnit streamUnit		= base.OnSave( streamData );

		streamUnit.AddInternal( "Magazine", m_Magazine );

		if ( m_FlashLight != null )
			streamUnit.AddInternal( "FlashLightActive", m_FlashLight.Activated );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad ( Override )
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
	protected override void Update()
	{
		// Zom fire
		if ( InputManager.Inputs.Fire2 && m_InTransition == false && m_IsRecharging == false )
		{
			OnSecondaryFire();
			return;
		}

		// Check
		if ( Player.Instance.IsRunning && WeaponManager.Instance.IsZoomed && m_InTransition == false )
		{
			WeaponManager.Instance.ZoomOut();
		}

		// Locked
		if ( m_LockTimer > 0f )
		{
			m_LockTimer -= Time.deltaTime;
			return;
		}

		// Fire
		m_IsFiring = false;
		if ( InputManager.Inputs.Fire1 && m_Magazine > 0 && m_InTransition == false && m_NeedRecharge == false )
		{
			m_Laser.enabled = true;
			m_AudioSourceFire.Play();
		}

		//Is Firing
		m_IsFiring = m_Laser.enabled;

		// Stop Firing
		if ( InputManager.Inputs.Fire1Released || m_Magazine <= 0f )
		{
			m_Laser.enabled = false;
			m_AudioSourceFire.Stop();
		}

		if ( m_Magazine < m_MagazineCapacity && m_IsFiring == false )
		{
			m_AmmoRestoreCounter -= Time.deltaTime;
			if ( m_AmmoRestoreCounter < 0f )
			{
				m_Magazine ++;
				m_AmmoRestoreCounter = m_AmmoUnitRechargeDelay;
				m_MagazineSlider.value = ( float )( ( float )m_Magazine / ( float )m_MagazineCapacity );
				Color current = Color.Lerp( Color.blue, m_StartEmissiveColor, m_MagazineSlider.value );
				m_Renderer.material.SetColor( "_EmissionColor", current );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FixedUpdate
	private void FixedUpdate()
	{
		// Hit target(s)
		if ( m_Laser.enabled == true)
		{
			if ( m_Laser.HasHit == true )
			{
				IEntity entity = m_Laser.RayCastHit.transform.GetComponent<IEntity>();
				if ( entity != null )
				{
					// Do damage scaled with time scale
					entity.OnHit( transform.position, Player.Instance, m_MainDamage * Time.timeScale, false );

					EffectManager.Instance.PlayEffect( EffectType.PLASMA, m_Laser.RayCastHit.point, m_Laser.RayCastHit.normal, 1 );
				}
				EffectManager.Instance.PlayEffect( EffectType.ENTITY_ON_HIT, m_Laser.RayCastHit.point, m_Laser.RayCastHit.normal, 1 );
			}
			m_Magazine --;


			// disperion scaled with time scale
			float dispersion = Random.Range( -m_FireDispersion, m_FireDispersion ) * ( 2f - m_MagazineSlider.value );
			CameraControl.Instance.ApplyDispersion( dispersion, 1f, 1f );

			// deviation scaled with time scale
			float deviation = Random.Range( -m_CamDeviation, m_CamDeviation ) * ( 2f - m_MagazineSlider.value );
			CameraControl.Instance.ApplyDeviation( deviation, 1f, 0.5f );

			m_MagazineSlider.value = ( float )( ( float )m_Magazine / ( float )m_MagazineCapacity );
		}

		UI.Instance.InGame.UpdateUI();
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

}
