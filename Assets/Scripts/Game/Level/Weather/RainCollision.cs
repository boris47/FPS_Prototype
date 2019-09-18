using UnityEngine;
using System.Collections.Generic;

namespace WeatherSystem {

	public class RainCollision : MonoBehaviour {

		public	ParticleSystem					m_RainExplosion			= null;

		private	ParticleSystem					m_RainParticleSystem	= null;
		private	Color32							m_Color					= new Color32( 255, 255, 255, 255 );
		private	List<ParticleCollisionEvent>	m_CollisionEvents		= new List<ParticleCollisionEvent>();


		//////////////////////////////////////////////////////////////////////////
		// START
		private void Awake()
		{
			m_RainParticleSystem = GetComponent<ParticleSystem>();
			if ( m_RainParticleSystem == null )
			{
				enabled = false;
				return;
			}

			m_CollisionEvents.Capacity = m_RainParticleSystem.main.maxParticles / 3;
		}


		//////////////////////////////////////////////////////////////////////////
		// Emit
		ParticleSystem.EmitParams param = new ParticleSystem.EmitParams();
		private void Emit( ref ParticleSystem particle, Vector3 position )
		{
			int count = Random.Range( 1, 4 );
			while ( count > 0 )
			{
				float yVelocity = Random.Range(  0.5f, 1.0f );
				float zVelocity = Random.Range( -0.5f, 1.0f );
				float xVelocity = Random.Range( -0.5f, 1.0f );
				const float lifetime = 0.1f;// UnityEngine.Random.Range(0.25f, 0.75f);
				float size = Random.Range( 0.05f, 0.1f );

				param.position			= position;
				param.velocity			= new Vector3( xVelocity, yVelocity, zVelocity );
				param.startLifetime		= lifetime;
				param.startSize			= size;
				param.startColor		= m_Color;

				particle.Emit( param, 1 );

				count--;
			}
		}
		

		//////////////////////////////////////////////////////////////////////////
		// UNITY
		private void OnParticleCollision( GameObject obj )
		{
			if ( obj.tag == "Player" )
				return;

			if ( m_RainExplosion != null && m_RainParticleSystem != null )
			{
				int count = m_RainParticleSystem.GetCollisionEvents( obj, m_CollisionEvents );
				for ( int i = 0; i < count; i++ )
				{
					if ( Random.value > 0.75f )
						continue;

//					ParticleCollisionEvent evt = m_CollisionEvents[ i ];
					this.Emit( ref m_RainExplosion, m_CollisionEvents[ i ].intersection );
				}
			}
		}

	}

}