
using System;
using UnityEngine;


public abstract class Turret : NonLiveEntity {

	[Header("Turret Properties")]

	[SerializeField]
	private		GameObject		m_BulletGameObject	= null;

	[SerializeField]
	protected	float			m_ShotDelay			= 0.7f;

	[SerializeField]
	protected	float			m_DamageMax			= 2f;

	[SerializeField]
	protected	float			m_DamageMin			= 0.5f;

	protected	Vector3			m_ScaleVector		= new Vector3( 1.0f, 0.0f, 1.0f );


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		base.Awake();

		// LOAD CONFIGURATION
		{
			GameManager.Configs.GetSection( m_SectionName = gameObject.name, ref m_SectionRef );
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

			m_EntityType			= ENTITY_TYPE.ROBOT;
		}

		// BULLETS POOL CREATION
		{
			GameObject	bulletGO		= m_BulletGameObject;
			m_Pool = new GameObjectsPool<Bullet>( ref bulletGO, 5, destroyModel : false, actionOnObject : ( Bullet o ) =>
			{
				o.SetActive( false );
				o.Setup( damageMin : m_DamageMin, damageMax : m_DamageMax, canPenetrate : false, whoRef : this, weapon : null );
				Physics.IgnoreCollision( o.Collider, m_PhysicCollider, ignore : true );
				if ( m_Shield != null )
					Physics.IgnoreCollision( o.Collider, m_Shield.Collider, ignore : true );
			} );
			m_Pool.ContainerName = name + "BulletPool";
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FaceToPoint ( Override )
	protected override void FaceToPoint( float deltaTime )
	{
		Vector3 dirToPosition		= ( m_PointToFace - transform.position );
		Vector3 dirGunToPosition	= ( m_PointToFace - m_GunTransform.position );

		Vector3 vBodyForward		= Vector3.Scale( dirToPosition,	m_ScaleVector );
		transform.forward			= Vector3.RotateTowards( transform.forward, vBodyForward, m_BodyRotationSpeed * deltaTime, 0.0f );
		
		m_AllignedToPoint			= Vector3.Angle( transform.forward, vBodyForward ) < 7f;
		if ( m_AllignedToPoint )
		{
			m_GunTransform.forward	=  Vector3.RotateTowards( m_GunTransform.forward, dirGunToPosition, m_GunRotationSpeed * deltaTime, 0.0f );
		}

		m_AllignedGunToPoint		= Vector3.Angle( m_GunTransform.forward, dirGunToPosition ) < 7f;
	}

	//////////////////////////////////////////////////////////////////////////
	// FireLongRange ( Override )
	protected virtual void FireLongRange( float deltaTime )
	{
		m_ShotTimer -= deltaTime;
		if ( m_ShotTimer > 0 )
				return;

		m_ShotTimer = m_ShotDelay;

		IBullet bullet = m_Pool.GetComponent();
		bullet.Shoot( position: m_FirePoint.position, direction: m_FirePoint.forward );
		
		m_FireAudioSource.Play();
	}


	//////////////////////////////////////////////////////////////////////////
	// FireCloseRange ( Override )
	protected void FireCloseRange( float deltaTime )
	{
		//	Nothing by default
	}

}
