
using UnityEngine;

namespace Entities
{
	public interface IControllableEntity<T> where T : EntityController, new()
	{
		T Controller { get; }
	}

	public abstract partial class Entity
	{
		[SerializeField, ReadOnly]
		protected EntityController m_Controller = null;

		public EntityController Controller => m_Controller;

		public void SetController(EntityController entityController)
		{
			m_Controller = entityController;
		}
	}
}
