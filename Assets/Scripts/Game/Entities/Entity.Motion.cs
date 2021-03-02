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


	// Set the motion type
	protected		void	SetMotionType(EMotionType newMotionType)
	{
		if (m_CurrentMotionType != newMotionType)
		{
			MotionStrategyBase newMotionStrategy = null;

			switch (newMotionType)
			{
				case EMotionType.NONE:		newMotionStrategy = gameObject.AddComponent<MotionStrategy_Empty>();	break;
				case EMotionType.WALK:		newMotionStrategy = gameObject.AddComponent<MotionStrategy_Walk>();		break;
				case EMotionType.PLATFORM:	newMotionStrategy = gameObject.AddComponent<MotionStrategy_Platform>();	break;
				case EMotionType.FLY:		newMotionStrategy = gameObject.AddComponent<MotionStrategy_Fly>();		break;
				case EMotionType.SWIM:		newMotionStrategy = gameObject.AddComponent<MotionStrategy_Swim>();		break;
			}

			UnityEngine.Assertions.Assert.IsNotNull(newMotionStrategy);

			Destroy(m_MotionStrategy);

			m_MotionStrategy = newMotionStrategy;

			m_CurrentMotionType = newMotionType;

			m_MotionStrategy.Setup(this, m_HeadTransform, m_BodyTransform);
		}
	}

}