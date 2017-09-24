using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ILiveEntity {

	long	GetState();

	void	Resetm_States();

	bool	IsMoving();
	bool	IsIdle();
	bool	IsLeaning();
	bool	IsWalking();
	bool	IsRunning();
	bool	IsJumping();
	bool	IsHanging();
	bool	IsFalling();
	bool	IsCrouched();

	void	SetMoving();
	void	SetIdle();
	void	SetLeaning(  bool b );
	void	SetWalking(  bool b );
	void	SetRunning(  bool b );
	void	SetJumping(  bool b );
	void	SetHanging(  bool b );
	void	SetFalling(  bool b );
	void	SetCrouched( bool b );

	bool	Motion_IsWalking();
	bool	Motion_IsSwimming();
	bool	Motion_IsFlying();
	bool	Motion_IsP1ToP2();

	void	SetMotionType( LIVE_ENTITY.MotionType i );
	void	SetPlainWalking();
	void	SetFlying();
	void	SetSwimming();
	void	SetClimbing();

}

public partial class LiveEntity : Entity, ILiveEntity {
	

	protected	float	m_LastLandTime					= Defaults.FLOAT_ZERO;

	[SerializeField]
	protected	float	m_Health						= Defaults.FLOAT_ZERO;

	public		float	Health {
		get { return m_Health; }
		set { m_Health = value; }
	}


	// Stamina always reach 1.0f
	[SerializeField]
	protected	float	m_Stamina						= Defaults.FLOAT_ZERO;
	public		float	Statmina {
		get { return m_Stamina; }
		set { m_Stamina = Mathf.Clamp01( value ); }
	}

	protected	float	m_ViewRange						= Defaults.FLOAT_ZERO;
	public		float	ViewRange {
		get { return m_ViewRange; }
		set { m_ViewRange = Mathf.Clamp( value, 0.0f, 9999.0f ); }
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
	
	protected	EntityFlags				m_States			= new EntityFlags();
	protected	EntityFlags				m_MotionFlag		= new EntityFlags();


	// Var used for smooth movements of entity
	protected	float	m_MoveSmooth					= Defaults.FLOAT_ZERO;
	protected	float	m_StrafeSmooth					= Defaults.FLOAT_ZERO;
	protected	float	m_VerticalSmooth				= Defaults.FLOAT_ZERO;

	protected	bool	m_IsUnderSomething				= false;
	protected	bool	m_Tiredness						= false;
	protected	bool	m_Grounded						= true;
	protected	bool	m_HeavyFall						= false;
	protected	bool	m_Landed						= false;



	public		long	GetState() 						{ return this.m_States.GetState(); }

	public		void	Resetm_States()					{ m_States.Reset(); }

	public		bool	IsMoving() 						{ return m_States.HasState(  ( byte )LIVE_ENTITY.States.Moving ); }
	public		bool	IsIdle()						{ return !m_States.HasState( ( byte )LIVE_ENTITY.States.Moving ); }
	public		bool	IsLeaning()						{ return m_States.HasState(  ( byte )LIVE_ENTITY.States.Leaning ); }
	public		bool	IsWalking()						{ return m_States.HasState(  ( byte )LIVE_ENTITY.States.Walking ); }
	public		bool	IsRunning()						{ return m_States.HasState(  ( byte )LIVE_ENTITY.States.Running ); }
	public		bool	IsJumping()						{ return m_States.HasState(  ( byte )LIVE_ENTITY.States.Jumping ); }
	public		bool	IsHanging()						{ return m_States.HasState(  ( byte )LIVE_ENTITY.States.Hanging ); }
	public		bool	IsFalling()						{ return m_States.HasState(  ( byte )LIVE_ENTITY.States.Falling ); }
	public		bool	IsCrouched()					{ return m_States.HasState(  ( byte )LIVE_ENTITY.States.Crouched ); }

	public		void	SetMoving()						{ m_States.SetState( ( byte )LIVE_ENTITY.States.Moving, true ); }
	public		void	SetIdle()						{ m_States.SetState( ( byte )LIVE_ENTITY.States.Moving, false ); }
	public		void	SetLeaning(  bool b )			{ m_States.SetState( ( byte )LIVE_ENTITY.States.Leaning, b ); }
	public		void	SetWalking(  bool b )			{ m_States.SetState( ( byte )LIVE_ENTITY.States.Walking, b ); }
	public		void	SetRunning(  bool b )			{ m_States.SetState( ( byte )LIVE_ENTITY.States.Running, b ); }
	public		void	SetJumping(  bool b )			{ m_States.SetState( ( byte )LIVE_ENTITY.States.Jumping, b ); }
	public		void	SetHanging(  bool b )			{ m_States.SetState( ( byte )LIVE_ENTITY.States.Hanging, b ); }
	public		void	SetFalling(  bool b )			{ m_States.SetState( ( byte )LIVE_ENTITY.States.Falling, b ); }
	public		void	SetCrouched( bool b )			{ m_States.SetState( ( byte )LIVE_ENTITY.States.Crouched, b ); }


	public		bool	Motion_IsWalking()				{ return m_MotionFlag.HasState( ( byte )LIVE_ENTITY.MotionType.Walking ); }
	public		bool	Motion_IsSwimming()				{ return m_MotionFlag.HasState( ( byte )LIVE_ENTITY.MotionType.Swimming ); }
	public		bool	Motion_IsFlying()				{ return m_MotionFlag.HasState( ( byte )LIVE_ENTITY.MotionType.Flying ); }
	public		bool	Motion_IsP1ToP2()				{ return m_MotionFlag.HasState( ( byte )LIVE_ENTITY.MotionType.P1ToP2 ); }








}
