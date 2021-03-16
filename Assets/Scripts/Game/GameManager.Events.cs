﻿
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;


[System.Serializable]
public class GameEvent      : UnityEngine.Events.UnityEvent { }
[System.Serializable]
public class GameEventArg1	: UnityEngine.Events.UnityEvent< UnityEngine.GameObject > { }
[System.Serializable]
public class GameEventArg2	: UnityEngine.Events.UnityEvent< UnityEngine.GameObject, UnityEngine.GameObject > { }
[System.Serializable]
public class GameEventArg3	: UnityEngine.Events.UnityEvent< UnityEngine.GameObject, UnityEngine.GameObject, UnityEngine.GameObject > { }
[System.Serializable]
public class GameEventArg4	: UnityEngine.Events.UnityEvent< UnityEngine.GameObject, UnityEngine.GameObject, UnityEngine.GameObject, UnityEngine.GameObject > { }




//	DELEGATES FOR EVENTS
public struct GameEvents
{
	// SAVE & LOAD
	public	delegate	bool		StreamingEvent(StreamData streamData, ref StreamUnit streamUnit);	// StreamEvents.OnSave & StreamEvents.OnLoad

	// PAUSE
	public	delegate	void		OnPauseSetEvent(bool isPaused);				// PauseEvents.OnPauseSet

	// UPDATES
	public	delegate	void		OnThinkEvent();								// UpdateEvents.OnThink
	public	delegate	void		OnPhysicFrameEvent(float FixedDeltaTime);	// UpdateEvents.OnPhysicFrame
	public	delegate	void		OnFrameEvent(float DeltaTime);				// UpdateEvents.OnFrame

	// OTHERS
	public	delegate	void		VoidArgsEvent();
}


//////////////////////////////////////////////////////////////////
//						SAVE & LOAD								//
//////////////////////////////////////////////////////////////////

public enum EStreamingState: short
{
	NONE, SAVING, SAVE_COMPLETE, LOADING, LOAD_COMPLETE
}

public interface IStreamableByEvents
{
	bool OnSave(StreamData streamData, ref StreamUnit streamUnit);

	bool OnLoad(StreamData streamData, ref StreamUnit streamUnit);
}

/// <summary> Clean interface of only GameManager Save&load Section </summary>
public interface IStreamEvents
{
	/// <summary> Events called when game is saving </summary>
		event		GameEvents.StreamingEvent		OnSave;

	/// <summary> Events called when game has saved </summary>
		event		GameEvents.StreamingEvent		OnSaveComplete;

	/// <summary> Events called when game is loading ( Events are called along more frames !! ) </summary>
		event		GameEvents.StreamingEvent		OnLoad;

	/// <summary> Events called when game has been loaded ( Events are called along more frames !! ) </summary>
		event		GameEvents.StreamingEvent		OnLoadComplete;

	/// <summary> Save current play </summary>
					void							Save	(string filePath = "SaveFile.txt", bool isAutomaic = false);

	/// <summary> Load a file </summary>
					void							Load	(string fileName = "SaveFile.txt");

	/// <summary> Return the current state of the manager </summary>
	EStreamingState									State	{ get; }
}

// SAVE & LOAD IMPLEMENTATION
public sealed partial class GameManager : IStreamEvents
{
	private const			string							ENCRIPTION_KEY			= "Boris474Ever";

	/// <summary> Streaming events interface </summary>
	public	static			IStreamEvents					StreamEvents			=> m_Instance;
	
	/// <summary> Return the current stream State </summary>
	EStreamingState			IStreamEvents.State				=> m_SaveLoadState;

	public class SaveFileinfo
	{
		public	string	fileName	= "";
		public	string	filePath	= "";
		public	string	saveTime	= "";
		public	bool	isAutomatic = false;
	}

	// Events
	private	event			GameEvents.StreamingEvent		m_OnSave					= delegate { return true; };
	private	event			GameEvents.StreamingEvent		m_OnSaveComplete			= delegate { return true; };

	private					List<GameEvents.StreamingEvent>	m_OnLoad					= new List<GameEvents.StreamingEvent>();
	private					List<GameEvents.StreamingEvent>	m_OnLoadComplete			= new List<GameEvents.StreamingEvent>();
	private					EStreamingState					m_SaveLoadState				= EStreamingState.NONE;

	#region INTERFACE
	// INTERFACE START


	/// <summary> Events called when game is saving </summary>
	event GameEvents.StreamingEvent IStreamEvents.OnSave
	{
		add		{ if (value.IsNotNull())	m_OnSave += value; }
		remove	{ if (value.IsNotNull())	m_OnSave -= value; }
	}

	/// <summary> Events called when game is saving </summary>
	event GameEvents.StreamingEvent IStreamEvents.OnSaveComplete
	{
		add		{ if (value.IsNotNull())	m_OnSaveComplete += value; }
		remove	{ if (value.IsNotNull())	m_OnSaveComplete -= value; }
	}

	/// <summary> Events called when game is loading ( Events are called along more frames !! ) </summary>
	event GameEvents.StreamingEvent IStreamEvents.OnLoad
	{
		add		{ if (value.IsNotNull()) m_OnLoad.Add(value); }
		remove	{ if (value.IsNotNull()) m_OnLoad.Remove(value); }
	}

	/// <summary> Events called when game has been loaded ( Events are called along more frames !! ) </summary>
	event GameEvents.StreamingEvent IStreamEvents.OnLoadComplete
	{
		add		{ if (value.IsNotNull()) m_OnLoadComplete.Add(value); }
		remove	{ if (value.IsNotNull()) m_OnLoadComplete.Remove(value); }
	}
	// INTERFACE END
#endregion INTERFACE

	// Vars
	private		readonly Aes						m_Encryptor		= Aes.Create();
	private		readonly Rfc2898DeriveBytes			m_PDB			= new Rfc2898DeriveBytes
	( 
		password:	ENCRIPTION_KEY,
		salt :		new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 }
	);


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Used to save data of all registered objects </summary>
	void						IStreamEvents.Save( string filePath, bool isAutomaic )
	{
		// TODO: CHECK FOR AUTOMAIC SAVE

		// Conditions
		if (m_SaveLoadState == EStreamingState.SAVING )
		{
			UnityEngine.Debug.Log( "Another save must finish write actions !!" );
			return;
		}

		m_SaveLoadState = EStreamingState.SAVING;

		PlayerPrefs.SetInt( "SaveSceneIdx", UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex );
		PlayerPrefs.SetString( "SaveFilePath", filePath );

		StreamData streamData = new StreamData();
		StreamUnit streamUnit = null;
		// call all save callbacks
		m_OnSave( streamData, ref streamUnit );

		// Thread Body
		void body()
		{
			print("Saving...");

			// Serialize data
			string toSave = JsonUtility.ToJson(streamData, prettyPrint: true);
		//	Encrypt( ref toSave );
			File.WriteAllText(filePath, toSave);
		}

		// Thread OnCompletion
		void onCompletion()
		{
			m_SaveLoadState = EStreamingState.SAVE_COMPLETE;
			m_OnSaveComplete(streamData, ref streamUnit);
			print("Saved!");
		}
		MultiThreading.CreateThread( body, bCanStart: true, onCompletion );
	}
	
	//////////////////////////////////////////////////////////////////////////
	/// <summary> Used to load data for all registered objects </summary>
	void						IStreamEvents.Load( string fileName )
	{
		// Conditions
		if (m_SaveLoadState == EStreamingState.SAVING || m_SaveLoadState == EStreamingState.LOADING )
		{
			UnityEngine.Debug.Log( "Cannot load while loading or saving !!" );
			return;
		}

		if ( string.IsNullOrEmpty( fileName ) )
		{
			UnityEngine.Debug.Log( "Cannot load because invalid filename !!" );
			return;
		}

		// Body
		m_SaveLoadState = EStreamingState.LOADING;
		InputManager.IsEnabled = false;
		print( $"Loading {System.IO.Path.GetFileNameWithoutExtension( fileName )}..." );

		// Deserialize data
		string toLoad = File.ReadAllText( fileName );
//		Decrypt( ref toLoad );
		StreamData streamData = JsonUtility.FromJson<StreamData>( toLoad );
		StreamUnit streamUnit = null;
		if ( streamData.IsNotNull() )
		{
			CoroutinesManager.Start(LoadCO( streamData, streamUnit ), "GameManager::Load: Start of loading" );
		}
		else
		{
			m_SaveLoadState = EStreamingState.LOAD_COMPLETE;
			InputManager.IsEnabled = true;
			Debug.LogError( $"GameManager::Load:: Save \"{fileName}\" cannot be loaded" );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private IEnumerator LoadCO( StreamData streamData, StreamUnit streamUnit )
	{
		yield return null;

		var copy = m_OnLoad.ConvertAll(e => new GameEvents.StreamingEvent(e));

		int counter = 0;
		foreach(GameEvents.StreamingEvent _delegate in copy)
		{
			counter += 1;
			_delegate( streamData, ref streamUnit );
			yield return null;
		}

		yield return null;

		foreach ( GameEvents.StreamingEvent _delegate in m_OnLoadComplete )
		{
			_delegate( streamData, ref streamUnit );
			yield return null;
		}

		print( "Loaded!" );

		m_SaveLoadState = EStreamingState.LOAD_COMPLETE;
//		InputManager.IsEnabled = true;
	}

	
	// https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp
	//////////////////////////////////////////////////////////////////////////
	private		void			Encrypt( ref string clearText )
	{
		byte[] clearBytes = Encoding.Unicode.GetBytes( clearText );
		m_Encryptor.Key = m_PDB.GetBytes( 32 ); m_Encryptor.IV = m_PDB.GetBytes( 16 );

		using (MemoryStream stream = new MemoryStream())
		{
			using (CryptoStream crypter = new CryptoStream(stream, m_Encryptor.CreateEncryptor(), CryptoStreamMode.Write))
			{
				crypter.Write(clearBytes, 0, clearBytes.Length);
			}
			clearText = System.Convert.ToBase64String( stream.ToArray() );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private		void			Decrypt( ref string cipherText )
	{
		byte[] cipherBytes = System.Convert.FromBase64String( cipherText.Replace( " ", "+" ) );
		m_Encryptor.Key = m_PDB.GetBytes( 32 ); m_Encryptor.IV = m_PDB.GetBytes( 16 );

		using (MemoryStream stream = new MemoryStream())
		{
			using (CryptoStream crypter = new CryptoStream(stream, m_Encryptor.CreateDecryptor(), CryptoStreamMode.Write))
			{
				crypter.Write( cipherBytes, 0, cipherBytes.Length );
			}
			cipherText = Encoding.Unicode.GetString( stream.ToArray() );
		}
	}
}

[System.Serializable]
public class StreamUnit
{
	[System.Serializable]
	protected class MyKeyValuePair
	{
		[SerializeField]
		public string Key = string.Empty;

		[SerializeField]
		public string Value = string.Empty;

		public MyKeyValuePair(string Key, string Value)
		{
			this.Key = Key;
			this.Value = Value;
		}
	}

	[SerializeField]
	public string Name = "";

	[SerializeField]
	public Vector3 Position = Vector3.zero;

	[SerializeField]
	public Quaternion Rotation = Quaternion.identity;

	[SerializeField]
	private List<MyKeyValuePair> Internals = new List<MyKeyValuePair>();

	public StreamUnit(string name)
	{
		this.Name = name;
	}

	//////////////////////////////////////////////////////////////////////////
	public	void		SetInternal( string key, object value )
	{
		MyKeyValuePair keyValue = null;
		int index = Internals.FindIndex( ( MyKeyValuePair kv ) => kv.Key == key );
		if ( index == -1 )
		{
			MyKeyValuePair kv = new MyKeyValuePair( key, value.ToString() );
			Internals.Add( kv );
		}
		else
		{
			keyValue = Internals[ index ];
			keyValue.Value = value.ToString();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	bool		RemoveInternal( string key )
	{
		int index = Internals.FindIndex( ( MyKeyValuePair kv ) => kv.Key == key );
		bool found = ( index != -1 );
		if ( found )
		{
			Internals.RemoveAt( index );
		}
		return found;
	}


	//////////////////////////////////////////////////////////////////////////
	public	bool		HasInternal( string key )
	{
		int index = Internals.FindIndex( ( MyKeyValuePair kv ) => kv.Key == key );
		return index > -1;
	}


	//////////////////////////////////////////////////////////////////////////
	public	string		GetInternal( string key )
	{
		int keyValueIndex = Internals.FindIndex( ( MyKeyValuePair kv ) => kv.Key == key );
		if (keyValueIndex > -1 && !string.IsNullOrEmpty(Internals[keyValueIndex].Value))
		{
			return Internals[keyValueIndex].Value;
		}
		Debug.Log($"Cannot retrieve value for key {key}");
		return string.Empty;
	}


	//////////////////////////////////////////////////////////////////////////
	public	bool		GetAsBool( string key )
	{
		if (bool.TryParse(GetInternal(key), out bool result))
		{
			return result;
		}
		Debug.Log($"Streamunit:GetAsBool: Cannot retrieve value for key {key} as BOOLEAN");
		return false;
	}


	//////////////////////////////////////////////////////////////////////////
	public	int			GetAsInt( string key )
	{
		if (int.TryParse(GetInternal(key), out int result))
		{
			return result;
		}
		Debug.Log($"Streamunit:GetAsInt: Cannot retrieve value for key {key} as INTEGER");
		return -1;
	}


	//////////////////////////////////////////////////////////////////////////
	public	float		GetAsFloat( string key )
	{
		if (float.TryParse(GetInternal(key), out float result))
		{
			return result;
		}
		Debug.Log($"Streamunit:GetAsFloat: Cannot retrieve value for key {key} as FLOAT");
		return 0.0f;
	}



	//////////////////////////////////////////////////////////////////////////
	public	T			GetAsEnum<T>( string key ) where T : struct
	{
		if (Utils.Converters.StringToEnum(GetInternal(key), out T result) == false)
		{
			return result;
		}
		Debug.Log($"Streamunit:GetAsEnum: Cannot retrieve value for key {key} as ENUM");
		return default(T);
	}


	//////////////////////////////////////////////////////////////////////////
	public Vector3		GetAsVector( string key )
	{
		if (Utils.Converters.StringToVector3(GetInternal(key), out Vector3 result ))
		{
			return result;
		}
		Debug.Log($"Streamunit:GetAsVector: Cannot retrieve value for key {key} as VECTOR3");
		return Vector3.zero;
	}


	//////////////////////////////////////////////////////////////////////////
	public Quaternion	GetAsQuaternion( string key )
	{
		if (Utils.Converters.StringToQuaternion(GetInternal(key), out Quaternion result) )
		{
			return result;
		}
		Debug.Log($"Streamunit:GetAsQuaternion: Cannot retrieve value for key {key} as QUATERNION");
		return Quaternion.identity;
	}
}

[System.Serializable]
public class StreamData
{
	[SerializeField]
	private List<StreamUnit> m_Data = new List<StreamUnit>();


	//////////////////////////////////////////////////////////////////////////
	/// <summary> To be used ONLY during the SAVE event, otherwise nothing happens </summary>
	public	StreamUnit	NewUnit( Object unityObj )
	{
		CustomAssertions.IsTrue(GameManager.StreamEvents.State == EStreamingState.SAVING);

		StreamUnit streamUnit = m_Data.Find((StreamUnit data) => data.Name == unityObj.name);
		if (streamUnit == null)
		{
			streamUnit = new StreamUnit(unityObj.name);
			m_Data.Add(streamUnit);
		}
		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> To be used ONLY during the LOAD event, otherwise nothing happen </summary>
	public	bool		TryGetUnit( Object unityObj, out StreamUnit streamUnit )
	{
	//	if ( GameManager.StreamEvents.State != StreamingState.LOAD_COMPLETE )
	//		return false;

		CustomAssertions.IsTrue(GameManager.StreamEvents.State == EStreamingState.LOADING);

		streamUnit = m_Data.Find((StreamUnit data) => data.Name == unityObj.name);
		return streamUnit.IsNotNull(); ;
	}

}


//////////////////////////////////////////////////////////////////
//							PAUSE								//
//////////////////////////////////////////////////////////////////

public interface IPauseEvents
{
	/// <summary> Events called when game is setting on pause </summary>
		event		GameEvents.OnPauseSetEvent		OnPauseSet;

					void							SetPauseState( bool bPauseState );
}

// PAUSE IMPLEMENTATION
public partial class GameManager : IPauseEvents
{
	private event			GameEvents.OnPauseSetEvent		m_OnPauseSet			= delegate { };

	public	static			IPauseEvents					PauseEvents				=> m_Instance;

	/// <summary> Events called when game is setting on pause </summary>
	event GameEvents.OnPauseSetEvent IPauseEvents.OnPauseSet
	{
		add		{ if (value.IsNotNull())	m_OnPauseSet += value; }
		remove	{ if (value.IsNotNull())	m_OnPauseSet -= value; }
	}

	// Vars
	private	static			bool				m_IsPaused				= false;
	public	static			bool				IsPaused				=> m_IsPaused;

	private					float				m_PrevTimeScale			= 1f;
	private					bool				m_PrevCanParseInput		= false;
	private					bool				m_PrevInputEnabled		= false;


	//////////////////////////////////////////////////////////////////////////
	void	IPauseEvents.SetPauseState(bool bIsPauseRequested)
	{
		if (bIsPauseRequested != m_IsPaused)
		{
			m_IsPaused = bIsPauseRequested;
			m_OnPauseSet(m_IsPaused);

		//	GlobalManager.SetCursorVisibility(bIsPauseRequested);
			SoundManager.IsPaused = bIsPauseRequested;
		
			if (bIsPauseRequested)
			{
				m_PrevTimeScale					= Time.timeScale;
				m_PrevCanParseInput				= GlobalManager.InputMgr.HasCategoryEnabled(EInputCategory.CAMERA);
				m_PrevInputEnabled				= InputManager.IsEnabled;

				InputManager.IsEnabled			= false;
				Time.timeScale					= 0f;
				GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, false);

				UIManager.Instance.GoToMenu(UIManager.PauseMenu);
			}
			else
			{
				GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, m_PrevCanParseInput);
				Time.timeScale					= m_PrevTimeScale;
				InputManager.IsEnabled			= m_PrevInputEnabled;

				UIManager.Instance.GoToMenu(UIManager.InGame);
			}
			GlobalManager.Instance.RequireFrameSkip();
		}
	}
}



//////////////////////////////////////////////////////////////////
//							UPDATES								//
//////////////////////////////////////////////////////////////////

public interface IUpdateEvents
{
	/// <summary> TODO </summary>
		event		GameEvents.OnThinkEvent			OnThink;

	/// <summary> TODO </summary>
		event		GameEvents.OnPhysicFrameEvent	OnPhysicFrame;

	/// <summary> TODO </summary>
		event		GameEvents.OnFrameEvent			OnFrame;

		event		GameEvents.OnFrameEvent			OnLateFrame;
}

// UPDATES IMPLEMENTATION
public partial class GameManager : IUpdateEvents
{
	private static event	GameEvents.OnThinkEvent			m_OnThink				= delegate { };
	private static event	GameEvents.OnPhysicFrameEvent	m_OnPhysicFrame			= delegate { };
	private static event	GameEvents.OnFrameEvent			m_OnFrame				= delegate { };
	private static event	GameEvents.OnFrameEvent			m_OnLateFrame			= delegate { };

	public	static			IUpdateEvents					UpdateEvents			=> m_Instance;

	event		GameEvents.OnThinkEvent			IUpdateEvents.OnThink
	{
		add		{	if (value.IsNotNull())	m_OnThink += value;	}
		remove	{	if (value.IsNotNull())	m_OnThink -= value;	}
	}

	event		GameEvents.OnPhysicFrameEvent	IUpdateEvents.OnPhysicFrame
	{
		add		{	if (value.IsNotNull())	m_OnPhysicFrame += value; }
		remove	{	if (value.IsNotNull())	m_OnPhysicFrame -= value; }
	}

	event		GameEvents.OnFrameEvent			IUpdateEvents.OnFrame
	{
		add		{	if (value.IsNotNull())	m_OnFrame += value;	}
		remove	{	if (value.IsNotNull())	m_OnFrame -= value; }
	}

	event		GameEvents.OnFrameEvent			IUpdateEvents.OnLateFrame
	{
		add		{	if (value.IsNotNull())	m_OnLateFrame += value;	}
		remove	{	if (value.IsNotNull())	m_OnLateFrame -= value; }
	}
}

