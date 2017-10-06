using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class LiveEntity : Entity {
	

	protected	float	m_LastLandTime					= Defaults.FLOAT_ZERO;


	protected	Quaternion	m_FaceDirection				= Quaternion.identity;
	public		Quaternion FaceDirection {
		get { return m_FaceDirection; }
		set { m_FaceDirection = value; }
	}


	[SerializeField]
	protected	float	m_Health						= Defaults.FLOAT_ZERO;

	public		float	Health {
		get { return m_Health; }
		set { m_Health = value; }
	}


	// Stamina always reach 1.0f
	[SerializeField]
	protected	float	m_Stamina						= Defaults.FLOAT_ZERO;
	public		float	Stamina {
		get { return m_Stamina; }
		set { m_Stamina = Mathf.Clamp01( value ); }
	}

	protected	float	m_ViewRange						= Defaults.FLOAT_ZERO;
	public		float	ViewRange {
		get { return m_ViewRange; }
		set { m_ViewRange = Mathf.Clamp( value, 0.0f, 9999.0f ); }
	}

	protected	bool	m_Grounded						= true;
	public		bool	Grounded {
		get { return m_Grounded; }
		set { m_Grounded = value; }
	}


	// Movements
	[SerializeField]
	protected	float	m_WalkSpeed						= Defaults.FLOAT_ZERO;
	[SerializeField]
	protected	float	m_RunSpeed						= Defaults.FLOAT_ZERO;
	[SerializeField]
	protected	float	m_CrouchSpeed					= Defaults.FLOAT_ZERO;
	[SerializeField]
	protected	float	m_ClimbSpeed					= Defaults.FLOAT_ZERO;
		
	protected	float	m_WalkJumpCoef					= Defaults.FLOAT_ZERO;
	protected	float	m_RunJumpCoef					= Defaults.FLOAT_ZERO;
	protected	float	m_CrouchJumpCoef				= Defaults.FLOAT_ZERO;
		
	protected	float	m_WalkStamina					= Defaults.FLOAT_ZERO;
	protected	float	m_RunStamina					= Defaults.FLOAT_ZERO;
	protected	float	m_CrouchStamina					= Defaults.FLOAT_ZERO;

	protected	float	m_JumpForce						= Defaults.FLOAT_ZERO;
	protected	float	m_JumpStamina					= Defaults.FLOAT_ZERO;
		
	protected	float	m_StaminaRestore				= Defaults.FLOAT_ZERO;
	protected	float	m_StaminaRunMin					= Defaults.FLOAT_ZERO;
	protected	float	m_StaminaJumpMin				= Defaults.FLOAT_ZERO;
	
	


	// Var used for smooth movements of entity
	protected	float	m_MoveSmooth					= Defaults.FLOAT_ZERO;
	protected	float	m_StrafeSmooth					= Defaults.FLOAT_ZERO;
	protected	float	m_VerticalSmooth				= Defaults.FLOAT_ZERO;

	protected	bool	m_IsUnderSomething				= false;
	protected	bool	m_Tiredness						= false;
	
	protected	bool	m_HeavyFall						= false;
	protected	bool	m_Landed						= false;
	

}
