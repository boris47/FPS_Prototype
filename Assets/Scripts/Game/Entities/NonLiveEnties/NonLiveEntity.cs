
using UnityEngine;


[RequireComponent( typeof ( AI_Behaviours.Brain ) )]
public abstract partial class NonLiveEntity : Entity {
	
	[Header("Non Live Entity Properties")]

	[SerializeField]
	protected		float				m_BodyRotationSpeed			= 5f;

	[SerializeField]
	protected		float				m_GunRotationSpeed			= 5f;

//	[SerializeField]
	protected		AudioSource			m_FireAudioSource			= null;
	protected		Shield				m_Shield					= null;

	protected		Transform			m_GunTransform				= null;
	protected		Transform			m_FirePoint					= null;

	protected		GameObjectsPool<Bullet> m_Pool					= null;
	protected		float				m_ShotTimer					= 0f;

	protected		Vector3				m_PointToFace				= Vector3.zero;
	protected		Vector3				m_StartMovePosition			= Vector3.zero;
	protected		bool				m_IsMoving					= false;
	protected		bool				m_AllignedToPoint			= false;
	protected		bool				m_AllignedGunToPoint		= false;

	protected		float				m_DistanceToTravel			= 0f;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	protected	override	void	Awake()
	{
		base.Awake();

		m_FireAudioSource	= GetComponent<AudioSource>();
		m_Shield			= GetComponentInChildren<Shield>();

		m_GunTransform		= transform.Find( "Gun" );

		m_FirePoint			= m_GunTransform.GetChild( 0 );
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	public	override	void	OnFrame( float deltaTime )
	{
		base.OnFrame( deltaTime );
		
		if ( m_Brain.State != BrainState.NORMAL )
		{
			if ( m_Brain.CurrentTargetInfo.HasTarget == true )
			{
				m_PointToFace		= m_Brain.CurrentTargetInfo.CurrentTarget.transform.position;
				m_DistanceToTravel	= ( transform.position - m_PointToFace ).sqrMagnitude;
			}

			FaceToPoint( deltaTime );	// m_PointToFace

			if ( m_AllignedToPoint && m_IsMoving == false )
			{
				m_IsMoving = true;
				m_StartMovePosition = transform.position;
			}

			GoAtPoint( deltaTime );
		}
		
	}


	//////////////////////////////////////////////////////////////////////////
	// FaceToPoint
	protected	abstract	void	FaceToPoint( float deltaTime );

	//////////////////////////////////////////////////////////////////////////
	// GoAtPoint
	protected	abstract	void	GoAtPoint( float deltaTime );


	//////////////////////////////////////////////////////////////////////////
	// FireLongRange
	protected	abstract	void	FireLongRange( float deltaTime );

	//////////////////////////////////////////////////////////////////////////
	// FireCloseRange
	protected	abstract	void	FireCloseRange( float deltaTime );
	

}
