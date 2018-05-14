﻿
using UnityEngine;

[RequireComponent( typeof ( AI_Behaviours.Brain ) )]
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
	
	protected		bool				m_AllignedGunToPoint		= false;


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
	// OnSave ( Override )
	protected override StreamingUnit OnSave( StreamingData streamingData )
	{
		StreamingUnit streamingUnit = base.OnSave( streamingData );

		if ( m_Shield != null )
		{
			streamingUnit.ShieldStatus = m_Shield.Status;
		}

		return base.OnSave( streamingData );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad ( Override )
	protected override StreamingUnit OnLoad( StreamingData streamingData )
	{
		StreamingUnit streamingUnit = base.OnSave( streamingData );

		if ( streamingUnit.ShieldStatus > -1f )
		{
			( m_Shield as IShield ).Status = streamingUnit.ShieldStatus;
		}

		return base.OnLoad( streamingData );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnThink ( Override )
	public override void OnThink()
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
