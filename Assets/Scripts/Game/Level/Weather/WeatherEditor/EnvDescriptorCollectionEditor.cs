using UnityEngine;
using UnityEditor;

namespace WeatherSystem {

	public class EnvDescriptorCollectionEditor : EditorWindow {

		public	static	EnvDescriptorCollectionEditor		m_Window				= null;
	
		private			EnvDescriptorsCollection			m_CurrentCollection		= null;

		public static void Init( string name )
		{
			if ( System.IO.Directory.Exists( System.IO.Path.Combine( WeatherEditor.RESOURCE_PATH, name ) ) == true )
				return;

			if ( m_Window != null )
			{
				m_Window.Close();
				m_Window = null;
			}
			m_Window = EditorWindow.GetWindow<EnvDescriptorCollectionEditor>( true, "Collection Editor" );

			string assetPath = System.IO.Path.Combine( WeatherEditor.RESOURCE_PATH, name );

			// Ensure directory exists
			System.IO.Directory.CreateDirectory( assetPath );

			m_Window.m_CurrentCollection = ScriptableObject.CreateInstance<EnvDescriptorsCollection>();
			m_Window.m_CurrentCollection.name = name;
			m_Window.m_CurrentCollection.AssetPath = ( assetPath + "//" + name + ".asset" );

			AssetDatabase.CreateAsset( m_Window.m_CurrentCollection, m_Window.m_CurrentCollection.AssetPath );
			AssetDatabase.SaveAssets();


			WeatherEditor.m_Window.m_WeathersCollection.Weathers.Add( m_Window.m_CurrentCollection );

		}

		private void OnGUI()
		{
			if ( GUILayout.Button( "Close" ) )
				m_Window.Close();
		}

		private void OnDestroy()
		{
			EditorUtility.SetDirty( WeatherEditor.m_Window.m_WeathersCollection );
			AssetDatabase.SaveAssets();
		}
	}

}