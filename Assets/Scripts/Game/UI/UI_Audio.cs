using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Audio : MonoBehaviour {


	//////////////////////////////////////////////////////////////////////////
	// OnMusicVolumeSet
	public	void	OnMusicVolumeSet( float value )
	{
		SoundManager.Instance.MusicVolume = value;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSoundsVolumeSet
	public	void	OnSoundsVolumeSet( float value )
	{
		SoundManager.Instance.SoundVolume = value;
	}
}
