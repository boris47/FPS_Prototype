
using UnityEngine;
using CutScene;

public enum EMovementType
{
	STATIONARY,
	WALK,
	CROUCHED,
	RUN
}
/*
public interface IEntitySimulation
{
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
*/

public abstract partial class Entity
{
	[Header("Entity: Cutscene Simulation")]

	// CUTSCENE MANAGER
	[SerializeField, ReadOnly]
	protected	CutsceneEntityManager		m_CutsceneManager				= null;
	[SerializeField, ReadOnly]
	protected	bool						m_MovementOverrideEnabled		= false;

	protected	Vector3						m_SimulationStartPosition		= Vector3.zero;

	public		CutsceneEntityManager		CutsceneManager					=> m_CutsceneManager;

	//////////////////////////////////////////////////////////////////////////
	////// <summary> Enter Simulation State </summary>
	public	abstract	void	EnterSimulationState();

	//////////////////////////////////////////////////////////////////////////
	////// <summary> Before Simulation Stage </summary>
	public	abstract	void	BeforeSimulationStage(EMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget = 1f);

	//////////////////////////////////////////////////////////////////////////
	////// <summary> Simulate Movement, Return true if is Busy otherwise false </summary>
	public	abstract	bool	SimulateMovement(EMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget = 1f);

	//////////////////////////////////////////////////////////////////////////
	////// <summary> After Simulation Stage </summary>
	public	abstract	void	AfterSimulationStage(EMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget = 1f);

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Exit Simulation State </summary>
	public	abstract	void	ExitSimulationState();
}