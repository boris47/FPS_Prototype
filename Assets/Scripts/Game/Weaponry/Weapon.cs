
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EWeaponState
{
	DRAWED, STASHED
}

public enum EWeaponSubState
{
	IDLE, RELOADING, FIRING, TRANSITION
}


public enum EFireMode
{
	SINGLE, BURST, AUTO, NONE
}

/////////////////////////////////////////

public interface IWeaponZoom
{
	Vector3					ZoomOffset			{ get; }
	float					ZoomingTime			{ get; }
	float					ZoomSensitivity		{ get; }
	float					ZoomFactor			{ get; }
}

public interface IModifiable
{
	void					ApplyModifier			( Database.Section modifier );
	void					ResetBaseConfiguration	();
	void					RemoveModifier			( Database.Section modifier );
}

public interface IWeapon : IWeaponZoom, IModifiable
{	
	Transform				Transform						{ get; }
	bool					Enabled							{ get; set; }
	EWeaponState			WeaponState						{ get; }
	EWeaponSubState			WeaponSubState					{ get; }
	IAttachments			Attachments						{ get; }

	bool					TryGetModuleBySlot				( EWeaponSlots slot, out WPN_BaseModule weaponModule );
	bool					TryGetModuleSlot				( EWeaponSlots slot, out WeaponModuleSlot moduleSlot );	

	Database.Section		Section							{ get; }
	string					OtherInfo						{ get; }

	void					ApplyDeviation					( float deviation, float weightX = 1f, float weightY = 1f );
	void					ApplyDispersion					( float dispersion, float weightX = 1f, float weightY = 1f );
	void					ApplyFallFeedback				( float delta, float weightX = 1.0f, float weightY = 1.0f );
	void					AddRecoil						( float recoil );

	Vector3					Deviation						{ get; }
	Vector3					Dispersion						{ get; }
	Vector3					FallFeedback					{ get; }
	float					Recoil							{ get; }

	bool					CanChangeWeapon					();
	void					OnWeaponChange					();
	float					Draw							();
	float					Stash							();

	void					Hide							();
	void					Show							();
}



[System.Serializable]
public abstract partial class Weapon : MonoBehaviour, IWeapon
{
	public		const	float						RECOVERY_SPEED_MULT				= 4.0f;
	private		const	float						MAX_RECOIL						= 0.6f;

	[Header("Weapon Properties")]

	[SerializeField, ReadOnly]
	protected		Vector3						m_ZoomOffset					= Vector3.zero;

	// SECTION
	protected		Database.Section			m_WpnSection					= null;
	protected		string						m_WpnBaseSectionName			=> GetType().FullName;


	// WEAPON STATE
	[SerializeField]
	protected		EWeaponState				m_WeaponState					= EWeaponState.STASHED;
	[SerializeField]
	protected		EWeaponSubState				m_WeaponSubState				= EWeaponSubState.IDLE;

	// INTERNALS
	protected		Vector3						m_StartOffset					= Vector3.zero;
	protected		float						m_BaseZoomFactor				= 1.0f;
	protected		float						m_BaseZoomingTime				= 1.0f;
	protected		float						m_BaseZoomSensitivity			= 1.0f;
	protected		List<Database.Section>		m_Modifiers						= new List<Database.Section>();
	protected		Vector3						m_Deviation						= Vector3.zero;
	protected		Vector3						m_Dispersion					= Vector3.zero;
	protected		Vector3						m_FallFeedback					= Vector3.zero;
	protected		float						m_Recoil						= 0.0f;

	// INTERFACE START
					bool						IWeapon.Enabled					{ get => enabled; set => enabled = value; }

					Transform					IWeapon.Transform				=> transform;
					EWeaponState				IWeapon.WeaponState				=> m_WeaponState;
					EWeaponSubState				IWeapon.WeaponSubState			=> m_WeaponSubState;
					Database.Section			IWeapon.Section					=> m_WpnSection;
					string						IWeapon.OtherInfo				=> OtherInfo;

					Vector3						IWeapon.Deviation				=> m_Deviation;
					Vector3						IWeapon.Dispersion				=> m_Dispersion;
					Vector3						IWeapon.FallFeedback			=> m_FallFeedback;
					float						IWeapon.Recoil					=> m_Recoil;

					Vector3						IWeaponZoom.ZoomOffset			=> m_ZoomOffset;

					float						IWeaponZoom.ZoomingTime			=> m_BaseZoomingTime;
					float						IWeaponZoom.ZoomSensitivity		=> GetZoomSensitivity();
					float						IWeaponZoom.ZoomFactor			=> m_BaseZoomFactor;
	// INTERFACE END

	// UNITY COMPONENTS
	protected		Animator					m_Animator						= null;
	private			Renderer[]					m_WeaponRenderes				= null;

	// ANIMATIONS
//	protected		AnimationClip				m_FireAnim						= null;
	protected		AnimationClip				m_ReloadAnim					= null;
	protected		AnimationClip				m_DrawAnim						= null;

	// Weapon Flags
	protected		bool						m_IsLocked						= false;
	protected bool m_NeedRecharge =>
		m_PrimaryWeaponModuleSlot.WeaponModule.NeedReload() ||
		m_SecondaryWeaponModuleSlot.WeaponModule.NeedReload() ||
		m_TertiaryWeaponModuleSlot.WeaponModule.NeedReload();


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	string				OtherInfo
	{
		get
		{
			string primaryModule	= m_PrimaryWeaponModuleSlot.WeaponModule.ModuleSection.GetSectionName();
			string secondaryModule	= m_SecondaryWeaponModuleSlot.WeaponModule.ModuleSection.GetSectionName();
			string tertiaryModule	= m_TertiaryWeaponModuleSlot.WeaponModule.ModuleSection.GetSectionName();
			return $"{primaryModule},{secondaryModule},{tertiaryModule}";
		}
	}



	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			Awake()
	{
		System.Diagnostics.Stopwatch m_StopWatch = new System.Diagnostics.Stopwatch();
		m_StopWatch.Start();

		bool bIsInitilalizedSuccessfully = true;

		// Animations
		{
			bIsInitilalizedSuccessfully &= Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL, out m_Animator );
//			bIsInitilalizedSuccessfully &= this.m_Animator.GetClipFromAnimator( "fire",		ref m_FireAnim );
			bIsInitilalizedSuccessfully &= m_Animator.GetClipFromAnimator( "reload",	ref m_ReloadAnim );
			bIsInitilalizedSuccessfully &= m_Animator.GetClipFromAnimator( "draw",		ref m_DrawAnim );
		}

//		this.m_AreAttachmentsAllowed = this.transform.TrySearchComponentInChild( "Attachments", ref this.m_AttachmentRoot );

		bIsInitilalizedSuccessfully &= bIsInitilalizedSuccessfully && GlobalManager.Configs.TryGetSection( m_WpnBaseSectionName, out m_WpnSection );

		// ATTACHMENTS
		bIsInitilalizedSuccessfully &= bIsInitilalizedSuccessfully && InitializeAttachments();

		// Registering game events
		GameManager.StreamEvents.OnSave += OnSave;
		GameManager.StreamEvents.OnLoad += OnLoad;

		bIsInitilalizedSuccessfully &= bIsInitilalizedSuccessfully && ReloadBaseConfiguration();

		foreach(IWeaponAttachment attachment in transform.GetComponentsInChildren<IWeaponAttachment>())
		{
			attachment.OnAttach();
		}

		// Only if the construction complete successflly, the weapon get registered
		if ( bIsInitilalizedSuccessfully )
		{
			WeaponManager.Instance.RegisterWeapon( this );
		}
		m_StopWatch.Stop();
		print( "Weapon: " + m_WpnBaseSectionName + " loaded in " + m_StopWatch.Elapsed.Milliseconds + "ms" );
	}


	//////////////////////////////////////////////////////////////////////////
	private					bool			ReloadBaseConfiguration()
	{
		bool result = true;
		m_PrimaryWeaponModuleSlot.TrySetModule(this,	typeof( WPN_BaseModuleEmpty ) );
		m_SecondaryWeaponModuleSlot.TrySetModule(this,	typeof( WPN_BaseModuleEmpty ) );
		m_TertiaryWeaponModuleSlot.TrySetModule(this,	typeof( WPN_BaseModuleEmpty ) );

		m_Modifiers.Clear();

		// LOAD BASE CONFIGURATION
		{
			m_WpnSection.TryAsVec3( "ZoomOffset", out m_ZoomOffset, null );

			m_BaseZoomFactor		= m_WpnSection.AsFloat( "BaseZoomFactor", m_BaseZoomFactor );
			m_BaseZoomingTime		= m_WpnSection.AsFloat( "BaseZoomingTime", m_BaseZoomingTime );
			m_BaseZoomSensitivity	= m_WpnSection.AsFloat( "BaseZoomSensitivity", m_BaseZoomSensitivity );

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
	public					bool			TryGetModuleBySlot( EWeaponSlots slot, out WPN_BaseModule weaponModule )
	{
		weaponModule = null;
		switch ( slot )
		{
			case EWeaponSlots.PRIMARY:		weaponModule = m_PrimaryWeaponModuleSlot.WeaponModule;		break;
			case EWeaponSlots.SECONDARY:	weaponModule = m_SecondaryWeaponModuleSlot.WeaponModule;	break;
			case EWeaponSlots.TERTIARY:		weaponModule = m_TertiaryWeaponModuleSlot.WeaponModule;	break;
			default:	break;
		}
		return weaponModule.IsNotNull();
	}


	//////////////////////////////////////////////////////////////////////////
	public					bool			TryGetModuleSlot( EWeaponSlots slot, out WeaponModuleSlot moduleSlot )
	{
		moduleSlot = null;
		switch ( slot )
		{
			case EWeaponSlots.PRIMARY:		moduleSlot = m_PrimaryWeaponModuleSlot;		break;
			case EWeaponSlots.SECONDARY:	moduleSlot = m_SecondaryWeaponModuleSlot;		break;
			case EWeaponSlots.TERTIARY:		moduleSlot = m_TertiaryWeaponModuleSlot;		break;
			default:	break;
		}
		return moduleSlot.IsNotNull();
	}


	//////////////////////////////////////////////////////////////////////////
	public static			string			GetModuleSlotName( EWeaponSlots slot )
	{
		string result = "";
		switch ( slot )
		{
			case EWeaponSlots.PRIMARY:		result = "PrimaryWeaponModule";		break;
			case EWeaponSlots.SECONDARY:	result = "SecondaryWeaponModule";	break;
			case EWeaponSlots.TERTIARY:		result = "TertiaryWeaponModule";	break;
			default:	break;
		}
		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	public					void			ApplyModifier( Database.Section modifier )
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
	public					void			ResetBaseConfiguration()
	{
		// Reload Base Configuration
		ReloadBaseConfiguration();
	}


	//////////////////////////////////////////////////////////////////////////
	public					void			RemoveModifier( Database.Section modifier )
	{
		if (m_Modifiers.Contains( modifier ) )
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
	private					float			GetZoomSensitivity()
	{
		float zoomSensitivity		= m_BaseZoomSensitivity;

		if (Attachments.HasAttachment<WPN_WeaponAttachment_Zoom>())
		{
			WPN_WeaponAttachment_Zoom attachment = Attachments.GetAttachment<WPN_WeaponAttachment_Zoom>();
			zoomSensitivity = attachment.ZoomSensitivityMultiplier;
		}
		
		return zoomSensitivity;
	}
	

	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			OnEnable()
	{
		//										COMMAND								COMMAND ID						ACTION							PREDICATE
		GlobalManager.InputMgr.BindCall( EInputCommands.PRIMARY_FIRE_PRESS,		"Wpn_Primary_Fire_Start",		PrimaryFire_Start,		Predicate_PrimaryFire_Start	);
		GlobalManager.InputMgr.BindCall( EInputCommands.PRIMARY_FIRE_HOLD,		"Wpn_Primary_Fire_Update",		PrimaryFire_Update,	Predicate_PrimaryFire_Update	);
		GlobalManager.InputMgr.BindCall( EInputCommands.PRIMARY_FIRE_RELEASE,	"Wpn_Primary_Fire_End",			PrimaryFire_End,		Predicate_PrimaryFire_End		);

		GlobalManager.InputMgr.BindCall( EInputCommands.SECONDARY_FIRE_PRESS,	"Wpn_Secondary_Fire_Start",		SecondaryFire_Start,	Predicate_SecondaryFire_Start	);
		GlobalManager.InputMgr.BindCall( EInputCommands.SECONDARY_FIRE_HOLD,	"Wpn_Secondary_Fire_Update",	SecondaryFire_Update,	Predicate_SecondaryFire_Update	);
		GlobalManager.InputMgr.BindCall( EInputCommands.SECONDARY_FIRE_RELEASE,	"Wpn_Secondary_Fire_End",		SecondaryFire_End,		Predicate_SecondaryFire_End	);

		GlobalManager.InputMgr.BindCall( EInputCommands.TERTIARY_FIRE_PRESS,	"Wpn_Tertiary_Fire_Start",		TertiaryFire_Start,	Predicate_TertiaryFire_Start	);
		GlobalManager.InputMgr.BindCall( EInputCommands.TERTIARY_FIRE_HOLD,		"Wpn_Tertiary_Fire_Update",		TertiaryFire_Update,	Predicate_TertiaryFire_Update	);
		GlobalManager.InputMgr.BindCall( EInputCommands.TERTIARY_FIRE_RELEASE,	"Wpn_Tertiary_Fire_End",		TertiaryFire_End,		Predicate_TertiaryFire_End		);

		GlobalManager.InputMgr.BindCall( EInputCommands.RELOAD_WPN,				"Wpn_Reload",					Reload,				Predicate_Reload				);

		GlobalManager.InputMgr.BindCall( EInputCommands.STATE_RUN,				"Wpn_ExitZoom",
			() => { WeaponManager.Instance.ZoomOut(); },
			delegate() { return  Player.Instance.IsRunning && WeaponManager.Instance.IsZoomed; }
		);
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
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			Reload()
	{
		CoroutinesManager.Start( ReloadCO( OnEndReload ), "Weapon::Reload: Reloading co" );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		bool		OnSave( StreamData streamData, ref StreamUnit streamUnit )
	{
		streamUnit	= streamData.NewUnit(gameObject );
		
		streamUnit.SetInternal( "PrimaryModule", m_PrimaryWeaponModuleSlot.WeaponModule.ModuleSection.GetSectionName() );

		m_PrimaryWeaponModuleSlot.WeaponModule.OnSave( streamUnit );
		m_SecondaryWeaponModuleSlot.WeaponModule.OnSave( streamUnit );
		m_TertiaryWeaponModuleSlot.WeaponModule.OnSave( streamUnit );

		// Flashlight
//		if (this.m_FlashLight != null )
//		{
//			streamUnit.SetInternal( "FlashLightActive", this.m_FlashLight.IsActive );
//		}

		// Save Weapon Modules Data

		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		bool		OnLoad( StreamData streamData, ref StreamUnit streamUnit )
	{
		m_Animator.Play( "draw", -1, 0.99f );

		bool bResult = streamData.TryGetUnit(this, out streamUnit );
		if ( bResult )
		{
			m_PrimaryWeaponModuleSlot.WeaponModule.OnLoad( streamUnit );
			m_SecondaryWeaponModuleSlot.WeaponModule.OnLoad( streamUnit );
			m_TertiaryWeaponModuleSlot.WeaponModule.OnLoad( streamUnit );

			m_Deviation = Vector3.zero;
			m_Dispersion = Vector3.zero;

			// Flashlight
//			if (this.m_FlashLight != null )
//			{
//				this.m_FlashLight.SetActive( streamUnit.GetAsBool( "FlashLightActive") );
//			}

			// Load Weapon Modules Data

			UIManager.InGame.UpdateUI();
		}
		return bResult;
	}


	//////////////////////////////////////////////////////////////////////////
	public					void			ApplyDeviation( float deviation, float weightX = 1.0f, float weightY = 1.0f )
	{
		m_Deviation.x += Random.Range( -deviation, deviation ) * weightX;
		m_Deviation.y += Random.Range( -deviation, deviation ) * weightY;
	}


	//////////////////////////////////////////////////////////////////////////
	public					void			ApplyDispersion( float dispersion, float weightX = 1.0f, float weightY = 1.0f )
	{
		m_Dispersion.y += Random.Range( -dispersion, dispersion ) * weightX;	// Horizontal
		m_Dispersion.z += Random.Range( 0.0f, dispersion ) * weightY;			// Vertical
	}


	//////////////////////////////////////////////////////////////////////////
	public					void			AddRecoil( float recoil )
	{
		m_Recoil = Mathf.Min( m_Recoil + recoil, MAX_RECOIL );
	}

	//////////////////////////////////////////////////////////////////////////
	public void	ApplyFallFeedback( float delta, float weightX = 1.0f, float weightY = 1.0f )
	{
		m_FallFeedback.x = delta * weightX;
		m_FallFeedback.y = delta * weightY;
//		m_WpnFallFeedback = Vector3.ClampMagnitude( m_WpnCurrentDeviation, WPN_FALL_FEEDBACK_CLAMP_VALUE );
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
		m_WeaponState	= EWeaponState.DRAWED;
		m_WeaponSubState = EWeaponSubState.TRANSITION;

		m_IsLocked = true;
		TimersManager.Instance.AddTimerScaled(m_DrawAnim.length, () =>
		{
			m_WeaponSubState = EWeaponSubState.IDLE;
			m_IsLocked = false;
		} );

		return m_DrawAnim.length;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Start the stash animation, return the seconds to wait </summary>
	public		virtual		float			Stash()
	{
		m_Animator.Play( "stash", -1, 0f );
		m_WeaponState	= EWeaponState.STASHED;
		m_WeaponSubState = EWeaponSubState.IDLE;

		m_IsLocked = true;
		TimersManager.Instance.AddTimerScaled(m_DrawAnim.length, () =>
		{
			m_WeaponSubState = EWeaponSubState.IDLE;
			m_IsLocked = false;
		} );

		return m_DrawAnim.length;
	}
	
	
	//////////////////////////////////////////////////////////////////////////
	public					void			Hide()
	{
		if (transform.TrySearchComponents(ESearchContext.CHILDREN, out m_WeaponRenderes, (r) => r.enabled == true))
		{
			foreach ( Renderer r in m_WeaponRenderes )
			{
				r.enabled = false;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public					void			Show()
	{
		if (m_WeaponRenderes.IsNotNull())
		{
			System.Array.ForEach( m_WeaponRenderes, r => r.enabled = true );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public					void			OnCutsceneEnd()
	{
		m_Deviation = Vector3.zero;
		m_Dispersion = Vector3.zero;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			Update() { }


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			LateUpdate()
	{
		float deltaTime = Time.deltaTime;

		m_Dispersion	= Vector3.Lerp( m_Dispersion,	Vector3.zero, deltaTime * RECOVERY_SPEED_MULT );
		m_Deviation		= Vector3.Lerp( m_Deviation,	Vector3.zero, deltaTime * RECOVERY_SPEED_MULT );
		m_FallFeedback	= Vector3.Lerp( m_FallFeedback,	Vector3.zero, deltaTime * RECOVERY_SPEED_MULT );
		m_Recoil		= Mathf.Lerp( m_Recoil, 0.0f, deltaTime * RECOVERY_SPEED_MULT );
	}


	//////////////////////////////////////////////////////////////////////////
	protected				IEnumerator		ReloadCO( System.Action onReloadEnd )
	{
		m_IsLocked = true;

		m_PrimaryWeaponModuleSlot.WeaponModule.enabled = false;
		m_SecondaryWeaponModuleSlot.WeaponModule.enabled = false;
		m_TertiaryWeaponModuleSlot.WeaponModule.enabled = false;

		bool wasZoomed = false;
		if ( wasZoomed = WeaponManager.Instance.IsZoomed )
		{
			Attachments.DeactivateAttachment<WPN_WeaponAttachment_Zoom>();
			yield return new WaitWhile( () => WeaponManager.Instance.IsZoomed );
		}
		m_WeaponSubState = EWeaponSubState.RELOADING;

		// Reload animation
		{
			m_Animator.Play(m_ReloadAnim.name, -1, 0f );
			float rechargeTimer = m_ReloadAnim.length * m_Animator.speed; // / 2f;
			yield return new WaitForSeconds( rechargeTimer );
		}

		m_PrimaryWeaponModuleSlot.WeaponModule.enabled = true;
		m_SecondaryWeaponModuleSlot.WeaponModule.enabled = true;
		m_TertiaryWeaponModuleSlot.WeaponModule.enabled = true;

		m_WeaponSubState = EWeaponSubState.IDLE;
		m_IsLocked = false;

		if (wasZoomed)
		{
			Attachments.ActivateAttachment<WPN_WeaponAttachment_Zoom>();
		}

		onReloadEnd();
	}

	
	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			OnDestroy()
	{
		m_PrimaryWeaponModuleSlot.WeaponModule.enabled = false;
		m_SecondaryWeaponModuleSlot.WeaponModule.enabled = false;
		m_TertiaryWeaponModuleSlot.WeaponModule.enabled = false;

		foreach(IWeaponAttachment attachment in transform.GetComponentsInChildren<IWeaponAttachment>())
		{
			attachment.OnDetach();
		}

		if ( GameManager.StreamEvents.IsNotNull() )
		{
			GameManager.StreamEvents.OnSave -= OnSave;
			GameManager.StreamEvents.OnLoad -= OnLoad;
		}
	}
	
}
