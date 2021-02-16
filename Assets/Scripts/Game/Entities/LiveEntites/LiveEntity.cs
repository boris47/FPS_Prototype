using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract partial class LiveEntity : Entity {

	protected	IFoots				m_Foots						= null;
	public		IFoots				Foots
	{
		get { return m_Foots; }
	}
	[Header("Live Entity Properties")]

	[SerializeField]
	protected	float				m_OxygenBaseLevel			= 100f;
	public		float				OxygenBaseLevel
	{
		get { return m_OxygenBaseLevel; }
	}


	[SerializeField]
	protected	float				m_OxygenCurrentLevel		= 100f;
	public		float				OxygenCurrentLevel
	{
		get { return m_OxygenCurrentLevel; }
	}





	[SerializeField]
	protected	float				m_UseDistance				= 1f;


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
	[System.NonSerialized]
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
//	[System.NonSerialized]
	protected	float				m_ForwardSmooth				= 0f;
//	[System.NonSerialized]
	protected	float				m_RightSmooth				= 0f;
//	[System.NonSerialized]
	protected	float				m_UpSmooth					= 0f;
//	[System.NonSerialized]
	protected	bool				m_IsUnderSomething			= false;
	
	/// <summary> If currently is tired ( stanco ) </summary>
	[System.NonSerialized]
	protected	bool				m_Tiredness					= false;
	/// <summary> In current frame the player has completed a heavy fall </summary>
	[System.NonSerialized]
	protected	bool				m_HeavyFall					= false;
	/// <summary> In current frame the player has reached the ground </summary>
	[System.NonSerialized]
	protected	bool				m_Landed					= false;

	//////////////////////////////////////////////////////////////////////////
	/// <summary> The range is [0, 100] by default </summary>
	public	virtual void	AddOxygenAmmount( float Ammount )
	{
		m_OxygenCurrentLevel = Mathf.Min(m_OxygenCurrentLevel + Ammount, m_OxygenBaseLevel );
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnFrame( float DeltaTime )
	{
		//		base.OnFrame( DeltaTime );

		m_OxygenCurrentLevel -= DeltaTime;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		EnterSimulationState()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		BeforeSimulationStage( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		// Movement
		RequestMovement( destination );

		// Look At
		if ( target )
		{
			SetTransformToLookAt( target, ELookTargetMode.HEAD_ONLY );
		}
		else
		{
			StopLooking();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	bool		SimulateMovement( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget = 1 )
	{
		return false;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		AfterSimulationStage( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		NavStop();

		StopLooking();
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		ExitSimulationState()
	{
		
	}

}
