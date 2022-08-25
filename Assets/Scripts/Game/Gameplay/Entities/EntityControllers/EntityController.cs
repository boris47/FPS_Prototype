
using UnityEngine;

namespace Entities
{
	/// <summary>
	/// A PlayerController is used by human players to control the Player, while an AIController implements the artificial intelligence for the entity they control.<br />
	/// Controllers take control of an Entity with the Possess function, and give up control of the Pawn with the UnPossess function.<br />
	///
	/// Controllers receive notifications for many of the events occurring for the entity they are controlling.<br />
	/// This gives the Controller the opportunity to implement the behavior in response to this event, intercepting the event and superseding the Entity's default behavior.<br />
	/// <b>NOTE</b>: There is a one-to-one relationship between Controllers and Entities; meaning, each Controller controls only one Entity at any given time
	/// </summary>
	[DefaultExecutionOrder(-5)]
	public abstract class EntityController : MonoBehaviour
	{
		[SerializeField, ReadOnly]
		private					Entity							m_ControllableEntity				= null;

		[SerializeField]
		private					bool							m_PossessOnAwake					= true;


		//--------------------
		public					Entity							ControlledEntity					=> m_ControllableEntity;


		//////////////////////////////////////////////////////////////////////////
		protected virtual void Awake()
		{
			if (m_PossessOnAwake && gameObject.TryGetComponent(out Entity controllableEntity))
			{
				Possess(controllableEntity);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void Possess(in Entity entity)
		{
			// TODO Check for authority

			bool bNewEntity = m_ControllableEntity != entity;

			if (bNewEntity)
			{
				// New Entity for this controller, so release the controlled one
				if (m_ControllableEntity.IsNotNull())
				{
					UnPossess();
				}

				// If entity has controller then it must release entity
				entity.Controller?.UnPossess();

				// Mutual assignment
				m_ControllableEntity = entity;
				entity.SetController(this);
				OnPossess(entity);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected abstract void OnPossess(in Entity entity);

		//////////////////////////////////////////////////////////////////////////
		public void UnPossess()
		{
			m_ControllableEntity.SetController(null);
			OnUnPossess(m_ControllableEntity);

			m_ControllableEntity = null;
		}

		//////////////////////////////////////////////////////////////////////////
		protected abstract void OnUnPossess(in Entity entity);
	}
}


