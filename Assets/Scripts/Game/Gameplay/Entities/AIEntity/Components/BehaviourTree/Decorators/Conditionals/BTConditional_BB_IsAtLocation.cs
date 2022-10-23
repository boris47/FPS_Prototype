using UnityEngine;


namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Is At or Close To Position", "Condition for closness at target entity or target position")]
	public class BTConditional_BB_IsAtLocation : BTConditional
	{
		public const float kMinRadius = 0.01f;

		[SerializeReference, ToNodeInspector, BlackboardKeyType(typeof(BBEntry_Position), typeof(BBEntry_Entity))]
		private				BlackboardEntryKey								m_BlackboardKey									= null;

		[SerializeField, Min(kMinRadius), ToNodeInspector]
		private				float											m_AcceptableRadius								= kMinRadius;

		//////////////////////////////////////////////////////////////////////////
		protected override void StartObserve(in BTNodeInstanceData InThisNodeInstanceData)
		{
			InThisNodeInstanceData.BehaviourTreeInstanceData.AddTickable(InThisNodeInstanceData, OnTick);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void StopObserve(in BTNodeInstanceData InThisNodeInstanceData)
		{
			InThisNodeInstanceData.BehaviourTreeInstanceData.RemoveTickable(InThisNodeInstanceData);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnTick(BTNodeInstanceData InThisNodeInstanceData, float InDeltaTime)
		{
			base.OnTick(InThisNodeInstanceData, InDeltaTime);

		//	if (GetEvaluation(InThisNodeInstanceData))
		//	{
		//
		//	}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override bool GetEvaluation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			bool outValue = false;
			if (m_BlackboardKey.IsNotNull() && InThisNodeInstanceData.TryGetEntry(m_BlackboardKey, out BBEntry_Position OutPosition))
			{
				Vector3 currentPosition = InThisNodeInstanceData.Controller.Entity.Body.position;
				Vector3 targetPosition = OutPosition.Value;
				outValue = Vector3.SqrMagnitude(currentPosition - targetPosition) < m_AcceptableRadius * m_AcceptableRadius;
			}
			if (m_BlackboardKey.IsNotNull() && InThisNodeInstanceData.TryGetEntry(m_BlackboardKey, out BBEntry_Entity OutEntity))
			{
				Vector3 currentPosition = InThisNodeInstanceData.Controller.Entity.Body.position;
				Vector3 targetPosition = OutEntity.Value.Body.position;
				outValue = Vector3.SqrMagnitude(currentPosition - targetPosition) < m_AcceptableRadius * m_AcceptableRadius;
			}
			return outValue;
		}
	}
}
