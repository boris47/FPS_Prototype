﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("MoveTo Action", "Request owner to move close to specified position or entity")]
	public class BTTask_MoveCloseTo : BTTaskNode
	{
		private const	float								s_MinRadius				= 0.001f;

		[SerializeField, Min(s_MinRadius), ToNodeInspector(bShowLabel: true)]
		public			float								m_AcceptableRadius		= 1f;

		[SerializeField, ToNodeInspector("Position or Entity To Evaluate")]
		private			BlackboardEntryKey					m_BlackboardKey			= null;

		[SerializeField, ToNodeInspector("Dimension")]
		private			ESpace								m_Space					= ESpace.ThreeDimensional;



		//////////////////////////////////////////////////////////////////////////
		protected override void OnAwakeInternal(in BehaviourTree InBehaviourTree)
		{
			base.OnAwakeInternal(InBehaviourTree);

			Utils.CustomAssertions.IsNotNull(m_BlackboardKey);
			Utils.CustomAssertions.IsTrue(m_AcceptableRadius >= s_MinRadius);
		}

		//////////////////////////////////////////////////////////////////////////
		private bool TryGetPositionToReach(out Vector3 OutPosition)
		{
			Vector3? position = null;
			if (BehaviourTree.Owner.Blackboard.TryGetEntry(m_BlackboardKey, out BBEntry_PositionToReach BB_Position))
			{
				position = BB_Position.Value;
			}
			else if (BehaviourTree.Owner.Blackboard.TryGetEntry(m_BlackboardKey, out BBEntry_EntityToEvaluate BB_TargetSeen))
			{
				position = BB_TargetSeen.Value.Body.position;
			}
			OutPosition = position.GetValueOrDefault();
			return position.HasValue;
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
				if (TryGetPositionToReach(out Vector3 positionToReach))
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
			if (TryGetPositionToReach(out Vector3 positionToReach))
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
