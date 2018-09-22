
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Threading;

//	DELEGATES FOR EVENTS
public delegate StreamUnit StreamingEvent( StreamData streamData );


public class SaveFileinfo {

	public	string	fileName	= "";
	public	string	filePath	= "";
	public	string	saveTime	= "";
	public	bool	isAutomatic = false;

}

public enum SaveLoadState {
	NONE, SAVING, LOADING
}

// INTERFACE
/// <summary> Clean interface of only GameManager class </summary>
public interface SaveManagement {

	// PROPERTIES

	/// <summary> Events called when game is saving </summary>
			event	StreamingEvent		OnSave;

	/// <summary> Events called when game is loading </summary>
			event	StreamingEvent		OnLoad;

	/// <summary> Save current play </summary>
					void				Save	( string fileName, bool isAutomaic = false );

	/// <summary> Load a file </summary>
					void				Load	( string fileName );

	SaveLoadState			State	{ get; }
}


public partial class GameManager : SaveManagement {

	private const	string				ENCRIPTION_KEY	= "Boris474Ever";

	private	static	SaveManagement		m_SaveManagement = null;
	public	static	SaveManagement		SaveManagement
	{
		get { return m_SaveManagement; }
	}
	
	/// <summary> Events called when game is saving </summary>
	public event StreamingEvent			OnSave			= null;

	/// <summary> Events called when game is loading </summary>
	public event StreamingEvent			OnLoad			= null;


	private		Coroutine				m_SaveLoadCO	= null;
	private		Thread					m_SavingThread	= null;

	private		Rfc2898DeriveBytes		m_PDB			= new Rfc2898DeriveBytes( ENCRIPTION_KEY,
				new byte[] {
					0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
				} );

	private		Aes						m_Encryptor		= Aes.Create();

	private		SaveLoadState			m_SaveLoadState	= SaveLoadState.NONE;
	SaveLoadState	SaveManagement.State
	{
		get { return m_SaveLoadState; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Save
	/// <summary> Used to save data of puzzles </summary>
	public		void			Save( string fileName = "SaveFile.txt", bool isAutomaic = false )
	{
		// TODO: CHECK FOR AUTOMAIC SAVE

		if ( m_SaveLoadCO != null )
		{
			UnityEngine.Debug.Log( "Another save must finish write actions !!" );
			return;
		}

		m_SaveLoadState = SaveLoadState.SAVING;

		StreamData streamData = new StreamData();

		// call all save callbacks
		OnSave( streamData );

		// write data on disk
		string toSave = JsonUtility.ToJson( streamData, prettyPrint: false );

		print( "Saving" );
		toSave = Encrypt( toSave );
		m_SavingThread = new Thread( () => { File.WriteAllText( fileName, toSave ); });
		m_SavingThread.Start();
		m_SaveLoadCO = StartCoroutine( SaveLoadCO( m_SavingThread ) );
	}


	//////////////////////////////////////////////////////////////////////////
	// SaveLoadCO ( Coroutine )
	private	IEnumerator SaveLoadCO( Thread savingThread )
	{
		while( savingThread.ThreadState == ThreadState.Running )
		{
			yield return null;
		}
		print( "Saved" );
		m_SavingThread = null;
		m_SaveLoadCO = null;

		
		if ( PlayerPrefs.HasKey( "SaveSceneIdx" ) == false )
		{
			PlayerPrefs.DeleteKey( "SaveSceneIdx" );
			PlayerPrefs.SetInt( "SaveSceneIdx", UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex );
		}

		m_SaveLoadState = SaveLoadState.NONE;
	}


	//////////////////////////////////////////////////////////////////////////
	// Load
	/// <summary> Used to send signal of load to all registered callbacks </summary>
	public		void			Load( string fileName = "SaveFile.txt" )
	{
		if ( m_SaveLoadCO != null || fileName == null || fileName.Length == 0 )
			return;
		
		m_SaveLoadState = SaveLoadState.LOADING;

		InputManager.IsEnabled = false;

		// load data from disk
		string toLoad = File.ReadAllText( fileName );
		StreamData streamData = JsonUtility.FromJson< StreamData >( Decrypt( toLoad ) );
		if ( streamData == null )
		{
			Debug.LogError( "GameManager::Load:: Save \"" + fileName +"\" cannot be loaded" );
			return;
		}

		// call all load callbacks
		OnLoad( streamData );

		InputManager.IsEnabled = true;

		m_SaveLoadState = SaveLoadState.NONE;
	}

	
	// https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp
	//////////////////////////////////////////////////////////////////////////
	// Encrypt
	private string Encrypt( string clearText )
	{
		return clearText;

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
		return clearText;
#pragma warning restore CS0162 // È stato rilevato codice non raggiungibile
	}


	//////////////////////////////////////////////////////////////////////////
	// Decrypt
	private string Decrypt( string cipherText )
	{
		return cipherText;

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
			crypter.Close();
			crypter.Dispose();
			cipherText = Encoding.Unicode.GetString( stream.ToArray() );
		}
		stream.Dispose();
		return cipherText;
#pragma warning restore CS0162 // È stato rilevato codice non raggiungibile
	}


}

[System.Serializable]
public	class MyKeyValuePair {
	[SerializeField]
	public	string	Key;
	[SerializeField]
	public	string	Value;
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
	// AddInternal
	public	void		AddInternal( string key, object value )
	{
		MyKeyValuePair keyValue = new MyKeyValuePair();
		keyValue.Key	= key;
		keyValue.Value	= value.ToString();
		Internals.Add( keyValue );
	}


	//////////////////////////////////////////////////////////////////////////
	// HasInternal
	public	bool		HasInternal( string key )
	{
		MyKeyValuePair keyValue = Internals.Find( ( MyKeyValuePair kv ) => kv.Key == key );
		return keyValue != null;
	}


	//////////////////////////////////////////////////////////////////////////
	// GetInternal
	private	string		GetInternal( string key )
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
	// GetAsBool
	public	bool		GetAsBool( string key )
	{
		string value = GetInternal( key );
		bool result = false;
		bool.TryParse( value, out result );
		return result;
	}

	//////////////////////////////////////////////////////////////////////////
	// GetAsInt
	public	int			GetAsInt( string key )
	{
		string value = GetInternal( key );
		int result = 0;
		int.TryParse( value, out result );
		return result;
	}

	//////////////////////////////////////////////////////////////////////////
	// GetAsFloat
	public	float		GetAsFloat( string key )
	{
		string value = GetInternal( key );
		float result = 0f;
		float.TryParse( value, out result );
		return result;
	}

	//////////////////////////////////////////////////////////////////////////
	// GetAsEnum
	public	T			GetAsEnum<T>( string key )
	{
		string value = GetInternal( key );
		return ( T ) System.Enum.Parse( typeof( T ), value );
	}

	//////////////////////////////////////////////////////////////////////////
	// GetAsEnum
	public Vector3		GetAsVector( string key )
	{
		string value = GetInternal( key );
		Vector3 result = Vector3.zero;
		Utils.Converters.StringToVector( value, ref result );
		return result;
	}

	//////////////////////////////////////////////////////////////////////////
	// GetAsQuaternion
	public Quaternion	GetAsQuaternion( string key )
	{
		string value = GetInternal( key );
		Quaternion result = Quaternion.identity;
		Utils.Converters.StringToQuaternion( value, ref result );
		return result;
	}

}


[SerializeField]
public class StreamData {

	[SerializeField]
	private List<StreamUnit>		m_Data = new List<StreamUnit>();


	//////////////////////////////////////////////////////////////////////////
	/// <summary> To be used ONLY during the SAVE event </summary>
	public	StreamUnit	NewUnit( GameObject gameObject )
	{
		if ( GameManager.SaveManagement.State != SaveLoadState.SAVING )
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
	/// <summary> To be used ONLY during the LOAD event </summary>
	public	bool		GetUnit( GameObject gameObject, ref StreamUnit streamUnit )
	{
		if ( GameManager.SaveManagement.State != SaveLoadState.LOADING )
			return false;

//		Debug.Log( gameObject.name );
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
