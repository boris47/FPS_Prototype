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
			if (!this.TryGetComponent(out this.m_RainParticleSystem))
			{
				this.enabled = false;
				return;
			}

			this.m_CollisionEvents.Capacity = this.m_RainParticleSystem.main.maxParticles / 3;
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

				this.param.position			= position;
				this.param.velocity			= new Vector3( xVelocity, yVelocity, zVelocity );
				this.param.startLifetime		= lifetime;
				this.param.startSize			= size;
				this.param.startColor		= this.m_Color;

				particle.Emit(this.param, 1 );

				count--;
			}
		}
		

		//////////////////////////////////////////////////////////////////////////
		// UNITY
		private void OnParticleCollision( GameObject obj )
		{
			if ( obj.tag == "Player" )
				return;

			if (this.m_RainExplosion != null && this.m_RainParticleSystem != null )
			{
				int count = this.m_RainParticleSystem.GetCollisionEvents( obj, this.m_CollisionEvents );
				for ( int i = 0; i < count; i++ )
				{
					if ( Random.value > 0.75f )
						continue;

//					ParticleCollisionEvent evt = m_CollisionEvents[ i ];
					this.Emit( ref this.m_RainExplosion, this.m_CollisionEvents[ i ].intersection );
				}
			}
		}

	}

}