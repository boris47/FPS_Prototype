using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public partial interface IWeaponManager {

	GameObject			GameObject						{ get; }
	bool				Enabled							{ get; set; }

	bool				IsZoomed						{ get; }
	IWeapon				CurrentWeapon					{ get; set; }
	int					CurrentWeaponIndex				{ get; set; }
	float				ZoomSensitivity					{ get; }
	float				ZoomFactor						{ get; }
	bool				IsChangingWeapon				{ get; }

	void				RegisterWeapon					( Weapon wpn );
	Coroutine			ZoomIn							( Vector3? ZoomOffset = null, float ZoomFactor = -1f, float ZoomingTime = -1f, float ZoomSensitivity = -1f, Image ZoomFrame = null );
	Coroutine			ZoomOut							();
	void				ChangeWeaponRequest				( int wpnIdx );

}

[System.Serializable]
public partial class WeaponManager : MonoBehaviour, IWeaponManager {
	
	private static	IWeaponManager	m_Instance						= null;
	public static	IWeaponManager	Instance
	{
		get { return m_Instance; }
	}

	private			List<Weapon>	m_WeaponsList					= new List<Weapon>();
	private			bool			m_IsReady						= false;
	private			bool			IsEnabled() => this.m_IsReady;

	// INTERFACE START
	public			GameObject		GameObject						{ get { return this.gameObject; } }
	public			bool			Enabled							{ get { return this.enabled; } set { this.enabled = value; } }

	public			bool			IsZoomed						{ get { return this.m_ZoomedIn; } }
	public			IWeapon			CurrentWeapon					{ get; set; }
	public			int				CurrentWeaponIndex				{ get; set; }
	public			bool			IsChangingWeapon				{ get { return this.m_IsChangingWpn != false; } }
	public			float			ZoomSensitivity					{ get { return this.m_ZoomSensitivity; } }
	public			float			ZoomFactor						{ get { return this.m_ZoomFactor; } }
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
	// Awake
	private				void		Awake()
	{
		// Singleton
		if ( m_Instance != null )
		{
			Destroy(this.gameObject );
			return;
		}
		DontDestroyOnLoad( this );
		m_Instance = this;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private				void		OnEnable()
	{
		GameManager.StreamEvents.OnSave += this.OnSave;
		GameManager.StreamEvents.OnLoad += this.OnLoad;

		GlobalManager.InputMgr.BindCall( EInputCommands.SELECTION1, 		"WeaponChange_0",		() => this.ChangeWeapon( 0, 0 ), this.IsEnabled	);
		GlobalManager.InputMgr.BindCall( EInputCommands.SELECTION2, 		"WeaponChange_1",		() => this.ChangeWeapon( 1, 0 ), this.IsEnabled	);
		GlobalManager.InputMgr.BindCall( EInputCommands.SELECTION3, 		"WeaponChange_2",		() => this.ChangeWeapon( 2, 0 ), this.IsEnabled	);
		GlobalManager.InputMgr.BindCall( EInputCommands.SELECTION4, 		"WeaponChange_3",		() => this.ChangeWeapon( 3, 0 ), this.IsEnabled	);
		GlobalManager.InputMgr.BindCall( EInputCommands.SELECTION5, 		"WeaponChange_4",		() => this.ChangeWeapon( 4, 0 ), this.IsEnabled	);
		GlobalManager.InputMgr.BindCall( EInputCommands.SELECTION6, 		"WeaponChange_5",		() => this.ChangeWeapon( 5, 0 ), this.IsEnabled	);
		GlobalManager.InputMgr.BindCall( EInputCommands.SELECTION7,			"WeaponChange_6",		() => this.ChangeWeapon( 6, 0 ), this.IsEnabled	);
		GlobalManager.InputMgr.BindCall( EInputCommands.SELECTION8,			"WeaponChange_7",		() => this.ChangeWeapon( 7, 0 ), this.IsEnabled	);
		GlobalManager.InputMgr.BindCall( EInputCommands.SELECTION9,			"WeaponChange_8",		() => this.ChangeWeapon( 8, 0 ), this.IsEnabled	);

		GlobalManager.InputMgr.BindCall( EInputCommands.SWITCH_NEXT,		"WeaponChange_Next",	() => this.ChangeWeapon( -1,  1 ), this.IsEnabled	);
		GlobalManager.InputMgr.BindCall( EInputCommands.SWITCH_PREVIOUS,	"WeaponChange_Prev",	() => this.ChangeWeapon( -1, -1 ), this.IsEnabled	);
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDisable
	private				void		OnDisable()
	{
		if ( GameManager.StreamEvents.IsNotNull() )
		{
			GameManager.StreamEvents.OnSave -= this.OnSave;
			GameManager.StreamEvents.OnLoad -= this.OnLoad;
		}

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
	}


	//////////////////////////////////////////////////////////////////////////
	// WeaponComparer ( Static )
	private static		int			WeaponOrderComparer( Weapon a, Weapon b )
	{
		int indexA = a.transform.GetSiblingIndex();
		int indexB = b.transform.GetSiblingIndex();

		return ( indexA > indexB ) ? 1 : ( indexA < indexB ) ? -1 : 0;
	}



	//////////////////////////////////////////////////////////////////////////
	// Start
	private IEnumerator Start()
	{
		yield return CoroutinesManager.WaitPendingCoroutines();

		this.DisableAllWeapons();

		this.m_WeaponsList.Sort( WeaponOrderComparer );

		// Set current weapon
		this.CurrentWeapon = this.GetWeaponByIndex(this.CurrentWeaponIndex );

		// Enable current weapon
		this.CurrentWeapon.Transform.gameObject.SetActive( true );
		this.CurrentWeapon.Enabled = true;
		this.CurrentWeapon.Draw();

		//		m_ZoomSensitivity = CurrentWeapon.ZoomSensitivity;

		this.m_StartCameraFOV = CameraControl.Instance.MainCamera.fieldOfView;

		// Make sure that ui show data of currnt active weapon
		UIManager.InGame.UpdateUI();

		this.m_IsReady = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave
	private				StreamUnit	OnSave( StreamData streamData )
	{
		StreamUnit streamUnit	= streamData.NewUnit(this.gameObject );

		streamUnit.SetInternal( "CurrentWeaponIndex", this.CurrentWeaponIndex );
		streamUnit.SetInternal( "ZoomedIn", this.m_ZoomedIn );
		streamUnit.SetInternal( "StartOffset",					Utils.Converters.Vector3ToString(this.m_StartOffset ) );
		streamUnit.SetInternal( "FinalOffset",					Utils.Converters.Vector3ToString(this.m_FinalOffset ) );
		streamUnit.SetInternal( "ZoomFactor", this.m_ZoomFactor );
		streamUnit.SetInternal( "ZoomingTime", this.m_ZoomingTime );
		streamUnit.SetInternal( "ZoomSensitivity", this.m_ZoomSensitivity );
		streamUnit.SetInternal( "StartCameraFOV", this.m_StartCameraFOV );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad
	private				StreamUnit	OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = null;
		streamData.GetUnit(this.gameObject, ref streamUnit );

		this.StopAllCoroutines();
		this.m_ZoomingTime					= 0f;
		this.m_IsChangingWpn					= false;

		// Current Weapon
		{
			this.DisableAllWeapons();

			this.CurrentWeaponIndex			= streamUnit.GetAsInt("CurrentWeaponIndex" );
			this.CurrentWeapon				= this.GetWeaponByIndex(this.CurrentWeaponIndex );
			this.CurrentWeapon.Transform.gameObject.SetActive( true );
			this.CurrentWeapon.Enabled		= true;
		}

		// Zoom
		{
			this.m_ZoomedIn					= streamUnit.GetAsBool( "ZoomedIn" );
			this.m_StartOffset				= streamUnit.GetAsVector( "StartOffset" );
			this.m_FinalOffset				= streamUnit.GetAsVector( "FinalOffset" );
			this.m_ZoomFactor				= streamUnit.GetAsFloat( "ZoomFactor" );
			this.m_ZoomingTime				= streamUnit.GetAsFloat( "ZoomingTime" );
			this.m_ZoomSensitivity			= streamUnit.GetAsFloat( "ZoomSensitivity" );
			this.m_StartCameraFOV			= streamUnit.GetAsFloat( "StartCameraFOV" );

			CameraControl.Instance.WeaponPivot.localPosition = (this.m_ZoomedIn == true ) ? this.m_FinalOffset : this.m_StartOffset;

			float cameraFinalFov = this.m_StartCameraFOV / this.m_ZoomFactor;
			CameraControl.Instance.MainCamera.fieldOfView = (this.m_ZoomedIn == true ) ? cameraFinalFov : this.m_StartCameraFOV;
		}

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// DisableAllWeapons
	private				void		DisableAllWeapons()
	{
		this.m_WeaponsList.ForEach( w => { w.enabled = false; w.gameObject.SetActive( false ); } );
	}
	

	//////////////////////////////////////////////////////////////////////////
	// ZoomIGetWeaponByIndex
	private				IWeapon		GetWeaponByIndex( int WpnIdx )
	{
		return ( WpnIdx > -1 && WpnIdx < this.m_WeaponsList.Count ) ? this.m_WeaponsList[WpnIdx] : null;
	}
	

	//////////////////////////////////////////////////////////////////////////
	// RegisterWeapon
	public				void		RegisterWeapon( Weapon weapon )
	{
		this.m_WeaponsList.Add( weapon );

		this.CurrentWeapon = weapon;
	}


	//////////////////////////////////////////////////////////////////////////
	// ZoomIn
	public				Coroutine	ZoomIn( Vector3? ZoomOffset, float ZoomFactor = -1f, float ZoomingTime = -1f, float ZoomSensitivity = -1f, Image ZoomFrame = null )
	{
		if (this.m_ZoomedIn == false && this.m_IsChangingZoom == false )
		{
			Vector3 zoomOffset		= ZoomOffset.HasValue	? ZoomOffset.Value  : this.CurrentWeapon.ZoomOffset;
			float zoomFactor		= ZoomFactor > 0f		? ZoomFactor  : this.CurrentWeapon.ZoomFactor;
			float zoomingTime		= ZoomingTime > 0f		? ZoomingTime : this.CurrentWeapon.ZoomingTime;
			float zoomSensitivity	= ZoomSensitivity > 0f	? ZoomSensitivity : this.CurrentWeapon.ZoomSensitivity;

			this.m_StartOffset		= CameraControl.Instance.WeaponPivot.localPosition;
			this.m_StartCameraFOV	= CameraControl.Instance.MainCamera.fieldOfView;
			this.m_FinalOffset		= zoomOffset;
			this.m_ZoomFactor		= ZoomFactor;
			this.m_ZoomingTime		= ZoomingTime;
			this.m_ZoomSensitivity	= ZoomSensitivity;
			this.m_ZoomFrame			= ZoomFrame;


			this.m_IsChangingZoom = true;
			return CoroutinesManager.Start(this.Internal_ZoomInCO(), "WeaponManger::ZoomIn: co" );
		}
		return null;
	}


	//////////////////////////////////////////////////////////////////////////
	// ZoomOut
	public				Coroutine	ZoomOut()
	{
		if (this.m_ZoomedIn == true && this.m_IsChangingZoom == false )
		{
			this.m_IsChangingZoom = true;
			return CoroutinesManager.Start(this.Internal_ZoomOutCO(), "WeaponManager::ZoomOut: co" );
		}
		return null;
	}


	//////////////////////////////////////////////////////////////////////////
	// ChangeWeaponRequest
	public				void		ChangeWeaponRequest( int wpnIdx )
	{
		if (this.m_IsChangingWpn == true )
			return;

		if (this.GetWeaponByIndex(this.CurrentWeaponIndex ).CanChangeWeapon() == false )
			return;

		CoroutinesManager.Start(this.ChangeWeaponCO( wpnIdx ), "WeaponManager::ChangeWeaponRequest: Switching to weapon " + wpnIdx );
	}
	

	//////////////////////////////////////////////////////////////////////////
	// ChangeWeapon
	private				void		ChangeWeapon( int index, int versus = 0 )
	{
		if (this.m_IsChangingWpn == true )
			return;

		// If same weapon index of current weapon
		if ( index == this.CurrentWeaponIndex )
		{
			// based on current weapon state choose action
			if (this.CurrentWeapon.WeaponState == EWeaponState.DRAWED )
				this.CurrentWeapon.Stash();
			else
				this.CurrentWeapon.Draw();

			return;
		}

		// Skip if cannot change the current weapon
		if (this.CurrentWeapon.CanChangeWeapon() == false )
			return;

		// For a valid index
		if ( index > -1 && index < this.m_WeaponsList.Count )
		{
			// Start weapon change
			CoroutinesManager.Start(this.ChangeWeaponCO( index ), "WeaponManager::ChangeWeapon: Changing weapon" );
			return;
		}

		// if a versus is definded, find out next weapon index
		int lastWeapIdx = this.m_WeaponsList.Count - 1;
		int newWeaponIdx = this.CurrentWeaponIndex + versus;

		if ( newWeaponIdx == -1 )			newWeaponIdx = lastWeapIdx;
		if ( newWeaponIdx > lastWeapIdx )	newWeaponIdx = 0;

		// in case there is only a weapon available
		if ( newWeaponIdx == this.CurrentWeaponIndex )
			return;

		// Start weapon change
		CoroutinesManager.Start(this.ChangeWeaponCO( newWeaponIdx ), "WeaponManager::ChangeWeapon: Changing weapon 2" );
	}


	//////////////////////////////////////////////////////////////////////////
	// ChangeWeaponCO ( Coroutine )
	private				IEnumerator	ChangeWeaponCO( int newWeaponIdx )
	{
		this.m_IsChangingWpn = true;

		// Exit from zoom
		if (this.m_ZoomedIn == true )
		{
			yield return this.ZoomOut();
		}
		
		IWeapon currentWeapon	= this.GetWeaponByIndex(this.CurrentWeaponIndex );
		IWeapon nextWeapon		= this.GetWeaponByIndex( newWeaponIdx );

		// last event before changing
		currentWeapon.OnWeaponChange();		//  Weapon properties reset ( enable = false )

		// If weapon is drawed play stash animation
		if ( ( currentWeapon.WeaponState == EWeaponState.DRAWED ) )
		{
			// Play stash animation and get animation time
			float stashTime			= this.CurrentWeapon.Stash();

			// wait for stash animation to terminate
			yield return new WaitForSeconds( stashTime );
		}

		// switch active object
		currentWeapon.Transform.gameObject.SetActive( false );
		nextWeapon.Transform.gameObject.SetActive( true );

		// set current weapon index and ref
		this.CurrentWeaponIndex		= newWeaponIdx;
		this.CurrentWeapon			= nextWeapon;

		// Play draw animation and get animation time
		float drawTime			= this.CurrentWeapon.Draw();
		yield return new WaitForSeconds( drawTime * 0.8f );

		// Update UI
		UIManager.InGame.UpdateUI();

		// weapon return active
		this.CurrentWeapon.Enabled	= true;
		this.m_IsChangingWpn			= false;
	}


	//////////////////////////////////////////////////////////////////////////
	// ZoomInCO ( Coroutine )
	private				IEnumerator	Internal_ZoomInCO()
	{
		float cameraFinalFov = this.m_StartCameraFOV / this.m_ZoomFactor;
//		CurrentWeapon.Enabled = false;

		float	interpolant = 0f;
		float	currentTime = 0f;

		Transform weaponPivot = CameraControl.Instance.WeaponPivot;
		Camera mainCamera = CameraControl.Instance.MainCamera;

		// Transition
		while( interpolant < 1f )
		{
			currentTime					+= Time.deltaTime;
			interpolant					=  currentTime / this.m_ZoomingTime;
			weaponPivot.localPosition	= Vector3.Lerp(this.m_StartOffset, this.m_FinalOffset, interpolant );
			mainCamera.fieldOfView		= Mathf.Lerp(this.m_StartCameraFOV, cameraFinalFov, interpolant );
			yield return null;
		}

///		CameraControl.Instance.HeadMove.AmplitudeMult /= m_ZoomFactor;

		if (this.m_ZoomFrame != null )
		{
			UIManager.InGame.SetFrame(this.m_ZoomFrame );
			this.CurrentWeapon.Hide();
		}
		UIManager.InGame.HideCrosshair();

		//		CurrentWeapon.Enabled = true;
		this.m_ZoomedIn = true;
		this.m_IsChangingZoom = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// ZoomOut ( Coroutine )
	private				IEnumerator	Internal_ZoomOutCO()
	{
		float	cameraCurrentFov = CameraControl.Instance.MainCamera.fieldOfView;
//		CurrentWeapon.Enabled = false;

		float	interpolant = 0f;
		float	currentTime = 0f;

		Transform weaponPivot = CameraControl.Instance.WeaponPivot;
		Camera mainCamera = CameraControl.Instance.MainCamera;

		if (this.m_ZoomFrame != null )
		{
			UIManager.InGame.SetFrame( null );
			this.CurrentWeapon.Show();
		}
		UIManager.InGame.ShowCrosshair();

		// Transition
		while( interpolant < 1f )
		{	
			currentTime					+= Time.deltaTime;
			interpolant					= currentTime / this.m_ZoomingTime;
			weaponPivot.localPosition	= Vector3.Lerp(this.m_FinalOffset, this.m_StartOffset, interpolant );
			mainCamera.fieldOfView		= Mathf.Lerp( cameraCurrentFov, this.m_StartCameraFOV, interpolant );
			yield return null;
		}

		///		CameraControl.Instance.HeadMove.AmplitudeMult *= m_ZoomFactor;


		//		CurrentWeapon.Enabled = true;
		this.m_ZoomedIn = false;
		this.m_IsChangingZoom = false;
	}

	
	//////////////////////////////////////////////////////////////////////////
	// OnDestroy
	private void OnDestroy()
	{
		if ( (Object)m_Instance != this )
			return;

		m_Instance = null;
		this.m_IsReady = false;
	}
	
}
