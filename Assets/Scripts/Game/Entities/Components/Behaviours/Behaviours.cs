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

[System.Serializable]
public enum ELookTargetType : short
{
	POSITION,
	TRANSFORM
};

[System.Serializable]
public class LookData
{
	public bool				HasLookAtObject		= false;
	public Vector3			PointToLookAt		= Vector3.zero;
	public Transform		TransformToLookAt	= null;
	public ELookTargetType	LookTargetType		= ELookTargetType.POSITION;
};



public interface IEntityComponent_Behaviours
{
	TargetInfo				TargetInfo				{ get; }
	EntityBlackBoardData	BlackBoardData			{ get; }
	EBrainState				BrainState				{ get; }

	void					ChangeState				(in EBrainState newState);

	void					OnDestinationReached	(in Vector3 position);

	void					SetTransformToLookAt	(in Transform t);
	void					SetPointToLookAt		(in Vector3 point);
	void					StopLooking				();
}

public abstract class Behaviours_Base : EntityComponent, IEntityComponent_Behaviours
{
	[SerializeField]
	protected	LookData					m_LookData						= new LookData();
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

	public		LookData					LookData						=> m_LookData;
	


	//////////////////////////////////////////////////////////////////////////
	protected override void Resolve_Internal(Entity entity, Section entitySection)
	{
		UnityEngine.Assertions.Assert.IsTrue(entity.EntityType != EEntityType.ACTOR);
		
		// AI BEHAVIOURS
		{
			UnityEngine.Assertions.Assert.IsTrue(entitySection.HasKey("BehaviourEvasive"));
			UnityEngine.Assertions.Assert.IsTrue(entitySection.HasKey("BehaviourNormal"));
			UnityEngine.Assertions.Assert.IsTrue(entitySection.HasKey("BehaviourAlarmed"));
			UnityEngine.Assertions.Assert.IsTrue(entitySection.HasKey("BehaviourSeeker"));
			UnityEngine.Assertions.Assert.IsTrue(entitySection.HasKey("BehaviourAttacker"));

			SetBehaviour( EBrainState.EVASIVE,	entitySection.AsString( "BehaviourEvasive"	) );
			SetBehaviour( EBrainState.NORMAL,	entitySection.AsString( "BehaviourNormal"	) );
			SetBehaviour( EBrainState.ALARMED,	entitySection.AsString( "BehaviourAlarmed"	) );
			SetBehaviour( EBrainState.SEEKER,	entitySection.AsString( "BehaviourSeeker"	) );
			SetBehaviour( EBrainState.ATTACKER, entitySection.AsString( "BehaviourAttacker"	) );
		}

		// BLACKBOARD
		m_BlackBoardData = new EntityBlackBoardData(m_Entity)
		{
			EntityRef = m_Entity,
			LookData = m_LookData,
			TargetInfo = m_TargetInfo,
			SpawnBodyLocation = m_Entity.Body.position,
			SpawnBodyRotation = m_Entity.Body.rotation,
			SpawnHeadLocation = m_Entity.Body.position,
			SpawnHeadRotation = m_Entity.Body.rotation,
		};
	}


	//////////////////////////////////////////////////////////////////////////
	public override void Enable()
	{
		base.Enable();

		if (m_CurrentBrainState == EBrainState.COUNT)
		{
			ChangeState(EBrainState.NORMAL);
		}

		Blackboard.Instance.Register(m_Entity.Id, m_BlackBoardData);
	}


	//////////////////////////////////////////////////////////////////////////
	public override void Disable()
	{
		Blackboard.Instance.UnRegister(m_Entity);

		base.Disable();
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
	private void SetBehaviour(in EBrainState brainState, in string behaviourId)
	{
		// Pre-set empty behaviour as default
		m_Behaviours[(int)brainState] = new Behaviour_Empty();

		if (behaviourId == null || behaviourId.Trim().Length == 0)
		{
			Debug.Log($"Setting invalid behaviour for state {brainState}, with id {behaviourId}, for entity {m_Entity.name}");
			return;
		}

		// Check behaviour id validity
		System.Type type = System.Type.GetType(behaviourId.Trim());
		if (type == null)
		{
			Debug.Log($"Setting invalid behaviour with id {behaviourId}");
			return;
		}

		// Check behaviour type as child of AIBehaviour
		if (!type.IsSubclassOf(typeof(AIBehaviour)))
		{
			Debug.Log($"Class Requested is not a supported behaviour {behaviourId}");
			return;
		}

		AIBehaviour behaviour = System.Activator.CreateInstance(type) as AIBehaviour;

		// Behaviour assignment
		m_Behaviours[(int)brainState] = behaviour;
	}
	
	
	//////////////////////////////////////////////////////////////////////////
	public abstract void ChangeState(in EBrainState newState);


	//////////////////////////////////////////////////////////////////////////
	public abstract void OnDestinationReached(in Vector3 position);


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Set the Transform to Look At </summary>
	public		virtual		void	SetTransformToLookAt(in Transform targetTransform)
	{
		UnityEngine.Assertions.Assert.IsNotNull(targetTransform);
		m_LookData.HasLookAtObject		= true;
		m_LookData.TransformToLookAt	= targetTransform;
		m_LookData.PointToLookAt		= Vector3.zero;
		m_LookData.LookTargetType		= ELookTargetType.TRANSFORM;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Set the point to Look At </summary>
	public		virtual		void	SetPointToLookAt(in Vector3 point)
	{
		m_LookData.HasLookAtObject		= true;
		m_LookData.TransformToLookAt	= null;
		m_LookData.PointToLookAt		= point;
		m_LookData.LookTargetType		= ELookTargetType.POSITION;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Stop looking to target point or target transform </summary>
	public		virtual		void	StopLooking()
	{
		m_LookData.HasLookAtObject		= false;
		m_LookData.TransformToLookAt	= null;
		m_LookData.PointToLookAt		= Vector3.zero;
		m_LookData.LookTargetType		= ELookTargetType.POSITION;
	}
}

public class EntityComponentContainer_Behaviours<T> : EntityComponentContainer where T : Behaviours_Base, new()
{
	public override System.Type type { get; } = typeof(T);
}
