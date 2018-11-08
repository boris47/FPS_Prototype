using UnityEngine;
using Database;

#if UNITY_EDITOR

using UnityEditor;

namespace WeatherSystem {

	public class WindowDescriptorEditor : EditorWindow {
	
		public	static	WindowDescriptorEditor		m_Window				= null;

		private			EnvDescriptor				m_CurrentDescriptor		= null;

		private		 string					  m_AmbColorString		= "Insert color as string";
		private		 string					  m_SkyColorString		= "Insert color as string";
		private		 string					  m_SunColorString		= "Insert color as string";
		private		 string					  m_SunRotationString		= "Insert vector as string";
		
		/////////////////////////////////////////////////////////////////////////////
		// Init ( EDITING )
		public static void Init( EnvDescriptor thisDescriptor )
		{
			if ( m_Window != null )
			{
				m_Window.Close();
				m_Window = null;
			}
			m_Window = EditorWindow.GetWindow<WindowDescriptorEditor>( true, "Descriptor Editor" );
			m_Window.minSize = m_Window.maxSize = new Vector2( 300f, 470f );

			m_Window.m_CurrentDescriptor = thisDescriptor;
			WeatherManager.Internal.EditorDescriptorLinked = true;
		}

		private const float BUTTON_WIDTH = 180f;
		/////////////////////////////////////////////////////////////////////////////
		// UNITY
		private void OnGUI()
		{
			GUILayout.Label( "DESCRIPTOR " + m_CurrentDescriptor.Identifier );
			GUILayout.Space( 10f );



			// Ambient Color
			GUILayout.Label( "Ambient Color" );
			GUILayout.BeginHorizontal();
			{
				m_CurrentDescriptor.AmbientColor = EditorGUILayout.ColorField ( m_CurrentDescriptor.AmbientColor, GUILayout.MaxWidth( 50f ) );
//				GUILayout.Label( "Ambient Color String" );
				m_AmbColorString = EditorGUILayout.TextArea ( m_AmbColorString, GUILayout.MinWidth( BUTTON_WIDTH ) );
				if ( GUILayout.Button( "GO" ) )
				{
					Utils.Converters.StringToColor( m_AmbColorString, ref m_CurrentDescriptor.AmbientColor );
					m_AmbColorString = "";
				}
			}
			GUILayout.EndHorizontal();


			// Ambient Effects
			GUILayout.Label( "Ambient Effects" );
			m_CurrentDescriptor.AmbientEffects = EditorGUILayout.ObjectField( m_CurrentDescriptor.AmbientEffects, typeof( AudioCollection ), false ) as AudioCollection;



			// Fog Factor
			GUILayout.Label( "Fog Factor" );
			m_CurrentDescriptor.FogFactor = EditorGUILayout.Slider( m_CurrentDescriptor.FogFactor, 0.0f, 1.0f );




			// Fog Factor
			GUILayout.Label( "Rain Intensity" );
			m_CurrentDescriptor.RainIntensity = EditorGUILayout.Slider( m_CurrentDescriptor.RainIntensity, 0.0f, 1.0f );




			// Sky Material
			GUILayout.Label( "Sky Cube Map" );
			m_CurrentDescriptor.SkyCubemap = EditorGUILayout.ObjectField( m_CurrentDescriptor.SkyCubemap, typeof( Cubemap ), false ) as Cubemap;




			// Sky Color
			GUILayout.Label( "Sky Color" );
			GUILayout.BeginHorizontal();
			{
				m_CurrentDescriptor.SkyColor = EditorGUILayout.ColorField (m_CurrentDescriptor.SkyColor, GUILayout.MaxWidth( 50f ) );
//				GUILayout.Label( "Sky Color String" );
				m_SkyColorString = EditorGUILayout.TextArea ( m_SkyColorString, GUILayout.MinWidth( BUTTON_WIDTH ) );
				if ( GUILayout.Button( "GO" ) )
				{
					Utils.Converters.StringToColor( m_SkyColorString, ref m_CurrentDescriptor.SkyColor );
					m_SkyColorString = "";
				}
			}
			GUILayout.EndHorizontal();




			// Sun Color
			GUILayout.Label( "Sun Color" );
			GUILayout.BeginHorizontal();
			{
				m_CurrentDescriptor.SunColor = EditorGUILayout.ColorField (m_CurrentDescriptor.SunColor, GUILayout.MaxWidth( 50f ) );
//				GUILayout.Label( "Sky Color String" );
				m_SunColorString = EditorGUILayout.TextArea ( m_SunColorString, GUILayout.MinWidth( BUTTON_WIDTH ) );
				if ( GUILayout.Button( "GO" ) )
				{
					Utils.Converters.StringToColor( m_SunColorString, ref m_CurrentDescriptor.SunColor );
					m_SunColorString = "";
				}
			}
			GUILayout.EndHorizontal();




			// Sun Color
			GUILayout.Label( "Sun Rotation V3" );
//			GUILayout.BeginHorizontal();
			{
				m_CurrentDescriptor.SunRotation = EditorGUILayout.Vector3Field ( "", m_CurrentDescriptor.SunRotation );
//				GUILayout.Label( "Sun Rotation String" );
				m_SunRotationString = EditorGUILayout.TextArea ( m_SunRotationString, GUILayout.MinWidth( BUTTON_WIDTH ) );
				GUILayout.BeginHorizontal();
				if ( GUILayout.Button( "GO" ) )
				{
					Utils.Converters.StringToVector( m_SunRotationString, ref m_CurrentDescriptor.SunRotation );
					m_SunRotationString = "";
				}
				if ( GUILayout.Button( "SET CURRENT" ) )
				{
					m_CurrentDescriptor.SunRotation = WeatherManager.Instance.Sun.transform.rotation.eulerAngles;
				}
				GUILayout.EndHorizontal();
			}
//			GUILayout.EndHorizontal();


			if ( GUILayout.Button( "Read Config File" ) )
			{
				string path = EditorUtility.OpenFilePanel( "Pick a config file", "", "ltx" );
				if ( path.Length == 0 )
					return;

				SectionMap reader = new SectionMap();
				if ( reader.LoadFile( path ) == false )
				{
					 EditorUtility.DisplayDialog( "Error !", "Selected file cannot be parsed !", "OK" );
				}
				else
				{
					Database.Section section = null;
					reader.GetSection( m_CurrentDescriptor.Identifier + ":00", ref section );
					if ( section != null )
					{
						Utils.Converters.StringToColor( section.GetRawValue("ambient_color"),		ref m_CurrentDescriptor.AmbientColor	);
						Utils.Converters.StringToColor( section.GetRawValue("sky_color"),			ref m_CurrentDescriptor.SkyColor		);
						Utils.Converters.StringToColor(	section.GetRawValue("sun_color"),			ref m_CurrentDescriptor.SunColor		);
						Utils.Converters.StringToVector(section.GetRawValue("sun_rotation"),		ref m_CurrentDescriptor.SunRotation		);
						section = null;
						reader = null;
					}
				}
			}

			GUILayout.Space( 10f );
			if ( GUILayout.Button( "CLOSE" ) )
				EditorWindow.focusedWindow.Close();
		}

		/////////////////////////////////////////////////////////////////////////////
		// UNITY
		private void OnDestroy()
		{
			m_CurrentDescriptor.set = true;

			if ( m_AmbColorString.Length > 0 )
				Utils.Converters.StringToColor( m_AmbColorString, ref m_CurrentDescriptor.AmbientColor );

			if ( m_SkyColorString.Length > 0 )
				Utils.Converters.StringToColor( m_SkyColorString, ref m_CurrentDescriptor.SkyColor );

			if ( m_SunColorString.Length > 0 )
				Utils.Converters.StringToColor( m_SunColorString, ref m_CurrentDescriptor.SunColor );

			if ( m_SunRotationString.Length > 0 )
				Utils.Converters.StringToVector( m_SunRotationString, ref m_CurrentDescriptor.SunRotation );

			WeatherManager.Internal.EditorDescriptorLinked = false;
		}

	}

}

#endif