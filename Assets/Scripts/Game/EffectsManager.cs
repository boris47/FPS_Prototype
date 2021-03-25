﻿
using UnityEngine;

public sealed class EffectsManager : MonoBehaviourSingleton<EffectsManager>
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


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		CustomAssertions.IsNotNull(m_ParticleEffects = transform.Find("ParticleEffects"));
		CustomAssertions.IsNotNull(m_AudioSources = transform.Find("AudioSources"));
		CustomAssertions.IsTrue(m_ParticleEffects.MapComponentsInChildrenToArray<ParticleSystem, EEffecs>(out m_Effects));
		CustomAssertions.IsTrue(m_AudioSources.MapComponentsInChildrenToArray<CustomAudioSource, EEffecs>(out m_CustomAudioSource));
	}


	//////////////////////////////////////////////////////////////////////////
	public void PlayEffect(EEffecs effectType, Vector3 position, Vector3 direction, int? count = 0, float gameObjectLife = 5f)
	{
		if (count.HasValue)
		{
			ParticleSystem p = m_Effects[(int)effectType];
			{
				p.transform.position = position;
				p.transform.forward = direction;
			}
			p.Emit(count.Value);
		}
		else
		{
			ParticleSystem p = Instantiate(m_Effects[(int)effectType]);
			{
				p.transform.position = position;
				p.transform.forward = direction;
				p.Play(withChildren: true);
			}
			Destroy(p.gameObject, gameObjectLife);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void PlaceTracer(Vector3 startPosition, Vector3 endPosition)
	{

	}


	//////////////////////////////////////////////////////////////////////////
	public void PlayExplosionSound(EEffecs effectType, Vector3 position)
	{
		CustomAudioSource source = m_CustomAudioSource[(int)effectType];
		if (source)
		{
			source.transform.position = position;
			source.Play();
		}

		//this.m_ExplosionSource.transform.position = position;
		//(this.m_ExplosionSource as ICustomAudioSource ).Play();
	}

}
