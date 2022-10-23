using UnityEngine;


namespace Entities.AI.Components.Behaviours
{
	using Senses;

	[BTNodeDetails("Listen For Sight Events", "Enable listenting for a sense")]
	public class BTTask_SenseListener_Sight : BTTaskNode
	{
		[SerializeReference, ToNodeInspector, BlackboardKeyType(typeof(BBEntry_Entity))]
		private				BlackboardEntryKey								m_BlackboardKey									= null;

		//////////////////////////////////////////////////////////////////////////
		protected override RuntimeDataBase CreateRuntimeDataInstance(in BTNodeInstanceData InThisNodeInstanceData)
		{
			return new RuntimeData(InThisNodeInstanceData.Controller, m_BlackboardKey);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			EBTNodeState outState = EBTNodeState.FAILED;
			if (m_BlackboardKey.IsValid())
			{
				RuntimeData nodeRuntimeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
				{
					nodeRuntimeData.Enable();
				}
				outState = EBTNodeState.SUCCEEDED;
			}
			return outState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeReset(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnNodeReset(InThisNodeInstanceData);

			RuntimeData nodeRuntimeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
			nodeRuntimeData.Disable();
		}



		//////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////
		protected class RuntimeData : RuntimeDataBase
		{
			private readonly AIController m_Controller = null;
			private readonly BlackboardEntryKey m_BlackboardKey = null;

			public RuntimeData(in AIController InController, in BlackboardEntryKey InBlackboardKey)
			{
				m_Controller = InController;
				m_BlackboardKey = InBlackboardKey;
			}

			//////////////////////////////////////////////////////////////////////////
			public void Enable() => m_Controller.PerceptionComponent.OnNewSenseEvent += OnNewSenseEvent;

			//////////////////////////////////////////////////////////////////////////
			public void Disable() => m_Controller.PerceptionComponent.OnNewSenseEvent -= OnNewSenseEvent;

			//////////////////////////////////////////////////////////////////////////
			private void OnNewSenseEvent(in SenseEvent newSenseEvent)
			{
				if (newSenseEvent is SightEvent sightEvent)
				{
					switch (sightEvent.TargetInfoType)
					{
						case ESightTargetEventType.ACQUIRED:
						{
							GetBlackboardInstance(m_Controller).SetEntryValue<BBEntry_Entity, Entity>(m_BlackboardKey, sightEvent.EntitySeen);
							break;
						}
						case ESightTargetEventType.CHANGED:
						{
							GetBlackboardInstance(m_Controller).SetEntryValue<BBEntry_Entity, Entity>(m_BlackboardKey, sightEvent.EntitySeen);
							break;
						}
						case ESightTargetEventType.LOST:
						{
							GetBlackboardInstance(m_Controller).RemoveEntry(m_BlackboardKey);
							break;
						}
					}
				}
			}
		}
	}
}
