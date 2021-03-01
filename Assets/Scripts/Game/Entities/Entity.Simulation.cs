
using UnityEngine;
using CutScene;

public enum ESimMovementType
{
	STATIONARY,
	WALK,
	CROUCHED,
	RUN
}

public partial interface IEntity
{
	// Cutscene manager, that take control over entity during cutscene sequences
	CutsceneEntityManager	CutsceneManager					{ get; }

	bool					HasCutsceneManager				{ get; }
}

public interface IEntitySimulation
{
	Vector3		StartPosition			{ get; set; }

	/// <summary> Enter Simulation State </summary>
	void		EnterSimulationState	();

	/// <summary> Before Simulation Stage </summary>
	void		BeforeSimulationStage	( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget );

	/// <summary> Simulate Movement, Return true if is Busy otherwise false </summary>
	bool		SimulateMovement		( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget = 1f );

	/// <summary> After Simulation Stage </summary>
	void		AfterSimulationStage	( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget );

	/// <summary> Exit Simulation State </summary>
	void		ExitSimulationState		();

}

// Cutscene Simulation Manager
//[RequireComponent( typeof( CutsceneEntityManager ) ) ]
public abstract partial class Entity : IEntitySimulation {

	CutsceneEntityManager	IEntity.CutsceneManager				{ get { return m_CutsceneManager; }	}
	bool					IEntity.HasCutsceneManager			{ get { return m_HasCutsceneManager; } }

	Vector3					IEntitySimulation.StartPosition		{ get; set; }

	[Header("Cutscene Manager")]

	// CUTSCENE MANAGER
	protected	CutsceneEntityManager		m_CutsceneManager				= null;
	protected	bool						m_HasCutsceneManager			= false;

	protected	bool						m_MovementOverrideEnabled		= false;
	protected	Vector3						m_SimulationStartPosition		= Vector3.zero;


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Enter Simulation State </summary>
	void	IEntitySimulation.EnterSimulationState()
	{
		EnterSimulationState();
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Before Simulation Stage </summary>
	void	IEntitySimulation.BeforeSimulationStage( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		BeforeSimulationStage( movementType, destination, target, timeScaleTarget );
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Simulate Movement, Return true if is Busy otherwise false </summary>
	bool	IEntitySimulation.SimulateMovement( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		return SimulateMovement( movementType, destination, target, timeScaleTarget );
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> After Simulation Stage </summary>
	void	IEntitySimulation.AfterSimulationStage( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		AfterSimulationStage( movementType, destination, target, timeScaleTarget );
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Exit Simulation State </summary>
	void	IEntitySimulation.ExitSimulationState()
	{
		ExitSimulationState();
	}



	//////////////////////////////////////////////////////////////////////////
	protected	abstract	void	EnterSimulationState();

	//////////////////////////////////////////////////////////////////////////
	protected	abstract	void	ExitSimulationState();

	//////////////////////////////////////////////////////////////////////////
	protected	abstract	void	BeforeSimulationStage( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget );

	//////////////////////////////////////////////////////////////////////////
	protected	abstract	bool	SimulateMovement( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget = 1f );

	//////////////////////////////////////////////////////////////////////////
	protected	abstract	void	AfterSimulationStage( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget );

}