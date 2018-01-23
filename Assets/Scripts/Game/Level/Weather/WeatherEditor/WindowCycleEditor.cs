using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

namespace WeatherSystem {

	public class WindowCycleEditor : EditorWindow {

		public	static	WindowCycleEditor		m_Window			= null;
	
		private			WeatherCycle			m_CurrentCycle		= null;


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
			m_Window = EditorWindow.GetWindow<WindowCycleEditor>( true, "Cycle Editor" );

			string assetPath = WindowWeatherEditor.RESOURCE_PATH + "/" + name;

			// Ensure directory exists
			System.IO.Directory.CreateDirectory( assetPath );

			m_Window.m_CurrentCycle = ScriptableObject.CreateInstance<WeatherCycle>();
			m_Window.m_CurrentCycle.name = name;
			m_Window.m_CurrentCycle.FolderPath = assetPath;
			m_Window.m_CurrentCycle.AssetPath = ( assetPath + "/" + name + ".asset" );
			m_Window.m_CurrentCycle.WeatherName = name;

			AssetDatabase.CreateAsset( m_Window.m_CurrentCycle, m_Window.m_CurrentCycle.AssetPath );

			if ( m_Window.m_CurrentCycle.Descriptors == null )
				m_Window.m_CurrentCycle.Descriptors = new List<EnvDescriptor>();

			// create descriptors
			Vector2 centerPoint = new Vector2( Screen.width / 2f, Screen.height / 2f );
			for ( int i = 0; i < 24; i++ )
			{
				EnvDescriptor envDescriptor = ScriptableObject.CreateInstance<EnvDescriptor>();
				string identifier = "";
				WeatherManager.TransoformTime( i * 3600, ref identifier, false );
				envDescriptor.Identifier = identifier;
				envDescriptor.AssetPath = ( assetPath + "/" + identifier + ".asset" );
				AssetDatabase.CreateAsset( envDescriptor, envDescriptor.AssetPath );
				m_Window.m_CurrentCycle.Descriptors.Add( envDescriptor );
				EditorUtility.SetDirty( envDescriptor);
			}
			EditorUtility.SetDirty( m_Window.m_CurrentCycle );
			WindowWeatherEditor.m_Window.m_WeathersCycles.Cycles.Add( m_Window.m_CurrentCycle );
			EditorUtility.SetDirty( WindowWeatherEditor.m_Window.m_WeathersCycles );
			AssetDatabase.SaveAssets();
//			EditorUtility.SetDirty( WindowWeatherEditor.m_Window.m_WeathersCycles );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// Init
		public static void Init( WeatherCycle cycle )
		{
			if ( m_Window != null )
			{
				m_Window.Close();
				m_Window = null;
			}
			m_Window = EditorWindow.GetWindow<WindowCycleEditor>( true, "cycle Editor" );
			m_Window.m_CurrentCycle = cycle;
		}


		/////////////////////////////////////////////////////////////////////////////
		/// UNITY
		private void OnGUI()
		{

//			Vector2 centerPoint = new Vector2( Screen.width/2f, Screen.height/2f );









			GUI.Button( new Rect( Screen.width/2, Screen.height/2, 20,30 ), "Press me" );
			
			

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