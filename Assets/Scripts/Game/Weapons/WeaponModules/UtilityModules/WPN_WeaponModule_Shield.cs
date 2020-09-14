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
	public	override	bool	OnAttach			( IWeapon w, EWeaponSlots slot )
	{
		string moduleSectionName = this.GetType().FullName;
		this.m_WeaponRef = w;
		if ( GlobalManager.Configs.GetSection( moduleSectionName, ref this.m_ModuleSection ) == false )			// Get Module Section
			return false;

		this.m_ShieldLife = this.m_ModuleSection.AsFloat( "BaseShieldLife", 50f );

		string modulePrefabPath = null;
		if (this.m_ModuleSection.bAsString( "Module_Prefab", ref modulePrefabPath ) )
		{
			GameObject modulePrefab = Resources.Load( modulePrefabPath ) as GameObject;
			if ( modulePrefab )
			{
				this.m_ShieldGO = Instantiate<GameObject>( modulePrefab, this.transform );
				this.m_ShieldGO.transform.localPosition = Vector3.zero;
				this.m_ShieldGO.transform.localRotation = Quaternion.identity;

				this.m_RigidBody	= this.m_ShieldGO.GetComponentInChildren<Rigidbody>();
				this.m_Shield	= this.m_ShieldGO.GetComponentInChildren<Shield>();

				this.m_Shield.enabled = false;
				this.m_ShieldInterface = this.m_Shield as IShield;

				this.m_ShieldInterface.Setup(this.m_ShieldLife, EShieldContext.WEAPON );
				this.m_ShieldInterface.OnHit += this.OnShieldHit;
			}
		}


		if (this.InternalSetup(this.m_ModuleSection ) == false )
			return false;

		return true;
	}

	public override void OnDetach()
	{
		
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
		streamUnit.SetInternal( "TimeToWaitBeforeRestore", this.m_TimeToWaitBeforeRestore );
		streamUnit.SetInternal( "RestorationSpeed", this.m_RestorationSpeed );
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public	override	bool	OnLoad			( StreamUnit streamUnit )
	{
		this.m_TimeToWaitBeforeRestore = streamUnit.GetAsFloat( "TimeToWaitBeforeRestore" );
		this.m_RestorationSpeed = streamUnit.GetAsFloat( "RestorationSpeed" );
		return true;
	}
	
	public	override	bool	CanChangeWeapon	() {  return true; }
	public	override	bool	CanBeUsed		() {  return true; }
	public	override	void	OnWeaponChange	() { }
	public	override	bool	NeedReload		() { return false; }
	public	override	void	OnAfterReload	() { }


	//////////////////////////////////////////////////////////////////////////
	protected	override	void	InternalUpdate	( float DeltaTime )
	{
		if (this.m_TimeToWaitBeforeRestore > 0.0f )
		{
			this.m_TimeToWaitBeforeRestore -= DeltaTime;
			return;
		}

		bool needRestoration = this.m_ShieldInterface.Status < this.m_ShieldInterface.StartStatus;
		if ( needRestoration )
		{
			this.m_ShieldInterface.Status += DeltaTime  * this.m_RestorationSpeed;

			if (this.m_Shield.enabled == false )
			{
				this.m_Shield.enabled = true;
				this.m_RigidBody.detectCollisions = true;
			}
		}
	}

	

	//////////////////////////////////////////////////////////////////////////
	private	void		OnShieldHit( Vector3 startPosition, Entity whoRef, Weapon weaponRef, EDamageType damageType, float damage, bool canPenetrate = false )
	{
		this.m_TimeToWaitBeforeRestore = 3.0f;
		if (this.m_ShieldInterface.Status <= 0.0f )
		{
			this.m_RigidBody.detectCollisions = false;
		}
	}

	
	//////////////////////////////////////////////////////////////////////////
	public override		void	OnStart()
	{
		if (this.m_ShieldInterface.Status > 0.0f )
		{
			this.m_Shield.enabled = true;
			this.m_RigidBody.detectCollisions = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public override void OnEnd()
	{
		this.m_Shield.enabled = false;
		this.m_RigidBody.detectCollisions = false;
	}


	protected override void OnDestroy()
	{
		base.OnDestroy();
		Destroy( this.m_ShieldGO );
	}
}
