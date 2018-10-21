
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum EffectType {
	ENTITY_ON_HIT,
	AMBIENT_ON_HIT,
	EXPLOSION,
	ELETTRO,
	PLASMA
}


public class EffectManager : MonoBehaviour {

	public	static	EffectManager			Instance								= null;

	[ SerializeField ]
	private		ParticleSystem				m_ParticleSystemEntityOnHit				= null;

	[ SerializeField ]
	private		ParticleSystem				m_ParticleSystemAmbientOnHit			= null;

	[ SerializeField ]
	private		ParticleSystem				m_ParticleSystemElettroEffect			= null;

	[ SerializeField ]
	private		ParticleSystem				m_ParticleSystemPlasmaEffect			= null;

	[ SerializeField ]
	private		ParticleSystem				m_ParticleSystemBigExplosion			= null;

	[ SerializeField ]
	private		CustomAudioSource			m_ExplosionSource						= null;


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

		if ( m_ParticleSystemEntityOnHit			== null ||
			m_ParticleSystemAmbientOnHit			== null ||
			m_ParticleSystemBigExplosion			== null ||
			m_ParticleSystemElettroEffect			== null ||
			m_ParticleSystemPlasmaEffect			== null
		)
		return;
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayEntityOnHit
	public	void	PlayEntityOnHit( Vector3 position, Vector3 direction, int count = 3 )
	{
		m_ParticleSystemEntityOnHit.transform.position = position;
		m_ParticleSystemEntityOnHit.transform.forward = direction;
		m_ParticleSystemEntityOnHit.Emit( count );
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayAmbientOnHit
	public	void	PlayAmbientOnHit( Vector3 position, Vector3 direction, int count = 3 )
	{
		m_ParticleSystemAmbientOnHit.transform.position = position;
		m_ParticleSystemAmbientOnHit.transform.forward = direction;
		m_ParticleSystemAmbientOnHit.Emit( count );
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayAmbientOnHit
	public	void	PlayElettroEffect( Vector3 position, Vector3 direction, int count = 3 )
	{
		m_ParticleSystemElettroEffect.transform.position = position;
		m_ParticleSystemElettroEffect.transform.forward = direction;
		m_ParticleSystemElettroEffect.Emit( count );
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayAmbientOnHit
	public	void	PlayPlasmaEffect( Vector3 position, Vector3 direction, int count = 3 )
	{
		m_ParticleSystemPlasmaEffect.transform.position = position;
		m_ParticleSystemPlasmaEffect.transform.forward = direction;
		m_ParticleSystemPlasmaEffect.Emit( count );
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayEntityExplosion
	public	void	PlayEntityExplosion( Vector3 position, Vector3 direction )
	{
		ParticleSystem instantiated = Instantiate( m_ParticleSystemBigExplosion );
		instantiated.transform.position = position;
		instantiated.transform.forward = direction;
		instantiated.Play( withChildren : true );
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayerExplosionSound
	public	void	PlayExplosionSound( Vector3 position )
	{
		m_ExplosionSource.transform.position = position;
		( m_ExplosionSource as ICustomAudioSource ).Play();
	}

}
