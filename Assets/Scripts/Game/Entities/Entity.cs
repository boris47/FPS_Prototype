using UnityEngine;

public enum EEntityType : short
{
	NONE,
	ACTOR,
	HUMAN,
	ROBOT,
	ANIMAL
};

[RequireComponent(typeof(Rigidbody))]
public abstract partial class Entity : MonoBehaviour//, IIdentificable<uint>
{
	private	static uint									CurrentID					= 0;

	// INTERNALS
	[Header("Entity: Base")]
	[SerializeField]
	protected	float									m_Health					= 1.0f;
	[SerializeField, ReadOnly]
	protected	bool									m_HasShield					= false;
	[SerializeField, ReadOnly]
	protected	Shield									m_Shield					= null;
	[SerializeField]
	protected	Rigidbody								m_RigidBody					= null;
	[SerializeField]
	protected	Collider								m_PhysicCollider			= null;
	[SerializeField]
	protected	Collider								m_TriggerCollider			= null;
	[SerializeField]
	protected	Transform								m_EffectsPivot				= null;
	[SerializeField]
	protected	Transform								m_HeadTransform				= null;
	[SerializeField]
	protected	Transform								m_BodyTransform				= null;
	[SerializeField, ReadOnly]
	protected	Transform								m_Targettable				= null;
	[SerializeField]
	protected	EntityGroup								m_Group						= null;


	protected	abstract EEntityType					m_EntityType				{ get; }
	protected	abstract EntityComponentContainer[]		m_RequiredComponents		{ get; }
	protected 	uint									m_Id						= 0;
	protected	Database.Section						m_SectionRef				= null;
	protected 	string									m_SectionName				= "None";

	/// <summary> Physic collider, only manage entity in space </summary>
	public		Collider								PhysicCollider				=> m_PhysicCollider;
	/// <summary> Trigger collider, used for interactions with incoming objects or trigger areas
	public		Collider								TriggerCollider				=> m_TriggerCollider;
	/// <summary> The entity rigidbody </summary>
	public		Rigidbody								EntityRigidBody				=> m_RigidBody;

	public		uint									Id							=> m_Id;
	public		float									Health						=> m_Health;
	public		Shield									EntityShield				=> m_Shield;

	public		bool									IsAlive						=> m_Health > 0.0f;
	public		string									SectionName					=> m_SectionName;
	public		Transform								EffectsPivot				=> m_EffectsPivot;
	public		EEntityType								EntityType					=> m_EntityType;
	public		Transform								Head						=> m_HeadTransform;
	public		Transform								Body						=> m_BodyTransform;
	public		Transform								Targettable					=> m_Targettable;
	public		EntityGroup								EntityGroup					=> m_Group;

	// INTERFACE IIdentificable START
	//			uint									IIdentificable<uint>.ID		=> m_Id;
	// INTERFACE IIdentificable END
	

	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void	Awake()
	{
		m_Id			= CurrentID++;
		m_SectionName	= GetType().FullName;

		// config file
		if (!GlobalManager.Configs.TryGetSection(m_SectionName, out m_SectionRef))
		{
			print($"Cannot find cfg section \"{m_SectionName}\" for entity {name}");
			Destroy(gameObject);
			return;
		}
		
		// TRANSFORMS
		{
			m_HeadTransform		= m_HeadTransform ?? transform.Find("Head");
			m_BodyTransform		= m_BodyTransform ?? transform.Find("Body");
			m_EffectsPivot		= m_EffectsPivot ?? transform.Find("EffectsPivot");

			m_Targettable		= m_BodyTransform;

			UnityEngine.Assertions.Assert.IsNotNull(m_HeadTransform, $"Entity {name} has not head");
			UnityEngine.Assertions.Assert.IsNotNull(m_BodyTransform, $"Entity {name} has not body");
		}

		// ESSENTIALS CHECK (Assigned in prefab)
		{
			UnityEngine.Assertions.Assert.IsTrue(m_PhysicCollider.IsNotNull() && !m_PhysicCollider.isTrigger, "Invalid Physic Collider");
			UnityEngine.Assertions.Assert.IsTrue(m_TriggerCollider.IsNotNull() && m_TriggerCollider.isTrigger, "Invalid Trigger Collider");
			UnityEngine.Assertions.Assert.IsNotNull(m_RigidBody, "Invalid RigidBody");
		}

		m_Health = m_SectionRef.AsFloat("Health", 100.0f);
		m_RigidBody.mass = m_SectionRef.AsFloat("phMass", 80.0f);

		// SHIELD
		if (m_HasShield = Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL_AND_CHILDREN, out m_Shield))
		{
			m_Shield.OnHit += OnShieldHit;
		}

		// CUTSCENE MANAGER
		m_CutsceneManager = gameObject.GetComponentInChildren<CutScene.CutsceneEntityManager>();

		// Entity Components
		SetupComponents();
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Return if this entity can trigger with Trigger Areas </summary>
	public		virtual		bool	CanTrigger()
	{
		return true;
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Set the collision state between physic and trigger(if required) collider and another collider </summary>
	public void SetLocalCollisionStateWith(in Collider collider, in bool bAlsoTriggerCollider = true)
	{
		if (bAlsoTriggerCollider)
		{
			Physics.IgnoreCollision(collider, m_TriggerCollider, ignore: true);
		}
		Physics.IgnoreCollision(collider, m_PhysicCollider, ignore: true);
	//	Physics.IgnoreCollision( collider, m_PlayerNearAreaTrigger, ignore: true );
	//	Physics.IgnoreCollision( collider, m_PlayerFarAreaTrigger, ignore: true );
	//	Physics.IgnoreCollision( collider, m_Foots.Collider, ignore: true );
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Set the collision state of each collider on this entity end its children with another collider </summary>
	public					void	SetCollisionStateWith(Collider coll, bool state)
	{
		foreach(Collider collider in GetComponentsInChildren<Collider>(includeInactive: true))
		{
			Physics.IgnoreCollision(collider, coll, ignore: !state);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public void SetGroup(EntityGroup group)
	{
		m_Group = group;
	}
}