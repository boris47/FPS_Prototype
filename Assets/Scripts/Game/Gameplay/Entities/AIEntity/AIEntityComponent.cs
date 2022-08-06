
using UnityEngine;

namespace Entities.AI.Components
{
	[RequireComponent(typeof(AIController))]
	public class AIEntityComponent : EntityComponent
	{
		[SerializeField, ReadOnly]
		protected			AIController									m_Controller							= null;


		//////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_Controller));

			base.Awake();
		}
	}
}
