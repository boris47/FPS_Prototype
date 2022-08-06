
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components
{
	public class AIEntityDroneMovementComponent : AIEntityMovementComponent
	{
		[SerializeField]
		protected List<Collider> m_StoredCollisionPositions = new List<Collider>();

		//	private Vector3 m_AveragePosition = Vector3.zero;

		private Entity m_Target = null;

		public float speed = 0.01f;

		//////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			if (Utils.CustomAssertions.IsNotNull(GameManager.CyclesEvents))
			{
				GameManager.CyclesEvents.OnPhysicFrame += OnPhysicFrame;
			//	GameManager.CyclesEvents.OnThink += OnThink;
			}

			base.OnEnable();

			m_Controller.PerceptionComponent.Senses.OnNewSenseEvent += Senses_OnNewSenseEvent;
		}

		private void Senses_OnNewSenseEvent(in Senses.SenseEvent newSenseEvent)
		{
			if (newSenseEvent.SenseType == ESenses.SIGHT)
			{
				var eventt = newSenseEvent as Senses.SightEvent;
				if (eventt.TargetInfoType == Senses.ETargetInfoType.ACQUIRED || eventt.TargetInfoType == Senses.ETargetInfoType.CHANGED)
				{
					m_Target = eventt.EntitySeen;
					bHasPath = global::AI.Pathfinding.AStarSearch.FindPath(transform.position, lastTargetPosition = m_Target.Targettable.position, out m_Path);
				}
				else
				{
					bHasPath = global::AI.Pathfinding.AStarSearch.FindPath(transform.position, lastTargetPosition = m_Target.Targettable.position, out m_Path);
					m_Target = null;
				}
			}
		}

		[SerializeField]
		private Vector3[] m_Path = null;
		private uint m_Index = 0u;
		private bool bHasPath = false;
		private Vector3 lastTargetPosition = Vector3.zero;

		private void OnDrawGizmos()
		{
			if (m_Path.IsNotNull())
			{
				for (int i = 1, Length = m_Path.Length; i < Length; i++)
				{
					Gizmos.DrawLine(m_Path[i - 1], m_Path[i]);
				}
			}
		}

		/*
		private void OnThink()
		{
			if (m_Target.IsNotNull())
			{
				bHasPath = global::AI.Pathfinding.AStarSearch.Instance.FindPath(transform.position, m_Target.Targettable.position, out m_Path);
			}
			else
			{
				bHasPath = false;
				m_Path = null;
				m_Owner.Rigidbody.velocity = Vector3.zero;
			}
		}
		*/
		//////////////////////////////////////////////////////////////////
		private void OnPhysicFrame(float InFixedDeltaTime)
		{
			/*
			Vector3 averagePosition = Vector3.zero;

			// Temp
			m_TargetPosition = m_Owner.Rigidbody.position + Vector3.forward;
			Debug.Log("Remove this as soon the feature is completed");
			// Temp

			if (m_StoredCollisionPositions.Any())
			{
				// Calculate the average position
				m_AveragePosition = averagePosition = m_StoredCollisionPositions.Aggregate
				(
					m_StoredCollisionPositions.First().ClosestPoint(m_Owner.Body.position),
					delegate(Vector3 current, Collider other)
					{
						return current += other.ClosestPoint(m_Owner.Body.position);
					}
				) / (float)m_StoredCollisionPositions.Count;
			}

			// Temp, Evolve this !
			Vector3 direction = (m_TargetPosition - m_Owner.Rigidbody.position).normalized;
			Vector3 force = direction * speed;

			m_Owner.Rigidbody.AddForce(force, ForceMode.VelocityChange);
			// Temp

			*/


			if (bHasPath)
			{
				if (m_Owner.Rigidbody.position.Distance(m_Path[m_Index])<0.2f)
				{
					if (m_Path.IsValidIndex(m_Index + 1u))
					{
						++m_Index;
					}
				}

				if (m_Target)
				{
					if (lastTargetPosition.Distance(m_Target.Targettable.position) > 0.2f)
					{
						bHasPath = global::AI.Pathfinding.AStarSearch.FindPath(transform.position, lastTargetPosition = m_Target.Targettable.position, out m_Path);
						m_Index = 0u;
					}
					m_Controller.PerceptionComponent.Senses.GetSense<Senses.Sight>().transform.LookAt(m_Target.Targettable);
				}

				Vector3 direction = (m_Path[m_Index] - m_Owner.Rigidbody.position).normalized;
				Vector3 force = direction * speed;
				m_Owner.Rigidbody.velocity = force;
			}
			
		}

		//////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();

			if (GameManager.CyclesEvents.IsNotNull())
			{
				GameManager.CyclesEvents.OnPhysicFrame -= OnPhysicFrame;
			//	GameManager.CyclesEvents.OnThink -= OnThink;
			}
		}
		/*
		//////////////////////////////////////////////////////////////////
		protected override void OnTriggerEnterEv(in Collider other)
		{
			base.OnTriggerEnterEv(other);

			if (Utils.CustomAssertions.IsTrue(!m_StoredCollisionPositions.Contains(other)))
			{
				m_StoredCollisionPositions.Add(other);
			}
		}

		//////////////////////////////////////////////////////////////////
		protected override void OnTriggerExitEv(in Collider other)
		{
			base.OnTriggerExitEv(other);

			if (Utils.CustomAssertions.IsTrue(m_StoredCollisionPositions.Contains(other)))
			{
				m_StoredCollisionPositions.Remove(other);
			}
		}
		*/
	//	private void OnDrawGizmos()
	//	{
	//		Gizmos.DrawSphere(m_AveragePosition, 1.3f);
	//	}
	}
}
