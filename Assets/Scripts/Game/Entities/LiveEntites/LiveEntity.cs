[System.Serializable]
public abstract partial class LiveEntity : Entity
{
//	[Header("Live Entity Properties")]

//	[SerializeField]
//	protected	float				m_OxygenBaseLevel			= 100f;

//	[SerializeField]
//	protected	float				m_OxygenCurrentLevel		= 100f;

//	[SerializeField]
//	protected	float				m_Stamina					= 0f;

	
//	public		float				OxygenBaseLevel				=> m_OxygenBaseLevel;
//	public		float				OxygenCurrentLevel			=> m_OxygenCurrentLevel;
//	public		float				Stamina
//	{
//		get => m_Stamina;
//		set => m_Stamina = Mathf.Clamp01(value);
//	}

	/*
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
	*/
	
	/// <summary> In current frame the player has completed a heavy fall </summary>
//	[System.NonSerialized]
//	protected	bool				m_HeavyFall					= false;
	/// <summary> In current frame the player has reached the ground </summary>
//	[System.NonSerialized]
//	protected	bool				m_Landed					= false;

	//////////////////////////////////////////////////////////////////////////
/*	public	virtual void	AddOxygenAmmount( float Ammount )
	{
		m_OxygenCurrentLevel = Mathf.Min(m_OxygenCurrentLevel + Ammount, m_OxygenBaseLevel);
	}
*/

/*	//////////////////////////////////////////////////////////////////////////
	protected override void OnFrame(float deltaTime)
	{
		base.OnFrame(deltaTime);

	//	m_OxygenCurrentLevel -= deltaTime;
	}
*/
	/*
	//////////////////////////////////////////////////////////////////////////
	public	override	void		EnterSimulationState()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	public	override	void		BeforeSimulationStage( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
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
	public	override	bool		SimulateMovement( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget = 1 )
	{
		return false;
	}


	//////////////////////////////////////////////////////////////////////////
	public	override	void		AfterSimulationStage( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		NavStop();

		StopLooking();
	}


	//////////////////////////////////////////////////////////////////////////
	public	override	void		ExitSimulationState()
	{
		
	}
	*/
}
