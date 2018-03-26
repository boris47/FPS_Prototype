
using UnityEngine;

public abstract partial class NonLiveEntity : Entity {

	// COLLIDER EVENT
	protected virtual void OnCollisionEnter( Collision collision )
	{
		Bullet bullet = collision.gameObject.GetComponent<Bullet>();
		if ( bullet == null )
			return;

		float  Damage	= Random.Range( bullet.DamageMin, bullet.DamageMax );
		Entity Who		= bullet.WhoRef;

		if ( bullet.WhoRef is Player )
		{
			if ( bullet.IsCloseRange )
				OnHurt( ref Who, Damage );
			else
				OnHit( ref Who, Damage );
		}

		bullet.SetActive( false );
	}

	
	// VIEW TRIGGER EVENT
	private void OnTriggerEnter( Collider other )
	{
		Entity entity = other.GetComponent<Entity>();
		if ( entity is LiveEntity )
		{
			if ( m_Targets.Contains( entity ) )
				return;

			m_Targets.Add( entity );
		}

	}


	// VIEW TRIGGER EVENT
	private void OnTriggerExit( Collider other )
	{
		Entity entity = other.GetComponent<Entity>();
		if ( entity is LiveEntity )
		{
			if ( m_Targets.Contains( entity ) == false )
				return;

			m_Targets.Remove( entity );
		}

	}
	

}
