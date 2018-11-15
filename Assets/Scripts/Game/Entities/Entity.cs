
using UnityEngine;
using Database;
using UnityEngine.AI;

public partial interface IEntity {
	// Entity Transform
	Transform				Transform						{ get; }

	// Generic flag for entity state
	bool					IsActive						{ get; }

	// Entity unique ID
	uint					ID								{ get; }

	// Entity Health
	float					Health							{ get; }

	// Entity Type
	ENTITY_TYPE				EntityType						{ get; }

	// Entity Shield
	Shield					Shield							{ get; }

	// Entity Section
	string					Section							{ get; }

	// RigidBody
	Rigidbody				RigidBody						{ get; }

	// Physic collider, only manage entity in space
	Collider				PhysicCollider					{ get; }

	// Trigger collider, used for interactions with incoming objects or trigger areas
	Collider				TriggerCollider					{ get; }

	// Transform where to play effects at
	Transform				EffectsPivot					{ get; }

	// Entity brain
	IBrain					Brain							{ get; }
}




//					Physics intreractions,		Entity volume,		   Navigation
[RequireComponent( typeof( Rigidbody ), typeof( CapsuleCollider ), typeof( NavMeshAgent ) ) ]
public abstract partial class Entity : MonoBehaviour, IEntity {

	private	static uint			CurrentID							= 0;
	public	static uint			NewID()								{ return CurrentID++; }

	// INTERFACE START
				Transform				IEntity.Transform					{	get { return m_Targettable;		}	}
				bool					IEntity.IsActive					{	get { return m_IsActive;		}	}
				uint					IEntity.ID							{	get { return m_ID;				}	}
				float					IEntity.Health						{	get { return m_Health;			}	}
				Shield					IEntity.Shield						{	get { return m_Shield;			}	}
				string					IEntity.Section						{	get { return m_SectionName;		}	}
				Rigidbody				IEntity.RigidBody					{	get { return m_RigidBody;		}	}
				Collider				IEntity.PhysicCollider				{	get { return m_PhysicCollider;	}	}
				Collider				IEntity.TriggerCollider				{	get { return m_TriggerCollider;	}	}
				Transform				IEntity.EffectsPivot				{	get { return m_EffectsPivot;	}	}
				IBrain					IEntity.Brain						{	get { return this;				}	}
				ENTITY_TYPE				IEntity.EntityType					{	get { return m_EntityType;		}	}
	// INTERFACE END

	// GETTERS START
	public		uint						Id								{ get { return m_ID; } }
	public		bool						IsAlive							{ get { return m_Health > 0.0f; } }
	public		bool						IsAllignedHeadToPoint			{ get { return m_IsAllignedHeadToPoint; } }
	public		bool						IsAllignedBodyToPoint			{ get { return m_IsAllignedBodyToPoint; } }
	public		bool						IsAllignedGunToPoint			{ get { return m_IsAllignedGunToPoint; } }
	public		bool						HasLookAtObject					{ get { return m_LookData.HasLookAtObject; } }
	public		bool						HasDestination					{ get { return m_HasDestination; } }
	public		float						MinEngageDistance				{ get { return m_MinEngageDistance; } }
	public		float						MaxAgentSpeed					{ get { return m_MaxAgentSpeed; } }

	public		Vector3						HeadPosition					{ get { return m_HeadTransform.position; } }
	public		Quaternion					HeadRotation					{ get { return m_HeadTransform.rotation; } }

	public		Vector3						BodyPosition					{ get { return m_BodyTransform.position; } }
	public		Quaternion					BodyRotation					{ get { return m_BodyTransform.rotation; } }
	// GETTERS END

	public		IEntity						Interface						{ get { return m_Interface; } }

	// INTERNALS
	[Header("Entity Properties")]
	[SerializeField]
	protected	float						m_Health						= 1f;
	protected	Shield						m_Shield						= null;
	protected	bool						m_IsActive						= true;
	protected 	uint						m_ID							= 0;
	protected	Section						m_SectionRef					= null;
	protected 	string						m_SectionName					= "None";
	protected 	ENTITY_TYPE					m_EntityType					= ENTITY_TYPE.NONE;
	protected	Rigidbody					m_RigidBody						= null;
	protected	Collider					m_PhysicCollider				= null;
	protected	Collider					m_TriggerCollider				= null;
	protected	Transform					m_EffectsPivot					= null;
	protected	IEntity						m_Interface						= null;

	// AI
	protected	TargetInfo					m_TargetInfo					= new TargetInfo();
	protected	EntityBlackBoardData		m_BlackBoardData				= null;

	[SerializeField]
	protected	float						m_MinEngageDistance				= 0f;

//	[SerializeField]
	protected	RespawnPoint				m_RespawnPoint					= null;


	// ORIENTATION
	protected	Quaternion					m_RotationToAllignTo			= Quaternion.identity;
	protected	LookData					m_LookData						= new LookData();


	// NAVIGATION
	protected	bool						m_HasDestination				= false;
	protected	Vector3						m_DestinationToReachPosition	= Vector3.zero;
	protected	float						m_MaxAgentSpeed					= 0.0f;


	// Flag set if body of entity is aligned with target
	protected	bool						m_IsAllignedBodyToPoint			= false;

	// Flag set if head of entity is aligned with target
	protected	bool						m_IsAllignedHeadToPoint			= false;

	// Flag set if gun of entity is aligned with target
	protected   bool                        m_IsAllignedGunToPoint			= false;

	// Transforms
	protected	Transform					m_HeadTransform					= null;
	protected	Transform					m_BodyTransform					= null;
	protected	Transform					m_GunTransform					= null;
	protected	Transform					m_FirePoint						= null;

	[Header("Orientation")]
	[SerializeField]
	protected	float						m_BodyRotationSpeed				= 5.0f;

	[SerializeField]
	protected	float						m_HeadRotationSpeed				= 5.0f;

	protected	Transform					m_Targettable					= null;

	protected	bool 						m_IsOK							= true;




	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void	Awake()
	{
		m_ID				= NewID();
		m_Interface			= this as IEntity;

		// TRANSFORMS
		{
			m_BodyTransform		= transform.Find( "Body" );
			m_HeadTransform		= m_BodyTransform.Find( "Head" );
			m_Targettable		= m_HeadTransform;
			m_EffectsPivot		= transform.Find( "EffectsPivot" );
		}

		// ESSENTIALS CHECK
		m_IsOK = true;
		{
			// PHYSIC COLLIDER
			m_IsOK   =	Utils.Base.SearchComponent( gameObject, ref m_PhysicCollider, SearchContext.LOCAL, (p) => { return p && p.isTrigger == false; } );

			// TRIGGER COLLIDER ( PLAYER ONLY )
			if ( m_EntityType == ENTITY_TYPE.ACTOR )
			{
				m_IsOK	&= Utils.Base.SearchComponent( gameObject, ref m_TriggerCollider, SearchContext.LOCAL, (p) => { return p && p.isTrigger == true;  } );
			}

			// RIGIDBODY
			m_IsOK	&=	Utils.Base.SearchComponent( gameObject, ref m_RigidBody, SearchContext.LOCAL );

			if ( m_IsOK && m_NavAgent != null && ( m_EntityType == ENTITY_TYPE.ACTOR ) == false )
			{
				m_IsOK	&= m_NavAgent.isOnNavMesh;
			}

			if ( m_IsOK == false && ( m_EntityType == ENTITY_TYPE.ACTOR ) == false )
			{
				print( name + " is not OK" );
			}
		}

		// SHIELD
		Utils.Base.SearchComponent( gameObject, ref m_Shield, SearchContext.CHILDREN );

		// CUTSCENE MANAGER
		Utils.Base.SearchComponent( gameObject, ref m_CutsceneManager, SearchContext.CHILDREN );

		// AI
		{
			// NAV AGENT
			Utils.Base.SearchComponent( gameObject, ref m_NavAgent, SearchContext.LOCAL	);

			// FIELD OF VIEW
			Utils.Base.SearchComponent( gameObject, ref m_FieldOfView, SearchContext.CHILDREN );

			// BLACKBOARD
			if ( Blackboard.IsEntityRegistered( m_ID ) == false )
			{
				m_BlackBoardData	= new EntityBlackBoardData()
				{
					EntityRef		= this,
					Transform		= m_Targettable,
					HeadTransform	= m_HeadTransform,
					BodyTransform	= m_BodyTransform,
					LookData		= m_LookData,
					TargetInfo		= m_TargetInfo,
				};

				Blackboard.Register( m_ID, m_BlackBoardData );
			}
			m_BlackBoardData = Blackboard.GetData( m_ID );

			// BRAINSTATE
			m_CurrentBrainState = BrainState.COUNT;
		}

	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void	Start()
	{

	}

/*
	//////////////////////////////////////////////////////////////////////////
	private					void	FixedUpdate()
	{
		if ( GameManager.IsPaused == true )
			return;

//		this.OnPhysicFrame( Time.fixedDeltaTime );
	}
*/

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Set the trasform to Look At </summary>
	public	virtual		void	SetTrasformTolookAt( Transform t )
	{
		m_LookData.HasLookAtObject		= true;
		m_LookData.TrasformToLookAt		= t;
		m_LookData.PointToLookAt		= Vector3.zero;
		m_LookData.LookTargetType		= LookTargetType.TRANSFORM;
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Set the point to Look At </summary>
	public	virtual		void	SetPointToLookAt( Vector3 point )
	{
		m_LookData.HasLookAtObject		= true;
		m_LookData.TrasformToLookAt		= null;
		m_LookData.PointToLookAt		= point;
		m_LookData.LookTargetType		= LookTargetType.POSITION;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Stop looking to target point or target </summary>
	public	virtual		void	StopLooking()
	{
		m_LookData.HasLookAtObject		= false;
		m_LookData.TrasformToLookAt		= null;
		m_LookData.PointToLookAt		= Vector3.zero;
		m_LookData.LookTargetType		= LookTargetType.POSITION;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Return if this player can trigger with Trigger Areas </summary>
	public		virtual		bool	CanTrigger()
	{
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Return if this player can fire </summary>
	public		virtual	bool	CanFire()
	{
		return false;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Set the Collision state with another collider </summary>
	protected				void	SetCollisionStateWith( Collider coll, bool state )
	{
		Collider[] thisColliders = GetComponentsInChildren<Collider>( includeInactive: true );
		for ( int i = 0; i < thisColliders.Length; i++ )
		{
			Collider thisColl = thisColliders[i];
			Physics.IgnoreCollision( thisColl, coll, ignore: !state );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	UpdateHeadRotation()
	{
		if ( m_LookData.HasLookAtObject == false )
			return;

		// HEAD
		{
			Vector3 pointOnThisPlane = Utils.Math.ProjectPointOnPlane( m_BodyTransform.up, m_HeadTransform.position, m_LookData.PointToLookAt );
			Vector3 dirToPosition = ( pointOnThisPlane - m_HeadTransform.position );

			m_IsAllignedHeadToPoint = Vector3.Angle( m_HeadTransform.forward, dirToPosition ) < 12f;
			{
				float speed = m_HeadRotationSpeed * ( ( m_TargetInfo.HasTarget ) ? 3.0f : 1.0f );

				if ( dirToPosition != Vector3.zero )
				{
					m_RotationToAllignTo.SetLookRotation( dirToPosition, m_BodyTransform.up );
					m_HeadTransform.rotation = Quaternion.RotateTowards( m_HeadTransform.rotation, m_RotationToAllignTo, speed * Time.deltaTime );
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual	void	FireLongRange()
	{

	}

}
