
using UnityEngine;


public class TurretHidden : Turret {

	private		bool		m_InTransition	= false;
	private		bool		m_IsEnabled		= false;
	private		Animator	m_Animator		= null;

	private	bool			m_Horizontal	= true;

	protected	Vector3 m_ScaleVectorH = new Vector3( 1.0f, 1.0f, 0.0f );
	protected	Vector3 m_ScaleVectorV = new Vector3( 1.0f, 0.0f, 1.0f );



	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		base.Awake();

		m_Animator = GetComponent<Animator>();

		m_Horizontal = Vector3.Cross( Vector3.up, transform.up ).x != 0f;

	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetAquired ( Override )
	public override void OnTargetAquired( TargetInfo_t targetInfo )
	{
		base.OnTargetAquired( targetInfo );		// m_TargetInfo = targetInfo;

		m_Brain.ChangeState( BrainState.ATTACKING );

		if ( m_IsEnabled == false )
		{
			Activate();
			return;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetChanged ( Override )
	public override void OnTargetChanged( TargetInfo_t targetInfo )
	{
		base.OnTargetChanged( targetInfo );		// m_TargetInfo = targetInfo;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetLost ( Override )
	public override void OnTargetLost( TargetInfo_t targetInfo )
	{
		base.OnTargetLost( targetInfo );		// m_TargetInfo = default( TargetInfo_t );

		if ( m_Brain.State == BrainState.ATTACKING )
		{
			m_Brain.ChangeState( BrainState.NORMAL );
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
//			m_PointToFace		= bullet.StartPosition;
			return;
		}

		float damage = Random.Range( bullet.DamageMin, bullet.DamageMax );
		m_Health -= damage;

		if ( m_Health < 0f )
			OnKill();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnKill ( Override )
	public		override	void	OnKill()
	{
		base.OnKill();
		m_Pool.Destroy();
		Destroy( gameObject );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnThink ( Override )
	public		override	void	OnThink()
	{
		if ( m_InTransition == true )
			return;
		
		base.OnThink();
	}


	private		void	Rotate()
	{

	}


	// Update forward direction and gun rotation
	//////////////////////////////////////////////////////////////////////////
	// OnFrame ( Override )
	public		override	void	OnFrame( float deltaTime )
	{
		if ( m_IsEnabled == false )
			return;

		if ( m_InTransition == true )
			return;

		if ( m_Brain.State == BrainState.ALARMED )
		{
			Rotate();
		}

		if ( m_Brain.State == BrainState.ATTACKING )
		{
			if ( m_TargetInfo.HasTarget == true )
			{
				m_PointToFace		= m_TargetInfo.CurrentTarget.transform.position;
			}
			FaceToPoint( deltaTime );	// m_PointToFace
		}

		if ( m_AllignedGunToPoint == false )
			return;

		FireLongRange( deltaTime );	
	}

	//////////////////////////////////////////////////////////////////////////
	// FaceToPoint ( Override )
	protected override void FaceToPoint( float deltaTime )
	{
		Vector3 dirToPosition		= ( m_PointToFace - transform.position ).normalized;
		Vector3 dirGunToPosition	= ( m_PointToFace - m_GunTransform.position ).normalized;
		dirToPosition				= Vector3.Scale( dirToPosition, m_Horizontal ? m_ScaleVectorH : m_ScaleVectorV );

		Quaternion rotation			= Quaternion.LookRotation( dirToPosition, transform.up );
		transform.rotation			= Quaternion.RotateTowards( transform.rotation, rotation, m_BodyRotationSpeed * deltaTime );


		m_AllignedToPoint			= Quaternion.Angle( transform.rotation, rotation ) < 7f;
		if ( m_AllignedToPoint )
		{
			m_GunTransform.forward	=  Vector3.RotateTowards( m_GunTransform.forward, dirGunToPosition, m_GunRotationSpeed * deltaTime, 0.0f );
		}

		m_AllignedGunToPoint		= Vector3.Angle( m_GunTransform.forward, dirGunToPosition ) < 7f;
	}


	private		void	Activate()
	{
		m_IsEnabled = true;
		m_InTransition = true;
		m_Animator.Play( "Enable", -1, 0.0f );
	}


	private		void	Deactivate()
	{
		m_IsEnabled = false;
		m_InTransition = true;
		m_Animator.Play( "Disable", -1, 0.0f );
	}

	private		void	OnEndAnimation()
	{
		m_InTransition = false;
	}


}
