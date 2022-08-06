
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "", menuName = "Scene Management/Scene Definition")]
public class SceneDefinition : ScriptableObject, System.IEquatable<SceneDefinition>
{
	[SerializeField]
	private string m_SceneName = string.Empty;

	[SerializeField]
	private bool m_IsGameScene = true;

	[SerializeField]
	private SceneReference m_MainScene = null;

	[SerializeField]
	private List<SceneReference> m_SubScenes = new List<SceneReference>();

	private string m_SceneDefinitionId = System.Guid.NewGuid().ToString();



	public string SceneDefinitionId => m_SceneDefinitionId;
	public string SceneName => m_SceneName;
	public bool IsGameScene => m_IsGameScene;
	public string ScenePath => m_MainScene.ScenePath;
	public SceneReference[] SubScenes => m_SubScenes.ToArray();

	public bool IncludedInBuild => SceneRef.buildIndex >= 0;
	public Scene SceneRef => SceneManager.GetSceneByPath(m_MainScene.ScenePath);



	// For Comparison
	bool System.IEquatable<SceneDefinition>.Equals(SceneDefinition other)
	{
		if (other is null)
			return false;

		return this.SceneDefinitionId == other.SceneDefinitionId;
	}
	public override bool Equals(object obj) => Equals(obj as SceneDefinition);
	public override int GetHashCode() => (SceneDefinitionId, SceneName).GetHashCode();
}


// Custom comparer for the SceneDefinition
class SceneDefinitionComparer : IEqualityComparer<SceneReference>
{
	// SceneDefinition are equal if their ids are equal.
	public bool Equals(SceneReference x, SceneReference y)
	{
		//Check whether the compared objects reference the same data.
		if (Object.ReferenceEquals(x, y))
		{
			return true;
		}

		//Check whether any of the compared objects is null.
		if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
		{
			return false;
		}

		//Check whether the SceneDefinition' properties are equal.
		return x.ScenePath == y.ScenePath;
	}

	// If Equals() returns true for a pair of objects
	// then GetHashCode() must return the same value for these objects.

	public int GetHashCode(SceneReference sceneReference)
	{
		//Check whether the object is null
		if (Object.ReferenceEquals(sceneReference, null))
		{
			return 0;
		}

		//Calculate the hash code for the SceneDefinition.
		return sceneReference.ScenePath?.GetHashCode() ?? 0;
	}
}
