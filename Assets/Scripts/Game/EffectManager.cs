
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
	public	void	PlayEffect( EffectType effectType,  Vector3 position, Vector3 direction, int count = 0 )
	{

		ParticleSystem p = null;
		if ( count > 0 )
		{
			p = m_Effects[ (int) effectType ];
			p.transform.position = position;
			p.transform.forward = direction;
			p.Emit( count );
		}
		else
		{
			p = Instantiate( m_Effects[ (int) effectType ] );
			p.transform.position = position;
			p.transform.forward = direction;
			p.Play( withChildren : true );
			Destroy( p.gameObject, 5.0f );
		}
	}
	

	//////////////////////////////////////////////////////////////////////////
	// PlayerExplosionSound
	public	void	PlayExplosionSound( Vector3 position )
	{
		m_ExplosionSource.transform.position = position;
		( m_ExplosionSource as ICustomAudioSource ).Play();
	}

}
