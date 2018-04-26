
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum EffectType {
	SHOCK
}


public class EffectManager : MonoBehaviour {

	public	static	EffectManager			Instance								= null;

	[ SerializeField ]
	private		ParticleSystem				m_ParticleSystemEntityOnHit				= null;

	[ SerializeField ]
	private		ParticleSystem				m_ParticleSystemAmbientOnHit			= null;

	[ SerializeField ]
	private		Transform					m_ExplosionParticleSystemsCollection	= null;

	[ SerializeField ]
	private		Transform					m_ElettroParticleSystemsCollection		= null;


	[ SerializeField ]
	private		CustomAudioSource			m_ExplosionSource						= null;

	private		ParticleSystem[]			m_ExplosionParticleSystems				= null;
	private		ParticleSystem[]			m_ElettroParticleSystems				= null;


	private struct LongParticleSystemData {
		public	ParticleSystem	ps;
		public	Transform		target;
	}

	private	List<LongParticleSystemData>	m_ActiveParticleSystems					= new List<LongParticleSystemData>();


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
			m_ExplosionParticleSystemsCollection	== null ||
			m_ExplosionSource						== null ||
			m_ElettroParticleSystemsCollection		== null
		)
		return;


		m_ExplosionParticleSystems	= m_ExplosionParticleSystemsCollection.GetComponentsInChildren<ParticleSystem>();
		m_ElettroParticleSystems	= m_ElettroParticleSystemsCollection.GetComponentsInChildren<ParticleSystem>();
	}


	//////////////////////////////////////////////////////////////////////////
	// AttachAndPlay
	public	ParticleSystem	AttachAndPlay( IEnumerable<ParticleSystem> collection, Transform target, int particleCount = -1 )
	{
		ParticleSystem ps = GetFreeParticleSystem( collection );
		if ( ps == null )
			return null;

		if ( particleCount > 0 )
		{
			ps.Emit( particleCount );
		}
		else
		{
			ps.Play( withChildren : true );
		}

		m_ActiveParticleSystems.Add( new LongParticleSystemData() { ps = ps, target = target } );
		return ps;
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
	// PlayElettroHit
	public	ParticleSystem	PlayElettroHit( Transform target )
	{
		return AttachAndPlay( m_ElettroParticleSystems, target );
	}


	//////////////////////////////////////////////////////////////////////////
	// GetFreeParticleSystem
	private	ParticleSystem	GetFreeParticleSystem( IEnumerable<ParticleSystem> collection )
	{
		foreach( var ps in collection )
		{
			if ( ps.isPlaying )
				continue;
			return ps;
		}
		print( "EffectManager::GetFreeParticleSystem: Cannot find a valid particle system !!" );
		return null;
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayEntityExplosion
	public	void	PlayEntityExplosion( Vector3 position, Vector3 direction )
	{
		ParticleSystem ps =	GetFreeParticleSystem( m_ExplosionParticleSystems );
		if ( ps == null )
			return;

		ps.transform.position = position;
		ps.transform.forward = direction;
		ps.Play( withChildren : true );
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayerExplosionSound
	public	void	PlayExplosionSound( Vector3 position )
	{
		m_ExplosionSource.transform.position = position;
		( m_ExplosionSource as ICustomAudioSource ).Play();
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private	void	Update()
	{
		for ( int i = 0; i < m_ActiveParticleSystems.Count; i++ )
		{
			LongParticleSystemData pair = m_ActiveParticleSystems[ i ];
			pair.ps.transform.position = pair.target.position;

			if ( pair.ps.IsAlive() == false )
			{
				pair.ps.transform.localPosition = Vector3.zero;
				m_ActiveParticleSystems.RemoveAt( i );
				return;
			}
		}
	}

}
