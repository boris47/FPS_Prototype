
using UnityEngine;

namespace Entities
{
	[RequireComponent(typeof(Entity))]
	public class EntityComponent : MonoBehaviour
	{
		[SerializeField, ReadOnly]
		protected			Entity										m_Owner									= null;


		protected virtual void Awake()
		{
			Utils.CustomAssertions.IsTrue(gameObject.TrySearchComponent(ESearchContext.LOCAL_AND_PARENTS, out m_Owner));
		}

		protected virtual void OnValidate()
		{
			
		}

		protected virtual void OnEnable()
		{
			
		}

		protected virtual void OnDisable()
		{
			
		}

		protected virtual void OnDestroy()
		{
			
		}
	}
}
