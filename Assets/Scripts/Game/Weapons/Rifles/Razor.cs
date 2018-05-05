
using UnityEngine;
using UnityEngine.UI;



public class Razor : Weapon
{

	[Header("Razor Properties")]

	[SerializeField]
	private		float							m_BeamLength				= 50f;

	[SerializeField]
	private		Laser							m_Laser						= null;

	[SerializeField]
	protected	Renderer						m_Renderer					= null;


	private		Color							m_StartEmissiveColor		= Color.clear;

	private		Canvas							m_Canvas					= null;
	private		Image							m_Panel						= null;
	private		Text							m_AmmoText					= null;


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
		if ( m_Laser == null )
		{
			enabled = false;
			return;
		}

		base.Awake();

		m_StartEmissiveColor = m_Renderer.material.GetColor( "_EmissionColor" );

		m_Canvas	= GetComponentInChildren<Canvas>();
		m_Panel		= m_Canvas.transform.GetChild(0).GetComponent<Image>();
		m_AmmoText	= m_Panel.transform.GetChild(0).GetComponent<Text>();
		m_AmmoText.text = m_Magazine.ToString();

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
		return base.CanChangeWeapon();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnWeaponChange ( override )
	public		override	void			OnWeaponChange()
	{
		base.OnWeaponChange();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad ( Override )
	protected	override	StreamingUnit	OnLoad( StreamingData streamingData )
	{
		return base.OnLoad( streamingData );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave ( Override )
	protected	override	StreamingUnit	OnSave( StreamingData streamingData )
	{
		return base.OnSave( streamingData );
	}


	//////////////////////////////////////////////////////////////////////////
	// Update ( override )
	protected	override	void			Update()
	{
		// Zom fire
		if ( InputManager.Inputs.Fire2 && m_InTransition == false && m_IsRecharging == false )
		{
			OnSecondaryFire();
			return;
		}

		// Check
		if ( Player.Instance.IsRunning && WeaponManager.Instance.Zoomed && m_InTransition == false )
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
			m_IsFiring = true;
		}

		// Stop Firing
		if ( InputManager.Inputs.Fire1Released )
		{
			m_Laser.enabled = false;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FixedUpdate
	private void FixedUpdate()
	{
		// Hit target(s)
		if ( m_Laser.Target != null )
		{
			IEntity entity = m_Laser.Target.GetComponent<IEntity>();
			if ( entity != null )
			{
				entity.OnHit( transform.position, Player.Instance, m_MainDamage, false );
				EffectManager.Instance.PlayEntityOnHit( m_Laser.HitPoint, ( entity.Transform.position - transform.position ).normalized, 1 );
			}
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

}
