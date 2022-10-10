
using UnityEngine;

namespace Entities.AI.Components
{
	using Entities.AI.Components.Behaviours;

	public class AIBehaviorTreeComponent : AIEntityComponent
	{
		[SerializeField]
		private				BehaviourTree									m_BehaviourTreeAsset							= null;

		[SerializeField, ReadOnly]
		private				BehaviourTreeInstanceData						m_TreeInstanceData								= null;

		public				BehaviourTree									BehaviourTreeAsset								=> m_BehaviourTreeAsset;



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
		protected override void OnEnable()
		{
			base.OnEnable();

			if (m_BehaviourTreeAsset.IsNotNull() && m_TreeInstanceData.IsNotNull() && m_BehaviourTreeAsset.StartTree(m_TreeInstanceData))
			{
				if (Utils.CustomAssertions.IsNotNull(GameManager.CyclesEvents))
				{
					GameManager.CyclesEvents.OnFrame += OnFrame;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnFrame(float InDeltaTime)
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
				GameManager.CyclesEvents.OnFrame -= OnFrame;
			}

			base.OnDisable();
		}
	}
}
