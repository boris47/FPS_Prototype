
using System.Collections.Generic;
using UnityEngine;

public class SoundsDatabase : ScriptableObject
{
	[System.Serializable]
	public class SoundsDatabaseItem
	{
		public AudioClip AudioClip = null;
		public ESoundType SoundType = default;
	}

	[SerializeField]
	public List<SoundsDatabaseItem> SoundResourceItems = new List<SoundsDatabaseItem>();

}
