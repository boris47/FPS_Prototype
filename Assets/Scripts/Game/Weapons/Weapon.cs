
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IWeaponZoom {

	Vector3					ZoomOffset			 { get; }
	float					ZoomingTime			 { get; }
	float					ZommSensitivity		 { get; }
	float					ZoomFactor			{ get; }
}

public interface IWeapon :  IWeaponZoom {
	
	Transform				Transform						{ get; }
	bool					Enabled							{ get; set; }
	WeaponState				WeaponState						{ get; }
	bool					IsFiring						{ get; }
	IFlashLight				FlashLight						{ get; }

	string					OtherInfo						{ get; }

	bool					CanChangeWeapon					();
	void					OnWeaponChange					();
	float					Draw							();
	float					Stash							();
}


[System.Serializable]
public abstract class Weapon : MonoBehaviour, IWeapon {

//	protected	delegate	void	FireFunction();

	[Header("Weapon Properties")]

	[SerializeField]	protected		Vector3		m_ZoomOffset				= Vector3.zero;
	[SerializeField]	protected		float		m_ZoomingTime				= 1f;

	// SECTION NAME
	protected		string							m_WpnBaseSectionName		= "";

	// ATTACHMENTS
	protected		IFlashLight						m_FlashLight				= null;
	protected		Laser							m_Laser						= null;
	protected		GranadeLauncher					m_GranadeLauncher			= null;

	// WEAPON STATE
	protected		WeaponState						m_WeaponState				= WeaponState.STASHED;
	protected		WeaponSubState					m_WeaponSubState			= WeaponSubState.IDLE;

	// INTERNALS
	protected		Vector3							m_StartOffset				= Vector3.zero;
	protected		float							m_ZoomFactor				= 1.0f;
	protected		float							m_ZoomSensitivity			= 1.0f;

	// INTERFACE START
	Transform				IWeapon.Transform			{ get { return transform; } }
	bool					IWeapon.Enabled				{ get { return enabled; } set { enabled = value; } }
	bool					IWeapon.IsFiring			{ get { return m_IsFiring; } }
	IFlashLight				IWeapon.FlashLight			{ get { return m_FlashLight; } }
	WeaponState				IWeapon.WeaponState			{ get { return m_WeaponState; } }
	string					IWeapon.OtherInfo			{ get { return OtherInfo; } }

	Vector3					IWeaponZoom.ZoomOffset		{ get { return m_ZoomOffset; } }
	float					IWeaponZoom.ZoomingTime		{ get { return m_ZoomingTime; } }
	float					IWeaponZoom.ZommSensitivity	{ get { return GetZoomSensitivity(); } }
	float					IWeaponZoom.ZoomFactor		{ get { return m_ZoomFactor; } }
	// INTERFACE END

	
	// UNITY COMPONENTS
	protected		Animator						m_Animator					= null;
	
	// ANIMATIONS
	protected		AnimationClip					m_FireAnim					= null;
	protected		AnimationClip					m_ReloadAnim				= null;
	protected		AnimationClip					m_DrawAnim					= null;

	// Weapon Flags
	protected		bool							m_IsLocked					= false;
	protected		bool							m_IsFiring					= false;
	protected		bool							m_NeedRecharge
	{
		get {
			IWPN_FireModule firemod1 = m_PrimaryWeaponModule as IWPN_FireModule;
			IWPN_FireModule firemod2 = m_SecondaryWeaponModule as IWPN_FireModule;

			return ( ( firemod1 != null && firemod1.NeedRecharge ) || ( firemod2 != null && firemod2.NeedRecharge ) );
		}
	}

	// WEAPON MODULES
	[Header("WeaponModules")]
	[SerializeField]
	protected		WPN_BaseModule					m_PrimaryWeaponModule		= new WPN_BaseModuleEmpty();
	[SerializeField]
	protected		WPN_BaseModule					m_SecondaryWeaponModule		= new WPN_BaseModuleEmpty();
	[SerializeField]
	protected		WPN_BaseModule					m_TertiaryWeaponModule		= new WPN_BaseModuleEmpty();

	protected	abstract		string				OtherInfo { get; }


	////////////////////////////////////////////////////
	////////////////////////////////////////////////////
	////////////////////////////////////////////////////


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Virtual )
	protected	virtual	void				Awake()
	{
		m_WpnBaseSectionName = this.GetType().FullName;

		WeaponManager.Instance.RegisterWpn( this );

		// animations
		{
			bool result = true;
			result &= Utils.Base.SearchComponent<Animator>( gameObject, ref m_Animator, SearchContext.LOCAL);// transform.GetComponent<Animator>();
			result &= m_Animator.GetClipFromAnimator( "fire",		ref m_FireAnim );
			result &= m_Animator.GetClipFromAnimator( "reload",		ref m_ReloadAnim );
			result &= m_Animator.GetClipFromAnimator( "draw",		ref m_DrawAnim );
			Debug.Log( "Animations for weapon " + m_WpnBaseSectionName + "are " + ( ( result ) ? "correctly loaded" : "invalid!!!" ) );
		}

		// laser
		m_Laser = GetComponentInChildren<Laser>();

		// granade launcher
		m_GranadeLauncher = GetComponentInChildren<GranadeLauncher>();

		// flashlight
		m_FlashLight = GetComponentInChildren<FlashLight>() as IFlashLight;

		// Registering game events
		GameManager.StreamEvents.OnSave += OnSave;
		GameManager.StreamEvents.OnLoad += OnLoad;

		// LOAD CONFIGURATION
		{
			Database.Section section = null;
			if ( GameManager.Configs.bGetSection( m_WpnBaseSectionName, ref section ) )
			{
				m_ZoomingTime		= section.AsFloat( "ZoomingTime", m_ZoomingTime );
				m_ZoomSensitivity	= section.AsFloat( "ZoomSensitivity", m_ZoomSensitivity );
				m_ZoomFactor		= section.AsFloat( "ZoomFactor", m_ZoomFactor );

				// Primary Weapon Module
				{
					string WeaponModuleSection = null;
					if ( section.bAsString( "PrimaryWeaponModule", ref WeaponModuleSection ) )
					{
						LoadWeaponModule( m_WpnBaseSectionName, this, WeaponModuleSection, ref m_PrimaryWeaponModule );
					}
				}

				// Secondary Weapon Module
				{
					string WeaponModuleSection = null;
					if ( section.bAsString( "SecondaryWeaponModule", ref WeaponModuleSection ) )
					{
						LoadWeaponModule( m_WpnBaseSectionName, this, WeaponModuleSection, ref m_SecondaryWeaponModule );
					}
				}

				//Tertiary Weapon Module
				{
					string WeaponModuleSection = null;
					if ( section.bAsString( "TertiaryWeaponModule", ref WeaponModuleSection ) )
					{
						LoadWeaponModule( m_WpnBaseSectionName, this, WeaponModuleSection, ref m_TertiaryWeaponModule );
					}
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// LoadWeaponModule
	private	static	bool					LoadWeaponModule( string weaponSection, IWeapon wpn, string weaponModuleSection, ref WPN_BaseModule weaponModule )
	{
		System.Type type = System.Type.GetType( weaponModuleSection.Trim() );
		if ( type == null )
		{
			Debug.Log( "Weapon:AWAKE: " + weaponSection + ": Setting invalid weapon module \"" + weaponModuleSection + "\"" );
			return false;
		}
			
		// Check module type as child of WPN_BaseModule
		if ( type.IsSubclassOf( typeof( WPN_BaseModule ) ) == false )
		{
			Debug.Log( "Weapon:AWAKE: " + weaponSection + ": Class Requested is not a supported weapon module, \"" + weaponModuleSection + "\"" );
			return false;
		}
		
		weaponModule = wpn.Transform.gameObject.AddComponent( type ) as WPN_BaseModule;
		weaponModule.Setup( wpn );
//		weaponModule = System.Activator.CreateInstance( type, new object[1] { this } ) as WPN_BaseModule;
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// GetZoomSensitivity
	private		float						GetZoomSensitivity()
	{
		float zommSensitivity = m_ZoomSensitivity;
		if ( m_PrimaryWeaponModule != null )
		{
			zommSensitivity *= m_PrimaryWeaponModule.ZoomSensitivity;
		}

		if ( m_SecondaryWeaponModule != null )
		{
			zommSensitivity *= m_SecondaryWeaponModule.ZoomSensitivity;
		}
		return zommSensitivity;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnValidate ( Virtual )
	protected	virtual	void				OnValidate()
	{
		m_ZoomingTime = Mathf.Max( m_ZoomingTime, 0.1f );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable ( Virtual )
	protected	virtual		void			OnEnable()
	{
		//										COMMAND								COMMAND ID						ACTION							PREDICATE
		GameManager.InputMgr.BindCall( eInputCommands.PRIMARY_FIRE_PRESS,		"Wpn_Primary_Fire_Start",		PrimaryFire_Start,		Predicate_PrimaryFire_Start		);
		GameManager.InputMgr.BindCall( eInputCommands.PRIMARY_FIRE_HOLD,		"Wpn_Primary_Fire_Update",		PrimaryFire_Update,		Predicate_PrimaryFire_Update	);
		GameManager.InputMgr.BindCall( eInputCommands.PRIMARY_FIRE_RELEASE,		"Wpn_Primary_Fire_End",			PrimaryFire_End,		Predicate_PrimaryFire_End		);

		GameManager.InputMgr.BindCall( eInputCommands.SECONDARY_FIRE_PRESS,		"Wpn_Secondary_Fire_Start",		SecondaryFire_Start,	Predicate_SecondaryFire_Start	);
		GameManager.InputMgr.BindCall( eInputCommands.SECONDARY_FIRE_HOLD,		"Wpn_Secondary_Fire_Update",	SecondaryFire_Update,	Predicate_SecondaryFire_Update	);
		GameManager.InputMgr.BindCall( eInputCommands.SECONDARY_FIRE_RELEASE,	"Wpn_Secondary_Fire_End",		SecondaryFire_End,		Predicate_SecondaryFire_End		);

		GameManager.InputMgr.BindCall( eInputCommands.TERTIARY_FIRE_PRESS,		"Wpn_Tertiary_Fire_Start",		TertiaryFire_Start,		Predicate_TertiaryFire_Start	);
		GameManager.InputMgr.BindCall( eInputCommands.TERTIARY_FIRE_HOLD,		"Wpn_Tertiary_Fire_Update",		TertiaryFire_Update,	Predicate_TertiaryFire_Update	);
		GameManager.InputMgr.BindCall( eInputCommands.TERTIARY_FIRE_RELEASE,	"Wpn_Tertiary_Fire_End",		TertiaryFire_End,		Predicate_TertiaryFire_End		);

		GameManager.InputMgr.BindCall( eInputCommands.RELOAD_WPN,				"Wpn_Reload",					Reload,					Predicate_Reload );

		GameManager.InputMgr.BindCall( eInputCommands.STATE_RUN,				"Wpn_ExitZoom",
			() => { WeaponManager.Instance.ZoomOut(); },
			delegate() { return  Player.Instance.IsRunning && WeaponManager.Instance.IsZoomed; }
		);
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDisable ( Virtual )
	protected	virtual		void			OnDisable()
	{
		GameManager.InputMgr.UnbindCall( eInputCommands.PRIMARY_FIRE_PRESS,		"Wpn_Primary_Fire_Start"	);
		GameManager.InputMgr.UnbindCall( eInputCommands.PRIMARY_FIRE_HOLD,		"Wpn_Primary_Fire_Update"	);
		GameManager.InputMgr.UnbindCall( eInputCommands.PRIMARY_FIRE_RELEASE,	"Wpn_Primary_Fire_End"		);

		GameManager.InputMgr.UnbindCall( eInputCommands.SECONDARY_FIRE_PRESS,	"Wpn_Secondary_Fire_Start"	);
		GameManager.InputMgr.UnbindCall( eInputCommands.SECONDARY_FIRE_HOLD,	"Wpn_Secondary_Fire_Update"	);
		GameManager.InputMgr.UnbindCall( eInputCommands.SECONDARY_FIRE_RELEASE,	"Wpn_Secondary_Fire_End"	);

		GameManager.InputMgr.UnbindCall( eInputCommands.TERTIARY_FIRE_PRESS,	"Wpn_Tertiary_Fire_Start"	);
		GameManager.InputMgr.UnbindCall( eInputCommands.TERTIARY_FIRE_HOLD,		"Wpn_Tertiary_Fire_Update"	);
		GameManager.InputMgr.UnbindCall( eInputCommands.TERTIARY_FIRE_RELEASE,	"Wpn_Tertiary_Fire_End"		);

		GameManager.InputMgr.UnbindCall( eInputCommands.RELOAD_WPN,				"Wpn_Reload" );

		GameManager.InputMgr.UnbindCall( eInputCommands.STATE_RUN,				"Wpn_ExitZoom" );
	}


	// PREDICATES	START
	protected	virtual		bool			Predicate_PrimaryFire_Start()		{ return true; }
	protected	virtual		bool			Predicate_PrimaryFire_Update()		{ return true; }
	protected	virtual		bool			Predicate_PrimaryFire_End()			{ return true; }

	protected	virtual		bool			Predicate_SecondaryFire_Start()		{ return true; }
	protected	virtual		bool			Predicate_SecondaryFire_Update()	{ return true; }
	protected	virtual		bool			Predicate_SecondaryFire_End()		{ return true; }

	protected	virtual		bool			Predicate_TertiaryFire_Start()		{ return true; }
	protected	virtual		bool			Predicate_TertiaryFire_Update()		{ return true; }
	protected	virtual		bool			Predicate_TertiaryFire_End()		{ return true; }

	protected	virtual		bool			Predicate_Reload()					{ return true; }
	// PREDICATES	END

	
	//////////////////////////////////////////////////////////////////////////
	// Primary Fire
	protected	abstract	void			PrimaryFire_Start();
	protected	virtual		void			PrimaryFire_Update() { }
	protected	abstract	void			PrimaryFire_End();
	
	//////////////////////////////////////////////////////////////////////////
	// Secondary Fire
	protected	abstract	void			SecondaryFire_Start();
	protected	virtual		void			SecondaryFire_Update() { }
	protected	abstract	void			SecondaryFire_End();

	//////////////////////////////////////////////////////////////////////////
	// Tertiary Fire
	protected	abstract	void			TertiaryFire_Start();
	protected	virtual		void			TertiaryFire_Update() { }
	protected	abstract	void			TertiaryFire_End();


	//////////////////////////////////////////////////////////////////////////
	// Reload ( Virtual )
	protected	virtual		void			Reload()
	{ }


	//////////////////////////////////////////////////////////////////////////
	// OnSave ( Virtual )
	protected	virtual		StreamUnit		OnSave( StreamData streamData )
	{
		StreamUnit streamUnit	= streamData.NewUnit( gameObject );

		if ( m_PrimaryWeaponModule is IWPN_FireModule )
		{
			uint mag = ( m_PrimaryWeaponModule as IWPN_FireModule ).Magazine;
			streamUnit.AddInternal( "PrimaryModule_Magazine", mag );
		}

		if ( m_SecondaryWeaponModule is IWPN_FireModule )
		{
			uint mag = ( m_SecondaryWeaponModule as IWPN_FireModule ).Magazine;
			streamUnit.AddInternal( "SecondaryModule_Magazine", mag );
		}

		// Save Weapon Modules Data

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad ( Virtual )
	protected	virtual		StreamUnit		OnLoad( StreamData streamData )
	{
		m_Animator.Play( "draw", -1, 0.99f );

		StreamUnit streamUnit = null;
		if ( streamData.GetUnit( gameObject, ref streamUnit ) )
		{
			m_IsFiring = false;

			if ( m_PrimaryWeaponModule is IWPN_FireModule )
			{
				uint mag = (uint)streamUnit.GetAsInt( "PrimaryModule_Magazine" );
				( m_PrimaryWeaponModule as IWPN_FireModule ).OnLoad( mag );
			}

			if ( m_SecondaryWeaponModule is IWPN_FireModule )
			{
				uint mag = (uint)streamUnit.GetAsFloat( "SecondaryModule_Magazine" );
				( m_SecondaryWeaponModule as IWPN_FireModule ).OnLoad( mag );
			}

			// Load Weapon Modules Data

			UI.Instance.InGame.UpdateUI();
		}
		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// CanChangeWeapon ( Virtual )
	public		virtual		bool			CanChangeWeapon()
	{
		return m_IsLocked == false && m_PrimaryWeaponModule.CanChangeWeapon() && m_SecondaryWeaponModule.CanChangeWeapon();	
	}


	//////////////////////////////////////////////////////////////////////////
	// OnWeaponChange ( Virtual )
	/// <summary> Called before weapon change </summary>
	public		virtual		void			OnWeaponChange()
	{
		enabled			= false;
	}


	//////////////////////////////////////////////////////////////////////////
	// Draw ( virtual )
	public		virtual		float			Draw()
	{
		m_Animator.Play( "draw", -1, 0f );
		m_WeaponState	= WeaponState.DRAWED;
		return m_DrawAnim.length;
	}


	//////////////////////////////////////////////////////////////////////////
	// Stash ( virtual )
	public		virtual		float			Stash()
	{
		m_Animator.Play( "stash", -1, 0f );
		m_WeaponState	= WeaponState.STASHED;
		return m_DrawAnim.length;
	}


	//////////////////////////////////////////////////////////////////////////
	// Update ( Abstract )
	protected	abstract	void			Update();


	protected	IEnumerator	ReloadCO( System.Action onReloadEnd )
	{
		m_IsLocked = true;

		m_PrimaryWeaponModule.enabled = false;
		m_SecondaryWeaponModule.enabled = false;

		if ( WeaponManager.Instance.IsZoomed )
		{
			yield return WeaponManager.Instance.ZoomOutCO();
		}

		// Reload animation
		{
			m_Animator.Play( m_ReloadAnim.name, -1, 0f );
			float rechargeTimer = m_ReloadAnim.length * m_Animator.speed; // / 2f;
			yield return new WaitForSeconds( rechargeTimer );
		}

		m_PrimaryWeaponModule.enabled = true;
		m_SecondaryWeaponModule.enabled = true;

		m_IsLocked = false;

		onReloadEnd();
	}

	/*
	//////////////////////////////////////////////////////////////////////////
	// OnDestroy ( Virtual )
	protected	virtual void OnDestroy()
	{
		GameManager.StreamEvents.OnSave -= OnSave;
		GameManager.StreamEvents.OnLoad -= OnLoad;
	}
	*/
}


/////////////////////////////////////////
/////////////////////////////////////////
public enum WeaponState {
	DRAWED, STASHED
}

public enum WeaponSubState {
	IDLE, RELOADING, FIRING, TRANSITION
}

/////////////////////////////////////////
/////////////////////////////////////////

public enum FireModes {
	SINGLE, BURST, AUTO, NONE
}