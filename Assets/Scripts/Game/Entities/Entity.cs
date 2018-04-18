
using CFG_Reader;
using UnityEngine;
using AI_Behaviours;


public interface IEntity {
	Transform				Transform						{	get;		}
	bool					IsActive						{	get;		}
	uint					ID								{	get;		}
	float					Health							{	get;		}
	string					Section							{	get;		}
	Rigidbody				RigidBody						{	get;		}
	CapsuleCollider			PhysicCollider					{	get;		}
	IBrain					Brain							{	get;		}
	CutsceneEntityManager	CutsceneManager					{	get; set;	}

	void					OnFrame							( float deltaTime );
	void					OnThink							();

	void					OnTargetAquired					( TargetInfo_t targetInfo );
	void					OnTargetChanged					( TargetInfo_t targetInfo );
	void					OnTargetLost					( TargetInfo_t targetInfo );

	void					OnHit							( ref IBullet bullet );
	void					OnKill							();
}


public interface IEntitySimulation {
	Vector3		StarPosition			{ get; set; }

	void		EnterSimulationState	();
	void		ExitSimulationState		();
	bool		SimulateMovement		( Entity.SimulationMovementType movementType, Vector3 destination, Transform target, float deltaTime, float interpolant = 0f );

}


[RequireComponent( typeof( Rigidbody ), typeof( CapsuleCollider ) )]
public abstract partial class Entity : MonoBehaviour, IEntity, IEntitySimulation {

	public enum ENTITY_TYPE {
		NONE,
		ACTOR,
		HUMAN,
		ROBOT,
		ANIMAL,
		OBJECT
	};

	public enum SimulationMovementType {
		WALK,
		WALK_CROUCHED
	}

	[Header("Entity Properties")]
	private	static uint			CurrentID							= 0;
	public	static uint			NewID()								{ return CurrentID++; }

	// INTERFACE START
				Transform				IEntity.Transform					{	get { return transform;			}	}
				bool					IEntity.IsActive					{	get { return m_IsActive;		}	}
				uint					IEntity.ID							{	get { return m_ID;				}	}
				float					IEntity.Health						{	get { return m_Health;			}	}
				string					IEntity.Section						{	get { return m_SectionName;		}	}
				Rigidbody				IEntity.RigidBody					{	get { return m_RigidBody;		}	}
				CapsuleCollider			IEntity.PhysicCollider				{	get { return m_PhysicCollider;	}	}
				IBrain					IEntity.Brain						{	get { return m_Brain;			}	}
				CutsceneEntityManager	IEntity.CutsceneManager				{	get { return m_CutsceneManager; } set { m_CutsceneManager = value; } }
	// INTERFACE END

	// INTERNALS
	protected	float						m_Health						= 1f;
	protected	bool						m_IsActive						= true;
	protected 	uint						m_ID							= 0;
	protected	Section						m_SectionRef					= null;
	protected 	string						m_SectionName					= "None";
	protected 	ENTITY_TYPE					m_EntityType					= ENTITY_TYPE.NONE;
	protected	Rigidbody					m_RigidBody						= null;
	protected	CapsuleCollider				m_PhysicCollider				= null;

	// AI
	protected	IBrain						m_Brain							= null;
	protected	TargetInfo_t				m_TargetInfo					= default( TargetInfo_t );
//	protected	Vector3						m_LastAttackPosition			= Vector3.zero;			// store start position of hitting bullet

	[SerializeField]
	protected	float						m_MinEngageDistance				= 0f;

	[SerializeField]
	protected	RespawnPoint				m_RespawnPoint					= null;


	// CUTSCENE DATA
	[SerializeField]
	protected	CutsceneEntityManager		m_CutsceneManager				= null;

	protected	bool						m_MovementOverrideEnabled		= false;
	protected	Vector3						m_SimulationStartPosition		= Vector3.zero;
	protected	float						m_SimulationdDistanceToTravel	= 0f;


	// NAVIGATION
	protected	bool						m_HasDestination				= false;
	protected	bool						m_HasFaceTarget					= false;
	protected	Vector3						m_Destination					= Vector3.zero;
	protected	Vector3						m_PointToFace					= Vector3.zero;
	protected	bool						m_IsMoving						= false;

	// Flag set if body of entity is aligned with target
	protected	bool						m_IsAllignedBodyToDestination	= false;

	// Position saved at start of movement ( used for distances check )
	protected	Vector3						m_StartMovePosition				= Vector3.zero;
				Vector3						IEntitySimulation.StarPosition { get { return m_StartMovePosition; } set { m_StartMovePosition = value; } }
	protected	float						m_DistanceToTravel				= 0f;


	protected	bool 						m_IsOK							= false;




	//////////////////////////////////////////////////////////////////////////
	// Awake
	protected	virtual	void	Awake()
	{
		m_ID				= NewID();
		m_PhysicCollider	= GetComponent<CapsuleCollider>();
		m_RigidBody			= GetComponent<Rigidbody>();
		m_Brain				= GetComponent<IBrain>();

		if ( this is Player )
			return;

		m_Brain.FieldOfView.OnTargetAquired = OnTargetAquired;
		m_Brain.FieldOfView.OnTargetChanged = OnTargetChanged;
		m_Brain.FieldOfView.OnTargetLost	= OnTargetLost;
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	public	abstract	void	OnFrame( float deltaTime );



	//////////////////////////////////////////////////////////////////////////
	// EnterSimulationState
	public virtual void	EnterSimulationState()
	{
		m_MovementOverrideEnabled = true;
		m_SimulationStartPosition = transform.position;
	}


	//////////////////////////////////////////////////////////////////////////
	// ExitSimulationState
	public virtual void	ExitSimulationState()
	{
		m_MovementOverrideEnabled = false;
		m_SimulationStartPosition = Vector3.zero;
	}


	//////////////////////////////////////////////////////////////////////////
	// SimulateMovement
	public abstract	bool	SimulateMovement( SimulationMovementType movementType, Vector3 destination, Transform target, float deltaTime, float interpolant = 0f );

}
