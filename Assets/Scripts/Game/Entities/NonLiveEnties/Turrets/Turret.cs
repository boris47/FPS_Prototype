
using UnityEngine;


public abstract class Turret : NonLiveEntity {

	[Header("Turret Properties")]

	[SerializeField]
	private		Bullet			m_Bullet					= null;

	[SerializeField]
	protected	float			m_ShotDelay					= 0.7f;

	[SerializeField]
	protected	float			m_DamageMax					= 2f;

	[SerializeField]
	protected	float			m_DamageMin					= 0.5f;

	[SerializeField, ReadOnly]
	protected	int				m_PoolSize					= 5;

	private		Laser			m_Laser						= null;


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	void	Awake()
	{
		base.Awake();

		// LOAD CONFIGURATION
		{
			GameManager.Configs.GetSection( m_SectionName, ref m_SectionRef );
			if ( m_SectionRef == null )
			{
				print( "Cannot find cfg section for entity " + name );
				Destroy( gameObject );
				return;
			}

			m_Health				= m_SectionRef.AsFloat( "Health", 60.0f );

			float shieldStatus		= m_SectionRef.AsFloat( "Shield", 0.0f );
			if ( m_Shield != null )
				( m_Shield as IShield ).Status = shieldStatus;

			m_DamageMax				= m_SectionRef.AsFloat( "DamageMax", 2.0f );
			m_DamageMin				= m_SectionRef.AsFloat( "DamageMin", 0.5f );
			m_PoolSize				= m_SectionRef.AsInt( "PoolSize", m_PoolSize );

			m_EntityType			= ENTITY_TYPE.ROBOT;
		}

		m_Laser = GetComponentInChildren<Laser>();
		if ( m_Laser != null )
		{
			m_Laser.LaserLength = m_Brain.FieldOfView.Distance;
			m_Laser.LayerMaskToExclude = LayerMask.NameToLayer("Bullets");
		}
		// BULLETS POOL CREATION
		{
			GameObject	bulletGO		= m_Bullet.gameObject;
			m_Pool = new GameObjectsPool<Bullet>
			(
				model			: bulletGO,
				size			: ( uint ) m_PoolSize,
				containerName	: name + "BulletPool",
				actionOnObject	: ( Bullet o ) =>
				{
					o.SetActive( false );
					o.Setup( damageMin : m_DamageMin, damageMax : m_DamageMax, canPenetrate : false, whoRef : this, weapon : null );
					this.SetCollisionStateWith( o.Collider, false );

					// this allow to receive only trigger enter callback
					Player.Instance.DisableCollisionsWith( o.Collider );
				}
			);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Override )
	public		override	void	OnHit( IBullet bullet )
	{
		// Avoid friendly fire
		if ( bullet.WhoRef is NonLiveEntity )
			return;
		
		base.OnHit( bullet ); // set start bullet position as point to face at if not attacking

		if ( m_Shield != null && m_Shield.Status > 0f )
		{
			if ( m_Shield.IsUnbreakable == false )
			{
				m_Shield.OnHit( bullet );
			}
			if ( bullet.CanPenetrate == false )
				return;
		}

		float damage = Random.Range( bullet.DamageMin, bullet.DamageMax );
		m_Health -= damage;

		if ( m_Health <= 0f )
			OnKill();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Override )
	public		override	void	OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
	{
		// Avoid friendly fire
		if ( whoRef is NonLiveEntity )
			return;
		
		base.OnHit( startPosition, whoRef, 0f ); // set start bullet position as point to face at if not attacking

		if ( m_Shield != null && m_Shield.Status > 0f )
		{
			if ( m_Shield.IsUnbreakable == false )
			{
				m_Shield.OnHit( damage );
			}
			if ( canPenetrate == false )
				return;
		}

		m_Health -= damage;

		if ( m_Health <= 0f )
			OnKill();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetAquired ( Override )
	public		override	void	OnTargetAquired( TargetInfo_t targetInfo )
	{
		base.OnTargetAquired( targetInfo );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetChanged ( Override )
	public		override	void	OnTargetChanged( TargetInfo_t targetInfo )
	{
		base.OnTargetChanged( targetInfo ); // 	m_TargetInfo = targetInfo;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetUpdate ( Override )
	public		override	void	OnTargetUpdate( TargetInfo_t targetInfo )
	{
		base.OnTargetUpdate( targetInfo );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetLost ( Override )
	public		override	void	OnTargetLost( TargetInfo_t targetInfo )
	{
		// Set brain to ALLARMED mode
//		m_Brain.ChangeState( BrainState.NORMAL );

		// TODO manage alarmed state
	}
	

	//////////////////////////////////////////////////////////////////////////
	// OnFrame ( Override )
	protected	override	void	OnFrame( float deltaTime )
	{
		base.OnFrame( deltaTime );

		// Update internal timer
		m_ShotTimer -= deltaTime;

		if ( m_TargetInfo.HasTarget == true )
		{
			if ( m_Brain.State != BrainState.ATTACKER )
				m_Brain.ChangeState( BrainState.ATTACKER );

			SetPoinToLookAt( m_TargetInfo.CurrentTarget.Transform.position );
		}
		
		// if has target point to face at set
		if ( m_HasLookAtObject )
		{
			FaceToPoint( deltaTime );   // m_PointToFace
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnKill ( Override )
	public		override	void	OnKill()
	{
		base.OnKill();
//		m_Pool.Destroy();
		gameObject.SetActive( false );
	}
	

	//////////////////////////////////////////////////////////////////////////
	// FaceToPoint ( Override )
	protected	override	void	FaceToPoint( float deltaTime )
	{
		Vector3 pointOnThisPlane		= Utils.Math.ProjectPointOnPlane( transform.up, m_BodyTransform.position, m_PointToFace );

		Vector3 dirToPosition			= ( pointOnThisPlane - m_BodyTransform.position );
		Vector3 dirGunToPosition		= ( m_PointToFace - m_GunTransform.position );

		Quaternion	bodyRotation		= Quaternion.LookRotation( dirToPosition, transform.up );
		m_BodyTransform.rotation		= Quaternion.RotateTowards( m_BodyTransform.rotation, bodyRotation, m_BodyRotationSpeed * deltaTime );
		
		m_IsAllignedBodyToPoint	= Vector3.Angle( m_BodyTransform.forward, dirToPosition ) < 2f;

		if ( m_IsAllignedBodyToPoint == false )
		{
			m_IsAllignedHeadToPoint = false;
			return;
		}
		/*
		m_IsAllignedHeadToPoint			= Vector3.Angle( m_GunTransform.forward, dirGunToPosition ) < 1.2f;
		if ( m_IsAllignedHeadToPoint == false )
		{
			float signedAngleToTarget = Vector3.SignedAngle( m_GunTransform.forward, dirGunToPosition, m_GunTransform.right );
			float currentAngle = m_GunTransform.transform.localRotation.eulerAngles.x;
			currentAngle -= currentAngle > 180f ? 360f : 0f;
			float rotation = m_GunRotationSpeed * Utils.Math.Sign( signedAngleToTarget ) * deltaTime;
			if ( Mathf.Abs( currentAngle + rotation ) < ( m_Brain.FieldOfView.Angle / 2f ) )
			{
				m_GunTransform.Rotate( Vector3.right, rotation, Space.Self );	
			}
		}

	*/
	}


	//////////////////////////////////////////////////////////////////////////
	// FireLongRange ( Override )
	protected	override	void	FireLongRange( float deltaTime )
	{
		if ( m_ShotTimer > 0 )
				return;

		m_ShotTimer = m_ShotDelay;
		
		IBullet bullet = m_Pool.GetComponent();
		bullet.Shoot( position: m_FirePoint.position, direction: m_FirePoint.forward );
		
		m_FireAudioSource.Play();
	}

}
