using UnityEngine;

[System.Serializable]
public enum ELookTargetType : short
{
	POSITION,
	TRANSFORM
};

[System.Serializable]
public enum ELookTargetMode : short
{
	HEAD_ONLY,
	WITH_BODY
}

public class LookData
{
	public bool				HasLookAtObject		= false;
	public Vector3			PointToLookAt		= Vector3.zero;
	public Transform		TransformToLookAt	= null;
	public ELookTargetType	LookTargetType		= ELookTargetType.POSITION;
	public ELookTargetMode	LookTargetMode		= ELookTargetMode.HEAD_ONLY;
};

public interface IBehaviours_Common : IEntityComponent_Behaviours
{
	LookData				LookData				{ get; }
	FieldOfView				FieldOfView				{ get; }
}

public class Behaviours_Common : Behaviours_Base
{
	[SerializeField]
	protected	LookData					m_LookData						= new LookData();
	[SerializeField]
	protected	float						m_BodyRotationSpeed				= 5.0f;
	[SerializeField]
	protected	float						m_HeadRotationSpeed				= 5.0f;
	[SerializeField]
	protected	Quaternion					m_RotationToAllignTo			= Quaternion.identity;

	protected	FieldOfView					m_FieldOfView					= null;

	public		LookData					LookData						=> m_LookData;
	public		FieldOfView					FieldOfView						=> m_FieldOfView;

	//////////////////////////////////////////////////////////////////////////
	protected override void Resolve_Internal(Entity entity, Database.Section entitySection)
	{
		base.Resolve_Internal(entity, entitySection);

		UnityEngine.Assertions.Assert.IsTrue(entity.EntityType != EEntityType.ACTOR);

		// FIELD OF VIEW
		// TODO maybe insteand of assert this, maybe would be better create a new one and configure be entity section
		UnityEngine.Assertions.Assert.IsTrue(Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL_AND_CHILDREN, out m_FieldOfView));
		m_FieldOfView.SetViewPoint(entity.Head);

		// Target type by section
		string targetType = string.Empty;
		if (entitySection.TryAsString("DefaultTarget", out targetType) && Utils.Converters.StringToEnum(targetType, out EEntityType type))
		{
			m_FieldOfView.TargetType = type;
		}

		// BRAINSTATE
		m_CurrentBrainState = EBrainState.COUNT;
		{
			Brain_Enable();             // Setup for field of view and memory
			Brain_SetActive( true );	// Brain updates activation

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

				ChangeState( EBrainState.NORMAL );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		// BLACKBOARD
		m_BlackBoardData = new EntityBlackBoardData(m_Entity)
		{
			EntityRef			= m_Entity,
			LookData			= m_LookData,
			TargetInfo			= m_TargetInfo,
			SpawnBodyLocation	= m_Entity.Body.position,
			SpawnBodyRotation	= m_Entity.Body.rotation,
			SpawnHeadLocation	= m_Entity.Body.position,
			SpawnHeadRotation	= m_Entity.Body.rotation,
		};
		Blackboard.Register(m_Entity.Id, m_BlackBoardData);
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		Blackboard.UnRegister(m_Entity);
	}


	//////////////////////////////////////////////////////////////////////////
	protected	void	Brain_Enable()
	{
		m_FieldOfView.Setup();

		Brain_SetActive(true);

		m_Entity.Memory.EnableMemory();
	}


	//////////////////////////////////////////////////////////////////////////
	protected	void	Destroy_Disable()
	{
		Brain_SetActive(false);

		m_Entity.Memory.DisableMemory();
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void SetBehaviour(EBrainState brainState, string behaviourId)
	{
		// Pre-set empty behaviour as default
		m_Behaviours[(int)brainState] = new Behaviour_Empty();

		if (behaviourId == null || behaviourId.Trim().Length == 0)
		{
			Debug.Log("Brain.SetBehaviour Setting invalid behaviour for state " + brainState + ", with id" + behaviourId + ", for entity (section) " + m_Entity.SectionName);
			return;
		}

		// Check behaviour id validity
		System.Type type = System.Type.GetType(behaviourId.Trim());
		if (type == null)
		{
			Debug.Log("Brain.SetBehaviour Setting invalid behaviour with id " + behaviourId);
			return;
		}

		// Check behaviour type as child of AIBehaviour
		if (!type.IsSubclassOf(typeof(AIBehaviour)))
		{
			Debug.Log("Brain.SetBehaviour Class Requested is not a supported behaviour " + behaviourId);
			return;
		}

		// Setup of the instanced behaviour
		AIBehaviour behaviour = System.Activator.CreateInstance(type) as AIBehaviour;
		behaviour.Setup(m_Entity.Id);

		// Behaviour assignment
		m_Behaviours[(int)brainState] = behaviour;
	}



	//////////////////////////////////////////////////////////////////////////
	/// <summary> Set the Transform to Look At </summary>
	public		virtual		void	SetTransformToLookAt( Transform t, ELookTargetMode LookMode = ELookTargetMode.HEAD_ONLY )
	{
		m_LookData.HasLookAtObject		= true;
		m_LookData.TransformToLookAt	= t;
		m_LookData.PointToLookAt		= Vector3.zero;
		m_LookData.LookTargetType		= ELookTargetType.TRANSFORM;
		m_LookData.LookTargetMode		= LookMode;
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Set the point to Look At </summary>
	public		virtual		void	SetPointToLookAt( Vector3 point, ELookTargetMode LookMode = ELookTargetMode.HEAD_ONLY )
	{
		m_LookData.HasLookAtObject		= true;
		m_LookData.TransformToLookAt	= null;
		m_LookData.PointToLookAt		= point;
		m_LookData.LookTargetType		= ELookTargetType.POSITION;
		m_LookData.LookTargetMode		= LookMode;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Stop looking to target point or target </summary>
	public		virtual		void	StopLooking()
	{
		m_LookData.HasLookAtObject		= false;
		m_LookData.TransformToLookAt	= null;
		m_LookData.PointToLookAt		= Vector3.zero;
		m_LookData.LookTargetType		= ELookTargetType.POSITION;
		m_LookData.LookTargetMode		= ELookTargetMode.HEAD_ONLY;
	}

	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void	UpdateHeadRotation()
	{
		if (m_LookData.HasLookAtObject == false )
			return;
		/*
		// HEAD
		{
			Vector3 pointToLookAt = m_LookData.LookTargetType == ELookTargetType.TRANSFORM ? m_LookData.TransformToLookAt.position : m_LookData.PointToLookAt;

			// point on the head 'Horizontal'  plane
			Vector3 pointOnHeadPlane	= Utils.Math.ProjectPointOnPlane( m_BodyTransform.up, m_HeadTransform.position, pointToLookAt );

			// point on the entity 'Horizontal' plane
			Vector3 pointOnEntityPlane	= Utils.Math.ProjectPointOnPlane( m_BodyTransform.up, transform.position, pointToLookAt );

			// Direction from head to point
			Vector3 dirHeadToPosition	= ( pointOnHeadPlane - m_HeadTransform.position );

			// Direction from entity to point
			Vector3 dirEntityToPosition	= ( pointOnEntityPlane - transform.position );

			// Angle between head and projected point
			float lookDeltaAngle = Vector3.Angle( m_HeadTransform.forward, dirHeadToPosition );

			// Current head allignment state
			bool isCurrentlyAlligned = lookDeltaAngle < 4f;
		
			// Head allignment comparison and event
			{
				bool wasPreviousAlligned = m_IsAllignedHeadToPoint;
				if ( wasPreviousAlligned == false && isCurrentlyAlligned == true )
				{
					OnLookRotationReached(m_HeadTransform.forward );
				}
			}

			// Flags assignment
			m_IsAllignedHeadToPoint			= isCurrentlyAlligned;
			m_IsDisallignedHeadWithPoint	= lookDeltaAngle > 90f;
			
			// Rotation Speed
			float rotationSpeed = m_HeadRotationSpeed * ( (m_TargetInfo.HasTarget ) ? 3.0f : 1.0f ) * Time.deltaTime;

			// Execute Rotation
			if (m_LookData.LookTargetMode == ELookTargetMode.WITH_BODY )
			{
				m_RotationToAllignTo.SetLookRotation( dirEntityToPosition, m_BodyTransform.up );
				transform.rotation = Quaternion.RotateTowards(transform.rotation, m_RotationToAllignTo, rotationSpeed );
			}
			// Head only
			else
			{
				m_RotationToAllignTo.SetLookRotation( dirHeadToPosition, m_BodyTransform.up );
				m_HeadTransform.rotation = Quaternion.RotateTowards(m_HeadTransform.rotation, m_RotationToAllignTo, rotationSpeed );
			}
		}
		*/
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual void	Brain_SetActive(bool State)
	{
		m_IsBrainActive = State;

		if (m_IsBrainActive)
		{
			GameManager.FieldsOfViewManager.RegisterAgent(m_FieldOfView, m_FieldOfView.UpdateFOV);
		}
		else
		{
			GameManager.FieldsOfViewManager?.UnregisterAgent(m_FieldOfView);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	OnThinkBrain()
	{
		if (m_IsBrainActive)
		{
			m_Entity.Memory.UpdateMemory();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public override void ChangeState(EBrainState newState)
	{
		if (newState != m_CurrentBrainState)
		{
			UnityEngine.Assertions.Assert.IsNotNull(GameManager.UpdateEvents);

			if (m_CurrentBehaviour.IsNotNull())
			{
				m_CurrentBehaviour.OnDisable();

				m_FieldOfView.OnTargetAquired			= null;
				m_FieldOfView.OnTargetChanged			= null;
				m_FieldOfView.OnTargetLost				= null;

				GameManager.UpdateEvents.OnThink		-= m_CurrentBehaviour.OnThink;
				GameManager.UpdateEvents.OnPhysicFrame	-= m_CurrentBehaviour.OnPhysicFrame;
				GameManager.UpdateEvents.OnFrame		-= m_CurrentBehaviour.OnFrame;
				GameManager.UpdateEvents.OnLateFrame	-= m_CurrentBehaviour.OnLateFrame;

				m_Entity.OnEvent_Killed					-= m_CurrentBehaviour.OnKilled;
			}

			{
				m_CurrentBrainState = newState;
				AIBehaviour nextBehaviour = m_Behaviours[(int)newState];
				UnityEngine.Assertions.Assert.IsNotNull(nextBehaviour, $"next behaviour is not valid for {name}");
				m_CurrentBehaviour = m_Behaviours[(int)newState];
			}

			{
				m_Entity.OnEvent_Killed					+= m_CurrentBehaviour.OnKilled;

				GameManager.UpdateEvents.OnThink		+= m_CurrentBehaviour.OnThink;
				GameManager.UpdateEvents.OnPhysicFrame	+= m_CurrentBehaviour.OnPhysicFrame;
				GameManager.UpdateEvents.OnFrame		+= m_CurrentBehaviour.OnFrame;
				GameManager.UpdateEvents.OnLateFrame	+= m_CurrentBehaviour.OnLateFrame;

				m_FieldOfView.OnTargetAquired			= m_CurrentBehaviour.OnTargetAcquired;
				m_FieldOfView.OnTargetChanged			= m_CurrentBehaviour.OnTargetChange;
				m_FieldOfView.OnTargetLost				= m_CurrentBehaviour.OnTargetLost;

				m_CurrentBehaviour.OnEnable();
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public override void OnDestinationReached(Vector3 position)
	{
		m_CurrentBehaviour.OnDestinationReached(position);
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void	Brain_OnReset()
	{
		ChangeState(EBrainState.NORMAL);
		m_FieldOfView.OnReset();
	}
}