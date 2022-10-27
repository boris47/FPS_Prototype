using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	using Senses;

	[BTNodeDetails("Listen For Hear Events", "Enable listenting for a sense")]
	public sealed class BTTask_SenseListener_Hear : BTTask_SenseListenerBase
	{
		[SerializeReference, ToNodeInspector, BlackboardKeyType(typeof(BBEntry_SoundHeardEvent))]
		private				BlackboardEntryKey								m_BlackboardKey									= null;

		//////////////////////////////////////////////////////////////////////////
		protected override RuntimeDataBase CreateRuntimeDataInstance(in BTNodeInstanceData InThisNodeInstanceData)
		{
			return new SenseListenerRuntimeData(InThisNodeInstanceData.Controller, m_BlackboardKey);
		}

		//////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////
		private sealed class SenseListenerRuntimeData : RuntimeData
		{
			private readonly BlackboardEntryKey BlackboardKey		= null;

			public SenseListenerRuntimeData(in AIController InController, in BlackboardEntryKey InBlackboardKey) : base(InController)
			{
				BlackboardKey = InBlackboardKey;
			}

			//////////////////////////////////////////////////////////////////////////
			protected override void OnNewSenseEvent(in SenseEvent newSenseEvent)
			{
				if (newSenseEvent is HearingEvent hearingEvent)
				{
					GetBlackboardInstance(Controller).SetEntryValue<BBEntry_SoundHeardEvent, HearingEvent>(BlackboardKey, hearingEvent);
				}
			}
		}
	}
}
