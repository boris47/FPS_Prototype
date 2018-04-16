using UnityEngine;
using System.Collections;

public class EffectManager : MonoBehaviour {

	public	static	EffectManager	Instance						= null;

	[ SerializeField ]
	private		ParticleSystem		m_ParticleSystemEntityOnHit		= null;

	[ SerializeField ]
	private		ParticleSystem		m_ParticleSystemAmbientOnHit	= null;

	[ SerializeField ]
	private		ParticleSystem		m_ParticleSystemEntityExplosion	= null;


	[ SerializeField ]
	private		CustomAudioSource	m_ExplosionSource				= null;




	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void	Awake()
	{
		if ( m_ParticleSystemEntityOnHit == null || m_ParticleSystemAmbientOnHit == null || m_ParticleSystemEntityExplosion == null || m_ExplosionSource == null )
			return;

		if ( Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		Instance = this;
		DontDestroyOnLoad( this );
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
	// PlayEntityExplosion
	public	void	PlayEntityExplosion( Vector3 position, Vector3 direction )
	{
		m_ParticleSystemEntityExplosion.transform.position = position;
		m_ParticleSystemEntityExplosion.transform.forward = direction;
		m_ParticleSystemEntityExplosion.Play( withChildren : true );
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayerExplosionSound
	public	void	PlayerExplosionSound( Vector3 position )
	{
		m_ExplosionSource.transform.position = position;
		( m_ExplosionSource as ICustomAudioSource ).Play();
	}

}
