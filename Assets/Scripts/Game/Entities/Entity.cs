
using UnityEngine;
using UnityEngine.AI;


public partial interface IEntity : IIdentificable<uint> {

	/// <summary> Generic flag for entity state </summary>
	bool					IsActive						{ get; }

	/// <summary> Entity Health </summary>
	float					Health							{ get; }

	/// <summary> Entity Type </summary>
	EEntityType				EntityType						{ get; }

	/// <summary> Entity Shield </summary>
	IShield					Shield							{ get; }

	/// <summary> Entity Section </summary>
	string					Section							{ get; }

	/// <summary> RigidBody </summary>
	Rigidbody				RigidBody						{ get; }

	/// <summary> Physic collider, only manage entity in space </summary>
	Collider				PhysicCollider					{ get; }

	/// <summary> Trigger collider, used for interactions with incoming objects or trigger areas
	Collider				TriggerCollider					{ get; }

	/// <summary> Transform where to play effects at </summary>
	Transform				EffectsPivot					{ get; }

	/// <summary> Group all entity class events and functions </summary>
	IEntityEvents			Events							{ get; }

	/// <summary> Is the group this entity belongs to </summary>
	IEntityGroup			GroupRef						{ get; }

	/// <summary> Entity brain </summary>
	IBrain					Brain							{ get; }

	/// <summary> Return the entity object </summary>
	Entity					AsEntity						{ get; }
}




//					Physics intreractions,		Entity volume,		   Navigation
[RequireComponent( typeof( Rigidbody ), typeof( Collider ), typeof( NavMeshAgent ) )]
public abstract partial class Entity : MonoBehaviour, IEntity, IIdentificable<uint> {

	private	static uint			CurrentID							= 0;
	public	static uint			NewID()										{ return CurrentID++; }

	// INTERFACE START
				bool						IEntity.IsActive				{	get { return m_IsActive;		}	}
				float						IEntity.Health					{	get { return m_Health;			}	}
				IShield						IEntity.Shield					{	get { return m_Shield;			}	}
				string						IEntity.Section					{	get { return m_SectionName;		}	}
				Rigidbody					IEntity.RigidBody				{	get { return m_RigidBody;		}	}
				Collider					IEntity.PhysicCollider			{	get { return m_PhysicCollider;	}	}
				Collider					IEntity.TriggerCollider			{	get { return m_TriggerCollider;	}	}
				Transform					IEntity.EffectsPivot			{	get { return m_EffectsPivot;	}	}
				EEntityType					IEntity.EntityType				{	get { return m_EntityType;		}	}
				IBrain						IEntity.Brain					{	get { return m_BrainInstance;	}	}
				IEntityEvents				IEntity.Events					{	get { return m_EventsInterface;	}	}
				IEntityGroup				IEntity.GroupRef				{	get { return m_EntityGroup;		}	}
				Entity						IEntity.AsEntity				{	get { return this;				}	}

				uint						IIdentificable<uint>.ID			{	get { return m_ID;				}	}
	// INTERFACE END

	// GETTERS START
	public		bool						IsAlive							{ get { return m_Health > 0.0f; } }
	public		bool						IsAllignedHeadToPoint			{ get { return m_IsAllignedHeadToPoint; } }
	public		bool						IsDisallignedHeadWithPoint		{ get { return m_IsDisallignedHeadWithPoint; } }
	public		bool						IsAllignedBodyToPoint			{ get { return m_IsAllignedBodyToPoint; } }
//	public		bool						IsAllignedGunToPoint			{ get { return m_IsAllignedGunToPoint; } }
	public		bool						HasLookAtObject					{ get { return m_LookData.HasLookAtObject; } }
	public		bool						HasDestination					{ get { return m_HasDestination; } }
	public		float						MinEngageDistance				{ get { return m_MinEngageDistance; } }
	public		float						MaxAgentSpeed					{ get { return m_MaxAgentSpeed; } }

	public		Vector3						HeadPosition					{ get { return m_HeadTransform.position; } }
	public		Quaternion					HeadRotation					{ get { return m_HeadTransform.rotation; } }

	public		Vector3						BodyPosition					{ get { return m_BodyTransform.position; } }
	public		Quaternion					BodyRotation					{ get { return m_BodyTransform.rotation; } }
	// GETTERS END

	public		IEntity						AsInterface						{ get { return m_Interface; } }

	// INTERNALS
	[Header("Entity Properties")]
	[SerializeField]
	protected	float						m_Health						= 1f;
	protected	IShield						m_Shield						= null;
	protected	bool						m_HasShield						= false;
	protected	bool						m_IsActive						= true;
	protected 	uint						m_ID							= 0;
	protected	Database.Section			m_SectionRef					= null;
	protected 	string						m_SectionName					= "None";
	protected 	EEntityType					m_EntityType					= EEntityType.NONE;
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

	protected	bool						m_IsDisallignedHeadWithPoint		= false;

	// Flag set if gun of entity is aligned with target
//	protected   bool                        m_IsAllignedGunToPoint			= false;

	// Transforms
	protected	Transform					m_HeadTransform					= null;
	protected	Transform					m_BodyTransform					= null;
//	protected	Transform					m_GunTransform					= null;
//	protected	Transform					m_FirePoint						= null;

	[Header("Orientation")]
	[SerializeField]
	protected	float						m_BodyRotationSpeed				= 5.0f;

	[SerializeField]
	protected	float						m_HeadRotationSpeed				= 5.0f;

	protected	Transform					m_Targettable					= null;

	protected	bool 						m_IsOK							= true;
	
	public		Vector3						SpawnPoint;
	public		Vector3						SpawnDirection;

	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void	Awake()
	{
		m_ID				= NewID();
		m_Interface			= this as IEntity;
		m_EventsInterface	= this as IEntityEvents;
		m_EntityGroup		= this as IEntityGroup;

		EnableEvents();

		// config file
		if ( GlobalManager.Configs.TryGetSection(m_SectionName, out m_SectionRef ) == false )
		{
			print( "Cannot find cfg section \""+ m_SectionName +"\" for entity " + name );
			Destroy(gameObject );
			return;
		}

		
		// TRANSFORMS
		{
			m_BodyTransform		= transform;//.Find( "Body" );
			m_HeadTransform		= m_BodyTransform.Find( "Head" );

			UnityEngine.Assertions.Assert.IsNotNull
			(
				m_HeadTransform,
				"Entity " + name + " has not head"
			);

			m_Targettable		= m_HeadTransform;
			m_EffectsPivot		= transform.Find( "EffectsPivot" );
		}


		SpawnPoint = m_BodyTransform.position;
		SpawnDirection = m_HeadTransform.forward;

		// ESSENTIALS CHECK
		m_IsOK = true;
		{
			// PHYSIC COLLIDER
			m_IsOK   =	Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL, out m_PhysicCollider, c => !c.isTrigger );

			// m_TriggerCollider
			m_IsOK   =	Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL, out m_TriggerCollider, c => c.isTrigger );

			// RIGIDBODY
			m_IsOK	&=	Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL, out m_RigidBody);
		}

		// SHIELD
		if (m_HasShield = Utils.Base.TrySearchComponent(gameObject, ESearchContext.CHILDREN, out m_Shield ) )
		{
			m_Shield.OnHit += OnShieldHit;
		}


		// CUTSCENE MANAGER
		m_HasCutsceneManager = Utils.Base.TrySearchComponent(gameObject, ESearchContext.CHILDREN, out m_CutsceneManager );

		// AI
		{
			// NAV AGENT
			m_HasNavAgent = Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL, out m_NavAgent);

			// FIELD OF VIEW
			m_HasFieldOfView = Utils.Base.TrySearchComponent(gameObject, ESearchContext.CHILDREN, out m_FieldOfView);

			// BLACKBOARD
			if ( Blackboard.IsEntityRegistered(m_ID ) == false )
			{
				m_BlackBoardData	= new EntityBlackBoardData(m_Targettable, m_HeadTransform, m_BodyTransform )
				{
					EntityRef		= this,
					LookData		= m_LookData,
					TargetInfo		= m_TargetInfo,
				};

				Blackboard.Register(m_ID, m_BlackBoardData );
			}
			m_BlackBoardData = Blackboard.GetData(m_ID );

			// BRAINSTATE
			m_CurrentBrainState = EBrainState.COUNT;
		}

		if (m_IsOK && m_HasNavAgent )
		{
			m_IsOK	&= m_NavAgent.isOnNavMesh;
		}

		if (m_IsOK == false )
		{
			print(name + " is not OK" );
		}

		// Executed only for non player entities
		if ( (m_EntityType == EEntityType.ACTOR ) == false )
		{
			Brain_Setup();             // Setup for field of view and memory
			Brain_SetActive( true );	// Brain updates activation

			// AI BEHAVIOURS
			{
				Brain.SetBehaviour( EBrainState.EVASIVE, m_SectionRef.AsString( "BehaviourEvasive"	), false );
				Brain.SetBehaviour( EBrainState.NORMAL, m_SectionRef.AsString( "BehaviourNormal"	), true  );
				Brain.SetBehaviour( EBrainState.ALARMED, m_SectionRef.AsString( "BehaviourAlarmed"	), false );
				Brain.SetBehaviour( EBrainState.SEEKER, m_SectionRef.AsString( "BehaviourSeeker"	), false );
				Brain.SetBehaviour( EBrainState.ATTACKER, m_SectionRef.AsString( "BehaviourAttacker"	), false );

				ChangeState( EBrainState.NORMAL );
			}
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
	/// <summary> Set the Transform to Look At </summary>
	public	virtual		void	SetTransformToLookAt( Transform t, ELookTargetMode LookMode = ELookTargetMode.HEAD_ONLY )
	{
		m_LookData.HasLookAtObject		= true;
		m_LookData.TransformToLookAt		= t;
		m_LookData.PointToLookAt		= Vector3.zero;
		m_LookData.LookTargetType		= ELookTargetType.TRANSFORM;
		m_LookData.LookTargetMode		= LookMode;
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Set the point to Look At </summary>
	public	virtual		void	SetPointToLookAt( Vector3 point, ELookTargetMode LookMode = ELookTargetMode.HEAD_ONLY )
	{
		m_LookData.HasLookAtObject		= true;
		m_LookData.TransformToLookAt		= null;
		m_LookData.PointToLookAt		= point;
		m_LookData.LookTargetType		= ELookTargetType.POSITION;
		m_LookData.LookTargetMode		= LookMode;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Stop looking to target point or target </summary>
	public	virtual		void	StopLooking()
	{
		m_LookData.HasLookAtObject		= false;
		m_LookData.TransformToLookAt		= null;
		m_LookData.PointToLookAt		= Vector3.zero;
		m_LookData.LookTargetType		= ELookTargetType.POSITION;
		m_LookData.LookTargetMode		= ELookTargetMode.HEAD_ONLY;
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
	public		virtual	void	FireWeapon()
	{

	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Set the Collision state with another collider </summary>
	public				void	SetCollisionStateWith( Collider coll, bool state )
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
		if (m_LookData.HasLookAtObject == false )
			return;

		// HEAD
		{
			Vector3 pointToLookAt = m_LookData.LookTargetType == ELookTargetType.TRANSFORM ? m_LookData.TransformToLookAt.position : m_LookData.PointToLookAt;

			// point on the head 'Horizontal'  plane
			Vector3 pointOnHeadPlane	= Utils.Math.ProjectPointOnPlane( m_BodyTransform.up, m_HeadTransform.position, pointToLookAt );

			// point on the entity 'Horizontal' plane
			Vector3 pointOnEntityPlane	= Utils.Math.ProjectPointOnPlane( m_BodyTransform.up, transform.position, pointToLookAt );

			// Direction from head to point
			Vector3 dirHeadToPosition	= ( pointOnHeadPlane - m_HeadTransform.position );

			// Direction from entity to point
			Vector3 dirEntityToPosition	= ( pointOnEntityPlane - transform.position );

			// Angle between head and projected point
			float lookDeltaAngle = Vector3.Angle( m_HeadTransform.forward, dirHeadToPosition );

			// Current head allignment state
			bool isCurrentlyAlligned = lookDeltaAngle < 4f;
		
			// Head allignment comparison and event
			{
				bool wasPreviousAlligned = m_IsAllignedHeadToPoint;
				if ( wasPreviousAlligned == false && isCurrentlyAlligned == true )
				{
					OnLookRotationReached(m_HeadTransform.forward );
				}
			}

			// Flags assignment
			m_IsAllignedHeadToPoint			= isCurrentlyAlligned;
			m_IsDisallignedHeadWithPoint	= lookDeltaAngle > 90f;
			
			// Rotation Speed
			float rotationSpeed = m_HeadRotationSpeed * ( (m_TargetInfo.HasTarget ) ? 3.0f : 1.0f ) * Time.deltaTime;

			// Execute Rotation
			if (m_LookData.LookTargetMode == ELookTargetMode.WITH_BODY )
			{
				m_RotationToAllignTo.SetLookRotation( dirEntityToPosition, m_BodyTransform.up );
				transform.rotation = Quaternion.RotateTowards(transform.rotation, m_RotationToAllignTo, rotationSpeed );
			}
			// Head only
			else
			{
				m_RotationToAllignTo.SetLookRotation( dirHeadToPosition, m_BodyTransform.up );
				m_HeadTransform.rotation = Quaternion.RotateTowards(m_HeadTransform.rotation, m_RotationToAllignTo, rotationSpeed );
			}
		}
	}

}



public enum EEntityType : uint {
	NONE,
	ACTOR,
	HUMAN,
	ROBOT,
	ANIMAL,
	OBJECT
};

public	enum ELookTargetType : uint {
	POSITION,
	TRANSFORM
};

public	enum ELookTargetMode : uint {
	HEAD_ONLY,
	WITH_BODY
}

public class LookData {
	public	bool			HasLookAtObject		= false;
	public	Vector3			PointToLookAt		= Vector3.zero;
	public	Transform		TransformToLookAt	= null;
	public	ELookTargetType	LookTargetType		= ELookTargetType.POSITION;
	public	ELookTargetMode	LookTargetMode		= ELookTargetMode.HEAD_ONLY;
};