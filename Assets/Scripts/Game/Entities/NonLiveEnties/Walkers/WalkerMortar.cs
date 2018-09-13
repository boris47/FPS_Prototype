
using UnityEngine;


public class WalkerMortar : Walker {


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		m_SectionName = this.GetType().FullName;

		base.Awake();
	}


	//////////////////////////////////////////////////////////////////////////
	// Update ( Override )
	protected override void OnFrame( float deltaTime )
	{
		// Update internal timer
		m_ShotTimer -= deltaTime;
		
		if ( m_TargetInfo.HasTarget == true )
		{
			if ( m_Brain.State != BrainState.ATTACKING )
				m_Brain.ChangeState( BrainState.ATTACKING );
			
//			m_PointToFace = m_TargetInfo.CurrentTarget.Transform.position;
//			m_HasFaceTarget = true;

//			m_Destination = m_TargetInfo.CurrentTarget.Transform.position;
//			m_NavHasDestination = true;

//			m_DistanceToTravel	= ( transform.position - m_PointToFace ).sqrMagnitude;
		}

		// if has target point to face at set
//		if ( m_HasFaceTarget )
//		{
//			FaceToPoint( deltaTime );   // m_PointToFace
//		}

		// if body is alligned with target start moving
		if ( m_IsAllignedBodyToDestination && m_NavCanMoveAlongPath == false )
		{
			m_NavCanMoveAlongPath = true;
			m_StartMovePosition = transform.position;
		}

		// if has destination set
//		if ( m_NavHasDestination && m_IsAllignedBodyToDestination )
		{
//			if ( m_TargetInfo.HasTarget && ( transform.position - m_TargetInfo.CurrentTarget.Transform.position ).sqrMagnitude > m_MinEngageDistance * m_MinEngageDistance )
//				GoAtPoint( deltaTime );	// m_Destination
///			else
//				GoAtPoint( deltaTime );	// m_Destination
		}

		// if gun alligned, fire
		if ( m_IsAllignedGunToPoint == true )
		{
			FireLongRange( deltaTime );
		}
	}

	/*
	//////////////////////////////////////////////////////////////////////////
	// FaceToPoint ( Override )
	protected override void FaceToPoint( float deltaTime )
	{
		Vector3 pointOnThisPlane			= Utils.Math.ProjectPointOnPlane( transform.up, transform.position, m_PointToFace );
		Vector3 dirToPosition				= ( pointOnThisPlane - transform.position );

		Vector3 vBodyForward				= Vector3.Scale( dirToPosition,	m_ScaleVector );
		transform.forward					= Vector3.RotateTowards( transform.forward, vBodyForward, m_BodyRotationSpeed * deltaTime, 0.0f );

		Vector3 ballisticDirOfGun			= Vector3.zero;

		m_IsAllignedBodyToDestination		= Vector3.Angle( transform.forward, vBodyForward ) < 7f;
		if ( m_IsAllignedBodyToDestination && m_TargetInfo.HasTarget == true )
		{
			float angle = CalculateFireAngle( 0f, m_GunTransform.position, m_PointToFace );
			ballisticDirOfGun = m_GunTransform.forward + BallisticVelocity( m_PointToFace, angle ).normalized;

			m_GunTransform.forward			=  Vector3.RotateTowards( m_GunTransform.forward, ballisticDirOfGun, m_GunRotationSpeed * deltaTime, 0.0f );
		}

		m_IsAllignedGunToPoint				= Vector3.Angle( m_GunTransform.forward, ballisticDirOfGun ) < 3f;
	}
	*/

	// https://unity3d.college/2017/06/30/unity3d-cannon-projectile-ballistics/
	//////////////////////////////////////////////////////////////////////////
	// BallisticVelocity
	private Vector3 BallisticVelocity( Vector3 destination, float angle )
	{
		Vector3 dir = destination - m_GunTransform.position;	// get Target Direction
		float height = dir.y;									// get height difference
		dir.y = 0;												// retain only the horizontal difference
		float dist = dir.magnitude;								// get horizontal direction
		float a = angle * Mathf.Deg2Rad;						// Convert angle to radians
		dir.y = dist * Mathf.Tan(a);							// set dir to the elevation angle.
		dist += height / Mathf.Tan(a);							// Correction for small height differences

		// Calculate the velocity magnitude
		float velocity = Mathf.Sqrt(dist * Physics.gravity.magnitude / Mathf.Sin(2 * a));
		return velocity * dir.normalized;						// Return a normalized vector.
	}


	//////////////////////////////////////////////////////////////////////////
	// CalculateProjectileFiringSolution
	private float CalculateFireAngle( float alt, Vector3 startPosition, Vector3 endPosition )
	{ 
		Bullet bullet = m_Pool.GetAsModel();
		float bulletVelocity = bullet.Velocity;

		Vector2 a = new Vector2( startPosition.x, startPosition.z );
		Vector2 b = new Vector2( endPosition.x, endPosition.z );
		float dis = Vector2.Distance( a, b );
		alt = -( startPosition.y - m_TargetInfo.CurrentTarget.Transform.position.y );
		
		float g = Mathf.Abs( Physics.gravity.y );
				
		float dis2 = dis * dis;
		float vel2 = bulletVelocity * bulletVelocity;
		float vel4 = bulletVelocity * bulletVelocity * bulletVelocity * bulletVelocity;
		float num;
		float sqrt = vel4 - g * ( ( g * dis2 ) + ( 2f * alt * vel2 ) );
		if ( sqrt < 0 )
			return(45f);

		//Direct Fire
		if ( Vector3.Distance( startPosition, endPosition ) > bulletVelocity / 2f )
			num = vel2 - Mathf.Sqrt( vel4 - g * ( ( g * dis2 ) + ( 2f * alt * vel2 ) ) );
		else
			num = vel2 + Mathf.Sqrt( vel4 - g * ( ( g * dis2 ) + ( 2f * alt * vel2 ) ) );
		
		float dom = g * dis;
		float angle = Mathf.Atan( num / dom );

		return angle * Mathf.Rad2Deg;
	}

}
