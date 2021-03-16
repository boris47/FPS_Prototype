using UnityEngine;
using UnityEngine.AI;

public interface INavigation_Common : IEntityComponent_Navigation
{

}

public class Navigation_Common : Navigation_Base, INavigation_Common
{
	protected const float				PATH_SEARCH_TIMEOUT				= 3.0f;

	/// <summary> Flag set if body of entity is aligned with target </summary>
	protected	bool					m_IsAllignedBodyToPoint			= false;

	/// <summary> Flag set if head of entity is aligned with target </summary>
	protected	bool					m_IsAllignedHeadToPoint			= false;
	
	/// <summary>  </summary>
	protected	bool					m_IsDisallignedHeadWithPoint	= false;

	[SerializeField]
	protected	float					m_MinEngageDistance				= 0f;

	public		bool					IsAllignedHeadToPoint			=> m_IsAllignedHeadToPoint;
	public		bool					IsDisallignedHeadWithPoint		=> m_IsDisallignedHeadWithPoint;
	public		bool					IsAllignedBodyToPoint			=> m_IsAllignedBodyToPoint;
	public		float					MinEngageDistance				=> m_MinEngageDistance;

	//////////////////////////////////////////////////////////////////////////
	protected override void Resolve_Internal(Entity entity, Database.Section entitySection)
	{
		CustomAssertions.IsTrue(Utils.Base.TrySearchComponent(entity.gameObject, ESearchContext.LOCAL, out m_NavAgent));
		CustomAssertions.IsTrue(m_NavAgent.isOnNavMesh, $"m_NavAgent.isOnNavMesh is false for {name}");
	}


	//////////////////////////////////////////////////////////////////////////
	public override void RequestMovement(Vector3 Destination)
	{
		if (!m_HasPendingPathRequest)
		{
			m_PendingPathRequestCO = CoroutinesManager.Start(RequestMovementCO(Destination), "Entity::RequestMovement: Request of movement");
			m_HasPendingPathRequest = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public override void NavStop()
	{
		m_NavAgent.isStopped = true;

		// Reset internals
		NavReset();
	}


	//////////////////////////////////////////////////////////////////////////
	public override void OnPathSearchTimeOutReached()
	{

	}


	//////////////////////////////////////////////////////////////////////////
	public override void NavReset()
	{
		m_HasDestination		= false;
		m_DestinationToReach	= Vector3.zero;
		m_NavCanMoveAlongPath	= false;

		if (m_HasPendingPathRequest)
		{
			m_HasPendingPathRequest = false;

			if (m_PendingPathRequestCO.IsNotNull())
			{
				StopCoroutine(m_PendingPathRequestCO);
			}
			m_PendingPathRequestCO = null;
		}
	}


	/////////////////////////////////////////////////////////////////////////
	private System.Collections.IEnumerator RequestMovementCO(Vector3 destination)
	{
		m_NavAgent.SetDestination(destination);
		m_NavAgent.isStopped = true;

		float currentSearchTime = 0.0f;
		while (m_NavAgent.pathPending)
		{
			if (m_NavAgent.pathStatus == NavMeshPathStatus.PathPartial)
			{
				print($"For {name} destination is unreachable");
				m_HasPendingPathRequest = false;
				yield break;
			}

			currentSearchTime += Time.fixedUnscaledDeltaTime;
			if (currentSearchTime > PATH_SEARCH_TIMEOUT)
			{
				print("On Path Search TimeOut Reached");
				OnPathSearchTimeOutReached();
				m_HasPendingPathRequest = false;
				yield break;
			}
			yield return null;
		}
		m_NavAgent.isStopped			= false;
		m_HasPendingPathRequest			= false;
		m_PendingPathRequestCO			= null;
		m_HasDestination				= true;
		m_DestinationToReach			= destination;
		m_NavCanMoveAlongPath			= true;
		OnPathReady_Internal(destination);
	}
}
