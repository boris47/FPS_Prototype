﻿
using UnityEngine;


public		delegate	void	OnValueChange( float value );

public class SoundManager : MonoBehaviourSingleton<SoundManager>
{
	private static	event		System.Action<bool>		m_OnPauseSet			= delegate { };
	private	static	event		OnValueChange			m_OnMusicVolumeChange	= delegate { };
	private	static	event		OnValueChange			m_OnSoundVolumeChange	= delegate { };
	private	static	event		OnValueChange			m_OnPitchChange			= delegate { };

	public static	event		System.Action<bool>		OnPauseSet
	{
		add		{ if (value.IsNotNull()) m_OnPauseSet += value; }
		remove	{ if (value.IsNotNull()) m_OnPauseSet -= value; }
	}

	public	static	event		OnValueChange			OnMusicVolumeChange
	{
		add		{ if (value.IsNotNull()) m_OnMusicVolumeChange += value; }
		remove	{ if (value.IsNotNull()) m_OnMusicVolumeChange -= value; }
	}

	public	static	event		OnValueChange			OnSoundVolumeChange
	{
		add		{ if (value.IsNotNull()) m_OnSoundVolumeChange += value; }
		remove	{ if (value.IsNotNull()) m_OnSoundVolumeChange -= value; }
	}

	public	static	event		OnValueChange			OnPitchChange
	{
		add		{ if (value.IsNotNull()) m_OnPitchChange += value; }
		remove	{ if (value.IsNotNull()) m_OnPitchChange -= value; }
	}

	// EDITOR ONLY
	[SerializeField, ReadOnly]
	private			bool		m_IsPaused			= false;

	[SerializeField, ReadOnly]
	private			float		m_MusicVolume		= 1f;

	[SerializeField, ReadOnly]
	private			float		m_SoundVolume		= 1f;

	[SerializeField, ReadOnly]
	private			float		m_Pitch				= 1f;


	//------------------------------------------------------------
	public static bool IsPaused
	{
		get => m_Instance.m_IsPaused;
		set
		{
			UpdatePauseState(value);
		}
	}

	//------------------------------------------------------------
	public static float MusicVolume
	{
		get => m_Instance.m_MusicVolume;
		set
		{
			UpdateMusicVolume(value);
		}
	}

	//------------------------------------------------------------
	public static float SoundVolume
	{
		get => m_Instance.m_SoundVolume;
		set
		{
			UpdateSoundVolume(value);
		}
	}

	//------------------------------------------------------------
	public static float Pitch
	{
		get => m_Instance.m_Pitch;
		set
		{
			UpdatePitch(value);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private static void UpdatePauseState(bool value)
	{
		m_Instance.m_IsPaused = value;

		m_OnPauseSet(value);
	}

	//////////////////////////////////////////////////////////////////////////
	private static void UpdateMusicVolume(float value)
	{
		m_Instance.m_MusicVolume = value;

		m_OnMusicVolumeChange(value);
	}


	//////////////////////////////////////////////////////////////////////////
	private static void UpdateSoundVolume(float value)
	{
		m_Instance.m_SoundVolume = value;

		m_OnSoundVolumeChange(value);
	}


	//////////////////////////////////////////////////////////////////////////
	private static void UpdatePitch(float value)
	{
		m_Instance.m_Pitch = value;

		m_OnPitchChange(value);
	}


	//////////////////////////////////////////////////////////////////////////
	public static void OnSceneLoaded()
	{
		m_OnMusicVolumeChange(m_Instance.m_MusicVolume);
		m_OnSoundVolumeChange(m_Instance.m_SoundVolume);
		m_OnPitchChange(m_Instance.m_Pitch);
	}
}
