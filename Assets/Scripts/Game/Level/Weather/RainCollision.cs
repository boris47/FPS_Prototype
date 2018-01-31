using UnityEngine;
using System.Collections.Generic;

public class RainCollision : MonoBehaviour {

	public	ParticleSystem					m_RainExplosion			= null;

	private	ParticleSystem					m_RainParticleSystem	= null;
	private	Color32							m_Color					= new Color32( 255, 255, 255, 255 );
	private	List<ParticleCollisionEvent>	m_CollisionEvents		= new List<ParticleCollisionEvent>();


	//////////////////////////////////////////////////////////////////////////
	// START
	private void Start()
	{
		m_RainParticleSystem = GetComponent<ParticleSystem>();
		if ( m_RainParticleSystem == null )
		{
			enabled = false;
			return;
		}

		m_CollisionEvents.Capacity = m_RainParticleSystem.main.maxParticles;
	}


	//////////////////////////////////////////////////////////////////////////
	// Emit
	private void Emit( ParticleSystem p, ref Vector3 pos )
	{
		int count = UnityEngine.Random.Range( 1, 4 );
		while ( count != 0 )
		{
			float yVelocity = UnityEngine.Random.Range( 0.5f, 1.0f);
			float zVelocity = UnityEngine.Random.Range(-0.5f, 1.0f);
			float xVelocity = UnityEngine.Random.Range(-0.5f, 1.0f);
			const float lifetime = 0.1f;// UnityEngine.Random.Range(0.25f, 0.75f);
			float size = UnityEngine.Random.Range(0.05f, 0.1f);

			ParticleSystem.EmitParams param = new ParticleSystem.EmitParams();
			param.position = pos;
			param.velocity = new Vector3(xVelocity, yVelocity, zVelocity);
			param.startLifetime = lifetime;
			param.startSize = size;
			param.startColor = m_Color;

			p.Emit(param, 1);

			count--;
		}
	}
		

	//////////////////////////////////////////////////////////////////////////
	// UNITY
	private void OnParticleCollision( GameObject obj )
	{
		if ( m_RainExplosion != null && m_RainParticleSystem != null)
		{
			int count = m_RainParticleSystem.GetCollisionEvents( obj, m_CollisionEvents );
			for ( int i = 0; i < count; i++ )
			{
				if ( Random.value > 0.75f )
					continue;

				ParticleCollisionEvent evt = m_CollisionEvents[i];
				Vector3 pos = evt.intersection;
				this.Emit( m_RainExplosion, ref pos );
			}
		}
	}

}