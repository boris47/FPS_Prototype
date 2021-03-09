
using UnityEngine;


[System.Serializable]
public partial class Player : Human
{
//	[Header("Player Properties")]

	public	static	Player							Instance								{ get; private set; } = null;

//	private		Collider							m_PlayerNearAreaTrigger					= null;
//	public		Collider							PlayerNearAreaTrigger					=> m_PlayerNearAreaTrigger;

//	private		Collider							m_PlayerFarAreaTrigger					= null;
//	public		Collider							PlayerFarAreaTrigger					=> m_PlayerFarAreaTrigger;

	protected	override ERotationsMode				m_LookTargetMode						=> ERotationsMode.NONE;
	protected	override EEntityType				m_EntityType							=> EEntityType.ACTOR;
	protected	override EntityComponentContainer[] m_RequiredComponents					=> new EntityComponentContainer[]
	{
		new EntityComponentContainer_Motion<Motion_Common>(),
		new EntityComponentContainer_Interactions<Interactions_Common>(),
		new EntityComponentContainer_Inventory<Inventory_Player>(),
	};

	public		new		IMotion_Common				Motion									=> base.Motion as IMotion_Common;
	public		new		IInventory_Player			Inventory								=> base.Inventory as IInventory_Player;


	//////////////////////////////////////////////////////////////////////////
	protected	override	void	Awake()
	{
		// Singleton
		if (Instance.IsNotNull())
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(this);

		base.Awake();

		UnityEngine.Assertions.Assert.IsNotNull(m_CutsceneManager);

	//	transform.TrySearchComponentByChildName("PNAT", out m_PlayerNearAreaTrigger);
	//	transform.TrySearchComponentByChildName("PFAT", out m_PlayerFarAreaTrigger);

		// Player Data
		{
			// Walking
	//		m_SectionRef.AsMultiValue("Walk",		1, 2, 3, out m_WalkSpeed,	out m_WalkJumpCoef,		out m_WalkStamina);

			// Running
	//		m_SectionRef.AsMultiValue("Run",		1, 2, 3, out m_RunSpeed,	out m_RunJumpCoef,		out m_RunStamina);

			// Crouched
	//		m_SectionRef.AsMultiValue("Crouch",		1, 2, 3, out m_CrouchSpeed, out m_CrouchJumpCoef,	out m_CrouchStamina);

	///		m_FallDistanceThreshold		= m_SectionRef.AsFloat( "FallDistanceThreshold", m_FallDistanceThreshold);

			// Climbing
	//		m_ClimbSpeed				= m_SectionRef.AsFloat( "Climb", m_ClimbSpeed);

			// Jumping
			{
	///			m_SectionRef.AsMultiValue("Jump", 1, 2, out m_JumpForce, out m_JumpStamina);
			}

			// Stamina
			{
	///			m_StaminaRestore		= m_SectionRef.AsFloat("StaminaRestore", 0.0f);
	///			m_StaminaRunMin			= m_SectionRef.AsFloat("StaminaRunMin",  0.3f);
	///			m_StaminaJumpMin		= m_SectionRef.AsFloat("StaminaJumpMin", 0.4f);
			}
		}

		m_RigidBody.maxAngularVelocity = 0f;
		m_RigidBody.useGravity = false;
	//	m_Stamina			= 1.0f;
//		GroundSpeedModifier = 1.0f;
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	Start()
	{
		FPSEntityCamera.Instance.SetViewPoint(this);
	}

	//////////////////////////////////////////////////////////////////////////
	protected override void OnEnable()
	{
		base.OnEnable();

		Motion.SetMotionType(EMotionType.GROUNDED); // Default
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDisable()
	{
		Motion.SetMotionType(EMotionType.NONE); // also Unbind bindings

		base.OnDisable();
	}
}