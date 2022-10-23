using UnityEngine;

namespace Entities.AI
{
	using Components;
	using Components.Senses;

	/// <summary>
	/// AIController can be attached to a AIEntity to control its actions.
	/// AIControllers manage the artificial intelligence for the AIEntity they control.
	/// </summary>
	[RequireComponent(typeof(AIBrainComponent))]
	[RequireComponent(typeof(AIPerceptionComponent))]
	[RequireComponent(typeof(AIBehaviorTreeComponent))]
	public partial class AIController : EntityController
	{
		[SerializeField, ReadOnly]
		private				AIBrainComponent				m_BrainComponent					= null;
		[SerializeField, ReadOnly]
		private				AIPerceptionComponent			m_PerceptionComponent				= null;
		[SerializeField, ReadOnly]
		private				AIBehaviorTreeComponent			m_BehaviorTreeComponent				= null;

		[SerializeField]
		protected			bool							m_StartBrainOnPossess				= false;

		public				AIBrainComponent				BrainComponent						=> m_BrainComponent;
		public				AIPerceptionComponent			PerceptionComponent					=> m_PerceptionComponent;
		public				AIBehaviorTreeComponent			BehaviorTreeComponent				=> m_BehaviorTreeComponent;

		public				AIEntity						Entity								{ get; private set; } = null;



		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			if (m_BrainComponent.IsNotNull() || Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_BrainComponent)))
			{

			}

			if (m_PerceptionComponent.IsNotNull() || Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_PerceptionComponent)))
			{
				// TODO: Sense event handling should belong to entity brain instead!?
				m_PerceptionComponent.OnNewSenseEvent += HandleSenseEvent;
			}

			if (m_BehaviorTreeComponent.IsNotNull() || Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_BehaviorTreeComponent)))
			{

			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnValidate()
		{
			gameObject.TryGetComponent(out m_BrainComponent);
			gameObject.TryGetComponent(out m_PerceptionComponent);
			gameObject.TryGetComponent(out m_BehaviorTreeComponent);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnPossess(in Entity entity)
		{
			Entity = entity as AIEntity;
			if (m_StartBrainOnPossess)
			{

			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnUnPossess(in Entity entity)
		{
			Entity = null;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool RequestMoveTo(in Entity InTargetEntity)
		{
			return Entity.RequestMoveTo(InTargetEntity);
		}

		//////////////////////////////////////////////////////////////////////////
		public bool RequestMoveTo(in Vector3 InTargetPosition)
		{
			return Entity.RequestMoveTo(InTargetPosition);
		}

		//////////////////////////////////////////////////////////////////////////
		public bool IsCloseEnoughTo(in Entity InTargetEntity) => Entity.IsCloseEnoughTo(InTargetEntity);

		//////////////////////////////////////////////////////////////////////////
		public bool IsCloseEnoughTo(in Vector3 InTargetPosition) => Entity.IsCloseEnoughTo(InTargetPosition);

		//////////////////////////////////////////////////////////////////////////
		public void Stop(in bool bImmediately) => Entity.Stop(bImmediately);


		//////////////////////////////////////////////////////////////////////////
		private void HandleSenseEvent(in SenseEvent senseEvent)
		{
			static void HandleSenseEvent_Damage(in AIController controller, in DamageEvent damageEvent)
			{

			}
			static void HandleSenseEvent_Hearing(in AIController controller, in HearingEvent hearingEvent)
			{

			}
			static void HandleSenseEvent_Sight(in AIController controller, in SightEvent sightEvent)
			{
				switch (sightEvent.TargetInfoType)
				{
					case ESightTargetEventType.ACQUIRED:
					{
						(Entity EntitySeen, Vector3 SeenPosition, Vector3 ViewerPosition) = sightEvent.AsTargetAcquiredEvent();

						// Remove memory of previous enemy entity
						//	controller.BrainComponent.MemoryComponent.RemoveMemory(controller.m_EnemyEntityMemoryIdentifier);

					//	controller.BehaviorTreeComponent.BlackboardInstanceData.SetEntryValue<BBEntry_Entity, Entity>("EntitySeen", EntitySeen);

						// Set current enemy entity
					//	controller.Blackboard.SetEntryValue<BBEntry_TargetEntity, Entity>(controller.m_CurrentEnemyOnSight, EntitySeen);
						return;
					}
					case ESightTargetEventType.CHANGED:
					{
						(Entity EntitySeen, Vector3 SeenPosition, Vector3 ViewerPosition) = sightEvent.AsTargetChangedEvent();

						// Set current enemy entity
				//		controller.Blackboard.SetEntryValue<BBEntry_TargetEntity, Entity>(controller.m_CurrentEnemyOnSight, EntitySeen);
						return;
					}
					case ESightTargetEventType.LOST:
					{
						(Entity LostTarget, Vector3 SeenPosition, Vector3 LastDirection, Vector3 ViewerPosition) = sightEvent.AsTargetLostEvent();

						// Remove key for current enemy entity
					//	controller.Blackboard.RemoveEntry(controller.m_CurrentEnemyOnSight);
					//	controller.BehaviorTreeComponent.BlackboardInstanceData.RemoveEntry("EntitySeen");

						// Set memory of this entity last position and movement
					//	controller.BrainComponent.MemoryComponent.AddTrajectoryToMemory(controller.m_EnemyEntityMemoryIdentifier, SeenPosition, LastDirection);
						return;
					}
				}
				Utils.CustomAssertions.IsTrue(false, controller, $"Invalid {nameof(sightEvent.TargetInfoType)} of {nameof(SightEvent)} with value '{sightEvent.TargetInfoType}'");
			}
			static void HandleSenseEvent_Team(in AIController controller, in TeamEvent teamEvent)
			{
				switch (teamEvent.MessageType)
				{
					case ETeamMessageType.DAMAGE:
					{
						(Vector3 HittedPosition, Vector3 Direction, EDamageType DamageType) = teamEvent.AsDamageMessage();
						return;
					}
					case ETeamMessageType.SOUND:
					{
						(Vector3 SoundPosition, ESoundType SoundType) = teamEvent.AsSoundMessage();
						return;
					}

					case ETeamMessageType.HOSTILE:
					{
						(Vector3 WorldPosition, Vector3 Direction, Entity Entity) = teamEvent.AsHostileEvent();
						return;
					}
					case ETeamMessageType.HOSTILE_LOST:
					{
						(Vector3 WorldLastPosition, Vector3 LastDirection, Entity Entity/*null here*/) = teamEvent.AsHostileEvent();
						return;
					}
				}
				Utils.CustomAssertions.IsTrue(false, controller, $"Invalid {nameof(teamEvent.MessageType)} of {nameof(TeamEvent)} with value '{teamEvent.MessageType}'");
			}

			Utils.CustomAssertions.IsNotNull(senseEvent);
			switch (senseEvent.SenseType)
			{
				case ESenses.DAMAGE:	HandleSenseEvent_Damage(this, senseEvent as DamageEvent);	return;
				case ESenses.HEARING:	HandleSenseEvent_Hearing(this, senseEvent as HearingEvent);	return;
				case ESenses.SIGHT:		HandleSenseEvent_Sight(this, senseEvent as SightEvent);		return;
				case ESenses.TEAM:		HandleSenseEvent_Team(this, senseEvent as TeamEvent);		return;
			}
			Utils.CustomAssertions.IsTrue(false, this, $"Cannot handle sense event of type {senseEvent.SenseType}");
		}
	}
}
