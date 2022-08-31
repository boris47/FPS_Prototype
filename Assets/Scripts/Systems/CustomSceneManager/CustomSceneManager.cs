using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


[System.Serializable]
public enum ESceneAction : uint
{
	NONE		= 0,
	NEXT		= 1,
	PREVIOUS	= 2,
}


public class CustomSceneManager : GlobalMonoBehaviourSingleton<CustomSceneManager>
{
	[SerializeField]
	private SceneDefinitionCollection m_SceneDefinitionCollection = null;

	[SerializeField]
	private Loading m_LoadingPrefab = null;

	[SerializeField, ReadOnly]
	private SceneDefinition m_CurrentSceneDefinition = null;

	private static event System.Action s_BeforeSceneChange = delegate { };
	public static event System.Action BeforeSceneChange
	{
		add		{ if (value.IsNotNull()) s_BeforeSceneChange += value; }
		remove	{ if (value.IsNotNull()) s_BeforeSceneChange -= value; }
	}

	public static System.Func<string, IEnumerator> SaveSystemLoadSave { get; } = delegate(string _) { return null; };

	private static event System.Action s_AfterSceneChange = delegate { };
	public static event System.Action AfterSceneChange
	{
		add		{ if (value.IsNotNull()) s_AfterSceneChange += value; }
		remove	{ if (value.IsNotNull()) s_AfterSceneChange -= value; }
	}

	private Loading m_LoadingView = null;
	private Scene m_LoadingScene = default;


	//////////////////////////////////////////////////////////////////////////
	protected override void OnAfterSceneLoad()
	{
		base.OnAfterSceneLoad();

		if (Utils.CustomAssertions.IsNotNull(m_LoadingPrefab, this, $"{nameof(m_LoadingPrefab)} is null"))
		{
			Scene currentActiveScene = SceneManager.GetActiveScene();
			{
				m_LoadingScene = SceneManager.CreateScene("Loading", new CreateSceneParameters(LocalPhysicsMode.None));
				SceneManager.SetActiveScene(m_LoadingScene);
				m_LoadingView = Instantiate(m_LoadingPrefab);
				Utils.CustomAssertions.IsNotNull(m_LoadingView, this);
			}
			SceneManager.SetActiveScene(currentActiveScene);

			// If more than one scene definition has been found
			if (Utils.CustomAssertions.IsNotNull(m_SceneDefinitionCollection, this, "DefinitionCollection is null") &&
				Utils.CustomAssertions.IsTrue(m_SceneDefinitionCollection.SceneDefinitions.IsValidIndex(0), this, "There must be at least one scene definition"))
			{
				// Get current active scene path
				string currentScenePath = currentActiveScene.path;
				
				// Retrieve its definition
				SceneDefinition currentSceneDefinition = m_SceneDefinitionCollection.SceneDefinitions.FirstOrDefault(def => def.ScenePath == currentScenePath);
				if (Utils.CustomAssertions.IsNotNull(currentSceneDefinition, this, $"Cannot retrieve scene definition for loaded scene {currentScenePath}"))
				{
					m_CurrentSceneDefinition = currentSceneDefinition;
				}
			}
		}
	}


	/////////////////////////////////////////////////////////////////
	/// <summary> Return true if the current scene index is a play scene index </summary>
	public static bool IsGameScene() => IsGameScene(m_Instance.m_CurrentSceneDefinition);
	

	/////////////////////////////////////////////////////////////////
	/// <summary> Return true if the given scene index is a play scene index </summary>
	public static bool IsGameScene(in SceneDefinition InSceneDefinition) => InSceneDefinition.IsGameScene;


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Launch load of a scene asynchronously </summary>
	public static void LoadSceneAsync(in SceneDefinition InSceneDefinition, string InSaveFile = null)
	{
		CoroutinesManager.Start(m_Instance.LoadSceneAsyncCO(InSceneDefinition, InSaveFile), $"{nameof(CustomSceneManager)}::{nameof(LoadSceneAsync)}: Loading {InSceneDefinition.SceneName}");
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Start the unload of a scene, return the coroutine managing the async operation </summary>
	public static IEnumerator UnLoadSceneAsync(in SceneDefinition InSceneDefinition)
	{
		return UnLoadSceneAsync(InSceneDefinition.ScenePath);
	}

	/// <summary> Start the unload of a scene, return the coroutine managing the async operation </summary>
	public static IEnumerator UnLoadSceneAsync(in string InScenePath)
	{
		IEnumerator enumerator = null;
		Scene scene = SceneManager.GetSceneByPath(InScenePath);
		if (scene.isLoaded)
		{
			enumerator = m_Instance.UnLoadSceneAsyncCO(scene);
			CoroutinesManager.Start(enumerator, $"{nameof(CustomSceneManager)}::{nameof(UnLoadSceneAsync)}:Async unload of {scene.name}");
			return enumerator;
		}
		return enumerator;

	}




	//////////////////////////////////////////////////////////////////////////
	/// <summary> Internal coroutine that load a scene asynchronously </summary>
	private IEnumerator LoadSceneAsyncCO(SceneDefinition InSceneDefinition, string InSaveFile = null)
	{
		System.Diagnostics.Stopwatch loadWatch = new System.Diagnostics.Stopwatch();
		loadWatch.Start();

		// Enable Loading UI
		m_LoadingView.SetLoadingSceneName(InSceneDefinition.SceneName);
		m_LoadingView.Show();
		{
			m_LoadingView.SetSubTask($"Preparing to load '{InSceneDefinition.SceneName}'");
			{
				s_BeforeSceneChange();

				// Wait for every coroutine
				yield return CoroutinesManager.WaitPendingCoroutines();

				// Set Loading Scene as currently active one
				SceneManager.SetActiveScene(m_LoadingScene);
			}
			m_LoadingView.EndSubTask();

			m_LoadingView.SetSubTask($"Unloading '{m_CurrentSceneDefinition.SceneName}'");
			{
				// First unload non-shared sub-scenes
				var scenesToUnload = m_CurrentSceneDefinition.SubScenes.Except(InSceneDefinition.SubScenes).ToList();
				foreach (SceneReference subScene in scenesToUnload)
				{
					yield return UnLoadSceneAsync(subScene);
				}

				// Unload master level
				yield return UnLoadSceneAsync(m_CurrentSceneDefinition);
			}
			m_LoadingView.EndSubTask();

			List<AsyncOperation> loadSceneAsyncOperations = new List<AsyncOperation>();
			m_LoadingView.SetSubTask($"Start load of {InSceneDefinition.SceneName}");
			{
				// Start async load of master scene
				AsyncOperation loadSceneAsyncOperation = SceneManager.LoadSceneAsync(InSceneDefinition.ScenePath, LoadSceneMode.Additive); // Forcing additive because the loading scene

				// We want this operation to impact performance less than possible
				loadSceneAsyncOperation.priority = 0;
				loadSceneAsyncOperation.allowSceneActivation = false;
				loadSceneAsyncOperations.Add(loadSceneAsyncOperation);

				// Sub scenes
				foreach (SceneReference subScene in InSceneDefinition.SubScenes.Except(m_CurrentSceneDefinition.SubScenes))
				{
					AsyncOperation loadSubSceneAsyncOperation = SceneManager.LoadSceneAsync(subScene.ScenePath, LoadSceneMode.Additive); // Forcing additive because the loading scene
					loadSubSceneAsyncOperation.priority = 1;
					loadSubSceneAsyncOperation.allowSceneActivation = false;
					loadSceneAsyncOperations.Add(loadSubSceneAsyncOperation);
				}

				const float sceneLoadInfluence = 0.8f;
				float count = (float)(InSceneDefinition.SubScenes.Length + 1);

				// While loading set progress
				while (!loadSceneAsyncOperations.All(asyncOperation => asyncOperation.progress >= 0.9f))
				{
					float totalLoadingPercentagle = loadSceneAsyncOperations.Sum(ap => ap.progress) / count; // subScenes + master

					m_LoadingView.SetProgress(totalLoadingPercentagle * sceneLoadInfluence); // Loading of the scenes influence by XX% the total loading percentage
					yield return null;
				}

		//		yield return new WaitForSecondsRealtime(1f);

				foreach (AsyncOperation op in loadSceneAsyncOperations)
				{
					op.allowSceneActivation = true;
				//	yield return null;			// !!! At this moment it works, no need to activate frame by frame
				}

				while(!loadSceneAsyncOperations.All(asyncOperation => asyncOperation.isDone))
				{
					yield return null;
				}

				m_CurrentSceneDefinition = InSceneDefinition;
			}
			m_LoadingView.EndSubTask();

			m_LoadingView.SetSubTask("Activation");
			{
				// Ensure that loaded scene is also the active one
				SceneManager.SetActiveScene(InSceneDefinition.SceneRef);

				// Wait for every launched coroutine
				m_LoadingView.SetProgress(0.70f);
				yield return CoroutinesManager.WaitPendingCoroutines();
			}
			m_LoadingView.EndSubTask();

			if (!string.IsNullOrEmpty(InSaveFile))
			{
				Utils.CustomAssertions.IsTrue(SaveSystemLoadSave.IsNotNull());
				// Prevent the scene from to continue running
				// at scope exit restore scene continuum
				using (new SceneFreezer_Multi())
				{
					m_LoadingView.SetProgress(0.9f);
					m_LoadingView.SetSubTask("Loading of saved data");
					{
						yield return SaveSystemLoadSave(InSaveFile);
					}
					m_LoadingView.EndSubTask();
				}
			}

	//		// load save if given
	//		if (!string.IsNullOrEmpty(InSaveFile))
	//		{
	//			// Prevent the scene from to continue running
	//			// at scope exit restore scene continuum
	//			using (new SceneFreezer(null))
	//			{
	//				m_LoadingView.SetProgress(0.95f);
	//				m_LoadingView.SetSubTask("Load of saved data");
	//				{
	//					yield return SaveSystem.ApplySave(InSaveFile);
	//				}
	//				m_LoadingView.EndSubTask();
	//			}
	//		}

			m_LoadingView.SetProgress(1.00f);
		}

		m_LoadingView.Hide();
		loadWatch.Stop();
		Debug.LogFormat("Loading {0} took {1}ms.", InSceneDefinition.SceneName, loadWatch.ElapsedMilliseconds);
	}


	/////////////////////////////////////////////////////////////////
	private IEnumerator UnLoadSceneAsyncCO(Scene scene)
	{
		AsyncOperation operation = SceneManager.UnloadSceneAsync( scene );

		// We want this operation to impact performance less than possible
		operation.priority = 0;

		yield return operation;
	}



	private class SceneFreezer_Single : System.IDisposable
	{
		private readonly float m_PreviousFixedDeltaTime = 0f;
		private readonly float m_PreviousTimeScale = 0f;
		private readonly Scene m_Scene = default;
		private readonly Scene m_CurrentActiveScene = default;

		public SceneFreezer_Single(in Scene InScene)
		{

			m_PreviousFixedDeltaTime = Time.fixedDeltaTime;
			m_PreviousTimeScale = Time.timeScale;
			m_Scene = InScene;
			m_CurrentActiveScene = SceneManager.GetActiveScene();

			SceneManager.SetActiveScene(InScene);

			// Stop physic simulations
			Physics.autoSimulation = false;
			Time.fixedDeltaTime = float.MaxValue;

			// Setting the time scale to Zero in order to freeze everything but continue to receive unity messages
			Time.timeScale = 0f;
		}

		void System.IDisposable.Dispose()
		{
			if (m_Scene != default)
			{
				SceneManager.SetActiveScene(m_CurrentActiveScene);
			}

			Time.fixedDeltaTime = m_PreviousFixedDeltaTime;
			Time.timeScale = m_PreviousTimeScale;
			Physics.autoSimulation = true;
		}
	}

	private class SceneFreezer_Multi : System.IDisposable
	{
		private struct SceneTimeData
		{
			public string scenePath;
			public float previousFixedDeltaTime;
			public float previousTimeScale;
			public bool autoSimulation;
		}
		private readonly List<SceneTimeData> m_ScenesData = new List<SceneTimeData>();

		private readonly Scene m_CurrentActiveScene = default;

		public SceneFreezer_Multi()
		{
			// Save current active scene in order to restore it as active scene after freezer has been used
			m_CurrentActiveScene = SceneManager.GetActiveScene();

			for (int index = 0, count = SceneManager.sceneCount; index < count; index++)
			{
				// Get loaded scene by index
				Scene loadedScene = SceneManager.GetSceneAt(index);

				// Set current scene in order to get scene relative time data
				SceneManager.SetActiveScene(loadedScene);

				// Create a new SceneTimeData with scene data
				SceneTimeData sceneTimeData = new SceneTimeData()
				{
					scenePath = loadedScene.path,
					previousFixedDeltaTime = Time.fixedDeltaTime,
					previousTimeScale = Time.timeScale,
					autoSimulation = Physics.autoSimulation
				};
				m_ScenesData.Add(sceneTimeData);

				// APPLY FREEZE
				{
					// Stop physic simulations
					Physics.autoSimulation = false;
					Time.fixedDeltaTime = float.MaxValue;

					// Setting the time scale to Zero in order to freeze everything but continue to receive unity messages
					Time.timeScale = 0f;
				}
			}
		}

		void System.IDisposable.Dispose()
		{
			foreach (SceneTimeData sceneTimeData in m_ScenesData)
			{
				Scene scene = SceneManager.GetSceneByPath(sceneTimeData.scenePath);

				// Only for still loaded scene
				if (scene.isLoaded)
				{
					SceneManager.SetActiveScene(scene);
					{
						Time.fixedDeltaTime = sceneTimeData.previousFixedDeltaTime;
						Time.timeScale = sceneTimeData.previousTimeScale;
						Physics.autoSimulation = sceneTimeData.autoSimulation;
					}
				}
			}

			// Restore active scene before freeze request
			SceneManager.SetActiveScene(m_CurrentActiveScene);
		}
	}
}
