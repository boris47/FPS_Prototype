
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Threading;

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

	private const	string				ENCRIPTION_KEY	= "Boris474Ever";
	
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

		StreamingData streamingData = new StreamingData();

		// call all save callbacks
		OnSave( streamingData );

		// write data on disk
		string toSave = JsonUtility.ToJson( streamingData, prettyPrint: false );

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
	}


	//////////////////////////////////////////////////////////////////////////
	// Load
	/// <summary> Used to send signal of load to all registered callbacks </summary>
	public		void			Load( string fileName = "SaveFile.txt" )
	{
		if ( m_SaveLoadCO != null || fileName == null || fileName.Length == 0 )
			return;
		
		InputManager.IsEnabled = false;

		// load data from disk
		string toLoad = File.ReadAllText( fileName );
		StreamingData streamingData = JsonUtility.FromJson< StreamingData >( Decrypt( toLoad ) );
		if ( streamingData == null )
		{
			Debug.LogError( "GameManager::Load:: Save \"" + fileName +"\" cannot be loaded" );
			return;
		}

		// call all load callbacks
		OnLoad( streamingData );

		InputManager.IsEnabled = true;
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
public class StreamingUnit {

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
public class StreamingData {

	[SerializeField]
	private List<StreamingUnit>		m_Data = new List<StreamingUnit>();


	//////////////////////////////////////////////////////////////////////////
	// NewUnit
	public	StreamingUnit	NewUnit( GameObject gameObject )
	{
		StreamingUnit streamingUnit		= new StreamingUnit();

		int index = m_Data.FindIndex( ( StreamingUnit data ) => data.InstanceID == gameObject.GetInstanceID() );
		if ( index > -1 )
		{
			Debug.Log( gameObject.name + " already saved" );
			streamingUnit = m_Data[index];
		}

		streamingUnit.InstanceID		= gameObject.GetInstanceID();
		streamingUnit.Name				= gameObject.name;

		m_Data.Add( streamingUnit );

		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// GetUnit
	public	bool	GetUnit( GameObject gameObject, ref StreamingUnit streamingUnit )
	{
//		Debug.Log( gameObject.name );
		int GOInstanceID = gameObject.GetInstanceID();
		int index = m_Data.FindIndex( ( StreamingUnit data ) => data.InstanceID == GOInstanceID );

		if ( index == -1 )
		{
			index = m_Data.FindIndex( ( StreamingUnit data ) => data.Name == gameObject.name );
		}

		bool found = index > -1;
		if ( found )
		{
			streamingUnit = m_Data[ index ];
		}

		return found;
	}

}
