
using UnityEngine;

namespace Entities.AI.Components
{
	[RequireComponent(typeof(AIController))]
	[RequireComponent(typeof(AIEntity))]
	public class AIEntityComponent : EntityComponent, IOwnedComponent<AIEntity>
	{
		[SerializeField, ReadOnly]
		private				AIEntity										m_Entity								= null;
		
		[SerializeField, ReadOnly]
		protected			AIController									m_Controller							= null;

		//--------------------
		public				AIEntity										Entity									=> m_Entity;

		public				AIEntity										Owner									=> m_Owner as AIEntity;


		//////////////////////////////////////////////////////////////////////////
		// Awake is called when the script instance is being loaded
		protected override void Awake()
		{
			base.Awake();

			Utils.CustomAssertions.IsTrue(gameObject.TrySearchComponent(ESearchContext.LOCAL_AND_PARENTS, out m_Controller));

			m_Entity = m_Owner as AIEntity;
		}

		//////////////////////////////////////////////////////////////////////////
		// This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only)
		protected override void OnValidate()
		{
			gameObject.TryGetIfNotAssigned(ref m_Controller);

			m_Entity = m_Owner as AIEntity;
		}
	}
}
