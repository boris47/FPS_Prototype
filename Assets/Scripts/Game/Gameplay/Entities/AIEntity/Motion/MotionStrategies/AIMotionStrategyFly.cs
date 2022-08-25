
using UnityEngine;

namespace Entities.AI.Components
{
	[Configurable(nameof(m_Config), "AI/MotionStrategies/" + nameof(AIMotionStrategyFly))]
	public class AIMotionStrategyFly : AIMotionStrategyBase
	{
		[SerializeField, ReadOnly]
		private AIConfigurationFly m_Config = null;

		public override Vector3 Position => transform.position;


		protected override void Awake()
		{
			base.Awake();

			Utils.CustomAssertions.IsTrue(this.TryGetConfiguration(out m_Config));
		}

		public override void SetNewDestination(in Vector3 InDestination)
		{
			
		}

		private void Update()
		{
			
		}
	}
}
