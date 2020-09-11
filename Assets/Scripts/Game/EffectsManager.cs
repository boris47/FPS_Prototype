
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class EffectsManager : SingletonMonoBehaviour<EffectsManager>
{
	public enum EEffecs
	{
		ENTITY_ON_HIT
		, AMBIENT_ON_HIT
		, ELETTRO
		, PLASMA
		, EXPLOSION

		, MUZZLE
		, SMOKE

//		, TRACER
	};

	[SerializeField, ReadOnly]
	private		Transform					m_ParticleEffects	= null;
	[SerializeField, ReadOnly]
	private		Transform					m_AudioSources		= null;

	[SerializeField, ReadOnly]
	private		ParticleSystem[]			m_Effects			= null;
	[SerializeField, ReadOnly]
	private		CustomAudioSource[]			m_CustomAudioSource	= null;

	private		bool						m_IsOK				= true;


	//////////////////////////////////////////////////////////////////////////
	private void	Awake()
	{
		this.m_ParticleEffects = this.transform.Find("ParticleEffects");
		this.m_AudioSources = this.transform.Find("AudioSources");

		this.m_IsOK &= this.m_ParticleEffects.MapComponentsInChildrenToArray<ParticleSystem, EEffecs>( ref this.m_Effects );
		this.m_IsOK &= this.m_AudioSources.MapComponentsInChildrenToArray<CustomAudioSource, EEffecs>( ref this.m_CustomAudioSource );
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	PlayEffect( EEffecs effectType,  Vector3 position, Vector3 direction, int count = 0, float gameObjectLife = 5f )
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


	//////////////////////////////////////////////////////////////////////////
	public	void	PlaceTracer( Vector3 startPosition, Vector3 endPosition )
	{

	}
	

	//////////////////////////////////////////////////////////////////////////
	public	void	PlayExplosionSound( EEffecs effectType, Vector3 position )
	{
		CustomAudioSource source = this.m_CustomAudioSource[(int) effectType];
		if (source)
		{
			source.transform.position = position;
			source.Play();
		}

//		this.m_ExplosionSource.transform.position = position;
//		(this.m_ExplosionSource as ICustomAudioSource ).Play();
	}

}
