using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif

namespace WeatherSystem {

	[System.Serializable]
	public class WeatherCycle : ScriptableObject/*, IResourceComposite*/ {

		[SerializeField][ReadOnly]
		public	string					AssetPath			= string.Empty;

		[SerializeField]
		public	string[]				DescriptorsPaths	= new string[ 24 ];

		[SerializeField]
		public	EnvDescriptor[]			LoadedDescriptors	= new EnvDescriptor[24];


		public	bool	LoadFromPresetFile( string path )
		{
			if ( path.Length == 0 )
					return false;

			SectionMap reader = new SectionMap();
			if ( reader.LoadFile( path ) == false )
			{
#if UNITY_EDITOR
					EditorUtility.DisplayDialog( "Error !", "Selected file cannot be parsed !", "OK" );
#endif
			}
			else
			{
				foreach( EnvDescriptor descriptor in this.LoadedDescriptors )
				{
					Debug.Log( "Parsing data for descripter " + descriptor.Identifier );

					Database.Section section = null;
					if ( reader.GetSection( descriptor.Identifier, ref section ) )
					{
						Utils.Converters.StringToColor( section.GetRawValue("ambient_color"),		ref descriptor.AmbientColor	);
						Utils.Converters.StringToColor( section.GetRawValue("sky_color"),			ref descriptor.SkyColor		);
						Utils.Converters.StringToColor(	section.GetRawValue("sun_color"),			ref descriptor.SunColor		);

						descriptor.FogFactor		= section.AsFloat( "fog_density" );
						descriptor.RainIntensity	= section.AsFloat( "rain_density" );

						if ( section.HasKey( "sun_rotation" ) )
						{
							Utils.Converters.StringToVector(section.GetRawValue("sun_rotation"),		ref descriptor.SunRotation	);
						}
						else if ( section.HasKey( "sun_altitude" ) && section.HasKey( "sun_longitude") )
						{
							descriptor.SunRotation = Utils.Math.VectorByHP
							(
								section.AsFloat( "sun_altitude" ),
								section.AsFloat( "sun_longitude" )
							);
						}

					}
					descriptor.set = true;
#if UNITY_EDITOR
					EditorUtility.SetDirty( descriptor );
#endif
					Debug.Log( "Data parsed correctly" );
					section = null;
				}
				reader = null;
#if UNITY_EDITOR
				EditorUtility.SetDirty( this );
				AssetDatabase.SaveAssets();
#endif
			}
			return true;
		}

		/*
		//////////////////////////////////////////////////////////////////////////
		bool	IResourceComposite.NeedToBeLoaded()
		{
			if ( LoadedDescriptors.Count < DescriptorsPaths.Length )
				return true;

			bool bAreLoaded = !LoadedDescriptors.TrueForAll( d => d != null );
			return bAreLoaded;
		}



		//////////////////////////////////////////////////////////////////////////
		void		IResourceComposite.Reinit()
		{
			LoadedDescriptors.Clear();
		}


		//////////////////////////////////////////////////////////////////////////
		string[]	IResourceComposite.GetChildPaths()
		{
			return DescriptorsPaths;
		}


		//////////////////////////////////////////////////////////////////////////
		void		IResourceComposite.AddChild( UnityEngine.Object child )
		{
			EnvDescriptor childConverted = child as EnvDescriptor;
			LoadedDescriptors.Add( childConverted );
		}
		*/
	}

}