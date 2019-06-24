using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryPoint : MonoBehaviour {

	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnBeforeSceneLoad ()
	{
		UnityEngine.Assertions.Assert.raiseExceptions = true;
		Debug.developerConsoleVisible = true;
//		print( "EntryPoint::OnBeforeSceneLoad" );
	}

	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void AfterSceneLoad ()
	{
//		print( "EntryPoint::AfterSceneLoad" );
	}

	/*
	private	static	void	CheckOrCreateInstance<T>() where T : Component
	{
		T instance = FindObjectOfType<T>();
		if ( instance == null )
		{
			new GameObject( typeof(T).Name ).AddComponent<T>();
		}
		else
		{
			instance.SendMessage( "Initialize" );
		}
	}
	*/
}
