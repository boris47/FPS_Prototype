
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

}
