
using System;
using UnityEngine;


public abstract partial class NonLiveEntity : Entity {
	
	[Header("Non Live Entity Properties")]
	[Space]

	[SerializeField]
	protected		float				m_GunAllignmentSpeed		= 5.0f;


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		Awake()
	{
		base.Awake();
	}
	

	//////////////////////////////////////////////////////////////////////////
	protected	override	void		EnterSimulationState()
	{
		NavStop();

		StopLooking();
	}

	//////////////////////////////////////////////////////////////////////////
	protected	override	void		BeforeSimulationStage( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		// Movement
		RequestMovement( destination );
//		Debug.Log("BeforeSimulationStage " + destination.ToString() );

		// Look At
		if ( target )
		{
			SetTransformToLookAt( target, ELookTargetMode.HEAD_ONLY );
		}
		else
		{
			StopLooking();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	bool		SimulateMovement( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget = 1 )
	{
		bool isBusy = true;

		isBusy &= m_HasDestination || m_HasPendingPathRequest;

//		isBusy |= HasLookAtObject;

		return isBusy;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		AfterSimulationStage( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		NavStop();

		StopLooking();
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		ExitSimulationState()
	{
		
	}


//	public override bool CanFire()
//	{
//		return m_IsAllignedGunToPoint;
//	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
