
using UnityEditor;
using UnityEngine;


public partial class SoundManager
{
	//////////////////////////////////////////////////////////////////////////
	[CustomEditor(typeof(SoundsDatabase))]
	internal class BehaviourTreeCustomEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (GUILayout.Button("Edit Sounds"))
			{
				SoundManagerEditor.Init();
			}
		}
	}

	private class SoundManagerEditor : EditorWindow
	{
		private const	string							SOUNDS_DATABASE_PATH		= "Assets/Resources/Sounds/SoundsDatabase.asset";

		private	static	SoundManagerEditor				m_Window					= null;
		private	static	SoundsDatabase					m_Database					= null;
		private static	AudioSource						m_AudioSource				= null;

		private Vector2									m_ScrollPosition			= Vector2.zero;

		[MenuItem("Window/Sounds Manager")]
		public static void Init()
		{
			if (m_Window.IsNotNull())
			{
				return;
			}

			m_Database = AssetDatabase.LoadAssetAtPath<SoundsDatabase>(SOUNDS_DATABASE_PATH);
			if (!m_Database)
			{
				m_Database = ScriptableObject.CreateInstance<SoundsDatabase>();
				EditorUtility.SetDirty(m_Database);
				System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(SOUNDS_DATABASE_PATH));
				AssetDatabase.CreateAsset(m_Database, SOUNDS_DATABASE_PATH);
				AssetDatabase.SaveAssetIfDirty(m_Database);
			}

			GameObject go = new GameObject();
			go.hideFlags = HideFlags.HideAndDontSave;
			m_AudioSource = go.AddComponent<AudioSource>();

			m_Window = EditorWindow.GetWindow<SoundManagerEditor>(true, "Sound Manager");
			m_Window.minSize = new Vector2(800f, 200f);
			m_Window.maxSize = new Vector2(1200f, 600f);
		}

		private void OnGUI()
		{
			GUILayout.Space(10f);
			m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
			{
				if (GUILayout.Button("CreateNew"))
				{
					m_Database.SoundResourceItems.Add(new SoundsDatabase.SoundsDatabaseItem());
				}

				GUILayout.BeginHorizontal();
				for (int i = m_Database.SoundResourceItems.Count - 1; i >= 0; i--)
				{
					var item = m_Database.SoundResourceItems[i];

					var newSoundType = (ESoundType)EditorGUILayout.EnumPopup("Sound type:", item.SoundType);
					if (item.SoundType != newSoundType)
					{
						Debug.Log("new type");
						item.SoundType = newSoundType;
						EditorUtility.SetDirty(m_Database);
					}

					var newAudioClip = (AudioClip)EditorGUILayout.ObjectField("Clip:", item.AudioClip, typeof(AudioClip), false);
					if (item.AudioClip != newAudioClip)
					{
						item.AudioClip = newAudioClip;
						EditorUtility.SetDirty(m_Database);
					}
					if (item.AudioClip.IsNotNull() && GUILayout.Button("Play"))
					{
						m_AudioSource.PlayOneShot(item.AudioClip);
					}
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
		}


		private void OnDestroy()
		{
			AssetDatabase.SaveAssets();
			m_AudioSource.gameObject.Destroy();
			m_Window = null;
		}
	}
}


