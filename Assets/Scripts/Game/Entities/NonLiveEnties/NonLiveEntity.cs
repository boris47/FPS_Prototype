
using System;
using UnityEngine;


public abstract partial class NonLiveEntity : Entity
{	
//	[Header("Non Live Entity Properties")]


	//////////////////////////////////////////////////////////////////////////
	public	override	void		EnterSimulationState()
	{
	//	NavStop();

	//	StopLooking();
	}

	//////////////////////////////////////////////////////////////////////////
	public	override	void		BeforeSimulationStage( EMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		// Movement
	//	RequestMovement( destination );

		// Look At
	//	if ( target )
	//	{
	//		SetTransformToLookAt( target, ELookTargetMode.HEAD_ONLY );
	//	}
	//	else
	//	{
	//		StopLooking();
	//	}
	}


	//////////////////////////////////////////////////////////////////////////
	public	override	bool		SimulateMovement( EMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget = 1 )
	{
		bool isBusy = true;

	//	isBusy &= m_HasDestination || m_HasPendingPathRequest;

//		isBusy |= HasLookAtObject;

		return isBusy;
	}


	//////////////////////////////////////////////////////////////////////////
	public	override	void		AfterSimulationStage( EMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
	//	NavStop();

	//	StopLooking();
	}


	//////////////////////////////////////////////////////////////////////////
	public	override	void		ExitSimulationState()
	{
		
	}
}
