using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is present only once in loading scene
/// </summary>
public class Loading : MonoBehaviour
{
	[SerializeField]
	private			Slider		m_LoadingBar					= null;
	[SerializeField]
	private			Text		m_LoadingLevelNameText			= null;
	[SerializeField]
	private			Text		m_LoadingSubTaskText			= null;

//	private			Stopwatch	m_StopWatch						= new Stopwatch();
	private			float		m_CurrentProgressValue			= 0.0f;
//	private			bool		m_IsInitializedInternal			= false;

	/*
	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		gameObject.SetActive(false);

		bool bIsInitializedInternal = m_LoadingBar.IsNotNull() && m_LoadingLevelNameText.IsNotNull() && m_LoadingSubTaskText.IsNotNull();
		if (!m_IsInitializedInternal)
		{
			UnityEngine.Debug.LogError("Loading script has initialization issues");
#if UNITY_EDITOR
			UnityEditor.EditorApplication.ExitPlaymode();
#endif
		}
	}
	*/
	//////////////////////////////////////////////////////////////////////////
	private void LateUpdate()
	{
		m_LoadingBar.value = Mathf.MoveTowards(m_LoadingBar.value, m_CurrentProgressValue, Time.unscaledDeltaTime);
	}

	//////////////////////////////////////////////////////////////////////////
	public void Show()
	{
		ResetBar();

		gameObject.SetActive(true);
	}

	//////////////////////////////////////////////////////////////////////////
	public void Hide()
	{
		ResetBar();

		gameObject.SetActive(false);
	}

	//////////////////////////////////////////////////////////////////////////
	private void ResetBar()
	{
		m_CurrentProgressValue = 0.0f;
		m_LoadingBar.value = 0.0f;
	}

	//////////////////////////////////////////////////////////////////////////
	public void SetLoadingSceneName(string scene)
	{
		m_LoadingLevelNameText.text = $"Loading: {scene}";
	}

	//////////////////////////////////////////////////////////////////////////
	public void EndSubTask()
	{
//		m_StopWatch.Stop();
	//	UnityEngine.Debug.Log($"Step '{Instance.m_LoadingSubTaskText.text}' required {m_StopWatch.ElapsedMilliseconds}ms.");
	}

	//////////////////////////////////////////////////////////////////////////
	public void SetSubTask(string subTaskName)
	{
//		m_StopWatch.Reset(); m_StopWatch.Start();
		m_LoadingSubTaskText.text = subTaskName;
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Accept range 0f..1f </summary>
	public void SetProgress(float CurrentProgress)
	{
		m_CurrentProgressValue = Mathf.Clamp01(CurrentProgress);
	}
}
