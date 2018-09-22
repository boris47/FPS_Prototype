
using UnityEngine;


public class TurretHidden : Turret {

	private		bool		m_InTransition	= false;
	private		bool		m_IsEnabled		= false;
	private		Animator	m_Animator		= null;

	private		Vector3		m_BulletStartPositon = Vector3.zero;


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		m_SectionName = this.GetType().FullName;

		base.Awake();

		m_Animator = GetComponent<Animator>();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave ( Override )
	protected override StreamUnit OnSave( StreamData streamData )
	{
		StreamUnit streamUnit = base.OnSave( streamData );
		if ( streamUnit == null )
			return null;

		streamUnit.AddInternal( "IsEnabled", m_IsEnabled );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad ( Override )
	protected override StreamUnit OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = base.OnLoad( streamData );
		if ( streamUnit == null )
			return null;

		m_IsEnabled = streamUnit.GetAsBool( "IsEnabled" );
		
		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetAquired ( Override )
	public override void OnTargetAquired( TargetInfo_t targetInfo )
	{
		m_TargetInfo = targetInfo;

		// now point to face is target position
//		m_PointToFace = m_TargetInfo.CurrentTarget.Transform.position;
//		m_HasFaceTarget = true;

		// now point to reach is target position
//		m_Destination = m_TargetInfo.CurrentTarget.transform.position;
//		m_NavHasDestination = true;

		m_Brain.ChangeState( BrainState.ATTACKING );

		if ( m_IsEnabled == false )
		{
			Activate();
			return;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Override )
	public		override	void	OnHit( IBullet bullet )
	{
		// Avoid friendly fire
		if ( bullet.WhoRef is NonLiveEntity )
			return;

		base.OnHit( bullet );

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
	protected		override	void	OnFrame( float deltaTime )
	{
		// Update internal timer
		m_ShotTimer -= deltaTime;

		if ( m_IsEnabled == false )
			return;

		if ( m_InTransition == true )
			return;

		if ( m_Brain.State == BrainState.ALARMED )
		{
//			FaceToPoint( deltaTime );
		}

		if ( m_Brain.State == BrainState.ATTACKING )
		{
			if ( m_TargetInfo.HasTarget == true )
			{
//				m_PointToFace		= m_TargetInfo.CurrentTarget.Transform.position;
			}
//			FaceToPoint( deltaTime );	// m_PointToFace

			if ( m_IsAllignedHeadToPoint == false )
			return;

			FireLongRange( deltaTime );	
		}
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
//		if ( m_IsEnabled == true )
//			m_PointToFace = m_BulletStartPositon;

		m_InTransition = false;
	}

}
