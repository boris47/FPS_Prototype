
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//	DELEGATES FOR EVENTS
public delegate StreamingUnit StreamingEvent( StreamingData streamingData );


public class SaveFileinfo {

	public	string	fileName	= "";
	public	string	filePath	= "";
	public	string	saveTime	= "";
	public	bool	isAutomatic = false;

}

// INTERFACE
/// <summary> Clean interface of only GameManager class </summary>
public interface IGameManager_SaveManagement {

	// PROPERTIES

	/// <summary> Events called when game is saving </summary>
			event	StreamingEvent		OnSave;

	/// <summary> Events called when game is loading </summary>
			event	StreamingEvent		OnLoad;

	/// <summary> Save current play </summary>
					void				Save	( string fileName, bool isAutomaic = false );

	/// <summary> Load a file </summary>
					void				Load	( string fileName );
}



public partial class GameManager : IGameManager_SaveManagement {

	/// <summary> Events called when game is saving </summary>
	public event StreamingEvent			OnSave	= null;

	/// <summary> Events called when game is loading </summary>
	public event StreamingEvent			OnLoad	= null;


	//////////////////////////////////////////////////////////////////////////
	// Save
	/// <summary> Used to save data of puzzles </summary>
	public void Save( string fileName, bool isAutomaic = false )
	{
		// TODO: CHECK FOR AUTOMAIC SAVE

		StreamingData streamingData = new StreamingData();

		// call all save callbacks
		OnSave( streamingData );

		// write data on disk
		string toSave = JsonUtility.ToJson( streamingData, prettyPrint: true );
		System.IO.File.WriteAllText( "SaveFile.txt", toSave );
	}


	//////////////////////////////////////////////////////////////////////////
	// Load
	/// <summary> Used to send signal of load to all registered callbacks </summary>
	public void Load( string fileName = "SaveFile.txt" )
	{
		if ( fileName == null || fileName.Length == 0 )
			return;
		
		// load data from disk
		string toLoad = System.IO.File.ReadAllText( fileName );
		StreamingData streamingData = JsonUtility.FromJson< StreamingData >( toLoad );

		if ( streamingData == null )
		{
			Debug.LogError( "GameManager::Load:: Save \"" + fileName +"\" cannot be loaded" );
			return;
		}

		// call all load callbacks
		OnLoad( streamingData );
	}

}

[System.Serializable]
public class StreamingUnit {
	[SerializeField]
	public	int			InstanceID		= -1;

	[SerializeField]
	public	string		Name			= "";

	[SerializeField]
	public	Vector3		Position		= Vector3.zero;
	[SerializeField]
	public	Quaternion	Rotation		= Quaternion.identity;

	public	float		ShieldStatus	= -1f;

	[SerializeField]
	public	string		Internals		= "";


	public	void		AddInternal( string keyValue )
	{
		Internals += ( ( Internals.Length > 0 ) ? ", " : "" ) + keyValue;
	}

}

[SerializeField]
public class StreamingData {

	[SerializeField]
	public List<StreamingUnit> Data = new List<StreamingUnit>();

}
