
using UnityEngine;
using CFG_Reader;
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
	Entity.ENTITY_TYPE		EntityType						{ get; }

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

	public enum ENTITY_TYPE {
		NONE,
		ACTOR,
		HUMAN,
		ROBOT,
		ANIMAL,
		OBJECT
	};

	[Header("Entity Properties")]
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
	public		bool						IsAllignedHeadToPoin			{ get { return m_IsAllignedHeadToPoint; } }
	public		bool						IsAllignedBodyToPoint			{ get { return m_IsAllignedBodyToPoint; } }
	public		bool						IsAllignedGunToPoint			{ get { return m_IsAllignedGunToPoint; } }
	public		bool						HasLookAtObject					{ get { return m_HasLookAtObject; } }
	public		bool						HasDestination					{ get { return m_HasDestination; } }
	public		float						MinEngageDistance				{ get { return m_MinEngageDistance; } }
	// GETTERS END


	public		IEntity						Interface						{ get { return m_Interface; } }

	// INTERNALS
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
	protected	bool						m_IsPlayer						= true;

	// AI
	protected	TargetInfo_t				m_TargetInfo					= default( TargetInfo_t );

	[SerializeField]
	protected	float						m_MinEngageDistance				= 0f;

//	[SerializeField]
	protected	RespawnPoint				m_RespawnPoint					= null;


	// ORIENTATION
	protected	bool						m_HasLookAtObject				= false;
	protected	Vector3						m_PointToLookAt					= Vector3.zero;
	protected	Transform					m_TrasformToLookAt				= null;
	protected	Quaternion					m_RotationToAllignTo			= Quaternion.identity;


	// NAVIGATION
	protected	bool						m_HasDestination				= false;
	protected	Vector3						m_DestinationToReachPosition	= Vector3.zero;


	// Flag set if foots of entity is aligned with target
	protected	bool						m_IsAllignedFootsToDestination	= false;

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

		m_IsPlayer = ( m_EntityType == ENTITY_TYPE.ACTOR );

		m_EffectsPivot		= transform.Find( "EffectsPivot" );

		if ( m_IsPlayer == false )
		{
			m_BodyTransform		= transform.Find( "Body" );
			m_HeadTransform		= m_BodyTransform.Find( "Head" );
			m_Targettable = m_HeadTransform;
		}
		else
		{
			m_Targettable = transform;
		}

		m_IsOK   =	Utils.Base.SearchComponent( gameObject, ref m_PhysicCollider,		SearchContext.LOCAL, (p) => { return p && p.isTrigger == false; } );

		if ( m_IsPlayer == true )
		{
			m_IsOK	&= Utils.Base.SearchComponent( gameObject, ref m_TriggerCollider,		SearchContext.LOCAL, (p) => { return p && p.isTrigger == true;  } );
		}

		m_IsOK	&=	Utils.Base.SearchComponent( gameObject, ref m_RigidBody,			SearchContext.LOCAL );

		Utils.Base.SearchComponent( gameObject, ref m_NavAgent,				SearchContext.LOCAL	);

		if ( m_IsOK && m_NavAgent != null && m_IsPlayer == false )
			m_IsOK	&= m_NavAgent.isOnNavMesh;

		if ( m_IsOK == false && m_IsPlayer == false )
			print( name + " is not OK" );

		Utils.Base.SearchComponent( gameObject, ref m_Shield,				SearchContext.CHILDREN );
		Utils.Base.SearchComponent( gameObject, ref m_CutsceneManager,		SearchContext.CHILDREN );


		Blackboard.Register( m_ID );
		
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
		m_TrasformToLookAt	= t;
		m_PointToLookAt		= Vector3.zero;
		m_HasLookAtObject	= true;
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Set the point to Look At </summary>
	public	virtual		void	SetPointToLookAt( Vector3 point )
	{
		m_PointToLookAt		= point;
		m_TrasformToLookAt	= null;
		m_HasLookAtObject	= true;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Stop looking to target point or target </summary>
	public	virtual		void	StopLooking()
	{
		m_HasLookAtObject = false;
		m_TrasformToLookAt = null;
		m_PointToLookAt = Vector3.zero;
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
		if ( m_HasLookAtObject == false )
			return;

		// HEAD
		{
			Vector3 pointOnThisPlane = Utils.Math.ProjectPointOnPlane( m_BodyTransform.up, m_HeadTransform.position, m_PointToLookAt );
			Vector3 dirToPosition = ( pointOnThisPlane - m_HeadTransform.position );

			m_IsAllignedHeadToPoint = Vector3.Angle( m_HeadTransform.forward, dirToPosition ) < 12f;
			{
				float speed = m_HeadRotationSpeed * ( ( m_TargetInfo.HasTarget ) ? 3.0f : 1.0f );

				m_RotationToAllignTo.SetLookRotation( dirToPosition, m_BodyTransform.up );
				m_HeadTransform.rotation = Quaternion.RotateTowards( m_HeadTransform.rotation, m_RotationToAllignTo, speed * Time.deltaTime );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	abstract	void	FireLongRange();

}
