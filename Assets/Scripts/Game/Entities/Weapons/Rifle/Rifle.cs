
using UnityEngine;

public class Rifle : Weapon
{
	[Header("Rifle Properties")]

	[SerializeField]
	private		Animator						m_Animator							= null;

	[SerializeField]
	private		AnimationClip					m_FireAnim							= null;

	[SerializeField]
	private		AnimationClip					m_ReloadAnim						= null;

	[SerializeField]
	private		AnimationClip					m_DrawAnim							= null;

	[SerializeField]
	private		AudioSource						m_AudioSourceFire1					= null;

	[SerializeField]
	private		AudioSource						m_AudioSourceFire2					= null;

	[SerializeField]
	private		float							m_ShotDelay							= 0f;

	[SerializeField]
	private		float							m_CamDeviation						= 0.8f;

	[SerializeField]
	private		float							m_FireDispersion					= 0.05f;
	

	private		float							m_FireTimer							= 0f;
	private		float							m_LockTimer							= 0f;
	private		uint							m_BrustCount						= 0;
	private		GameObjectsPool<Bullet>			m_PoolBulletsFirst					= null;
	private		GameObjectsPool<Bullet>			m_PoolBulletsSecond					= null;
	private		float							m_AnimatorStdSpeed					= 1f;


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override void	Awake()
	{
		if ( m_FireAnim == null )
			Debug.LogError("Please assign a fire aimation in the inspector!");
		if ( m_ReloadAnim == null )
			Debug.LogError("Please assign a reload animation in the inspector!");
		if ( m_DrawAnim == null )
			Debug.LogError("Please assign a draw animation in the inspector!");

		base.Awake();

		// LOAD CONFIGURATION
		{
			CFG_Reader.Section section = null;
			GameManager.Configs.GetSection( "Rifle", ref section );

			m_Damage = section.AsFloat( "Damage", m_Damage );
		}

		m_Magazine = m_MagazineCapacity;

		// BULLETS POOL CREATION
		{
			if ( m_Bullet1GameObject != null && m_FirstFireAvaiable )
			{
				m_PoolBulletsFirst = new GameObjectsPool<Bullet>( ref m_Bullet1GameObject, m_MagazineCapacity, destroyModel : false, actionOnObject : ( Bullet o ) =>
				{
					o.SetActive( false );
					o.Setup( damage : m_Damage, canPenetrate : false, whoRef : Player.Instance, weapon : this );
					Physics.IgnoreCollision( o.Collider, ( Player.Instance as IEntity ).PhysicCollider, ignore : true );
				} );
				m_PoolBulletsFirst.ContainerName = "RifleBulletsPoolFirst";
			}


			if ( m_Bullet2GameObject != null && m_SecondFireAvaiable )
			{
				m_PoolBulletsSecond = new GameObjectsPool<Bullet>( ref m_Bullet2GameObject, 10, destroyModel : false, actionOnObject : ( Bullet o ) =>
				{
					o.SetActive( false );
					o.Setup( whoRef : Player.Instance, weapon : this );
					Physics.IgnoreCollision( o.Collider, ( Player.Instance as IEntity ).PhysicCollider, ignore : true );
				} );
				m_PoolBulletsSecond.ContainerName = "RifleBulletsPoolSecond";
			}

		}

		Player.Instance.CurrentWeapon = this;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private		void OnEnable()
	{
		UI_InGame.Instance.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private		void Update()
	{
		m_FireTimer -= Time.deltaTime;
		
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
			m_Magazine = m_MagazineCapacity;
			m_BrustCount = 0;
			m_NeedRecharge = false;
			UI_InGame.Instance.UpdateUI();
		}

		if ( InputManager.Inputs.ItemAction1 && m_InTransition == false )
		{
			if ( m_ZoomedIn == true )
				StartCoroutine( ZoomOut() );
			else
				StartCoroutine( ZoomIn() );
		}

		// Fire mode cycle
		if ( InputManager.Inputs.ItemAction2 )
		{
			if ( m_FireMode == FireModes.AUTO )
				m_FireMode = FireModes.SINGLE;
			else
				m_FireMode ++;

			UI_InGame.Instance.UpdateUI();
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


		if ( Player.Instance.IsRunning && m_ZoomedIn && m_InTransition == false )
		{
			StartCoroutine( ZoomOut() );
		}

		if ( m_Magazine <= 0 || InputManager.Inputs.Reload || m_NeedRecharge )
		{
//			m_AnimatorStdSpeed = anim.speed;
//			anim.speed = 2f;

			if ( m_ZoomedIn )
			{
				if ( m_InTransition == false )
				{
					StartCoroutine( ZoomOut() );
					m_NeedRecharge = true;
				}
				return;
			}

			m_Animator.Play( m_ReloadAnim.name, -1, 0f );
			m_LockTimer = m_ReloadAnim.length; // / 2f;
		}
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
		if ( m_FireMode == FireModes.BURST && ( InputManager.Inputs.Fire1Loop || ( InputManager.Inputs.Fire2 && m_SecondFireAvaiable ) ) && m_BrustCount < 3 )
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


	//////////////////////////////////////////////////////////////////////////
	// ConfigureShot
	private		void	ConfigureShot( bool fireFirst )
	{
		if ( fireFirst )
			m_FireTimer = m_ShotDelay;
		else
			m_FireTimer = m_ShotDelay * 15f;

//		if ( m_ZoomedIn == false )
			m_Animator.Play( m_FireAnim.name, -1, 0f );
			
		m_Magazine --;

		// BULLET
		IBullet bullet = fireFirst ?  m_PoolBulletsFirst.GetComponent() : m_PoolBulletsSecond.GetComponent();

		// POSITION
		Vector3 position = fireFirst ? m_FirePointFirst.position : m_FirePointSecond.position;

		// DIRECTION
		Vector3 dispersion = new Vector3 ( Random.Range( -1f, 1f ), Random.Range( -1f, 1f ), Random.Range( -1f, 1f ) ) * m_FireDispersion;
		Vector3 direction = fireFirst ? ( m_FirePointFirst.forward + dispersion ).normalized : m_FirePointSecond.forward;

		// AUDIOSOURCE
		AudioSource audioSource = ( fireFirst ) ? m_AudioSourceFire1 : m_AudioSourceFire2;

		// CAM DISPERSION
		float finalDispersion = m_CamDeviation * bullet.RecoilMult;
		finalDispersion	*= Player.Instance.IsCrouched			? 0.50f : 1.00f;
		finalDispersion	*= Player.Instance.IsMoving				? 1.50f : 1.00f;
		finalDispersion	*= Player.Instance.IsRunning			? 2.00f : 1.00f;
		finalDispersion *= ( m_FireMode == FireModes.SINGLE )	? 0.50f : 1.00f;
		finalDispersion *= ( m_FireMode == FireModes.BURST )	? 0.80f : 1.00f;
		finalDispersion *= ( m_FireMode == FireModes.AUTO )		? 1.10f : 1.00f;
		finalDispersion	*= ( m_ZoomedIn == true )				? 0.80f : 1.00f;

		// SHOOT
		Shoot( ref bullet, ref position, ref direction, ref audioSource, finalDispersion );

		// UPDATE UI
		UI_InGame.Instance.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	// Shoot
	private		void	Shoot( ref IBullet bullet, ref Vector3 position, ref Vector3 direction, ref AudioSource audioSource, float camDispersion )
	{
		bullet.Shoot( position: position, direction: direction );
		audioSource.Play();
		CameraControl.Instance.ApplyDispersion( camDispersion );
	}


}
