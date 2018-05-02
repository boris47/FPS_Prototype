
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
	bool					IsFiring			{ get; }
	float					Damage				{ get; }
	uint					Magazine			{ get; }
	uint					MagazineCapacity	{ get; }
	FireModes				FireMode			{ get; }
	Transform				FirePoint1			{ get; }
	Transform				FirePoint2			{ get; }
	IFlashLight				FlashLight			{ get; }
	float					CamDeviation		{ get; }
	float					FireDispersion		{ get; }
	float					SlowMotionCoeff		{ get; }
	bool					FirstFireAvaiable	{ get; }
	bool					SecondFireAvaiable	{ get; }
	float					ZommSensitivity		{ get; }
	float					ZoomFactor			{ get; }

	Animator				Animator			{ get; }

	bool					CanChangeWeapon		();
	void					OnWeaponChange		();
}




public abstract class Weapon : MonoBehaviour, IWeapon {

	[Header("Weapon Properties")]

	public static	IWeapon[]						Array						= null;

	[SerializeField]
	protected		GameObject						m_Bullet1GameObject			= null;

	[SerializeField]
	protected		GameObject						m_Bullet2GameObject			= null;

	[SerializeField]
	protected		Vector3							m_ZoomOffset				= Vector3.zero;

	[SerializeField]
	protected		float							m_Damage					= 5f;

	[SerializeField, ReadOnly]
	protected		uint							m_Magazine					= 27;

	[SerializeField]
	protected		uint							m_MagazineCapacity			= 27;

	[SerializeField]
	protected		FireModes						m_FireMode					= FireModes.AUTO;

	[SerializeField]
	protected		Transform						m_FirePointFirst			= null;

	[SerializeField]
	protected		Transform						m_FirePointSecond			= null;

	[SerializeField, Range( 1, 4 )]
	protected		uint							m_BrustSize					= 3;

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

	[SerializeField]
	protected		Renderer						m_Renderer					= null;

	[SerializeField]
	private			CustomAudioSource				m_AudioSourceFire1			= null;

	[SerializeField]
	private			CustomAudioSource				m_AudioSourceFire2			= null;


	protected		bool							m_FirstFireAvaiable			= true;
	protected		bool							m_SecondFireAvaiable		= true;
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
	float					IWeapon.Damage				{ get { return m_Damage; } }
	uint					IWeapon.Magazine			{ get { return m_Magazine; } }
	uint					IWeapon.MagazineCapacity	{ get { return m_MagazineCapacity; } }
	FireModes				IWeapon.FireMode			{ get { return m_FireMode; } }
	Transform				IWeapon.FirePoint1			{ get { return m_FirePointFirst; } }
	Transform				IWeapon.FirePoint2			{ get { return m_FirePointSecond; } }
	IFlashLight				IWeapon.FlashLight			{ get { return m_FlashLight; } }
	float					IWeapon.CamDeviation		{ get { return m_CamDeviation; } }
	float					IWeapon.FireDispersion		{ get { return m_FireDispersion; } }
	float					IWeapon.SlowMotionCoeff		{ get { return m_SlowMotionCoeff; } }
	bool					IWeapon.FirstFireAvaiable	{ get { return m_FirstFireAvaiable; } }
	bool					IWeapon.SecondFireAvaiable	{ get { return m_SecondFireAvaiable; } }
	float					IWeapon.ZommSensitivity		{ get { return m_ZommSensitivity; } }
	float					IWeapon.ZoomFactor			{ get { return m_ZoomFactor; } }
	// INTERFACE END

	
	protected		Animator						m_Animator					= null;
	public			Animator						Animator
	{
		get { return m_Animator; }
	}


	protected		float							m_LockTimer					= 0f;
	
	protected		AnimationClip					m_FireAnim					= null;
	protected		AnimationClip					m_ReloadAnim				= null;
	protected		AnimationClip					m_DrawAnim					= null;
	protected		string							m_SectionName				= "";
	protected		float							m_FireTimer					= 0f;

	protected		uint							m_BrustCount				= 0;
	protected		GameObjectsPool<Bullet>			m_PoolBulletsFirst			= null;
	protected		GameObjectsPool<Bullet>			m_PoolBulletsSecond			= null;
//	protected		float							m_AnimatorStdSpeed			= 1f;
	protected		bool							m_IsRecharging				= false;
	
	

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
		
		m_FlashLight = GetComponentInChildren<IFlashLight>();

		// animations
		m_Animator		= transform.GetComponent<Animator>();
		m_FireAnim		= m_Animator.GetClipFromAnimator( "fire" );
		m_ReloadAnim	= m_Animator.GetClipFromAnimator( "reload" );
		m_DrawAnim		= m_Animator.GetClipFromAnimator( "draw" );

		GameManager.Instance.OnSave += OnSave;
		GameManager.Instance.OnLoad += OnLoad;

		m_SectionName = this.GetType().FullName;

		// LOAD CONFIGURATION
		{
			CFG_Reader.Section section = null;
			GameManager.Configs.GetSection( m_SectionName, ref section );

			m_Damage			= section.AsFloat( "Damage", m_Damage );
			m_ZoomingTime		= section.AsFloat( "ZoomingTime", m_ZoomingTime );
			m_ZommSensitivity	= section.AsFloat( "ZommSensitivity", m_ZommSensitivity );
			m_ZoomFactor		= section.AsFloat( "ZoomFactor", m_ZoomFactor );
		}

		m_Magazine = m_MagazineCapacity;

		// BULLETS POOL CREATION
		{
			if ( m_Bullet1GameObject != null && m_FirstFireAvaiable )
			{
				m_PoolBulletsFirst = new GameObjectsPool<Bullet>
				(
					model			: ref m_Bullet1GameObject,
					size			: m_MagazineCapacity,
					containerName	: "RifleBulletsPoolFirst",
					permanent		: true,
					actionOnObject	: ( Bullet o ) =>
					{
						o.SetActive( false );
						o.Setup( damage : m_Damage, canPenetrate : false, whoRef : Player.Instance, weapon : this );
						Physics.IgnoreCollision( o.Collider, ( Player.Instance as IEntity ).PhysicCollider, ignore : true );
					}
				);
			}


			if ( m_Bullet2GameObject != null && m_SecondFireAvaiable )
			{
				m_PoolBulletsSecond = new GameObjectsPool<Bullet>
				(
					model			: ref m_Bullet2GameObject,
					size			: 10,
					containerName	: "RifleBulletsPoolSecond",
					permanent		: true,
					actionOnObject	: ( Bullet o ) =>
					{
						o.SetActive( false );
						o.Setup( whoRef : Player.Instance, weapon : this );
						Physics.IgnoreCollision( o.Collider, ( Player.Instance as IEntity ).PhysicCollider, ignore : true );
					}
				);
			}
		}
		m_LockTimer = m_DrawAnim.length;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnValidate
	protected	virtual	void	OnValidate()
	{
		m_ZoomingTime = Mathf.Max( m_ZoomingTime, 0.1f );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave
	protected	virtual	StreamingUnit	OnSave( StreamingData streamingData )
	{
		StreamingUnit streamingUnit		= new StreamingUnit();
		streamingUnit.InstanceID		= gameObject.GetInstanceID();
		streamingUnit.Name				= gameObject.name;

		streamingUnit.AddInternal( "Magazine = " + m_Magazine );
		streamingUnit.AddInternal( "Firemode = " + m_FireMode.ToString() );

		if ( m_FlashLight != null )
			streamingUnit.AddInternal( "FlashLightActive = " + m_FlashLight.Activated );

		streamingData.Data.Add( streamingUnit );
		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad
	protected	virtual	StreamingUnit	OnLoad( StreamingData streamingData )
	{
		int instanceID				= gameObject.GetInstanceID();
		StreamingUnit streamingUnit	= streamingData.Data.Find( ( StreamingUnit data ) => data.InstanceID == instanceID );
		if ( streamingUnit == null )
			return null;

		KeyValue[] internals = Utils.Base.GetKeyValues( streamingUnit.Internals );

		// MAGAZINE
		{
			uint magazine = m_MagazineCapacity;
			if ( uint.TryParse( internals[0].Value, out magazine ) )
			{
				m_Magazine = magazine;
			}
		}

		// FIREMODE
		{
			m_FireMode = ( FireModes ) System.Enum.Parse( typeof( FireModes ), internals[1].Value );
		}

		// FLASHLIGHT
		if ( m_FlashLight != null )
		{
			bool state = internals[2].Value.ToLower() == "true" ? true : false;
			m_FlashLight.SetActive( state );
		}

		UI.Instance.InGame.UpdateUI();
		return streamingUnit;
	}
	

	//////////////////////////////////////////////////////////////////////////
	// Update
	protected	virtual		void Update()
	{
		m_FireTimer -= Time.deltaTime;
		
		if ( InputManager.Inputs.ItemAction1 && m_InTransition == false && m_IsRecharging == false )
		{
			if ( WeaponManager.Instance.Zoomed == false )
				WeaponManager.Instance.ZoomIn( this, m_ZoomOffset, m_ZoomingTime );
			else
				WeaponManager.Instance.ZoomOut();
		}

		// Reloading
		if ( m_LockTimer > 0f )
		{
			m_LockTimer -= Time.deltaTime;
			return;
		}
		
		// Just after reload
		if ( m_LockTimer < 0f )
		{
//			anim.speed = m_AnimatorStdSpeed;
			m_LockTimer = 0f;
			if ( m_IsRecharging == true )
			{
				m_IsRecharging = false;
				m_Magazine = m_MagazineCapacity;
				UI.Instance.InGame.UpdateUI();
			}
			m_BrustCount = 0;
			m_NeedRecharge = false;
		}


		// Fire mode cycle
		if ( InputManager.Inputs.ItemAction2 )
		{
			if ( m_FireMode == FireModes.AUTO )
				m_FireMode = FireModes.SINGLE;
			else
				m_FireMode ++;

			UI.Instance.InGame.UpdateUI();
		}

		// End For Brust mode
		if ( m_BrustCount > 0 && InputManager.Inputs.Fire1Released )
		{
			m_BrustCount = 0;
		}

		if ( m_FireTimer > 0 )
			return;

		m_IsFiring = false;
		if ( m_FirstFireAvaiable && m_Magazine > 0 && m_InTransition == false && m_NeedRecharge == false )
		{
			switch ( m_FireMode )
			{
				case FireModes.SINGLE:
					FireSingleMode();
					break;
				case FireModes.BURST:
					FireBrustMode();
					break;
				case FireModes.AUTO:
					FireAutoMode();
					break;
			}
		}


		if ( Player.Instance.IsRunning && WeaponManager.Instance.Zoomed && m_InTransition == false )
		{
			WeaponManager.Instance.ZoomOut();
//			StartCoroutine( ZoomOut() );
		}

		if ( m_Magazine <= 0 || ( InputManager.Inputs.Reload && m_Magazine < m_MagazineCapacity ) || m_NeedRecharge )
		{
//			m_AnimatorStdSpeed = anim.speed;
//			anim.speed = 2f;

			if ( WeaponManager.Instance.Zoomed )
			{
				if ( m_InTransition == false )
				{
					WeaponManager.Instance.ZoomOut();
//					StartCoroutine( ZoomOut() );
					m_NeedRecharge = true;
				}
				return;
			}

			m_Animator.Play( m_ReloadAnim.name, -1, 0f );
			m_LockTimer = m_ReloadAnim.length; // / 2f;
			m_IsRecharging = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// CanChangeWeapon
	public	virtual	bool	CanChangeWeapon()
	{
		if ( m_InTransition == true )
			return false;

		if ( m_LockTimer > 0 )
			return false;

		return true;	
	}


	//////////////////////////////////////////////////////////////////////////
	// FireSingleMode
	public	virtual	void	OnWeaponChange()
	{
		m_IsRecharging	= false;
		m_NeedRecharge	= false;
		m_BrustCount	= 0;
		m_LockTimer		= 0f;
		m_FireTimer		= 0f;
		enabled			= false;
	}


	//////////////////////////////////////////////////////////////////////////
	// FireSingleMode
	protected	virtual		void	FireSingleMode()
	{
		if (  m_FireMode == FireModes.SINGLE && ( InputManager.Inputs.Fire1 || ( InputManager.Inputs.Fire2 && m_SecondFireAvaiable ) ) )
		{
			ConfigureShot( fireFirst : InputManager.Inputs.Fire1 );
			m_IsFiring = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FireBrustMode
	protected	virtual		void	FireBrustMode()
	{
		if ( m_FireMode == FireModes.BURST && ( InputManager.Inputs.Fire1Loop || ( InputManager.Inputs.Fire2 && m_SecondFireAvaiable ) ) && m_BrustCount < m_BrustSize )
		{
			if ( InputManager.Inputs.Fire1Loop )
				m_BrustCount ++;

			ConfigureShot( fireFirst : InputManager.Inputs.Fire1Loop );
			m_IsFiring = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FireAutoMode
	protected	virtual		void	FireAutoMode()
	{
		if ( m_FireMode == FireModes.AUTO && ( InputManager.Inputs.Fire1Loop || ( InputManager.Inputs.Fire2 && m_SecondFireAvaiable ) ) )
		{
			ConfigureShot( fireFirst : InputManager.Inputs.Fire1Loop );
			m_IsFiring = true;
		}
	}


	Vector3 dispVector = new Vector3 ();
	//////////////////////////////////////////////////////////////////////////
	// ConfigureShot
	protected	virtual		void	ConfigureShot( bool fireFirst )
	{
		if ( fireFirst )
			m_FireTimer = m_ShotDelay;
		else
			m_FireTimer = m_ShotDelay * 15f;

		m_Animator.Play( m_FireAnim.name, -1, 0f );
			
		m_Magazine --;

		// BULLET
		IBullet bullet = fireFirst ?  m_PoolBulletsFirst.GetComponent() : m_PoolBulletsSecond.GetComponent();

		// POSITION
		Vector3 position = fireFirst ? m_FirePointFirst.position : m_FirePointSecond.position;

		// DIRECTION
		dispVector.Set( Random.Range( -1f, 1f ), Random.Range( -1f, 1f ), Random.Range( -1f, 1f ) );
		dispVector /= WeaponManager.Instance.Zoomed ? m_ZoomFactor : 1f;

		Vector3 direction = fireFirst ? m_FirePointFirst.forward : m_FirePointSecond.forward;

		// AUDIOSOURCE
		ICustomAudioSource audioSource = ( fireFirst ) ? m_AudioSourceFire1 : m_AudioSourceFire2;

		// CAM DISPERSION
		float finalDispersion = m_CamDeviation * bullet.RecoilMult;
		finalDispersion	*= Player.Instance.IsCrouched			? 0.50f : 1.00f;
		finalDispersion	*= Player.Instance.IsMoving				? 1.50f : 1.00f;
		finalDispersion	*= Player.Instance.IsRunning			? 2.00f : 1.00f;
		finalDispersion *= ( m_FireMode == FireModes.SINGLE )	? 0.50f : 1.00f;
		finalDispersion *= ( m_FireMode == FireModes.BURST )	? 0.80f : 1.00f;
		finalDispersion *= ( m_FireMode == FireModes.AUTO )		? 1.10f : 1.00f;
		finalDispersion	*= WeaponManager.Instance.Zoomed		? 0.80f : 1.00f;

		// SHOOT
		Shoot( ref bullet, ref position, ref direction, ref audioSource, finalDispersion );

		// UPDATE UI
		UI.Instance.InGame.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	// Shoot
	protected	virtual		void	Shoot( ref IBullet bullet, ref Vector3 position, ref Vector3 direction, ref ICustomAudioSource audioSource, float camDispersion )
	{
		bullet.Shoot( position: position, direction: direction );
		audioSource.Play();
		CameraControl.Instance.ApplyDispersion( camDispersion );
	}

}
