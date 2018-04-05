
using UnityEngine;
using System.Collections;
using System;

public abstract class Drone : NonLiveEntity {

	[Header("Drone Properties")]

	[SerializeField]
	private		GameObject		m_BulletGameObject			= null;

	[SerializeField]
	protected	float			m_ShotDelay					= 0.7f;

	[SerializeField]
	protected	float			m_DamageLongRangeMax		= 2f;

	[SerializeField]
	protected	float			m_DamageLongRangeMin		= 0.5f;

	[SerializeField]
	protected	float			m_DamageCloseRange			= 5f;

	[SerializeField]
	protected	float			m_CloseCombatRange			= 1.2f;

	[SerializeField]
	protected	float			m_CloseCombatDelay			= 1f;

	[SerializeField]
	protected	float			m_MoveMaxSpeed				= 3f;


	protected	Entity			m_Instance					= null;
	protected	float			m_CloseCombatDelayInternal	= 0f;


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		base.Awake();

		m_Instance = this;

		// LOAD CONFIGURATION
		{
			GameManager.Configs.GetSection( m_SectionName = gameObject.name, ref m_SectionRef );
			if ( m_SectionRef == null )
			{
				print( name + " cannot find his section !!" );
				Destroy( gameObject );
				return;
			}

			m_Health				= m_SectionRef.AsFloat( "Health",				30.0f );
			float shieldStatus		= m_SectionRef.AsFloat( "Shield",				60.0f );
			if ( m_Shield != null )
				( m_Shield as IShield ).Status = shieldStatus;

			m_MoveMaxSpeed			= m_SectionRef.AsFloat( "MoveMaxSpeed",			1.0f );

			m_DamageLongRangeMax	= m_SectionRef.AsFloat( "DamageLongRangeMax",	2.0f );
			m_DamageLongRangeMin	= m_SectionRef.AsFloat( "DamageLongRangeMin",	0.5f );
			m_DamageCloseRange		= m_SectionRef.AsFloat( "DamageCloseRange",		5.0f );

			m_CloseCombatRange		= m_SectionRef.AsFloat( "CloseCombatRange",		1.2f );
			m_CloseCombatDelay		= m_SectionRef.AsFloat( "CloseCombatDelay",		1.2f );

			m_EntityType			= ENTITY_TYPE.ROBOT;
		}

		// BULLETS POOL CREATION
		{
			GameObject	bulletGO		= m_BulletGameObject;
			m_Pool = new GameObjectsPool<Bullet>( ref bulletGO, 5, destroyModel : false, actionOnObject : ( Bullet o ) =>
			{
				o.SetActive( false );
				o.Setup( damageMin : m_DamageLongRangeMin, damageMax : m_DamageLongRangeMax, canPenetrate : false, whoRef : this, weapon : null );
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
		m_CloseCombatDelayInternal -= deltaTime;
		m_ShotTimer -= Time.deltaTime;

		// set direction to player
		Vector3 vBodyForward		= Vector3.Scale( dirToPosition,		new Vector3( 1.0f, 0.0f, 1.0f ) );
		transform.forward			= Vector3.RotateTowards( transform.forward, vBodyForward, m_BodyRotationSpeed * deltaTime, 0.0f );

		m_AllignedToPoint			= Vector3.Angle( transform.forward, vBodyForward ) < 7f;
		if ( m_AllignedToPoint )
		{
			m_GunTransform.forward	=  Vector3.RotateTowards( m_GunTransform.forward, dirGunToPosition, m_GunRotationSpeed * deltaTime, 0.0f );
		}

		m_AllignedGunToPoint		= Vector3.Angle( m_GunTransform.forward, dirGunToPosition ) < 7f;
	}


	//////////////////////////////////////////////////////////////////////////
	// GoAtPoint ( Override )
	protected override void GoAtPoint( float deltaTime )
	{
		if ( m_IsMoving == false )
			return;

		Vector3 dirToPosition	 = ( m_PointToFace - transform.position );
		float	travelledDistance = ( m_StartMovePosition - transform.position ).sqrMagnitude;
		if ( travelledDistance > m_DistanceToTravel )   // point reached
		{
			if ( m_Brain.State == BrainState.ALARMED )
				m_Brain.ChangeState( BrainState.NORMAL );
			m_IsMoving = false;
			m_StartMovePosition = m_PointToFace = Vector3.zero;
			return;
		}

		transform.position		+= dirToPosition.normalized * m_MoveMaxSpeed * deltaTime;
	}



	//////////////////////////////////////////////////////////////////////////
	// FireLongRange ( Override )
	protected override void FireLongRange( float deltaTime )
	{
		if ( m_ShotTimer > 0 )
				return;

		m_ShotTimer = m_ShotDelay;

		Bullet bullet = m_Pool.GetComponent();
		bullet.Shoot( position: m_FirePoint.position, direction: m_FirePoint.forward );
		
		m_FireAudioSource.Play();
	}


	//////////////////////////////////////////////////////////////////////////
	// FireCloseRange ( Override )
	protected override void FireCloseRange( float deltaTime )
	{
		if ( m_CloseCombatDelayInternal < 0f )
		{
			m_CloseCombatDelayInternal = m_CloseCombatDelay;
//			m_Brain.CurrentTargetInfo.CurrentTarget.OnHit( ref m_Instance, m_DamageCloseRange );

			// TODO: add a attack/hit effect
		}	
	}

}
