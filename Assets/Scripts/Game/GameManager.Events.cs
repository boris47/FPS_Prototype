
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
public struct GameEvents {
	// SAVE & LOAD
	public	delegate	StreamUnit	StreamingEvent( StreamData streamData );	// StreamEvents.OnSave & StreamEvents.OnLoad

	// PAUSE
	public	delegate	void		OnPauseSetEvent( bool isPaused );			// PauseEvents.OnPauseSet

	// UPDATES
	public	delegate	void		OnThinkEvent();								// UpdateEvents.OnThink
	public	delegate	void		OnPhysicFrameEvent( float FixedDeltaTime );	// UpdateEvents.OnPhysicFrame
	public	delegate	void		OnFrameEvent( float DeltaTime );			// UpdateEvents.OnFrame

	// OTHERS
	public	delegate	void		VoidArgsEvent();
}


//////////////////////////////////////////////////////////////////
//						SAVE & LOAD								//
//////////////////////////////////////////////////////////////////

public enum EStreamingState
{
	NONE, SAVING, SAVE_COMPLETE, LOADING, LOAD_COMPLETE
}

public interface IStreamableByEvents {

	StreamUnit	OnSave( StreamData streamData );

	StreamUnit	OnLoad( StreamData streamData );
}

/// <summary> Clean interface of only GameManager Save&load Section </summary>
public interface IStreamEvents {

	/// <summary> Events called when game is saving </summary>
		event		GameEvents.StreamingEvent		OnSave;

	/// <summary> Events called when game has saved </summary>
		event		GameEvents.StreamingEvent		OnSaveComplete;

	/// <summary> Events called when game is loading ( Events are called along more frames !! ) </summary>
		event		GameEvents.StreamingEvent		OnLoad;

	/// <summary> Events called when game has been loaded ( Events are called along more frames !! ) </summary>
		event		GameEvents.StreamingEvent		OnLoadComplete;

	/// <summary> Save current play </summary>
					void									Save	( string filePath = "SaveFile.txt", bool isAutomaic = false );

	/// <summary> Load a file </summary>
					void									Load	( string fileName = "SaveFile.txt" );

	/// <summary> Return the current state of the manager </summary>
	EStreamingState											State	{ get; }
}

// SAVE & LOAD IMPLEMENTATION
public sealed partial class GameManager : IStreamEvents
{
	// Save slot infos
	public class SaveFileinfo
	{
		public	string	fileName	= "";
		public	string	filePath	= "";
		public	string	saveTime	= "";
		public	bool	isAutomatic = false;
	}

	private const	string				ENCRIPTION_KEY	= "Boris474Ever";
	private	static	IStreamEvents		m_StreamEvents	= null;

	// Events
	private	event	GameEvents.StreamingEvent		m_OnSave			= delegate ( StreamData streamData ) { return null; };
	private	event	GameEvents.StreamingEvent		m_OnSaveComplete	= delegate ( StreamData streamData ) { return null; };

//	private	event	GameEvents.StreamingEvent		m_OnLoad			= delegate ( StreamData streamData ) { return null; };
	private			List<GameEvents.StreamingEvent>	m_OnLoad = new List<GameEvents.StreamingEvent>();
//	private	event	GameEvents.StreamingEvent		m_OnLoadComplete	= delegate ( StreamData streamData ) { return null; };
	private			List<GameEvents.StreamingEvent>	m_OnLoadComplete = new List<GameEvents.StreamingEvent>();
	private			EStreamingState					m_SaveLoadState		= EStreamingState.NONE;

	#region INTERFACE
	// INTERFACE START
	/// <summary> Streaming events interface </summary>
	public static IStreamEvents StreamEvents => m_StreamEvents;

	/// <summary> Return the current stream State </summary>
	EStreamingState IStreamEvents.State => m_SaveLoadState;

	/// <summary> Events called when game is saving </summary>
	event GameEvents.StreamingEvent IStreamEvents.OnSave
	{
		add		{ if ( value != null )	m_OnSave += value; }
		remove	{ if ( value != null )	m_OnSave -= value; }
	}

	/// <summary> Events called when game is saving </summary>
	event GameEvents.StreamingEvent IStreamEvents.OnSaveComplete
	{
		add		{ if ( value != null )	m_OnSaveComplete += value; }
		remove	{ if ( value != null )	m_OnSaveComplete -= value; }
	}

	/// <summary> Events called when game is loading ( Events are called along more frames !! ) </summary>
	event GameEvents.StreamingEvent IStreamEvents.OnLoad
	{
		add		{ if ( value != null ) m_OnLoad.Add(value); }// m_OnLoad += value; }
		remove	{ if ( value != null ) m_OnLoad.Remove(value); }// m_OnLoad -= value; }
	}

	/// <summary> Events called when game has been loaded ( Events are called along more frames !! ) </summary>
	event GameEvents.StreamingEvent IStreamEvents.OnLoadComplete
	{
//		add		{ if ( value != null )	m_OnLoadComplete += value; }
//		remove	{ if ( value != null )	m_OnLoadComplete -= value; }
		add		{ if ( value != null ) m_OnLoadComplete.Add(value); }// m_OnLoadComplete += value; }
		remove	{ if ( value != null ) m_OnLoadComplete.Remove(value); }// m_OnLoadComplete -= value; }
	}
	// INTERFACE END
#endregion INTERFACE

	// Vars
	private		Aes						m_Encryptor		= Aes.Create();
	private		Rfc2898DeriveBytes		m_PDB			= new Rfc2898DeriveBytes( 
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

		// call all save callbacks
		m_OnSave( streamData );

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
			m_OnSaveComplete(streamData);
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

		if ( streamData != null )
		{
			CoroutinesManager.Start(LoadCO( streamData ), "GameManager::Load: Start of loading" );
		}
		else
		{
			m_SaveLoadState = EStreamingState.LOAD_COMPLETE;
			InputManager.IsEnabled = true;
			Debug.LogError( $"GameManager::Load:: Save \"{fileName}\" cannot be loaded" );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private IEnumerator LoadCO( StreamData streamData )
	{
		yield return null;

		foreach(GameEvents.StreamingEvent _delegate in m_OnLoad)
		{
			_delegate( streamData );
			yield return null;
		}

		yield return null;

		foreach ( GameEvents.StreamingEvent _delegate in m_OnLoadComplete )
		{
			_delegate( streamData );
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
	[SerializeField]
	public	int						InstanceID		= -1;

	[SerializeField]
	public	string					Name			= "";

	[SerializeField]
	public	Vector3					Position		= Vector3.zero;

	[SerializeField]
	public	Quaternion				Rotation		= Quaternion.identity;

	[System.Serializable]
	protected class MyKeyValuePair
	{
		[SerializeField]
		public	string	Key = string.Empty;

		[SerializeField]
		public	string	Value = string.Empty;

		public	MyKeyValuePair( string Key, string Value )
		{
			this.Key	= Key;
			this.Value	= Value;
		}
	}

	[SerializeField]
	private	List<MyKeyValuePair>	Internals		= new List<MyKeyValuePair>();

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
	public	StreamUnit	NewUnit( GameObject gameObject )
	{
		if ( GameManager.StreamEvents.State != EStreamingState.SAVING )
			return null;

		StreamUnit streamUnit		= null;
		int index = m_Data.FindIndex( ( StreamUnit data ) => data.InstanceID == gameObject.GetInstanceID() );
		if ( index > -1 )
		{
//			Debug.Log( gameObject.name + " already saved" );
			streamUnit = m_Data[index];
		}
		else
		{
			streamUnit					= new StreamUnit();
			streamUnit.InstanceID		= gameObject.GetInstanceID();
			streamUnit.Name				= gameObject.name;

			m_Data.Add( streamUnit );
		}

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> To be used ONLY during the LOAD event, otherwise nothing happen </summary>
	public	bool		TryGetUnit( GameObject gameObject, out StreamUnit streamUnit )
	{
	//	if ( GameManager.StreamEvents.State != StreamingState.LOAD_COMPLETE )
	//		return false;

		streamUnit = null;
		int GOInstanceID = gameObject.GetInstanceID();
		int index = m_Data.FindIndex( ( StreamUnit data ) => data.InstanceID == GOInstanceID );

		if ( index == -1 )
		{
			index = m_Data.FindIndex( ( StreamUnit data ) => data.Name == gameObject.name );
		}

		bool found = index > -1;
		if ( found )
		{
			streamUnit = m_Data[ index ];
		}

		return found;
	}

}


//////////////////////////////////////////////////////////////////
//							PAUSE								//
//////////////////////////////////////////////////////////////////

public interface IPauseEvents {
	/// <summary> Events called when game is setting on pause </summary>
	event		GameEvents.OnPauseSetEvent		OnPauseSet;

	void							SetPauseState( bool bPauseState );
}

// PAUSE IMPLEMENTATION
public partial class GameManager : IPauseEvents {

	private	static	IPauseEvents	m_PauseEvents = null;
	public	static	IPauseEvents	PauseEvents
	{
		get { return m_PauseEvents; }
	}

	// Event
	private static event	GameEvents.OnPauseSetEvent	m_OnPauseSet			= delegate { };

	/// <summary> Events called when game is setting on pause </summary>
	event GameEvents.OnPauseSetEvent IPauseEvents.OnPauseSet
	{
		add		{ if ( value != null )	m_OnPauseSet += value; }
		remove	{ if ( value != null )	m_OnPauseSet -= value; }
	}

	// Vars
	private	static			bool				m_IsPaused				= false;
	public	static			bool				IsPaused
	{
		get { return m_IsPaused; }
	}

	private					float				m_PrevTimeScale			= 1f;
	private					bool				m_PrevCanParseInput		= false;
	private					bool				m_PrevInputEnabled		= false;


	//////////////////////////////////////////////////////////////////////////
	void	IPauseEvents.SetPauseState( bool bIsPauseRequested )
	{
		if ( bIsPauseRequested == m_IsPaused )
			return;

		m_IsPaused = bIsPauseRequested;
		m_OnPauseSet( m_IsPaused );

		GlobalManager.SetCursorVisibility( bIsPauseRequested == true );
		SoundManager.IsPaused = bIsPauseRequested;
		
		if ( bIsPauseRequested == true )
		{
			UIManager.Instance.GoToMenu( UIManager.PauseMenu );
			m_PrevTimeScale					= Time.timeScale;
			m_PrevCanParseInput				= GlobalManager.InputMgr.HasCategoryEnabled(EInputCategory.CAMERA);
			m_PrevInputEnabled					= InputManager.IsEnabled;

			Time.timeScale							= 0f;

//			CameraControl.Instance.CanParseInput	= false;
			GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, false);
			InputManager.IsEnabled					= false;
		}
		else
		{
			UIManager.Instance.GoToMenu( UIManager.InGame );
			Time.timeScale							= m_PrevTimeScale;
			GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, m_PrevCanParseInput);
			InputManager.IsEnabled					= m_PrevInputEnabled;
		}
		m_SkipOneFrame = true;
	}

}



//////////////////////////////////////////////////////////////////
//							UPDATES								//
//////////////////////////////////////////////////////////////////

public interface IUpdateEvents {
	/// <summary> TODO </summary>
		event		GameEvents.OnThinkEvent			OnThink;

	/// <summary> TODO </summary>
		event		GameEvents.OnPhysicFrameEvent	OnPhysicFrame;

	/// <summary> TODO </summary>
		event		GameEvents.OnFrameEvent			OnFrame;

		event		GameEvents.OnFrameEvent			OnLateFrame;
}

// UPDATES IMPLEMENTATION
public partial class GameManager : IUpdateEvents {

	private	static	IUpdateEvents		m_UpdateEvents	= null;
	public	static	IUpdateEvents		UpdateEvents
	{
		get { return m_UpdateEvents; }
	}

	private static event	GameEvents.OnThinkEvent			m_OnThink				= delegate { };
	private static event	GameEvents.OnPhysicFrameEvent	m_OnPhysicFrame			= delegate { };
	private static event	GameEvents.OnFrameEvent			m_OnFrame				= delegate { };
	private static event	GameEvents.OnFrameEvent			m_OnLateFrame			= delegate { };


	event		GameEvents.OnThinkEvent			IUpdateEvents.OnThink
	{
		add		{	if ( value != null )	m_OnThink += value;	}
		remove	{	if ( value != null )	m_OnThink -= value;	}
	}

	event		GameEvents.OnPhysicFrameEvent	IUpdateEvents.OnPhysicFrame
	{
		add		{	if ( value != null )	m_OnPhysicFrame += value; }
		remove	{	if ( value != null )	m_OnPhysicFrame -= value; }
	}

	event		GameEvents.OnFrameEvent			IUpdateEvents.OnFrame
	{
		add		{	if ( value != null )	m_OnFrame += value;	}
		remove	{	if ( value != null )	m_OnFrame -= value; }
	}

	event		GameEvents.OnFrameEvent			IUpdateEvents.OnLateFrame
	{
		add		{	if ( value != null )	m_OnLateFrame += value;	}
		remove	{	if ( value != null )	m_OnLateFrame -= value; }
	}
}

