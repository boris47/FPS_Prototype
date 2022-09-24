using UnityEngine;

namespace Entities.AI
{
	using Components;
	using Components.Senses;
	
	/// <summary>
	/// The AIController is more focused on responding to input from the environment and game world.<br />
	/// The job of the AIController is to observe the world around it and make decisions and react accordingly without explicit input from a human player.<br />
	/// </summary>
	[RequireComponent(typeof(AIBrainComponent))]
	[RequireComponent(typeof(AIPerceptionComponent))]
	[RequireComponent(typeof(AIBehaviorTreeComponent))]
	[RequireComponent(typeof(Blackboard))]
    public partial class AIController : EntityController
    {
        [SerializeField, ReadOnly]
        private				AIBrainComponent				m_BrainComponent					= null;
		[SerializeField, ReadOnly]
		private				AIPerceptionComponent			m_PerceptionComponent				= null;
		[SerializeField, ReadOnly]
		private				AIBehaviorTreeComponent			m_BehaviorTreeComponent				= null;
		[SerializeField, ReadOnly]
		private				Blackboard						m_BlackBoard						= null;

		[SerializeField]
		protected			bool							m_StartBrainOnPossess				= false;

		public				AIBrainComponent				BrainComponent						=> m_BrainComponent;
		public				AIPerceptionComponent			PerceptionComponent					=> m_PerceptionComponent;
		public				AIBehaviorTreeComponent			BehaviorTreeComponent				=> m_BehaviorTreeComponent;
		public				Blackboard						Blackboard							=> m_BlackBoard;

		public				AIEntity						Entity								{ get; private set; } = null;



		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			if (Utils.CustomAssertions.IsTrue(gameObject.TryGetIfNotAssigned(ref m_BrainComponent)))
			{

			}

			if (Utils.CustomAssertions.IsTrue(gameObject.TryGetIfNotAssigned(ref m_PerceptionComponent)))
			{
				// TODO: Sense event handling should belong to entity brain instead!?
				m_PerceptionComponent.Senses.OnNewSenseEvent += HandleSenseEvent;
			}

			if (Utils.CustomAssertions.IsTrue(gameObject.TryGetIfNotAssigned(ref m_BehaviorTreeComponent)))
			{

			}

			if (Utils.CustomAssertions.IsTrue(gameObject.TryGetIfNotAssigned(ref m_BlackBoard)))
			{

			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnValidate()
		{
			gameObject.TryGetIfNotAssigned(ref m_BrainComponent);
			gameObject.TryGetIfNotAssigned(ref m_PerceptionComponent);
			gameObject.TryGetIfNotAssigned(ref m_BehaviorTreeComponent);
			gameObject.TryGetIfNotAssigned(ref m_BlackBoard);
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
		public BlackboardEntryKey m_onEntitySeen = null;
		public BlackboardEntryKey m_EntityLost = null;
		public MemoryIdentifier m_EntityMemoryIdentifier = null;
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
						controller.BrainComponent.MemoryComponent.RemoveMemory(controller.m_EntityMemoryIdentifier);
						controller.Blackboard.SetEntryValue<BBEntry_EntityToEvaluate, Entity>(controller.m_onEntitySeen, EntitySeen);
						controller.Blackboard.RemoveEntry(controller.m_EntityLost);
						return;
					}
					case ESightTargetEventType.CHANGED:
					{
						(Entity EntitySeen, Vector3 SeenPosition, Vector3 ViewerPosition) = sightEvent.AsTargetChangedEvent();
						return;
					}
					case ESightTargetEventType.LOST:
					{
						(Entity LostTarget, Vector3 SeenPosition, Vector3 LastDirection, Vector3 ViewerPosition) = sightEvent.AsTargetLostEvent();
						controller.BrainComponent.MemoryComponent.AddTrajectoryToMemory(controller.m_EntityMemoryIdentifier, SeenPosition, LastDirection);
						controller.Blackboard.SetEntryValue<BBEntry_EntityToEvaluate, Entity>(controller.m_EntityLost, LostTarget);
						controller.Blackboard.RemoveEntry(controller.m_onEntitySeen);
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
