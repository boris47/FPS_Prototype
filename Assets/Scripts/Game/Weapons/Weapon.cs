
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
	Transform				FirePoint			{ get; }
	IFlashLight				FlashLight			{ get; }
	float					CamDeviation		{ get; }
	float					FireDispersion		{ get; }
	float					SlowMotionCoeff		{ get; }
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
	protected		Bullet							m_Bullet1					= null;

	[SerializeField]
	protected		Bullet							m_Bullet2					= null;

	[SerializeField]
	protected		float							m_MainDamage				= 2f;

	[SerializeField]
	protected		Vector3							m_ZoomOffset				= Vector3.zero;

	[SerializeField, ReadOnly]
	protected		uint							m_Magazine					= 27;

	[SerializeField]
	protected		uint							m_MagazineCapacity			= 27;

	[SerializeField]
	protected		FireModes						m_FireMode					= FireModes.AUTO;

	[SerializeField]
	protected		Transform						m_FirePoint					= null;

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
	protected		CustomAudioSource				m_AudioSourceFire1			= null;

	[SerializeField]
	protected		CustomAudioSource				m_AudioSourceFire2			= null;

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
	FireModes				IWeapon.FireMode			{ get { return m_FireMode; } }
	Transform				IWeapon.FirePoint			{ get { return m_FirePoint; } }
	IFlashLight				IWeapon.FlashLight			{ get { return m_FlashLight; } }
	float					IWeapon.CamDeviation		{ get { return m_CamDeviation; } }
	float					IWeapon.FireDispersion		{ get { return m_FireDispersion; } }
	float					IWeapon.SlowMotionCoeff		{ get { return m_SlowMotionCoeff; } }
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
	protected		GameObjectsPool<Bullet>			m_PoolBullets1				= null;
	protected		GameObjectsPool<Bullet>			m_PoolBullets2				= null;
//	protected		float							m_AnimatorStdSpeed			= 1f;
	protected		bool							m_IsRecharging				= false;
	
	protected		Vector3							m_DispersionVector			= new Vector3 ();
	
	private	delegate	void	FireFunction();
	private			FireFunction					m_FireFunction				= null;



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

		if ( m_Bullet1 == null )
		{
			print( "Weapon " + name + " need a defined bullet to use " );
			enabled = false;
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

			m_MainDamage		= section.AsFloat( "Damage", m_MainDamage );
			m_ZoomingTime		= section.AsFloat( "ZoomingTime", m_ZoomingTime );
			m_ZommSensitivity	= section.AsFloat( "ZommSensitivity", m_ZommSensitivity );
			m_ZoomFactor		= section.AsFloat( "ZoomFactor", m_ZoomFactor );
		}

		m_Magazine = m_MagazineCapacity;
		SelectFireFunction();

		// BULLETS POOL CREATION
		{
			if ( m_Bullet1 != null )
			{
				GameObject bulletGO = m_Bullet1.gameObject;
				m_PoolBullets1 = new GameObjectsPool<Bullet>
				(
					model			: m_Bullet1.gameObject,
					size			: m_MagazineCapacity,
					containerName	: "RifleBulletsPool1",
					permanent		: true,
					actionOnObject	: ( Bullet o ) =>
					{
						o.SetActive( false );
						o.Setup( damage : m_MainDamage, canPenetrate : false, whoRef : Player.Instance, weapon : this );
						Physics.IgnoreCollision( o.Collider, ( Player.Instance as IEntity ).PhysicCollider, ignore : true );
					}
				);
			}
			/*
			if ( m_Bullet2 != null )
			{
				m_PoolBullets2 = new GameObjectsPool<Bullet>
				(
					model			: m_Bullet2.gameObject,
					size			: m_MagazineCapacity,
					containerName	: "RifleBulletsPool2",
					permanent		: true,
					actionOnObject	: ( Bullet o ) =>
					{
						o.SetActive( false );
						o.Setup( damage : m_MainDamage, canPenetrate : false, whoRef : Player.Instance, weapon : this );
						Physics.IgnoreCollision( o.Collider, ( Player.Instance as IEntity ).PhysicCollider, ignore : true );
					}
				);
			}
			*/
		}
		m_LockTimer = m_DrawAnim.length;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnValidate ( Virtual )
	protected	virtual	void				OnValidate()
	{
		m_ZoomingTime = Mathf.Max( m_ZoomingTime, 0.1f );
	}


	//////////////////////////////////////////////////////////////////////////
	// SelectFireFunction
	protected	virtual		void			SelectFireFunction()
	{
		switch ( m_FireMode )
		{
			case FireModes.SINGLE:
				m_FireFunction = OnFireSingleMode;
				break;
			case FireModes.BURST:
				m_FireFunction = OnFireBrustMode;
				break;
			case FireModes.AUTO:
				m_FireFunction = OnFireAutoMode;
				break;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave ( Virtual )
	protected	virtual		StreamingUnit	OnSave( StreamingData streamingData )
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
	// OnLoad ( Virtual )
	protected	virtual		StreamingUnit	OnLoad( StreamingData streamingData )
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
			SelectFireFunction();
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
	// OnEndReload ( Virtual )
	protected	virtual		void			OnEndReload()
	{
		m_Magazine = m_MagazineCapacity;
		UI.Instance.InGame.UpdateUI();
	}
	

	//////////////////////////////////////////////////////////////////////////
	// Update ( Virtual )
	protected	virtual		void			Update()
	{
		m_FireTimer -= Time.deltaTime;
		
		if ( Player.Instance.ChosingDodgeRotation == true )
			return;
		
		// Secondary fire
		if ( InputManager.Inputs.Fire2 && m_InTransition == false && m_IsRecharging == false )
		{
			OnSecondaryFire();
			return;
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
				OnEndReload();
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
			SelectFireFunction();
		}

		if ( m_FireTimer > 0 )
			return;

		m_IsFiring = false;
		if ( m_Magazine > 0 && m_InTransition == false && m_NeedRecharge == false )
		{
			m_FireFunction();
		}


		if ( Player.Instance.IsRunning && WeaponManager.Instance.Zoomed && m_InTransition == false )
		{
			WeaponManager.Instance.ZoomOut();
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
	// CanChangeWeapon ( Abstract )
	protected	abstract	void			OnSecondaryFire();


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


	//////////////////////////////////////////////////////////////////////////
	// FireSingleMode ( Abstract )
	protected	abstract	void			OnFireSingleMode();


	//////////////////////////////////////////////////////////////////////////
	// FireBrustMode ( Abstract )
	protected	abstract	void			OnFireBrustMode();


	//////////////////////////////////////////////////////////////////////////
	// FireAutoMode ( Abstract )
	protected	abstract	void			OnFireAutoMode();


	//////////////////////////////////////////////////////////////////////////
	// ConfigureShot ( Abstract )
	protected	abstract	void			ConfigureShot();


	//////////////////////////////////////////////////////////////////////////
	// Shoot ( Abstract )
	protected	abstract	void			Shoot( IBullet bullet, Vector3 position, Vector3 direction, ICustomAudioSource audioSource, float camDispersion );

}
