
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
}


public abstract class Weapon : MonoBehaviour, IWeapon {

	[Header("Weapon Properties")]

	[SerializeField]
	protected	GameObject				m_Bullet1GameObject			= null;

	[SerializeField]
	protected	GameObject				m_Bullet2GameObject			= null;

	[SerializeField]
	protected	Vector3					m_ZoomOffset				= Vector3.zero;

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
	}


	protected	IEnumerator	ZoomIn()
	{
		float	interpolant = 0f;
		bool	prevFirstFireAvaiable = m_FirstFireAvaiable;
		bool	prevSecondFireAvaiable = m_SecondFireAvaiable;
		float	cameraStartFov = Camera.main.fieldOfView;
		float	cameraFinalFov = cameraStartFov * 0.5f;
		m_StartOffset = CameraControl.Instance.WeaponPivot.localPosition;
		m_FirstFireAvaiable = false;
		m_InTransition = true;


		while( interpolant < 1f )
		{
			interpolant += Time.deltaTime * 2f;
			CameraControl.Instance.WeaponPivot.localPosition = Vector3.Lerp( m_StartOffset, m_ZoomOffset, interpolant );
			CameraControl.Instance.MainCamera.fieldOfView = Mathf.Lerp( cameraStartFov, cameraFinalFov, interpolant );
			yield return null;
		}

		m_FirstFireAvaiable = prevFirstFireAvaiable;
		m_SecondFireAvaiable = prevSecondFireAvaiable;
		m_InTransition = false;
		m_ZoomedIn = true;
	}


	protected	IEnumerator	ZoomOut()
	{
		float	interpolant = 0f;
		bool	prevFirstFireAvaiable = m_FirstFireAvaiable;
		bool	prevSecondFireAvaiable = m_SecondFireAvaiable;
		float	cameraStartFov = Camera.main.fieldOfView;
		float	cameraFinalFov = cameraStartFov * 2.0f;

		m_FirstFireAvaiable = false;
		m_InTransition = true;

		while( interpolant < 1f )
		{	
			interpolant += Time.deltaTime * 2f;
			CameraControl.Instance.WeaponPivot.localPosition = Vector3.Lerp( m_ZoomOffset, m_StartOffset, interpolant );
			CameraControl.Instance.MainCamera.fieldOfView = Mathf.Lerp( cameraStartFov, cameraFinalFov, interpolant );
			yield return null;
		}

		m_FirstFireAvaiable = prevFirstFireAvaiable;
		m_SecondFireAvaiable = prevSecondFireAvaiable;
		m_InTransition = false;
		m_ZoomedIn = false;
	}



}
