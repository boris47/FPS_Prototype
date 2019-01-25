
using UnityEngine;
using CutScene;

public partial interface IEntity {

	// Cutscene manager, that take control over entity during cutscene sequences
	CutsceneEntityManager	CutsceneManager					{ get; }

}

public interface IEntitySimulation {
	Vector3		StartPosition			{ get; set; }

	void		EnterSimulationState	();
	void		ExitSimulationState		();
	bool		SimulateMovement		( SimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget = 1f );

}

// Cutscene Simulation Manager
[RequireComponent( typeof( CutsceneEntityManager ) ) ]
public abstract partial class Entity : IEntitySimulation {

	CutsceneEntityManager	IEntity.CutsceneManager				{ get { return m_CutsceneManager; }	}

	Vector3					IEntitySimulation.StartPosition		{ get; set; }

	[Header("Cutscene Manager")]

	// CUTSCENE MANAGER
	protected	CutsceneEntityManager		m_CutsceneManager				= null;

	protected	bool						m_MovementOverrideEnabled		= false;
	protected	Vector3						m_SimulationStartPosition		= Vector3.zero;


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Set the Collision state with another collider </summary>
	void	IEntitySimulation.EnterSimulationState()
	{
		this.EnterSimulationState();
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Set the Collision state with another collider </summary>
	void	IEntitySimulation.ExitSimulationState()
	{
		this.ExitSimulationState();
	}


	//////////////////////////////////////////////////////////////////////////
	bool	IEntitySimulation.SimulateMovement( SimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		return this.SimulateMovement( movementType, destination, target, timeScaleTarget );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	abstract	void	EnterSimulationState();


	//////////////////////////////////////////////////////////////////////////
	protected	abstract	void	ExitSimulationState();


	//////////////////////////////////////////////////////////////////////////
	protected	abstract	bool	SimulateMovement( SimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget = 1f );


}

public enum SimMovementType {
	STATIONARY,
	WALK,
	CROUCHED,
	RUN
}