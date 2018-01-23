using UnityEngine;
using UnityEditor;

namespace WeatherSystem {

	public class WindowDescriptorEditor : EditorWindow {
	
		public	static	WindowDescriptorEditor		m_Window				= null;


		private		EnvDescriptor					m_CurrentDescriptor		= null;


		/////////////////////////////////////////////////////////////////////////////
		/// Init
		public static void Init( WeatherCycle cycle, string name )
		{
			if ( System.IO.File.Exists( cycle.FolderPath + "/" + name + ".asset" ) == true )
			{
				return;
			}

			if ( m_Window != null )
			{
				m_Window.Close();
				m_Window = null;
			}
			m_Window = EditorWindow.GetWindow<WindowDescriptorEditor>( true, "Descriptor Editor" );

			m_Window.m_CurrentDescriptor = ScriptableObject.CreateInstance<EnvDescriptor>();
			m_Window.m_CurrentDescriptor.name = name;
			m_Window.m_CurrentDescriptor.AssetPath = ( cycle.FolderPath + "/" + name + ".asset" );

			AssetDatabase.CreateAsset( m_Window.m_CurrentDescriptor, m_Window.m_CurrentDescriptor.AssetPath );
			AssetDatabase.SaveAssets();

			cycle.Descriptors.Add( m_Window.m_CurrentDescriptor );
		}


		/////////////////////////////////////////////////////////////////////////////
		/// Init
		public static void Init( EnvDescriptor thisDescriptor )
		{
			if ( m_Window != null )
			{
				m_Window.Close();
				m_Window = null;
			}
			m_Window = EditorWindow.GetWindow<WindowDescriptorEditor>( true, "Descriptor Editor" );

			m_Window.m_CurrentDescriptor = thisDescriptor;
		}



		/////////////////////////////////////////////////////////////////////////////
		/// UNITY
		private void OnGUI()
		{
			


		}

		private void OnDestroy()
		{
			EditorUtility.SetDirty( m_CurrentDescriptor );
			AssetDatabase.SaveAssets();
		}

	}

}