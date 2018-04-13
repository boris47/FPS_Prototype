
using UnityEngine;
using System.Collections;

public enum FireModes {
	SINGLE,
	BURST,
	AUTO
}

public interface IWeapon {

	Transform				Transform			{ get; }
	bool					Enabled				{ get; set; }
	float					Damage				{ get; }
	uint					Magazine			{ get; }
	uint					MagazineCapacity	{ get; }
	FireModes				FireMode			{ get; }
	Transform				FirePoint1			{ get; }
	Transform				FirePoint2			{ get; }
	float					SlowMotionCoeff		{ get; }
	bool					FirstFireAvaiable	{ get; }
	bool					SecondFireAvaiable	{ get; }

	Animator				Animator			{ get; }

	bool					CanChangeWeapon		();
	void					OnWeaponChange		();
}




public abstract class Weapon : MonoBehaviour, IWeapon {

	[Header("Weapon Properties")]

	public static	IWeapon[]			Array						= null;

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
				bool					IWeapon.Enabled				{ get { return enabled; } set { enabled = value; } }
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


	protected	Animator				m_Animator					= null;
	public		Animator				Animator
	{
		get { return m_Animator; }
	}


	protected	float					m_LockTimer					= 0f;
	
	protected	AnimationClip			m_FireAnim					= null;
	protected	AnimationClip			m_ReloadAnim				= null;
	protected	AnimationClip			m_DrawAnim					= null;

	

	//////////////////////////////////////////////////////////////////////////
	// Awake ( Virtual )
	protected	virtual	void	Awake()
	{
		// Create weapons list
		if ( Array == null )
		{
			Array = new IWeapon[ CameraControl.Instance.WeaponPivot.childCount ];
		}

		// Assign this weapon in the list
		Array[ transform.GetSiblingIndex() ] = this;

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
		
		// animations
		m_Animator		= transform.GetComponent<Animator>();
		m_FireAnim		= m_Animator.GetClipFromAnimator( "fire" );
		m_ReloadAnim	= m_Animator.GetClipFromAnimator( "reload" );
		m_DrawAnim		= m_Animator.GetClipFromAnimator( "draw" );
	}

	//////////////////////////////////////////////////////////////////////////
	// OnValidate
	private void OnValidate()
	{
		m_ZoomingTime = Mathf.Max( m_ZoomingTime, 0.1f );
	}


	public	abstract	bool	CanChangeWeapon();


	public	abstract	void	OnWeaponChange();

}
