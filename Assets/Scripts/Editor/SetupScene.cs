using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;


#if UNITY_EDITOR
using UnityEditor;

using WeatherSystem;
using QuestSystem;

public class SetupScene : EditorWindow  {

	[MenuItem( "Window/Scene/MainSceneSetup" )]
	private	static	void	MainSceneSetup()
	{

		foreach ( GameObject go in Object.FindObjectsOfType<GameObject>() )
		{
//			DestroyImmediate( go );
		}
		
		LoadPrefab<GlobalManager>		( prefabPath:"Prefabs/Essentials/GlobalManager"			);

		LoadPrefab<CustomSceneManager>	( prefabPath:"Prefabs/Essentials/CustomSceneManager"		);
		
		LoadPrefab<ResourceManager>		( prefabPath:"Prefabs/Essentials/ResourceManager"			);
		
		LoadPrefab<SoundManager>		( prefabPath:"Prefabs/Essentials/SoundManager"				);

		LoadPrefab<EffectsManager>		( prefabPath:"Prefabs/Essentials/EffectsManager"			);

//		LoadPrefab<CustomSceneManager>	( prefabPath:"Prefabs/Essentials/CustomSceneManager"		);
		
		LoadPrefab<UIManager>			( prefabPath: "Prefabs/Essentials/UIManager"				);

		LoadPrefab<WeatherManager>		( prefabPath: "Prefabs/Essentials/WeatherManager"			);
		
		EditorSceneManager.SaveOpenScenes();
	}


	[MenuItem( "Window/Scene/InGameSceneSetup" )]
	private	static	void	InGameSceneSetup()
	{

		MainSceneSetup();
		
		LoadPrefab<GameManager>			( prefabPath:"Prefabs/Essentials/InGame/GameManager"		);

		LoadPrefab<CameraControl>		( prefabPath:"Prefabs/Essentials/InGame/Camera_InGame"		);

		LoadPrefab<LocalQuestManager>	( prefabPath:"Prefabs/Essentials/InGame/LocalQuestManager"	);

		LoadPrefab<Player>				( prefabPath:"Prefabs/Essentials/InGame/Player"				);

		LoadPrefab<WeaponManager>		( prefabPath:"Prefabs/Essentials/InGame/WeaponManager"		);

		LoadPrefab<SurfaceManager>		( prefabPath: "Prefabs/Essentials/SurfaceManager"			);

		EditorSceneManager.SaveOpenScenes();
	}

	private	static	T	SpawnIfNotFound<T>( bool bDontDestroyOnLoad = false ) where T : Component
	{
		T result = FindObjectOfType<T>();
		if ( result == null )
		{
			GameObject go = new GameObject( typeof( T ).Name );
			result = go.AddComponent<T>();
			if ( bDontDestroyOnLoad )
			{
				Object.DontDestroyOnLoad(go);
			}
		}
		return result;
	}

	


	private	static	void	LoadPrefab<T>( string prefabPath ) where T : Component
	{
		T result = FindObjectOfType<T>();
		if ( result )
		{
			DestroyImmediate( result.transform.root.gameObject );
		}

		Debug.Log( "Creating " + prefabPath );

		GameObject loaded = Resources.Load<GameObject>( prefabPath );
		GameObject go = Object.Instantiate( loaded ) as GameObject;

		EditorSceneManager.MarkSceneDirty( go.scene );

		PrefabUtility.ConnectGameObjectToPrefab( go, loaded );
	}
	
}


#endif