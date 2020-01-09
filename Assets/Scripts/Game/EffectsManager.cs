
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EffectType {
	ENTITY_ON_HIT,
	AMBIENT_ON_HIT,
	ELETTRO,
	PLASMA,
	EXPLOSION,

	MUZZLE,
	SMOKE,

	TRACER
};


public class EffectsManager : MonoBehaviour {

	private	static	EffectsManager			m_Instance								= null;
	public static EffectsManager			Instance
	{
		get { return m_Instance; }
	}

	[ SerializeField ]
	private		CustomAudioSource			m_ExplosionSource						= null;


	private		ParticleSystem[]			m_Effects			= null;


	private		Transform					m_ParticleEffects	= null;
	private		bool						m_IsOK				= true;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void	Awake()
	{
		// Singleton
		if ( m_Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		DontDestroyOnLoad( this );
		m_Instance = this;
		
		m_ParticleEffects = transform.Find("ParticleEffects");

		m_IsOK = m_ParticleEffects.PairComponentsInChildrenIntoArray<ParticleSystem, EffectType>( ref m_Effects );
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayEntityOnHit
	public	void	PlayEffect( EffectType effectType,  Vector3 position, Vector3 direction, int count = 0, float gameObjectLife = 5f )
	{
		if ( m_IsOK == false )
			return;

		ParticleSystem p = null;
		if ( count > 0 )
		{
			p = m_Effects[ (int) effectType ];
			{
				p.transform.position = position;
				p.transform.forward = direction;
			}
			p.Emit( count );
		}
		else
		{
			p = Instantiate( m_Effects[ (int) effectType ] );
			{
				p.transform.position = position;
				p.transform.forward = direction;
				p.Play( withChildren : true );
			}
			Destroy( p.gameObject, gameObjectLife );
		}
	}

	public	void	PlaceTracer( Vector3 startPosition, Vector3 endPosition )
	{

	}
	

	//////////////////////////////////////////////////////////////////////////
	// PlayerExplosionSound
	public	void	PlayExplosionSound( Vector3 position )
	{
		if ( m_ExplosionSource == null )
			return;

		m_ExplosionSource.transform.position = position;
		( m_ExplosionSource as ICustomAudioSource ).Play();
	}

}
