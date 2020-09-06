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
			WindowWeatherEditor.GetWMGR().EDITOR_EditorDescriptorLinked = true;
		}

		

		private const float BUTTON_WIDTH = 180f;
		/////////////////////////////////////////////////////////////////////////////
		// UNITY
		private void OnGUI()
		{
			WindowWeatherEditor.GetWMGR().EDITOR_EditorDescriptorLinked = true;
			GUILayout.Label( "DESCRIPTOR " + this.m_CurrentDescriptor.Identifier );
			GUILayout.Space( 10f );



			// Ambient Color
			GUILayout.Label( "Ambient Color" );
			GUILayout.BeginHorizontal();
			{
				this.m_CurrentDescriptor.AmbientColor = EditorGUILayout.ColorField (this.m_CurrentDescriptor.AmbientColor, GUILayout.MaxWidth( 50f ) );
				//				GUILayout.Label( "Ambient Color String" );
				this.m_AmbColorString = EditorGUILayout.TextArea (this.m_AmbColorString, GUILayout.MinWidth( BUTTON_WIDTH ) );
				if ( GUILayout.Button( "GO" ) )
				{
					Utils.Converters.StringToColor(this.m_AmbColorString, ref this.m_CurrentDescriptor.AmbientColor );
					this.m_AmbColorString = "";
				}
			}
			GUILayout.EndHorizontal();


			// Ambient Effects
			GUILayout.Label( "Ambient Effects" );
			this.m_CurrentDescriptor.AmbientEffects = EditorGUILayout.ObjectField(this.m_CurrentDescriptor.AmbientEffects, typeof( AudioCollection ), false ) as AudioCollection;



			// Fog Factor
			GUILayout.Label( "Fog Factor" );
			this.m_CurrentDescriptor.FogFactor = EditorGUILayout.Slider(this.m_CurrentDescriptor.FogFactor, 0.0f, 1.0f );




			// Fog Factor
			GUILayout.Label( "Rain Intensity" );
			this.m_CurrentDescriptor.RainIntensity = EditorGUILayout.Slider(this.m_CurrentDescriptor.RainIntensity, 0.0f, 1.0f );



			// Sky Cube Map
			GUILayout.Label( "Sky Cube Map" );
			this.m_CurrentDescriptor.SkyCubemap = EditorGUILayout.ObjectField(this.m_CurrentDescriptor.SkyCubemap, typeof( Cubemap ), false ) as Cubemap;



			// Sky Color
			GUILayout.Label( "Sky Color" );
			GUILayout.BeginHorizontal();
			{
				this.m_CurrentDescriptor.SkyColor = EditorGUILayout.ColorField (this.m_CurrentDescriptor.SkyColor, GUILayout.MaxWidth( 50f ) );
				//				GUILayout.Label( "Sky Color String" );
				this.m_SkyColorString = EditorGUILayout.TextArea (this.m_SkyColorString, GUILayout.MinWidth( BUTTON_WIDTH ) );
				if ( GUILayout.Button( "GO" ) )
				{
					Utils.Converters.StringToColor(this.m_SkyColorString, ref this.m_CurrentDescriptor.SkyColor );
					this.m_SkyColorString = "";
				}
			}
			GUILayout.EndHorizontal();



			// Sun Color
			GUILayout.Label( "Sun Color" );
			GUILayout.BeginHorizontal();
			{
				this.m_CurrentDescriptor.SunColor = EditorGUILayout.ColorField (this.m_CurrentDescriptor.SunColor, GUILayout.MaxWidth( 50f ) );
				//				GUILayout.Label( "Sky Color String" );
				this.m_SunColorString = EditorGUILayout.TextArea (this.m_SunColorString, GUILayout.MinWidth( BUTTON_WIDTH ) );
				if ( GUILayout.Button( "GO" ) )
				{
					Utils.Converters.StringToColor(this.m_SunColorString, ref this.m_CurrentDescriptor.SunColor );
					this.m_SunColorString = "";
				}
			}
			GUILayout.EndHorizontal();



			// Sun Color
			GUILayout.Label( "Sun Rotation V3" );
//			GUILayout.BeginHorizontal();
			{
				this.m_CurrentDescriptor.SunRotation = EditorGUILayout.Vector3Field ( "", this.m_CurrentDescriptor.SunRotation );
				//				GUILayout.Label( "Sun Rotation String" );
				this.m_SunRotationString = EditorGUILayout.TextArea (this.m_SunRotationString, GUILayout.MinWidth( BUTTON_WIDTH ) );
				GUILayout.BeginHorizontal();
				if ( GUILayout.Button( "GO" ) )
				{
					Utils.Converters.StringToVector(this.m_SunRotationString, ref this.m_CurrentDescriptor.SunRotation );
					this.m_SunRotationString = "";
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
					reader.GetSection(this.m_CurrentDescriptor.Identifier + ":00", ref section );
					if ( section != null )
					{
						Utils.Converters.StringToColor( section.GetRawValue("ambient_color"),		ref this.m_CurrentDescriptor.AmbientColor	);
						Utils.Converters.StringToColor( section.GetRawValue("sky_color"),			ref this.m_CurrentDescriptor.SkyColor		);
						Utils.Converters.StringToColor(	section.GetRawValue("sun_color"),			ref this.m_CurrentDescriptor.SunColor		);
						Utils.Converters.StringToVector(section.GetRawValue("sun_rotation"),		ref this.m_CurrentDescriptor.SunRotation		);
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
			this.m_CurrentDescriptor.set = true;

			if (this.m_AmbColorString.Length > 0 )
				Utils.Converters.StringToColor(this.m_AmbColorString, ref this.m_CurrentDescriptor.AmbientColor );

			if (this.m_SkyColorString.Length > 0 )
				Utils.Converters.StringToColor(this.m_SkyColorString, ref this.m_CurrentDescriptor.SkyColor );

			if (this.m_SunColorString.Length > 0 )
				Utils.Converters.StringToColor(this.m_SunColorString, ref this.m_CurrentDescriptor.SunColor );

			if (this.m_SunRotationString.Length > 0 )
				Utils.Converters.StringToVector(this.m_SunRotationString, ref this.m_CurrentDescriptor.SunRotation );

			WindowWeatherEditor.GetWMGR().EDITOR_EditorDescriptorLinked = false;
		}

	}

}

#endif