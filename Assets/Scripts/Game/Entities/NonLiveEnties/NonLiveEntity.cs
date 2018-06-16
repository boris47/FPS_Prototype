
using UnityEngine;

[RequireComponent( typeof ( Brain ) )]
public abstract partial class NonLiveEntity : Entity {
	
	[Header("Non Live Entity Properties")]

	[SerializeField]
	protected		float				m_BodyRotationSpeed			= 5f;

	[SerializeField]
	protected		float				m_GunRotationSpeed			= 5f;

	protected		ICustomAudioSource	m_FireAudioSource			= null;
	protected		Shield				m_Shield					= null;

	protected		Transform			m_BodyTransform				= null;
	protected		Transform			m_GunTransform				= null;
	protected		Transform			m_FirePoint					= null;

	protected		GameObjectsPool<Bullet> m_Pool					= null;
	protected		float				m_ShotTimer					= 0f;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	protected	override	void	Awake()
	{
		base.Awake();

		m_FireAudioSource	= GetComponent<ICustomAudioSource>();
		m_Shield			= GetComponentInChildren<Shield>();

		m_BodyTransform		= transform.Find( "Body" );
		m_GunTransform		= m_BodyTransform.Find( "Gun" );
		m_FirePoint			= m_GunTransform.Find( "FirePoint" );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnFrame ( Override )
	protected	override	void	OnFrame( float deltaTime )
	{	
		// Update internal timer
		m_ShotTimer -= deltaTime;
		
		if ( m_TargetInfo.HasTarget == true )
		{
			if ( m_Brain.State != BrainState.ATTACKING )
				m_Brain.ChangeState( BrainState.ATTACKING );
			
			m_PointToFace = m_TargetInfo.CurrentTarget.Transform.position;
			m_HasFaceTarget = true;

			m_Destination = m_TargetInfo.CurrentTarget.Transform.position;
			m_HasDestination = true;

			m_DistanceToTravel	= ( transform.position - m_PointToFace ).sqrMagnitude;
		}

		// if has target point to face at set
		if ( m_HasFaceTarget )
		{
			FaceToPoint( deltaTime );   // m_PointToFace
		}

		// if body is alligned with target start moving
		if ( m_IsAllignedBodyToDestination && m_IsMoving == false )
		{
			m_IsMoving = true;
			m_StartMovePosition = transform.position;
		}

		// if has destination set
		if ( m_HasDestination && m_IsAllignedBodyToDestination )
		{
			GoAtPoint( deltaTime );	// m_Destination
		}

		// if gun alligned, fire
		if ( m_IsAllignedGunToPoint == true && m_TargetInfo.HasTarget == true )
		{
			FireLongRange( deltaTime );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave ( Override )
	protected override StreamingUnit OnSave( StreamingData streamingData )
	{
		StreamingUnit streamingUnit = base.OnSave( streamingData );

		// Health
		streamingUnit.AddInternal( "Health", m_Health );

		// Shield
		if ( m_Shield != null )
		{
			streamingUnit.ShieldStatus = m_Shield.Status;
		}

		// Internals
		streamingUnit.AddInternal( "HasDestination",				m_HasDestination );
		streamingUnit.AddInternal( "HasFaceTarget",					m_HasFaceTarget );
		streamingUnit.AddInternal( "Destination",					Utils.Converters.Vector3ToString( m_Destination ) );
		streamingUnit.AddInternal( "PointToFace",					Utils.Converters.Vector3ToString( m_PointToFace ) );
		streamingUnit.AddInternal( "IsMoving",						m_IsMoving );
		streamingUnit.AddInternal( "IsAllignedBodyToDestination",	m_IsAllignedBodyToDestination );
		streamingUnit.AddInternal( "IsAllignedGunToPoint",			m_IsAllignedGunToPoint );
		streamingUnit.AddInternal( "StartMovePosition",				Utils.Converters.Vector3ToString( m_StartMovePosition ) );
		streamingUnit.AddInternal( "DistanceToTravel",				m_DistanceToTravel );
		
		// Body and Gun
		{
			streamingUnit.AddInternal( "BodyRotation",				Utils.Converters.QauternionToString( m_BodyTransform.localRotation ) );
			streamingUnit.AddInternal( "GunRotation",				Utils.Converters.QauternionToString( m_GunTransform.localRotation ) );
		}

		// Brain state
		streamingUnit.AddInternal( "BrainState", m_Brain.State );

		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad ( Override )
	protected override StreamingUnit OnLoad( StreamingData streamingData )
	{
		StreamingUnit streamingUnit = base.OnLoad( streamingData );
		if ( streamingUnit == null )
			return null;

		// Health
		m_Health = streamingUnit.GetAsFloat( "Health" );

		// Shield
		if ( streamingUnit.ShieldStatus > -1f )
		{
			( m_Shield as IShield ).Status = streamingUnit.ShieldStatus;
		}

		// Internals
		m_HasDestination					= streamingUnit.GetAsBool( "HasDestination" );
		m_HasFaceTarget						= streamingUnit.GetAsBool( "HasFaceTarget" );
		m_Destination						= streamingUnit.GetAsVector( "Destination" );
		m_PointToFace						= streamingUnit.GetAsVector( "PointToFace" );
		m_IsMoving							= streamingUnit.GetAsBool( "IsMoving" );
		m_IsAllignedBodyToDestination		= streamingUnit.GetAsBool( "IsAllignedBodyToDestination" );
		m_IsAllignedGunToPoint				= streamingUnit.GetAsBool( "IsAllignedGunToPoint" );
		m_StartMovePosition					= streamingUnit.GetAsVector( "StartMovePosition" );
		m_DistanceToTravel					= streamingUnit.GetAsFloat( "DistanceToTravel" );

		// Body and Gun
		{
			m_BodyTransform.localRotation	= streamingUnit.GetAsQuaternion( "BodyRotation" );;
			m_GunTransform.localRotation	= streamingUnit.GetAsQuaternion( "GunRotation" );
		}

		// Brain state
		m_Brain.ChangeState ( streamingUnit.GetAsEnum<BrainState>( "BrainState" ) );

		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnThink ( Override )
	public		override	void	OnThink()
	{	}


	//////////////////////////////////////////////////////////////////////////
	// FaceToPoint ( Abstract )
	protected	abstract	void	FaceToPoint( float deltaTime );

	//////////////////////////////////////////////////////////////////////////
	// GoAtPoint ( Abstract )
	protected	abstract	void	GoAtPoint( float deltaTime );


	//////////////////////////////////////////////////////////////////////////
	// FireLongRange ( Abstract )
	protected	abstract	void	FireLongRange( float deltaTime );


	//////////////////////////////////////////////////////////////////////////
	// EnterSimulationState ( Override )
	public override void EnterSimulationState()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	// ExitSimulationState ( Override )
	public override void ExitSimulationState()
	{
		
	}

}
