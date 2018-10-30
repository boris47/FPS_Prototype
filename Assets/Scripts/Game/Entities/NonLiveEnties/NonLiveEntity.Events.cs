
using UnityEngine;
using UnityEngine.AI;


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
//		streamUnit.AddInternal( "BrainState", m_Brain.State );

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
//		m_Brain.ChangeState ( streamUnit.GetAsEnum<BrainState>( "BrainState" ) );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////

	protected	override	void		OnTargetAquired( TargetInfo targetInfo )
	{
		m_TargetInfo.Update( targetInfo );

		base.OnTargetAquired( m_TargetInfo );
	}


	//////////////////////////////////////////////////////////////////////////

	protected	override	void		OnTargetChanged( TargetInfo targetInfo )
	{
		m_TargetInfo.Update( targetInfo );

		base.OnTargetChanged( m_TargetInfo );
	}


	//////////////////////////////////////////////////////////////////////////

	protected	override	void		OnTargetLost( TargetInfo targetInfo )
	{
		base.OnTargetLost( m_TargetInfo );

		m_TargetInfo.Reset();
	}


	//////////////////////////////////////////////////////////////////////////

	public		override	void		OnDestinationReached( Vector3 Destination )
	{
		base.OnDestinationReached(  Destination );
	}
	

	//////////////////////////////////////////////////////////////////////////

	protected	override	void		OnPhysicFrame( float fixedDeltaTime )
	{
		base.OnPhysicFrame( fixedDeltaTime );
	}
	

	//////////////////////////////////////////////////////////////////////////
	protected	bool	WasMoving = false;
	protected	override	void		OnFrame( float deltaTime )
	{
		m_ShotTimer -= deltaTime;

		if ( m_NavAgent != null )
		{
			bool nowIsMoving = m_NavAgent.velocity.sqrMagnitude > 0.0f;
			if ( m_HasDestination == true && WasMoving == true && nowIsMoving == false )
			{
				OnDestinationReached( transform.position );
			}
			WasMoving = m_NavAgent.velocity.sqrMagnitude > 0.0f;
		}

		base.OnFrame( deltaTime );
	}


	//////////////////////////////////////////////////////////////////////////

	protected	override	void		OnThink()
	{
		base.OnThink();
	}

	
	//////////////////////////////////////////////////////////////////////////

	protected	override	void		OnKill()
	{
		base.OnKill();
	}

}
