
using UnityEngine;

namespace Entities.AI.Components
{
	using Entities.AI.Components.Behaviours;

	public class AIBehaviorTreeComponent : AIEntityComponent
	{
		[SerializeField]
		private				BehaviourTree									m_BehaviourTreeAsset							= null;

		
		private				BehaviourTreeInstanceData						m_TreeInstanceData								= null;

		public				BehaviourTree									BehaviourTreeAsset								=> m_BehaviourTreeAsset;
		public				BlackboardInstanceData							BlackboardInstanceData							=> m_TreeInstanceData.Blackboard;



		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			if (m_BehaviourTreeAsset.IsNotNull())
			{
				m_TreeInstanceData = BehaviourTree.CreateInstanceFrom(m_BehaviourTreeAsset, m_Controller);
				m_BehaviourTreeAsset.OnAwake(m_TreeInstanceData);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnValidate()
		{
			base.OnValidate();

			if (m_BehaviourTreeAsset == null)
			{
				m_TreeInstanceData = null;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();

			if (m_BehaviourTreeAsset.IsNotNull() && m_TreeInstanceData.IsNotNull() && m_BehaviourTreeAsset.StartTree(m_TreeInstanceData))
			{
				if (Utils.CustomAssertions.IsNotNull(GameManager.CyclesEvents))
				{
					GameManager.CyclesEvents.OnThink += UpdateTree;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void UpdateTree(float InDeltaTime)
		{
			if (m_TreeInstanceData.IsNotNull())
			{
				m_BehaviourTreeAsset.UpdateTree(m_TreeInstanceData, InDeltaTime);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			if (m_BehaviourTreeAsset.IsNotNull() && m_TreeInstanceData.IsNotNull())
			{
				m_BehaviourTreeAsset.StopTree(m_TreeInstanceData);
			}

			if (GameManager.CyclesEvents.IsNotNull())
			{
				GameManager.CyclesEvents.OnThink -= UpdateTree;
			}

			base.OnDisable();
		}

#if UNITY_EDITOR
		public static class Editor
		{
			public static BehaviourTreeInstanceData GetBehaviourTreeInstanceData(in AIBehaviorTreeComponent InComponent)
			{
				return InComponent.m_TreeInstanceData.IsNotNull() ? InComponent.m_TreeInstanceData : null;
			}
		}
#endif
	}
}
