
using UnityEngine;


public interface IWeapon {

	Transform				Transform			{ get; }
	bool					Enabled				{ get; set; }
	bool					IsFiring			{ get; }
	float					Damage				{ get; }
	uint					Magazine			{ get; }
	uint					MagazineCapacity	{ get; }
	Transform				FirePoint			{ get; }
	IFlashLight				FlashLight			{ get; }
	float					CamDeviation		{ get; }
	float					FireDispersion		{ get; }
	float					SlowMotionCoeff		{ get; }
	float					ZommSensitivity		{ get; }
	float					ZoomFactor			{ get; }

	string					OtherInfo			 { get; }

	Animator				Animator			{ get; }

	bool					CanChangeWeapon		();
	void					OnWeaponChange		();
}




public abstract class Weapon : MonoBehaviour, IWeapon {

	[Header("Weapon Properties")]

	public static	IWeapon[]						Array						= null;

	[SerializeField]
	protected		float							m_MainDamage				= 2f;

	[SerializeField]
	protected		Vector3							m_ZoomOffset				= Vector3.zero;

	[SerializeField, ReadOnly]
	protected		uint							m_Magazine					= 27;

	[SerializeField]
	protected		uint							m_MagazineCapacity			= 27;

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

	[SerializeField]
	protected		float							m_ZoomingTime				= 1f;

	[SerializeField]
	protected		float							m_ZommSensitivity			= 1f;


	protected		Vector3							m_StartOffset				= Vector3.zero;
	protected		bool							m_InTransition				= false;
	protected		bool							m_NeedRecharge				= false;
	protected		float							m_ZoomFactor				= 1f;
	protected		bool							m_IsFiring					= false;
	protected		IFlashLight						m_FlashLight				= null;


	// INTERFACE START
	Transform				IWeapon.Transform			{ get { return transform; } }
	bool					IWeapon.Enabled				{ get { return enabled; } set { enabled = value; } }
	bool					IWeapon.IsFiring			{ get { return m_IsFiring; } }
	float					IWeapon.Damage				{ get { return m_MainDamage; } }
	uint					IWeapon.Magazine			{ get { return m_Magazine; } }
	uint					IWeapon.MagazineCapacity	{ get { return m_MagazineCapacity; } }
	Transform				IWeapon.FirePoint			{ get { return m_FirePoint; } }
	IFlashLight				IWeapon.FlashLight			{ get { return m_FlashLight; } }
	float					IWeapon.CamDeviation		{ get { return m_CamDeviation; } }
	float					IWeapon.FireDispersion		{ get { return m_FireDispersion; } }
	float					IWeapon.SlowMotionCoeff		{ get { return m_SlowMotionCoeff; } }
	float					IWeapon.ZommSensitivity		{ get { return m_ZommSensitivity; } }
	float					IWeapon.ZoomFactor			{ get { return m_ZoomFactor; } }
	string					IWeapon.OtherInfo			{ get { return OtherInfo; } }
	// INTERFACE END

	protected	abstract		string				OtherInfo { get; }
	
	
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

	protected		uint							m_BrustCount				= 0;
//	protected		float							m_AnimatorStdSpeed			= 1f;
	protected		bool							m_IsRecharging				= false;
	
	protected		Vector3							m_DispersionVector			= new Vector3 ();
	
	protected	delegate	void	FireFunction();



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
		
		m_FlashLight = GetComponentInChildren<IFlashLight>();

		// animations
		m_Animator		= transform.GetComponent<Animator>();
		m_FireAnim		= m_Animator.GetClipFromAnimator( "fire" );
		m_ReloadAnim	= m_Animator.GetClipFromAnimator( "reload" );
		m_DrawAnim		= m_Animator.GetClipFromAnimator( "draw" );

		m_AudioSourceFire = GetComponent<ICustomAudioSource>();

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
		m_LockTimer = m_DrawAnim.length;
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
		StreamingUnit streamingUnit		= new StreamingUnit();
		streamingUnit.InstanceID		= gameObject.GetInstanceID();
		streamingUnit.Name				= gameObject.name;

		streamingData.Data.Add( streamingUnit );
		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad ( Virtual )
	protected	virtual		StreamingUnit	OnLoad( StreamingData streamingData )
	{
		int instanceID				= gameObject.GetInstanceID();
		StreamingUnit streamingUnit	= streamingData.Data.Find( ( StreamingUnit data ) => data.InstanceID == instanceID );
		if ( streamingUnit == null )
			return null;
		
		UI.Instance.InGame.UpdateUI();
		return streamingUnit;
	}
	

	//////////////////////////////////////////////////////////////////////////
	// Update ( Abstract )
	protected	abstract	void			Update();

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
		m_BrustCount	= 0;
		m_LockTimer		= 0f;
		m_FireTimer		= 0f;
		enabled			= false;
	}
}
