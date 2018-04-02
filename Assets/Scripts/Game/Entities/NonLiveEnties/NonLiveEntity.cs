
using UnityEngine;


public abstract partial class NonLiveEntity : Entity {
	
	[SerializeField]
	protected		float				m_MaxDamage			= 7f;

	[SerializeField]
	protected		float				m_MinDamage			= 3f;

	[SerializeField]
	protected		float				m_ShotDelay			= 0.7f;

//	[SerializeField]
	protected		AudioSource			m_FireAudioSource	= null;
	protected		Shield				m_Shield			= null;

	protected		Transform			m_GunTransform		= null;
	protected		Transform			m_FirePoint			= null;

	protected		GameObjectsPool<Bullet> m_Pool			= null;
	protected		float				m_ShotTimer			= 0f;

	protected		bool				m_AllignedToTarget	= false;
	protected		bool				m_AllignedGunToTarget	= false;



	protected override void Awake()
	{
		base.Awake();

		m_FireAudioSource	= GetComponent<AudioSource>();
		m_Shield			= GetComponentInChildren<Shield>();

		m_GunTransform		= transform.Find( "Gun" );

		m_FirePoint			= m_GunTransform.GetChild( 0 );
	}


	protected override void Update()
	{
		base.Update();
	}
	

}
