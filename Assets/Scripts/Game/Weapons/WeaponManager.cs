using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IWeaponManager {

	GameObject			GameObject						{ get; }
	bool				Enabled							{ get; set; }

	bool				IsZoomed						{ get; }
	IWeapon				CurrentWeapon					{ get; set; }
	int					CurrentWeaponIndex				{ get; set; }
	float				ZoomSensitivity					{ get; }
	bool				IsChangingWeapon				{ get; }

	void				RegisterWpn						( Weapon wpn );
	void				ZoomIn							();
	void				ZoomOut							();
	IEnumerator			ZoomInCO						();
	IEnumerator			ZoomOutCO						();
	void				ChangeWeaponRequest				( int wpnIdx );

}

[System.Serializable]
public class WeaponManager : MonoBehaviour, IWeaponManager {
	
	public static	IWeaponManager	Instance						= null;

	private			List<Weapon>	WeaponsList						= new List<Weapon>(10);

	// INTERFACE START
	public			GameObject		GameObject						{ get { return gameObject; } }
	public			bool			Enabled							{ get { return enabled; } set { enabled = value; } }

	public			bool			IsZoomed						{ get { return m_ZoomedIn; } }
	public			IWeapon			CurrentWeapon					{ get; set; }
	public			int				CurrentWeaponIndex				{ get; set; }
	public			bool			IsChangingWeapon				{ get { return m_IsChangingWpn != false; } }
	public			float			ZoomSensitivity					{ get { return m_ZoomSensitivity; } }
	// INTERFACE END

	// ZOOMING
	private		bool				m_ZoomedIn						= false;
	private		Vector3				m_StartOffset					= Vector3.zero;
	private		Vector3				m_FinalOffset					= Vector3.zero;
	private		float				m_ZoomingTime					= 0f;
	private		float				m_StartCameraFOV				= 0f;
	private		float				m_ZoomSensitivity				= 1.0f;

	// TRANSITIONS
	private		bool				m_IsChangingWpn					= false;
	private		bool				m_IsChangingZoom				= false;


	////////////////////////////////////////////////////
	////////////////////////////////////////////////////
	// WEAPON STATICS

	public	void	DisableAll()
	{
		WeaponsList.ForEach( w => { w.enabled = false; w.transform.gameObject.SetActive( false ); } );
	}
	
	public	IWeapon	GetByIndex( int WpnIdx )
	{
		return ( WpnIdx > -1 && WpnIdx < WeaponsList.Count ) ? WeaponsList[WpnIdx] : null;
	}

	public	int		Count
	{
		get { return WeaponsList.Count; }
	}

	public	void	RegisterWpn( Weapon wpn )
	{
		WeaponsList.Add( wpn );
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		if ( Instance != null )
		{
			print( "WeaponManager: Object set inactive" );
			gameObject.SetActive( false );
			return;
		}
		Instance = this as IWeaponManager;
		DontDestroyOnLoad( this );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
		GameManager.StreamEvents.OnSave += OnSave;
		GameManager.StreamEvents.OnLoad += OnLoad;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDisable
	private void OnDisable()
	{
		GameManager.StreamEvents.OnSave -= OnSave;
		GameManager.StreamEvents.OnLoad -= OnLoad;
	}


	//////////////////////////////////////////////////////////////////////////
	// WeaponComparer ( Static )
	private static int WeaponOrderComparer( Weapon a, Weapon b )
	{
		int indexA = a.transform.GetSiblingIndex();
		int indexB = b.transform.GetSiblingIndex();

		return ( indexA > indexB ) ? 1 : ( indexA < indexB ) ? -1 : 0;
	}


	//////////////////////////////////////////////////////////////////////////
	// Start
	private				void	Start()
	{
		DisableAll();

		WeaponsList.Sort( WeaponOrderComparer );

		// Set current weapon
		CurrentWeapon = GetByIndex( CurrentWeaponIndex );

		// Enable current weapon
		CurrentWeapon.Transform.gameObject.SetActive( true );
		CurrentWeapon.Enabled = true;
		CurrentWeapon.Draw();

		m_ZoomSensitivity = CurrentWeapon.ZoomSensitivity;

		m_StartCameraFOV = Camera.main.fieldOfView;

		// Make sure that ui show data of currnt active weapon
		UI.Instance.InGame.UpdateUI();

		GameManager.InputMgr.BindCall( eInputCommands.SELECTION1, 		"WeaponChange_0",	() => ChangeWeapon( 0, 0 ) );
		GameManager.InputMgr.BindCall( eInputCommands.SELECTION2, 		"WeaponChange_1",	() => ChangeWeapon( 1, 0 ) );
		GameManager.InputMgr.BindCall( eInputCommands.SELECTION3, 		"WeaponChange_2",	() => ChangeWeapon( 2, 0 ) );
		GameManager.InputMgr.BindCall( eInputCommands.SELECTION4, 		"WeaponChange_3",	() => ChangeWeapon( 3, 0 ) );
		GameManager.InputMgr.BindCall( eInputCommands.SELECTION5, 		"WeaponChange_4",	() => ChangeWeapon( 4, 0 ) );
		GameManager.InputMgr.BindCall( eInputCommands.SELECTION6, 		"WeaponChange_5",	() => ChangeWeapon( 5, 0 ) );
		GameManager.InputMgr.BindCall( eInputCommands.SELECTION7,		"WeaponChange_6",	() => ChangeWeapon( 6, 0 ) );
		GameManager.InputMgr.BindCall( eInputCommands.SELECTION8,		"WeaponChange_7",	() => ChangeWeapon( 7, 0 ) );
		GameManager.InputMgr.BindCall( eInputCommands.SELECTION9,		"WeaponChange_8",	() => ChangeWeapon( 8, 0 ) );

		GameManager.InputMgr.BindCall( eInputCommands.SWITCH_NEXT,		"WeaponChange_Next",	() => ChangeWeapon( -1,  1 ) );
		GameManager.InputMgr.BindCall( eInputCommands.SWITCH_PREVIOUS,	"WeaponChange_Prev",	() => ChangeWeapon( -1, -1 ) );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave
	private				StreamUnit	OnSave( StreamData streamData )
	{
		StreamUnit streamUnit	= streamData.NewUnit( gameObject );

		streamUnit.SetInternal( "CurrentWeaponIndex",			CurrentWeaponIndex );
		streamUnit.SetInternal( "ZoomedIn",						m_ZoomedIn );
		streamUnit.SetInternal( "StartOffset",					Utils.Converters.Vector3ToString( m_StartOffset ) );
		streamUnit.SetInternal( "FinalOffset",					Utils.Converters.Vector3ToString( m_FinalOffset ) );
		streamUnit.SetInternal( "ZoomingTime",					m_ZoomingTime );
		streamUnit.SetInternal( "StartCameraFOV",				m_StartCameraFOV );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad
	private				StreamUnit	OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = null;
		streamData.GetUnit( gameObject, ref streamUnit );

		StopAllCoroutines();
		m_ZoomingTime					= 0f;
		m_IsChangingWpn					= false;

		// Current Weapon
		{
			DisableAll();

			CurrentWeaponIndex			= streamUnit.GetAsInt("CurrentWeaponIndex" );
			CurrentWeapon				= GetByIndex( CurrentWeaponIndex );
			CurrentWeapon.Transform.gameObject.SetActive( true );
			CurrentWeapon.Enabled		= true;
		}

		// Zoom
		{
			m_ZoomedIn					= streamUnit.GetAsBool( "ZoomedIn" );
			m_StartOffset				= streamUnit.GetAsVector( "StartOffset" );
			m_FinalOffset				= streamUnit.GetAsVector( "FinalOffset" );
			m_ZoomingTime				= streamUnit.GetAsFloat( "ZoomingTime" );
			m_StartCameraFOV			= streamUnit.GetAsFloat( "StartCameraFOV" );

			CameraControl.Instance.WeaponPivot.localPosition = ( m_ZoomedIn == true ) ? m_FinalOffset : m_StartOffset;

			float cameraFinalFov = m_StartCameraFOV / CurrentWeapon.ZoomFactor;
			Camera.main.fieldOfView = ( m_ZoomedIn == true ) ? cameraFinalFov : m_StartCameraFOV;
		}

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// ZoomIn
	public				void		ZoomIn()
	{
		if ( m_ZoomedIn == false && m_IsChangingZoom == false )
		{
			m_IsChangingZoom = true;
			StartCoroutine( ZoomInCO() );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ZoomIn
	public				void		ZoomOut()
	{
		if ( m_ZoomedIn == true && m_IsChangingZoom == false )
		{
			m_IsChangingZoom = true;
			StartCoroutine( ZoomOutCO() );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ZoomInCO ( Coroutine )
	public				IEnumerator	ZoomInCO()
	{
		m_StartOffset		= CameraControl.Instance.WeaponPivot.localPosition;
		m_StartCameraFOV	= CameraControl.Instance.MainCamera.fieldOfView;
		m_FinalOffset		= CurrentWeapon.ZoomOffset;
		m_ZoomingTime		= CurrentWeapon.ZoomingTime;

		yield return StartCoroutine( Internal_ZoomInCO() );
	}


	//////////////////////////////////////////////////////////////////////////
	// ZoomOutCO ( Coroutine )
	public				IEnumerator	ZoomOutCO()
	{
		if ( m_ZoomedIn == true )
		{
			yield return StartCoroutine( Internal_ZoomOutCO() );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ChangeWeaponRequest
	public				void		ChangeWeaponRequest( int wpnIdx )
	{
		if ( m_IsChangingWpn == true )
			return;

		if ( GetByIndex( CurrentWeaponIndex ).CanChangeWeapon() == false )
			return;

		StartCoroutine( ChangeWeaponCO( wpnIdx ) );
	}
	

	//////////////////////////////////////////////////////////////////////////
	// ChangeWeapon
	private				void		ChangeWeapon( int index, int versus = 0 )
	{
		if ( m_IsChangingWpn == true )
			return;

		// If same weapon index of current weapon
		if ( index == CurrentWeaponIndex )
		{
			// based on current weapon state choose action
			if ( CurrentWeapon.WeaponState == WeaponState.DRAWED )
				CurrentWeapon.Stash();
			else
				CurrentWeapon.Draw();

			return;
		}

		// Skip if cannot change the current weapon
		if ( CurrentWeapon.CanChangeWeapon() == false )
			return;

		// For a valid index
		if ( index > -1 && index < WeaponsList.Count )
		{
			// Start weapon change
			StartCoroutine( ChangeWeaponCO( index ) );
			return;
		}

		// if a versus is definded, find out next weapon index
		int lastWeapIdx = WeaponsList.Count - 1;
		int newWeaponIdx = CurrentWeaponIndex + versus;

		if ( newWeaponIdx == -1 )			newWeaponIdx = lastWeapIdx;
		if ( newWeaponIdx > lastWeapIdx )	newWeaponIdx = 0;

		// in case there is only a weapon available
		if ( newWeaponIdx == CurrentWeaponIndex )
			return;

		// Start weapon change
		StartCoroutine( ChangeWeaponCO( newWeaponIdx ) );
	}


	//////////////////////////////////////////////////////////////////////////
	// ChangeWeaponCO ( Coroutine )
	private				IEnumerator	ChangeWeaponCO( int newWeaponIdx )
	{
		m_IsChangingWpn = true;

		// Exit from zoom
		if ( m_ZoomedIn == true )
		{
			yield return StartCoroutine( ZoomOutCO() );
		}
		
		IWeapon currentWeapon	= GetByIndex( CurrentWeaponIndex );
		IWeapon nextWeapon		= GetByIndex( newWeaponIdx );

		// last event before changing
		currentWeapon.OnWeaponChange();		//  Weapon properties reset ( enable = false )

		// If weapon is drawed play stash animation
		if ( ( currentWeapon.WeaponState == WeaponState.DRAWED ) )
		{
			// Play stash animation and get animation time
			float stashTime			= CurrentWeapon.Stash();

			// wait for stash animation to terminate
			yield return new WaitForSeconds( stashTime );
		}

		// switch active object
		currentWeapon.Transform.gameObject.SetActive( false );
		nextWeapon.Transform.gameObject.SetActive( true );

		// set current weapon index and ref
		CurrentWeaponIndex		= newWeaponIdx;
		CurrentWeapon			= nextWeapon;

		// Play draw animation and get animation time
		float drawTime			= CurrentWeapon.Draw();
		yield return new WaitForSeconds( drawTime * 0.8f );

		// Update UI
		UI.Instance.InGame.UpdateUI();

		// weapon return active
		CurrentWeapon.Enabled	= true;
		m_IsChangingWpn			= false;
		m_ZoomSensitivity		= CurrentWeapon.ZoomSensitivity;
	}


	//////////////////////////////////////////////////////////////////////////
	// ZoomInCO ( Coroutine )
	private				IEnumerator	Internal_ZoomInCO()
	{
		float cameraFinalFov = m_StartCameraFOV / CurrentWeapon.ZoomFactor;
//		CurrentWeapon.Enabled = false;

		float	interpolant = 0f;
		float	currentTime = 0f;

		Transform weaponPivot = CameraControl.Instance.WeaponPivot;
		Camera mainCamera = CameraControl.Instance.MainCamera;

		// Transition
		while( interpolant < 1f )
		{
			currentTime					+= Time.deltaTime;
			interpolant					=  currentTime / m_ZoomingTime;
			weaponPivot.localPosition	= Vector3.Lerp( m_StartOffset, m_FinalOffset, interpolant );
			mainCamera.fieldOfView		= Mathf.Lerp( m_StartCameraFOV, cameraFinalFov, interpolant );
			yield return null;
		}

		CameraControl.Instance.HeadMove.AmplitudeMult /= CurrentWeapon.ZoomFactor;

//		CurrentWeapon.Enabled = true;
		m_ZoomedIn = true;
		m_IsChangingZoom = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// ZoomOut ( Coroutine )
	private				IEnumerator	Internal_ZoomOutCO()
	{
		float	cameraCurrentFov = Camera.main.fieldOfView;
//		CurrentWeapon.Enabled = false;

		float	interpolant = 0f;
		float	currentTime = 0f;

		Transform weaponPivot = CameraControl.Instance.WeaponPivot;
		Camera mainCamera = CameraControl.Instance.MainCamera;

		// Transition
		while( interpolant < 1f )
		{	
			currentTime					+= Time.deltaTime;
			interpolant					= currentTime / m_ZoomingTime;
			weaponPivot.localPosition	= Vector3.Lerp( m_FinalOffset, m_StartOffset, interpolant );
			mainCamera.fieldOfView		= Mathf.Lerp( cameraCurrentFov, m_StartCameraFOV, interpolant );
			yield return null;
		}

		CameraControl.Instance.HeadMove.AmplitudeMult *= CurrentWeapon.ZoomFactor;

//		CurrentWeapon.Enabled = true;
		m_ZoomedIn = false;
		m_IsChangingZoom = false;
	}

	
	//////////////////////////////////////////////////////////////////////////
	// OnDestroy
	private void OnDestroy()
	{
		Instance = null;
	}
	
}
