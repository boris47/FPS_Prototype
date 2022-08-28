
using UnityEngine;

namespace Entities
{
	interface IOwnedComponent<T> where T : Entity, new()
	{
		T Owner { get; }
	}

	[RequireComponent(typeof(Entity))]
	public class EntityComponent : MonoBehaviour
	{
		[SerializeField, ReadOnly]
		protected			Entity										m_Owner									= null;

		//////////////////////////////////////////////////////////////////////////
		// Awake is called when the script instance is being loaded
		protected virtual void Awake()
		{
			if (m_Owner == null)
			{
				Utils.CustomAssertions.IsTrue(gameObject.TrySearchComponent(ESearchContext.LOCAL_AND_PARENTS, out m_Owner));
			}
		}

		//////////////////////////////////////////////////////////////////////////
		// This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only)
		protected virtual void OnValidate()
		{
			gameObject.TryGetIfNotAssigned(ref m_Owner);
		}


		//////////////////////////////////////////////////////////////////////////
		// This function is called when the object becomes enabled and active
		protected virtual void OnEnable()
		{
			
		}

		//////////////////////////////////////////////////////////////////////////
		// This function is called when the behaviour becomes disabled or inactive
		protected virtual void OnDisable()
		{
			
		}

		//////////////////////////////////////////////////////////////////////////
		// This function is called when the MonoBehaviour will be destroyed
		protected virtual void OnDestroy()
		{
			
		}
	}
}
