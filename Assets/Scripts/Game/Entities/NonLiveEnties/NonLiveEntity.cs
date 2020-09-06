
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

		Utils.Base.SearchComponent(this.gameObject, ref this.m_FireAudioSource, ESearchContext.LOCAL );

		this.m_GunTransform		= this.m_HeadTransform.Find( "Gun" );
		this.m_FirePoint			= this.m_GunTransform.Find( "FirePoint" );
	}
	

	//////////////////////////////////////////////////////////////////////////
	protected	override	void		EnterSimulationState()
	{
		this.NavStop();

		this.StopLooking();
	}

	//////////////////////////////////////////////////////////////////////////
	protected	override	void		BeforeSimulationStage( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		// Movement
		this.RequestMovement( destination );
//		Debug.Log("BeforeSimulationStage " + destination.ToString() );

		// Look At
		if ( target )
		{
			this.SetTransformToLookAt( target, ELookTargetMode.HEAD_ONLY );
		}
		else
		{
			this.StopLooking();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	bool		SimulateMovement( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget = 1 )
	{
		bool isBusy = true;

		isBusy &= this.m_HasDestination || this.m_HasPendingPathRequest;

//		isBusy |= HasLookAtObject;

		return isBusy;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		AfterSimulationStage( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		this.NavStop();

		this.StopLooking();
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		ExitSimulationState()
	{
		
	}


	public override bool CanFire()
	{
		return this.m_IsAllignedGunToPoint;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		this.m_Pool.Destroy();
	}
}
