
using UnityEngine;


namespace Entities.AI.Components
{
	[RequireComponent(typeof(AIMemoryComponent))]
	public partial class AIBrainComponent : AIEntityComponent
	{
		[SerializeField, ReadOnly]
		private				AIMemoryComponent				m_MemoryComponent					= null;

		public				AIMemoryComponent				MemoryComponent						=> m_MemoryComponent;


		protected override void Awake()
		{
			base.Awake();

			if (m_MemoryComponent.IsNotNull() || Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_MemoryComponent)))
			{

			}
		}

		protected override void OnValidate()
		{
			base.OnValidate();

			gameObject.TryGetComponent(out m_MemoryComponent);
		}
	}
}
