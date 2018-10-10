
using UnityEngine;


public abstract partial class NonLiveEntity : Entity {
	
	[Header("Non Live Entity Properties")]
	[Space]

	[Header("Orientation")]
	[SerializeField]
	protected		float				m_BodyRotationSpeed			= 5.0f;

	[SerializeField]
	protected       float               m_HeadRotationSpeed			= 5.0f;

	[SerializeField]
	protected		float				m_GunRotationSpeed			= 5.0f;

	[SerializeField]
	protected		float				m_FireDispersion			= 0.01f;

	// Transforms
	protected		Transform			m_HeadTransform				= null;
	protected		Transform			m_BodyTransform				= null;
	protected		Transform			m_FootsTransform			{ get { return transform; } }
	protected		Transform			m_GunTransform				= null;
	protected		Transform			m_FirePoint					= null;

	// Weapon
	protected		GameObjectsPool<Bullet> m_Pool					= null;
	protected		float				m_ShotTimer					= 0.0f;
	protected		ICustomAudioSource	m_FireAudioSource			= null;



	//////////////////////////////////////////////////////////////////////////
	protected	override	void		Awake()
	{
		base.Awake();


		if ( GameManager.Configs.GetSection( m_SectionName, ref m_SectionRef ) == false )
		{
			print( "Cannot find cfg section for entity " + name );
			Destroy( gameObject );
			return;
		}

		// AI BEHAVIOURS
		{
			m_Brain.SetBehaviour( BrainState.EVASIVE,	m_SectionRef.AsString( "BehaviourEvasive"	), false );
			m_Brain.SetBehaviour( BrainState.NORMAL,	m_SectionRef.AsString( "BehaviourNormal"	), true  );
			m_Brain.SetBehaviour( BrainState.ALARMED,	m_SectionRef.AsString( "BehaviourAlarmed"	), false );
			m_Brain.SetBehaviour( BrainState.SEEKER,	m_SectionRef.AsString( "BehaviourSeeker"	), false );
			m_Brain.SetBehaviour( BrainState.ATTACKER,	m_SectionRef.AsString( "BehaviourAttacker"	), false );

			m_Brain.ChangeState( BrainState.NORMAL );
		}

		Utils.Base.SearchComponent( gameObject, ref m_FireAudioSource, SearchContext.LOCAL );

//		m_FootsTransform	= transform
		m_BodyTransform		= transform.Find( "Body" );
		m_HeadTransform		= m_BodyTransform.Find( "Head" );
		m_GunTransform		= m_HeadTransform.Find( "Gun" );
		m_FirePoint			= m_GunTransform.Find( "FirePoint" );

	}

	//////////////////////////////////////////////////////////////////////////
	protected	abstract	void		FaceToPoint( float deltaTime );


	//////////////////////////////////////////////////////////////////////////
	protected	abstract	void		FireLongRange( float deltaTime );


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		EnterSimulationState()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		ExitSimulationState()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	bool		SimulateMovement( SimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget = 1 )
	{
		return false;
	}

}
