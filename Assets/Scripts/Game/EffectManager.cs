
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum EffectType {
	ENTITY_ON_HIT,
	AMBIENT_ON_HIT,
	ELETTRO,
	PLASMA,
	EXPLOSION,
	COUNT
}


public class EffectManager : MonoBehaviour {

	public	static	EffectManager			Instance								= null;

	[ SerializeField ]
	private		CustomAudioSource			m_ExplosionSource						= null;


	private		ParticleSystem[]			m_Effects = new ParticleSystem[ (int)EffectType.COUNT ];


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void	Awake()
	{
		if ( Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		Instance = this;
		DontDestroyOnLoad( this );

		bool result = true;
		{
			for ( int i = 0; i < (int)EffectType.COUNT; i++ )
			{
				ParticleSystem particleSystem = null;
				result &= Utils.Base.SearchComponent( gameObject, ref particleSystem, SearchContext.CHILDREN, ( p ) => { return p.transform.GetSiblingIndex() == i; } );
				m_Effects[i] = particleSystem;
			}
		}
		if ( result == false )
			return;
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayEntityOnHit
	public	void	PlayEntityOnHit( Vector3 position, Vector3 direction, int count = 3 )
	{
		ParticleSystem p = m_Effects[ (int) EffectType.ENTITY_ON_HIT ];
		p.transform.position = position;
		p.transform.forward = direction;
		p.Emit( count );
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayAmbientOnHit
	public	void	PlayAmbientOnHit( Vector3 position, Vector3 direction, int count = 3 )
	{
		ParticleSystem p = m_Effects[ (int) EffectType.AMBIENT_ON_HIT ];
		p.transform.position = position;
		p.transform.forward = direction;
		p.Emit( count );
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayAmbientOnHit
	public	void	PlayElettroEffect( Vector3 position, Vector3 direction, int count = 3 )
	{
		ParticleSystem p = m_Effects[ (int) EffectType.ELETTRO ];
		p.transform.position = position;
		p.transform.forward = direction;
		p.Emit( count );
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayAmbientOnHit
	public	void	PlayPlasmaEffect( Vector3 position, Vector3 direction, int count = 3 )
	{
		ParticleSystem p = m_Effects[ (int) EffectType.PLASMA ];
		p.transform.position = position;
		p.transform.forward = direction;
		p.Emit( count );
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayEntityExplosion
	public	void	PlayEntityExplosion( Vector3 position, Vector3 direction )
	{
		ParticleSystem p = Instantiate( m_Effects[ (int) EffectType.EXPLOSION ] );
		p.transform.position = position;
		p.transform.forward = direction;
		p.Play( withChildren : true );
		Destroy( p, 5.0f );
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayerExplosionSound
	public	void	PlayExplosionSound( Vector3 position )
	{
		m_ExplosionSource.transform.position = position;
		( m_ExplosionSource as ICustomAudioSource ).Play();
	}

}
