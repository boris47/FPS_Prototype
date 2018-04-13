
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
	protected		bool				m_IsMoving					= false;
	protected		bool				m_AllignedToPoint			= false;
	protected		bool				m_AllignedGunToPoint		= false;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	protected	override	void	Awake()
	{
		base.Awake();

		m_FireAudioSource	= GetComponent<AudioSource>();
		m_Shield			= GetComponentInChildren<Shield>();

		m_GunTransform		= transform.Find( "Gun" );

		m_FirePoint			= m_GunTransform.GetChild( 0 );

		SoundEffectManager.Instance.RegisterSource( ref m_FireAudioSource );
		m_FireAudioSource.volume = SoundEffectManager.Instance.Volume;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnFrame
	public	override	void	OnFrame( float deltaTime )
	{
		base.OnFrame( deltaTime );
		
		if ( m_Brain.State != BrainState.NORMAL )
		{
			if ( m_TargetInfo.HasTarget == true )
			{
				m_PointToFace		= m_TargetInfo.CurrentTarget.transform.position;
			}
			FaceToPoint( deltaTime );	// m_PointToFace
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FaceToPoint
	protected	abstract	void	FaceToPoint( float deltaTime );

	//////////////////////////////////////////////////////////////////////////
	// GoAtPoint
//	protected	abstract	void	GoAtPoint( float deltaTime );


	//////////////////////////////////////////////////////////////////////////
	// FireLongRange
//	protected	abstract	void	FireLongRange( float deltaTime );

	//////////////////////////////////////////////////////////////////////////
	// FireCloseRange
//	protected	abstract	void	FireCloseRange( float deltaTime );

	
	private void OnDestroy()
	{
		SoundEffectManager.Instance.UnRegisterSource( ref m_FireAudioSource );
	}

}
