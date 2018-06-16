
using CFG_Reader;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CutScene;

public interface IEntity {
	Transform				Transform						{	get;		}
	bool					IsActive						{	get;		}
	uint					ID								{	get;		}
	float					Health							{	get;		}
	string					Section							{	get;		}
	Rigidbody				RigidBody						{	get;		}
	CapsuleCollider			PhysicCollider					{	get;		}
	Transform				EffectsPivot					{	get;		}
	IBrain					Brain							{	get;		}
	CutsceneEntityManager	CutsceneManager					{	get; set;	}

	bool					CanTrigger();

	void					OnThink							();

	void					OnTargetAquired					( TargetInfo_t targetInfo );
	void					OnTargetChanged					( TargetInfo_t targetInfo );
	void					OnTargetLost					( TargetInfo_t targetInfo );

	void					OnHit							( IBullet bullet );
	void					OnHit							( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false );
	void					OnKill							();
}


public interface IEntitySimulation {
	Vector3		StartPosition			{ get; set; }

	void		EnterSimulationState	();
	void		ExitSimulationState		();
	bool		SimulateMovement		( Entity.SimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget = 1f );

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

	public enum SimMovementType {
		WALK,
		CROUCHED,
		RUN
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
				Transform				IEntity.EffectsPivot				{	get { return m_EffectsPivot;	}	}
				IBrain					IEntity.Brain						{	get { return m_Brain;			}	}
				CutsceneEntityManager	IEntity.CutsceneManager				{	get { return m_CutsceneManager; } set { m_CutsceneManager = value; } }
	// INTERFACE END

	public		IEntity						Interface						{ get { return m_Interface; } }

	// INTERNALS
	protected	float						m_Health						= 1f;
	protected	bool						m_IsActive						= true;
	protected 	uint						m_ID							= 0;
	protected	Section						m_SectionRef					= null;
	protected 	string						m_SectionName					= "None";
	protected 	ENTITY_TYPE					m_EntityType					= ENTITY_TYPE.NONE;
	protected	Rigidbody					m_RigidBody						= null;
	protected	CapsuleCollider				m_PhysicCollider				= null;
	protected	Transform					m_EffectsPivot					= null;
	protected	IEntity						m_Interface						= null;

	// AI
	protected	IBrain						m_Brain							= null;
	protected	TargetInfo_t				m_TargetInfo					= default( TargetInfo_t );
//	protected	Vector3						m_LastAttackPosition			= Vector3.zero;			// store start position of hitting bullet

	[SerializeField]
	protected	float						m_MinEngageDistance				= 0f;

//	[SerializeField]
	protected	RespawnPoint				m_RespawnPoint					= null;


	// CUTSCENE DATA
	protected	CutsceneEntityManager		m_CutsceneManager				= null;

	protected	bool						m_MovementOverrideEnabled		= false;
	protected	Vector3						m_SimulationStartPosition		= Vector3.zero;


	// NAVIGATION
	protected	bool						m_HasDestination				= false;
	protected	bool						m_HasFaceTarget					= false;
	protected	Vector3						m_Destination					= Vector3.zero;
	protected	Vector3						m_PointToFace					= Vector3.zero;
	protected	bool						m_IsMoving						= false;

	// Flag set if body of entity is aligned with target
	protected	bool						m_IsAllignedBodyToDestination	= false;

	// Flag set if gun of entity is aligned with target
	protected	bool						m_IsAllignedGunToPoint			= false;

	// Position saved at start of movement ( used for distances check )
	protected	Vector3						m_StartMovePosition				= Vector3.zero;
				Vector3						IEntitySimulation.StartPosition { get { return m_StartMovePosition; } set { m_StartMovePosition = value; } }
	protected	float						m_DistanceToTravel				= 0f;


	protected	bool 						m_IsOK							= false;




	//////////////////////////////////////////////////////////////////////////
	// Awake ( virtual )
	protected	virtual	void	Awake()
	{
		m_ID				= NewID();
		m_Interface			= this as IEntity;
		m_PhysicCollider	= GetComponent<CapsuleCollider>();
		m_RigidBody			= GetComponent<Rigidbody>();
		m_Brain				= GetComponent<IBrain>();

		m_EffectsPivot		= transform.Find( "EffectsPivot" );

		GameManager.Instance.OnSave += OnSave;
		GameManager.Instance.OnLoad += OnLoad;

		if ( this is Player )
			return;

		m_Brain.FieldOfView.OnTargetAquired = OnTargetAquired;
		m_Brain.FieldOfView.OnTargetChanged = OnTargetChanged;
		m_Brain.FieldOfView.OnTargetLost	= OnTargetLost;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave ( virtual )
	protected	virtual	StreamingUnit	OnSave( StreamingData streamingData )
	{
		if ( m_IsActive == false )
			return null;

		StreamingUnit streamingUnit		= streamingData.NewUnit( gameObject );
		streamingUnit.Position			= transform.position;
		streamingUnit.Rotation			= transform.rotation;

		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad ( virtual )
	protected	virtual		StreamingUnit	OnLoad( StreamingData streamingData )
	{
		StreamingUnit streamingUnit = null;
		if ( streamingData.GetUnit( gameObject, ref streamingUnit ) == false )
		{
			gameObject.SetActive( false );
			m_IsActive = false;
			return null;
		}

		gameObject.SetActive( true );
		m_IsActive						= true;

		// Entity
		m_TargetInfo					= default( TargetInfo_t );
		m_HasDestination				= false;
		m_HasFaceTarget					= false;
		m_Destination					= Vector3.zero;
		m_PointToFace					= Vector3.zero;
		m_IsMoving						= false;
		m_IsAllignedBodyToDestination	= false;
		m_StartMovePosition				= Vector3.zero;
		m_DistanceToTravel				= 0f;

		// NonLiveEntity
		m_IsAllignedGunToPoint			= false;

		transform.position = streamingUnit.Position;
		transform.rotation = streamingUnit.Rotation;
		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// SetDestination ( Virtual )
	public	virtual		void	SetDestination( Vector3 destination )
	{
		m_Destination = destination;
		m_HasDestination = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// SetPoinToFace ( Firtual )
	public	virtual		void	SetPoinToFace( Vector3 point )
	{
		m_PointToFace = point;
		m_HasFaceTarget = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// CanTrigger ( virtual )
	public	virtual		bool	CanTrigger()
	{
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// Update ( virtual )
	protected	virtual	void	Update()
	{
		if ( GameManager.IsPaused == true )
			return;

		// Only every 10 frames
		if ( Time.frameCount % 5 == 0 )
			return;

		this.OnFrame( Time.deltaTime );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnFrame ( Abstract )
	protected	abstract	void	OnFrame( float deltaTime );

	//////////////////////////////////////////////////////////////////////////
	// EnterSimulationState ( Abstract )
	public	abstract	void	EnterSimulationState();


	//////////////////////////////////////////////////////////////////////////
	// ExitSimulationState ( Abstract )
	public	abstract	void	ExitSimulationState();

	//////////////////////////////////////////////////////////////////////////
	// SimulateMovement ( Abstract )
	public virtual	bool	SimulateMovement( SimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget = 1f )
	{ return false; }

}
