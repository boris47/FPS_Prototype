
using UnityEngine;

#if UNITY_EDITOR
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.SceneManagement;
#endif

[CreateAssetMenu(fileName = "", menuName = "Scene Management/Scene Definition Collection")]
public class SceneDefinitionCollection : ScriptableObject
{
	[SerializeField]
	private SceneDefinition[] m_SceneDefinitions = new SceneDefinition[0];


	public SceneDefinition[] SceneDefinitions => m_SceneDefinitions;


#if UNITY_EDITOR
	[CustomEditor(typeof(SceneDefinitionCollection))]
	// ^ This is the script we are making a custom editor for.
	public class ImageCustomConfigEditor : Editor
	{
		private SceneDefinitionCollection instance = null;

		private void OnEnable()
		{
			instance = target as SceneDefinitionCollection;
		}

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Update Build Scenes"))
			{
				HashSet<string> scenesSet = new HashSet<string>();
				foreach (SceneDefinition sceneDefinition in instance.SceneDefinitions)
				{
					scenesSet.Add(sceneDefinition.ScenePath);
					foreach (SceneReference subSceneReference in sceneDefinition.SubScenes)
					{
						scenesSet.Add(subSceneReference);
					}
				}
				EditorBuildSettings.scenes = scenesSet.Select(scenePath => new EditorBuildSettingsScene(scenePath, true)).ToArray();
			}
		}
	}
#endif
}
