using UnityEngine;
using System.Collections;

public class EffectManager : MonoBehaviour {

	public	static	EffectManager	Instance								= null;

	[ SerializeField ]
	private		ParticleSystem		m_ParticleSystemEntityOnHit				= null;

	[ SerializeField ]
	private		ParticleSystem		m_ParticleSystemAmbientOnHit			= null;

	[ SerializeField ]
	private		GameObject			m_ExplosionParticleSystemsCollection	= null;

	[ SerializeField ]
	private		CustomAudioSource	m_ExplosionSource						= null;

	private		ParticleSystem[]	m_ExplosionParticleSystems				= null;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void	Awake()
	{
		if ( m_ParticleSystemEntityOnHit == null || m_ParticleSystemAmbientOnHit == null || m_ExplosionParticleSystemsCollection == null || m_ExplosionSource == null )
			return;

		if ( Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		Instance = this;
		DontDestroyOnLoad( this );

		m_ExplosionParticleSystems = m_ExplosionParticleSystemsCollection.GetComponentsInChildren<ParticleSystem>();
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayEntityOnHit
	public	void	PlayEntityOnHit( Vector3 position, Vector3 direction )
	{
		m_ParticleSystemEntityOnHit.transform.position = position;
		m_ParticleSystemEntityOnHit.transform.forward = direction;
		m_ParticleSystemEntityOnHit.Emit( 3 );
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayAmbientOnHit
	public	void	PlayAmbientOnHit( Vector3 position, Vector3 direction )
	{
		m_ParticleSystemAmbientOnHit.transform.position = position;
		m_ParticleSystemAmbientOnHit.transform.forward = direction;
		m_ParticleSystemAmbientOnHit.Emit( 3 );
	}


	//////////////////////////////////////////////////////////////////////////
	// GetFreeParticleSystem
	private	ParticleSystem	GetFreeParticleSystem()
	{
		foreach( ParticleSystem ps in m_ExplosionParticleSystems )
		{
			if ( ps.isPlaying )
				continue;

			return ps;
		}
		return null;
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayEntityExplosion
	public	void	PlayEntityExplosion( Vector3 position, Vector3 direction )
	{
		ParticleSystem ps =	GetFreeParticleSystem();
		if ( ps == null )
			return;

		ps.transform.position = position;
		ps.transform.forward = direction;
		ps.Play( withChildren : true );
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayerExplosionSound
	public	void	PlayerExplosionSound( Vector3 position )
	{
		m_ExplosionSource.transform.position = position;
		( m_ExplosionSource as ICustomAudioSource ).Play();
	}

}
