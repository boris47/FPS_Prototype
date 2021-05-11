
using System.Collections;
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

public partial interface IWeapon : IWeaponZoom
{	
	Transform				Transform						{ get; }
	bool					Enabled							{ get; set; }
	EWeaponState			WeaponState						{ get; }
	EWeaponSubState			WeaponSubState					{ get; }
	WeaponPivot				WeaponPivot						{ get; }

	Entity					Owner							{ get; }
	Database.Section		Section							{ get; }
	string					OtherInfo						{ get; }
	void					SetOwner						( Entity entity );

	bool					CanChangeWeapon					();
	void					OnWeaponChange					();
	float					Draw							();
	float					Stash							();

	void					Hide							();
	void					Show							();
}



public abstract partial class WeaponBase : MonoBehaviour, IWeapon
{
	[System.Serializable]
	protected class WeaponData
	{
		public	Vector3		ZoomOffset					= Vector3.zero;
		public	float		BaseZoomFactor				= 0f;
		public	float		BaseZoomingTime				= 0f;
		public	float		BaseZoomSensitivity			= 1f;

		public void AssignFrom(WeaponData other)
		{
			ZoomOffset = other.ZoomOffset;
			BaseZoomFactor = other.BaseZoomFactor;
			BaseZoomingTime = other.BaseZoomingTime;
			BaseZoomSensitivity = other.BaseZoomSensitivity;
		}
	}

	[Header("Weapon Properties")]

	protected		WeaponData					m_WeaponData					= new WeaponData();
	


	// SECTION
	protected		Database.Section			m_WpnSection					= null;
	protected		string						m_WpnBaseSectionName			=> GetType().FullName;
	protected		Entity						m_Owner							= null;


	// WEAPON STATE
	[SerializeField, ReadOnly]
	protected		EWeaponState				m_WeaponState					= EWeaponState.STASHED;
	[SerializeField, ReadOnly]
	protected		EWeaponSubState				m_WeaponSubState				= EWeaponSubState.IDLE;

	[SerializeField, ReadOnly]
	private			WeaponPivot					m_WeaponPivot					= null;

	public			WeaponPivot					WeaponPivot						=> m_WeaponPivot;

	// INTERFACE START
					bool						IWeapon.Enabled					{ get => enabled; set => enabled = value; }
					Transform					IWeapon.Transform				=> transform;
					EWeaponState				IWeapon.WeaponState				=> m_WeaponState;
					EWeaponSubState				IWeapon.WeaponSubState			=> m_WeaponSubState;
					Entity						IWeapon.Owner					=> m_Owner;
					Database.Section			IWeapon.Section					=> m_WpnSection;
					string						IWeapon.OtherInfo				=> OtherInfo;
					Vector3						IWeaponZoom.ZoomOffset			=> m_WeaponData.ZoomOffset;
					float						IWeaponZoom.ZoomingTime			=> m_WeaponData.BaseZoomingTime;
					float						IWeaponZoom.ZoomSensitivity		=> GetZoomSensitivity();
					float						IWeaponZoom.ZoomFactor			=> m_WeaponData.BaseZoomFactor;
	// INTERFACE END

	// UNITY COMPONENTS
	protected		Animator					m_Animator						= null;
	private			Renderer[]					m_WeaponRenderes				= null;

	// ANIMATIONS
	//protected		AnimationClip				m_FireAnim						= null;
	protected		AnimationClip				m_ReloadAnim					= null;
	protected		AnimationClip				m_DrawAnim						= null;

	// Weapon Flags
	protected		bool						m_IsLocked						= false;
	protected		bool						m_NeedRecharge =>
		m_PrimaryWeaponModuleSlot.WeaponModule.NeedReload() ||
		m_SecondaryWeaponModuleSlot.WeaponModule.NeedReload();


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	string				OtherInfo
	{
		get
		{
			string primaryModule	= m_PrimaryWeaponModuleSlot.WeaponModule.ModuleSection.GetSectionName();
			string secondaryModule	= m_SecondaryWeaponModuleSlot.WeaponModule.ModuleSection.GetSectionName();
			return $"{primaryModule},{secondaryModule}";
		}
	}



	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			Awake()
	{
		// Animations
		{
			if (CustomAssertions.IsTrue(Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL, out m_Animator)))
			{
			//	CustomAssertions.IsTrue(m_Animator.GetClipFromAnimator("fire", ref m_FireAnim));
				CustomAssertions.IsTrue(m_Animator.GetClipFromAnimator("reload", ref m_ReloadAnim));
				CustomAssertions.IsTrue(m_Animator.GetClipFromAnimator("draw", ref m_DrawAnim));
			}
		}

		// Weapon Seaction
		if (CustomAssertions.IsTrue(GlobalManager.Configs.TryGetSection(m_WpnBaseSectionName, out m_WpnSection)))
		{
			CustomAssertions.IsTrue(GlobalManager.Configs.TrySectionToOuter(m_WpnSection, m_WeaponData, skipUndefinedFields: true));
		}


		// Weapon Pivot
		CustomAssertions.IsTrue(Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL_AND_PARENTS, out m_WeaponPivot));

		// TODO Remove ASAP
		SetOwner(Player.Instance);

		// Modules & Attachments
		{
		//	InitializeModules();

			// 
			InitializeAttachments();

			// 
			RestoreBaseConfiguration();

			foreach (IWeaponAttachment attachment in transform.GetComponentsInChildren<IWeaponAttachment>())
			{
				attachment.OnAttach();
			}
		}

		// The weapons and modules and attachments must be sabed in any case, event if the wepoan is not active at save moment
		if (CustomAssertions.IsNotNull(GameManager.SaveAndLoad))
		{
			GameManager.SaveAndLoad.OnSave += OnSave;
			GameManager.SaveAndLoad.OnLoad += OnLoad;
		}

		WeaponManager.Instance.RegisterWeapon(this);
	}


	//////////////////////////////////////////////////////////////////////////
	public void SetOwner(Entity entity)
	{
		m_Owner = entity;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnDestroy()
	{
		// Deregister only on destruction
		if (GameManager.SaveAndLoad.IsNotNull())
		{
			GameManager.SaveAndLoad.OnSave -= OnSave;
			GameManager.SaveAndLoad.OnLoad -= OnLoad;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnEnable()
	{
		// Re-Activate attachments (Pheraps is not a good idea to force as activated)
		// Actitve and internal state should be set using default settings or saved ones
	//	foreach (IWeaponAttachment attachment in transform.GetComponentsInChildren<IWeaponAttachment>())
	//	{
	//		attachment.SetActive(true);
	//	}

		CustomAssertions.IsNotNull(GlobalManager.InputMgr);
		{
			//										COMMAND								COMMAND ID						ACTION							PREDICATE
			GlobalManager.InputMgr.BindCall(EInputCommands.PRIMARY_FIRE_PRESS,		"Wpn_Primary_Fire_Start",		PrimaryFire_Start,		Predicate_PrimaryFire_Start);
			GlobalManager.InputMgr.BindCall(EInputCommands.PRIMARY_FIRE_HOLD,		"Wpn_Primary_Fire_Update",		PrimaryFire_Update,		Predicate_PrimaryFire_Update);
			GlobalManager.InputMgr.BindCall(EInputCommands.PRIMARY_FIRE_RELEASE,	"Wpn_Primary_Fire_End",			PrimaryFire_End,		Predicate_PrimaryFire_End);

			GlobalManager.InputMgr.BindCall(EInputCommands.SECONDARY_FIRE_PRESS,	"Wpn_Secondary_Fire_Start",		SecondaryFire_Start,	Predicate_SecondaryFire_Start);
			GlobalManager.InputMgr.BindCall(EInputCommands.SECONDARY_FIRE_HOLD,		"Wpn_Secondary_Fire_Update",	SecondaryFire_Update,	Predicate_SecondaryFire_Update);
			GlobalManager.InputMgr.BindCall(EInputCommands.SECONDARY_FIRE_RELEASE,	"Wpn_Secondary_Fire_End",		SecondaryFire_End,		Predicate_SecondaryFire_End);

			GlobalManager.InputMgr.BindCall(EInputCommands.RELOAD_WPN,				"Wpn_Reload",					Reload,					Predicate_Reload);

			void UnZoom() => WeaponManager.Instance.ZoomOut();
			bool Predicate_UnZoom() => Player.Instance.Motion.MotionStrategy.States.IsRunning && WeaponManager.Instance.IsZoomed;
			GlobalManager.InputMgr.BindCall(EInputCommands.STATE_RUN,				"Wpn_ExitZoom",					UnZoom,					Predicate_UnZoom);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnDisable()
	{
		CustomAssertions.IsNotNull(GlobalManager.InputMgr);
		{
			GlobalManager.InputMgr.UnbindCall(EInputCommands.PRIMARY_FIRE_PRESS,		"Wpn_Primary_Fire_Start");
			GlobalManager.InputMgr.UnbindCall(EInputCommands.PRIMARY_FIRE_HOLD,			"Wpn_Primary_Fire_Update");
			GlobalManager.InputMgr.UnbindCall(EInputCommands.PRIMARY_FIRE_RELEASE,		"Wpn_Primary_Fire_End");

			GlobalManager.InputMgr.UnbindCall(EInputCommands.SECONDARY_FIRE_PRESS,		"Wpn_Secondary_Fire_Start");
			GlobalManager.InputMgr.UnbindCall(EInputCommands.SECONDARY_FIRE_HOLD,		"Wpn_Secondary_Fire_Update");
			GlobalManager.InputMgr.UnbindCall(EInputCommands.SECONDARY_FIRE_RELEASE,	"Wpn_Secondary_Fire_End");

			GlobalManager.InputMgr.UnbindCall(EInputCommands.RELOAD_WPN,				"Wpn_Reload");

			GlobalManager.InputMgr.UnbindCall(EInputCommands.STATE_RUN,					"Wpn_ExitZoom");
		}

	//	foreach (IWeaponAttachment attachment in transform.GetComponentsInChildren<IWeaponAttachment>())
	//	{
	//		attachment.SetActive(false);
	//	}

	//	CustomAssertions.IsNotNull(m_PrimaryWeaponModuleSlot.WeaponModule);
	//	m_PrimaryWeaponModuleSlot.WeaponModule.enabled = false;

	//	CustomAssertions.IsNotNull(m_SecondaryWeaponModuleSlot.WeaponModule);
	//	m_SecondaryWeaponModuleSlot.WeaponModule.enabled = false;
	}


	//////////////////////////////////////////////////////////////////////////
	private void RestoreBaseConfiguration()
	{
		// Restore weapon data
		CustomAssertions.IsTrue(GlobalManager.Configs.TrySectionToOuter(m_WpnSection, m_WeaponData));

		LoadAndConfigureModule(this, m_WpnSection, m_PrimaryWeaponModuleSlot);

		LoadAndConfigureModule(this, m_WpnSection, m_SecondaryWeaponModuleSlot);

		m_Modifiers.Clear();
	}


	//////////////////////////////////////////////////////////////////////////
	private float GetZoomSensitivity()
	{
		float zoomSensitivity		= m_WeaponData.BaseZoomSensitivity;

		if (Attachments.HasAttachment<WPN_WeaponAttachment_Zoom_Base>())
		{
			var attachment = Attachments.GetAttachment<WPN_WeaponAttachment_Zoom_Base>();
			zoomSensitivity = attachment.ZoomSensitivityMultiplier;
		}
		
		return zoomSensitivity;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void Reload()
	{
		CoroutinesManager.Start(ReloadCO(OnEndReload), "Weapon::Reload: Reloading co");
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual StreamUnit OnSave(StreamData streamData)
	{
		StreamUnit streamUnit = streamData.NewUnit(gameObject);

		streamUnit.SetInternal("PrimaryModule", m_PrimaryWeaponModuleSlot.WeaponModule.ModuleSection.GetSectionName());

		m_PrimaryWeaponModuleSlot.WeaponModule.OnSave(streamUnit);
		m_SecondaryWeaponModuleSlot.WeaponModule.OnSave(streamUnit);
		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual StreamUnit OnLoad(StreamData streamData)
	{
		m_Animator.Play("draw", -1, 0.99f);

		bool bResult = streamData.TryGetUnit(this, out StreamUnit streamUnit);
		if (bResult)
		{
			m_PrimaryWeaponModuleSlot.WeaponModule.OnLoad(streamUnit);
			m_SecondaryWeaponModuleSlot.WeaponModule.OnLoad(streamUnit);

			UIManager.InGame.UpdateUI();
		}
		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Return if the current weapon allow the change at this time </summary>
	public virtual bool CanChangeWeapon()
	{
		bool result = m_IsLocked == false;
		result &= m_PrimaryWeaponModuleSlot.WeaponModule.CanChangeWeapon();
		result &= m_SecondaryWeaponModuleSlot.WeaponModule.CanChangeWeapon();
		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Called before weapon change </summary>
	public virtual void OnWeaponChange()
	{
		m_PrimaryWeaponModuleSlot.WeaponModule.OnWeaponChange();
		m_SecondaryWeaponModuleSlot.WeaponModule.OnWeaponChange();
		enabled = false;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Callback after the reload animation </summary>
	protected virtual void OnEndReload()
	{
		m_PrimaryWeaponModuleSlot.WeaponModule.OnAfterReload();
		m_SecondaryWeaponModuleSlot.WeaponModule.OnAfterReload();

		// Update UI
		UIManager.InGame.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Start the draw animation, return the seconds to wait </summary>
	public virtual float Draw()
	{
		m_Animator.Play("draw", -1, 0f);
		m_WeaponState = EWeaponState.DRAWED;
		m_WeaponSubState = EWeaponSubState.TRANSITION;

		m_IsLocked = true;
		TimersManager.Instance.AddTimerScaled(m_DrawAnim.length, () =>
		{
			m_WeaponSubState = EWeaponSubState.IDLE;
			m_IsLocked = false;
		});

		return m_DrawAnim.length;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Start the stash animation, return the seconds to wait </summary>
	public virtual float Stash()
	{
		m_Animator.Play("stash", -1, 0f);
		m_WeaponState = EWeaponState.STASHED;
		m_WeaponSubState = EWeaponSubState.IDLE;

		m_IsLocked = true;
		TimersManager.Instance.AddTimerScaled(m_DrawAnim.length, () =>
		{
			m_WeaponSubState = EWeaponSubState.IDLE;
			m_IsLocked = false;
		});

		return m_DrawAnim.length;
	}


	//////////////////////////////////////////////////////////////////////////
	public void Hide()
	{
		if (transform.TrySearchComponents(ESearchContext.LOCAL_AND_CHILDREN, out m_WeaponRenderes, r => r.enabled))
		{
			foreach (Renderer r in m_WeaponRenderes)
			{
				r.enabled = false;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void Show()
	{
		if (m_WeaponRenderes.IsNotNull())
		{
			System.Array.ForEach(m_WeaponRenderes, r => r.enabled = true);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected IEnumerator ReloadCO(System.Action onReloadEnd)
	{
		m_IsLocked = true;

		m_PrimaryWeaponModuleSlot.WeaponModule.enabled = false;
		m_SecondaryWeaponModuleSlot.WeaponModule.enabled = false;

		if (WeaponManager.Instance.IsZoomed)
		{
			Attachments.DeactivateAttachment<WPN_WeaponAttachment_Zoom_Base>();
			yield return new WaitWhile(() => WeaponManager.Instance.IsZoomed);
		}
		m_WeaponSubState = EWeaponSubState.RELOADING;

		// Reload animation
		{
			m_Animator.Play(m_ReloadAnim.name, -1, 0f);
			float rechargeTimer = m_ReloadAnim.length * m_Animator.speed; // / 2f;
			yield return new WaitForSeconds(rechargeTimer);
		}

		m_PrimaryWeaponModuleSlot.WeaponModule.enabled = true;
		m_SecondaryWeaponModuleSlot.WeaponModule.enabled = true;

		m_WeaponSubState = EWeaponSubState.IDLE;
		m_IsLocked = false;

		onReloadEnd();
	}
}
