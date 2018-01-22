using UnityEngine;
using System.Collections.Generic;

using UnityEditor;

namespace WeatherSystem {

	public class WeatherEditor : EditorWindow {
	
		public const string RESOURCE_PATH			= "Assets/Resources/Weather/Descriptors";
		public const string COLLECTION_FILENAME	= "WeatherCollection";


		public	static	WeatherEditor				m_Window				= null;

		public	WeathersCollection					m_WeathersCollection	= null;

		private Vector2								m_ScrollPosition		= Vector2.zero;

		[ MenuItem ( "Window/Weather Manager" ) ]
		public static void Init()
		{
			if ( m_Window != null )
			{
				m_Window.Close();
				m_Window = null;
			}
			m_Window = EditorWindow.GetWindow<WeatherEditor>( true, "Weather Manager" );

			// Create directory if not exists
			if ( System.IO.Directory.Exists( RESOURCE_PATH ) == false )
				System.IO.Directory.CreateDirectory( RESOURCE_PATH );


			// Create or load asset
			string assetPath = System.IO.Path.Combine( RESOURCE_PATH, COLLECTION_FILENAME ) + ".asset";
			if ( System.IO.File.Exists( assetPath ) == false )
			{
				m_Window.m_WeathersCollection = ScriptableObject.CreateInstance<WeathersCollection>();
				AssetDatabase.CreateAsset( m_Window.m_WeathersCollection, assetPath );
				AssetDatabase.SaveAssets();
			}
			else
			{
				m_Window.m_WeathersCollection = AssetDatabase.LoadAssetAtPath<WeathersCollection>( assetPath );
			}

			if ( m_Window.m_WeathersCollection.Weathers == null )
				m_Window.m_WeathersCollection.Weathers = new List<EnvDescriptorsCollection>();

			/*
			string[] weatherPaths = System.IO.Directory.GetDirectories( RESOURCE_PATH );
			if ( weatherPaths.Length == 0 )
				return;

			foreach ( string weatherPath in weatherPaths )
			{
				// Get directory name and path
				string WeatherName = System.IO.Path.GetFileName( weatherPath );
				string WeatherDir  = System.IO.Path.GetDirectoryName( weatherPath );

				// Create collection
				EnvDescriptorsCollection collection = ScriptableObject.CreateInstance<EnvDescriptorsCollection>();
				collection.WeatherName = directoryName;
				collection.Descriptors = new List<EnvDescriptor>();

				// Get all descriptors
				string[] files = System.IO.Directory.GetFiles( descriptorsPath );
				foreach ( string filePath in files )
				{
					string descrptorName = System.IO.Path.GetFileNameWithoutExtension( filePath );
				
					// Skip *.meta files
					if ( filePath.EndsWith( "meta" ) )
						continue;

					EnvDescriptor envDescriptor = AssetDatabase.LoadAssetAtPath<EnvDescriptor>
						( System.IO.Path.Combine( descriptorsPath, descrptorName ) );

					collection.Descriptors.Add( envDescriptor );

				}
			}
			*/
		}


		private void DeleteWeather( WeathersCollection collection, int idx )
		{
			EditorUtility.SetDirty( collection );
			Debug.Log( "Removing weather " + collection.Weathers[ idx ].name );
			string assetPath = collection.Weathers[ idx ].AssetPath;
			System.IO.Directory.Delete( System.IO.Path.GetDirectoryName( assetPath ), true );
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		private void DeleteDescriptor( EnvDescriptorsCollection collection, int idx )
		{
			EditorUtility.SetDirty( collection );
			Debug.Log( "Removing descriptor " + collection.Descriptors[ idx ].name );
			string assetPath = collection.AssetPath;
			collection.Descriptors.RemoveAt( idx );
			AssetDatabase.DeleteAsset( assetPath );
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		private void OnGUI()
		{
		
			if ( GUILayout.Button( "Create Weather" ) )
			{
				// TODO: NEW DEDICATED WINDOW
				ValueStepWindow.Init<string>( () => EnvDescriptorCollectionEditor.Init( ValueStepWindow.Value ), null );
			
			}

			if ( m_WeathersCollection == null || m_WeathersCollection.Weathers == null || m_WeathersCollection.Weathers.Count == 0 )
				return;

			
			m_ScrollPosition = GUILayout.BeginScrollView( m_ScrollPosition );
			{
				GUILayout.BeginHorizontal();
				{
					for ( int i = 0; i < m_WeathersCollection.Weathers.Count; i++ )
					{
						EnvDescriptorsCollection envCollection = m_WeathersCollection.Weathers[ i ];
						if ( envCollection == null )
						{
//							m_WeathersCollection.Weathers.RemoveAt( i -- );
							continue;
						}

						if ( GUILayout.Button( "New Descriptor" ) )
						{
							ValueStepWindow.Init<string>( () => DescriptorEditor.Init( envCollection, ValueStepWindow.Value ), null );
						}

						GUILayout.Label( envCollection.name );
						if ( GUILayout.Button( "Delete" ) )
						{
							DeleteWeather( m_WeathersCollection, i );
							continue;
						}


						if ( envCollection.Descriptors == null || envCollection.Descriptors.Count == 0 )
							continue;


						GUILayout.BeginVertical();
						{
							for ( int j = 0; j < envCollection.Descriptors.Count; j++ )
							{
								EnvDescriptor descriptor = envCollection.Descriptors[ j ];
								if ( descriptor == null )
								{
//									envCollection.Descriptors.RemoveAt( j -- );
									continue;
								}

								GUILayout.BeginHorizontal();
								GUILayout.Label( descriptor.name );

								if ( GUILayout.Button( "Edit" ) )
								{
									// TODO EDIT WINDOW FOR ENVDESCRIPTOR
									DescriptorEditor.Init( descriptor );
								}
								if ( GUILayout.Button( "Del" ) )
								{
									DeleteDescriptor( envCollection, j-- );
									continue;
								}
								GUILayout.EndHorizontal();
							}
						}
						GUILayout.EndVertical();
					}
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
	
		}

		private void OnDestroy()
		{
			if ( DescriptorEditor.m_Window != null )
				DescriptorEditor.m_Window.Close();

			if ( EnvDescriptorCollectionEditor.m_Window != null )
				EnvDescriptorCollectionEditor.m_Window.Close();

			if ( ValueStepWindow.m_Window != null )
				ValueStepWindow.m_Window.Close();

		}
	}

}