
using UnityEngine;

public enum WeaponState {
	DRAWED, STASHED
}

public interface IWeaponZoom {

	Vector3					ZoomOffset			 { get; }
	float					ZoomingTime			 { get; }
	float					ZommSensitivity		 { get; }
	float					ZoomFactor			{ get; }
}

public interface IWeapon :  IWeaponZoom {

	Transform				Transform			{ get; }
	bool					Enabled				{ get; set; }
	WeaponState				WeaponState			{ get; }
	bool					IsFiring			{ get; }
	float					Damage				{ get; }
	uint					Magazine			{ get; }
	uint					MagazineCapacity	{ get; }
	Transform				FirePoint			{ get; }
	IFlashLight				FlashLight			{ get; }
	float					CamDeviation		{ get; }
	float					FireDispersion		{ get; }
	float					SlowMotionCoeff		{ get; }

	string					OtherInfo			 { get; }

	Animator				Animator			{ get; }

	bool					CanChangeWeapon		();
	void					OnWeaponChange		();
	float					Draw();
	float					Stash();
}



public abstract class Weapon : MonoBehaviour, IWeapon {

	[Header("Weapon Properties")]

	public static	IWeapon[]						Array						= null;

	[SerializeField]
	protected		float							m_MainDamage				= 2f;

	[SerializeField]
	protected		Vector3							m_ZoomOffset				= Vector3.zero;

	[SerializeField]
	protected		float							m_ZoomingTime				= 1f;

	[SerializeField]
	protected		float							m_ZommSensitivity			= 1f;

	[SerializeField, ReadOnly]
	protected		uint							m_Magazine					= 1;

	[SerializeField]
	protected		uint							m_MagazineCapacity			= 1;

	[SerializeField]
	protected		Transform						m_FirePoint					= null;

	[SerializeField]
	protected		float							m_ShotDelay					= 0f;

	[SerializeField]
	protected		float							m_CamDeviation				= 0.8f;

	[SerializeField]
	protected		float							m_FireDispersion			= 0.05f;

	[SerializeField,Range(0.1f, 2f)]
	protected		float							m_SlowMotionCoeff			= 1f;



	protected		WeaponState						m_WeaponState				= WeaponState.STASHED;
	protected		Vector3							m_StartOffset				= Vector3.zero;
	protected		bool							m_InTransition				= false;
	protected		bool							m_NeedRecharge				= false;
	protected		float							m_ZoomFactor				= 1f;
	protected		bool							m_IsFiring					= false;
	protected		IFlashLight						m_FlashLight				= null;
	protected		Laser							m_Laser						= null;

	protected	abstract		string				OtherInfo { get; }

	// INTERFACE START
	Transform				IWeapon.Transform			{ get { return transform; } }
	bool					IWeapon.Enabled				{ get { return enabled; } set { enabled = value; } }
	bool					IWeapon.IsFiring			{ get { return m_IsFiring; } }
	float					IWeapon.Damage				{ get { return m_MainDamage; } }
	uint					IWeapon.Magazine			{ get { return m_Magazine; } }
	uint					IWeapon.MagazineCapacity	{ get { return m_MagazineCapacity; } }
	Transform				IWeapon.FirePoint			{ get { return m_FirePoint; } }
	IFlashLight				IWeapon.FlashLight			{ get { return m_FlashLight; } }
	WeaponState				IWeapon.WeaponState			{ get { return m_WeaponState; } }
	float					IWeapon.CamDeviation		{ get { return m_CamDeviation; } }
	float					IWeapon.FireDispersion		{ get { return m_FireDispersion; } }
	float					IWeapon.SlowMotionCoeff		{ get { return m_SlowMotionCoeff; } }
	string					IWeapon.OtherInfo			{ get { return OtherInfo; } }

	Vector3					IWeaponZoom.ZoomOffset		{ get { return m_ZoomOffset; } }
	float					IWeaponZoom.ZoomingTime		{ get { return m_ZoomingTime; } }
	float					IWeaponZoom.ZommSensitivity	{ get { return m_ZommSensitivity; } }
	float					IWeaponZoom.ZoomFactor		{ get { return m_ZoomFactor; } }
	// INTERFACE END
	
	protected		Animator						m_Animator					= null;
	public			Animator						Animator
	{
		get { return m_Animator; }
	}

	protected		ICustomAudioSource				m_AudioSourceFire			= null;


	protected		float							m_LockTimer					= 0f;
	
	protected		AnimationClip					m_FireAnim					= null;
	protected		AnimationClip					m_ReloadAnim				= null;
	protected		AnimationClip					m_DrawAnim					= null;
	protected		string							m_SectionName				= "";
	protected		float							m_FireTimer					= 0f;

//	protected		float							m_AnimatorStdSpeed			= 1f;
	protected		bool							m_IsRecharging				= false;
	
	protected		Vector3							m_DispersionVector			= new Vector3 ();
	
	protected	delegate	void	FireFunction();


	public static void	DisableAll()
	{
		System.Array.ForEach( Array, ( IWeapon w ) => {  w.Enabled = false; w.Transform.gameObject.SetActive( false ); } );
	}



	//////////////////////////////////////////////////////////////////////////
	// Awake ( Virtual )
	protected	virtual	void				Awake()
	{
		// Create weapons list
		if ( Array == null )
		{
			Array = new IWeapon[ CameraControl.Instance.WeaponPivot.childCount ];
		}

		// Assign this weapon in the list
		Array[ transform.GetSiblingIndex() ] = this;
		
		m_FlashLight = GetComponentInChildren<FlashLight>() as IFlashLight;

		// animations
		m_Animator		= transform.GetComponent<Animator>();
		m_FireAnim		= m_Animator.GetClipFromAnimator( "fire" );
		m_ReloadAnim	= m_Animator.GetClipFromAnimator( "reload" );
		m_DrawAnim		= m_Animator.GetClipFromAnimator( "draw" );

		// laser
		m_Laser			= transform.GetComponentInChildren<Laser>();

		m_AudioSourceFire = GetComponent<CustomAudioSource>() as ICustomAudioSource;

		GameManager.Instance.OnSave += OnSave;
		GameManager.Instance.OnLoad += OnLoad;

		m_SectionName = this.GetType().FullName;

		// LOAD CONFIGURATION
		{
			CFG_Reader.Section section = null;
			if ( GameManager.Configs.GetSection( m_SectionName, ref section ) )
			{
				m_MainDamage		= section.AsFloat( "Damage", m_MainDamage );
				m_ZoomingTime		= section.AsFloat( "ZoomingTime", m_ZoomingTime );
				m_ZommSensitivity	= section.AsFloat( "ZommSensitivity", m_ZommSensitivity );
				m_ZoomFactor		= section.AsFloat( "ZoomFactor", m_ZoomFactor );
			}
		}

		m_Magazine = m_MagazineCapacity;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnValidate ( Virtual )
	protected	virtual	void				OnValidate()
	{
		m_ZoomingTime = Mathf.Max( m_ZoomingTime, 0.1f );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave ( Virtual )
	protected	virtual		StreamingUnit	OnSave( StreamingData streamingData )
	{
		StreamingUnit streamingUnit	= streamingData.NewUnit( gameObject );

		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad ( Virtual )
	protected	virtual		StreamingUnit	OnLoad( StreamingData streamingData )
	{
		m_Animator.Play( "draw", -1, 0.99f );

		StreamingUnit streamingUnit = null;
		if ( streamingData.GetUnit( gameObject, ref streamingUnit ) == false )
			return null;
		
		m_InTransition = false;
		m_NeedRecharge = false;
		m_IsFiring = false;

		UI.Instance.InGame.UpdateUI();
		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// CanChangeWeapon ( Virtual )
	public		virtual		bool			CanChangeWeapon()
	{
		if ( m_InTransition == true )
			return false;

		if ( m_LockTimer > 0 )
			return false;

		return true;	
	}


	// Called before weapon change
	//////////////////////////////////////////////////////////////////////////
	// FireSingleMode ( Virtual )
	public		virtual		void			OnWeaponChange()
	{
		m_IsRecharging	= false;
		m_NeedRecharge	= false;
		m_LockTimer		= 0f;
		m_FireTimer		= 0f;
		enabled			= false;
	}


	//////////////////////////////////////////////////////////////////////////
	// Draw ( virtual )
	public		virtual		float			Draw()
	{
		m_Animator.Play( "draw", -1, 0f );
		m_LockTimer		= m_DrawAnim.length;
		m_WeaponState	= WeaponState.DRAWED;
		return m_LockTimer;
	}


	//////////////////////////////////////////////////////////////////////////
	// Stash ( virtual )
	public		virtual		float			Stash()
	{
		m_Animator.Play( "stash", -1, 0f );
		m_LockTimer		= m_DrawAnim.length;
		m_WeaponState	= WeaponState.STASHED;
		return m_LockTimer;
	}


	//////////////////////////////////////////////////////////////////////////
	// Update ( Abstract )
	protected	abstract	void			Update();

}
