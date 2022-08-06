
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class SoundManager : GlobalMonoBehaviourSingleton<SoundManager>
{
	protected override void OnInitialize()
	{
		base.OnInitialize();

		SceneManager.sceneLoaded += OnSceneLoaded;

		LoadDatabase();
	}
}
