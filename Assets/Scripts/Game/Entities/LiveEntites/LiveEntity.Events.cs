
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

			if ( m_Health <= 0f )
			{
				print( "Morte da caduta" );
			}

		}
	}

	/*
	private void OnCollisionEnter( Collision collision )
	{
		IBullet bullet = collision.gameObject.GetComponent<IBullet>();
		if ( bullet == null )
			return;

		if ( bullet.WhoRef is LiveEntity )
			return;

		// long range attack
		OnHit( ref bullet );
	}
	*/
	

	float currentHitTime;
	private void OnTriggerEnter( Collider other )
	{
		
		IBullet bullet = other.GetComponent<IBullet>();
		if ( bullet == null )
			return;

		// Avoid hits on the same frame
		if ( currentHitTime == Time.time )
		{
			return;
		}
		currentHitTime = Time.time;

		if ( bullet.WhoRef is LiveEntity )
			return;

		// long range attack
		OnHit( ref bullet );

		bullet.SetActive( false );
	}
	
}