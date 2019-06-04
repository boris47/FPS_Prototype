
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Threading;


[System.Serializable]
public class GameEvent	  : UnityEngine.Events.UnityEvent { }

public interface IStreamableByEvents {

	StreamUnit	OnSave( StreamData streamData );

	StreamUnit	OnLoad( StreamData streamData );
}

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

public enum StreamingState {
	NONE, SAVING, SAVE_COMPLETE, LOADING, LOAD_COMPLETE
}

/// <summary> Clean interface of only GameManager Save&load Section </summary>
public interface StreamEvents {

	/// <summary> Events called when game is saving </summary>
		event		GameEvents.StreamingEvent		OnSave;

	/// <summary> Events called when game has saved </summary>
		event		GameEvents.StreamingEvent		OnSaveComplete;

	/// <summary> Events called when game is loading </summary>
		event		GameEvents.StreamingEvent		OnLoad;

	/// <summary> Events called when game has loaded </summary>
		event		GameEvents.StreamingEvent		OnLoadComplete;

	/// <summary> Save current play </summary>
					void									Save	( string filePath = "SaveFile.txt", bool isAutomaic = false );

	/// <summary> Load a file </summary>
					void									Load	( string fileName = "SaveFile.txt" );

	/// <summary> Return the current state of the manager </summary>
	StreamingState											State	{ get; }
}

// SAVE & LOAD IMPLEMENTATION
public partial class GameManager : StreamEvents {

	// Save slot infos
	public class SaveFileinfo {

		public	string	fileName	= "";
		public	string	filePath	= "";
		public	string	saveTime	= "";
		public	bool	isAutomatic = false;

	}

	private const	string				ENCRIPTION_KEY	= "Boris474Ever";
	private	static	StreamEvents		m_StreamEvents	= null;

	// Events
	private	event	GameEvents.StreamingEvent		m_OnSave			= delegate ( StreamData streamData ) { return null; };
	private	event	GameEvents.StreamingEvent		m_OnSaveComplete	= delegate ( StreamData streamData ) { return null; };
	private	event	GameEvents.StreamingEvent		m_OnLoad			= delegate ( StreamData streamData ) { return null; };
	private	event	GameEvents.StreamingEvent		m_OnLoadComplete	= delegate ( StreamData streamData ) { return null; };
	private			StreamingState					m_SaveLoadState		= StreamingState.NONE;

#region INTERFACE
	// INTERFACE START
	/// <summary> Streaming events interface </summary>
	public static	StreamEvents		StreamEvents
	{
		get { return m_StreamEvents; }
	}

	/// <summary> Return the current stream State </summary>
	StreamingState	StreamEvents.State
	{
		get { return m_SaveLoadState; }
	}

	/// <summary> Events called when game is saving </summary>
	event GameEvents.StreamingEvent StreamEvents.OnSave
	{
		add		{ if ( value != null )	m_OnSave += value; }
		remove	{ if ( value != null )	m_OnSave -= value; }
	}

	/// <summary> Events called when game is saving </summary>
	event GameEvents.StreamingEvent StreamEvents.OnSaveComplete
	{
		add		{ if ( value != null )	m_OnSaveComplete += value; }
		remove	{ if ( value != null )	m_OnSaveComplete -= value; }
	}

	/// <summary> Events called when game is loading </summary>
	event GameEvents.StreamingEvent StreamEvents.OnLoad
	{
		add		{ if ( value != null )	m_OnLoad += value; }
		remove	{ if ( value != null )	m_OnLoad -= value; }
	}

	/// <summary> Events called when game is loading </summary>
	event GameEvents.StreamingEvent StreamEvents.OnLoadComplete
	{
		add		{ if ( value != null )	m_OnLoadComplete += value; }
		remove	{ if ( value != null )	m_OnLoadComplete -= value; }
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
	void						StreamEvents.Save( string filePath, bool isAutomaic )
	{
		// TODO: CHECK FOR AUTOMAIC SAVE

		// Conditions
		if ( m_SaveLoadState == StreamingState.SAVING )
		{
			UnityEngine.Debug.Log( "Another save must finish write actions !!" );
			return;
		}

		m_SaveLoadState = StreamingState.SAVING;

		PlayerPrefs.SetInt( "SaveSceneIdx", UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex );
		PlayerPrefs.SetString( "SaveFilePath", filePath );

		StreamData streamData = new StreamData();

		// call all save callbacks
		m_OnSave( streamData );

		// Thread Body
		System.Action body = delegate()
		{
			print( "Saving..." );

			// Serialize data
			string toSave = JsonUtility.ToJson( streamData, prettyPrint: true );
//			Encrypt( ref toSave );
			File.WriteAllText( filePath, toSave );
		};

		// Thread OnCompletion
		System.Action onCompletion = delegate()
		{
			m_SaveLoadState = StreamingState.SAVE_COMPLETE;
			m_OnSaveComplete( streamData );
			print( "Saved!" );
		};
		MultiThreading.CreateThread( body: body, bCanStart: true, onCompletion: onCompletion );
	}
	
	//////////////////////////////////////////////////////////////////////////
	/// <summary> Used to load data for all registered objects </summary>
	void						StreamEvents.Load( string fileName )
	{
		// Conditions
		if ( m_SaveLoadState == StreamingState.SAVING || m_SaveLoadState == StreamingState.LOADING )
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
		m_SaveLoadState = StreamingState.LOADING;
		InputManager.IsEnabled = false;
		print( "Loading "  + System.IO.Path.GetFileNameWithoutExtension( fileName ) + "..." );

		// Deserialize data
		string toLoad = File.ReadAllText( fileName );
//		Decrypt( ref toLoad );
		StreamData streamData = JsonUtility.FromJson< StreamData >( toLoad );

		if ( streamData != null )
		{
			StartCoroutine( LoadCO( streamData ) );
		}
		else
		{
			m_SaveLoadState = StreamingState.LOAD_COMPLETE;
			InputManager.IsEnabled = true;
			Debug.LogError( "GameManager::Load:: Save \"" + fileName + "\" cannot be loaded" );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private IEnumerator LoadCO( StreamData streamData )
	{
		yield return null;

		// call all load callbacks
		m_OnLoad( streamData );

		yield return null;

		m_OnLoadComplete( streamData );
		print( "Loaded!" );

		m_SaveLoadState = StreamingState.LOAD_COMPLETE;
		InputManager.IsEnabled = true;
	}

	
	// https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp
	//////////////////////////////////////////////////////////////////////////
	private		void			Encrypt( ref string clearText )
	{
		// TODO re-enable encryption
#pragma warning disable CS0162 // È stato rilevato codice non raggiungibile
		byte[] clearBytes = Encoding.Unicode.GetBytes( clearText );
		m_Encryptor.Key = m_PDB.GetBytes( 32 );
		m_Encryptor.IV = m_PDB.GetBytes( 16 );

		MemoryStream stream = new MemoryStream();
		{
			CryptoStream crypter = new CryptoStream( stream, m_Encryptor.CreateEncryptor(), CryptoStreamMode.Write );
			{
				crypter.Write( clearBytes, 0, clearBytes.Length );
			}
			crypter.Close();
			crypter.Dispose();
			clearText = System.Convert.ToBase64String( stream.ToArray() );
		}
		stream.Dispose();
#pragma warning restore CS0162 // È stato rilevato codice non raggiungibile
	}


	//////////////////////////////////////////////////////////////////////////
	private		void			Decrypt( ref string cipherText )
	{
		// TODO re-enable decryption
#pragma warning disable CS0162 // È stato rilevato codice non raggiungibile
		cipherText = cipherText.Replace( " ", "+" );
		byte[] cipherBytes = System.Convert.FromBase64String( cipherText );
		m_Encryptor.Key = m_PDB.GetBytes( 32 );
		m_Encryptor.IV = m_PDB.GetBytes( 16 );

		MemoryStream stream = new MemoryStream();
		{
			CryptoStream crypter = new CryptoStream( stream, m_Encryptor.CreateDecryptor(), CryptoStreamMode.Write );
			{
				crypter.Write( cipherBytes, 0, cipherBytes.Length );
			}
			cipherText = Encoding.Unicode.GetString( stream.ToArray() );
		//	crypter.Close();
		//	crypter.Dispose();
		}
		stream.Dispose();
#pragma warning restore CS0162 // È stato rilevato codice non raggiungibile
	}
}


[System.Serializable]
public class StreamUnit {

	[SerializeField]
	public	int						InstanceID		= -1;

	[SerializeField]
	public	string					Name			= "";

	[SerializeField]
	public	Vector3					Position		= Vector3.zero;

	[SerializeField]
	public	Quaternion				Rotation		= Quaternion.identity;

	[SerializeField]
	private	List<MyKeyValuePair>	Internals		= new List<MyKeyValuePair>();


	//////////////////////////////////////////////////////////////////////////
	public	void		SetInternal( string key, object value )
	{
		MyKeyValuePair keyValue = null;
		int index = Internals.FindIndex( ( MyKeyValuePair kv ) => kv.Key == key );
		if ( index == -1 )
		{
			//SetInternal( key, value );
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
		MyKeyValuePair keyValue = Internals.Find( ( MyKeyValuePair kv ) => kv.Key == key );
		return keyValue != null;
	}


	//////////////////////////////////////////////////////////////////////////
	public	string		GetInternal( string key )
	{
		MyKeyValuePair keyValue = Internals.Find( ( MyKeyValuePair kv ) => kv.Key == key );
		if ( keyValue == null || keyValue.Value == null )
		{
			Debug.Log( "Cannot retrieve value for key " + key );
			return "";
		}
		return keyValue.Value;
	}


	//////////////////////////////////////////////////////////////////////////
	public	bool		GetAsBool( string key )
	{
		string value = GetInternal( key );
		bool result = false;
		if ( bool.TryParse( value, out result ) == false )
		{
			Debug.Log( "Cannot retrieve value for key  " + key + " as BOOLEAN" );
		}
		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	public	int			GetAsInt( string key )
	{
		string value = GetInternal( key );
		int result = 0;
		if ( int.TryParse( value, out result ) == false )
		{
			Debug.Log( "Cannot retrieve value for key  " + key + " as INTEGER" );
		}
		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	public	float		GetAsFloat( string key )
	{
		string value = GetInternal( key );
		float result = 0f;
		if ( float.TryParse( value, out result ) == false )
		{
			Debug.Log( "Cannot retrieve value for key  " + key + " as FLOAT" );
		}
		return result;
	}



	//////////////////////////////////////////////////////////////////////////
	public	T			GetAsEnum<T>( string key )
	{
		string value = GetInternal( key );
		T result = default( T );
		if ( Utils.Converters.StringToEnum( value, ref result ) == false )
		{
			Debug.Log( "Cannot retrieve value for key  " + key + " as ENUM" );
		}

		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	public Vector3		GetAsVector( string key )
	{
		string value = GetInternal( key );
		Vector3 result = Vector3.zero;
		if ( Utils.Converters.StringToVector( value, ref result ) == false )
		{
			Debug.Log( "Cannot retrieve value for key  " + key + " as VECTOR" );
		}
		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	public Quaternion	GetAsQuaternion( string key )
	{
		string value = GetInternal( key );
		Quaternion result = Quaternion.identity;
		if ( Utils.Converters.StringToQuaternion( value, ref result ) == false )
		{
			Debug.Log( "Cannot retrieve value for key  " + key + " as QUATERNION" );
		}
		return result;
	}
}

[System.Serializable]
public class StreamData {

	[SerializeField]
	private List<StreamUnit> m_Data = new List<StreamUnit>();


	//////////////////////////////////////////////////////////////////////////
	/// <summary> To be used ONLY during the SAVE event, otherwise nothing happens </summary>
	public	StreamUnit	NewUnit( GameObject gameObject )
	{
		if ( GameManager.StreamEvents.State != StreamingState.SAVING )
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
	public	bool		GetUnit( GameObject gameObject, ref StreamUnit streamUnit )
	{
//		if ( GameManager.StreamEvents.State != StreamingState.LOAD_COMPLETE )
//			return false;

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

public interface PauseEvents {
	/// <summary> Events called when game is setting on pause </summary>
		event		GameEvents.OnPauseSetEvent		OnPauseSet;

					void							SetPauseState( bool bPauseState );
}

// PAUSE IMPLEMENTATION
public partial class GameManager : PauseEvents {

	private	static	PauseEvents	m_PauseEvents = null;
	public	static	PauseEvents	PauseEvents
	{
		get { return m_PauseEvents; }
	}

	// Event
	private static event	GameEvents.OnPauseSetEvent	m_OnPauseSet			= delegate { };

	/// <summary> Events called when game is setting on pause </summary>
	event GameEvents.OnPauseSetEvent PauseEvents.OnPauseSet
	{
		add		{ if ( value != null )	m_OnPauseSet += value; }
		remove	{ if ( value != null )	m_OnPauseSet -= value; }
	}

	// Vars
	private	static			bool				m_IsPaused				= false;
	public	static			bool				IsPaused
	{
		get { return m_IsPaused; }
		set { m_PauseEvents.SetPauseState( value ); }
	}

	private					float				m_PrevTimeScale			= 1f;
	private					bool				m_PrevCanParseInput		= false;
	private					bool				m_PrevInputEnabled		= false;


	//////////////////////////////////////////////////////////////////////////
	void	PauseEvents.SetPauseState( bool bPauseState )
	{
		if ( bPauseState == m_IsPaused )
			return;

		m_OnPauseSet( bPauseState );

		m_IsPaused = bPauseState;
		UI.Instance.GoToMenu( UI.Instance.InGame.transform );
		UI.Instance.SetPauseMenuState( bPauseState );

		Cursor.visible = bPauseState == true;
		Cursor.lockState = bPauseState == true ? CursorLockMode.None : CursorLockMode.Locked;
		
		if ( bPauseState == true )
		{
			m_PrevTimeScale							= Time.timeScale;
			m_PrevCanParseInput						= CameraControl.Instance.CanParseInput;
			m_PrevInputEnabled						= InputManager.IsEnabled;
			Time.timeScale							= 0f;
			CameraControl.Instance.CanParseInput	= false;
			InputManager.IsEnabled					= false;
		}
		else
		{
			Time.timeScale							= m_PrevTimeScale;
			CameraControl.Instance.CanParseInput	= m_PrevCanParseInput;
			InputManager.IsEnabled					= m_PrevInputEnabled;
		}
		m_SkipOneFrame = true;
	}

}



//////////////////////////////////////////////////////////////////
//							UPDATES								//
//////////////////////////////////////////////////////////////////

public interface UpdateEvents {
	/// <summary> TODO </summary>
		event		GameEvents.OnThinkEvent			OnThink;

	/// <summary> TODO </summary>
		event		GameEvents.OnPhysicFrameEvent	OnPhysicFrame;

	/// <summary> TODO </summary>
		event		GameEvents.OnFrameEvent			OnFrame;

		event		GameEvents.OnFrameEvent			OnLateFrame;
}

// UPDATES IMPLEMENTATION
public partial class GameManager : UpdateEvents {

	private	static	UpdateEvents		m_UpdateEvents	= null;
	public	static	UpdateEvents		UpdateEvents
	{
		get { return m_UpdateEvents; }
	}

	private static event	GameEvents.OnThinkEvent			m_OnThink				= delegate { };
	private static event	GameEvents.OnPhysicFrameEvent	m_OnPhysicFrame			= delegate { };
	private static event	GameEvents.OnFrameEvent			m_OnFrame				= delegate { };
	private static event	GameEvents.OnFrameEvent			m_OnLateFrame			= delegate { };


	event		GameEvents.OnThinkEvent			UpdateEvents.OnThink
	{
		add		{	if ( value != null )	m_OnThink += value;	}
		remove	{	if ( value != null )	m_OnThink -= value;	}
	}

	event		GameEvents.OnPhysicFrameEvent	UpdateEvents.OnPhysicFrame
	{
		add		{	if ( value != null )	m_OnPhysicFrame += value; }
		remove	{	if ( value != null )	m_OnPhysicFrame -= value; }
	}

	event		GameEvents.OnFrameEvent			UpdateEvents.OnFrame
	{
		add		{	if ( value != null )	m_OnFrame += value;	}
		remove	{	if ( value != null )	m_OnFrame -= value; }
	}

	event		GameEvents.OnFrameEvent			UpdateEvents.OnLateFrame
	{
		add		{	if ( value != null )	m_OnLateFrame += value;	}
		remove	{	if ( value != null )	m_OnLateFrame -= value; }
	}
}


[System.Serializable]
public class MyKeyValuePair {
	[SerializeField]
	public	string	Key;
	[SerializeField]
	public	string	Value;

	public	MyKeyValuePair( string Key, string Value )
	{
		this.Key	= Key;
		this.Value	= Value;
	}
}