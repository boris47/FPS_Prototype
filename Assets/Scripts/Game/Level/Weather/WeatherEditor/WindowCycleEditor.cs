using UnityEngine;
using UnityEditor;

namespace WeatherSystem {

	public class WindowCycleEditor : EditorWindow {

		public	static	WindowCycleEditor		m_Window			= null;
	
		private			WeatherCycle					m_CurrentCycle		= null;


		/////////////////////////////////////////////////////////////////////////////
		/// Init
		public static void Init( string name )
		{
			if ( System.IO.Directory.Exists( System.IO.Path.Combine( WindowWeatherEditor.RESOURCE_PATH, name ) ) == true )
				return;

			if ( m_Window != null )
			{
				m_Window.Close();
				m_Window = null;
			}
			m_Window = EditorWindow.GetWindow<WindowCycleEditor>( true, "Collection Editor" );

			string assetPath = WindowWeatherEditor.RESOURCE_PATH + "/" + name;

			// Ensure directory exists
			System.IO.Directory.CreateDirectory( assetPath );

			m_Window.m_CurrentCycle = ScriptableObject.CreateInstance<WeatherCycle>();
			m_Window.m_CurrentCycle.name = name;
			m_Window.m_CurrentCycle.FolderPath = assetPath;
			m_Window.m_CurrentCycle.AssetPath = ( assetPath + "/" + name + ".asset" );

			AssetDatabase.CreateAsset( m_Window.m_CurrentCycle, m_Window.m_CurrentCycle.AssetPath );
			AssetDatabase.SaveAssets();

			WindowWeatherEditor.m_Window.m_WeathersCycles.Cycles.Add( m_Window.m_CurrentCycle );
//			EditorUtility.SetDirty( WindowWeatherEditor.m_Window.m_WeathersCycles );

		}

		/////////////////////////////////////////////////////////////////////////////
		/// UNITY
		private void OnGUI()
		{
			if ( GUILayout.Button( "Close" ) )
				m_Window.Close();
		}

		private void OnDestroy()
		{
			EditorUtility.SetDirty( m_CurrentCycle );
			AssetDatabase.SaveAssets();
		}
	}

}