
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWeaponZoom {

	Vector3					ZoomOffset			{ get; }
	float					ZoomingTime			{ get; }
	float					ZoomSensitivity		{ get; }
	float					ZoomFactor			{ get; }
}

public interface IModifiable {

	void					ApplyModifier			( Database.Section modifier );
	void					ResetBaseConfiguration	();
	void					RemoveModifier			( Database.Section modifier );
}

public interface IAttachments {

	// Weapon Attachments
	IFlashLight				Flashlight						{ get; }
	bool					HasFlashlight					{ get; }
	ILaser					Laser							{ get; }
	bool					HasLaser						{ get; }
	IGranadeLauncher		GranadeLauncher					{ get; }
	bool					HasGranadeLauncher				{ get; }

}

public interface IWeapon :  IAttachments, IWeaponZoom, IModifiable {
	
	Transform				Transform						{ get; }
	bool					Enabled							{ get; set; }
	WeaponState				WeaponState						{ get; }

	bool					bGetModuleBySlot				( WeaponSlots slot, ref WPN_BaseModule weaponModule );
	bool					bGetModuleSlot					( WeaponSlots slot, ref WeaponModuleSlot moduleSlot );	

	Database.Section		Section							{ get; }
	string					OtherInfo						{ get; }

	bool					CanChangeWeapon					();
	void					OnWeaponChange					();
	float					Draw							();
	float					Stash							();

	void					Hide							();
	void					Show							();
}



[System.Serializable]
public abstract partial class Weapon : MonoBehaviour, IWeapon {

	[Header("Weapon Properties")]

	[SerializeField, ReadOnly]	protected		Vector3				m_ZoomOffset				= Vector3.zero;

	// SECTION
	protected		Database.Section						m_WpnSection				= null;
	protected		string									m_WpnBaseSectionName		= "";

	// ATTACHMENTS
	protected		IFlashLight								m_FlashLight				= null;
	protected		bool									m_bHasFlashlight			= false;
	protected		ILaser									m_Laser						= null;
	protected		bool									m_bHasLaser					= false;
	protected		IGranadeLauncher						m_GranadeLauncher			= null;
	protected		bool									m_bHasGranadeLauncher		= false;


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
					IFlashLight								IAttachments.Flashlight			{ get { return m_FlashLight; } }
					bool									IAttachments.HasFlashlight		{ get { return m_bHasFlashlight; } }
					ILaser									IAttachments.Laser				{ get { return m_Laser; } }
					bool									IAttachments.HasLaser			{ get { return m_bHasLaser; } }
					IGranadeLauncher						IAttachments.GranadeLauncher	{ get { return m_GranadeLauncher; } }
					bool									IAttachments.HasGranadeLauncher	{ get { return m_bHasGranadeLauncher; } }



					Transform								IWeapon.Transform			{ get { return transform; } }
					bool									IWeapon.Enabled				{ get { return enabled; } set { enabled = value; } }
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
//	protected		AnimationClip							m_FireAnim					= null;
	protected		AnimationClip							m_ReloadAnim				= null;
	protected		AnimationClip							m_DrawAnim					= null;

	// Weapon Flags
	protected		bool									m_IsLocked					= false;
	protected		bool									m_NeedRecharge
	{
		get {
			return m_PrimaryWeaponModuleSlot.WeaponModule.NeedReload() || 
				m_SecondaryWeaponModuleSlot.WeaponModule.NeedReload() || 
				m_TertiaryWeaponModuleSlot.WeaponModule.NeedReload();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	string				OtherInfo
	{
		get {
			string primaryModule	= m_PrimaryWeaponModuleSlot.WeaponModule.ModuleSection ? m_PrimaryWeaponModuleSlot.WeaponModule.ModuleSection.GetName() : "None";
			string secondaryModule	= m_SecondaryWeaponModuleSlot.WeaponModule.ModuleSection ? m_SecondaryWeaponModuleSlot.WeaponModule.ModuleSection.GetName() : "None";
			string tertiaryModule	= m_TertiaryWeaponModuleSlot.WeaponModule.ModuleSection ? m_TertiaryWeaponModuleSlot.WeaponModule.ModuleSection.GetName() : "None";
			return primaryModule + "," + secondaryModule + "," + tertiaryModule;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	void	UpdateAttachments()
	{
		// Flashlight
		m_bHasFlashlight		= Utils.Base.SearchComponent( gameObject, ref m_FlashLight, SearchContext.CHILDREN );

		// Laser
		m_bHasLaser				= Utils.Base.SearchComponent( gameObject, ref m_Laser, SearchContext.CHILDREN );

		// Granade Launcher
		m_bHasGranadeLauncher	= Utils.Base.SearchComponent( gameObject, ref m_GranadeLauncher, SearchContext.CHILDREN );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void				Awake()
	{
		System.Diagnostics.Stopwatch m_StopWatch = new System.Diagnostics.Stopwatch();
		m_StopWatch.Start();
		m_WpnBaseSectionName = this.GetType().FullName;

		bool weaponAwakeSuccess = true;

		// Animations
		{
			weaponAwakeSuccess &= Utils.Base.SearchComponent<Animator>( gameObject, ref m_Animator, SearchContext.LOCAL);
//			weaponAwakeSuccess &= m_Animator.GetClipFromAnimator( "fire",		ref m_FireAnim );
			weaponAwakeSuccess &= m_Animator.GetClipFromAnimator( "reload",		ref m_ReloadAnim );
			weaponAwakeSuccess &= m_Animator.GetClipFromAnimator( "draw",		ref m_DrawAnim );
//			Debug.Log( "Animations for weapon " + m_WpnBaseSectionName + " are " + ( ( weaponAwakeSuccess ) ? "correctly loaded" : "invalid!!!" ) );
		}


		// ATTACHMENTS
		UpdateAttachments();


		// Registering game events
		GameManager.StreamEvents.OnSave += OnSave;
		GameManager.StreamEvents.OnLoad += OnLoad;

		weaponAwakeSuccess &= ReloadBaseConfiguration();

		// Only if the construction complete successflly, the weapon get registered
		if ( weaponAwakeSuccess )
		{
			WeaponManager.Instance.RegisterWeapon( this );
		}
		m_StopWatch.Stop();
//		print( "Weapon: " + m_WpnBaseSectionName + " loaded in " + m_StopWatch.Elapsed.Milliseconds + "ms" );
	}


	//////////////////////////////////////////////////////////////////////////
	private		bool						ReloadBaseConfiguration()
	{
		bool result = true;
		m_PrimaryWeaponModuleSlot.TrySetModule		( this,	typeof( WPN_BaseModuleEmpty ) );
		m_SecondaryWeaponModuleSlot.TrySetModule	( this,	typeof( WPN_BaseModuleEmpty ) );
		m_TertiaryWeaponModuleSlot.TrySetModule		( this,	typeof( WPN_BaseModuleEmpty ) );

		m_Modifiers.Clear();

		// LOAD BASE CONFIGURATION
		if ( result &= GlobalManager.Configs.bGetSection( m_WpnBaseSectionName, ref m_WpnSection ) )
		{
			m_WpnSection.bAsVec3( "ZoomOffset", ref m_ZoomOffset, null );

			m_BaseZoomFactor		= m_WpnSection.AsFloat( "BaseZoomFactor",		m_BaseZoomFactor );
			m_BaseZoomingTime		= m_WpnSection.AsFloat( "BaseZoomingTime",		m_BaseZoomingTime );
			m_BaseZoomSensitivity	= m_WpnSection.AsFloat( "BaseZoomSensitivity",	m_BaseZoomSensitivity );

			// Primary Weapon Module
			result &= LoadAndConfigureModule( this, m_WpnSection, ref m_PrimaryWeaponModuleSlot );

			// Secondary Weapon Module
			result &= LoadAndConfigureModule( this, m_WpnSection, ref m_SecondaryWeaponModuleSlot );

			//Tertiary Weapon Module
			result &= LoadAndConfigureModule( this, m_WpnSection, ref m_TertiaryWeaponModuleSlot );
		}
		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	public		bool						bGetModuleBySlot( WeaponSlots slot, ref WPN_BaseModule weaponModule )
	{
		weaponModule = null;
		switch ( slot )
		{
			case WeaponSlots.PRIMARY:	weaponModule = m_PrimaryWeaponModuleSlot.WeaponModule;		break;
			case WeaponSlots.SECONDARY:	weaponModule = m_SecondaryWeaponModuleSlot.WeaponModule;	break;
			case WeaponSlots.TERTIARY:	weaponModule = m_TertiaryWeaponModuleSlot.WeaponModule;		break;
			default:	break;
		}

		return weaponModule != null;
	}


	//////////////////////////////////////////////////////////////////////////
	public		bool						bGetModuleSlot( WeaponSlots slot, ref WeaponModuleSlot moduleSlot )
	{
		moduleSlot = null;
		switch ( slot )
		{
			case WeaponSlots.PRIMARY:	moduleSlot = m_PrimaryWeaponModuleSlot;		break;
			case WeaponSlots.SECONDARY:	moduleSlot = m_SecondaryWeaponModuleSlot;		break;
			case WeaponSlots.TERTIARY:	moduleSlot = m_TertiaryWeaponModuleSlot;		break;
			default:	break;
		}

		return moduleSlot != null;
	}


	//////////////////////////////////////////////////////////////////////////
	public static	string					GetModuleSlotName( WeaponSlots slot )
	{
		string result = "";
		switch ( slot )
		{
			case WeaponSlots.PRIMARY: result = "PrimaryWeaponModule";
				break;
			case WeaponSlots.SECONDARY: result = "SecondaryWeaponModule";
				break;
			case WeaponSlots.TERTIARY: result = "TertiaryWeaponModule";
				break;
			default:
				break;
		}
		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	public		void						ApplyModifier( Database.Section modifier )
	{
		float MultZoomFactor			= modifier.AsFloat( "MultZoomFactor",					1.0f );
		float MultZoomingTime			= modifier.AsFloat( "MultZoomingTime",					1.0f );
		float MultZoomSensitivity		= modifier.AsFloat( "MultZoomSensitivity",				1.0f );

		m_BaseZoomFactor				*= MultZoomFactor;
		m_BaseZoomingTime				*= MultZoomingTime;
		m_BaseZoomSensitivity			*= MultZoomSensitivity;

		// Primary Weapon Module
		LoadAndConfigureModule( this, modifier, ref m_PrimaryWeaponModuleSlot );

		// Secondary Weapon Module
		LoadAndConfigureModule( this, modifier, ref m_SecondaryWeaponModuleSlot );

		// Tertiary Weapon Module
		LoadAndConfigureModule( this, modifier, ref m_TertiaryWeaponModuleSlot );
	}


	//////////////////////////////////////////////////////////////////////////
	public		void						ResetBaseConfiguration()
	{
		// Reload Base Configuration
		ReloadBaseConfiguration();
	}


	//////////////////////////////////////////////////////////////////////////
	public		void						RemoveModifier( Database.Section modifier )
	{
		if ( m_Modifiers.Contains( modifier ) )
		{
			m_Modifiers.Remove( modifier );
		}

		ResetBaseConfiguration();

		foreach( Database.Section otherModifier in m_Modifiers )
		{
			ApplyModifier( otherModifier );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private		float						GetZoomSensitivity()
	{
		float zoomSensitivity		= m_BaseZoomSensitivity;
		
		WPN_WeaponModule_Zoom zoomModule = null;
		if ( transform.SearchComponent( ref zoomModule, SearchContext.CHILDREN ) )
		{
			zoomSensitivity *= zoomModule.ZoomSensitivity;
		}
		
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
//		GameManager.UpdateEvents.OnFrame += m_PrimaryWeaponModuleSlot.WeaponModule.InternalUpdate;
//		GameManager.UpdateEvents.OnFrame += m_SecondaryWeaponModuleSlot.WeaponModule.InternalUpdate;
//		GameManager.UpdateEvents.OnFrame += m_TertiaryWeaponModuleSlot.WeaponModule.InternalUpdate;

		//										COMMAND								COMMAND ID						ACTION							PREDICATE
		GlobalManager.Instance.InputMgr.BindCall( eInputCommands.PRIMARY_FIRE_PRESS,		"Wpn_Primary_Fire_Start",		PrimaryFire_Start,		Predicate_PrimaryFire_Start		);
		GlobalManager.Instance.InputMgr.BindCall( eInputCommands.PRIMARY_FIRE_HOLD,		"Wpn_Primary_Fire_Update",		PrimaryFire_Update,		Predicate_PrimaryFire_Update	);
		GlobalManager.Instance.InputMgr.BindCall( eInputCommands.PRIMARY_FIRE_RELEASE,		"Wpn_Primary_Fire_End",			PrimaryFire_End,		Predicate_PrimaryFire_End		);

		GlobalManager.Instance.InputMgr.BindCall( eInputCommands.SECONDARY_FIRE_PRESS,		"Wpn_Secondary_Fire_Start",		SecondaryFire_Start,	Predicate_SecondaryFire_Start	);
		GlobalManager.Instance.InputMgr.BindCall( eInputCommands.SECONDARY_FIRE_HOLD,		"Wpn_Secondary_Fire_Update",	SecondaryFire_Update,	Predicate_SecondaryFire_Update	);
		GlobalManager.Instance.InputMgr.BindCall( eInputCommands.SECONDARY_FIRE_RELEASE,	"Wpn_Secondary_Fire_End",		SecondaryFire_End,		Predicate_SecondaryFire_End		);

		GlobalManager.Instance.InputMgr.BindCall( eInputCommands.TERTIARY_FIRE_PRESS,		"Wpn_Tertiary_Fire_Start",		TertiaryFire_Start,		Predicate_TertiaryFire_Start	);
		GlobalManager.Instance.InputMgr.BindCall( eInputCommands.TERTIARY_FIRE_HOLD,		"Wpn_Tertiary_Fire_Update",		TertiaryFire_Update,	Predicate_TertiaryFire_Update	);
		GlobalManager.Instance.InputMgr.BindCall( eInputCommands.TERTIARY_FIRE_RELEASE,	"Wpn_Tertiary_Fire_End",		TertiaryFire_End,		Predicate_TertiaryFire_End		);

		GlobalManager.Instance.InputMgr.BindCall( eInputCommands.RELOAD_WPN,				"Wpn_Reload",					Reload,					Predicate_Reload );

		GlobalManager.Instance.InputMgr.BindCall( eInputCommands.STATE_RUN,				"Wpn_ExitZoom",
			() => { WeaponManager.Instance.ZoomOut(); },
			delegate() { return  Player.Instance.IsRunning && WeaponManager.Instance.IsZoomed; }
		);
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			OnDisable()
	{
		// Update events un-registration
//		GameManager.UpdateEvents.OnFrame -= m_PrimaryWeaponModuleSlot.WeaponModule.InternalUpdate;
//		GameManager.UpdateEvents.OnFrame -= m_SecondaryWeaponModuleSlot.WeaponModule.InternalUpdate;
//		GameManager.UpdateEvents.OnFrame -= m_TertiaryWeaponModuleSlot.WeaponModule.InternalUpdate;

		GlobalManager.Instance.InputMgr.UnbindCall( eInputCommands.PRIMARY_FIRE_PRESS,		"Wpn_Primary_Fire_Start"	);
		GlobalManager.Instance.InputMgr.UnbindCall( eInputCommands.PRIMARY_FIRE_HOLD,		"Wpn_Primary_Fire_Update"	);
		GlobalManager.Instance.InputMgr.UnbindCall( eInputCommands.PRIMARY_FIRE_RELEASE,	"Wpn_Primary_Fire_End"		);

		GlobalManager.Instance.InputMgr.UnbindCall( eInputCommands.SECONDARY_FIRE_PRESS,	"Wpn_Secondary_Fire_Start"	);
		GlobalManager.Instance.InputMgr.UnbindCall( eInputCommands.SECONDARY_FIRE_HOLD,	"Wpn_Secondary_Fire_Update"	);
		GlobalManager.Instance.InputMgr.UnbindCall( eInputCommands.SECONDARY_FIRE_RELEASE,	"Wpn_Secondary_Fire_End"	);

		GlobalManager.Instance.InputMgr.UnbindCall( eInputCommands.TERTIARY_FIRE_PRESS,	"Wpn_Tertiary_Fire_Start"	);
		GlobalManager.Instance.InputMgr.UnbindCall( eInputCommands.TERTIARY_FIRE_HOLD,		"Wpn_Tertiary_Fire_Update"	);
		GlobalManager.Instance.InputMgr.UnbindCall( eInputCommands.TERTIARY_FIRE_RELEASE,	"Wpn_Tertiary_Fire_End"		);

		GlobalManager.Instance.InputMgr.UnbindCall( eInputCommands.RELOAD_WPN,				"Wpn_Reload" );

		GlobalManager.Instance.InputMgr.UnbindCall( eInputCommands.STATE_RUN,				"Wpn_ExitZoom" );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			Reload()
	{
		CoroutinesManager.Start( ReloadCO( OnEndReload ), "Weapon::Reload: Reloading co" );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		StreamUnit		OnSave( StreamData streamData )
	{
		StreamUnit streamUnit	= streamData.NewUnit( gameObject );
		
		streamUnit.SetInternal( "PrimaryModule", m_PrimaryWeaponModuleSlot.WeaponModule.ModuleSection.GetName() );

		m_PrimaryWeaponModuleSlot.WeaponModule.OnSave( streamUnit );
		m_SecondaryWeaponModuleSlot.WeaponModule.OnSave( streamUnit );
		m_TertiaryWeaponModuleSlot.WeaponModule.OnSave( streamUnit );

		// Flashlight
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
			m_PrimaryWeaponModuleSlot.WeaponModule.OnLoad( streamUnit );
			m_SecondaryWeaponModuleSlot.WeaponModule.OnLoad( streamUnit );
			m_TertiaryWeaponModuleSlot.WeaponModule.OnLoad( streamUnit );

			// Flashlight
			if ( m_FlashLight != null )
			{
				m_FlashLight.SetActive( streamUnit.GetAsBool( "FlashLightActive") );
			}

			// Load Weapon Modules Data

			UIManager.InGame.UpdateUI();
		}
		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Return if the current weapon allow the change at this time </summary>
	public		virtual		bool			CanChangeWeapon()
	{
		bool result = m_IsLocked == false;
		result &= m_PrimaryWeaponModuleSlot.WeaponModule.CanChangeWeapon();
		result &= m_SecondaryWeaponModuleSlot.WeaponModule.CanChangeWeapon();
		result &= m_TertiaryWeaponModuleSlot.WeaponModule.CanChangeWeapon();
		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Called before weapon change </summary>
	public		virtual		void			OnWeaponChange()
	{
		m_PrimaryWeaponModuleSlot.WeaponModule.OnWeaponChange();
		m_SecondaryWeaponModuleSlot.WeaponModule.OnWeaponChange();
		m_TertiaryWeaponModuleSlot.WeaponModule.OnWeaponChange();

		enabled			= false;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Callback after the reload animation </summary>
	protected	virtual		void			OnEndReload()
	{
		m_PrimaryWeaponModuleSlot.WeaponModule.OnAfterReload();
		m_SecondaryWeaponModuleSlot.WeaponModule.OnAfterReload();
		m_TertiaryWeaponModuleSlot.WeaponModule.OnAfterReload();

		// Update UI
		UIManager.InGame.UpdateUI();
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


	private List<Renderer> toBeShow = new List<Renderer>();
	//////////////////////////////////////////////////////////////////////////
	public		void						Hide							()
	{
		toBeShow.Clear();
		Renderer[] toBeHidden = null;
		transform.SearchComponents( ref toBeHidden, SearchContext.CHILDREN, (r) => r.enabled == true );

		toBeShow.AddRange( toBeHidden );

		foreach( Renderer r in GetComponentsInChildren<Renderer>() )
		{
			r.enabled = false;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public		void						Show							()
	{
		toBeShow.ForEach( ( r ) => r.enabled = true );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			Update() { }


	//////////////////////////////////////////////////////////////////////////
	protected				IEnumerator		ReloadCO( System.Action onReloadEnd )
	{
		m_IsLocked = true;

		m_PrimaryWeaponModuleSlot.WeaponModule.enabled = false;
		m_SecondaryWeaponModuleSlot.WeaponModule.enabled = false;
		m_TertiaryWeaponModuleSlot.WeaponModule.enabled = false;

		if ( WeaponManager.Instance.IsZoomed )
		{
			yield return WeaponManager.Instance.ZoomOut();
		}

		// Reload animation
		{
			m_Animator.Play( m_ReloadAnim.name, -1, 0f );
			float rechargeTimer = m_ReloadAnim.length * m_Animator.speed; // / 2f;
			yield return new WaitForSeconds( rechargeTimer );
		}

		m_PrimaryWeaponModuleSlot.WeaponModule.enabled = true;
		m_SecondaryWeaponModuleSlot.WeaponModule.enabled = true;
		m_TertiaryWeaponModuleSlot.WeaponModule.enabled = true;

		m_IsLocked = false;

		onReloadEnd();
	}

	
	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			OnDestroy()
	{
		m_PrimaryWeaponModuleSlot.WeaponModule.enabled = false;
		m_SecondaryWeaponModuleSlot.WeaponModule.enabled = false;
		m_TertiaryWeaponModuleSlot.WeaponModule.enabled = false;

		if ( GameManager.StreamEvents.IsNotNull() )
		{
			GameManager.StreamEvents.OnSave -= OnSave;
			GameManager.StreamEvents.OnLoad -= OnLoad;
		}
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