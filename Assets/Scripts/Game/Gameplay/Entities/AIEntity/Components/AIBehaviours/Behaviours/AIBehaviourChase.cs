
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	using Senses;

	public class AIBehaviourChase : AIBehaviourBase
	{
		[SerializeReference, ReadOnly]
		private				Entity											m_Target										= null;

		public override		string											Description										=> "Chase";


		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();

			if (Utils.CustomAssertions.IsTrue(Blackboard.TryGetEntry(AIBehaviourBlackboard.Keys.kCurrentLastEntityTarget, out AIBehaviourBlackboardEntryKeyValue<Entity> entry)))
			{
				Controller.RequestMoveTo(m_Target = entry.Value);
			}

			if (Utils.CustomAssertions.IsNotNull(GameManager.CyclesEvents))
			{
				GameManager.CyclesEvents.OnThink += OnThink;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			if (GameManager.CyclesEvents.IsNotNull())
			{
				GameManager.CyclesEvents.OnThink -= OnThink;
			}

			base.OnDisable();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnSightEvent(in SightEvent InEvent)
		{
			
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnThink(float InDeltaTime)
		{
			if (Controller.IsCloseEnoughTo(m_Target))
			{
				Controller.BehavioursManager.TryTransitionTo<AIBehaviourIdle>();
			}
		}
	}
}

