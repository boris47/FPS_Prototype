
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	using TypeReferences;

	/**
	 * The idea behaind this:
	 * - Different mental state, even specific for an entity, with its own blackboard for data handling
	 * - Possibility to switch to another mental state transferring need data if necessary
	 * - High level of scripting
	 * - Require a custom editor, very similar to BT one.
	 * Pro:
	 * - Easy custom logic composition
	 * - Modular
	 * Cons:
	 * - Can create a lot of asset based on granularity or extension
	 */
	public class AIBehavioursManager : AIEntityComponent
	{
		[SerializeReference, ReadOnly]
		private				AIBehaviourBase									m_CurrentBehaviour								= null;

		[SerializeReference, ReadOnly]
		private				AIBehaviourBlackboard							m_Blackboard									= null;

		public				AIBehaviourBlackboard							Blackboard										=> m_Blackboard;

		[SerializeField, Inherits(baseType: typeof(AIBehaviourBase), AllowAbstract = false, ShowNoneElement = false, Grouping = Grouping.ByNamespaceFlat, ShortName = true)]
		private				TypeReference									m_DefaultBehaviour								= typeof(AIBehaviourIdle);


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			m_Blackboard = ScriptableObject.CreateInstance<AIBehaviourBlackboard>();
			m_CurrentBehaviour = gameObject.AddChildWithComponent(m_DefaultBehaviour.Type.Name, m_DefaultBehaviour, true, false) as AIBehaviourBase;
			{
				m_CurrentBehaviour.Setup(Entity, m_Controller, m_Blackboard);
			}
			m_CurrentBehaviour.enabled = true;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryTransitionTo<T>() where T : AIBehaviourBase, new() => TryTransitionTo(typeof(T));

		//////////////////////////////////////////////////////////////////////////
		public bool TryTransitionTo(in System.Type InBehaviourType)
		{
			bool OutValue = false;
			{
				if (Utils.CustomAssertions.IsTrue(ReflectionHelper.IsInerithedFrom(typeof(AIBehaviourBase), InBehaviourType)))
				{
					if (m_CurrentBehaviour.GetType() != InBehaviourType)
					{
						m_CurrentBehaviour.gameObject.Destroy();

						m_CurrentBehaviour = gameObject.AddChildWithComponent(InBehaviourType.Name, InBehaviourType, true, false) as AIBehaviourBase;

						m_CurrentBehaviour.Setup(Entity, m_Controller, m_Blackboard);

						m_CurrentBehaviour.enabled = true;

						OutValue = true;
					}
				}
			}
			return OutValue;
		}
	}
}

