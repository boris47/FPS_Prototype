
using UnityEngine;

namespace Entities
{
	[RequireComponent(typeof(PlayerController))]
	public class PlayerEntity : Entity
	{
		public new PlayerController Controller => m_Controller as PlayerController;
	}
}
