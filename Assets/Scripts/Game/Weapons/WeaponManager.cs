using UnityEngine;
using System.Collections;

[System.Serializable]
public class WeaponManager : MonoBehaviour {
	
	
	public static	WeaponManager	Instance						= null;
	public WeaponManager	m_Instance
	{
		get { return Instance; }
	}

	private		bool				m_ZoomedIn						= false;
	public		bool				Zoomed							{ get { return m_ZoomedIn; } }
	public		IWeapon				CurrentWeapon					{ get; set; }
	public		int					CurrentWeaponIndex				{ get; set; }

	// ZOOMING
	private		Vector3				m_StartOffset					= Vector3.zero;
	private		Vector3				m_FinalOffset					= Vector3.zero;
	private		float				m_ZoomingTime					= 0f;
	private		float				m_StartCameraFOV				= 0f;

	// CHANGING WEAPON
	private		Coroutine			m_ChangingWpnCO					= null;
	public		bool				IsChangingWeapon				{ get { return m_ChangingWpnCO != null; } }




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
		Instance = this;
		DontDestroyOnLoad( this );
	}


	//////////////////////////////////////////////////////////////////////////
	// Start
	private				void	Start()
	{
		for ( int i = 0; i < CameraControl.Instance.WeaponPivot.childCount; i++ )
		{
			Transform weapon = CameraControl.Instance.WeaponPivot.GetChild( i );
			if ( i == CurrentWeaponIndex )
				continue;

			weapon.gameObject.SetActive( false );
		}

		// Set current weapon
		CurrentWeapon = Weapon.Array[ CurrentWeaponIndex ];

		// Enable current weapon
		CurrentWeapon.Enabled = true;

		// Make sure that ui show data of currnt active weapon
		UI.Instance.InGame.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	// ZoomIn
	public				void	ZoomIn( IWeapon weapon, Vector3 zoomOffset, float zoomingTime )
	{
		CurrentWeapon		= weapon;
		m_StartOffset		= CameraControl.Instance.WeaponPivot.localPosition;
		m_FinalOffset		= zoomOffset;
		m_ZoomingTime		= zoomingTime;
		m_StartCameraFOV	= CameraControl.Instance.MainCamera.fieldOfView;

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
	// ChangeWeapon
	public				void	ChangeWeapon( int versus )
	{
		if ( m_ChangingWpnCO != null )
			return;

		if ( Weapon.Array[ CurrentWeaponIndex ].CanChangeWeapon() == false )
			return;

		int lastWeapIdx = Weapon.Array.Length - 1;

		int tempIdx = versus + CurrentWeaponIndex;
		if ( tempIdx == -1 )
		{
			tempIdx = lastWeapIdx;
		}

		if ( tempIdx > lastWeapIdx )
		{
			tempIdx = 0;
		}

		m_ChangingWpnCO = StartCoroutine( ChangeWeaponCO( tempIdx ) );
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private void Update()
	{
		if ( m_ChangingWpnCO != null )
			return;

		if ( Weapon.Array[ CurrentWeaponIndex ].CanChangeWeapon() == false )
			return;

		if ( InputManager.Inputs.Selection1 )
			ChangeWeapon( 0 );

		if ( InputManager.Inputs.Selection2 )
			ChangeWeapon( 1 );

		if ( InputManager.Inputs.Selection3 )
			ChangeWeapon( 2 );

		// Weapon switch
		if ( InputManager.Inputs.SwitchPrev )
		{
			ChangeWeapon( -1 );
		}
		if ( InputManager.Inputs.SwitchNext )
		{
			ChangeWeapon( 1 );
		}

	}


	//////////////////////////////////////////////////////////////////////////
	// ChangeWeaponCO ( Coroutine )
	private				IEnumerator	ChangeWeaponCO( int wpnIndex )
	{
		// Play stash animation
		CurrentWeapon.Animator.Play( "stash", -1, 0.0f );

		// Exit from zoom
		if ( m_ZoomedIn == true )
		{
			StartCoroutine( ZoomOutCO() );
		}

		// time to wait before activate the other weapon
		float timeToWait = 2f;
		AnimationClip clip = CurrentWeapon.Animator.GetClipFromAnimator( "stash" );
		if ( clip != null )
			timeToWait = clip.length;

		// last event before changing
		Weapon.Array[ CurrentWeaponIndex ].OnWeaponChange();		//  Weapon properties reset ( enable = false )

		// wait for stash animation to terminate
		yield return new WaitForSeconds( timeToWait * 0.7f );

		// switch active object
		Weapon.Array[ CurrentWeaponIndex ].Transform.gameObject.SetActive( false );
		Weapon.Array[ wpnIndex ].Transform.gameObject.SetActive( true );

		// set current weapon index and ref
		CurrentWeaponIndex = wpnIndex;
		CurrentWeapon = Weapon.Array[ CurrentWeaponIndex ];

		// get draw animation clip
		clip = CurrentWeapon.Animator.GetClipFromAnimator( "draw" );
		if ( clip != null )
			timeToWait = clip.length;

		// Update UI
		UI.Instance.InGame.UpdateUI();

		// and wait its duraation
		yield return new WaitForSeconds( timeToWait * 0.8f );

		// weapon return active
		CurrentWeapon.Enabled = true;

		m_ChangingWpnCO = null;
	}


	//////////////////////////////////////////////////////////////////////////
	// ZoomInCO ( Coroutine )
	private				IEnumerator	ZoomInCO()
	{
		float cameraFinalFov = m_StartCameraFOV / CurrentWeapon.ZoomFactor;
		CurrentWeapon.Enabled = false;

		float	interpolant = 0f;
		float	currentTime = 0f;

		// Transition
		while( interpolant < 1f )
		{
			currentTime += Time.deltaTime;
			interpolant =  currentTime / m_ZoomingTime;
			CameraControl.Instance.WeaponPivot.localPosition	= Vector3.Lerp( m_StartOffset, m_FinalOffset, interpolant );
			CameraControl.Instance.MainCamera.fieldOfView		= Mathf.Lerp( m_StartCameraFOV, cameraFinalFov, interpolant );
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

		// Transition
		while( interpolant < 1f )
		{	
			currentTime += Time.deltaTime;
			interpolant = currentTime / m_ZoomingTime;
			CameraControl.Instance.WeaponPivot.localPosition	= Vector3.Lerp( m_FinalOffset, m_StartOffset, interpolant );
			CameraControl.Instance.MainCamera.fieldOfView		= Mathf.Lerp( cameraCurrentFov, m_StartCameraFOV, interpolant );
			yield return null;
		}

		CameraControl.Instance.HeadMove.AmplitudeMult *= CurrentWeapon.ZoomFactor;

		CurrentWeapon.Enabled = true;
		m_ZoomedIn = false;
	}

}
