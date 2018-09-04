
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



	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
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

		Laser laser = GetComponentInChildren<Laser>();
		if ( laser != null )
			laser.LaserLength = m_Brain.FieldOfView.Distance;

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
					Physics.IgnoreCollision( o.Collider, m_PhysicCollider, ignore : true );

					// this allow to receive only trigger enter callback
					Player.Instance.DisableCollisionsWith( o.Collider );

					if ( m_Shield != null )
						Physics.IgnoreCollision( o.Collider, m_Shield.Collider, ignore : true );
				}
			);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Override )
	public override void OnHit( IBullet bullet )
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
	public override void OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
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
	// OnKill ( Override )
	public override void OnKill()
	{
		base.OnKill();
//		m_Pool.Destroy();
		gameObject.SetActive( false );
	}

	/*
	//////////////////////////////////////////////////////////////////////////
	// OnTargetLost ( Override )
	public override void OnTargetLost( TargetInfo_t targetInfo )
	{
		// SEEKING MODE
		
		// now point to face is target position
		if ( targetInfo.CurrentTarget != null )
		{
			m_PointToFace = m_TargetInfo.CurrentTarget.Transform.position;
			m_HasFaceTarget = true;
		}

		// now point to reach is target position
//		m_Destination = m_TargetInfo.CurrentTarget.transform.position;
//		m_HasDestination = true;

		// Set brain to SEKKER mode
		m_Brain.ChangeState( BrainState.NORMAL );

		// Reset internal ref to target
		base.OnTargetLost( targetInfo );		// m_TargetInfo = default( TargetInfo_t );
	}
	*/
	/*
	//////////////////////////////////////////////////////////////////////////
	// FaceToPoint ( Override )
	protected override void FaceToPoint( float deltaTime )
	{
		Vector3 pointOnThisPlane		= Utils.Math.ProjectPointOnPlane( transform.up, m_BodyTransform.position, m_PointToFace );

		Vector3 dirToPosition			= ( pointOnThisPlane - m_BodyTransform.position );
		Vector3 dirGunToPosition		= ( m_PointToFace - m_GunTransform.position );

		Quaternion	bodyRotation		= Quaternion.LookRotation( dirToPosition, transform.up );
		m_BodyTransform.rotation		= Quaternion.RotateTowards( m_BodyTransform.rotation, bodyRotation, m_BodyRotationSpeed * deltaTime );
		
		m_IsAllignedBodyToDestination	= Vector3.Angle( m_BodyTransform.forward, dirToPosition ) < 2f;

		if ( m_IsAllignedBodyToDestination == false )
		{
			m_IsAllignedGunToPoint = false;
			return;
		}

		m_IsAllignedGunToPoint			= Vector3.Angle( m_GunTransform.forward, dirGunToPosition ) < 3f;

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
	/*
	//////////////////////////////////////////////////////////////////////////
	// GoAtPoint ( Override )
	protected override void GoAtPoint( float deltaTime )
	{ }
	*/

	//////////////////////////////////////////////////////////////////////////
	// FireLongRange ( Override )
	protected override void FireLongRange( float deltaTime )
	{
		if ( m_ShotTimer > 0 )
				return;

		m_ShotTimer = m_ShotDelay;
		
		IBullet bullet = m_Pool.GetComponent();
		bullet.Shoot( position: m_FirePoint.position, direction: m_FirePoint.forward );
		
		m_FireAudioSource.Play();
	}

}
