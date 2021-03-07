using Database;
using UnityEngine;

[System.Serializable]
public enum EBrainState
{
	EVASIVE = 0,
	NORMAL = 1,
	ALARMED = 2,
	SEEKER = 3,
	ATTACKER = 4,
	COUNT = 5
}

public interface IEntityComponent_Behaviours
{
	TargetInfo				TargetInfo				{ get; }
	EntityBlackBoardData	BlackBoardData			{ get; }
	EBrainState				BrainState				{ get; }

	void					ChangeState				(EBrainState newState);

	void					OnDestinationReached	(Vector3 position);
}

public abstract class Behaviours_Base : EntityComponent, IEntityComponent_Behaviours
{
	[SerializeField]
	protected	TargetInfo					m_TargetInfo					= new TargetInfo();
	[SerializeField]
	protected	EntityBlackBoardData		m_BlackBoardData				= null;
	[SerializeField]
	protected	EBrainState					m_CurrentBrainState				= EBrainState.COUNT;

	protected	AIBehaviour					m_CurrentBehaviour				= new Behaviour_Empty();
	protected	AIBehaviour[]				m_Behaviours					= new AIBehaviour[5] { null, null, null, null, null };
	protected	bool						m_IsBrainActive					= true;

	public		AIBehaviour					CurrentBehaviour				=> m_CurrentBehaviour;
	public		TargetInfo					TargetInfo						=> m_TargetInfo;
	public		EntityBlackBoardData		BlackBoardData					=> m_BlackBoardData;
	public		EBrainState					BrainState						=> m_CurrentBrainState;
	


	//////////////////////////////////////////////////////////////////////////
	protected override void Resolve_Internal(Entity entity, Section entitySection)
	{
		
	}

	//////////////////////////////////////////////////////////////////////////
	public override void OnSave(StreamUnit streamUnit)
	{
		base.OnSave(streamUnit);

		foreach (AIBehaviour b in m_Behaviours)
		{
			b.OnSave(streamUnit);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public override void OnLoad(StreamUnit streamUnit)
	{
		foreach (AIBehaviour b in m_Behaviours)
		{
			b.OnLoad(streamUnit);
		}

		m_TargetInfo = new TargetInfo();

		base.OnLoad(streamUnit);
	}

	//////////////////////////////////////////////////////////////////////////
	protected abstract void SetBehaviour(EBrainState brainState, string behaviourId);

	//////////////////////////////////////////////////////////////////////////
	public abstract void ChangeState(EBrainState newState);

	//////////////////////////////////////////////////////////////////////////
	public abstract void OnDestinationReached(Vector3 position);
}

public class EntityComponentContainer_Behaviours<T> : EntityComponentContainer where T : Behaviours_Base, new()
{
	public override System.Type type { get; } = typeof(T);
}
