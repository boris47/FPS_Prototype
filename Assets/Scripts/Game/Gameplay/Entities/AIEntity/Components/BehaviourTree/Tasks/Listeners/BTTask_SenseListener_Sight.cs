using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	using Senses;

	[BTNodeDetails("Listen For Target Sight Events", "Enable listenting for a sense")]
	public sealed class BTTask_SenseListener_Sight : BTTask_SenseListenerBase
	{
		[SerializeReference, ToNodeInspector, BlackboardKeyType(typeof(BBEntry_SightEvent))]
		private				BlackboardEntryKey								m_SightEventKey								= null;


		//////////////////////////////////////////////////////////////////////////
		protected override RuntimeDataBase CreateRuntimeDataInstance(in BTNodeInstanceData InThisNodeInstanceData)
		{
			return new SenseListenerRuntimeData(InThisNodeInstanceData.Controller, m_SightEventKey);
		}

		//////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////
		private sealed class SenseListenerRuntimeData : RuntimeData
		{
			private readonly BlackboardEntryKey SightEventKey			= null;

			public SenseListenerRuntimeData(in AIController InController, in BlackboardEntryKey InSightEventKey) : base(InController)
			{
				SightEventKey = InSightEventKey;
			}

			//////////////////////////////////////////////////////////////////////////
			protected override void OnNewSenseEvent(in SenseEvent newSenseEvent)
			{
				if (newSenseEvent is SightEvent sightEvent)
				{
					switch (sightEvent.TargetInfoType)
					{
						case ESightTargetEventType.ACQUIRED:
						{
							GetBlackboardInstance(Controller).SetEntryValue<BBEntry_SightEvent, SightEvent>(SightEventKey, sightEvent);
							break;
						}
						case ESightTargetEventType.CHANGED:
						{
							GetBlackboardInstance(Controller).SetEntryValue<BBEntry_SightEvent, SightEvent>(SightEventKey, sightEvent);
							break;
						}
						case ESightTargetEventType.LOST:
						{
							GetBlackboardInstance(Controller).SetEntryValue<BBEntry_SightEvent, SightEvent>(SightEventKey, sightEvent);
							break;
						}
					}
				}
			}
		}
	}
}
