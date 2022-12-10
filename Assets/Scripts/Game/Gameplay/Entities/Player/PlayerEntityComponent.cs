
using UnityEngine;

namespace Entities.Player.Components
{
	[RequireComponent(typeof(PlayerController))]
	[RequireComponent(typeof(PlayerEntity))]
	public class PlayerEntityComponent : EntityComponent, IOwnedComponent<PlayerEntity>
	{
		[SerializeField, ReadOnly]
		protected			PlayerController									m_Controller							= null;

		public				PlayerEntity										Owner =>								m_Owner as PlayerEntity;

		//////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_Controller));

			m_Owner = base.m_Owner as PlayerEntity;
		}
	}
}
