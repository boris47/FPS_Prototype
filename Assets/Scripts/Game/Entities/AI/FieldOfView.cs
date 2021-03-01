
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

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
	public IEntity CurrentTarget;
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

public interface IFieldOfView
{	
	OnTargetEvent		OnTargetAquired		{ set; }
	OnTargetEvent		OnTargetChanged		{ set; }
	OnTargetEvent		OnTargetLost		{ set; }

	float				Distance			{ get; set; }
	float				Angle				{ get; set; }
	EEntityType			TargetType			{ get; set; }

	void				Setup				();
	void				UpdateFOV			();
	void				OnReset				();
}



[RequireComponent( typeof( SphereCollider ) )]
public class FieldOfView : MonoBehaviour, IFieldOfView
{
	public		OnTargetEvent			OnTargetAquired			{ set { m_OnTargetAquired = value; } }
	public		OnTargetEvent			OnTargetChanged			{ set { m_OnTargetChanged = value; } }
	public		OnTargetEvent			OnTargetLost			{ set { m_OnTargetLost	  = value; } }
	public		OnTargetsAcquired		OnTargetsAcquired		{ set { m_OnTargetsAcquired = value; } }


	[SerializeField]
	private		Transform				m_ViewPoint				= null;

	[SerializeField, Range( 1f, 200f )]
	private		float					m_ViewDistance			= 10f;

	[SerializeField,Range( 1f, 150f)]
	private		float					m_ViewCone				= 100f;

	[SerializeField]
	private		EEntityType				m_EntityType			= EEntityType.NONE;
	private		EEntityType				m_PrevEntityType		= EEntityType.NONE;

	[SerializeField]
	private EAcquisitionStrategy		m_strategy				= EAcquisitionStrategy.CLOSEST;

//	[SerializeField]
//	private		LayerMask				m_LayerMask				 = default;


				float					IFieldOfView.Distance	{ get => m_ViewDistance; set => m_ViewDistance = value; }
				float					IFieldOfView.Angle		{ get => m_ViewCone; set => m_ViewCone = value; }
				EEntityType				IFieldOfView.TargetType	{ get => m_EntityType; set => m_EntityType = value; }

	private		OnTargetEvent			m_OnTargetAquired		= null;
	private		OnTargetEvent			m_OnTargetChanged		= null;
	private		OnTargetEvent			m_OnTargetLost			= null;
	private		OnTargetsAcquired		m_OnTargetsAcquired		= null;

	private		SphereCollider			m_ViewTriggerCollider	= null;
	private		List<Entity>			m_AllTargets			= new List<Entity>();
	private		Entity					m_Owner					= null;

	private		TargetInfo				m_CurrentTargetInfo		= new TargetInfo();

	private void	Awake()
	{
		m_ViewTriggerCollider = gameObject.GetOrAddIfNotFound<SphereCollider>();
		m_ViewTriggerCollider.isTrigger = true;
		m_ViewTriggerCollider.radius = m_ViewDistance;

//		m_LayerMask = Utils.LayersHelper.Layers_AllButOne("Shield");
	}

	private void	OnValidate()
	{
		m_ViewTriggerCollider = gameObject.GetOrAddIfNotFound<SphereCollider>();
		m_ViewTriggerCollider.isTrigger = true;
		m_ViewTriggerCollider.radius = m_ViewDistance;
	}

	/// <summary>  </summary>
	public	void	Setup()
	{
		UnityEngine.Assertions.Assert.IsNotNull(m_ViewTriggerCollider, "Collider required in this context");
	
		UnityEngine.Assertions.Assert.IsTrue(Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL_AND_PARENTS, out m_Owner));

		if (transform.parent && transform.parent.TrySearchComponents(ESearchContext.LOCAL_AND_CHILDREN, out Collider[] colliders))
		{
			System.Array.ForEach(colliders, c => Physics.IgnoreCollision(m_ViewTriggerCollider, c));
		}
	}


	/// <summary>  </summary>
	public void		SetViewPoint(in Transform viewPoint)
	{
		m_ViewPoint = viewPoint ?? m_ViewPoint;
	}

	/// <summary>  </summary>
	public	void	UpdateFOV()
	{
		HandleEntityTargetTypeChange();

		UnityEngine.Assertions.Assert.IsNotNull(m_ViewPoint);

		Vector3 viewPointPosition = m_ViewPoint.position;
		Vector3 viewPointDirection = m_ViewPoint.forward;

		bool IsValidEntity(in Entity target)
		{
			return target.IsAlive;
		}

		bool InsideViewCone(in Entity target)
		{
			Vector3 targettablePosition = target.Targettable.position;
			Vector3 direction = (targettablePosition - viewPointPosition);
			float angle = Vector3.Angle(direction, viewPointDirection);
			return (angle <= (m_ViewCone * 0.5f));
		}

		bool IsTargettable(in Entity target)
		{
			Vector3 direction = (target.Targettable.position - viewPointPosition);
			if (Physics.Raycast(viewPointPosition, direction, out RaycastHit m_RaycastHit, m_ViewDistance/*, 0, QueryTriggerInteraction.Ignore*/))
			{
				int colliderInstanceID = m_RaycastHit.collider.GetInstanceID();
				int entityPhysicColliderInstanceID = target.AsInterface.PhysicCollider.GetInstanceID();
				int shieldColliderInstanceID = target.AsInterface.Shield?.Collider.GetInstanceID() ?? -1;
				return (colliderInstanceID == entityPhysicColliderInstanceID || colliderInstanceID == shieldColliderInstanceID);
			}
			return false;
		}

		// If target is valid and visible select by strategy
		if (TrySelectTargetByStrategy(m_AllTargets.Where( e => IsValidEntity(e) && InsideViewCone(e) && IsTargettable(e)), m_strategy, out IEntity choosenTarget))
		{
			IEntity previousTarget = m_CurrentTargetInfo.CurrentTarget;
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
				if (previousTarget.ID != choosenTarget.ID)
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

	/// <summary> Return true if and entity has been choosen, otherwise false. Return false with empty list. </summary>
	private bool TrySelectTargetByStrategy(in IEnumerable<Entity> availableTargets, in EAcquisitionStrategy strategy, out IEntity entity)
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

	private static IEntity GetTargetByDistance(in IEnumerable<Entity> availableTargets, in EAcquisitionStrategy strategy, Vector3 point)
	{
		float Selector(Entity e) => (e.transform.position - point).sqrMagnitude;
		return strategy == EAcquisitionStrategy.CLOSEST ? availableTargets.MinBy(Selector) : availableTargets.MaxBy(Selector);
	}

	private static IEntity GetTargetByHealth(in IEnumerable<Entity> availableTargets, in EAcquisitionStrategy strategy)
	{
		return strategy == EAcquisitionStrategy.WEAKER ? availableTargets.MinBy(e => e.AsInterface.Health) : availableTargets.MaxBy(e => e.AsInterface.Health);
	}

	private static IEntity GetSameTrargetOrRandom(in IEnumerable<Entity> availableTargets, in EAcquisitionStrategy strategy, in TargetInfo m_CurrentTargetInfo)
	{
		return m_CurrentTargetInfo.CurrentTarget ?? availableTargets.Random();
	}

	/// <summary>  </summary>
	private void HandleEntityTargetTypeChange()
	{
		if (m_PrevEntityType != m_EntityType)
		{
			m_PrevEntityType = m_EntityType;

			m_AllTargets.Clear();

			void addToTargets(Collider c)
			{
				if (c.transform.TrySearchComponent(ESearchContext.LOCAL_AND_PARENTS, out Entity entity, e => e.AsInterface.EntityType == m_EntityType) && entity != m_Owner)
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

	public	void	OnReset()
	{
		m_CurrentTargetInfo.Reset();

		m_AllTargets.Clear();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.TrySearchComponent( ESearchContext.LOCAL_AND_PARENTS, out Entity entity) && entity.IsAlive && entity.AsInterface.EntityType == m_EntityType && !m_AllTargets.Contains(entity))
		{
			m_AllTargets.Add(entity);

			entity.OnEvent_Killed += ( Entity entityKilled ) =>
			{
				m_AllTargets.Remove(entityKilled);
			};
		}
	}

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