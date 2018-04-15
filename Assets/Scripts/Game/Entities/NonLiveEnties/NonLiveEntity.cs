
using UnityEngine;

[RequireComponent( typeof ( AI_Behaviours.Brain ) )]
public abstract partial class NonLiveEntity : Entity {
	
	[Header("Non Live Entity Properties")]

	[SerializeField]
	protected		float				m_BodyRotationSpeed			= 5f;

	[SerializeField]
	protected		float				m_GunRotationSpeed			= 5f;

//	[SerializeField]
	protected		ICustomAudioSource	m_FireAudioSource			= null;
	protected		Shield				m_Shield					= null;

	protected		Transform			m_GunTransform				= null;
	protected		Transform			m_FirePoint					= null;

	protected		GameObjectsPool<Bullet> m_Pool					= null;
	protected		float				m_ShotTimer					= 0f;
	
	protected		bool				m_AllignedGunToPoint		= false;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	protected	override	void	Awake()
	{
		base.Awake();

		m_FireAudioSource	= GetComponent<ICustomAudioSource>();
		m_Shield			= GetComponentInChildren<Shield>();

		m_GunTransform		= transform.Find( "Gun" );

		m_FirePoint			= m_GunTransform.GetChild( 0 );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnFrame ( Override )
	public	override	void	OnFrame( float deltaTime )
	{	
		// Update internal timer
		m_ShotTimer -= deltaTime;
		
		if ( m_TargetInfo.HasTarget == true )
		{
			if ( m_Brain.State != BrainState.ATTACKING )
				m_Brain.ChangeState( BrainState.ATTACKING );
			
			m_PointToFace = m_TargetInfo.CurrentTarget.transform.position;
			m_HasFaceTarget = true;

			m_Destination = m_TargetInfo.CurrentTarget.transform.position;
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
		if ( m_AllignedGunToPoint == true && m_TargetInfo.HasTarget == true )
		{
			FireLongRange( deltaTime );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnThink ( Override )
	public override void OnThink()
	{	}


	//////////////////////////////////////////////////////////////////////////
	// FaceToPoint
	protected	abstract	void	FaceToPoint( float deltaTime );

	//////////////////////////////////////////////////////////////////////////
	// GoAtPoint
	protected	abstract	void	GoAtPoint( float deltaTime );

	protected	abstract	void	FireLongRange( float deltaTime );

}
