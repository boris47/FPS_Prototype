using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	using Senses;

	[BTNodeDetails("Sense listener")]
	public abstract class BTTask_SenseListenerBase : BTTaskNode
	{
		//////////////////////////////////////////////////////////////////////////
		protected sealed override EBTNodeInitializationResult OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			RuntimeData nodeRuntimeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
			{
				nodeRuntimeData.Enable();
			}
			return EBTNodeInitializationResult.SUCCEEDED;
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override void OnNodeReset(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnNodeReset(InThisNodeInstanceData);

			RuntimeData nodeRuntimeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
			nodeRuntimeData.Disable();
		}


		//////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////
		protected abstract class RuntimeData : RuntimeDataBase
		{
			private readonly AIController m_Controller = null;

			protected AIController Controller => m_Controller;

			//////////////////////////////////////////////////////////////////////////
			public RuntimeData(in AIController InController)
			{
				m_Controller = InController;
			}

			//////////////////////////////////////////////////////////////////////////
			public void Enable() => m_Controller.PerceptionComponent.OnNewSenseEvent += OnNewSenseEvent;

			//////////////////////////////////////////////////////////////////////////
			public void Disable() => m_Controller.PerceptionComponent.OnNewSenseEvent -= OnNewSenseEvent;

			//////////////////////////////////////////////////////////////////////////
			protected abstract void OnNewSenseEvent(in SenseEvent newSenseEvent);
		}
	}
}
