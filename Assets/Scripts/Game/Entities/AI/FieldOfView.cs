
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public	delegate	void	OnTargetEvent( TargetInfo targetInfo );
public	delegate	void	OnTargetsAcquired( Entity[] entities );

public enum ETargetInfoType
{
	NONE, ACQUIRED, CHANGED, LOST
}

public enum EAcquisitionStrategy
{
	FAREST, CLOSEST, WEAKER, HARDER, TILL_ELIMINATION
}

[System.Serializable]
public class TargetInfo
{
	public bool HasTarget;
	public Entity CurrentTarget;
	public ETargetInfoType Type;

	public void Update(TargetInfo Infos)
	{
		HasTarget = Infos.HasTarget;
		CurrentTarget = Infos.CurrentTarget;
	}

	public void Reset()
	{
		Type = ETargetInfoType.NONE;
		HasTarget = false;
		CurrentTarget = null;
	}
}

[RequireComponent(typeof(SphereCollider))]
public class FieldOfView : MonoBehaviour
{
	private		const	float						UPDATE_TIME						= 200f; // 200 ms
	private				SphereCollider				m_ViewTriggerCollider			= null;

	private				EEntityType					m_PrevEntityType				= EEntityType.NONE;
	private				List<Entity>				m_AllTargets					= new List<Entity>();
	private				IEnumerable<Entity>			m_CurrentValidTargets			= new List<Entity>();
	private				Entity						m_Owner							= null;
	private				OnTargetEvent				m_OnTargetAquired				= null;
	private				OnTargetEvent				m_OnTargetChanged				= null;
	private				OnTargetEvent				m_OnTargetLost					= null;
	private				OnTargetsAcquired			m_OnTargetsAcquired				= null;
	private				TargetInfo					m_CurrentTargetInfo				= new TargetInfo();
	private				float						m_UpdateTimer					= 0f;

	[SerializeField]
	private				Transform					m_ViewPoint						= null;
	[SerializeField, Range(1f, 200f)]
	private				float						m_ViewDistance					= 10f;
	[SerializeField, Range(1f, 150f)]
	private				float						m_ViewCone						= 100f;
	[SerializeField]
	private				EEntityType					m_EntityType					= EEntityType.NONE;
	[SerializeField]
	private				EAcquisitionStrategy		m_strategy						= EAcquisitionStrategy.CLOSEST;

	public				OnTargetEvent				OnTargetAquired					{ set => m_OnTargetAquired   = value;  }
	public				OnTargetEvent				OnTargetChanged					{ set => m_OnTargetChanged   = value;  }
	public				OnTargetEvent				OnTargetLost					{ set => m_OnTargetLost	     = value;  }
	public				OnTargetsAcquired			OnTargetsAcquired				{ set => m_OnTargetsAcquired = value;  }

	public				float						Distance						{ get => m_ViewDistance; set => m_ViewDistance = value; }
	public				float						Angle							{ get => m_ViewCone;	 set => m_ViewCone     = value; }
	public				EEntityType					TargetType						{ get => m_EntityType;   set => m_EntityType   = value; }


	//////////////////////////////////////////////////////////////////////////
	private void	Awake()
	{
		if (CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_ViewTriggerCollider)))
		{
			CustomAssertions.IsTrue(m_ViewTriggerCollider.isTrigger);
			m_ViewTriggerCollider.radius = m_ViewDistance;
		}

		m_UpdateTimer = UnityEngine.Random.Range(1f, UPDATE_TIME);
	}

	//////////////////////////////////////////////////////////////////////////
	private void	OnValidate()
	{
		m_ViewTriggerCollider = m_ViewTriggerCollider ?? gameObject.GetOrAddIfNotFound<SphereCollider>();
		m_ViewTriggerCollider.isTrigger = true;
		m_ViewTriggerCollider.radius = m_ViewDistance;
	}

	private void OnEnable()
	{
		if (CustomAssertions.IsNotNull(GameManager.UpdateEvents))
		{
			GameManager.UpdateEvents.OnFrame += OnFrame;
			GameManager.UpdateEvents.OnThink += UpdateTarget;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnThink -= UpdateTarget;
			GameManager.UpdateEvents.OnFrame -= OnFrame;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public	void	Setup()
	{
		CustomAssertions.IsNotNull(m_ViewTriggerCollider, "Collider required in this context");
	
		CustomAssertions.IsTrue(Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL_AND_PARENTS, out m_Owner));

		if (transform.parent && transform.parent.TrySearchComponents(ESearchContext.LOCAL_AND_CHILDREN, out Collider[] colliders))
		{
			System.Array.ForEach(colliders, c => Physics.IgnoreCollision(m_ViewTriggerCollider, c, true));
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public void		SetViewPoint(in Transform viewPoint)
	{
		m_ViewPoint = viewPoint ?? m_ViewPoint;

		SelectTargetsByBaseConditions();
	}


	//////////////////////////////////////////////////////////////////////////
	private static bool IsValidEntity(in Entity target)
	{
		return target.IsAlive;
	}

	//////////////////////////////////////////////////////////////////////////
	private static bool InsideViewCone(in Entity target, in Vector3 viewPointPosition, in Vector3 viewPointDirection, in float viewCone)
	{
		Vector3 targettablePosition = target.Targettable.position;
		Vector3 direction = (targettablePosition - viewPointPosition);
		float angle = Vector3.Angle(direction, viewPointDirection);
		return (angle <= (viewCone * 0.5f));
	}

	//////////////////////////////////////////////////////////////////////////
	private static bool IsTargettable(in Entity target, in Vector3 viewPointPosition, in float viewDistance)
	{
		Vector3 direction = (target.Targettable.position - viewPointPosition);
		if (Physics.Raycast(viewPointPosition, direction, out RaycastHit m_RaycastHit, viewDistance/*, 0, QueryTriggerInteraction.Ignore*/))
		{
			int colliderInstanceID = m_RaycastHit.collider.GetInstanceID();
			int entityPhysicColliderInstanceID = target.PhysicCollider.GetInstanceID();
			int shieldColliderInstanceID = target.EntityShield?.Collider.GetInstanceID() ?? -1;
			return (colliderInstanceID == entityPhysicColliderInstanceID || colliderInstanceID == shieldColliderInstanceID);
		}
		return false;
	}


	//////////////////////////////////////////////////////////////////////////
	private void SelectTargetsByBaseConditions()
	{
		if (m_ViewPoint)
		{
			Vector3 p = m_ViewPoint.position;
			Vector3 d = m_ViewPoint.forward;
			m_CurrentValidTargets = m_AllTargets.Where(e => e && IsValidEntity(e) && InsideViewCone(e, p, d, m_ViewCone) && IsTargettable(e, p, m_ViewDistance));
		}
		else
		{
			m_CurrentValidTargets = System.Linq.Enumerable.Repeat<Entity>(null, 0);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnFrame(float deltaTime)
	{
		m_UpdateTimer -= deltaTime;
		if (m_UpdateTimer < 0f)
		{
			m_UpdateTimer = UPDATE_TIME;
			SelectTargetsByBaseConditions();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void UpdateTarget()
	{
		// If target is valid and visible select by strategy
		if (TrySelectTargetByStrategy(m_CurrentValidTargets, m_strategy, out Entity choosenTarget))
		{
			Entity previousTarget = m_CurrentTargetInfo.CurrentTarget;
			m_CurrentTargetInfo.CurrentTarget = choosenTarget;

			// SET NEW TARGET
			if (!m_CurrentTargetInfo.HasTarget)
			{
				m_CurrentTargetInfo.HasTarget = true;
				m_CurrentTargetInfo.Type = ETargetInfoType.ACQUIRED;
				m_OnTargetAquired(m_CurrentTargetInfo);
			}
			else
			// CHANGING A TARGET
			{
				if (previousTarget.IsNotNull() && previousTarget.Id != choosenTarget.Id)
				{
					m_CurrentTargetInfo.Type = ETargetInfoType.LOST;
					m_OnTargetChanged(m_CurrentTargetInfo);
				}
			}
		}
		else // when no target can be found (EX: m_AllTargets is empty)
		{
			// TARGET LOST
			if (m_CurrentTargetInfo.HasTarget)
			{
				m_CurrentTargetInfo.Type = ETargetInfoType.LOST;
				m_OnTargetLost(m_CurrentTargetInfo);
			}
			m_CurrentTargetInfo.Reset();
		}
		
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Return true if and entity has been choosen, otherwise false. Return false with empty list. </summary>
	private bool TrySelectTargetByStrategy(in IEnumerable<Entity> availableTargets, in EAcquisitionStrategy strategy, out Entity entity)
	{
		entity = null;

		if (availableTargets.Count() == 0)
		{
			return false;
		}

		switch (m_strategy)
		{
			case EAcquisitionStrategy.CLOSEST: case EAcquisitionStrategy.FAREST:
			{
				Vector3 viewPointOrigin = m_ViewPoint.position;
				entity = FieldOfView.GetTargetByDistance(availableTargets, strategy, viewPointOrigin);
				break;
			}
			case EAcquisitionStrategy.WEAKER: case EAcquisitionStrategy.HARDER:
			{
				entity = FieldOfView.GetTargetByHealth(availableTargets, strategy);
				break;
			}
			case EAcquisitionStrategy.TILL_ELIMINATION:
			{
				entity = FieldOfView.GetSameTrargetOrRandom(availableTargets, strategy, m_CurrentTargetInfo);
				break;
			}
		}

		return entity.IsNotNull();
	}

	//////////////////////////////////////////////////////////////////////////
	private static Entity GetTargetByDistance(in IEnumerable<Entity> availableTargets, in EAcquisitionStrategy strategy, Vector3 point)
	{
		float Selector(Entity e) => (e.transform.position - point).sqrMagnitude;
		return strategy == EAcquisitionStrategy.CLOSEST ? availableTargets.MinBy(Selector) : availableTargets.MaxBy(Selector);
	}

	//////////////////////////////////////////////////////////////////////////
	private static Entity GetTargetByHealth(in IEnumerable<Entity> availableTargets, in EAcquisitionStrategy strategy)
	{
		return strategy == EAcquisitionStrategy.WEAKER ? availableTargets.MinBy(e => e.Health) : availableTargets.MaxBy(e => e.Health);
	}

	//////////////////////////////////////////////////////////////////////////
	private static Entity GetSameTrargetOrRandom(in IEnumerable<Entity> availableTargets, in EAcquisitionStrategy strategy, in TargetInfo m_CurrentTargetInfo)
	{
		return m_CurrentTargetInfo.CurrentTarget ?? availableTargets.Random();
	}

	//////////////////////////////////////////////////////////////////////////
	private void HandleEntityTargetTypeChange()
	{
		if (m_PrevEntityType != m_EntityType)
		{
			m_PrevEntityType = m_EntityType;

			m_AllTargets.Clear();

			void addToTargets(Collider c)
			{
				if (c.transform.TrySearchComponent(ESearchContext.LOCAL_AND_PARENTS, out Entity entity, e => e.EntityType == m_EntityType) && entity != m_Owner)
				{
					m_AllTargets.Add(entity);
				}
			}

			// This avoid to the current entity being added
			List<Collider> list = new List<Collider>(Physics.OverlapSphere(transform.position, m_ViewTriggerCollider.radius, 1, QueryTriggerInteraction.Ignore));
	//		list.Remove(m_ViewTriggerCollider);
			list.ForEach(addToTargets);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public void	OnReset()
	{
		m_CurrentTargetInfo.Reset();

		m_AllTargets.Clear();
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.TrySearchComponent( ESearchContext.LOCAL_AND_PARENTS, out Entity entity) && entity.IsAlive && entity.EntityType == m_EntityType && !m_AllTargets.Contains(entity))
		{
			m_AllTargets.Add(entity);

			entity.OnEvent_Killed += ( Entity entityKilled ) =>
			{
				m_AllTargets.Remove(entityKilled);
			};
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerExit(Collider other)
	{
		if (other.transform.TrySearchComponent(ESearchContext.LOCAL_AND_PARENTS, out Entity entity) && m_AllTargets.Contains(entity))
		{
			m_AllTargets.Remove(entity);

			entity.OnEvent_Killed -= ( Entity entityKilled ) =>
			{
				m_AllTargets.Remove(entityKilled);
			};
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnDrawGizmosSelected()
	{
		float halfFOV = m_ViewCone * 0.5f;

		Transform currentViewPoint = m_ViewPoint ?? transform;
		Gizmos.matrix = Matrix4x4.TRS( currentViewPoint.position, Quaternion.Euler(transform.rotation * ( Vector3.one * 0.5f ) ), Vector3.one );

		for ( float i = 0; i < 180f; i += 10f )
		{
			float cos = Mathf.Cos( i * Mathf.Deg2Rad );
			float sin = Mathf.Sin( i * Mathf.Deg2Rad );

			Vector3 axisRight =  (currentViewPoint.up  * cos) +  (currentViewPoint.right  * sin);
			Vector3 axisLeft  =  (-currentViewPoint.up * cos) +  (-currentViewPoint.right * sin);

			// left
			Quaternion leftRayRotation		= Quaternion.AngleAxis( halfFOV, axisLeft );
			Vector3 leftRayDirection		= ( leftRayRotation  * currentViewPoint.forward ).normalized;
			Gizmos.DrawRay( Vector3.zero, leftRayDirection  * m_ViewDistance );

			// right
			Quaternion rightRayRotation		= Quaternion.AngleAxis(  halfFOV, axisRight );
			Vector3 rightRayDirection		= ( rightRayRotation * currentViewPoint.forward ).normalized;
			Gizmos.DrawRay( Vector3.zero, rightRayDirection * m_ViewDistance );
		}
		Gizmos.matrix = Matrix4x4.identity;
	}

}