
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IWeaponZoom {

	Vector3					ZoomOffset			{ get; }
	float					ZoomingTime			{ get; }
	float					ZoomSensitivity		{ get; }
	float					ZoomFactor			{ get; }
}

public interface IWeapon :  IWeaponZoom {
	
	Transform				Transform						{ get; }
	bool					Enabled							{ get; set; }
	WeaponState				WeaponState						{ get; }
	bool					IsFiring						{ get; }
	IFlashLight				FlashLight						{ get; }
	Laser					Laser							{ get; }
	GranadeLauncher			GranadeLauncher					{ get; }

	Database.Section		Section							{ get; }
	string					OtherInfo						{ get; }

	bool					CanChangeWeapon					();
	void					OnWeaponChange					();
	float					Draw							();
	float					Stash							();
}


[System.Serializable]
public abstract class Weapon : MonoBehaviour, IWeapon {

	[Header("Weapon Properties")]

	[SerializeField, ReadOnly]	protected		Vector3				m_ZoomOffset				= Vector3.zero;

	[Header("WeaponModules")]

	[SerializeField]	protected		WPN_BaseModule		m_PrimaryWeaponModule		= new WPN_BaseModuleEmpty();
	[SerializeField]	protected		WPN_BaseModule		m_SecondaryWeaponModule		= new WPN_BaseModuleEmpty();
	[SerializeField]	protected		WPN_BaseModule		m_TertiaryWeaponModule		= new WPN_BaseModuleEmpty();

	[SerializeField]	protected		Database.Section	m_WpnSection				= null;


	// SECTION NAME
	protected		string									m_WpnBaseSectionName		= "";

	// ATTACHMENTS
	protected		IFlashLight								m_FlashLight				= null;
	protected		Laser									m_Laser						= null;
	protected		GranadeLauncher							m_GranadeLauncher			= null;

	// WEAPON STATE
	protected		WeaponState								m_WeaponState				= WeaponState.STASHED;
	protected		WeaponSubState							m_WeaponSubState			= WeaponSubState.IDLE;

	// INTERNALS
	protected		Vector3									m_StartOffset				= Vector3.zero;
	protected		float									m_BaseZoomFactor			= 1.0f;
	protected		float									m_BaseZoomingTime			= 1.0f;
	protected		float									m_BaseZoomSensitivity		= 1.0f;
	protected		List<Database.Section>					m_Modifiers					= new List<Database.Section>();

	// INTERFACE START
					Transform								IWeapon.Transform			{ get { return transform; } }
					bool									IWeapon.Enabled				{ get { return enabled; } set { enabled = value; } }
					bool									IWeapon.IsFiring			{ get { return m_IsFiring; } }
					IFlashLight								IWeapon.FlashLight			{ get { return m_FlashLight; } }
					Laser									IWeapon.Laser				{ get { return m_Laser; } }
					GranadeLauncher							IWeapon.GranadeLauncher		{ get { return m_GranadeLauncher; } }
					WeaponState								IWeapon.WeaponState			{ get { return m_WeaponState; } }
					Database.Section						IWeapon.Section				{ get { return m_WpnSection; } }
					string									IWeapon.OtherInfo			{ get { return OtherInfo; } }

					Vector3									IWeaponZoom.ZoomOffset		{ get { return m_ZoomOffset; } }

					float									IWeaponZoom.ZoomingTime		{ get { return m_BaseZoomingTime; } }
					float									IWeaponZoom.ZoomSensitivity	{ get { return GetZoomSensitivity(); } }
					float									IWeaponZoom.ZoomFactor		{ get { return m_BaseZoomFactor; } }
	// INTERFACE END

	
	// UNITY COMPONENTS
	protected		Animator								m_Animator					= null;
	
	// ANIMATIONS
	protected		AnimationClip							m_FireAnim					= null;
	protected		AnimationClip							m_ReloadAnim				= null;
	protected		AnimationClip							m_DrawAnim					= null;

	// Weapon Flags
	protected		bool									m_IsLocked					= false;
	protected		bool									m_IsFiring					= false;
	protected		bool									m_NeedRecharge
	{
		get {
			return m_PrimaryWeaponModule.NeedReload() || m_SecondaryWeaponModule.NeedReload() || m_TertiaryWeaponModule.NeedReload();
		}
	}



	//////////////////////////////////////////////////////////////////////////
	protected virtual string OtherInfo
	{
		get {
			string primaryModule	= m_PrimaryWeaponModule.name;
			string secondaryModule	= m_SecondaryWeaponModule.name;
			string tertiaryModule	= m_TertiaryWeaponModule.name;
			return primaryModule + ", " + secondaryModule + ", " + tertiaryModule;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void				Awake()
	{
		m_WpnBaseSectionName = this.GetType().FullName;

		bool weaponAwakeSuccess = true;

		// Animations
		{
			weaponAwakeSuccess &= Utils.Base.SearchComponent<Animator>( gameObject, ref m_Animator, SearchContext.LOCAL);
			weaponAwakeSuccess &= m_Animator.GetClipFromAnimator( "fire",		ref m_FireAnim );
			weaponAwakeSuccess &= m_Animator.GetClipFromAnimator( "reload",		ref m_ReloadAnim );
			weaponAwakeSuccess &= m_Animator.GetClipFromAnimator( "draw",		ref m_DrawAnim );
			Debug.Log( "Animations for weapon " + m_WpnBaseSectionName + " are " + ( ( weaponAwakeSuccess ) ? "correctly loaded" : "invalid!!!" ) );
		}

		// Laser
		m_Laser = GetComponentInChildren<Laser>();

		// Granade Launcher
		m_GranadeLauncher = GetComponentInChildren<GranadeLauncher>();

		// Flashlight
		m_FlashLight = GetComponentInChildren<FlashLight>() as IFlashLight;

		// Registering game events
		GameManager.StreamEvents.OnSave += OnSave;
		GameManager.StreamEvents.OnLoad += OnLoad;

		m_PrimaryWeaponModule	= gameObject.AddComponent<WPN_BaseModuleEmpty>();
		m_SecondaryWeaponModule	= gameObject.AddComponent<WPN_BaseModuleEmpty>();
		m_TertiaryWeaponModule	= gameObject.AddComponent<WPN_BaseModuleEmpty>();

		// LOAD CONFIGURATION
		if ( weaponAwakeSuccess &= GameManager.Configs.bGetSection( m_WpnBaseSectionName, ref m_WpnSection ) )
		{
			m_WpnSection.bAsVec3( "ZoomOffset", ref m_ZoomOffset, null );

			m_BaseZoomingTime		= m_WpnSection.AsFloat( "BaseZoomingTime",		m_BaseZoomingTime );
			m_BaseZoomSensitivity	= m_WpnSection.AsFloat( "BaseZoomSensitivity",	m_BaseZoomSensitivity );
			m_BaseZoomFactor		= m_WpnSection.AsFloat( "BaseZoomFactor",		m_BaseZoomFactor );

			// Primary Weapon Module
			weaponAwakeSuccess &= LoadAndConfigureModule( this, m_WpnSection, "PrimaryWeaponModule", ref m_PrimaryWeaponModule );

			// Secondary Weapon Module
			weaponAwakeSuccess &= LoadAndConfigureModule( this, m_WpnSection, "SecondaryWeaponModule", ref m_SecondaryWeaponModule );

			//Tertiary Weapon Module
			weaponAwakeSuccess &= LoadAndConfigureModule( this, m_WpnSection, "TertiaryWeaponModule", ref m_TertiaryWeaponModule );
		}

		// Only if the construction complete successflly, the weapon get registered
		if ( weaponAwakeSuccess )
		{
			WeaponManager.Instance.RegisterWpn( this );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	protected		void					ApplyModifier( Database.Section modifier )
	{
		float MultZoomFactor			= modifier.AsFloat( "MultZoomFactor",					1.0f );
		float MultZoomingTime			= modifier.AsFloat( "MultZoomingTime",					1.0f );
		float MultZoomSensitivity		= modifier.AsFloat( "MultZoomSensitivity",				1.0f );

		m_BaseZoomFactor				*= MultZoomFactor;
		m_BaseZoomingTime				*= MultZoomingTime;
		m_BaseZoomSensitivity			*= MultZoomSensitivity;

		// Primary Weapon Module
		LoadAndConfigureModule( this, modifier, "PrimaryWeaponModule", ref m_PrimaryWeaponModule );

		// Secondary Weapon Module
		LoadAndConfigureModule( this, modifier, "SecondaryWeaponModule", ref m_SecondaryWeaponModule );

		// Tertiary Weapon Module
		LoadAndConfigureModule( this, modifier, "TertiaryWeaponModule", ref m_TertiaryWeaponModule );
	}


	//////////////////////////////////////////////////////////////////////////
	private	static	bool					LoadAndConfigureModule( IWeapon wpn, Database.Section section, string weaponModuleSectionName, ref WPN_BaseModule weaponModule )
	{
		string WeaponModuleSection = null;
		if ( section.AsBool( "Has" + weaponModuleSectionName ) && section.bAsString( weaponModuleSectionName, ref WeaponModuleSection ) )
		{
			Database.Section moduleSection = null;
			if ( GameManager.Configs.bGetSection( WeaponModuleSection, ref moduleSection ) )


			if ( LoadWeaponModule( wpn, WeaponModuleSection, ref weaponModule ) == false )
			{
				Destroy( weaponModule );
				weaponModule	= wpn.Transform.gameObject.AddComponent<WPN_BaseModuleEmpty>();
				return false;
			}

			ConfigureModule( section, weaponModuleSectionName, weaponModule );
		}
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	private	static	bool					LoadWeaponModule( IWeapon wpn, string weaponModuleSection, ref WPN_BaseModule weaponModule )
	{
		System.Type type = System.Type.GetType( weaponModuleSection.Trim() );
		if ( type == null )
		{
			Debug.Log( "Weapon:AWAKE: " + wpn.Section.Name() + ": Setting invalid weapon module \"" + weaponModuleSection + "\"" );
			return false;
		}
			
		// Check module type as child of WPN_BaseModule
		if ( type.IsSubclassOf( typeof( WPN_BaseModule ) ) == false )
		{
			Debug.Log( "Weapon:AWAKE: " + wpn.Section.Name() + ": Class Requested is not a supported weapon module, \"" + weaponModuleSection + "\"" );
			return false;
		}

		if ( weaponModule != null )
		{
			Destroy( weaponModule );
		}
		
		weaponModule = wpn.Transform.gameObject.AddComponent( type ) as WPN_BaseModule;

		return weaponModule.Setup( wpn );
	}


	//////////////////////////////////////////////////////////////////////////
	private static	void					ConfigureModule( Database.Section section, string weaponModuleSectionName, WPN_BaseModule weaponModule )
	{
		string[] mods = null;
		if ( section.bGetMultiAsArray( weaponModuleSectionName + "Mods", ref mods ) )
		{
			Database.Section modifierSection = null;
			foreach( string modifierSectionName in mods )
			{
				if ( GameManager.Configs.bGetSection( modifierSectionName, ref modifierSection ) )
				{
					weaponModule.ApplyModifier( modifierSection );
				}
			}

		}
	}



	//////////////////////////////////////////////////////////////////////////
	private		float						GetZoomSensitivity()
	{
		float zoomSensitivity		= m_BaseZoomSensitivity;

		System.Array.ForEach
		(
			GetComponents<WPN_WeaponModule_Zoom>(),
			s => zoomSensitivity *= s.ZoomSensitivity
		);
		
		return zoomSensitivity;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void				OnValidate()
	{
		m_BaseZoomingTime = Mathf.Max( m_BaseZoomingTime, 0.1f );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			OnEnable()
	{
		// Update events registration
		GameManager.UpdateEvents.OnFrame += m_PrimaryWeaponModule.InternalUpdate;
		GameManager.UpdateEvents.OnFrame += m_SecondaryWeaponModule.InternalUpdate;
		GameManager.UpdateEvents.OnFrame += m_TertiaryWeaponModule.InternalUpdate;

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
	protected	virtual		void			OnDisable()
	{
		// Update events un-registration
		GameManager.UpdateEvents.OnFrame -= m_PrimaryWeaponModule.InternalUpdate;
		GameManager.UpdateEvents.OnFrame -= m_SecondaryWeaponModule.InternalUpdate;
		GameManager.UpdateEvents.OnFrame -= m_TertiaryWeaponModule.InternalUpdate;

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


	#region		PREDICATES
	// PREDICATES	START
	protected virtual		bool			Predicate_Base() { return m_WeaponState == WeaponState.DRAWED && Player.Instance.ChosingDodgeRotation == false && m_IsLocked == false; }
	protected	virtual		bool			Predicate_PrimaryFire_Start()		{ return Predicate_Base() && m_PrimaryWeaponModule.CanBeUsed(); }
	protected	virtual		bool			Predicate_PrimaryFire_Update()		{ return Predicate_Base() && m_PrimaryWeaponModule.CanBeUsed(); }
	protected	virtual		bool			Predicate_PrimaryFire_End()			{ return Predicate_Base() && m_PrimaryWeaponModule.CanBeUsed(); }

	protected	virtual		bool			Predicate_SecondaryFire_Start()		{ return Predicate_Base() && m_SecondaryWeaponModule.CanBeUsed(); }
	protected	virtual		bool			Predicate_SecondaryFire_Update()	{ return Predicate_Base() && m_SecondaryWeaponModule.CanBeUsed(); }
	protected	virtual		bool			Predicate_SecondaryFire_End()		{ return Predicate_Base() && m_SecondaryWeaponModule.CanBeUsed(); }

	protected	virtual		bool			Predicate_TertiaryFire_Start()		{ return Predicate_Base() && m_TertiaryWeaponModule.CanBeUsed(); }
	protected	virtual		bool			Predicate_TertiaryFire_Update()		{ return Predicate_Base() && m_TertiaryWeaponModule.CanBeUsed(); }
	protected	virtual		bool			Predicate_TertiaryFire_End()		{ return Predicate_Base() && m_TertiaryWeaponModule.CanBeUsed(); }

	protected	virtual		bool			Predicate_Reload()					{ return Predicate_Base() && m_NeedRecharge == true; }
	// PREDICATES	END
	#endregion
	
	
	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			PrimaryFire_Start()		{	m_PrimaryWeaponModule.OnStart();	}
	protected	virtual		void			PrimaryFire_Update()	{	m_PrimaryWeaponModule.OnUpdate();	}
	protected	virtual		void			PrimaryFire_End()		{	m_PrimaryWeaponModule.OnEnd();		}
	
	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			SecondaryFire_Start()	{	m_SecondaryWeaponModule.OnStart();	}
	protected	virtual		void			SecondaryFire_Update()	{	m_SecondaryWeaponModule.OnUpdate();	}
	protected	virtual		void			SecondaryFire_End()		{	m_SecondaryWeaponModule.OnEnd();	}

	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			TertiaryFire_Update()	{	m_TertiaryWeaponModule.OnStart();	}
	protected	virtual		void			TertiaryFire_Start()	{	m_TertiaryWeaponModule.OnUpdate();	}
	protected	virtual		void			TertiaryFire_End()		{	m_TertiaryWeaponModule.OnEnd();		}



	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			Reload()
	{
		StartCoroutine( ReloadCO( OnEndReload ) );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		StreamUnit		OnSave( StreamData streamData )
	{
		StreamUnit streamUnit	= streamData.NewUnit( gameObject );
		
		m_PrimaryWeaponModule.OnSave( streamUnit );
		m_SecondaryWeaponModule.OnSave( streamUnit );
		m_TertiaryWeaponModule.OnSave( streamUnit );

		// FLASHLIGHT
		if ( m_FlashLight != null )
		{
			streamUnit.SetInternal( "FlashLightActive", m_FlashLight.Activated );
		}

		// Save Weapon Modules Data

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		StreamUnit		OnLoad( StreamData streamData )
	{
		m_Animator.Play( "draw", -1, 0.99f );

		StreamUnit streamUnit = null;
		if ( streamData.GetUnit( gameObject, ref streamUnit ) )
		{
			m_IsFiring = false;

			m_PrimaryWeaponModule.OnLoad( streamUnit );
			m_SecondaryWeaponModule.OnLoad( streamUnit );
			m_TertiaryWeaponModule.OnLoad( streamUnit );

			// FLASHLIGHT
			if ( m_FlashLight != null )
			{
				m_FlashLight.SetActive( streamUnit.GetAsBool( "FlashLightActive") );
			}

			// Load Weapon Modules Data

			UI.Instance.InGame.UpdateUI();
		}
		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Return if the current weapon allow the change at this time </summary>
	public		virtual		bool			CanChangeWeapon()
	{
		bool result = m_IsLocked == false;
		result &= m_PrimaryWeaponModule.CanChangeWeapon();
		result &= m_SecondaryWeaponModule.CanChangeWeapon();
		result &= m_TertiaryWeaponModule.CanChangeWeapon();
		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Called before weapon change </summary>
	public		virtual		void			OnWeaponChange()
	{
		m_PrimaryWeaponModule.OnWeaponChange();
		m_SecondaryWeaponModule.OnWeaponChange();
		m_TertiaryWeaponModule.OnWeaponChange();

		enabled			= false;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Callback after the reload animation </summary>
	protected	virtual		void			OnEndReload()
	{
		m_PrimaryWeaponModule.OnAfterReload();
		m_SecondaryWeaponModule.OnAfterReload();
		m_TertiaryWeaponModule.OnAfterReload();

		// Update UI
		UI.Instance.InGame.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Start the draw animation, return the seconds to wait </summary>
	public		virtual		float			Draw()
	{
		m_Animator.Play( "draw", -1, 0f );
		m_WeaponState	= WeaponState.DRAWED;
		return m_DrawAnim.length;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Start the stash animation, return the seconds to wait </summary>
	public		virtual		float			Stash()
	{
		m_Animator.Play( "stash", -1, 0f );
		m_WeaponState	= WeaponState.STASHED;
		return m_DrawAnim.length;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			Update() { }


	//////////////////////////////////////////////////////////////////////////
	protected				IEnumerator		ReloadCO( System.Action onReloadEnd )
	{
		m_IsLocked = true;

		m_PrimaryWeaponModule.enabled = false;
		m_SecondaryWeaponModule.enabled = false;
		m_TertiaryWeaponModule.enabled = false;

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
		m_TertiaryWeaponModule.enabled = true;

		m_IsLocked = false;

		onReloadEnd();
	}

	
	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			OnDestroy()
	{
		GameManager.StreamEvents.OnSave -= OnSave;
		GameManager.StreamEvents.OnLoad -= OnLoad;
	}
	
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