
using UnityEngine;

[RequireComponent( typeof ( AI.Brain ) )]
public abstract partial class NonLiveEntity : Entity {
	
	[Header("Non Live Entity Properties")]
	[Space]

	[Header("Navigation")]
	[SerializeField]
	protected		float				m_FeetsRotationSpeed		= 5f;

	[Header("Orientation")]
	[SerializeField]
	protected		float				m_BodyRotationSpeed			= 5f;

	[SerializeField]
	protected       float               m_HeadRotationSpeed			= 5f;

	[SerializeField]
	protected		float				m_GunRotationSpeed			= 5f;

	protected		ICustomAudioSource	m_FireAudioSource			= null;
	protected		Shield				m_Shield					= null;

	protected		Transform			m_HeadTransform				= null;
	protected		Transform			m_BodyTransform				= null;
	protected		Transform			m_FootsTransform			{ get { return transform; } }

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

//		m_FootsTransform	= transform
		m_BodyTransform		= transform.Find( "Body" );
		m_HeadTransform		= m_BodyTransform.Find( "Head" );
		m_GunTransform		= m_HeadTransform.Find( "Gun" );
		m_FirePoint			= m_GunTransform.Find( "FirePoint" );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnFrame ( Override )
	protected	override	void	OnFrame( float deltaTime )
	{	
		
	}


	//////////////////////////////////////////////////////////////////////////
	// NavUpdate ( Override )
	protected	override	void	NavUpdate( float Speed, float DeltaTime )
	{
		base.NavUpdate( Speed, DeltaTime );
	}


	//////////////////////////////////////////////////////////////////////////
	// NavUpdate ( Override )
	protected	override	void	NavMove( Vector3 CurrentDestination, float Speed, float DeltaTime )
	{

	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave ( Override )
	protected	override	StreamingUnit	OnSave( StreamingData streamingData )
	{
		StreamingUnit streamingUnit = base.OnSave( streamingData );

		// Health
		streamingUnit.AddInternal( "Health", m_Health );

		// Shield
		if ( m_Shield != null )
		{
			streamingUnit.AddInternal( "ShieldStatus", m_Shield.Status );
		}

		// Internals
//		streamingUnit.AddInternal( "HasDestination",				m_NavHasDestination );
//		streamingUnit.AddInternal( "HasFaceTarget",					m_HasFaceTarget );
//		streamingUnit.AddInternal( "Destination",					Utils.Converters.Vector3ToString( m_Destination ) );
//		streamingUnit.AddInternal( "PointToFace",					Utils.Converters.Vector3ToString( m_PointToFace ) );
		streamingUnit.AddInternal( "IsMoving",						m_NavCanMoveAlongPath );
		streamingUnit.AddInternal( "IsAllignedBodyToDestination",	m_IsAllignedBodyToPoint );
		streamingUnit.AddInternal( "IsAllignedGunToPoint",			m_IsAllignedHeadToPoint );
		streamingUnit.AddInternal( "StartMovePosition",				Utils.Converters.Vector3ToString( m_StartMovePosition ) );
//		streamingUnit.AddInternal( "DistanceToTravel",				m_DistanceToTravel );
		
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
	protected	override	StreamingUnit	OnLoad( StreamingData streamingData )
	{
		StreamingUnit streamingUnit = base.OnLoad( streamingData );
		if ( streamingUnit == null )
			return null;

		// Health
		m_Health = streamingUnit.GetAsFloat( "Health" );

		// Shield
		if ( streamingUnit.HasInternal( "ShieldStatus" ) )
		{
			( m_Shield as IShield ).Status = streamingUnit.GetAsFloat( "ShieldStatus" );//  .ShieldStatus;


		}

		// Internals
//		m_NavHasDestination					= streamingUnit.GetAsBool( "HasDestination" );
//		m_HasFaceTarget						= streamingUnit.GetAsBool( "HasFaceTarget" );
//		m_Destination						= streamingUnit.GetAsVector( "Destination" );
//		m_PointToFace						= streamingUnit.GetAsVector( "PointToFace" );
		m_NavCanMoveAlongPath							= streamingUnit.GetAsBool( "IsMoving" );
		m_IsAllignedBodyToPoint		= streamingUnit.GetAsBool( "IsAllignedBodyToDestination" );
		m_IsAllignedHeadToPoint				= streamingUnit.GetAsBool( "IsAllignedGunToPoint" );
		m_StartMovePosition					= streamingUnit.GetAsVector( "StartMovePosition" );
//		m_DistanceToTravel					= streamingUnit.GetAsFloat( "DistanceToTravel" );

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
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	// FaceToPoint ( Abstract )
	protected	abstract	void	FaceToPoint( float deltaTime );


	//////////////////////////////////////////////////////////////////////////
	// FireLongRange ( Abstract )
	protected	abstract	void	FireLongRange( float deltaTime );


	//////////////////////////////////////////////////////////////////////////
	// EnterSimulationState ( Override )
	public		override	void	EnterSimulationState()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	// ExitSimulationState ( Override )
	public		override	void	ExitSimulationState()
	{
		
	}

}
