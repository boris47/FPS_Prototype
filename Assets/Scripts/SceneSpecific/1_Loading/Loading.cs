using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
	public	static	Loading		Instance						{ get; private set;} = null;
	private	static	Stopwatch	m_StopWatch						= new Stopwatch();

	private			Slider		m_LoadingBar					= null;
	private			Text		m_LoadingLevelNameText			= null;
	private			Text		m_LoadingSubTaskText			= null;
	private			float		m_CurrentProgressValue			= 0.0f;
	private			bool		m_IsInitializedInternal			= false;


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		Instance = this;

		gameObject.SetActive(false);

		m_IsInitializedInternal =  transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_LoadingBar);
		m_IsInitializedInternal &= transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_LoadingLevelNameText, child => child.name == "LoadingSceneName");
		m_IsInitializedInternal &= transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_LoadingSubTaskText, child => child.name == "LoadingSubTask");

		if (!m_IsInitializedInternal)
		{
			UnityEngine.Debug.LogError("Loading Singleton has initialization issues");
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = UnityEditor.EditorApplication.isPaused = false;
#endif
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void LateUpdate()
	{
		m_LoadingBar.value = Mathf.MoveTowards(m_LoadingBar.value, m_CurrentProgressValue, Time.unscaledDeltaTime);
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		Instance = null;
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


	//////////////////////////////////////////////////////////////////////////
	public static void EndSubTask()
	{
		m_StopWatch.Stop();
	//	UnityEngine.Debug.Log($"Step '{Instance.m_LoadingSubTaskText.text}' required {m_StopWatch.ElapsedMilliseconds}ms.");
	}


	//////////////////////////////////////////////////////////////////////////
	public static void SetSubTask(string subTaskName)
	{
		m_StopWatch.Reset(); m_StopWatch.Start();
		Instance.m_LoadingSubTaskText.text = subTaskName;
	}


	//////////////////////////////////////////////////////////////////////////
	public static void SetProgress(float CurrentProgress)
	{
		Instance.m_CurrentProgressValue = Mathf.Clamp01(CurrentProgress);
	}
}
