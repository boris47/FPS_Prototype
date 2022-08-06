
using UnityEngine;

namespace Entities.AI.Components
{
	using Entities.AI.Components.Behaviours;

	public class AIBehaviorTreeComponent : AIEntityComponent
	{
		[SerializeField]
		private				BehaviourTree									m_BehaviourTree							= null;

	//	private				BehaviourTreeContext							m_Context								= null;

		public				BehaviourTree									BehaviourTree							=> m_BehaviourTree;



		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			if (m_BehaviourTree.IsNotNull())
			{
				m_BehaviourTree = BehaviourTree.CreateBehaviourTreeInstance(m_BehaviourTree);
				m_BehaviourTree.OnAwake(m_Controller);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();

			if (m_BehaviourTree.IsNotNull() && m_BehaviourTree.StartTree())
			{
				if (Utils.CustomAssertions.IsNotNull(GameManager.CyclesEvents))
				{
					GameManager.CyclesEvents.OnFrame += OnFrame;
					GameManager.CyclesEvents.OnPhysicFrame += OnFixedUpdate;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnFrame(float InDeltaTime)
		{
			m_BehaviourTree.UpdateFrame(InDeltaTime);

			m_BehaviourTree.Update();
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnFixedUpdate(float InFixedDeltaTime)
		{
			m_BehaviourTree.UpdateFixed();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			if (m_BehaviourTree.IsNotNull())
			{
				m_BehaviourTree.StopTree();
			}

			if (GameManager.CyclesEvents.IsNotNull())
			{
				GameManager.CyclesEvents.OnPhysicFrame -= OnFixedUpdate;
				GameManager.CyclesEvents.OnFrame -= OnFrame;
			}
			base.OnDisable();
		}
	}
}
