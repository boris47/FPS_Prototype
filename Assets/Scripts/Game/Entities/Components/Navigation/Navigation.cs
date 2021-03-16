using UnityEngine;
using UnityEngine.AI;

public interface IEntityComponent_Navigation
{
	/// <summary> Events called when path has been found for requested destination </summary>
	event			Navigation_Base.OnPathReadyDel	OnPathReady;

//	NavMeshAgent	NavAgent						{ get; }
	bool			NavCanMoveAlongPath				{ get; }
	bool			HasPendingPathRequest			{ get; }
	bool			HasDestination					{ get; }
	Vector3			DestinationToReach				{ get; }
	float			MaxAgentSpeed					{ get; set; }

	void			RequestMovement					(Vector3 destination);
	void			NavStop							();
	void			OnPathSearchTimeOutReached		();
	void			NavReset						();

}

public abstract class Navigation_Base : EntityComponent, IEntityComponent_Navigation
{
	public delegate void OnPathReadyDel(Vector3 destination);

	protected	event OnPathReadyDel	m_OnPathReady				   = delegate { };

	public event OnPathReadyDel OnPathReady
	{
		add		{ if (value.IsNotNull()) m_OnPathReady += value; }
		remove	{ if (value.IsNotNull()) m_OnPathReady -= value; }
	}

	[SerializeField]
	protected	NavMeshAgent			m_NavAgent						= null;
	[SerializeField]
	protected	bool					m_NavCanMoveAlongPath			= false;
	[SerializeField]
	protected	bool					m_HasPendingPathRequest			= false;
	[SerializeField]
	protected	bool					m_HasDestination				= false;
	[SerializeField]
	protected	Vector3					m_DestinationToReach			= Vector3.zero;
	[SerializeField]
	protected	float					m_MaxAgentSpeed					= 0.0f;

	protected	Coroutine				m_PendingPathRequestCO			= null;
	protected	bool					m_WasMoving						= false;

	public		bool					NavCanMoveAlongPath				=> m_NavCanMoveAlongPath;
	public		bool					HasPendingPathRequest			=> m_HasPendingPathRequest;
	public		bool					HasDestination					=> m_HasDestination;
	public		Vector3					DestinationToReach				=> m_DestinationToReach;
	public		float					MaxAgentSpeed					{ get => m_MaxAgentSpeed; set => m_MaxAgentSpeed = value; }

	//////////////////////////////////////////////////////////////////////////
	public abstract void RequestMovement(Vector3 Destination);

	//////////////////////////////////////////////////////////////////////////
	public abstract void NavStop();

	//////////////////////////////////////////////////////////////////////////
	public abstract void OnPathSearchTimeOutReached();

	//////////////////////////////////////////////////////////////////////////
	public abstract void NavReset();

	//////////////////////////////////////////////////////////////////////////
	protected void OnPathReady_Internal(Vector3 destination)
	{
		m_OnPathReady(destination);
	}

	//////////////////////////////////////////////////////////////////////////
	public override void OnSave(StreamUnit streamUnit)
	{
		base.OnSave(streamUnit);
	}

	//////////////////////////////////////////////////////////////////////////
	public override void OnLoad(StreamUnit streamUnit)
	{
		base.OnLoad(streamUnit);

		m_HasPendingPathRequest = false;
		m_NavCanMoveAlongPath = false;
		m_HasDestination = false;
	}

	protected void OnFrame(float deltaTime)
	{
		bool nowIsMoving = m_NavAgent.velocity.sqrMagnitude > 0.0f;
		if (m_HasDestination && m_WasMoving && !nowIsMoving)
		{
			m_Entity.Behaviours.OnDestinationReached(transform.position);
		}
		m_WasMoving = m_NavAgent.velocity.sqrMagnitude > 0.0f;
	}
}

public class EntityComponentContainer_Navigation<T> : EntityComponentContainer where T : Navigation_Base, new()
{
	public override System.Type type { get; } = typeof(T);
}
