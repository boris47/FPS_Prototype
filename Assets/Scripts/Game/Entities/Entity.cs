
using UnityEngine;
using CFG_Reader;
using CutScene;
using AI;

public partial interface IEntity {
	// Entity Transform
	Transform				Transform						{	get;		}

	// Generic flag for entity state
	bool					IsActive						{	get;		}

	// Entity unique ID
	uint					ID								{	get;		}

	// Entity Health
	float					Health							{	get;		}

	// Entity Section
	string					Section							{	get;		}

	// RigidBody
	Rigidbody				RigidBody						{	get;		}

	// Physic collider, only manage entity in space
	Collider				PhysicCollider					{	get;		}

	// Trigger collider, used for interactions with incoming objects or trigger areas
	CapsuleCollider			TriggerCollider					{	get;		}

	// Transform where to play effects at
	Transform				EffectsPivot					{	get;		}

	// Entity brain
	IBrain					Brain							{	get;		}

	// Cutscene manager, that take control over entity during cutscene sequences
	CutsceneEntityManager	CutsceneManager					{	get; set;	}

	bool					CanTrigger();
}


public interface IEntitySimulation {
	Vector3		StartPosition			{ get; set; }

	void		EnterSimulationState	();
	void		ExitSimulationState		();
	bool		SimulateMovement		( Entity.SimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget = 1f );

}

//					Physics intreractions,		entity volume,		Hit detection
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
				Collider				IEntity.PhysicCollider				{	get { return m_PhysicCollider;	}	}
				CapsuleCollider			IEntity.TriggerCollider				{	get { return m_TriggerCollider;	}	}
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
	protected	Collider					m_PhysicCollider				= null;
	protected	CapsuleCollider				m_TriggerCollider				= null;
	protected	Transform					m_EffectsPivot					= null;
	protected	IEntity						m_Interface						= null;

	// AI
	protected	IBrain						m_Brain							= null;
	protected	TargetInfo_t				m_TargetInfo					= default( TargetInfo_t );

	[SerializeField]
	protected	float						m_MinEngageDistance				= 0f;

//	[SerializeField]
	protected	RespawnPoint				m_RespawnPoint					= null;


	// CUTSCENE MANAGER
	protected	CutsceneEntityManager		m_CutsceneManager				= null;

	protected	bool						m_MovementOverrideEnabled		= false;
	protected	Vector3						m_SimulationStartPosition		= Vector3.zero;


	// NAVIGATION
	protected	bool						m_HasPointToFace				= false;
	protected	Vector3						m_PointToFace					= Vector3.zero;

	// Flag set if body of entity is aligned with target
	protected	bool						m_IsAllignedBodyToDestination	= false;

	// Flag set if head of entity is aligned with target
	protected	bool						m_IsAllignedHeadToPoint			= false;

	// Flag set if gun of entity is aligned with target
	protected   bool                        m_IsAllignedGunToPoint         = false;

	protected	int							m_TargetNodeIndex				= 0;

	
				Vector3						IEntitySimulation.StartPosition { get; set; }


	protected	bool 						m_IsOK							= false;




	//////////////////////////////////////////////////////////////////////////
	// Awake ( virtual )
	protected	virtual	void	Awake()
	{
		m_ID				= NewID();
		m_Interface			= this as IEntity;

		if ( ( m_PhysicCollider	= GetComponent<MeshCollider>() ) == null )
		{
			m_PhysicCollider	= GetComponent<CapsuleCollider>();
		}
		m_TriggerCollider	= GetComponent<CapsuleCollider>();
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
	protected	virtual		StreamingUnit	OnSave( StreamingData streamingData )
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
		m_NavHasDestination				= false;

		m_NavCanMoveAlongPath			= false;
		m_IsAllignedBodyToDestination	= false;
		m_StartMovePosition				= Vector3.zero;

		// NonLiveEntity
		m_IsAllignedHeadToPoint			= false;

		transform.position = streamingUnit.Position;
		transform.rotation = streamingUnit.Rotation;
		return streamingUnit;
	}


	


	//////////////////////////////////////////////////////////////////////////
	// SetPoinToFace ( Firtual )
	public	virtual		void	SetPoinToFace( Vector3 point )
	{
		m_PointToFace		= point;
		m_HasPointToFace	= true;
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
//		if ( Time.frameCount % 5 == 0 )
//			return;

		this.OnFrame( Time.deltaTime );
	}


	//////////////////////////////////////////////////////////////////////////
	// SetCollisionStateWith
	protected	void	SetCollisionStateWith( Collider coll, bool state )
	{
		Collider[] thisColliders = GetComponentsInChildren<Collider>( includeInactive: true );
		for ( int i = 0; i < thisColliders.Length; i++ )
		{
			Collider thisColl = thisColliders[i];
			Physics.IgnoreCollision( thisColl, coll, ignore: !state );
		}

	}


	//////////////////////////////////////////////////////////////////////////
	// OnFrame ( Abstract )
	protected	abstract	void	OnFrame( float deltaTime );

	//////////////////////////////////////////////////////////////////////////
	// EnterSimulationState ( Abstract )
	public		abstract	void	EnterSimulationState();


	//////////////////////////////////////////////////////////////////////////
	// ExitSimulationState ( Abstract )
	public		abstract	void	ExitSimulationState();

	//////////////////////////////////////////////////////////////////////////
	// SimulateMovement ( Abstract )
	public		virtual		bool	SimulateMovement( SimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget = 1f )
	{ return false; }

}
