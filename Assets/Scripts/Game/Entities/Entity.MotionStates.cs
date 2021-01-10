using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum EMotionType {
	None		= 1 << 0,
	Walking		= 1 << 1,
	Flying		= 1 << 2,
	Swimming	= 1 << 3,
	P1ToP2		= 1 << 4,
	NONE		= 1 << 5
};



public partial interface IEntity {
	EMotionType		MotionType							{ get; }
	
	/// <summary> Events called this entity motion state changed </summary>
	event			Entity.OnMotionStateChangedEvent	OnMotionStateChangedEvent;
}


public abstract partial class Entity : IEntity {

	// DELEGATE
	public	delegate	void		OnMotionStateChangedEvent( EMotionType prevState, EMotionType newState );

	// STORE VARIABLE
	protected	event	OnMotionStateChangedEvent					m_OnMotionStateChangedEvent			= delegate { };


	EMotionType		IEntity.MotionType							{ get { return m_CurrentMotionType; } }
	
	/// <summary> Events called this entity motion state changed </summary>
	event OnMotionStateChangedEvent IEntity.OnMotionStateChangedEvent
	{
		add		{ if ( value != null )	m_OnMotionStateChangedEvent += value; }
		remove	{ if ( value != null )	m_OnMotionStateChangedEvent -= value; }
	}

	[System.Serializable]
	protected class _States {

		public	bool	IsCrouched		= false;

		public	bool	IsMoving		= false;
		public	bool	IsWalking		= false;
		public	bool	IsRunning		= false;

		public	bool	IsLeaning		= false;

		public	bool	IsJumping		= false;
		public	bool	IsHanging		= false;
		public	bool	IsFalling		= false;

		public void Reset()
		{
			IsMoving = IsWalking = IsRunning = IsJumping = IsHanging = IsFalling = false;
		}
	};

	protected   _States			m_States			= new _States();
	[System.NonSerialized]
	protected	_States			m_PreviousStates	= new _States();


	// This variable control which physic to use on entity
	protected	EMotionType		m_CurrentMotionType		= EMotionType.NONE;
	public		EMotionType		CurrentMotionType
	{
		get { return m_CurrentMotionType; }
	}

	protected	EMotionType		m_PreviousMotionType	= EMotionType.NONE;
	public		EMotionType		PreviousMotionType
	{
		get { return m_PreviousMotionType; }
	}




	// STATES
	public		bool	IsMoving {		get { return m_States.IsMoving; }		set { m_States.IsMoving = value; }	}
	public		bool	IsIdle {		get { return  !m_States.IsMoving; }		set { m_States.IsMoving = !value; }	}
	public		bool	IsLeaning {		get { return m_States.IsLeaning; }		set { m_States.IsLeaning = value; }	}
	public		bool	IsWalking {		get { return m_States.IsWalking; }		set { m_States.IsWalking = value; }	}
	public		bool	IsRunning {		get { return m_States.IsRunning; }		set { m_States.IsRunning = value; }	}
	public		bool	IsJumping {		get { return m_States.IsJumping; }		set { m_States.IsJumping = value; }	}
	public		bool	IsHanging {		get { return m_States.IsHanging; }		set { m_States.IsHanging = value; }	}
	public		bool	IsFalling {		get { return m_States.IsFalling; }		set { m_States.IsFalling = value; }	}
	public		bool	IsCrouched {	get { return m_States.IsCrouched; }	set { m_States.IsCrouched = value; }}
	public		void	ResetStates()	{ m_States.Reset(); }





	// Set the motion type
	protected		void	SetMotionType( EMotionType NewState )
	{
		if (m_CurrentMotionType == NewState )
			return;

		m_OnMotionStateChangedEvent(m_CurrentMotionType, NewState );

		/*		System.Action<eMotionType, bool, bool, bool> SetMotionTypeInternal = delegate( eMotionType type, bool HoldCrouch, bool HoldJump, bool HoldRun )
				{
					InputManager.HoldCrouch	= HoldCrouch;
					InputManager.HoldJump	= HoldJump;
					InputManager.HoldRun	= HoldRun;
				};
		*/
		m_PreviousMotionType = m_CurrentMotionType;
		m_CurrentMotionType = NewState;
		//		switch( NewState )
		//		{	//																HoldCrouch, HoldJump, HoldRun
		//			case eMotionType.Walking:	{ SetMotionTypeInternal( NewState, false,	false,	true ); break; }
		//			case eMotionType.Flying:	{ SetMotionTypeInternal( NewState, true,	true,	true ); break; }
		//			case eMotionType.Swimming:	{ SetMotionTypeInternal( NewState, true,	true,	true ); break; }
		//			case eMotionType.P1ToP2:	{ SetMotionTypeInternal( NewState, false,	false,	true ); break; }
		//		}

		// Reset states
		m_States.Reset();
	}

}