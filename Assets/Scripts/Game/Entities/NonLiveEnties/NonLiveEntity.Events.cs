
using UnityEngine;

public abstract partial class NonLiveEntity : Entity {

	// COLLIDER EVENT
	protected virtual void OnCollisionEnter( Collision collision )
	{
		IBullet bullet = collision.gameObject.GetComponent<IBullet>();
		if ( bullet == null )
			return;

		float  Damage	= Random.Range( bullet.DamageMin, bullet.DamageMax );
		Entity Who		= bullet.WhoRef;

		// Avoid friendly fire
		if ( bullet.WhoRef is LiveEntity )
		{
			OnHit( ref Who, Damage );
		}

		bullet.SetActive( false );
	}

}
