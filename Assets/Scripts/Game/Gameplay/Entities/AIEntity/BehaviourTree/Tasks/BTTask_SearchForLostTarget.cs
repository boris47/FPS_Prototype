using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Search For Action", "Request owner to search for lost target")]
	public class BTTask_SearchForLostTarget : BTTaskNode
	{
		private const	float								s_MinRadius				= 0.001f;

		[SerializeField, Min(s_MinRadius), ToNodeInspector(bShowLabel: true)]
		public			float								m_AcceptableRadius		= 1f;

		[SerializeField, ToNodeInspector("Entity To Evaluate")]
		private			MemoryIdentifier					m_MemoryIdentifier		= null;

		[SerializeField, ToNodeInspector("Dimension")]
		private			ESpace								m_Space					= ESpace.ThreeDimensional;



		//////////////////////////////////////////////////////////////////////////
		protected override void OnAwakeInternal(in BehaviourTree InBehaviourTree)
		{
			base.OnAwakeInternal(InBehaviourTree);

			Utils.CustomAssertions.IsNotNull(m_MemoryIdentifier);
			Utils.CustomAssertions.IsTrue(m_AcceptableRadius >= s_MinRadius);
		}

		//////////////////////////////////////////////////////////////////////////
		private bool TryGetPositionAndDirection(out Vector3 OutPosition, out Vector3 OutDirection)
		{
			Vector3? position = null;
			Vector3? direction = null;
			if (BehaviourTree.Owner.BrainComponent.MemoryComponent.TryGetMemory(m_MemoryIdentifier, out MemoryValue<Ray> OutMemory) && OutMemory.Value.IsNotNull())
			{
				position = OutMemory.Value.origin;
				direction = OutMemory.Value.direction;
			}
			OutPosition = position.GetValueOrDefault();
			OutDirection = direction.GetValueOrDefault();
			return position.HasValue && direction.HasValue;
		}

		//////////////////////////////////////////////////////////////////////////
		private bool StillDistantFrom(in Vector3 InPosition)
		{
			bool bEvaluationResult = false;
			float squaredRange = m_AcceptableRadius * m_AcceptableRadius;
			switch (m_Space)
			{
				case ESpace.BiDimensional:
				{
					bEvaluationResult = BehaviourTree.Owner.transform.position.DistanceXZSqr(InPosition) > squaredRange;
					break;
				}
				case ESpace.ThreeDimensional:
				default:
				{
					bEvaluationResult = BehaviourTree.Owner.transform.position.DistanceSqr(InPosition) > squaredRange;
					break;
				}
			}
			return bEvaluationResult;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnActivation()
		{
			EBTNodeState OutState = base.OnActivation();
			if (OutState == EBTNodeState.RUNNING)
			{
				if (TryGetPositionAndDirection(out Vector3 positionToReach, out Vector3 direction))
				{
					if (StillDistantFrom(positionToReach))
					{
						if (!BehaviourTree.Owner.Entity.AIMotionManager.RequireMovementTo(positionToReach))
						{
							OutState = EBTNodeState.FAILED;

							BehaviourTree.Owner.Entity.AIMotionManager.Stop(bImmediately: true);
						}
					}
					else
					{
						OutState = EBTNodeState.SUCCEEDED;

						BehaviourTree.Owner.Entity.AIMotionManager.Stop(bImmediately: false);
					}
				}
				else
				{
					OutState = EBTNodeState.FAILED;

					BehaviourTree.Owner.Entity.AIMotionManager.Stop(bImmediately: true);
				}
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate()
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;

			if (TryGetPositionAndDirection(out Vector3 positionToReach, out Vector3 direction))
			{
				if (StillDistantFrom(positionToReach))
				{
					if (!BehaviourTree.Owner.Entity.AIMotionManager.RequireMovementTo(positionToReach))
					{
						OutState = EBTNodeState.FAILED;

						BehaviourTree.Owner.Entity.AIMotionManager.Stop(bImmediately: false);
					}
				}
				else
				{
					OutState = EBTNodeState.SUCCEEDED;

					BehaviourTree.Owner.Entity.AIMotionManager.Stop(bImmediately: false);

					BehaviourTree.Owner.BrainComponent.MemoryComponent.RemoveMemory(m_MemoryIdentifier);
				}
			}
			else
			{
				// key has been removed
				OutState = EBTNodeState.FAILED;

				// handle on owner side
				BehaviourTree.Owner.Entity.AIMotionManager.Stop(bImmediately: false);
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnTerminate()
		{
			
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnAbortNodeRequested(in bool bAbortImmediately)
		{
			// handle on owner side
			BehaviourTree.Owner.Entity.AIMotionManager.Stop(bAbortImmediately);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdateAborting()
		{
			return EBTNodeState.ABORTED;
		}
	}
}
