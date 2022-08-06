using UnityEngine;

namespace Entities
{
	using AI.Components;
	using AI.Components.Senses;
	
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
        protected		AIBrainComponent			m_BrainComponent					= null;
		[SerializeField, ReadOnly]
        protected		AIPerceptionComponent		m_PerceptionComponent				= null;
		[SerializeField, ReadOnly]
        protected		AIBehaviorTreeComponent		m_BehaviorTreeComponent				= null;
		[SerializeField, ReadOnly]
		protected		Blackboard					m_BlackBoard						= null;

		public			AIBrainComponent			BrainComponent						=> m_BrainComponent;
		public			AIPerceptionComponent		PerceptionComponent					=> m_PerceptionComponent;
		public			AIBehaviorTreeComponent		BehaviorTreeComponent				=> m_BehaviorTreeComponent;
		public			Blackboard					Blackboard							=> m_BlackBoard;

		[SerializeField]
		protected		bool						m_StartBrainOnPossess				= false;


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
				m_PerceptionComponent.Senses.OnNewSenseEvent += HandleSenseEvent;
			}

			if (m_BehaviorTreeComponent.IsNotNull() || Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_BehaviorTreeComponent)))
			{

			}

			if (m_BlackBoard.IsNotNull() || Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_BlackBoard)))
			{

			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnPossess(in Entity entity)
		{
			if (m_StartBrainOnPossess)
			{

			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnUnPossess(in Entity entity)
		{
			throw new System.NotImplementedException();
		}

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
					case ETargetInfoType.ACQUIRED:
					{
						(Entity EntitySeen, Vector3 SeenPosition, Vector3 ViewerPosition) = sightEvent.AsTargetAcquiredEvent();
						return;
					}
					case ETargetInfoType.CHANGED:
					{
						(Entity EntitySeen, Vector3 SeenPosition, Vector3 ViewerPosition) = sightEvent.AsTargetChangedEvent();
						return;
					}
					case ETargetInfoType.LOST:
					{
						(Entity LostTarget, Vector3 SeenPosition, Vector3 LastDirection, Vector3 ViewerPosition) = sightEvent.AsTargetLostEvent();
						return;
					}
				}
				Utils.CustomAssertions.IsTrue(false, $"Invalid {nameof(sightEvent.TargetInfoType)} of {nameof(SightEvent)} with value '{sightEvent.TargetInfoType}'");
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
				Utils.CustomAssertions.IsTrue(false, $"Invalid {nameof(teamEvent.MessageType)} of {nameof(TeamEvent)} with value '{teamEvent.MessageType}'");
			}

			Utils.CustomAssertions.IsNotNull(senseEvent);
			switch (senseEvent.SenseType)
			{
				case ESenses.DAMAGE:	HandleSenseEvent_Damage(this, senseEvent as DamageEvent);	return;
				case ESenses.HEARING:	HandleSenseEvent_Hearing(this, senseEvent as HearingEvent);	return;
				case ESenses.SIGHT:		HandleSenseEvent_Sight(this, senseEvent as SightEvent);		return;
				case ESenses.TEAM:		HandleSenseEvent_Team(this, senseEvent as TeamEvent);		return;
			}
			Utils.CustomAssertions.IsTrue(false, $"Cannot handle sense event of type {senseEvent.SenseType}");
		}
	}
}
