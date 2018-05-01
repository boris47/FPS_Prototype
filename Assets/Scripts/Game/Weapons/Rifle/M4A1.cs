
using UnityEngine;

public class M4A1 : Weapon
{
	[Header("Rifle Properties")]

	[SerializeField]
	private		CustomAudioSource				m_AudioSourceFire1					= null;

	[SerializeField]
	private		CustomAudioSource				m_AudioSourceFire2					= null;

	[SerializeField]
	private		float							m_ShotDelay							= 0f;

	[SerializeField]
	private		float							m_CamDeviation						= 0.8f;

	[SerializeField]
	private		float							m_FireDispersion					= 0.05f;
	

	private		float							m_FireTimer							= 0f;
	private		uint							m_BrustCount						= 0;
	private		GameObjectsPool<Bullet>			m_PoolBulletsFirst					= null;
	private		GameObjectsPool<Bullet>			m_PoolBulletsSecond					= null;
//	private		float							m_AnimatorStdSpeed					= 1f;
	private		bool							m_IsRecharging						= false;
//	private		float							m_CrosshairMaxDisp					= 1f;


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override void	Awake()
	{
		m_SectionName = this.GetType().FullName;

		base.Awake();

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
					destroyModel	: false,
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
					destroyModel	: false,
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
//		DontDestroyOnLoad( this );
		m_LockTimer = m_DrawAnim.length;
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private		void Update()
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
	public	override		bool	CanChangeWeapon()
	{
		if ( m_InTransition == true )
			return false;

		if ( m_LockTimer > 0 )
			return false;

		return true;	
	}


	//////////////////////////////////////////////////////////////////////////
	// FireSingleMode
	public	override		void	OnWeaponChange()
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
	private		void	FireSingleMode()
	{
		if (  m_FireMode == FireModes.SINGLE && ( InputManager.Inputs.Fire1 || ( InputManager.Inputs.Fire2 && m_SecondFireAvaiable ) ) )
		{
			ConfigureShot( fireFirst : InputManager.Inputs.Fire1 );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FireBrustMode
	private		void	FireBrustMode()
	{
		if ( m_FireMode == FireModes.BURST && ( InputManager.Inputs.Fire1Loop || ( InputManager.Inputs.Fire2 && m_SecondFireAvaiable ) ) && m_BrustCount < m_BrustSize )
		{
			if ( InputManager.Inputs.Fire1Loop )
				m_BrustCount ++;

			ConfigureShot( fireFirst : InputManager.Inputs.Fire1Loop );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FireAutoMode
	private		void	FireAutoMode()
	{
		if ( m_FireMode == FireModes.AUTO && ( InputManager.Inputs.Fire1Loop || ( InputManager.Inputs.Fire2 && m_SecondFireAvaiable ) ) )
		{
			ConfigureShot( fireFirst : InputManager.Inputs.Fire1Loop );
		}
	}

	Vector3 dispVector = new Vector3 ();
	//////////////////////////////////////////////////////////////////////////
	// ConfigureShot
	private		void	ConfigureShot( bool fireFirst )
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

		Vector3 direction = fireFirst ? ( m_FirePointFirst.forward + dispVector * m_FireDispersion ).normalized : m_FirePointSecond.forward;

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
	private		void	Shoot( ref IBullet bullet, ref Vector3 position, ref Vector3 direction, ref ICustomAudioSource audioSource, float camDispersion )
	{
		bullet.Shoot( position: position, direction: direction );
		audioSource.Play();
		CameraControl.Instance.ApplyDispersion( camDispersion );
	}

}
