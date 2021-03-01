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


public abstract partial class Entity : IEntity
{
	// DELEGATE
	public	delegate	void		OnMotionStateChangedEvent( EMotionType prevState, EMotionType newState );

	// STORE VARIABLE
	protected	event	OnMotionStateChangedEvent				m_OnMotionStateChangedEvent = delegate { };

	EMotionType		IEntity.MotionType							=> m_CurrentMotionType;
	
	/// <summary> Events called this entity motion state changed </summary>
	event OnMotionStateChangedEvent IEntity.OnMotionStateChangedEvent
	{
		add		{ if ( value.IsNotNull() )	m_OnMotionStateChangedEvent += value; }
		remove	{ if ( value.IsNotNull() )	m_OnMotionStateChangedEvent -= value; }
	}

	[System.Serializable]
	protected class EntityStates
	{
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

		public void Assign(EntityStates other)
		{
			IsCrouched		= other.IsCrouched	;

			IsMoving		= other.IsMoving	;
			IsWalking		= other.IsWalking	;
			IsRunning		= other.IsRunning	;
							  
			IsLeaning		= other.IsLeaning	;
				
			IsJumping		= other.IsJumping	;
			IsHanging		= other.IsHanging	;
			IsFalling		= other.IsFalling;
		}
	};
	[Header("Entity: Motion States")]

	[SerializeField]
	protected   EntityStates			m_States			= new EntityStates();
	[SerializeField]
	//[System.NonSerialized]
	protected	EntityStates			m_PreviousStates	= new EntityStates();


	// This variable control which physic to use on entity
	protected	EMotionType		m_CurrentMotionType		= EMotionType.NONE;
	public		EMotionType		CurrentMotionType		=> m_CurrentMotionType;

	protected	EMotionType		m_PreviousMotionType	= EMotionType.NONE;
	public		EMotionType		PreviousMotionType		=> m_PreviousMotionType;


	// STATES
	public		bool	IsMoving	{ get => m_States.IsMoving;   }
	public		bool	IsIdle		{ get => !m_States.IsMoving;  }
	public		bool	IsLeaning	{ get => m_States.IsLeaning;  }
	public		bool	IsWalking	{ get => m_States.IsWalking;  }
	public		bool	IsRunning	{ get => m_States.IsRunning;  }
	public		bool	IsJumping	{ get => m_States.IsJumping;  }
	public		bool	IsHanging	{ get => m_States.IsHanging;  }
	public		bool	IsFalling	{ get => m_States.IsFalling;  }
	public		bool	IsCrouched	{ get => m_States.IsCrouched; }


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