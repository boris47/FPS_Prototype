
using UnityEngine;


public class TurretHidden : Turret {

	private		bool		m_InTransition	= false;
	private		bool		m_IsEnabled		= false;
	private		Animator	m_Animator		= null;

	private		bool		m_Horizontal	= true;

	private		Vector3		m_ScaleVectorH	= new Vector3( 1.0f, 1.0f, 0.0f );
	private		Vector3		m_ScaleVectorV	= new Vector3( 1.0f, 0.0f, 1.0f );

	private		Vector3		m_BulletStartPositon = Vector3.zero;


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		m_SectionName = this.GetType().FullName;

		base.Awake();

		m_Animator = GetComponent<Animator>();

		m_Horizontal = Vector3.Cross( Vector3.up, transform.up ).x != 0f;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetAquired ( Override )
	public override void OnTargetAquired( TargetInfo_t targetInfo )
	{
		m_TargetInfo = targetInfo;

		// now point to face is target position
		m_PointToFace = m_TargetInfo.CurrentTarget.transform.position;
		m_HasFaceTarget = true;

		// now point to reach is target position
//		m_Destination = m_TargetInfo.CurrentTarget.transform.position;
//		m_HasDestination = true;

		m_Brain.ChangeState( BrainState.ATTACKING );

		if ( m_IsEnabled == false )
		{
			Activate();
			return;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Override )
	public		override	void	OnHit( ref IBullet bullet )
	{
		// Avoid friendly fire
		if ( bullet.WhoRef is NonLiveEntity )
			return;

		base.OnHit( ref bullet );

		if ( m_IsEnabled == false )
		{
			Activate();
			m_BulletStartPositon = bullet.StartPosition;
			return;
		}

		float damage = Random.Range( bullet.DamageMin, bullet.DamageMax );
		m_Health -= damage;

		if ( m_Health < 0f )
			OnKill();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnThink ( Override )
	public		override	void	OnThink()
	{
		if ( m_InTransition == true )
			return;
		
		base.OnThink();
	}


	// Update forward direction and gun rotation
	//////////////////////////////////////////////////////////////////////////
	// OnFrame ( Override )
	public		override	void	OnFrame( float deltaTime )
	{
		// Update internal timer
		m_ShotTimer -= deltaTime;

		if ( m_IsEnabled == false )
			return;

		if ( m_InTransition == true )
			return;

		if ( m_Brain.State == BrainState.ALARMED )
		{
			FaceToPoint( deltaTime );
		}

		if ( m_Brain.State == BrainState.ATTACKING )
		{
			if ( m_TargetInfo.HasTarget == true )
			{
				m_PointToFace		= m_TargetInfo.CurrentTarget.transform.position;
			}
			FaceToPoint( deltaTime );	// m_PointToFace

			if ( m_AllignedGunToPoint == false )
			return;

			FireLongRange( deltaTime );	
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FaceToPoint ( Override )
	protected override void FaceToPoint( float deltaTime )
	{
		Vector3 dirToPosition				= ( m_PointToFace - transform.position ).normalized;
		Vector3 dirGunToPosition			= ( m_PointToFace - m_GunTransform.position ).normalized;
		dirToPosition						= Vector3.Scale( dirToPosition, m_Horizontal ? m_ScaleVectorH : m_ScaleVectorV );

		Quaternion rotation					= Quaternion.LookRotation( dirToPosition, transform.up );
		transform.rotation					= Quaternion.RotateTowards( transform.rotation, rotation, m_BodyRotationSpeed * deltaTime );

		m_IsAllignedBodyToDestination		= Quaternion.Angle( transform.rotation, rotation ) < 7f;
		if ( m_IsAllignedBodyToDestination )
		{
			m_GunTransform.forward			=  Vector3.RotateTowards( m_GunTransform.forward, dirGunToPosition, m_GunRotationSpeed * deltaTime, 0.0f );
		}

		m_AllignedGunToPoint				= Vector3.Angle( m_GunTransform.forward, dirGunToPosition ) < 7f;
	}


	//////////////////////////////////////////////////////////////////////////
	// Activate
	private		void	Activate()
	{
		m_IsEnabled = true;
		m_InTransition = true;
		m_Animator.Play( "Enable", -1, 0.0f );
	}


	//////////////////////////////////////////////////////////////////////////
	// Deactivate
	private		void	Deactivate()
	{
		m_IsEnabled = false;
		m_InTransition = true;
		m_Animator.Play( "Disable", -1, 0.0f );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEndAnimation
	private		void	OnEndAnimation()
	{
		if ( m_IsEnabled == true )
			m_PointToFace = m_BulletStartPositon;

		m_InTransition = false;
	}

}
