
using UnityEngine;

public abstract partial class LiveEntity  {

	public		void	EvaluateFall( Vector3 fallDistance )
	{
		if ( IsFalling == false )
			return;

		float sqrMagnitude = fallDistance.sqrMagnitude;
//		print( "Fall evaluation, magnitude " + sqrMagnitude );
		if ( sqrMagnitude > m_FallDistanceThreshold * m_FallDistanceThreshold )
		{
//			float rapport = ( 1f - ( ( m_FallDistanceThreshold * m_FallDistanceThreshold ) / sqrMagnitude ) );

//			Entity empty = null;
//				Damage = ( magnitude + ( magnitude * rapport ) ) * 3f
//			float damage = rapport * sqrMagnitude;
//			print( "Extimated damage " + info.Damage );
//			this.OnHurt( ref empty, damage );

			if ( Health <= 0f )
			{
				print( "Morte da caduta" );
			}

		}
	}


	private void OnCollisionEnter( Collision collision )
	{
		Bullet bullet = collision.gameObject.GetComponent<Bullet>();
		if ( bullet == null )
			return;

		float damage	= Random.Range( bullet.DamageMin, bullet.DamageMax );
		Entity who		= bullet.WhoRef;

		if ( who is LiveEntity )
			return;

		if ( bullet.IsCloseRange )
		{
			// close range attack
			OnHurt( ref who, damage );
		}
		else
		{
			// long range attack
			OnHit( ref who, damage );
		}

	}

}