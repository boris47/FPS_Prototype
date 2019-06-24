
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

		Utils.Base.SearchComponent( gameObject, ref m_FireAudioSource, SearchContext.LOCAL );

		m_GunTransform		= m_HeadTransform.Find( "Gun" );
		m_FirePoint			= m_GunTransform.Find( "FirePoint" );
	}
	

	//////////////////////////////////////////////////////////////////////////
	protected	override	void		EnterSimulationState()
	{
		NavStop();

		StopLooking();
	}

	//////////////////////////////////////////////////////////////////////////
	protected	override	void		BeforeSimulationStage( SimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		// Movement
		RequestMovement( destination );
//		Debug.Log("BeforeSimulationStage " + destination.ToString() );

		// Look At
		if ( target )
		{
			SetTrasformToLookAt( target, LookTargetMode.HEAD_ONLY );
		}
		else
		{
			StopLooking();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	bool		SimulateMovement( SimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget = 1 )
	{
		bool isBusy = true;

		isBusy &= m_HasDestination || m_HasPendingPathRequest;

//		isBusy |= HasLookAtObject;

		return isBusy;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		AfterSimulationStage( SimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		NavStop();

		StopLooking();
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		ExitSimulationState()
	{
		
	}


	public override bool CanFire()
	{
		return m_IsAllignedGunToPoint;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		m_Pool.Destroy();
	}
}
