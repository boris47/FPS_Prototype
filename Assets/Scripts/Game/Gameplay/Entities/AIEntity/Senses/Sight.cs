
using System.Collections.Generic;
using UnityEngine;

// TODO Proper handling of ontargetlost (target is destroyed or disabled)
namespace Entities.AI.Components.Senses
{
	using System.Linq;

	[System.Serializable]
	public class SightEvent : SenseEvent
	{
		public override ESenses			SenseType				=> ESenses.SIGHT;
		public readonly ETargetInfoType TargetInfoType			= default;
		public readonly Vector3			SeenPosition			= Vector3.zero;
		public readonly Vector3			LastDirection			= Vector3.zero;
		public readonly Vector3			ViewerPosition			= Vector3.zero;
		public readonly Entity			EntitySeen				= null;

		public static SightEvent TargetAcquiredEvent(in Entity entitySeen, in Vector3 seenPosition, in Vector3 viewerPosition)
			=> new SightEvent(ETargetInfoType.ACQUIRED, entitySeen, seenPosition, viewerPosition);
		public static SightEvent TargetChangedEvent	(in Entity entitySeen, in Vector3 seenPosition, in Vector3 viewerPosition)
			=> new SightEvent(ETargetInfoType.CHANGED, entitySeen, seenPosition, viewerPosition);
		public static SightEvent TargetLostEvent	(in Entity lostTarget, in Vector3 lastSeenPosition, in Vector3 lastDirection, in Vector3 viewerPosition)
			=> new SightEvent(ETargetInfoType.LOST, lostTarget, lastSeenPosition, lastDirection, viewerPosition);

		private SightEvent(in ETargetInfoType targetInfoType, in Entity entitySeen, in Vector3 seenPosition, in Vector3 viewerPosition)
		{
			EntitySeen = entitySeen;
			TargetInfoType = targetInfoType;
			SeenPosition = seenPosition;
			ViewerPosition = viewerPosition;
			Utils.CustomAssertions.IsTrue(targetInfoType != ETargetInfoType.LOST && EntitySeen.IsNotNull());
		}

		private SightEvent(in ETargetInfoType targetInfoType, in Entity lostTarget, in Vector3 seenPosition, in Vector3 lastDirection, in Vector3 viewerPosition)
		{
			EntitySeen = lostTarget;
			TargetInfoType = targetInfoType;
			SeenPosition = seenPosition;
			ViewerPosition = viewerPosition;
			LastDirection = lastDirection;
		}

		public (Entity EntitySeen, Vector3 SeenPosition, Vector3 ViewerPosition) AsTargetAcquiredEvent() => (EntitySeen, SeenPosition, ViewerPosition);
		public (Entity EntitySeen, Vector3 SeenPosition, Vector3 ViewerPosition) AsTargetChangedEvent() => (EntitySeen, SeenPosition, ViewerPosition);
		public (Entity LostTarget, Vector3 SeenPosition, Vector3 LastDirection, Vector3 ViewerPosition) AsTargetLostEvent() => (EntitySeen, SeenPosition, LastDirection, ViewerPosition);
	}

	public enum ETargetInfoType
	{
		ACQUIRED, CHANGED, LOST
	}

	/// <summary> Handle the field of view of the entity </summary>
	internal class Sight : Sense
	{
		[SerializeField, Range(1f, 200f)]
		protected				float						m_ViewDistance					= 10f;
		[SerializeField, Range(1f, 150f)]
		protected				float						m_ViewCone						= 100f;


		private					SphereCollider				m_ViewTriggerCollider			= null;
		private					List<Entity>				m_AllTargets					= new List<Entity>();
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
		protected override void OnEnableInternal()
		{
			m_BrainComponent.Targets.OnRelationChanged += OnRelationChanged;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisableInternal()
		{
			m_BrainComponent.Targets.OnRelationChanged -= OnRelationChanged;
		}


		//////////////////////////////////////////////////////////////////////////
		private void OnRelationChanged()
		{
			// Make a sphere cast, select entities (different from owner), remove null entries
			m_AllTargets = Physics.OverlapSphere(transform.position, m_ViewTriggerCollider.radius, /*layerMask*/1, QueryTriggerInteraction.Ignore)
				.Select(c => (c.transform.TrySearchComponent(ESearchContext.LOCAL_AND_PARENTS, out Entity entity, e => m_BrainComponent.Targets.IsInterestedAt(e)) && entity != m_Owner) ? entity : null)
				.Select(e => e).ToList();
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnTargetAcquisitionStrategyChanged(ETargetAcquisitionStrategy newStrategy)
		{

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
				return currentTarget ?? availableTargets.Random();
			}

			OutEntity = null;
			if (InEligibleTargets.Any())
			{
				ETargetAcquisitionStrategy strategy = m_BrainComponent.Targets.TargetAcquisitionStrategy;
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
						if (m_BrainComponent.Targets.IsInterestedAt(entity))
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

			return m_AllTargets.Where(Selector).ToList();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnThink()
		{
			// UpdateEligibleTargets
			var eligibleTargets = FindEligibleTargets();

			// If target is valid and visible select by strategy
			if (TrySelectTargetByStrategy(eligibleTargets, out Entity choosenTarget))
			{
				Vector3 seenPosition = choosenTarget.Targettable.position;
				Vector3 viewerPosition = transform.position;
				Vector3 lastDirection = choosenTarget.Body.forward;

				// SET NEW TARGET
				if (m_CurrentTarget == null)
				{
					m_PerceptionComponent.Senses.OnSenseEvent(SightEvent.TargetAcquiredEvent(choosenTarget, seenPosition, viewerPosition));

					// Notify team if available
					m_PerceptionComponent.Senses.GetSense<Team>()?.Notify(choosenTarget, seenPosition, lastDirection);
				}
				else
				// CHANGING A TARGET
				{
					if (m_CurrentTarget.IsNotNull() && m_CurrentTarget.Id != choosenTarget.Id)
					{
						m_PerceptionComponent.Senses.OnSenseEvent(SightEvent.TargetChangedEvent(choosenTarget, seenPosition, viewerPosition));

						// Notify team if available
						m_PerceptionComponent.Senses.GetSense<Team>()?.Notify(choosenTarget, seenPosition, lastDirection);
					}
				}
				m_CurrentTarget = choosenTarget;
			}
			else // when no target can be found (EX: m_AllTargets is empty)
			{
				// TARGET LOST
				if (m_CurrentTarget.IsNotNull())
				{
					Vector3 lastSeenPosition = m_CurrentTarget.Targettable.position;
					Vector3 viewerPosition = transform.position;
					Vector3 lastDirection = m_CurrentTarget.Body.forward;

					m_PerceptionComponent.Senses.OnSenseEvent(SightEvent.TargetLostEvent(m_CurrentTarget, lastSeenPosition, lastDirection, viewerPosition));

					// Notify team if available
					m_PerceptionComponent.Senses.GetSense<Team>()?.Notify(null, lastSeenPosition, lastDirection);
					m_CurrentTarget = null;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnPhysicFrame(float fixedDeltaTime)
		{

		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnFrame(float deltaTime)
		{

		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnLateFrame(float deltaTime)
		{

		}

		//////////////////////////////////////////////////////////////////////////
		private void OnTriggerEnter(Collider other)
		{
			if (other.transform.TrySearchComponent(ESearchContext.LOCAL_AND_PARENTS, out Entity e) && e.IsAlive && m_BrainComponent.Targets.IsInterestedAt(e))
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
			Vector3 direction = (target.Targettable.position - viewPointPosition);
			float angle = Vector3.Angle(direction, viewPointDirection);
			return (angle <= (viewCone * 0.5f));
		}

		//////////////////////////////////////////////////////////////////////////
		private static bool IsTargettable(in Entity target, in Vector3 viewPointPosition, in float viewDistance)
		{
			bool bResult = false;
			if (Physics.Raycast(viewPointPosition, (target.Targettable.position - viewPointPosition), out RaycastHit m_RaycastHit, viewDistance))
			{
				int colliderInstanceID = m_RaycastHit.collider.GetInstanceID();
				int entityPhysicColliderInstanceID = target.PhysicCollider.GetInstanceID();
				int shieldColliderInstanceID = target.EntityShield?.Collider.GetInstanceID() ?? -1;
				bResult = (colliderInstanceID == entityPhysicColliderInstanceID || colliderInstanceID == shieldColliderInstanceID);
			}
			return bResult;
		}
	}
}
