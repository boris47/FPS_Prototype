using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WPN_WeaponModule_Shield : WPN_BaseModule, IWPN_UtilityModule {

	protected	float				m_ShieldLife					= 1.0f;

	protected	Rigidbody			m_RigidBody						= null;
	protected	Shield				m_Shield						= null;
	protected	IShield				m_ShieldInterface				= null;
	protected	GameObject			m_ShieldGO						= null;

	protected	float				m_TimeToWaitBeforeRestore		= 0.0f;
	protected	float				m_RestorationSpeed				= 5.0f;


	//////////////////////////////////////////////////////////////////////////
	public	override	bool	Setup			( IWeapon w, WeaponSlots slot )
	{
		string moduleSectionName = this.GetType().FullName;
		m_WeaponRef = w;
		if ( GameManager.Configs.bGetSection( moduleSectionName, ref m_ModuleSection ) == false )			// Get Module Section
			return false;

		m_ShieldLife = m_ModuleSection.AsFloat( "BaseShieldLife", 50f );

		string modulePrefabPath = null;
		if ( m_ModuleSection.bAsString( "Module_Prefab", ref modulePrefabPath ) )
		{
			GameObject modulePrefab = Resources.Load( modulePrefabPath ) as GameObject;
			if ( modulePrefab )
			{
				m_ShieldGO = Instantiate<GameObject>( modulePrefab, transform );
				m_ShieldGO.transform.localPosition = Vector3.zero;
				m_ShieldGO.transform.localRotation = Quaternion.identity;

				m_RigidBody	= m_ShieldGO.GetComponentInChildren<Rigidbody>();

				m_Shield	= m_ShieldGO.GetComponentInChildren<Shield>();
				m_Shield.enabled = false;
				m_ShieldInterface = m_Shield as IShield;

				m_ShieldInterface.Setup( m_ShieldLife );
				m_ShieldInterface.OnHit += OnShieldHit;
			}
		}


		if ( InternalSetup( m_ModuleSection ) == false )
			return false;

		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	bool	InternalSetup( Database.Section moduleSection )
	{
		return true;
	}


	//		MODIFIERS
	//////////////////////////////////////////////////////////////////////////
	public override void ApplyModifier( Database.Section modifier )
	{
		// Do actions here

		base.ApplyModifier( modifier );
	}


	public	override	void	ResetBaseConfiguration()
	{
		// Do actions here

		base.ResetBaseConfiguration();
	}

	public	override	void	RemoveModifier( Database.Section modifier )
	{
		// Do Actions here

		base.RemoveModifier( modifier );
	}


		//////////////////////////////////////////////////////////////////////////
	public	override	bool	OnSave			( StreamUnit streamUnit )
	{
		streamUnit.SetInternal( "TimeToWaitBeforeRestore", m_TimeToWaitBeforeRestore );
		streamUnit.SetInternal( "RestorationSpeed", m_RestorationSpeed );
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public	override	bool	OnLoad			( StreamUnit streamUnit )
	{
		m_TimeToWaitBeforeRestore = streamUnit.GetAsFloat( "TimeToWaitBeforeRestore" );
		m_RestorationSpeed = streamUnit.GetAsFloat( "RestorationSpeed" );
		return true;
	}
	
	public	override	bool	CanChangeWeapon	() {  return true; }
	public	override	bool	CanBeUsed		() {  return true; }
	public	override	void	OnWeaponChange	() { }
	public	override	bool	NeedReload		() { return false; }
	public	override	void	OnAfterReload	() { }


	//////////////////////////////////////////////////////////////////////////
	public	override	void	InternalUpdate	( float DeltaTime )
	{
		if ( m_TimeToWaitBeforeRestore > 0.0f )
		{
			m_TimeToWaitBeforeRestore -= DeltaTime;
			return;
		}

		bool needRestoration = m_ShieldInterface.Status < m_ShieldInterface.StartStatus;
		if ( needRestoration )
		{
			m_ShieldInterface.Status += DeltaTime  * m_RestorationSpeed;

			if ( m_Shield.enabled == false )
			{
				m_Shield.enabled = true;
				m_RigidBody.detectCollisions = true;
			}
		}
	}

	

	//////////////////////////////////////////////////////////////////////////
	private	void		OnShieldHit( Vector3 startPosition, Entity whoRef, Weapon weaponRef, float damage, bool canPenetrate = false )
	{
		m_TimeToWaitBeforeRestore = 3.0f;
		if ( m_ShieldInterface.Status <= 0.0f )
		{
			m_RigidBody.detectCollisions = false;
		}
	}

	
	//////////////////////////////////////////////////////////////////////////
	public override		void	OnStart()
	{
		if ( m_ShieldInterface.Status > 0.0f )
		{
			m_Shield.enabled = true;
			m_RigidBody.detectCollisions = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public override void OnEnd()
	{
		m_Shield.enabled = false;
		m_RigidBody.detectCollisions = false;
	}

}
