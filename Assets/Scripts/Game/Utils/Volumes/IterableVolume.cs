using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public interface IVolumeIterator
{
	void OnIterationStart();
	void OnIteration(Vector3 InPosition);
	void OnIterationCompleted();
}


public abstract class IterableVolume : MonoBehaviour
{
	//////////////////////////////////////////////////////////////////////////
	public abstract bool IterateOver(System.Action<Vector3> OnPosition);

#if UNITY_EDITOR
	[CustomEditor(typeof(IterableVolume))]
	public class IterableVolumeEditor : Editor
	{

		private IterableVolume instance = null;

		private void OnEnable()
		{
			instance = target as IterableVolume;
		}

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Do Action"))
			{
				foreach(IVolumeIterator iterator in instance.gameObject.GetComponents<IVolumeIterator>())
				{
					iterator.OnIterationStart();
					instance.IterateOver(iterator.OnIteration);
					iterator.OnIterationCompleted();
				}
			}
		}
	}
#endif
}
