using UnityEngine;
using System.Collections.Generic;


public enum EMotionType
{
	NONE		= 1 << 0,
	WALK		= 1 << 1,
	PLATFORM	= 1 << 2,
	FLY			= 1 << 3,
	SWIM		= 1 << 4,
	COUNT		= 1 << 5
};


public abstract class MotionStrategyBase : MonoBehaviour
{
	[System.Serializable]
	public class EntityStates
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
			IsCrouched		= other.IsCrouched;

			IsMoving		= other.IsMoving;
			IsWalking		= other.IsWalking;
			IsRunning		= other.IsRunning;
							  
			IsLeaning		= other.IsLeaning;
				
			IsJumping		= other.IsJumping;
			IsHanging		= other.IsHanging;
			IsFalling		= other.IsFalling;
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

	protected abstract List<MotionBindingsData>		m_Bindings { get; }

	[SerializeField]
	protected   EntityStates	m_States			= new EntityStates();

	//[SerializeField] //[System.NonSerialized]
	protected	EntityStates	m_PreviousStates	= new EntityStates();

	[SerializeField]
	protected	Entity			m_Entity			= null;

	[SerializeField]
	protected	Transform		m_Head				= null;

	[SerializeField]
	protected	Transform		m_Body				= null;

	[SerializeField]
	protected	Vector3			m_Move				= Vector3.zero;

	[SerializeField]
	protected	bool			m_bInputOverride	= false;

	public		EntityStates	States				=> m_States;


	//////////////////////////////////////////////////////////////////////////
	public virtual void Setup(Entity entity, Transform head, Transform body)
	{
		m_Entity = entity;
		m_Body = body;
		m_Head = head;

		UnityEngine.Assertions.Assert.IsNotNull(m_Entity);
		UnityEngine.Assertions.Assert.IsNotNull(m_Body);
		UnityEngine.Assertions.Assert.IsNotNull(m_Head);
	}

	
	//////////////////////////////////////////////////////////////////////////
	public void SetOverridenInputs(bool bOverrideState)
	{
		m_bInputOverride = bOverrideState;
	}


	//////////////////////////////////////////////////////////////////////////
	public abstract void OverrideMove(ESimMovementType movementType, Vector3 direction);


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnEnable()
	{
		foreach (MotionBindingsData bindingsData in m_Bindings)
		{
			GlobalManager.InputMgr.BindCall(bindingsData.command, bindingsData.id, bindingsData.action, bindingsData.predicate);
		}

		UnityEngine.Assertions.Assert.IsNotNull(GameManager.UpdateEvents);

		GameManager.UpdateEvents.OnPhysicFrame	+= OnPhysicFrame;
		GameManager.UpdateEvents.OnFrame		+= OnFrame;
		GameManager.UpdateEvents.OnLateFrame	+= OnLateFrame;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnDisable()
	{
		foreach (MotionBindingsData bindingsData in m_Bindings)
		{
			GlobalManager.InputMgr.UnbindCall(bindingsData.command, bindingsData.id);
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
	protected virtual void OnLateFrame(float DeltaTime)
	{
		// trace previuos states
		m_PreviousStates.Assign(m_States);

		// Reset "local" states
		m_States.Reset();
	}
}
