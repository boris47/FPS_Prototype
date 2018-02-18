using UnityEngine;
using UnityEditor;

[CreateAssetMenu( fileName = "", menuName = "Collections/Audio Collection" )]
public class AudioCollection : ScriptableObject {

	public	AudioClip[] AudioSources = null;

}