using UnityEngine;

public interface IBehaviours_Common : IEntityComponent_Behaviours
{
	LookData				LookData				{ get; }
	FieldOfView				FieldOfView				{ get; }
}

public class Behaviours_Common : Behaviours_Base
{
	[SerializeField]
	protected	float						m_BodyRotationSpeed				= 90.0f;
	[SerializeField]
	protected	float						m_HeadRotationSpeed				= 50.0f;
	[SerializeField]
	protected	Quaternion					m_RotationToAllignTo			= Quaternion.identity;

	protected	FieldOfView					m_FieldOfView					= null;

	public		FieldOfView					FieldOfView						=> m_FieldOfView;

	//////////////////////////////////////////////////////////////////////////
	protected override void Resolve_Internal(Entity entity, Database.Section entitySection)
	{
		base.Resolve_Internal(entity, entitySection);

		CustomAssertions.IsNotNull(entity.Body);
		CustomAssertions.IsNotNull(entity.Head);

		// FIELD OF VIEW
		// TODO maybe insteand of assert this, maybe would be better create a new one and configure be entity section
		if (!m_FieldOfView)
		{
			CustomAssertions.IsTrue(Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL_AND_CHILDREN, out m_FieldOfView));
		}
		m_FieldOfView.SetViewPoint(entity.Head);

		// Target type by section
		string targetType = string.Empty;
		if (entitySection.TryAsString("DefaultTarget", out targetType) && Utils.Converters.StringToEnum(targetType, out EEntityType type))
		{
			m_FieldOfView.TargetType = type;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public override void Enable()
	{
		base.Enable();

		foreach(AIBehaviour behaviour in m_Behaviours)
		{
			behaviour.Setup(m_BlackBoardData);
		}

		m_FieldOfView.Setup();

		Brain_SetActive(true);

		m_Entity.Memory.EnableMemory();

		CustomAssertions.IsNotNull(GameManager.UpdateEvents);

		GameManager.UpdateEvents.OnFrame += OnFrame;
	}


	//////////////////////////////////////////////////////////////////////////
	public override void Disable()
	{
		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnFrame -= OnFrame;
		}

		m_Entity.Memory.DisableMemory();

		Brain_SetActive(false);

		base.Disable();
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnFrame(float deltaTime)
	{
		UpdateLookAt(deltaTime);
	}

	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void	UpdateLookAt(float deltaTime)
	{
		if (m_LookData.HasLookAtObject)
		{
			Vector3 pointToLookAt = m_LookData.LookTargetType == ELookTargetType.TRANSFORM ? m_LookData.TransformToLookAt.position : m_LookData.PointToLookAt;

			float halfCone = m_FieldOfView.m_ViewCone;

			Vector2 clampsVert = new Vector2(-halfCone, halfCone) * 0.5f;
			m_Entity.LookAt(pointToLookAt, m_BodyRotationSpeed, m_HeadRotationSpeed, clampsVert, clampsVert, out bool isBodyAlligned, out bool isHeadAlligned);
		}

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
	public override void ChangeState(in EBrainState newState)
	{
		if (newState != m_CurrentBrainState)
		{
			CustomAssertions.IsNotNull(GameManager.UpdateEvents);

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
				CustomAssertions.IsNotNull(nextBehaviour, $"next behaviour is not valid for {name}");
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
	public override void OnDestinationReached(in Vector3 position)
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