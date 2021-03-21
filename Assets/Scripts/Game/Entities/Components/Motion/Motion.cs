using UnityEngine;

[System.Serializable]
public enum EEntityMotionType : byte
{
	NONE		= 0,
	GROUNDED	= 1,
	CLIMB		= 2,
	SWIM		= 3,
//	PLATFORM	= 4,
	FLY			= 5,
	DRIVE		= 6
};

public interface IEntityComponent_Motion
{
	bool							CanMove						{ get; }
	bool							IsMoving					{ get; }
	bool							IsIdle						{ get; }
	MotionStrategyBase				MotionStrategy				{ get; }

	/// <summary> Set the motion type </summary>
	void							SetMotionType				(in EEntityMotionType newMotionType, params object[] args);
}

public abstract class Motion_Base : EntityComponent, IEntityComponent_Motion
{
	[SerializeField]
	protected	EEntityMotionType				m_CurrentMotionType			= EEntityMotionType.NONE;
	[SerializeField]
	protected	MotionStrategyBase				m_MotionStrategy			= null;

	public		bool							CanMove						=> m_MotionStrategy.CanMove;
	public		EEntityMotionType				CurrentMotionType			=> m_CurrentMotionType;
	public		bool							IsMoving					=> m_MotionStrategy.States.IsMoving;
	public		bool							IsIdle						=> !m_MotionStrategy.States.IsMoving;
	public		MotionStrategyBase				MotionStrategy				=> m_MotionStrategy;


	//////////////////////////////////////////////////////////////////////////
	public override void OnLoad(StreamUnit streamUnit)
	{
		base.OnLoad(streamUnit);

		EEntityMotionType motionType = streamUnit.GetAsEnum<EEntityMotionType>("MotionType");
		SetMotionType(motionType);
	}


	//////////////////////////////////////////////////////////////////////////
	public void SetMotionType(in EEntityMotionType newMotionType, params object[] args)
	{
		if (m_CurrentMotionType != newMotionType)
		{
			var states = m_MotionStrategy?.States ?? new MotionStrategyBase.EntityStates();
			m_MotionStrategy?.Disable();
			Destroy(m_MotionStrategy);

			switch (newMotionType)
			{
				case EEntityMotionType.NONE:		m_MotionStrategy = gameObject.AddComponent<MotionStrategy_Empty>();		break;
				case EEntityMotionType.GROUNDED:	m_MotionStrategy = gameObject.AddComponent<MotionStrategy_Grounded>();	break;
				case EEntityMotionType.CLIMB:		m_MotionStrategy = gameObject.AddComponent<MotionStrategy_Climb>();		break;
			//	case EMotionType.PLATFORM:			m_MotionStrategy = gameObject.AddComponent<MotionStrategy_Platform>();	break;
				case EEntityMotionType.FLY:			m_MotionStrategy = gameObject.AddComponent<MotionStrategy_Fly>();		break;
				case EEntityMotionType.SWIM:		m_MotionStrategy = gameObject.AddComponent<MotionStrategy_Swim>();		break;
			}

			m_CurrentMotionType = newMotionType;

			// none motion type has no need to be setup and enabled
			if (newMotionType!= EEntityMotionType.NONE)
			{
				m_MotionStrategy.Setup(m_Entity, m_EntitySection, states, args);

				m_MotionStrategy.Enable();
			}
		}
	}
}

public class EntityComponentContainer_Motion<T> : EntityComponentContainer where T : Motion_Base, new()
{
	public override System.Type type { get; } = typeof(T);
}