
using System;
using UnityEngine;


public abstract partial class NonLiveEntity : Entity {
	
	[Header("Non Live Entity Properties")]
	[Space]

	[SerializeField]
	protected		float				m_GunRotationSpeed			= 5.0f;

	[SerializeField]
	protected		float				m_FireDispersion			= 0.01f;

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

		Utils.Base.SearchComponent( gameObject, ref m_FireAudioSource, SearchContext.LOCAL );

		m_GunTransform		= m_HeadTransform.Find( "Gun" );
		m_FirePoint			= m_GunTransform.Find( "FirePoint" );

	}
	
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


	public override bool CanFire()
	{
		return m_IsAllignedGunToPoint;
	}
}
