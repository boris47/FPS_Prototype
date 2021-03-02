using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract partial class LiveEntity : Entity
{
	protected	IFoots				m_Foots						= null;
	public		IFoots				Foots
	{
		get => m_Foots;
	}
	[Header("Live Entity Properties")]

	[SerializeField]
	protected	float				m_OxygenBaseLevel			= 100f;
	public		float				OxygenBaseLevel
	{
		get => m_OxygenBaseLevel;
	}


	[SerializeField]
	protected	float				m_OxygenCurrentLevel		= 100f;
	public		float				OxygenCurrentLevel
	{
		get => m_OxygenCurrentLevel;
	}


	[SerializeField]
	protected	float				m_UseDistance				= 1f;

	// Stamina always reach 1.0f
	[SerializeField]
	protected	float				m_Stamina					= 0f;
	public		float				Stamina
	{
		get => m_Stamina;
		set => m_Stamina = Mathf.Clamp01( value );
	}


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
