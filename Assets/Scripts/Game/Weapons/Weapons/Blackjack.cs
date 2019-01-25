
using System;
using UnityEngine;

public class Blackjack : Weapon {

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
		m_PrimaryWeaponModule.OnWeaponChange();
		m_SecondaryWeaponModule.OnWeaponChange();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEndReload
	private					void			OnEndReload()
	{
		m_PrimaryWeaponModule.OnAfterReload();
		m_SecondaryWeaponModule.OnAfterReload();

		UI.Instance.InGame.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	StreamUnit		OnSave( StreamData streamData )
	{

		StreamUnit streamUnit		= base.OnSave( streamData );

		// MAGAZINE
///		streamUnit.AddInternal( "Magazine", m_Magazine );

		// FIREMODE
//		streamUnit.AddInternal( "Firemode", m_FireMode );

		// FLASHLIGHT
		if ( m_FlashLight != null )
			streamUnit.AddInternal( "FlashLightActive", m_FlashLight.Activated );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	StreamUnit		OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = base.OnLoad( streamData );
		if ( streamUnit != null )
		{
			// FLASHLIGHT
			if ( m_FlashLight != null )
			{
				m_FlashLight.SetActive( streamUnit.GetAsBool( "FlashLightActive") );
			}
		}

		return streamUnit;
	}

	//////////////////////////////////////////////////////////////////////////
	private	bool	BaseCondition()
	{
		return m_WeaponState == WeaponState.DRAWED && Player.Instance.ChosingDodgeRotation == false && m_IsLocked == false;
	}

	//////////////////////////////////////////////////////////////////////////
	// Fire_Start ( Override )
	protected override bool Predicate_PrimaryFire_Start() { return BaseCondition() && m_PrimaryWeaponModule.CanBeUsed(); }
	protected	override	void			PrimaryFire_Start()
	{
		m_PrimaryWeaponModule.OnStart();
	}


	//////////////////////////////////////////////////////////////////////////
	// Fire_Update ( Override )
	protected override bool Predicate_PrimaryFire_Update() { return BaseCondition() && m_PrimaryWeaponModule.CanBeUsed(); }
	protected	override		void		PrimaryFire_Update()
	{
		m_PrimaryWeaponModule.OnUpdate();
	}

	//////////////////////////////////////////////////////////////////////////
	// Fire_End ( Override )
	protected override bool Predicate_PrimaryFire_End() { return BaseCondition() && m_PrimaryWeaponModule.CanBeUsed(); }
	protected	override	void			PrimaryFire_End()
	{
		m_PrimaryWeaponModule.OnEnd();
	}


	//////////////////////////////////////////////////////////////////////////
	// SecondaryFire_Start ( Override )
	protected override bool Predicate_SecondaryFire_Start() { return BaseCondition() && m_SecondaryWeaponModule.CanBeUsed(); }
	protected	override	void			SecondaryFire_Start()
	{
		m_SecondaryWeaponModule.OnStart();
	}


	//////////////////////////////////////////////////////////////////////////
	// SecondaryFire_Update ( Override )
	protected override bool Predicate_SecondaryFire_Update() { return BaseCondition() && m_SecondaryWeaponModule.CanBeUsed(); }
	protected	override		void		SecondaryFire_Update()
	{
		m_SecondaryWeaponModule.OnUpdate();
	}


	//////////////////////////////////////////////////////////////////////////
	// TertiaryFire_End ( Override )
	protected override bool Predicate_SecondaryFire_End() { return BaseCondition() && m_SecondaryWeaponModule.CanBeUsed(); }
	protected	override	void			SecondaryFire_End()
	{
		m_SecondaryWeaponModule.OnEnd();
	}








	//////////////////////////////////////////////////////////////////////////
	// TertiaryFire_Start ( Override )
	protected override bool Predicate_TertiaryFire_Start() { return BaseCondition() && m_TertiaryWeaponModule.CanBeUsed(); }
	protected	override	void			TertiaryFire_Start()
	{
		m_TertiaryWeaponModule.OnStart();
	}


	//////////////////////////////////////////////////////////////////////////
	// TertiaryFire_Update ( Override )
	protected override bool Predicate_TertiaryFire_Update() { return BaseCondition() && m_TertiaryWeaponModule.CanBeUsed(); }
	protected	override		void		TertiaryFire_Update()
	{
		m_TertiaryWeaponModule.OnUpdate();
	}


	//////////////////////////////////////////////////////////////////////////
	// TertiaryFire_End ( Override )
	protected override bool Predicate_TertiaryFire_End() { return BaseCondition() && m_TertiaryWeaponModule.CanBeUsed(); }
	protected	override	void			TertiaryFire_End()
	{
		m_TertiaryWeaponModule.OnEnd();
	}




















	//////////////////////////////////////////////////////////////////////////
	// Reload ( Override )
	protected override bool Predicate_Reload() { return BaseCondition() && m_NeedRecharge == true; }
	protected	override	void			Reload()
	{
		StartCoroutine( ReloadCO( OnEndReload ) );
	}


	//////////////////////////////////////////////////////////////////////////
	// Update ( override )
	protected	override	void			Update()
	{
		m_PrimaryWeaponModule.InternalUpdate();
		m_SecondaryWeaponModule.InternalUpdate();
	}

	
	//////////////////////////////////////////////////////////////////////////
	// OnDestroy
	private void OnDestroy()
	{
//		m_PoolBullets.Destroy();
	}
}
