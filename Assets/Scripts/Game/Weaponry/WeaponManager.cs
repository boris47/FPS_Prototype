using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public partial class WeaponManager : OnDemandSingleton<WeaponManager>, IWeaponManager
{
	private			List<WeaponBase>	m_WeaponsList					= new List<WeaponBase>();

	// INTERFACE START
	public			GameObject		GameObject						=> gameObject;
	public			bool			Enabled							{ get => enabled; set => enabled = value; }

	public			bool			IsZoomed						=> m_ZoomedIn;
	public			IWeapon			CurrentWeapon					{ get; set; }
	public			int				CurrentWeaponIndex				{ get; set; }
	public			bool			IsChangingZoom					=> m_IsChangingZoom;
	public			bool			IsChangingWeapon				=> m_IsChangingWpn;
	public			float			ZoomSensitivity					=> m_ZoomSensitivity;
	public			float			ZoomFactor						=> m_ZoomFactor;
	// INTERFACE END

	// ZOOM
	private		bool				m_ZoomedIn						= false;
	private		Vector3				m_StartOffset					= Vector3.zero;
	private		Vector3				m_FinalOffset					= Vector3.zero;
	private		float				m_ZoomFactor					= 0.0f;
	private		float				m_ZoomingTime					= 0.0f;
	private		float				m_StartCameraFOV				= 0.0f;
	private		float				m_ZoomSensitivity				= 1.0f;
	private		Image				m_ZoomFrame						= null;

	// TRANSITIONS
	private		bool				m_IsChangingWpn					= false;
	private		bool				m_IsChangingZoom				= false;


	//////////////////////////////////////////////////////////////////////////
	protected override void Awake()
	{
		base.Awake();

		if (CustomAssertions.IsNotNull(GameManager.StreamEvents))
		{
			GameManager.StreamEvents.OnSave += OnSave;
			GameManager.StreamEvents.OnLoad += OnLoad;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDestroy()
	{
		if (GameManager.StreamEvents.IsNotNull())
		{
			GameManager.StreamEvents.OnSave -= OnSave;
			GameManager.StreamEvents.OnLoad -= OnLoad;
		}

		base.OnDestroy();
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		GlobalManager.InputMgr.BindCall( EInputCommands.SELECTION1, 		"WeaponChange_0",		() => ChangeWeapon( 0, 0 ) );
		GlobalManager.InputMgr.BindCall( EInputCommands.SELECTION2, 		"WeaponChange_1",		() => ChangeWeapon( 1, 0 ) );
		GlobalManager.InputMgr.BindCall( EInputCommands.SELECTION3, 		"WeaponChange_2",		() => ChangeWeapon( 2, 0 ) );
		GlobalManager.InputMgr.BindCall( EInputCommands.SELECTION4, 		"WeaponChange_3",		() => ChangeWeapon( 3, 0 ) );
		GlobalManager.InputMgr.BindCall( EInputCommands.SELECTION5, 		"WeaponChange_4",		() => ChangeWeapon( 4, 0 ) );
		GlobalManager.InputMgr.BindCall( EInputCommands.SELECTION6, 		"WeaponChange_5",		() => ChangeWeapon( 5, 0 ) );
		GlobalManager.InputMgr.BindCall( EInputCommands.SELECTION7,			"WeaponChange_6",		() => ChangeWeapon( 6, 0 ) );
		GlobalManager.InputMgr.BindCall( EInputCommands.SELECTION8,			"WeaponChange_7",		() => ChangeWeapon( 7, 0 ) );
		GlobalManager.InputMgr.BindCall( EInputCommands.SELECTION9,			"WeaponChange_8",		() => ChangeWeapon( 8, 0 ) );

		GlobalManager.InputMgr.BindCall( EInputCommands.SWITCH_NEXT,		"WeaponChange_Next",	() => ChangeWeapon( -1,  1 ));
		GlobalManager.InputMgr.BindCall( EInputCommands.SWITCH_PREVIOUS,	"WeaponChange_Prev",	() => ChangeWeapon( -1, -1 ));

		GlobalManager.InputMgr.BindCall( EInputCommands.ATTACHMENT1,		"Zoom",
			() => CurrentWeapon.Attachments.ToggleAttachment<WPN_WeaponAttachment_Zoom_Base>(),
			() => CurrentWeapon.Attachments.HasAttachment<WPN_WeaponAttachment_Zoom_Base>()
		);

	//	GlobalManager.InputMgr.BindCall( EInputCommands.ATTACHMENT2,		"LaserPointer",
	//		() => CurrentWeapon.Attachments.ToggleAttachment<WPN_ModuleAttachment_LaserPointer>(),
	//		() => CurrentWeapon.Attachments.HasAttachment<WPN_WeaponAttachment_LaserPointer>()
	//	);

		GlobalManager.InputMgr.BindCall( EInputCommands.ATTACHMENT3,		"Flashlight",
			() => CurrentWeapon.Attachments.ToggleAttachment<WPN_WeaponAttachment_Flashlight>(),
			() => CurrentWeapon.Attachments.HasAttachment<WPN_WeaponAttachment_Flashlight>()
		);
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		GlobalManager.InputMgr.UnbindCall( EInputCommands.SELECTION1, 		"WeaponChange_0"	);
		GlobalManager.InputMgr.UnbindCall( EInputCommands.SELECTION2, 		"WeaponChange_1"	);
		GlobalManager.InputMgr.UnbindCall( EInputCommands.SELECTION3, 		"WeaponChange_2"	);
		GlobalManager.InputMgr.UnbindCall( EInputCommands.SELECTION4, 		"WeaponChange_3"	);
		GlobalManager.InputMgr.UnbindCall( EInputCommands.SELECTION5, 		"WeaponChange_4"	);
		GlobalManager.InputMgr.UnbindCall( EInputCommands.SELECTION6, 		"WeaponChange_5"	);
		GlobalManager.InputMgr.UnbindCall( EInputCommands.SELECTION7,		"WeaponChange_6"	);
		GlobalManager.InputMgr.UnbindCall( EInputCommands.SELECTION8,		"WeaponChange_7"	);
		GlobalManager.InputMgr.UnbindCall( EInputCommands.SELECTION9,		"WeaponChange_8"	);

		GlobalManager.InputMgr.UnbindCall( EInputCommands.SWITCH_NEXT,		"WeaponChange_Next" );
		GlobalManager.InputMgr.UnbindCall( EInputCommands.SWITCH_PREVIOUS,	"WeaponChange_Prev" );

		GlobalManager.InputMgr.UnbindCall( EInputCommands.ATTACHMENT1,		"Zoom"				);
	//	GlobalManager.InputMgr.UnbindCall( EInputCommands.ATTACHMENT2,		"LaserPointer"		);
		GlobalManager.InputMgr.UnbindCall( EInputCommands.ATTACHMENT3,		"Flashlight"		);
	}


	//////////////////////////////////////////////////////////////////////////
	private static int WeaponOrderComparer(WeaponBase a, WeaponBase b)
	{
		int indexA = a.transform.GetSiblingIndex();
		int indexB = b.transform.GetSiblingIndex();

		return (indexA > indexB) ? 1 : (indexA < indexB) ? -1 : 0;
	}


	//////////////////////////////////////////////////////////////////////////
	private IEnumerator Start()
	{
		yield return CoroutinesManager.WaitPendingCoroutines();
		print("On Start");
		DisableAllWeapons();

		m_WeaponsList.Sort(WeaponOrderComparer);

		// Set current weapon
		CurrentWeapon = GetWeaponByIndex(CurrentWeaponIndex);

		// Enable current weapon
		CurrentWeapon.Transform.gameObject.SetActive(true);
		CurrentWeapon.Draw();

		//m_ZoomSensitivity = CurrentWeapon.ZoomSensitivity;

		m_StartCameraFOV = FPSEntityCamera.Instance.MainCamera.fieldOfView;

		// Make sure that ui show data of currnt active weapon
		UIManager.InGame.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	private bool OnSave(StreamData streamData, ref StreamUnit streamUnit)
	{
		streamUnit = streamData.NewUnit(this);

		streamUnit.SetInternal("CurrentWeaponIndex", CurrentWeaponIndex);
		streamUnit.SetInternal("ZoomedIn", m_ZoomedIn);
		streamUnit.SetInternal("StartOffset", Utils.Converters.Vector3ToString(m_StartOffset));
		streamUnit.SetInternal("FinalOffset", Utils.Converters.Vector3ToString(m_FinalOffset));
		streamUnit.SetInternal("ZoomFactor", m_ZoomFactor);
		streamUnit.SetInternal("ZoomingTime", m_ZoomingTime);
		streamUnit.SetInternal("ZoomSensitivity", m_ZoomSensitivity);
		streamUnit.SetInternal("StartCameraFOV", m_StartCameraFOV);
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	private bool OnLoad(StreamData streamData, ref StreamUnit streamUnit)
	{
		StopAllCoroutines();
		m_ZoomingTime = 0f;
		m_IsChangingWpn = false;

		bool bResult = streamData.TryGetUnit(gameObject, out streamUnit);
		if (bResult)
		{
			// Current Weapon
			{
				DisableAllWeapons(); // TODO Wrong

				CurrentWeaponIndex = streamUnit.GetAsInt("CurrentWeaponIndex");
				CurrentWeapon = GetWeaponByIndex(CurrentWeaponIndex);
				CurrentWeapon.Transform.gameObject.SetActive(true);
			}

			// Zoom
			{
				m_ZoomedIn = streamUnit.GetAsBool("ZoomedIn");
				m_StartOffset = streamUnit.GetAsVector("StartOffset");
				m_FinalOffset = streamUnit.GetAsVector("FinalOffset");
				m_ZoomFactor = streamUnit.GetAsFloat("ZoomFactor");
				m_ZoomingTime = streamUnit.GetAsFloat("ZoomingTime");
				m_ZoomSensitivity = streamUnit.GetAsFloat("ZoomSensitivity");
				m_StartCameraFOV = streamUnit.GetAsFloat("StartCameraFOV");

				CurrentWeapon.WeaponPivot.transform.localPosition = m_ZoomedIn ? m_FinalOffset : m_StartOffset;

				float cameraFinalFov = m_StartCameraFOV / m_ZoomFactor;
				FPSEntityCamera.Instance.MainCamera.fieldOfView = m_ZoomedIn ? cameraFinalFov : m_StartCameraFOV;
			}
		}
		return bResult;
	}


	//////////////////////////////////////////////////////////////////////////
	private void DisableAllWeapons()
	{
		m_WeaponsList.ForEach(w => w.gameObject.SetActive(false));
	}


	//////////////////////////////////////////////////////////////////////////
	private IWeapon GetWeaponByIndex(int WpnIdx)
	{
		return m_WeaponsList.IsValidIndex(WpnIdx) ? m_WeaponsList[WpnIdx] : null;
	}


	//////////////////////////////////////////////////////////////////////////
	public void RegisterWeapon(WeaponBase weapon)
	{
		m_WeaponsList.Add(weapon);

		CurrentWeapon = weapon;
	}


	//////////////////////////////////////////////////////////////////////////
	public Coroutine ToggleZoom(Vector3? ZoomOffset, float ZoomFactor = -1f, float ZoomingTime = -1f, float ZoomSensitivity = -1f, Image ZoomFrame = null)
	{
		if (m_ZoomedIn)
		{
			return ZoomOut();
		}
		else
		{
			return ZoomIn(ZoomOffset, ZoomFactor, ZoomingTime, ZoomSensitivity, ZoomFrame);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public Coroutine ZoomIn(Vector3? ZoomOffset = null, float ZoomFactor = -1f, float ZoomingTime = -1f, float ZoomSensitivity = -1f, Image ZoomFrame = null)
	{
		if (!m_ZoomedIn && !m_IsChangingZoom)
		{
			Vector3 zoomOffset = ZoomOffset ?? CurrentWeapon.ZoomOffset;
			float zoomFactor = ZoomFactor > 0f ? ZoomFactor : CurrentWeapon.ZoomFactor;
			float zoomingTime = ZoomingTime > 0f ? ZoomingTime : CurrentWeapon.ZoomingTime;
			float zoomSensitivity = ZoomSensitivity > 0f ? ZoomSensitivity : CurrentWeapon.ZoomSensitivity;

			m_StartOffset = CurrentWeapon.WeaponPivot.transform.localPosition;
			m_StartCameraFOV = FPSEntityCamera.Instance.MainCamera.fieldOfView;
			m_FinalOffset = zoomOffset;
			m_ZoomFactor = ZoomFactor;
			m_ZoomingTime = ZoomingTime;
			m_ZoomSensitivity = ZoomSensitivity;
			m_ZoomFrame = ZoomFrame;


			m_IsChangingZoom = true;
			return CoroutinesManager.Start(Internal_ZoomInCO(), "WeaponManger::ZoomIn: co");
		}
		return null;
	}


	//////////////////////////////////////////////////////////////////////////
	public Coroutine ZoomOut()
	{
		if (m_ZoomedIn && !m_IsChangingZoom)
		{
			m_IsChangingZoom = true;
			return CoroutinesManager.Start(Internal_ZoomOutCO(), "WeaponManager::ZoomOut: co");
		}
		return null;
	}


	//////////////////////////////////////////////////////////////////////////
	public void ChangeWeaponRequest(int wpnIdx)
	{
		if (!m_IsChangingWpn && GetWeaponByIndex(CurrentWeaponIndex).CanChangeWeapon())
		{
			CoroutinesManager.Start(ChangeWeaponCO(wpnIdx), "WeaponManager::ChangeWeaponRequest: Switching to weapon " + wpnIdx);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void ChangeWeapon(int index, int versus = 0)
	{
		if (!m_IsChangingWpn)
		{
			// If same weapon index of current weapon
			if (index == CurrentWeaponIndex)
			{
				// based on current weapon state choose action
				if (CurrentWeapon.WeaponState == EWeaponState.DRAWED)
				{
					CurrentWeapon.Stash();
				}
				else
				{
					CurrentWeapon.Draw();
				}
			}
			else
			{
				if (CurrentWeapon.CanChangeWeapon())
				{
					// For a valid index (Direct weapon index)
					if (m_WeaponsList.IsValidIndex(index))
					{
						// Start weapon change
						CoroutinesManager.Start(ChangeWeaponCO(index), "WeaponManager::ChangeWeapon: Changing weapon");
					}
					else // Next or Previous weapon
					{
						// if a versus is definded, find out next weapon index
						int lastWeapIdx = m_WeaponsList.Count - 1;
						int newWeaponIdx = CurrentWeaponIndex + versus;

						if (newWeaponIdx == -1) newWeaponIdx = lastWeapIdx;
						if (newWeaponIdx > lastWeapIdx) newWeaponIdx = 0;

						// No weapon change if only one weapon is available
						if (newWeaponIdx != CurrentWeaponIndex)
						{
							// Start weapon change
							CoroutinesManager.Start(ChangeWeaponCO(newWeaponIdx), "WeaponManager::ChangeWeapon: Changing weapon 2");
						}
					}
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private IEnumerator ChangeWeaponCO(int newWeaponIdx)
	{
		m_IsChangingWpn = true;

		// Exit from zoom
		if (m_ZoomedIn)
		{
			CurrentWeapon.Attachments.DeactivateAttachment<WPN_WeaponAttachment_Zoom_Base>();
			yield return new WaitWhile(() => m_ZoomedIn);
		}

		IWeapon currentWeapon = GetWeaponByIndex(CurrentWeaponIndex);
		IWeapon nextWeapon = GetWeaponByIndex(newWeaponIdx);

		// last event before changing
		currentWeapon.OnWeaponChange();     //  Weapon properties reset ( enable = false )

		// If weapon is drawed play stash animation
		if ((currentWeapon.WeaponState == EWeaponState.DRAWED))
		{
			// Play stash animation and get animation time
			float stashTime = CurrentWeapon.Stash();

			// wait for stash animation to terminate
			yield return new WaitForSeconds(stashTime);
		}

		// switch active object
		currentWeapon.Transform.gameObject.SetActive(false);
		nextWeapon.Transform.gameObject.SetActive(true);

		// set current weapon index and ref
		CurrentWeaponIndex = newWeaponIdx;
		CurrentWeapon = nextWeapon;

		// Play draw animation and get animation time
		float drawTime = CurrentWeapon.Draw();
		yield return new WaitForSeconds(drawTime * 0.8f);

		// Update UI
		UIManager.InGame.UpdateUI();

		// weapon return active
		CurrentWeapon.Enabled = true;
		m_IsChangingWpn = false;
	}


	//////////////////////////////////////////////////////////////////////////
	private IEnumerator Internal_ZoomInCO()
	{
		float cameraFinalFov = m_StartCameraFOV / m_ZoomFactor;

		float	interpolant = 0f;
		float	currentTime = 0f;

		Transform weaponPivot = CurrentWeapon.WeaponPivot.transform;
		Camera mainCamera = FPSEntityCamera.Instance.MainCamera;

		// Transition
		while (interpolant < 1f)
		{
			currentTime					+= Time.deltaTime;
			interpolant					=  currentTime / m_ZoomingTime;
			weaponPivot.localPosition	= Vector3.Lerp(m_StartOffset, m_FinalOffset, interpolant );
			mainCamera.fieldOfView		= Mathf.Lerp(m_StartCameraFOV, cameraFinalFov, interpolant );
			yield return null;
		}

		if (m_ZoomFrame.IsNotNull())
		{
			UIManager.InGame.SetFrame(m_ZoomFrame);
			CurrentWeapon.Hide();
		}
		UIManager.InGame.HideCrosshairs();

		m_ZoomedIn = true;
		m_IsChangingZoom = false;
	}


	//////////////////////////////////////////////////////////////////////////
	private IEnumerator Internal_ZoomOutCO()
	{
		Camera	mainCamera = FPSEntityCamera.Instance.MainCamera;
		float	cameraCurrentFov = mainCamera.fieldOfView;

		float	interpolant = 0f;
		float	currentTime = 0f;

		var weaponPivot = CurrentWeapon.WeaponPivot.transform;

		if (m_ZoomFrame.IsNotNull())
		{
			UIManager.InGame.SetFrame(null);
			CurrentWeapon.Show();
		}
		UIManager.InGame.ShowCrosshairs();

		// Transition
		while (interpolant < 1f)
		{	
			currentTime					+= Time.deltaTime;
			interpolant					= currentTime / m_ZoomingTime;
			weaponPivot.localPosition	= Vector3.Lerp(m_FinalOffset, m_StartOffset, interpolant );
			mainCamera.fieldOfView		= Mathf.Lerp( cameraCurrentFov, m_StartCameraFOV, interpolant );
			yield return null;
		}

		m_ZoomedIn = false;
		m_IsChangingZoom = false;
	}
}
