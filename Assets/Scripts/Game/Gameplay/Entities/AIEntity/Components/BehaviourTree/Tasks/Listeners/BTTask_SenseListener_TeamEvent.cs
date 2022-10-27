using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	using Senses;

	[BTNodeDetails("Listen For Team Events", "Enable listenting for a sense")]
	public sealed class BTTask_SenseListener_TeamEvent : BTTask_SenseListenerBase
	{
		[SerializeReference, ToNodeInspector, BlackboardKeyType(typeof(BBEntry_TeamEvent_TargetSeen_Event))]
		private				BlackboardEntryKey								m_TargetSeenKey										= null;

		[SerializeReference, ToNodeInspector, BlackboardKeyType(typeof(BBEntry_TeamEvent_TargetLost_Event))]
		private				BlackboardEntryKey								m_TargetLostKey										= null;

		[SerializeReference, ToNodeInspector, BlackboardKeyType(typeof(BBEntry_TeamEvent_SoundHeard_Event))]
		private				BlackboardEntryKey								m_SoundHeardKey										= null;

		[SerializeReference, ToNodeInspector, BlackboardKeyType(typeof(BBEntry_TeamEvent_Damage_Event))]
		private				BlackboardEntryKey								m_DamageReceivedKey									= null;

		//////////////////////////////////////////////////////////////////////////
		protected override RuntimeDataBase CreateRuntimeDataInstance(in BTNodeInstanceData InThisNodeInstanceData)
		{
			return new SenseListenerRuntimeData(InThisNodeInstanceData.Controller, m_TargetSeenKey, m_TargetLostKey, m_SoundHeardKey, m_DamageReceivedKey);
		}

		//////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////
		private sealed class SenseListenerRuntimeData : RuntimeData
		{
			private readonly BlackboardEntryKey TargetSeenKey		= null;
			private readonly BlackboardEntryKey TargetLostKey		= null;
			private readonly BlackboardEntryKey SoundHeardKey		= null;
			private readonly BlackboardEntryKey DamageReceivedKey	= null;

			public SenseListenerRuntimeData(in AIController InController, in BlackboardEntryKey InTargetSeenKey,
				in BlackboardEntryKey InTargetLostKey,
				in BlackboardEntryKey InSoundHeardKey,
				in BlackboardEntryKey InDamageReceivedKey) : base(InController)
			{
				TargetSeenKey = InTargetSeenKey;
				TargetLostKey = InTargetLostKey;
				SoundHeardKey = InSoundHeardKey;
				DamageReceivedKey = InDamageReceivedKey;
			}

			//////////////////////////////////////////////////////////////////////////
			protected override void OnNewSenseEvent(in SenseEvent newSenseEvent)
			{
				if (newSenseEvent is TeamEvent teamEvent)
				{
					switch (teamEvent.MessageType)
					{
						case ETeamMessageType.HOSTILE:
						{
							GetBlackboardInstance(Controller).SetEntryValue<BBEntry_TeamEvent_TargetSeen_Event, TeamEvent>(TargetSeenKey, teamEvent);
							break;
						}
						case ETeamMessageType.HOSTILE_LOST:
						{
							GetBlackboardInstance(Controller).SetEntryValue<BBEntry_TeamEvent_TargetLost_Event, TeamEvent>(TargetLostKey, teamEvent);
							break;
						}
						case ETeamMessageType.SOUND:
						{
							GetBlackboardInstance(Controller).SetEntryValue<BBEntry_TeamEvent_SoundHeard_Event, TeamEvent>(SoundHeardKey, teamEvent);
							break;
						}
						case ETeamMessageType.DAMAGE:
						{
							GetBlackboardInstance(Controller).SetEntryValue<BBEntry_TeamEvent_Damage_Event, TeamEvent>(DamageReceivedKey, teamEvent);
							break;
						}
					}
				}
			}
		}
	}
}
