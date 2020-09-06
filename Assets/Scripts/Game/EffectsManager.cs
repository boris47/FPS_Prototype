
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EEffectType {
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
			Destroy(this.gameObject );
			return;
		}
		DontDestroyOnLoad( this );
		m_Instance = this;

		this.m_ParticleEffects = this.transform.Find("ParticleEffects");

		this.m_IsOK = this.m_ParticleEffects.PairComponentsInChildrenIntoArray<ParticleSystem, EEffectType>( ref this.m_Effects );
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayEntityOnHit
	public	void	PlayEffect( EEffectType effectType,  Vector3 position, Vector3 direction, int count = 0, float gameObjectLife = 5f )
	{
		if (this.m_IsOK == false )
			return;

		ParticleSystem p = null;
		if ( count > 0 )
		{
			p = this.m_Effects[ (int) effectType ];
			{
				p.transform.position = position;
				p.transform.forward = direction;
			}
			p.Emit( count );
		}
		else
		{
			p = Instantiate(this.m_Effects[ (int) effectType ] );
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
		if (this.m_ExplosionSource == null )
			return;

		this.m_ExplosionSource.transform.position = position;
		(this.m_ExplosionSource as ICustomAudioSource ).Play();
	}

}
