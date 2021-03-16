using UnityEngine;

public abstract class MotionStrategyBase : MonoBehaviour
{
	[System.Serializable]
	public class EntityStates
	{
		public	bool	IsCrouched		= false;

		public	bool	IsMoving		= false;
		public	bool	IsWalking		= false;
		public	bool	IsRunning		= false;


		public	bool	IsJumping		= false;
		public	bool	IsAcending		= false;
		public	bool	IsDescending	= false;

		public void Reset()
		{
			IsMoving = IsWalking = IsRunning = IsJumping = IsAcending = IsDescending = false;
		}

		public void Assign(EntityStates other)
		{
			IsCrouched		= other.IsCrouched;

			IsMoving		= other.IsMoving;
			IsWalking		= other.IsWalking;
			IsRunning		= other.IsRunning;
				
			IsJumping		= other.IsJumping;
			IsAcending		= other.IsAcending;
			IsDescending	= other.IsDescending;
		}
	};

	protected struct MotionBindingsData
	{
		public EInputCommands command;
		public string id;
		public System.Action action;
		public System.Func<bool> predicate;

		public MotionBindingsData(EInputCommands command, string id, System.Action action, System.Func<bool> predicate)
		{
			this.command = command;
			this.id = id;
			this.action = action;
			this.predicate = predicate;
		}
	}

	protected abstract MotionBindingsData[]			m_Bindings { get; }

	[SerializeField]
	protected				EntityStates	m_States			= new EntityStates();

	//[SerializeField] //[System.NonSerialized]
	protected				EntityStates	m_PreviousStates	= new EntityStates();

	[SerializeField]
	protected				Entity			m_Entity			= null;

	[SerializeField]
	protected				Transform		m_Head				= null;

	[SerializeField]
	protected				Transform		m_Body				= null;

	[SerializeField]
	protected				Vector3			m_Move				= Vector3.zero;

	public	abstract		bool			CanMove				{ get; }

	public					EntityStates	States				=> m_States;

	//////////////////////////////////////////////////////////////////////////
	public virtual void Setup(Entity entity, Database.Section entitySection)
	{
		m_Entity = entity;
		m_Head = entity.Head;
		m_Body = entity.Body;

		CustomAssertions.IsNotNull(m_Entity);
		CustomAssertions.IsNotNull(m_Body);
		CustomAssertions.IsNotNull(m_Head);

		Setup_Internal(entitySection);
	}

	//////////////////////////////////////////////////////////////////////////
	protected abstract void Setup_Internal(Database.Section entitySection);


	//////////////////////////////////////////////////////////////////////////
	public abstract void Move(EMovementType movementType, Vector3 direction);


	//////////////////////////////////////////////////////////////////////////
	public virtual void Enable()
	{
		if (m_Entity.EntityType == EEntityType.ACTOR)
		{
			foreach (MotionBindingsData bindingsData in m_Bindings)
			{
				GlobalManager.InputMgr.BindCall(bindingsData.command, bindingsData.id, bindingsData.action, bindingsData.predicate);
			}
		}

		CustomAssertions.IsNotNull(GameManager.UpdateEvents);

		GameManager.UpdateEvents.OnPhysicFrame	+= OnPhysicFrame;
		GameManager.UpdateEvents.OnFrame		+= OnFrame;
		GameManager.UpdateEvents.OnLateFrame	+= OnLateFrame;
	}


	//////////////////////////////////////////////////////////////////////////
	public virtual void Disable()
	{
		if (m_Entity.EntityType == EEntityType.ACTOR)
		{
			foreach (MotionBindingsData bindingsData in m_Bindings)
			{
				GlobalManager.InputMgr.UnbindCall(bindingsData.command, bindingsData.id);
			}
		}

		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnPhysicFrame	-= OnPhysicFrame;
			GameManager.UpdateEvents.OnFrame		-= OnFrame;
			GameManager.UpdateEvents.OnLateFrame	-= OnLateFrame;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnPhysicFrame(float fixedDeltaTime)
	{

	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnFrame(float deltaTime)
	{

	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnLateFrame(float deltaTime)
	{
		// trace previuos states
		m_PreviousStates.Assign(m_States);

		// Reset "local" states
		m_States.Reset();
	}
}
