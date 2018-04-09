
using CFG_Reader;
using UnityEngine;
using AI_Behaviours;


public interface IEntity {
	bool				IsActive						{	get; set;	}
	uint				ID								{	get;		}
	float				Health							{	get; set;	}
//	float				ViewRange						{	get; set;	}
	string				Section							{	get;		}
	Rigidbody			RigidBody						{	get;		}
	CapsuleCollider		PhysicCollider					{	get;		}
	Transform			Transform						{	get;		}
	IBrain				Brain							{	get;		}


	void				OnFrame( float deltaTime );
	void				OnThink();

	void				OnTargetAquired( TargetInfo_t targetInfo );
	void				OnTargetChanged( TargetInfo_t targetInfo );
	void				OnTargetLost( TargetInfo_t targetInfo );

	void				OnHit( ref IBullet bullet );
	void				OnKill();
}



[RequireComponent( typeof( Rigidbody ), typeof( CapsuleCollider ) )]
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
	private	static uint			CurrentID						= 0;
	public	static uint			NewID()							{ return CurrentID++; }


	[SerializeField]
	protected	float			m_Health						= 1f;

//	[SerializeField]
//	protected	float			m_ViewRange						= 10f;

	// INTERFACE START
				bool			IEntity.IsActive				{	get { return m_IsActive;		}	set { m_IsActive = value;		}	}
				uint			IEntity.ID						{	get { return m_ID;				}	}
				float			IEntity.Health					{	get { return m_Health;			}	set { m_Health		= value;	}	}
//				float			IEntity.ViewRange				{	get { return m_ViewRange;		}	set { m_ViewRange	= value;	}	}
				string			IEntity.Section					{	get { return m_SectionName;		}	}
				Rigidbody		IEntity.RigidBody				{	get { return m_RigidBody;		}	}
				CapsuleCollider	IEntity.PhysicCollider			{	get { return m_PhysicCollider;	}	}
				IBrain			IEntity.Brain					{	get { return m_Brain;		}	}
				Transform		IEntity.Transform				{	get { return transform;			}	}
	// INTERFACE END


	protected	bool			m_IsActive						= true;

	protected 	uint			m_ID							= 0;

	protected	Section			m_SectionRef					= null;

	protected 	string			m_SectionName					= "None";

	[System.NonSerialized]
	protected 	ENTITY_TYPE		m_EntityType					= ENTITY_TYPE.NONE;

	protected	Vector3			m_LastAttackPosition			= Vector3.zero;

	protected	Rigidbody		m_RigidBody						= null;

	protected	CapsuleCollider	m_PhysicCollider				= null;

	protected	IBrain			m_Brain							= null;

	protected	TargetInfo_t	m_TargetInfo					= default( TargetInfo_t );

	protected	bool 			m_IsOK							= false;




	//////////////////////////////////////////////////////////////////////////
	// Awake
	protected virtual	void				Awake()
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
	public virtual	void	OnFrame( float deltaTime )
	{

	}

}
