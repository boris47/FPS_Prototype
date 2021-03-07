using Database;
using UnityEngine;

[System.Serializable]
public enum EMotionType : byte
{
	NONE		= 0,
	GROUNDED	= 1,
	CLIMB		= 2,
//	PLATFORM	= 3,
	FLY			= 4,
	SWIM		= 5
};

public interface IEntityComponent_Motion
{
	bool							CanMove						{ get; }
	bool							IsMoving					{ get; }
	bool							IsIdle						{ get; }
	MotionStrategyBase				MotionStrategy				{ get; }

	/// <summary> Set the motion type </summary>
	void							SetMotionType				(EMotionType newMotionType);
}

public abstract class Motion_Base : EntityComponent, IEntityComponent_Motion
{
	[SerializeField]
	protected	EMotionType						m_CurrentMotionType			= EMotionType.NONE;
	[SerializeField]
	protected	MotionStrategyBase				m_MotionStrategy			= null;

	public		bool							CanMove						=> m_MotionStrategy.CanMove;
	public		EMotionType						CurrentMotionType			=> m_CurrentMotionType;
	public		bool							IsMoving					=> m_MotionStrategy.States.IsMoving;
	public		bool							IsIdle						=> !m_MotionStrategy.States.IsMoving;
	public		MotionStrategyBase				MotionStrategy				=> m_MotionStrategy;

	//////////////////////////////////////////////////////////////////////////
	public override void OnLoad(StreamUnit streamUnit)
	{
		base.OnLoad(streamUnit);

		EMotionType motionType = streamUnit.GetAsEnum<EMotionType>("MotionType");
		SetMotionType(motionType);
	}


	//////////////////////////////////////////////////////////////////////////
	public void SetMotionType(EMotionType newMotionType)
	{
		if (m_CurrentMotionType != newMotionType)
		{
			m_MotionStrategy?.Disable();
			Destroy(m_MotionStrategy);

			switch (newMotionType)
			{
				case EMotionType.NONE:		m_MotionStrategy = gameObject.AddComponent<MotionStrategy_Empty>();		break;
				case EMotionType.GROUNDED:	m_MotionStrategy = gameObject.AddComponent<MotionStrategy_Grounded>();	break;
				case EMotionType.CLIMB:		m_MotionStrategy = gameObject.AddComponent<MotionStrategy_Climb>();		break;
			//	case EMotionType.PLATFORM:	m_MotionStrategy = gameObject.AddComponent<MotionStrategy_Platform>();	break;
				case EMotionType.FLY:		m_MotionStrategy = gameObject.AddComponent<MotionStrategy_Fly>();		break;
				case EMotionType.SWIM:		m_MotionStrategy = gameObject.AddComponent<MotionStrategy_Swim>();		break;
			}

			m_CurrentMotionType = newMotionType;

			// none motion type has no need to be setup and enabled
			if (newMotionType!= EMotionType.NONE)
			{
				m_MotionStrategy.Setup(m_Entity, m_EntitySection);

				m_MotionStrategy.Enable();
			}
		}
	}
}

public class EntityComponentContainer_Motion<T> : EntityComponentContainer where T : Motion_Base, new()
{
	public override System.Type type { get; } = typeof(T);
}