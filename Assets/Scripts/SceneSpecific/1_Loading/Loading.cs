using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : InGameSingleton<Loading>
{
	private Slider m_LoadingBar = null;
	private Text m_LoadingLevelNameText = null;
	private Text m_LoadingSubTaskText = null;
	private float m_CurrentProgressValue = 0.0f;

	private bool m_IsInitializedInternal = false;

	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		gameObject.SetActive(false);

		m_IsInitializedInternal = transform.TrySearchComponent(ESearchContext.CHILDREN, out m_LoadingBar);
		m_IsInitializedInternal &= transform.TrySearchComponent(ESearchContext.CHILDREN, out m_LoadingLevelNameText, child => child.name == "LoadingSceneName");
		m_IsInitializedInternal &= transform.TrySearchComponent(ESearchContext.CHILDREN, out m_LoadingSubTaskText, child => child.name == "LoadingSubTask");

		if (!m_IsInitializedInternal)
		{
			Debug.LogError("Loading Singleton has initialization issues");
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = UnityEditor.EditorApplication.isPaused = false;
#endif
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public static void Show()
	{
		ResetBar();

		Instance.gameObject.SetActive(true);
	}



	//////////////////////////////////////////////////////////////////////////
	public static void Hide()
	{
		ResetBar();

		Instance.gameObject.SetActive(false);
	}


	//////////////////////////////////////////////////////////////////////////
	private static void ResetBar()
	{
		Instance.m_CurrentProgressValue = 0.0f;
		Instance.m_LoadingBar.value = 0.0f;
	}



	//////////////////////////////////////////////////////////////////////////
	public static void SetLoadingSceneName(ESceneEnumeration scene)
	{
		Instance.m_LoadingLevelNameText.text = $"Loading: {scene.ToString()}";
	}

	private static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
	//////////////////////////////////////////////////////////////////////////
	public static void EndSubTask()
	{
		stopWatch.Stop();
		Debug.Log($"Step '{Instance.m_LoadingSubTaskText.text}' required {stopWatch.ElapsedMilliseconds}ms.");
	}


	//////////////////////////////////////////////////////////////////////////
	public static void SetSubTask(string subTaskName)
	{
		stopWatch.Reset(); stopWatch.Start();
		Instance.m_LoadingSubTaskText.text = subTaskName;
	//	Debug.Log(subTaskName);
	}



	//////////////////////////////////////////////////////////////////////////
	public static void SetProgress(float CurrentProgress)
	{
		Instance.m_CurrentProgressValue = Mathf.Clamp01(CurrentProgress);
	}



	//////////////////////////////////////////////////////////////////////////
	private void LateUpdate()
	{
		Instance.m_LoadingBar.value = Mathf.MoveTowards(Instance.m_LoadingBar.value, Instance.m_CurrentProgressValue, Time.unscaledDeltaTime);
	}

}
