
using UnityEngine;
using UnityEngine.SceneManagement;
using Entities;

[System.Serializable]
public enum ESoundType
{
	WEAPON,
	STEP,
	OBJECT
}

public partial class SoundManager
{
	public delegate void OnSoundPlayDel(in Entity source, in ESoundType soundType, in Vector3 worldPosition);

	private static	event		System.Action<bool>		m_OnPauseSet			= delegate { };
	private	static	event		System.Action<float>	m_OnMusicVolumeChange	= delegate { };
	private	static	event		System.Action<float>	m_OnSoundVolumeChange	= delegate { };
	private	static	event		System.Action<float>	m_OnPitchChange			= delegate { };
	private	static	event		OnSoundPlayDel			m_OnSoundPlay			= delegate { };

	public static	event		System.Action<bool>		OnPauseSet
	{
		add		{ if (value.IsNotNull()) m_OnPauseSet += value; }
		remove	{ if (value.IsNotNull()) m_OnPauseSet -= value; }
	}

	public	static	event		System.Action<float>	OnMusicVolumeChange
	{
		add		{ if (value.IsNotNull()) m_OnMusicVolumeChange += value; }
		remove	{ if (value.IsNotNull()) m_OnMusicVolumeChange -= value; }
	}

	public	static	event		System.Action<float>	OnSoundVolumeChange
	{
		add		{ if (value.IsNotNull()) m_OnSoundVolumeChange += value; }
		remove	{ if (value.IsNotNull()) m_OnSoundVolumeChange -= value; }
	}

	public	static	event		System.Action<float>	OnPitchChange
	{
		add		{ if (value.IsNotNull()) m_OnPitchChange += value; }
		remove	{ if (value.IsNotNull()) m_OnPitchChange -= value; }
	}

	public	static	event		OnSoundPlayDel			OnSoundPlay
	{
		add		{ if (value.IsNotNull()) m_OnSoundPlay += value; }
		remove	{ if (value.IsNotNull()) m_OnSoundPlay -= value; }
	}

	[SerializeField, ReadOnly]
	private 		SoundsDatabase		m_Database				= null;

	[SerializeField, ReadOnly]
	private			float				m_MusicVolume			= 1f;

	[SerializeField, ReadOnly]
	private			float				m_SoundVolume			= 1f;

	[SerializeField, ReadOnly]
	private			float				m_Pitch					= 1f;

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

	private	void LoadDatabase()
	{
		m_Database = Resources.Load<SoundsDatabase>("Sounds/SoundsDatabase");
		Utils.CustomAssertions.IsNotNull(m_Database);
	}

	//////////////////////////////////////////////////////////////////////////
	public static void UpdatePauseState(bool value)
	{
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
	public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		m_OnMusicVolumeChange(m_Instance.m_MusicVolume);
		m_OnSoundVolumeChange(m_Instance.m_SoundVolume);
		m_OnPitchChange(m_Instance.m_Pitch);
	}

	//////////////////////////////////////////////////////////////////////////
	public void PlaySound(in Entity source, in AudioSource audioSource, AudioClip audioClip)
	{
		var item = m_Database.SoundResourceItems.Find(i => i.AudioClip == audioClip);
		if (item.IsNotNull())
		{
			m_OnSoundPlay(source, item.SoundType, audioSource.transform.position);
		}

		audioSource.PlayOneShot(audioClip);
	}
}
