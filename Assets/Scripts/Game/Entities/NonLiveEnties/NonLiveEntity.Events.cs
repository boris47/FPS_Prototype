
using UnityEngine;
using UnityEngine.AI;


public abstract partial class NonLiveEntity : Entity {

	
	//////////////////////////////////////////////////////////////////////////

	protected	override	StreamUnit	OnSave( StreamData streamData )
	{
		StreamUnit streamUnit = base.OnSave( streamData );

		// Health
		streamUnit.SetInternal( "Health", this.m_Health );

		// Shield
		if (this.m_Shield != null )
		{
			streamUnit.SetInternal( "ShieldStatus", this.m_Shield.Status );
		}



		// Internals
//		streamUnit.AddInternal( "HasDestination",				m_NavHasDestination );
//		streamUnit.AddInternal( "HasFaceTarget",					m_HasFaceTarget );
//		streamUnit.AddInternal( "Destination",					Utils.Converters.Vector3ToString( m_Destination ) );
//		streamUnit.AddInternal( "PointToFace",					Utils.Converters.Vector3ToString( m_PointToFace ) );
		streamUnit.SetInternal( "IsMoving", this.m_NavCanMoveAlongPath );
		streamUnit.SetInternal( "IsAllignedBodyToDestination", this.m_IsAllignedBodyToPoint );
		streamUnit.SetInternal( "IsAllignedGunToPoint", this.m_IsAllignedHeadToPoint );
//		streamUnit.AddInternal( "DistanceToTravel",				m_DistanceToTravel );
		
		// Body and Gun
		{
			streamUnit.SetInternal( "BodyRotation",				Utils.Converters.QuaternionToString(this.m_BodyTransform.localRotation ) );
			streamUnit.SetInternal( "GunRotation",				Utils.Converters.QuaternionToString(this.m_GunTransform.localRotation ) );
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
		this.m_Health = streamUnit.GetAsFloat( "Health" );

		// Shield
		if (this.m_Shield != null )
		{
			this.m_Shield.OnLoad( streamData );
		}

		// Internals
		//		m_NavHasDestination					= streamUnit.GetAsBool( "HasDestination" );
		//		m_HasFaceTarget						= streamUnit.GetAsBool( "HasFaceTarget" );
		//		m_Destination						= streamUnit.GetAsVector( "Destination" );
		//		m_PointToFace						= streamUnit.GetAsVector( "PointToFace" );
		this.m_NavCanMoveAlongPath							= streamUnit.GetAsBool( "IsMoving" );
		this.m_IsAllignedBodyToPoint		= streamUnit.GetAsBool( "IsAllignedBodyToDestination" );
		this.m_IsAllignedHeadToPoint				= streamUnit.GetAsBool( "IsAllignedGunToPoint" );
//		m_DistanceToTravel					= streamUnit.GetAsFloat( "DistanceToTravel" );

		// Body and Gun
		{
			this.m_BodyTransform.localRotation	= streamUnit.GetAsQuaternion( "BodyRotation" );;
			this.m_GunTransform.localRotation	= streamUnit.GetAsQuaternion( "GunRotation" );
		}

		// Brain state
//		m_Brain.ChangeState ( streamUnit.GetAsEnum<BrainState>( "BrainState" ) );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////

	protected	override	void		OnTargetAquired( TargetInfo targetInfo )
	{
		this.m_TargetInfo.Update( targetInfo );

		base.OnTargetAquired(this.m_TargetInfo );
	}


	//////////////////////////////////////////////////////////////////////////

	protected	override	void		OnTargetChanged( TargetInfo targetInfo )
	{
		this.m_TargetInfo.Update( targetInfo );

		base.OnTargetChanged(this.m_TargetInfo );
	}


	//////////////////////////////////////////////////////////////////////////

	protected	override	void		OnTargetLost( TargetInfo targetInfo )
	{
		base.OnTargetLost(this.m_TargetInfo );

		this.m_TargetInfo.Reset();
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
		this.m_ShotTimer -= deltaTime;

		if (this.m_NavAgent != null )
		{
			bool nowIsMoving = this.m_NavAgent.velocity.sqrMagnitude > 0.0f;
			if (this.m_HasDestination == true && this.WasMoving == true && nowIsMoving == false )
			{
				this.OnDestinationReached(this.transform.position );
			}
			this.WasMoving = this.m_NavAgent.velocity.sqrMagnitude > 0.0f;
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
