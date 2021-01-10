
using UnityEngine;


public class WalkerHidden : Walker {
	

///	private		bool		m_InTransition	= false;
///	private		bool		m_IsEnabled		= false;
	private		Animator	m_Animator		= null;

	private		Vector3		m_BulletStartPositon = Vector3.zero;



	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		m_SectionName = GetType().FullName;

		base.Awake();

		TryGetComponent(out m_Animator);
	}
	/*

	//////////////////////////////////////////////////////////////////////////
	// OnTargetAquired ( Override )
	protected override void OnTargetAquired( TargetInfo_t targetInfo )
	{
		base.OnTargetAquired( targetInfo );		// m_TargetInfo = targetInfo;

		m_Brain.ChangeState( BrainState.ATTACKER );

		if ( m_IsEnabled == false )
		{
			Activate();
			return;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetChanged ( Override )
	protected override void OnTargetChanged( TargetInfo_t targetInfo )
	{
		base.OnTargetChanged( targetInfo );		// m_TargetInfo = targetInfo;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetLost ( Override )
	protected override void OnTargetLost( TargetInfo_t targetInfo )
	{
		base.OnTargetLost( targetInfo );		// m_TargetInfo = default( TargetInfo_t );

		if ( m_Brain.State == BrainState.ATTACKER )
		{
			m_Brain.ChangeState( BrainState.NORMAL );
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

	
	// Update forward direction and gun rotation
	//////////////////////////////////////////////////////////////////////////
	// OnFrame ( Override )
	protected		override	void	OnFrame( float deltaTime )
	{
		if ( m_IsEnabled == false )
			return;

		if ( m_InTransition == true )
			return;

		if ( m_Brain.State != BrainState.NORMAL )
		{
			if ( m_IsAllignedBodyToPoint && m_NavCanMoveAlongPath == false )
			{
				m_NavCanMoveAlongPath = true;
				
			}

//			GoAtPoint( deltaTime );
		}

		// Not have a target
		if ( m_TargetInfo.HasTarget == false )
			return;

		// Not aligned to target
		if ( m_IsAllignedHeadToPoint == false )
			return;
		{
			FireLongRange( deltaTime );
		}
	}
	*/

	//////////////////////////////////////////////////////////////////////////
	// Activate
	private		void	Activate()
	{
		///		m_IsEnabled = true;
		///		m_InTransition = true;
		m_Animator.Play( "Enable", -1, 0.0f );
	}


	//////////////////////////////////////////////////////////////////////////
	// Deactivate
	private		void	Deactivate()
	{
		///		m_IsEnabled = false;
		///		m_InTransition = true;
		m_Animator.Play( "Disable", -1, 0.0f );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEndAnimation
	private		void	OnEndAnimation()
	{
//		if ( m_IsEnabled == true )
//			m_PointToFace = m_BulletStartPositon;

///		m_InTransition = false;
	}
	
}
