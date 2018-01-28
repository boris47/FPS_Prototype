
using UnityEngine;

/// <summary> Provides an easy wrapper to looping audio sources with nice transitions for volume when starting and stopping </summary>
public class LoopingAudioSource {

	public AudioSource AudioSource
	{
		get;
		set;
	}
	public float TargetVolume
	{
		get;
		private set;
	}


	//////////////////////////////////////////////////////////////////////////
	// CONSTRUCTOR
	public LoopingAudioSource()
	{
		TargetVolume = 1.0f;
	}


	//////////////////////////////////////////////////////////////////////////
	// SetVolume
	public void SetVolume( float targetVolume )
	{
		if ( AudioSource == null )
			return;

		if ( !AudioSource.isPlaying )
		{
			AudioSource.volume = 0.0f;
			AudioSource.Play();
		}
		TargetVolume = targetVolume;
	}


	//////////////////////////////////////////////////////////////////////////
	// Silence
	public void Silence()
	{
		TargetVolume = 0.0f;
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	public void Update()
	{
		if ( AudioSource == null )
			return;

		if ( AudioSource.isPlaying && ( AudioSource.volume = Mathf.Lerp( AudioSource.volume, TargetVolume, Time.deltaTime * 2f ) ) == 0.0f )
		{
			AudioSource.Stop();

		}
	}

}