
using UnityEngine;
using Database;
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
				bool						IEntity.IsActive				{	get { return this.m_IsActive;		}	}
				float						IEntity.Health					{	get { return this.m_Health;			}	}
				IShield						IEntity.Shield					{	get { return this.m_Shield;			}	}
				string						IEntity.Section					{	get { return this.m_SectionName;		}	}
				Rigidbody					IEntity.RigidBody				{	get { return this.m_RigidBody;		}	}
				Collider					IEntity.PhysicCollider			{	get { return this.m_PhysicCollider;	}	}
				Collider					IEntity.TriggerCollider			{	get { return this.m_TriggerCollider;	}	}
				Transform					IEntity.EffectsPivot			{	get { return this.m_EffectsPivot;	}	}
				EEntityType					IEntity.EntityType				{	get { return this.m_EntityType;		}	}
				IBrain						IEntity.Brain					{	get { return this.m_BrainInstance;	}	}
				IEntityEvents				IEntity.Events					{	get { return this.m_EventsInterface;	}	}
				IEntityGroup				IEntity.GroupRef				{	get { return this.m_EntityGroup;		}	}
				Entity						IEntity.AsEntity				{	get { return this;				}	}

				uint						IIdentificable<uint>.ID			{	get { return this.m_ID;				}	}
	// INTERFACE END

	// GETTERS START
	public		bool						IsAlive							{ get { return this.m_Health > 0.0f; } }
	public		bool						IsAllignedHeadToPoint			{ get { return this.m_IsAllignedHeadToPoint; } }
	public		bool						IsDisallignedHeadWithPoint		{ get { return this.m_IsDisallignedHeadWithPoint; } }
	public		bool						IsAllignedBodyToPoint			{ get { return this.m_IsAllignedBodyToPoint; } }
	public		bool						IsAllignedGunToPoint			{ get { return this.m_IsAllignedGunToPoint; } }
	public		bool						HasLookAtObject					{ get { return this.m_LookData.HasLookAtObject; } }
	public		bool						HasDestination					{ get { return this.m_HasDestination; } }
	public		float						MinEngageDistance				{ get { return this.m_MinEngageDistance; } }
	public		float						MaxAgentSpeed					{ get { return this.m_MaxAgentSpeed; } }

	public		Vector3						HeadPosition					{ get { return this.m_HeadTransform.position; } }
	public		Quaternion					HeadRotation					{ get { return this.m_HeadTransform.rotation; } }

	public		Vector3						BodyPosition					{ get { return this.m_BodyTransform.position; } }
	public		Quaternion					BodyRotation					{ get { return this.m_BodyTransform.rotation; } }
	// GETTERS END

	public		IEntity						AsInterface						{ get { return this.m_Interface; } }

	// INTERNALS
	[Header("Entity Properties")]
	[SerializeField]
	protected	float						m_Health						= 1f;
	protected	IShield						m_Shield						= null;
	protected	bool						m_HasShield						= false;
	protected	bool						m_IsActive						= true;
	protected 	uint						m_ID							= 0;
	protected	Section						m_SectionRef					= null;
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
	
	public		Vector3						SpawnPoint;
	public		Vector3						SpawnDirection;

	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void	Awake()
	{
		this.m_ID				= NewID();
		this.m_Interface			= this as IEntity;
		this.m_EventsInterface	= this as IEntityEvents;
		this.m_EntityGroup		= this as IEntityGroup;

		this.EnableEvents();

		// config file
		if ( GlobalManager.Configs.GetSection(this.m_SectionName, ref this.m_SectionRef ) == false )
		{
			print( "Cannot find cfg section \""+ this.m_SectionName +"\" for entity " + this.name );
			Destroy(this.gameObject );
			return;
		}

		
		// TRANSFORMS
		{
			this.m_BodyTransform		= this.transform;//.Find( "Body" );
			this.m_HeadTransform		= this.m_BodyTransform.Find( "Head" );

			UnityEngine.Assertions.Assert.IsNotNull
			(
				this.m_HeadTransform,
				"Entity " + this.name + " has not head"
			);

			this.m_Targettable		= this.m_HeadTransform;
			this.m_EffectsPivot		= this.transform.Find( "EffectsPivot" );
		}


		this.SpawnPoint = this.m_BodyTransform.position;
		this.SpawnDirection = this.m_HeadTransform.forward;

		// ESSENTIALS CHECK
		this.m_IsOK = true;
		{
			// PHYSIC COLLIDER
			this.m_IsOK   =	Utils.Base.SearchComponent(this.gameObject, out this.m_PhysicCollider, ESearchContext.LOCAL, c => !c.isTrigger );

			// m_TriggerCollider
			this.m_IsOK   =	Utils.Base.SearchComponent(this.gameObject, out this.m_TriggerCollider, ESearchContext.LOCAL, c => c.isTrigger );

			// RIGIDBODY
			this.m_IsOK	&=	Utils.Base.SearchComponent(this.gameObject, out this.m_RigidBody, ESearchContext.LOCAL );
		}

		// SHIELD
		if (this.m_HasShield = Utils.Base.SearchComponent(this.gameObject, out this.m_Shield, ESearchContext.CHILDREN ) )
		{
			this.m_Shield.OnHit += this.OnShieldHit;
		}


		// CUTSCENE MANAGER
		this.m_HasCutsceneManager = Utils.Base.SearchComponent(this.gameObject, out this.m_CutsceneManager, ESearchContext.CHILDREN );

		// AI
		{
			// NAV AGENT
			this.m_HasNavAgent = Utils.Base.SearchComponent(this.gameObject, out this.m_NavAgent, ESearchContext.LOCAL	);

			// FIELD OF VIEW
			this.m_HasFieldOfView = Utils.Base.SearchComponent(this.gameObject, out this.m_FieldOfView, ESearchContext.CHILDREN );

			// BLACKBOARD
			if ( Blackboard.IsEntityRegistered(this.m_ID ) == false )
			{
				this.m_BlackBoardData	= new EntityBlackBoardData(this.m_Targettable, this.m_HeadTransform, this.m_BodyTransform )
				{
					EntityRef		= this,
					LookData		= m_LookData,
					TargetInfo		= m_TargetInfo,
				};

				Blackboard.Register(this.m_ID, this.m_BlackBoardData );
			}
			this.m_BlackBoardData = Blackboard.GetData(this.m_ID );

			// BRAINSTATE
			this.m_CurrentBrainState = EBrainState.COUNT;
		}

		if (this.m_IsOK && this.m_HasNavAgent )
		{
			this.m_IsOK	&= this.m_NavAgent.isOnNavMesh;
		}

		if (this.m_IsOK == false )
		{
			print(this.name + " is not OK" );
		}

		// Executed only for non player entities
		if ( (this.m_EntityType == EEntityType.ACTOR ) == false )
		{
			this.Brain_Setup();             // Setup for field of view and memory
			this.Brain_SetActive( true );	// Brain updates activation

			// AI BEHAVIOURS
			{
				this.Brain.SetBehaviour( EBrainState.EVASIVE, this.m_SectionRef.AsString( "BehaviourEvasive"	), false );
				this.Brain.SetBehaviour( EBrainState.NORMAL, this.m_SectionRef.AsString( "BehaviourNormal"	), true  );
				this.Brain.SetBehaviour( EBrainState.ALARMED, this.m_SectionRef.AsString( "BehaviourAlarmed"	), false );
				this.Brain.SetBehaviour( EBrainState.SEEKER, this.m_SectionRef.AsString( "BehaviourSeeker"	), false );
				this.Brain.SetBehaviour( EBrainState.ATTACKER, this.m_SectionRef.AsString( "BehaviourAttacker"	), false );

				this.ChangeState( EBrainState.NORMAL );
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
		this.m_LookData.HasLookAtObject		= true;
		this.m_LookData.TransformToLookAt		= t;
		this.m_LookData.PointToLookAt		= Vector3.zero;
		this.m_LookData.LookTargetType		= ELookTargetType.TRANSFORM;
		this.m_LookData.LookTargetMode		= LookMode;
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Set the point to Look At </summary>
	public	virtual		void	SetPointToLookAt( Vector3 point, ELookTargetMode LookMode = ELookTargetMode.HEAD_ONLY )
	{
		this.m_LookData.HasLookAtObject		= true;
		this.m_LookData.TransformToLookAt		= null;
		this.m_LookData.PointToLookAt		= point;
		this.m_LookData.LookTargetType		= ELookTargetType.POSITION;
		this.m_LookData.LookTargetMode		= LookMode;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Stop looking to target point or target </summary>
	public	virtual		void	StopLooking()
	{
		this.m_LookData.HasLookAtObject		= false;
		this.m_LookData.TransformToLookAt		= null;
		this.m_LookData.PointToLookAt		= Vector3.zero;
		this.m_LookData.LookTargetType		= ELookTargetType.POSITION;
		this.m_LookData.LookTargetMode		= ELookTargetMode.HEAD_ONLY;
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
	public				void	SetCollisionStateWith( Collider coll, bool state )
	{
		Collider[] thisColliders = this.GetComponentsInChildren<Collider>( includeInactive: true );
		for ( int i = 0; i < thisColliders.Length; i++ )
		{
			Collider thisColl = thisColliders[i];
			Physics.IgnoreCollision( thisColl, coll, ignore: !state );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	UpdateHeadRotation()
	{
		if (this.m_LookData.HasLookAtObject == false )
			return;

		// HEAD
		{
			Vector3 pointToLookAt = this.m_LookData.LookTargetType == ELookTargetType.TRANSFORM ? this.m_LookData.TransformToLookAt.position : this.m_LookData.PointToLookAt;

			// point on the head 'Horizontal'  plane
			Vector3 pointOnHeadPlane	= Utils.Math.ProjectPointOnPlane(this.m_BodyTransform.up, this.m_HeadTransform.position,		 pointToLookAt );

			// point on the entity 'Horizontal' plane
			Vector3 pointOnEntityPlane	= Utils.Math.ProjectPointOnPlane(this.m_BodyTransform.up, this.transform.position, pointToLookAt );

			// Direction from head to point
			Vector3 dirHeadToPosition	= ( pointOnHeadPlane - this.m_HeadTransform.position );

			// Direction from entity to point
			Vector3 dirEntityToPosition	= ( pointOnEntityPlane - this.transform.position );

			// Angle between head and projected point
			float lookDeltaAngle = Vector3.Angle(this.m_HeadTransform.forward, dirHeadToPosition );

			// Current head allignment state
			bool isCurrentlyAlligned = lookDeltaAngle < 4f;
		
			// Head allignment comparison and event
			{
				bool wasPreviousAlligned = this.m_IsAllignedHeadToPoint;
				if ( wasPreviousAlligned == false && isCurrentlyAlligned == true )
				{
					this.OnLookRotationReached(this.m_HeadTransform.forward );
				}
			}

			// Flags assignment
			this.m_IsAllignedHeadToPoint			= isCurrentlyAlligned;
			this.m_IsDisallignedHeadWithPoint	= lookDeltaAngle > 90f;
			
			// Rotation Speed
			float rotationSpeed = this.m_HeadRotationSpeed * ( (this.m_TargetInfo.HasTarget ) ? 3.0f : 1.0f ) * Time.deltaTime;

			// Execute Rotation
			if (this.m_LookData.LookTargetMode == ELookTargetMode.WITH_BODY )
			{
				this.m_RotationToAllignTo.SetLookRotation( dirEntityToPosition, this.m_BodyTransform.up );
				this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, this.m_RotationToAllignTo, rotationSpeed );
			}
			// Head only
			else
			{
				this.m_RotationToAllignTo.SetLookRotation( dirHeadToPosition, this.m_BodyTransform.up );
				this.m_HeadTransform.rotation = Quaternion.RotateTowards(this.m_HeadTransform.rotation, this.m_RotationToAllignTo, rotationSpeed );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual	void	FireLongRange()
	{

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