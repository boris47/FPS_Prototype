
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
	IWeaponAttachment		Flashlight						{ get; }
	bool					HasFlashlight					{ get; }
	IWeaponAttachment		Laser							{ get; }
	bool					HasLaser						{ get; }
	IWeaponAttachment		GranadeLauncher					{ get; }
	bool					HasGranadeLauncher				{ get; }

}

public interface IWeapon :  IAttachments, IWeaponZoom, IModifiable {
	
	Transform				Transform						{ get; }
	bool					Enabled							{ get; set; }
	EWeaponState				WeaponState						{ get; }

	bool					bGetModuleBySlot				( EWeaponSlots slot, ref WPN_BaseModule weaponModule );
	bool					bGetModuleSlot					( EWeaponSlots slot, ref WeaponModuleSlot moduleSlot );	

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
	protected		bool									m_HasFlashlight			= false;
	protected		ILaser									m_Laser						= null;
	protected		bool									m_HasLaser					= false;
	protected		IGranadeLauncher						m_GranadeLauncher			= null;
	protected		bool									m_HasGranadeLauncher		= false;


	// WEAPON STATE
	protected		EWeaponState							m_WeaponState				= EWeaponState.STASHED;
	protected		EWeaponSubState							m_WeaponSubState			= EWeaponSubState.IDLE;

	// INTERNALS
	protected		Vector3									m_StartOffset				= Vector3.zero;
	protected		float									m_BaseZoomFactor			= 1.0f;
	protected		float									m_BaseZoomingTime			= 1.0f;
	protected		float									m_BaseZoomSensitivity		= 1.0f;
	protected		List<Database.Section>					m_Modifiers					= new List<Database.Section>();

	// INTERFACE START
					IWeaponAttachment						IAttachments.Flashlight			{ get { return this.m_FlashLight; } }
					bool									IAttachments.HasFlashlight		{ get { return this.m_HasFlashlight; } }
					IWeaponAttachment						IAttachments.Laser				{ get { return this.m_Laser; } }
					bool									IAttachments.HasLaser			{ get { return this.m_HasLaser; } }
					IWeaponAttachment						IAttachments.GranadeLauncher	{ get { return this.m_GranadeLauncher; } }
					bool									IAttachments.HasGranadeLauncher	{ get { return this.m_HasGranadeLauncher; } }



					Transform								IWeapon.Transform			{ get { return this.transform; } }
					bool									IWeapon.Enabled				{ get { return this.enabled; } set { this.enabled = value; } }
					EWeaponState							IWeapon.WeaponState			{ get { return this.m_WeaponState; } }
					Database.Section						IWeapon.Section				{ get { return this.m_WpnSection; } }
					string									IWeapon.OtherInfo			{ get { return this.OtherInfo; } }
	
					Vector3									IWeaponZoom.ZoomOffset		{ get { return this.m_ZoomOffset; } }
	
					float									IWeaponZoom.ZoomingTime		{ get { return this.m_BaseZoomingTime; } }
					float									IWeaponZoom.ZoomSensitivity	{ get { return this.GetZoomSensitivity(); } }
					float									IWeaponZoom.ZoomFactor		{ get { return this.m_BaseZoomFactor; } }
	
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
			return this.m_PrimaryWeaponModuleSlot.WeaponModule.NeedReload() ||
				this.m_SecondaryWeaponModuleSlot.WeaponModule.NeedReload() ||
				this.m_TertiaryWeaponModuleSlot.WeaponModule.NeedReload();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	string				OtherInfo
	{
		get {
			string primaryModule	= this.m_PrimaryWeaponModuleSlot.WeaponModule.ModuleSection ? this.m_PrimaryWeaponModuleSlot.WeaponModule.ModuleSection.GetName() : "None";
			string secondaryModule	= this.m_SecondaryWeaponModuleSlot.WeaponModule.ModuleSection ? this.m_SecondaryWeaponModuleSlot.WeaponModule.ModuleSection.GetName() : "None";
			string tertiaryModule	= this.m_TertiaryWeaponModuleSlot.WeaponModule.ModuleSection ? this.m_TertiaryWeaponModuleSlot.WeaponModule.ModuleSection.GetName() : "None";
			return primaryModule + "," + secondaryModule + "," + tertiaryModule;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	void	UpdateAttachments()
	{
		// Flashlight
		if(this.m_HasFlashlight = Utils.Base.SearchComponent(this.gameObject, ref this.m_FlashLight, ESearchContext.CHILDREN ) )
		{
			this.m_FlashLight.OnAttached();
		}

		// Laser
		if (this.m_HasLaser = Utils.Base.SearchComponent(this.gameObject, ref this.m_Laser, ESearchContext.CHILDREN ) )
		{
			this.m_Laser.OnAttached();
		}

		// Granade Launcher
		if (this.m_HasGranadeLauncher = Utils.Base.SearchComponent(this.gameObject, ref this.m_GranadeLauncher, ESearchContext.CHILDREN ))
		{
			this.m_GranadeLauncher.OnAttached();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void				Awake()
	{
		System.Diagnostics.Stopwatch m_StopWatch = new System.Diagnostics.Stopwatch();
		m_StopWatch.Start();
		this.m_WpnBaseSectionName = this.GetType().FullName;

		bool bIsInitilalizedSuccessfully = true;

		// Animations
		{
			bIsInitilalizedSuccessfully &= Utils.Base.SearchComponent<Animator>(this.gameObject, ref this.m_Animator, ESearchContext.LOCAL );
//			bIsInitilalizedSuccessfully &= m_Animator.GetClipFromAnimator( "fire",		ref m_FireAnim );
			bIsInitilalizedSuccessfully &= this.m_Animator.GetClipFromAnimator( "reload",	ref this.m_ReloadAnim );
			bIsInitilalizedSuccessfully &= this.m_Animator.GetClipFromAnimator( "draw",		ref this.m_DrawAnim );
		}

		// ATTACHMENTS
		this.UpdateAttachments();

		// Registering game events
		GameManager.StreamEvents.OnSave += this.OnSave;
		GameManager.StreamEvents.OnLoad += this.OnLoad;

		bIsInitilalizedSuccessfully &= this.ReloadBaseConfiguration();

		// Only if the construction complete successflly, the weapon get registered
		if ( bIsInitilalizedSuccessfully )
		{
			WeaponManager.Instance.RegisterWeapon( this );
		}
		m_StopWatch.Stop();
		print( "Weapon: " + this.m_WpnBaseSectionName + " loaded in " + m_StopWatch.Elapsed.Milliseconds + "ms" );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void				OnValidate()
	{
		this.m_BaseZoomingTime = Mathf.Max(this.m_BaseZoomingTime, 0.1f );
	}


	//////////////////////////////////////////////////////////////////////////
	private		bool						ReloadBaseConfiguration()
	{
		bool result = true;
		this.m_PrimaryWeaponModuleSlot.TrySetModule		( this,	typeof( WPN_BaseModuleEmpty ) );
		this.m_SecondaryWeaponModuleSlot.TrySetModule	( this,	typeof( WPN_BaseModuleEmpty ) );
		this.m_TertiaryWeaponModuleSlot.TrySetModule		( this,	typeof( WPN_BaseModuleEmpty ) );

		this.m_Modifiers.Clear();

		// LOAD BASE CONFIGURATION
		if ( result &= GlobalManager.Configs.GetSection(this.m_WpnBaseSectionName, ref this.m_WpnSection ) )
		{
			this.m_WpnSection.bAsVec3( "ZoomOffset", ref this.m_ZoomOffset, null );

			this.m_BaseZoomFactor		= this.m_WpnSection.AsFloat( "BaseZoomFactor", this.m_BaseZoomFactor );
			this.m_BaseZoomingTime		= this.m_WpnSection.AsFloat( "BaseZoomingTime", this.m_BaseZoomingTime );
			this.m_BaseZoomSensitivity	= this.m_WpnSection.AsFloat( "BaseZoomSensitivity", this.m_BaseZoomSensitivity );

			// Primary Weapon Module
			result &= LoadAndConfigureModule( this, this.m_WpnSection, ref this.m_PrimaryWeaponModuleSlot );

			// Secondary Weapon Module
			result &= LoadAndConfigureModule( this, this.m_WpnSection, ref this.m_SecondaryWeaponModuleSlot );

			//Tertiary Weapon Module
			result &= LoadAndConfigureModule( this, this.m_WpnSection, ref this.m_TertiaryWeaponModuleSlot );
		}
		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	public		bool						bGetModuleBySlot( EWeaponSlots slot, ref WPN_BaseModule weaponModule )
	{
		weaponModule = null;
		switch ( slot )
		{
			case EWeaponSlots.PRIMARY:	weaponModule = this.m_PrimaryWeaponModuleSlot.WeaponModule;		break;
			case EWeaponSlots.SECONDARY:	weaponModule = this.m_SecondaryWeaponModuleSlot.WeaponModule;	break;
			case EWeaponSlots.TERTIARY:	weaponModule = this.m_TertiaryWeaponModuleSlot.WeaponModule;		break;
			default:	break;
		}
	
		return weaponModule.IsNotNull();
	}


	//////////////////////////////////////////////////////////////////////////
	public		bool						bGetModuleSlot( EWeaponSlots slot, ref WeaponModuleSlot moduleSlot )
	{
		moduleSlot = null;
		switch ( slot )
		{
			case EWeaponSlots.PRIMARY:	moduleSlot = this.m_PrimaryWeaponModuleSlot;		break;
			case EWeaponSlots.SECONDARY:	moduleSlot = this.m_SecondaryWeaponModuleSlot;		break;
			case EWeaponSlots.TERTIARY:	moduleSlot = this.m_TertiaryWeaponModuleSlot;		break;
			default:	break;
		}

		return moduleSlot != null;
	}


	//////////////////////////////////////////////////////////////////////////
	public static	string					GetModuleSlotName( EWeaponSlots slot )
	{
		string result = "";
		switch ( slot )
		{
			case EWeaponSlots.PRIMARY: result = "PrimaryWeaponModule";
				break;
			case EWeaponSlots.SECONDARY: result = "SecondaryWeaponModule";
				break;
			case EWeaponSlots.TERTIARY: result = "TertiaryWeaponModule";
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

		this.m_BaseZoomFactor				*= MultZoomFactor;
		this.m_BaseZoomingTime				*= MultZoomingTime;
		this.m_BaseZoomSensitivity			*= MultZoomSensitivity;

		// Primary Weapon Module
		LoadAndConfigureModule( this, modifier, ref this.m_PrimaryWeaponModuleSlot );

		// Secondary Weapon Module
		LoadAndConfigureModule( this, modifier, ref this.m_SecondaryWeaponModuleSlot );

		// Tertiary Weapon Module
		LoadAndConfigureModule( this, modifier, ref this.m_TertiaryWeaponModuleSlot );
	}


	//////////////////////////////////////////////////////////////////////////
	public		void						ResetBaseConfiguration()
	{
		// Reload Base Configuration
		this.ReloadBaseConfiguration();
	}


	//////////////////////////////////////////////////////////////////////////
	public		void						RemoveModifier( Database.Section modifier )
	{
		if (this.m_Modifiers.Contains( modifier ) )
		{
			this.m_Modifiers.Remove( modifier );
		}

		this.ResetBaseConfiguration();

		foreach( Database.Section otherModifier in this.m_Modifiers )
		{
			this.ApplyModifier( otherModifier );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private		float						GetZoomSensitivity()
	{
		float zoomSensitivity		= this.m_BaseZoomSensitivity;
		
		WPN_WeaponModule_Zoom zoomModule = null;
		if (this.transform.SearchComponent( ref zoomModule, ESearchContext.CHILDREN ) )
		{
			zoomSensitivity *= zoomModule.ZoomSensitivity;
		}
		
		return zoomSensitivity;
	}



	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			OnEnable()
	{
		//										COMMAND								COMMAND ID						ACTION							PREDICATE
		GlobalManager.InputMgr.BindCall( EInputCommands.PRIMARY_FIRE_PRESS,		"Wpn_Primary_Fire_Start", this.PrimaryFire_Start, this.Predicate_PrimaryFire_Start		);
		GlobalManager.InputMgr.BindCall( EInputCommands.PRIMARY_FIRE_HOLD,		"Wpn_Primary_Fire_Update", this.PrimaryFire_Update, this.Predicate_PrimaryFire_Update	);
		GlobalManager.InputMgr.BindCall( EInputCommands.PRIMARY_FIRE_RELEASE,	"Wpn_Primary_Fire_End", this.PrimaryFire_End, this.Predicate_PrimaryFire_End		);

		GlobalManager.InputMgr.BindCall( EInputCommands.SECONDARY_FIRE_PRESS,	"Wpn_Secondary_Fire_Start", this.SecondaryFire_Start, this.Predicate_SecondaryFire_Start	);
		GlobalManager.InputMgr.BindCall( EInputCommands.SECONDARY_FIRE_HOLD,	"Wpn_Secondary_Fire_Update", this.SecondaryFire_Update, this.Predicate_SecondaryFire_Update	);
		GlobalManager.InputMgr.BindCall( EInputCommands.SECONDARY_FIRE_RELEASE,	"Wpn_Secondary_Fire_End", this.SecondaryFire_End, this.Predicate_SecondaryFire_End		);

		GlobalManager.InputMgr.BindCall( EInputCommands.TERTIARY_FIRE_PRESS,	"Wpn_Tertiary_Fire_Start", this.TertiaryFire_Start, this.Predicate_TertiaryFire_Start	);
		GlobalManager.InputMgr.BindCall( EInputCommands.TERTIARY_FIRE_HOLD,		"Wpn_Tertiary_Fire_Update", this.TertiaryFire_Update, this.Predicate_TertiaryFire_Update	);
		GlobalManager.InputMgr.BindCall( EInputCommands.TERTIARY_FIRE_RELEASE,	"Wpn_Tertiary_Fire_End", this.TertiaryFire_End, this.Predicate_TertiaryFire_End		);

		GlobalManager.InputMgr.BindCall( EInputCommands.RELOAD_WPN,				"Wpn_Reload", this.Reload, this.Predicate_Reload );

		GlobalManager.InputMgr.BindCall( EInputCommands.STATE_RUN,				"Wpn_ExitZoom",
			() => { WeaponManager.Instance.ZoomOut(); },
			delegate() { return  Player.Instance.IsRunning && WeaponManager.Instance.IsZoomed; }
		);

		if (this.m_HasLaser ) this.m_Laser.SetActive(true);
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			OnDisable()
	{
		GlobalManager.InputMgr.UnbindCall( EInputCommands.PRIMARY_FIRE_PRESS,		"Wpn_Primary_Fire_Start"	);
		GlobalManager.InputMgr.UnbindCall( EInputCommands.PRIMARY_FIRE_HOLD,		"Wpn_Primary_Fire_Update"	);
		GlobalManager.InputMgr.UnbindCall( EInputCommands.PRIMARY_FIRE_RELEASE,		"Wpn_Primary_Fire_End"		);

		GlobalManager.InputMgr.UnbindCall( EInputCommands.SECONDARY_FIRE_PRESS,		"Wpn_Secondary_Fire_Start"	);
		GlobalManager.InputMgr.UnbindCall( EInputCommands.SECONDARY_FIRE_HOLD,		"Wpn_Secondary_Fire_Update"	);
		GlobalManager.InputMgr.UnbindCall( EInputCommands.SECONDARY_FIRE_RELEASE,	"Wpn_Secondary_Fire_End"	);

		GlobalManager.InputMgr.UnbindCall( EInputCommands.TERTIARY_FIRE_PRESS,		"Wpn_Tertiary_Fire_Start"	);
		GlobalManager.InputMgr.UnbindCall( EInputCommands.TERTIARY_FIRE_HOLD,		"Wpn_Tertiary_Fire_Update"	);
		GlobalManager.InputMgr.UnbindCall( EInputCommands.TERTIARY_FIRE_RELEASE,	"Wpn_Tertiary_Fire_End"		);

		GlobalManager.InputMgr.UnbindCall( EInputCommands.RELOAD_WPN,				"Wpn_Reload" );

		GlobalManager.InputMgr.UnbindCall( EInputCommands.STATE_RUN,				"Wpn_ExitZoom" );

		if (this.m_HasLaser ) this.m_Laser.SetActive(false);
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			Reload()
	{
		CoroutinesManager.Start(this.ReloadCO(this.OnEndReload ), "Weapon::Reload: Reloading co" );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		StreamUnit		OnSave( StreamData streamData )
	{
		StreamUnit streamUnit	= streamData.NewUnit(this.gameObject );
		
		streamUnit.SetInternal( "PrimaryModule", this.m_PrimaryWeaponModuleSlot.WeaponModule.ModuleSection.GetName() );

		this.m_PrimaryWeaponModuleSlot.WeaponModule.OnSave( streamUnit );
		this.m_SecondaryWeaponModuleSlot.WeaponModule.OnSave( streamUnit );
		this.m_TertiaryWeaponModuleSlot.WeaponModule.OnSave( streamUnit );

		// Flashlight
		if (this.m_FlashLight != null )
		{
			streamUnit.SetInternal( "FlashLightActive", this.m_FlashLight.IsActive );
		}

		// Save Weapon Modules Data

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		StreamUnit		OnLoad( StreamData streamData )
	{
		this.m_Animator.Play( "draw", -1, 0.99f );

		StreamUnit streamUnit = null;
		if ( streamData.GetUnit(this.gameObject, ref streamUnit ) )
		{
			this.m_PrimaryWeaponModuleSlot.WeaponModule.OnLoad( streamUnit );
			this.m_SecondaryWeaponModuleSlot.WeaponModule.OnLoad( streamUnit );
			this.m_TertiaryWeaponModuleSlot.WeaponModule.OnLoad( streamUnit );

			// Flashlight
			if (this.m_FlashLight != null )
			{
				this.m_FlashLight.SetActive( streamUnit.GetAsBool( "FlashLightActive") );
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
		bool result = this.m_IsLocked == false;
		result &= this.m_PrimaryWeaponModuleSlot.WeaponModule.CanChangeWeapon();
		result &= this.m_SecondaryWeaponModuleSlot.WeaponModule.CanChangeWeapon();
		result &= this.m_TertiaryWeaponModuleSlot.WeaponModule.CanChangeWeapon();
		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Called before weapon change </summary>
	public		virtual		void			OnWeaponChange()
	{
		this.m_PrimaryWeaponModuleSlot.WeaponModule.OnWeaponChange();
		this.m_SecondaryWeaponModuleSlot.WeaponModule.OnWeaponChange();
		this.m_TertiaryWeaponModuleSlot.WeaponModule.OnWeaponChange();

		this.enabled			= false;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Callback after the reload animation </summary>
	protected	virtual		void			OnEndReload()
	{
		this.m_PrimaryWeaponModuleSlot.WeaponModule.OnAfterReload();
		this.m_SecondaryWeaponModuleSlot.WeaponModule.OnAfterReload();
		this.m_TertiaryWeaponModuleSlot.WeaponModule.OnAfterReload();

		// Update UI
		UIManager.InGame.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Start the draw animation, return the seconds to wait </summary>
	public		virtual		float			Draw()
	{
		this.m_Animator.Play( "draw", -1, 0f );
		this.m_WeaponState	= EWeaponState.DRAWED;

		this.m_IsLocked = true;
		TimersManager.Instance?.AddTimerScaled(this.m_DrawAnim.length, () => this.m_IsLocked = false );

		return this.m_DrawAnim.length;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Start the stash animation, return the seconds to wait </summary>
	public		virtual		float			Stash()
	{
		this.m_Animator.Play( "stash", -1, 0f );
		this.m_WeaponState	= EWeaponState.STASHED;

		this.m_IsLocked = true;
		TimersManager.Instance?.AddTimerScaled(this.m_DrawAnim.length, () => this.m_IsLocked = false );

		return this.m_DrawAnim.length;
	}


	private List<Renderer> toBeShow = new List<Renderer>();
	//////////////////////////////////////////////////////////////////////////
	public		void						Hide							()
	{
		this.toBeShow.Clear();
		Renderer[] toBeHidden = null;
		this.transform.SearchComponents( ref toBeHidden, ESearchContext.CHILDREN, (r) => r.enabled == true );

		this.toBeShow.AddRange( toBeHidden );

		foreach( Renderer r in this.GetComponentsInChildren<Renderer>() )
		{
			r.enabled = false;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public		void						Show							()
	{
		this.toBeShow.ForEach( ( r ) => r.enabled = true );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			Update() { }


	//////////////////////////////////////////////////////////////////////////
	protected				IEnumerator		ReloadCO( System.Action onReloadEnd )
	{
		this.m_IsLocked = true;

		this.m_PrimaryWeaponModuleSlot.WeaponModule.enabled = false;
		this.m_SecondaryWeaponModuleSlot.WeaponModule.enabled = false;
		this.m_TertiaryWeaponModuleSlot.WeaponModule.enabled = false;

		if ( WeaponManager.Instance.IsZoomed )
		{
			yield return WeaponManager.Instance.ZoomOut();
		}

		// Reload animation
		{
			this.m_Animator.Play(this.m_ReloadAnim.name, -1, 0f );
			float rechargeTimer = this.m_ReloadAnim.length * this.m_Animator.speed; // / 2f;
			yield return new WaitForSeconds( rechargeTimer );
		}

		this.m_PrimaryWeaponModuleSlot.WeaponModule.enabled = true;
		this.m_SecondaryWeaponModuleSlot.WeaponModule.enabled = true;
		this.m_TertiaryWeaponModuleSlot.WeaponModule.enabled = true;

		this.m_IsLocked = false;

		onReloadEnd();
	}

	
	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			OnDestroy()
	{
		this.m_PrimaryWeaponModuleSlot.WeaponModule.enabled = false;
		this.m_SecondaryWeaponModuleSlot.WeaponModule.enabled = false;
		this.m_TertiaryWeaponModuleSlot.WeaponModule.enabled = false;

		if ( GameManager.StreamEvents.IsNotNull() )
		{
			GameManager.StreamEvents.OnSave -= this.OnSave;
			GameManager.StreamEvents.OnLoad -= this.OnLoad;
		}
	}
	
}


/////////////////////////////////////////
/////////////////////////////////////////
public enum EWeaponState
{
	DRAWED, STASHED
}

public enum EWeaponSubState
{
	IDLE, RELOADING, FIRING, TRANSITION
}

/////////////////////////////////////////
/////////////////////////////////////////

public enum EFireMode
{
	SINGLE, BURST, AUTO, NONE
}