
using UnityEngine;


public abstract partial class NonLiveEntity : Entity {

	
	//////////////////////////////////////////////////////////////////////////
	protected	override	StreamUnit	OnSave( StreamData streamData )
	{
		StreamUnit streamUnit = base.OnSave( streamData );

		// Health
		streamUnit.AddInternal( "Health", m_Health );

		// Shield
		if ( m_Shield != null )
		{
			streamUnit.AddInternal( "ShieldStatus", m_Shield.Status );
		}



		// Internals
//		streamUnit.AddInternal( "HasDestination",				m_NavHasDestination );
//		streamUnit.AddInternal( "HasFaceTarget",					m_HasFaceTarget );
//		streamUnit.AddInternal( "Destination",					Utils.Converters.Vector3ToString( m_Destination ) );
//		streamUnit.AddInternal( "PointToFace",					Utils.Converters.Vector3ToString( m_PointToFace ) );
		streamUnit.AddInternal( "IsMoving",						m_NavCanMoveAlongPath );
		streamUnit.AddInternal( "IsAllignedBodyToDestination",	m_IsAllignedBodyToPoint );
		streamUnit.AddInternal( "IsAllignedGunToPoint",			m_IsAllignedHeadToPoint );
//		streamUnit.AddInternal( "DistanceToTravel",				m_DistanceToTravel );
		
		// Body and Gun
		{
			streamUnit.AddInternal( "BodyRotation",				Utils.Converters.QauternionToString( m_BodyTransform.localRotation ) );
			streamUnit.AddInternal( "GunRotation",				Utils.Converters.QauternionToString( m_GunTransform.localRotation ) );
		}

		// Brain state
		streamUnit.AddInternal( "BrainState", m_Brain.State );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	StreamUnit	OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = base.OnLoad( streamData );
		if ( streamUnit == null )
			return null;

		// Health
		m_Health = streamUnit.GetAsFloat( "Health" );

		// Shield
		if ( streamUnit.HasInternal( "ShieldStatus" ) )
		{
			( m_Shield as IShield ).Status = streamUnit.GetAsFloat( "ShieldStatus" );//  .ShieldStatus;


		}

		// Internals
//		m_NavHasDestination					= streamUnit.GetAsBool( "HasDestination" );
//		m_HasFaceTarget						= streamUnit.GetAsBool( "HasFaceTarget" );
//		m_Destination						= streamUnit.GetAsVector( "Destination" );
//		m_PointToFace						= streamUnit.GetAsVector( "PointToFace" );
		m_NavCanMoveAlongPath							= streamUnit.GetAsBool( "IsMoving" );
		m_IsAllignedBodyToPoint		= streamUnit.GetAsBool( "IsAllignedBodyToDestination" );
		m_IsAllignedHeadToPoint				= streamUnit.GetAsBool( "IsAllignedGunToPoint" );
//		m_DistanceToTravel					= streamUnit.GetAsFloat( "DistanceToTravel" );

		// Body and Gun
		{
			m_BodyTransform.localRotation	= streamUnit.GetAsQuaternion( "BodyRotation" );;
			m_GunTransform.localRotation	= streamUnit.GetAsQuaternion( "GunRotation" );
		}

		// Brain state
		m_Brain.ChangeState ( streamUnit.GetAsEnum<BrainState>( "BrainState" ) );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void		OnTargetAquired( TargetInfo_t targetInfo )
	{
		print( "OntargetAcquired" );

		m_TargetInfo = targetInfo;

		// now point to face is target position
		SetPoinToFace( m_TargetInfo.CurrentTarget.Transform.position );

		m_Brain.ChangeState( BrainState.ATTACKING );
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void		OnTargetUpdate( TargetInfo_t targetInfo )
	{

	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void		OnTargetChanged( TargetInfo_t targetInfo )
	{
		print( "OnTargetChanged" );

		m_TargetInfo = targetInfo;
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void		OnTargetLost( TargetInfo_t targetInfo )
	{
		print( "OnTargetLost" );

		m_TargetInfo = default( TargetInfo_t );

		m_Brain.ChangeState( BrainState.NORMAL );
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void		OnHit( IBullet bullet )
	{
		OnHit( bullet.StartPosition, bullet.WhoRef, 0f );
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void		OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
	{
		// Hit event, set ALARMED State if actual is NORMAL
		if ( m_Brain.State == BrainState.NORMAL )
		{
			m_Brain.ChangeState( BrainState.ALARMED );
		}

		// if is not attacking
		if ( m_Brain.State != BrainState.ATTACKING )
		{
			SetPoinToFace( startPosition );
		}
	}
	

	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnPhysicFrame( float fixedDeltaTime )
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnFrame( float deltaTime )
	{	
		
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void		OnThink()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void		OnKill()
	{
		base.OnKill();
	}

}
