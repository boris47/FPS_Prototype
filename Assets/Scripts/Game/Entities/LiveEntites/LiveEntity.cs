using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract partial class LiveEntity : Entity {
	
	protected	IFoots			m_Foots						= null;
	public		IFoots			Foots
	{
		get { return m_Foots; }
	}

	// DRAG
	protected	GameObject		m_GrabPoint					= null;
	protected	GameObject		m_GrabbedObject				= null;
	public		GameObject		GrabbedObject
	{
		get { return m_GrabbedObject; }
	}
	protected	float			m_GrabbedObjectMass			= 0f;
	protected	bool			m_GrabbedObjectUseGravity	= false;

	[SerializeField]
	protected	float			m_UseDistance				= 1f;


	// Face Direction
	protected	Quaternion		m_FaceDirection				= Quaternion.identity;
	public		Quaternion		FaceDirection
	{
		get { return m_FaceDirection; }
		set { m_FaceDirection = value; }
	}



	// PLAYER PARAMENTERS
	[SerializeField]
	protected	float			m_Health					= 0f;

	public		float			Health
	{
		get { return m_Health; }
		set { m_Health = value; }
	}


	// Stamina always reach 1.0f
	[SerializeField]
	protected	float			m_Stamina					= 0f;
	public		float			Stamina
	{
		get { return m_Stamina; }
		set { m_Stamina = Mathf.Clamp01( value ); }
	}

	protected	float			m_ViewRange					= 0f;
	public		float			ViewRange
	{
		get { return m_ViewRange; }
		set { m_ViewRange = Mathf.Clamp( value, 0.0f, 9999.0f ); }
	}

	protected	bool			m_Grounded					= false;
	public		bool			Grounded
	{
		get { return m_Grounded; }
		set { m_Grounded = value; }
	}

	protected	float			m_GroundSpeedModifier		= 1f;
	public		float			GroundSpeedModifier
	{
		get { return m_GroundSpeedModifier; }
		set { m_GroundSpeedModifier = value; }
	}



	// Movements
	[SerializeField]
	protected	float	m_WalkSpeed						= 0f;
	[SerializeField]
	protected	float	m_RunSpeed						= 0f;
	[SerializeField]
	protected	float	m_CrouchSpeed					= 0f;
	[SerializeField]
	protected	float	m_ClimbSpeed					= 0f;
		
	protected	float	m_WalkJumpCoef					= 0f;
	protected	float	m_RunJumpCoef					= 0f;
	protected	float	m_CrouchJumpCoef				= 0f;
		
	protected	float	m_WalkStamina					= 0f;
	protected	float	m_RunStamina					= 0f;
	protected	float	m_CrouchStamina					= 0f;

	protected	float	m_JumpForce						= 0f;
	protected	float	m_JumpStamina					= 0f;
		
	protected	float	m_StaminaRestore				= 0f;
	protected	float	m_StaminaRunMin					= 0f;
	protected	float	m_StaminaJumpMin				= 0f;
	
	


	// Var used for smooth movements of entity
	protected	float	m_MoveSmooth					= 0f;
	protected	float	m_StrafeSmooth					= 0f;
	protected	float	m_VerticalSmooth				= 0f;

	protected	bool	m_IsUnderSomething				= false;
	protected	bool	m_Tiredness						= false;
	
	protected	bool	m_HeavyFall						= false;
	protected	bool	m_Landed						= false;

	protected	float	m_LastLandTime					= 0f;

}
