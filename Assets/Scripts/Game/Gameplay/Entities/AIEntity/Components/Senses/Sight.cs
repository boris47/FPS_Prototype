
using System.Collections.Generic;
using UnityEngine;

// TODO Proper handling of ontargetlost (target is destroyed or disabled)
namespace Entities.AI.Components.Senses
{
	using System.Linq;

	public enum ESightTargetEventType
	{
		NONE, ACQUIRED, CHANGED, LOST, COUNT
	}

	/// <summary> Handle the field of view of the entity </summary>
	public class Sight : Sense
	{
		private const uint kMaxEntities = 20u;

		[SerializeField, Range(1f, 200f)]
		protected				float						m_ViewDistance					= 10f;
		[SerializeField, Range(1f, 150f)]
		protected				float						m_ViewCone						= 100f;


		private					SphereCollider				m_ViewTriggerCollider			= null;
		private readonly		List<Entity>				m_AllTargets					= new List<Entity>();
		private					Entity						m_CurrentTarget					= null;

		//////////////////////////////////////////////////////////////////////////
		protected override void SetupInternal()
		{
			m_ViewTriggerCollider = gameObject.GetOrAddIfNotFound<SphereCollider>();
			m_ViewTriggerCollider.isTrigger = true;

			// Get sight data from entity Config
			m_ViewTriggerCollider.radius = m_ViewDistance;
			// m_ViewCone
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnValidate()
		{
			SetupInternal();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnableInternal()
		{
			m_BrainComponent.OnRelationChanged += OnRelationChanged;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisableInternal()
		{
			m_BrainComponent.OnRelationChanged -= OnRelationChanged;
		}


		//////////////////////////////////////////////////////////////////////////
		private void OnRelationChanged()
		{
			m_AllTargets.Clear();

			Collider[] colliders = new Collider[kMaxEntities];
			for (int i = 0; i < Physics.OverlapSphereNonAlloc(transform.position, m_ViewTriggerCollider.radius, colliders, 1, QueryTriggerInteraction.Ignore); i++)
			{
				if (colliders[i].transform.TrySearchComponent(ESearchContext.LOCAL_AND_PARENTS, out Entity entity, e => e.IsNotNull() && e != m_Owner && m_BrainComponent.IsInterestedAt(e)))
				{
					m_AllTargets.Add(entity);
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private bool TrySelectTargetByStrategy(in IReadOnlyCollection<Entity> InEligibleTargets, out Entity OutEntity)
		{
			//////////////////////////////////////////////////////////////////////////
			static Entity GetTargetByDistance(in IReadOnlyCollection<Entity> availableTargets, in bool bClosest, Vector3 point)
			{
				float Selector(Entity e) => (e.transform.position - point).sqrMagnitude;
				return bClosest ? availableTargets.MinBy(Selector) : availableTargets.MaxBy(Selector);
			}

			//////////////////////////////////////////////////////////////////////////
			static Entity GetTargetByHealth(in IReadOnlyCollection<Entity> availableTargets, in bool bWeaker)
			{
				return bWeaker ? availableTargets.MinBy(e => e.Health) : availableTargets.MaxBy(e => e.Health);
			}

			//////////////////////////////////////////////////////////////////////////
			static Entity GetSameTrargetOrRandom(in IReadOnlyCollection<Entity> availableTargets, in Entity currentTarget)
			{
				return currentTarget.IsNotNull() ? currentTarget : availableTargets.Random();
			}

			OutEntity = null;
			if (InEligibleTargets.Any())
			{
				ETargetAcquisitionStrategy strategy = m_BrainComponent.TargetAcquisitionStrategy;
				switch (strategy)
				{
					case ETargetAcquisitionStrategy.CLOSEST:
					case ETargetAcquisitionStrategy.FAREST:
					{
						Vector3 viewPointOrigin = transform.position;
						bool bClosest = strategy == ETargetAcquisitionStrategy.CLOSEST;
						OutEntity = GetTargetByDistance(InEligibleTargets, bClosest, viewPointOrigin);
						break;
					}
					case ETargetAcquisitionStrategy.WEAKER:
					case ETargetAcquisitionStrategy.HARDER:
					{
						bool bWeaker = strategy == ETargetAcquisitionStrategy.WEAKER;
						OutEntity = GetTargetByHealth(InEligibleTargets, bWeaker);
						break;
					}
					case ETargetAcquisitionStrategy.TILL_ELIMINATION:
					{
						OutEntity = GetSameTrargetOrRandom(InEligibleTargets, m_CurrentTarget);
						break;
					}
				}
			}
			return OutEntity.IsNotNull();
		}

		//////////////////////////////////////////////////////////////////////////
		private IReadOnlyList<Entity> FindEligibleTargets()
		{
			bool Selector(Entity entity)
			{
				Vector3 eyesPosition = transform.position;
				Vector3 eyesDirection = transform.forward;

				// Is a valid entity?
				if (entity && entity.IsAlive)
				{
					// Is inside cone of view
					if (InsideViewCone(entity, eyesPosition, eyesDirection, m_ViewCone))
					{
						// Is an hostile entity to this one?
						if (m_BrainComponent.IsInterestedAt(entity))
						{
							// Is really visible
							if (IsTargettable(entity, eyesPosition, m_ViewDistance))
							{
								return true;
							}
						}
					}
				}
				return false;
			};

			return m_AllTargets.Where(Selector).ToArray();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnThink(float deltaTime)
		{
			// UpdateEligibleTargets
			IReadOnlyList<Entity> eligibleTargets = FindEligibleTargets();

			// If target is valid and visible select by strategy
			if (TrySelectTargetByStrategy(eligibleTargets, out Entity choosenTarget))
			{
				Vector3 seenPosition = choosenTarget.Targetable.position;
				Vector3 viewerPosition = transform.position;
				Vector3 targetVelocity = choosenTarget.GetVelocity();

				// SET NEW TARGET
				if (m_CurrentTarget == null)
				{
					m_PerceptionComponent.SendSenseEvent(SightEvent.NewTargetAcquiredEvent(choosenTarget, seenPosition, targetVelocity, viewerPosition));
				}
				else
				// CHANGING A TARGET
				{
					if (m_CurrentTarget.IsNotNull() && m_CurrentTarget.Id != choosenTarget.Id)
					{
						m_PerceptionComponent.SendSenseEvent(SightEvent.NewTargetChangedEvent(choosenTarget, seenPosition, targetVelocity, viewerPosition));
					}
				}
				m_CurrentTarget = choosenTarget;
			}
			else // when no target can be found (EX: m_AllTargets is empty)
			{
				// TARGET LOST
				if (m_CurrentTarget.IsNotNull())
				{
					Vector3 lastSeenPosition = m_CurrentTarget.Targetable.position;
					Vector3 viewerPosition = transform.position;
					Vector3 lastVelocity = m_CurrentTarget.GetVelocity();

					m_PerceptionComponent.SendSenseEvent(SightEvent.NewTargetLostEvent(m_CurrentTarget, lastSeenPosition, lastVelocity, viewerPosition));
					m_CurrentTarget = null;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnTriggerEnter(Collider other)
		{
			if (other.transform.TrySearchComponent(ESearchContext.LOCAL_AND_PARENTS, out Entity e) && e.IsAlive && m_BrainComponent.IsInterestedAt(e))
			{
				if (!m_AllTargets.Contains(e))
				{
					m_AllTargets.Add(e);
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnTriggerExit(Collider other)
		{
			if (other.transform.TrySearchComponent(ESearchContext.LOCAL_AND_PARENTS, out Entity entity))
			{
				m_AllTargets.Remove(entity);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnDrawGizmosSelected()
		{
			float halfFOV = m_ViewCone * 0.5f;

			Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.Euler(transform.rotation * (Vector3.one * 0.5f)), Vector3.one);

			Vector3 up = transform.up;
			Vector3 right = transform.right;
			Vector3 forward = transform.forward;

			for ( float i = 0; i < 180f; i += 10f )
			{
				float cos = Mathf.Cos(i * Mathf.Deg2Rad);
				float sin = Mathf.Sin(i * Mathf.Deg2Rad);

				Vector3 axisRight = (up  * cos) +  (right  * sin);
				Vector3 axisLeft  = (-up * cos) +  (-right * sin);

				// left
				Quaternion leftRayRotation		= Quaternion.AngleAxis(halfFOV, axisLeft);
				Vector3 leftRayDirection		= (leftRayRotation  * forward).normalized;
				Gizmos.DrawRay(Vector3.zero, leftRayDirection  * m_ViewDistance);

				// right
				Quaternion rightRayRotation		= Quaternion.AngleAxis(halfFOV, axisRight);
				Vector3 rightRayDirection		= (rightRayRotation * forward).normalized;
				Gizmos.DrawRay(Vector3.zero, rightRayDirection * m_ViewDistance);
			}
			Gizmos.matrix = Matrix4x4.identity;
		}


		//////////////////////////////////////////////////////////////////////////
		private static bool InsideViewCone(in Entity target, in Vector3 viewPointPosition, in Vector3 viewPointDirection, in float viewCone)
		{
			Vector3 direction = (target.Targetable.position - viewPointPosition);
			float angle = Vector3.Angle(direction, viewPointDirection);
			return (angle <= (viewCone * 0.5f));
		}

		//////////////////////////////////////////////////////////////////////////
		private static bool IsTargettable(in Entity target, in Vector3 viewPointPosition, in float viewDistance)
		{
			bool bResult = false;
			if (Physics.Raycast(viewPointPosition, (target.Targetable.position - viewPointPosition), out RaycastHit m_RaycastHit, viewDistance))
			{
				int colliderInstanceID = m_RaycastHit.collider.GetInstanceID();
				int entityPhysicColliderInstanceID = target.PrimaryCollider.GetInstanceID();
				int shieldColliderInstanceID = target.EntityShield.IsNotNull() ? target.EntityShield.Collider.GetInstanceID() : -1;
				bResult = (colliderInstanceID == entityPhysicColliderInstanceID || colliderInstanceID == shieldColliderInstanceID);
			}
			return bResult;
		}
	}
}
