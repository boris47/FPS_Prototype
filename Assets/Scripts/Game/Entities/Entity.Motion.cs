using UnityEngine;


public partial interface IEntity
{
	EMotionType		MotionType							{ get; }
}


public abstract partial class Entity : IEntity
{
	// DELEGATE
	public	delegate	void		OnMotionStateChangedEvent(EMotionType prevState, MotionStrategyBase prevMotion,  EMotionType newState, MotionStrategyBase currentMotion);

				EMotionType						IEntity.MotionType			=> m_CurrentMotionType;

	[Header("Entity: Motion States")]

	// This variable control which physic to use on entity
	protected	EMotionType						m_CurrentMotionType			= EMotionType.NONE;
	public		EMotionType						CurrentMotionType			=> m_CurrentMotionType;

	protected	MotionStrategyBase				m_MotionStrategy			= null;


	public		bool							IsGrounded					= false;
	public		float							GroundSpeedModifier			= 1.0f;

	// STATES
	public		bool	IsMoving	{ get => m_MotionStrategy.States.IsMoving;   }
	public		bool	IsIdle		{ get => !m_MotionStrategy.States.IsMoving;  }
	public		bool	IsLeaning	{ get => m_MotionStrategy.States.IsLeaning;  }
	public		bool	IsWalking	{ get => m_MotionStrategy.States.IsWalking;  }
	public		bool	IsRunning	{ get => m_MotionStrategy.States.IsRunning;  }
	public		bool	IsJumping	{ get => m_MotionStrategy.States.IsJumping;  }
	public		bool	IsHanging	{ get => m_MotionStrategy.States.IsHanging;  }
	public		bool	IsFalling	{ get => m_MotionStrategy.States.IsFalling;  }
	public		bool	IsCrouched	{ get => m_MotionStrategy.States.IsCrouched; }

	public void SetMotionType(int val)
	{
		SetMotionType((EMotionType)val);
	}

	// Set the motion type
	protected		void	SetMotionType(EMotionType newMotionType)
	{
		if (m_CurrentMotionType != newMotionType)
		{
			m_MotionStrategy?.Disable();
			Destroy(m_MotionStrategy);

			switch (newMotionType)
			{
				case EMotionType.NONE:		m_MotionStrategy = gameObject.AddComponent<MotionStrategy_Empty>();		break;
				case EMotionType.WALK:		m_MotionStrategy = gameObject.AddComponent<MotionStrategy_Walk>();		break;
				case EMotionType.PLATFORM:	m_MotionStrategy = gameObject.AddComponent<MotionStrategy_Platform>();	break;
				case EMotionType.FLY:		m_MotionStrategy = gameObject.AddComponent<MotionStrategy_Fly>();		break;
				case EMotionType.SWIM:		m_MotionStrategy = gameObject.AddComponent<MotionStrategy_Swim>();		break;
			}

			m_CurrentMotionType = newMotionType;

			if (newMotionType!= EMotionType.NONE)
			{
				m_MotionStrategy.Setup(this, m_HeadTransform, m_BodyTransform);

				m_MotionStrategy.Enable();
			}
		}
	}

}