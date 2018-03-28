using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract partial class LiveEntity : Entity {
	
	protected	IFoots				m_Foots						= null;
	public		IFoots				Foots
	{
		get { return m_Foots; }
	}

	[SerializeField]
	protected	float				m_UseDistance				= 1f;

	// GRABBING OBJECT
	[System.NonSerialized]
	protected	GameObject			m_GrabPoint					= null;
	[System.NonSerialized]
	protected	GameObject			m_GrabbedObject				= null;
	public		GameObject			GrabbedObject
	{
		get { return m_GrabbedObject; }
	}
	[System.NonSerialized]
	protected	float				m_GrabbedObjectMass			= 0f;
	[System.NonSerialized]
	protected	bool				m_GrabbedObjectUseGravity	= false;
	protected	bool				m_CanGrabObjects			= true;




	// LIVE ENTITY PARAMENTERS
	public		bool				IsGrounded					{ get; set; }
	public		float				GroundSpeedModifier			{ get; set; }


	// Stamina always reach 1.0f
	[SerializeField]
	protected	float				m_Stamina					= 0f;
	public		float				Stamina
	{
		get { return m_Stamina; }
		set { m_Stamina = Mathf.Clamp01( value ); }
	}


	// Movements
	[SerializeField]
	protected	float				m_WalkSpeed					= 0f;
	[SerializeField]
	protected	float				m_RunSpeed					= 0f;
	[SerializeField]
	protected	float				m_CrouchSpeed				= 0f;
	[SerializeField]
	protected	float				m_ClimbSpeed				= 0f;
	[System.NonSerialized]
	protected	float				m_WalkJumpCoef				= 0f;
	[System.NonSerialized]
	protected	float				m_RunJumpCoef				= 0f;
	[System.NonSerialized]
	protected	float				m_CrouchJumpCoef			= 0f;
	[System.NonSerialized]
	protected	float				m_WalkStamina				= 0f;
	[System.NonSerialized]
	protected	float				m_RunStamina				= 0f;
	[System.NonSerialized]
	protected	float				m_CrouchStamina				= 0f;
//	[System.NonSerialized]
	protected	float				m_FallDistanceThreshold		= 0f;
	[System.NonSerialized]
	protected	float				m_JumpForce					= 0f;
	[System.NonSerialized]
	protected	float				m_JumpStamina				= 0f;
	[System.NonSerialized]
	protected	float				m_StaminaRestore			= 0f;
	[System.NonSerialized]
	protected	float				m_StaminaRunMin				= 0f;
	[System.NonSerialized]
	protected	float				m_StaminaJumpMin			= 0f;
	
	


	// Var used for smooth movements of entity
	[System.NonSerialized]
	protected	float				m_MoveSmooth				= 0f;
	[System.NonSerialized]
	protected	float				m_StrafeSmooth				= 0f;
	[System.NonSerialized]
	protected	float				m_VerticalSmooth			= 0f;
	[System.NonSerialized]
	protected	bool				m_IsUnderSomething			= false;
	[System.NonSerialized]
	protected	bool				m_Tiredness					= false;
	[System.NonSerialized]
	protected	bool				m_HeavyFall					= false;
	[System.NonSerialized]
	protected	bool				m_Landed					= false;
	[System.NonSerialized]
	protected	float				m_LastLandTime				= 0f;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Update()
	{
		base.Update();
	}

}
