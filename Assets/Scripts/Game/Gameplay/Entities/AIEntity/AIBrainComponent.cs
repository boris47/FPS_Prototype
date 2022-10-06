
using UnityEngine;


namespace Entities.AI.Components
{
	[RequireComponent(typeof(AIMemoryComponent))]
	[RequireComponent(typeof(AIEntityActionsComponent))]
	public partial class AIBrainComponent : AIEntityComponent
	{
		[SerializeField, ReadOnly]
		private				AIEntityActionsComponent		m_AIEntityActionsComponent			= null;

		[SerializeField, ReadOnly]
		private				AIMemoryComponent				m_MemoryComponent					= null;


		public				AIEntityActionsComponent		EntityActionsComponent				=> m_AIEntityActionsComponent;
		public				AIMemoryComponent				MemoryComponent						=> m_MemoryComponent;


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			if (m_MemoryComponent.IsNotNull() || Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_MemoryComponent)))
			{

			}

			if (m_AIEntityActionsComponent.IsNotNull() || Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_AIEntityActionsComponent)))
			{

			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnValidate()
		{
			base.OnValidate();

			gameObject.TryGetComponent(out m_MemoryComponent);
			gameObject.TryGetComponent(out m_AIEntityActionsComponent);
		}

		//////////////////////////////////////////////////////////////////////////

	}
}
