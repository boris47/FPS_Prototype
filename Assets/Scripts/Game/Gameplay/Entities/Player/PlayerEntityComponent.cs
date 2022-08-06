
using UnityEngine;

namespace Entities.Player.Components
{
	[RequireComponent(typeof(PlayerController))]
	public class PlayerEntityComponent : EntityComponent
	{
		[SerializeField, ReadOnly]
		protected			PlayerController									m_Controller							= null;


		//////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_Controller));

			base.Awake();
		}
	}
}
