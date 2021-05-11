
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;



//	DELEGATES FOR EVENTS
public partial struct GameEvents
{
	// SAVE & LOAD
	public	delegate	StreamUnit		StreamingEvent(StreamData streamData);		// StreamEvents.OnSave & StreamEvents.OnLoad
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
public interface ISaveAndLoad
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
public sealed partial class GameManager : ISaveAndLoad
{
	private const			string							ENCRIPTION_KEY			= "Boris474Ever";

	/// <summary> Streaming events interface </summary>
	public	static			ISaveAndLoad					SaveAndLoad			=> m_Instance;
	
	/// <summary> Return the current stream State </summary>
	EStreamingState			ISaveAndLoad.State				=> m_SaveAndLoadState;

	public class SaveFileinfo
	{
		public	string	fileName	= "";
		public	string	filePath	= "";
		public	string	saveTime	= "";
		public	bool	isAutomatic = false;
	}

	// Events
	private	event			GameEvents.StreamingEvent		m_OnSave					= delegate { return null; };
	private	event			GameEvents.StreamingEvent		m_OnSaveComplete			= delegate { return null; };

	private					List<GameEvents.StreamingEvent>	m_OnLoad					= new List<GameEvents.StreamingEvent>();
	private					List<GameEvents.StreamingEvent>	m_OnLoadComplete			= new List<GameEvents.StreamingEvent>();
	private					EStreamingState					m_SaveAndLoadState			= EStreamingState.NONE;

	#region INTERFACE
	/// <summary> Events called when game is saving </summary>
	event GameEvents.StreamingEvent ISaveAndLoad.OnSave
	{
		add		{ if (value.IsNotNull())	m_OnSave += value; }
		remove	{ if (value.IsNotNull())	m_OnSave -= value; }
	}

	/// <summary> Events called when game is saving </summary>
	event GameEvents.StreamingEvent ISaveAndLoad.OnSaveComplete
	{
		add		{ if (value.IsNotNull())	m_OnSaveComplete += value; }
		remove	{ if (value.IsNotNull())	m_OnSaveComplete -= value; }
	}

	/// <summary> Events called when game is loading ( Events are called along more frames !! ) </summary>
	event GameEvents.StreamingEvent ISaveAndLoad.OnLoad
	{
		add		{ if (value.IsNotNull()) m_OnLoad.Add(value); }
		remove	{ if (value.IsNotNull()) m_OnLoad.Remove(value); }
	}

	/// <summary> Events called when game has been loaded ( Events are called along more frames !! ) </summary>
	event GameEvents.StreamingEvent ISaveAndLoad.OnLoadComplete
	{
		add		{ if (value.IsNotNull()) m_OnLoadComplete.Add(value); }
		remove	{ if (value.IsNotNull()) m_OnLoadComplete.Remove(value); }
	}
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
	void ISaveAndLoad.Save(string filePath, bool isAutomaic)
	{
		// TODO: CHECK FOR AUTOMAIC SAVE

		// Conditions
		if (m_SaveAndLoadState == EStreamingState.SAVING)
		{
			UnityEngine.Debug.LogError("Another save must finish write actions !!");
			return;
		}
		m_SaveAndLoadState = EStreamingState.SAVING;

		int sceneIndex = CustomSceneManager.CurrentSceneIndex;
			//UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
		StreamData streamData = new StreamData(sceneIndex);
		m_OnSave(streamData);

		// Thread Body
		void body()
		{
			print("Saving...");

			// Serialize data
			string toSave = JsonUtility.ToJson(streamData, prettyPrint: true);
			//Encrypt( ref toSave );
			System.IO.File.WriteAllText(filePath, toSave, Encoding.UTF8);
		}

		// Thread OnCompletion
		void onCompletion()
		{
			m_SaveAndLoadState = EStreamingState.SAVE_COMPLETE;
			m_OnSaveComplete(null);
			PlayerPrefs.SetString("SaveFilePath", filePath);
			print("Saved!");
		}
		MultiThreading.CreateThread(body, bCanStart: true, onCompletion);
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Used to load data for all registered objects </summary>
	void ISaveAndLoad.Load(string fileName)
	{
		// Conditions
		if (m_SaveAndLoadState == EStreamingState.SAVING || m_SaveAndLoadState == EStreamingState.LOADING)
		{
			UnityEngine.Debug.Log("Cannot load while loading or saving !!");
			return;
		}

		if (string.IsNullOrEmpty(fileName))
		{
			UnityEngine.Debug.Log($"Cannot load because invalid filename(${fileName}) !!");
			return;
		}

		// Body
		m_SaveAndLoadState = EStreamingState.LOADING;
		InputManager.IsEnabled = false;
		print($"Loading {System.IO.Path.GetFileNameWithoutExtension(fileName)}...");

		// Deserialize data
		string toLoad = System.IO.File.ReadAllText(fileName, Encoding.UTF8);
		//Decrypt( ref toLoad );
		StreamData streamData = JsonUtility.FromJson<StreamData>(toLoad);
		if (streamData.IsNotNull())
		{
			CoroutinesManager.Start(LoadCO(streamData), "GameManager::Load: Start of loading");
		}
		else
		{
			m_SaveAndLoadState = EStreamingState.LOAD_COMPLETE;
			InputManager.IsEnabled = true;
			Debug.LogError($"GameManager::Load:: Save \"{fileName}\" cannot be loaded");
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private IEnumerator LoadCO(StreamData streamData)
	{
		CoroutinesManager.AddCoroutineToPendingCount(1);
		yield return null;

		List<GameEvents.StreamingEvent> copy = m_OnLoad.ConvertAll(e => new GameEvents.StreamingEvent(e));

		foreach (GameEvents.StreamingEvent _delegate in copy)
		{
			_delegate(streamData);
			yield return null;
		}

		yield return null;

		foreach (GameEvents.StreamingEvent _delegate in m_OnLoadComplete)
		{
			_delegate(streamData);
			yield return null;
		}

		print("Loaded!");

		m_SaveAndLoadState = EStreamingState.LOAD_COMPLETE;
		CoroutinesManager.RemoveCoroutineFromPendingCount(1);
	}


	// https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp
	//////////////////////////////////////////////////////////////////////////
	private void Encrypt(ref string clearText)
	{
		byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
		m_Encryptor.Key = m_PDB.GetBytes(32); m_Encryptor.IV = m_PDB.GetBytes(16);

		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			using (CryptoStream crypter = new CryptoStream(stream, m_Encryptor.CreateEncryptor(), CryptoStreamMode.Write))
			{
				crypter.Write(clearBytes, 0, clearBytes.Length);
			}
			clearText = System.Convert.ToBase64String(stream.ToArray());
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void Decrypt(ref string cipherText)
	{
		byte[] cipherBytes = System.Convert.FromBase64String(cipherText.Replace(" ", "+"));
		m_Encryptor.Key = m_PDB.GetBytes(32); m_Encryptor.IV = m_PDB.GetBytes(16);

		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			using (CryptoStream crypter = new CryptoStream(stream, m_Encryptor.CreateDecryptor(), CryptoStreamMode.Write))
			{
				crypter.Write(cipherBytes, 0, cipherBytes.Length);
			}
			cipherText = Encoding.Unicode.GetString(stream.ToArray());
		}
	}


	private void ResetSaveAndLoadEvens()
	{
		m_OnSave							= delegate { return null; };
		m_OnSaveComplete					= delegate { return null; };
		m_OnLoad.Clear();					//= new List<GameEvents.StreamingEvent>();
		m_OnLoadComplete.Clear();			//= new List<GameEvents.StreamingEvent>();
		m_SaveAndLoadState					= EStreamingState.NONE;
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


	//////////////////////////////////////////////////////////////////////////
	public static implicit operator bool(StreamUnit unit) => unit.IsNotNull();
}

[System.Serializable]
public class StreamData
{
	[SerializeField]
	private		int						m_SceneIndex			= -1;
	[SerializeField]
	private		List<StreamUnit>		m_Data					= new List<StreamUnit>();

	public StreamData(int sceneIndex)
	{
		m_SceneIndex = sceneIndex;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> To be used ONLY during the SAVE event, otherwise nothing happens </summary>
	public StreamUnit NewUnit(Object unityObj)
	{
		CustomAssertions.IsTrue(GameManager.SaveAndLoad.State == EStreamingState.SAVING);

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
	public bool TryGetUnit(Object unityObj, out StreamUnit streamUnit)
	{
		//	if ( GameManager.StreamEvents.State != StreamingState.LOAD_COMPLETE )
		//		return false;

		CustomAssertions.IsTrue(GameManager.SaveAndLoad.State == EStreamingState.LOADING);

		streamUnit = m_Data.Find((StreamUnit data) => data.Name == unityObj.name);
		return streamUnit.IsNotNull(); ;
	}
}