
using UnityEngine;

[RequireComponent( typeof( SphereCollider) ) ]
public class ViewAreaTrigger : MonoBehaviour {

	[SerializeField, Range( 1f, 200f )]
	private		float					m_ViewRange				= 1f;

	private		IEntityInterface		m_Entity				= null;
	private		SphereCollider			m_ViewTriggerCollider	= null;



	private void Start()
	{
		m_Entity = transform.parent.GetComponent<IEntityInterface>();
		if ( m_Entity == null )
			print( transform.parent.name + " is not an \"Entity\"" );

		m_ViewTriggerCollider = GetComponent<SphereCollider>();
	}


	private void OnValidate()
	{
		m_ViewTriggerCollider = GetComponent<SphereCollider>();
		m_ViewTriggerCollider.radius = m_ViewRange;
	}

	// VIEW TRIGGER EVENT
	private void OnTriggerEnter( Collider other )
	{
		Entity entity = other.GetComponent<Entity>();
		if ( entity is LiveEntity )
		{
			m_Entity.OnViewAreaEnter( entity );
		}

	}


	// VIEW TRIGGER EVENT
	private void OnTriggerExit( Collider other )
	{
		Entity entity = other.GetComponent<Entity>();
		if ( entity is LiveEntity )
		{
			m_Entity.OnViewAreaExit( entity );
		}

	}

}
