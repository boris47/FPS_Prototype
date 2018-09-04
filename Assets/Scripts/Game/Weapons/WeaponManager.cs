using UnityEngine;
using System.Collections;

public interface IWeaponManager {

	GameObject			GameObject						{ get; }
	bool				Enabled							{ get; set; }

	bool				IsZoomed							{ get; }
	IWeapon				CurrentWeapon					{ get; set; }
	int					CurrentWeaponIndex				{ get; set; }
	bool				IsChangingWeapon				{ get; }

	void				ZoomIn							();
	void				ZoomOut							();
	void				ChangeWeaponRequest				( int wpnIdx );

}

[System.Serializable]
public class WeaponManager : MonoBehaviour, IWeaponManager {
	
	public static	IWeaponManager	Instance						= null;

	// INTERFACE START
	public			GameObject		GameObject						{ get { return gameObject; } }
	public			bool			Enabled							{ get { return enabled; } set { enabled = value; } }

	public			bool			IsZoomed							{ get { return m_ZoomedIn; } }
	public			IWeapon			CurrentWeapon					{ get; set; }
	public			int				CurrentWeaponIndex				{ get; set; }
	public			bool			IsChangingWeapon				{ get { return m_ChangingWpnCO != null; } }
	// INTERFACE END

	// ZOOMING
	private		bool				m_ZoomedIn						= false;
	private		Vector3				m_StartOffset					= Vector3.zero;
	private		Vector3				m_FinalOffset					= Vector3.zero;
	private		float				m_ZoomingTime					= 0f;
	private		float				m_StartCameraFOV				= 0f;

	// CHANGING WEAPON
	private		Coroutine			m_ChangingWpnCO					= null;




	//////////////////////////////////////////////////////////////////////////
	// Awake
	private				void	Awake()
	{
		if ( Instance != null )
		{
			print( "WeaponManager: Object set inactive" );
			gameObject.SetActive( false );
			return;
		}
		Instance = this as IWeaponManager;
		DontDestroyOnLoad( this );

		GameManager.Instance.OnSave += OnSave;
		GameManager.Instance.OnLoad += OnLoad;
	}


	//////////////////////////////////////////////////////////////////////////
	// Start
	private				void	Start()
	{
		Weapon.DisableAll();

		// Set current weapon
		CurrentWeapon = Weapon.Array[ CurrentWeaponIndex ];

		// Enable current weapon
		CurrentWeapon.Transform.gameObject.SetActive( true );
		CurrentWeapon.Enabled = true;
		CurrentWeapon.Draw();

		m_StartCameraFOV = Camera.main.fieldOfView;

		// Make sure that ui show data of currnt active weapon
		UI.Instance.InGame.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave
	private	StreamingUnit	OnSave( StreamingData streamingData )
	{
		StreamingUnit streamingUnit	= streamingData.NewUnit( gameObject );

		streamingUnit.AddInternal( "CurrentWeaponIndex",			CurrentWeaponIndex );
		streamingUnit.AddInternal( "ZoomedIn",						m_ZoomedIn );
		streamingUnit.AddInternal( "StartOffset",					Utils.Converters.Vector3ToString( m_StartOffset ) );
		streamingUnit.AddInternal( "FinalOffset",					Utils.Converters.Vector3ToString( m_FinalOffset ) );
		streamingUnit.AddInternal( "ZoomingTime",					m_ZoomingTime );
		streamingUnit.AddInternal( "StartCameraFOV",				m_StartCameraFOV );

		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad
	private	StreamingUnit	OnLoad( StreamingData streamingData )
	{
		StreamingUnit streamingUnit = null;
		streamingData.GetUnit( gameObject, ref streamingUnit );

		StopAllCoroutines();
		m_ZoomingTime					= 0f;
		m_ChangingWpnCO					= null;

		// Current Weapon
		{
			Weapon.DisableAll();

			CurrentWeaponIndex			= streamingUnit.GetAsInt("CurrentWeaponIndex" );
			CurrentWeapon				= Weapon.Array[ CurrentWeaponIndex ];
			CurrentWeapon.Transform.gameObject.SetActive( true );
			CurrentWeapon.Enabled		= true;
		}

		// Zoom
		{
			m_ZoomedIn					= streamingUnit.GetAsBool( "ZoomedIn" );
			m_StartOffset				= streamingUnit.GetAsVector( "StartOffset" );
			m_FinalOffset				= streamingUnit.GetAsVector( "FinalOffset" );
			m_ZoomingTime				= streamingUnit.GetAsFloat( "ZoomingTime" );
			m_StartCameraFOV			= streamingUnit.GetAsFloat( "StartCameraFOV" );

			CameraControl.Instance.WeaponPivot.localPosition = ( m_ZoomedIn == true ) ? m_FinalOffset : m_StartOffset;

			float cameraFinalFov = m_StartCameraFOV / CurrentWeapon.ZoomFactor;
			Camera.main.fieldOfView = ( m_ZoomedIn == true ) ? cameraFinalFov : m_StartCameraFOV;
		}

		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// ZoomIn
	public				void	ZoomIn()
	{
		if ( IsChangingWeapon == true )
			return;

		m_StartOffset		= CameraControl.Instance.WeaponPivot.localPosition;
		m_StartCameraFOV	= CameraControl.Instance.MainCamera.fieldOfView;
		m_FinalOffset		= CurrentWeapon.ZoomOffset;
		m_ZoomingTime		= CurrentWeapon.ZoomingTime;

		StartCoroutine( ZoomInCO() );
	}


	//////////////////////////////////////////////////////////////////////////
	// ZoomOut
	public				void	ZoomOut()
	{
		StartCoroutine( ZoomOutCO() );
	}


	//////////////////////////////////////////////////////////////////////////
	// ChangeWeaponRequest
	public				void	ChangeWeaponRequest( int wpnIdx )
	{
		if ( m_ChangingWpnCO != null )
			return;

		if ( Weapon.Array[ CurrentWeaponIndex ].CanChangeWeapon() == false )
			return;

		m_ChangingWpnCO = StartCoroutine( ChangeWeaponCO( wpnIdx ) );
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private				void	Update()
	{
		if ( m_ChangingWpnCO != null )
			return;

		if ( CurrentWeapon.CanChangeWeapon() == false )
			return;

		if ( InputManager.Inputs.Selection1 )	ChangeWeapon( 0 );
		if ( InputManager.Inputs.Selection2 )	ChangeWeapon( 1 );
		if ( InputManager.Inputs.Selection3 )	ChangeWeapon( 2 );
		if ( InputManager.Inputs.Selection4 )	ChangeWeapon( 3 );
		if ( InputManager.Inputs.Selection5 )	ChangeWeapon( 4 );
		if ( InputManager.Inputs.Selection6 )	ChangeWeapon( 5 );
		if ( InputManager.Inputs.Selection7 )	ChangeWeapon( 6 );
		if ( InputManager.Inputs.Selection8 )	ChangeWeapon( 7 );
		if ( InputManager.Inputs.Selection9 )	ChangeWeapon( 8 );

		// Weapon switch
		if ( InputManager.Inputs.SwitchPrev )
		{
			ChangeWeapon( -1, -1 );
		}
		if ( InputManager.Inputs.SwitchNext )
		{
			ChangeWeapon( -1, 1 );
		}

	}


	//////////////////////////////////////////////////////////////////////////
	// ChangeWeapon
	private				void	ChangeWeapon( int index, int versus = 0 )
	{
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

		int weaponsCount = Weapon.Array.Length;

		// For a valid index
		if ( index > -1 && index < weaponsCount && Weapon.Array[index] != null )
		{
			// Start weapon change
			m_ChangingWpnCO = StartCoroutine( ChangeWeaponCO( index ) );
			return;
		}

		// if a versus is definded, find out next weapon index
		int lastWeapIdx = weaponsCount - 1;
		int newWeaponIdx = CurrentWeaponIndex + versus;

		if ( newWeaponIdx == -1 )			newWeaponIdx = lastWeapIdx;
		if ( newWeaponIdx >  lastWeapIdx )	newWeaponIdx = 0;

		// in case there is only a weapon available
		if ( newWeaponIdx == CurrentWeaponIndex )
			return;

		// Start weapon change
		m_ChangingWpnCO = StartCoroutine( ChangeWeaponCO( newWeaponIdx ) );
	}


	//////////////////////////////////////////////////////////////////////////
	// ChangeWeaponCO ( Coroutine )
	private				IEnumerator	ChangeWeaponCO( int newWeaponIdx )
	{
		// Exit from zoom
		if ( m_ZoomedIn == true )
		{
			yield return StartCoroutine( ZoomOutCO() );
		}
		
		IWeapon currentWeapon	= Weapon.Array[ CurrentWeaponIndex ];
		IWeapon nextWeapon		= Weapon.Array[ newWeaponIdx ];

		// last event before changing
		currentWeapon.OnWeaponChange();		//  Weapon properties reset ( enable = false )

		// If weapon is drawed play stash animation
		if ( currentWeapon.WeaponState == WeaponState.DRAWED )
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

		// Update UI
		UI.Instance.InGame.UpdateUI();

		// weapon return active
		CurrentWeapon.Enabled	= true;
		m_ChangingWpnCO			= null;
	}


	//////////////////////////////////////////////////////////////////////////
	// ZoomInCO ( Coroutine )
	private				IEnumerator	ZoomInCO()
	{
		float cameraFinalFov = m_StartCameraFOV / CurrentWeapon.ZoomFactor;
		CurrentWeapon.Enabled = false;

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

		CurrentWeapon.Enabled = true;
		m_ZoomedIn = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// ZoomOut ( Coroutine )
	private				IEnumerator	ZoomOutCO()
	{
		float	cameraCurrentFov = Camera.main.fieldOfView;
		CurrentWeapon.Enabled = false;

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

		CurrentWeapon.Enabled = true;
		m_ZoomedIn = false;
	}

	/*
	//////////////////////////////////////////////////////////////////////////
	// OnDestroy
	private void OnDestroy()
	{
		Instance = null;
	}
	*/
}
