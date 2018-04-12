
using UnityEngine;
using System.Collections;

public enum FireModes {
	SINGLE,
	BURST,
	AUTO
}

public interface IWeapon {

	Transform				Transform			{ get; }
	float					Damage				{ get; }
	uint					Magazine			{ get; }
	uint					MagazineCapacity	{ get; }
	FireModes				FireMode			{ get; }
	Transform				FirePoint1			{ get; }
	Transform				FirePoint2			{ get; }
	float					SlowMotionCoeff		{ get; }
	bool					FirstFireAvaiable	{ get; }
	bool					SecondFireAvaiable	{ get; }

	void					ChangeWeapon( int versus );
}




public abstract class Weapon : MonoBehaviour, IWeapon {

	public	static	int					CurrentWeaponIndex			= 0;

	[Header("Weapon Properties")]

	[SerializeField]
	protected	GameObject				m_Bullet1GameObject			= null;

	[SerializeField]
	protected	GameObject				m_Bullet2GameObject			= null;

	[SerializeField]
	protected	Vector3					m_ZoomOffset				= Vector3.zero;

	[SerializeField]
	protected	AnimationClip			m_FireAnim					= null;

	[SerializeField]
	protected	AnimationClip			m_ReloadAnim				= null;

	[SerializeField]
	protected	AnimationClip			m_DrawAnim					= null;

	[SerializeField]
	protected	float					m_Damage					= 5f;

	[SerializeField]
	protected	uint					m_Magazine					= 27;

	[SerializeField]
	protected	uint					m_MagazineCapacity			= 27;

	[SerializeField]
	protected		FireModes			m_FireMode				= FireModes.AUTO;

	[SerializeField]
	protected	Transform				m_FirePointFirst			= null;

	[SerializeField]
	protected	Transform				m_FirePointSecond			= null;

	[SerializeField,Range(0.1f, 2f)]
	protected	float					m_SlowMotionCoeff			= 1f;

	[SerializeField]
	protected	float					m_ZoomingTime				= 1f;

	protected	bool					m_FirstFireAvaiable			= true;
	protected	bool					m_SecondFireAvaiable		= true;
	protected	bool					m_ZoomedIn					= false;
	protected	Vector3					m_StartOffset				= Vector3.zero;
	protected	bool					m_InTransition				= false;
	protected	bool					m_NeedRecharge				= false;

	// INTERFACE START
				Transform				IWeapon.Transform			{ get { return transform; } }
				float					IWeapon.Damage				{ get { return m_Damage; } }
				uint					IWeapon.Magazine			{ get { return m_Magazine; } }
				uint					IWeapon.MagazineCapacity	{ get { return m_MagazineCapacity; } }
				FireModes				IWeapon.FireMode			{ get { return m_FireMode; } }
				Transform				IWeapon.FirePoint1			{ get { return m_FirePointFirst; } }
				Transform				IWeapon.FirePoint2			{ get { return m_FirePointSecond; } }
				float					IWeapon.SlowMotionCoeff		{ get { return m_SlowMotionCoeff; } }
				bool					IWeapon.FirstFireAvaiable	{ get { return m_FirstFireAvaiable; } }
				bool					IWeapon.SecondFireAvaiable	{ get { return m_SecondFireAvaiable; } }
	// INTERFACE END


//	[SerializeField]
	protected		Animator			m_Animator					= null;
	protected		float				m_LockTimer					= 0f;

	private			Coroutine			m_ChangingWpnCO				= null;

	//////////////////////////////////////////////////////////////////////////
	// Awake ( Virtual )
	protected	virtual	void	Awake()
	{
		if ( m_Bullet1GameObject == null )
		{
			print( "Weapon " + name + " need a defined bullet to use " );
			m_FirstFireAvaiable  = false;
			enabled = false;
		}

		if ( m_FirePointFirst == null )
		{
			print( "Weapon " + name + " need a defined fire point for first bullet " );
			m_FirstFireAvaiable  = false;
			enabled = false;
		}


		if ( m_Bullet2GameObject == null || m_FirePointFirst == null )
		{
			m_SecondFireAvaiable = false;
		}

		m_Animator = transform.GetComponent<Animator>();
	}

	//////////////////////////////////////////////////////////////////////////
	// OnValidate
	private void OnValidate()
	{
		m_ZoomingTime = Mathf.Max( m_ZoomingTime, 0.1f );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnValidate ( Coroutine )
	protected	IEnumerator	ZoomIn()
	{
		bool	prevFirstFireAvaiable = m_FirstFireAvaiable;
		bool	prevSecondFireAvaiable = m_SecondFireAvaiable;
		float	cameraStartFov = Camera.main.fieldOfView;
		float	cameraFinalFov = cameraStartFov * 0.5f;
		m_StartOffset = CameraControl.Instance.WeaponPivot.localPosition;
		m_FirstFireAvaiable = false;
		m_InTransition = true;

		float	interpolant = 0f;
		float	currentTime = 0f;

		while( interpolant < 1f )
		{
			currentTime += Time.deltaTime;
			interpolant =  currentTime / m_ZoomingTime;
			CameraControl.Instance.WeaponPivot.localPosition	= Vector3.Lerp( m_StartOffset, m_ZoomOffset, interpolant );
			CameraControl.Instance.MainCamera.fieldOfView		= Mathf.Lerp( cameraStartFov, cameraFinalFov, interpolant );
			yield return null;
		}

		m_FirstFireAvaiable = prevFirstFireAvaiable;
		m_SecondFireAvaiable = prevSecondFireAvaiable;
		m_InTransition = false;
		m_ZoomedIn = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// ZoomOut ( Coroutine )
	protected	IEnumerator	ZoomOut()
	{
		bool	prevFirstFireAvaiable = m_FirstFireAvaiable;
		bool	prevSecondFireAvaiable = m_SecondFireAvaiable;
		float	cameraStartFov = Camera.main.fieldOfView;
		float	cameraFinalFov = cameraStartFov * 2.0f;

		m_FirstFireAvaiable = false;
		m_InTransition = true;

		float	interpolant = 0f;
		float	currentTime = 0f;

		while( interpolant < 1f )
		{	
			currentTime += Time.deltaTime;
			interpolant = currentTime / m_ZoomingTime;
			CameraControl.Instance.WeaponPivot.localPosition	= Vector3.Lerp( m_ZoomOffset, m_StartOffset, interpolant );
			CameraControl.Instance.MainCamera.fieldOfView		= Mathf.Lerp( cameraStartFov, cameraFinalFov, interpolant );
			yield return null;
		}

		m_FirstFireAvaiable = prevFirstFireAvaiable;
		m_SecondFireAvaiable = prevSecondFireAvaiable;
		m_InTransition = false;
		m_ZoomedIn = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// ChangeWeapon
	public	void	ChangeWeapon( int versus )
	{
		if ( m_ChangingWpnCO != null )
			return;

		Transform weaponPivot = CameraControl.Instance.transform.GetChild( 0 );
		int lastWeapIdx = weaponPivot.childCount - 1;

		int tempIdx = versus + CurrentWeaponIndex;
		if ( tempIdx == -1 )
		{
			tempIdx = lastWeapIdx;
		}

		if ( tempIdx > lastWeapIdx )
		{
			tempIdx = 0;
		}

		m_ChangingWpnCO = StartCoroutine( ChangeWeaponCO( weaponPivot, tempIdx ) );
	}


	//////////////////////////////////////////////////////////////////////////
	// ChangeWeaponCO ( Coroutine )
	private	IEnumerator	ChangeWeaponCO( Transform weaponPivot, int wpnIndex )
	{
		m_Animator.Play( "stash", -1, 0.0f );
		m_LockTimer = m_DrawAnim.length;

		yield return new WaitForSeconds( m_DrawAnim.length );

		weaponPivot.GetChild( wpnIndex ).gameObject.SetActive( true );
		weaponPivot.GetChild( CurrentWeaponIndex ).gameObject.SetActive( false );
		CurrentWeaponIndex = wpnIndex;
		m_ChangingWpnCO = null;
	}

}
