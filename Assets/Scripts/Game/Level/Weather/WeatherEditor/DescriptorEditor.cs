using UnityEngine;
using UnityEditor;

namespace WeatherSystem {

	public class DescriptorEditor : EditorWindow {
	
		public	static	DescriptorEditor		m_Window				= null;


		private		EnvDescriptor				m_CurrentDescriptor		= null;

		public static void Init( EnvDescriptorsCollection container, string name )
		{
			string containerPath = System.IO.Path.GetDirectoryName( container.AssetPath );
			if ( System.IO.Directory.Exists( System.IO.Path.Combine( containerPath, name ) ) == true )
				return;

			if ( m_Window != null )
			{
				m_Window.Close();
				m_Window = null;
			}

			m_Window = EditorWindow.GetWindow<DescriptorEditor>( true, "Descriptor Editor" );

			m_Window.m_CurrentDescriptor = ScriptableObject.CreateInstance<EnvDescriptor>();
			m_Window.m_CurrentDescriptor.name = name;
			m_Window.m_CurrentDescriptor.AssetPath = ( containerPath + "//" + name + ".asset" );

			AssetDatabase.CreateAsset( m_Window.m_CurrentDescriptor, m_Window.m_CurrentDescriptor.AssetPath );
			AssetDatabase.SaveAssets();

			container.Descriptors.Add( m_Window.m_CurrentDescriptor );

			EditorUtility.SetDirty( container );
		}


		// Editing
		public static void Init( EnvDescriptor thisDescriptor )
		{
			if ( m_Window != null )
			{
				m_Window.Close();
				m_Window = null;
			}
			m_Window = EditorWindow.GetWindow<DescriptorEditor>( true, "Descriptor Editor" );

			m_Window.m_CurrentDescriptor = thisDescriptor;
		}

		private void OnDestroy()
		{
			AssetDatabase.SaveAssets();
		}

	}

}