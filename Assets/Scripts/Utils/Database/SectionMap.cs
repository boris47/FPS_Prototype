
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Database;
using System.IO;
using System.Reflection;


public class SectionMap {

	// READING PHASES
	private enum EReadingPhase
	{
		NONE					= 0,
		READING_SECTION			= 1,
		READING_SECTION_LIST	= 2,
		READING_ARRAYDATA		= 3
	}


	// CONTAINERS
	private	Dictionary < string, Section >		m_SectionMap		= new Dictionary < string, Section > ();
	private	Dictionary < string, ArrayData >	m_ArrayDataMap		= new Dictionary< string, ArrayData > ();
	private	List < string >						m_FilePaths			= new List < string >();

	// INTERNAL VARS
	private	EReadingPhase						m_ReadingPhase		= EReadingPhase.NONE;
	private	Section								m_CurrentSection	= null;
	private	ArrayData							m_CurrentArrayData	= null;
	private	cLineValue							m_CurrentLineValue	= null;
	private	cMultiValue							m_CurrentMultiValue	= null;

	public	bool								IsOK
	{
		get; private set;
	}
	

	//////////////////////////////////////////////////////////////////////////
	// Section_Create
	// CREATE A NEW SECTION WHILE READING FILE
	private bool Section_Create( string sLine, string sFilePath, int iLine )
	{
		if (this.m_CurrentSection != null )
		{
			this.Section_Close();
		}

		int sectionNameCloseChar = sLine.IndexOf( "]", 0 );
		string sSectionName = sLine.Substring( 1, sectionNameCloseChar - 1 );

		Section bump = null;
		if (this.HasFileElement( sSectionName, ref bump ) )
		{
			Debug.LogError( "SectionMap::Section_Create:" + sFilePath + ":[" + iLine + "]: Section \"" + sSectionName + "\" already exists!" );
			return false;
		}

		this.m_ReadingPhase = EReadingPhase.READING_SECTION;

		string context = System.IO.Path.GetFileNameWithoutExtension( sFilePath );
		this.m_CurrentSection = new Section( sSectionName, context: context );

		// Get the name of mother section, if present
		int iIndex = sLine.IndexOf( ":", sectionNameCloseChar );
		if ( iIndex > 0 )
		{
			string[] mothers = sLine.Substring( iIndex + 1 ).Split( ',' );
			if ( mothers.Length == 0 )
			{
				Debug.LogError( "SectionMap::Section_Create:" + sFilePath + ":[" + iLine + "]: Section Mothers bad definition!" );
				return false;
			}

			foreach ( string motherName in mothers )
			{
				Section motherSection = null;
				if (this.GetSection( motherName, ref motherSection ) )
				{
					this.m_CurrentSection += motherSection;
				} else
				{
					Debug.LogError( "SectionMap::Section_Create:" + sFilePath + ":[" + iLine + "]: Section requested for inheritance \"" + motherName + "\" doesn't exist!" );
				}
			}
		}
			
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// Section_Add
	// INSERT A KEY VALUE PAIR INSIDE THE ACTUAL PARSING SECTION
	private bool Section_Add( KeyValue Pair, string sFilePath, int iLine )
	{
		cLineValue pLineValue = new cLineValue( Pair.Key , Pair.Value );
		if ( pLineValue.IsOK == false )
		{
			Debug.LogError( " SectionMap::Section_Add:LineValue invalid for key |" + Pair.Key + "| in Section |" + this.m_CurrentSection.GetName() + "| in file |" + sFilePath + "|" );
			return false;
		}
		this.m_CurrentSection.Add( pLineValue );
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// Section_Close
	// DEFINITELY SAVE THE PARSING SECTION
	private void Section_Close()
	{
		if (this.m_CurrentSection != null )
			this.m_SectionMap.Add(this.m_CurrentSection.GetName(), this.m_CurrentSection );

		this.m_CurrentSection = null;
		this.m_ReadingPhase = EReadingPhase.NONE;
	}


	//////////////////////////////////////////////////////////////////////////
	// ArrayData_Create
	// CREATE A NEW ARRAY DATA LIST WHILE READING FILE
	private bool ArrayData_Create( string sLine, string sFilePath, int iLine )
	{
		if (this.m_CurrentArrayData != null )
		{
			this.ArrayData_Close();
		}

		int ArrayDataNameCloseChar = sLine.IndexOf( "\'", 1 );
		string sArrayDataName = sLine.Substring( 1, ArrayDataNameCloseChar - 1 );

		ArrayData bump = null;
		if (this.HasFileElement( sArrayDataName, ref bump ) )
		{
			Debug.LogError( "SectionMap::ArrayData_Create:" + sFilePath + ":[" + iLine + "]: ArrayData \"" + sArrayDataName + "\" already exists!" );
			return false;
		}

		this.m_ReadingPhase = EReadingPhase.READING_ARRAYDATA;

		string context = System.IO.Path.GetFileNameWithoutExtension( sFilePath );
		this.m_CurrentArrayData = new ArrayData( sArrayDataName, context: context );

		// Get the name of mother , if present
		int iIndex = sLine.IndexOf( ":", ArrayDataNameCloseChar );
		if ( iIndex > 0 )
		{
			string[] mothers = sLine.Substring( iIndex + 1 ).Split( ',' );
			if ( mothers.Length == 0 )
			{
				Debug.LogError( "SectionMap::ArrayData_Create:" + sFilePath + ":[" + iLine + "]: ArrayData Mothers bad definition!" );
				return false;
			}

			foreach ( string motherName in mothers )
			{
				ArrayData mother = null;
				if (this.bGetArrayData( motherName, ref mother ) )
				{
					this.m_CurrentArrayData += mother;
				} else
				{
					Debug.LogError( "SectionMap::ArrayData_Create:" + sFilePath + ":[" + iLine + "]: ArrayData requested for inheritance \"" + motherName + "\" doesn't exist!" );
				}
			}
		}
			
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// ArrayData_Add
	// INSERT VALUE INSIDE THE ACTUAL PARSING ARRAY DATA LIST
	private	bool	ArrayData_Add( string sLine, string sFilePath, int iLine )
	{
		cLineValue pLineValue = new cLineValue( sFilePath + "_"+ iLine , sLine );
		if ( pLineValue.IsOK == false )
		{
			Debug.LogError( " SectionMap::ArrayData_Add: LineValue invalid at line |" + iLine + "| in Section |" + this.m_CurrentArrayData.GetName() + "| in file |" + sFilePath + "|" );
			return false;
		}
		this.m_CurrentArrayData.Add( pLineValue );
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// Section_Close
	// DEFINITELY SAVE THE PARSING SECTION
	private void ArrayData_Close()
	{
		if (this.m_CurrentArrayData != null )
			this.m_ArrayDataMap.Add(this.m_CurrentArrayData.GetName(), this.m_CurrentArrayData );

		this.m_CurrentArrayData = null;
		this.m_ReadingPhase = EReadingPhase.NONE;
	}


	private static readonly char[]	EscapeCharToRemove = new char[] { '\n','\b','\t','\r' };

	//////////////////////////////////////////////////////////////////////////
	// LoadFile
	// LOAD A FILE AND ALL INCLUDED FILES
	public bool LoadFile( string sFilePath )
	{
		if (this.IsLoaded( sFilePath ) )
			return true;

		this.IsOK = false;

		
		if ( Utils.String.IsAssetsPath( sFilePath ) )
		{
			Utils.String.ConvertFromAssetPathToResourcePath( ref sFilePath );
		}

		if ( Utils.String.IsAbsolutePath( sFilePath ) )
		{
			Utils.String.ConvertFromAbsolutePathToResourcePath( ref sFilePath );
		}



		TextAsset pTextAsset = Resources.Load( sFilePath ) as TextAsset;
		if ( pTextAsset == null )
		{
			Debug.LogError( "Reader::LoadFile:Error opening file: " + sFilePath );
			Application.Quit();
			return false;
		}

		// Remove escape chars to avoid the presence inside strings
		string[] vLines = pTextAsset.text.Split( '\n' );
		for ( int i = 0; i < vLines.Length; i++ )
		{
			vLines[i] = vLines[i].TrimInside( EscapeCharToRemove );
		}

		// Parse each line
		for ( int iLine = 1; iLine < vLines.Length + 1; iLine++ )
		{
			string sLine = vLines[ iLine - 1 ];
			if ( Utils.String.IsValid( ref sLine ) == false )
				continue;

			// INCLUSION
			/// Able to include file in same dir of root file
			if ( ( sLine[ 0 ] == '#' ) && sLine.Contains( "#include" ) )
			{
				if  (this.m_ReadingPhase != EReadingPhase.NONE )
				{
					Debug.LogError( " SectionMap::LoadFile:trying to load another file while " + this.m_ReadingPhase + " at line |" + iLine + "| in file |" + sFilePath + "| " );
					return false;
				}

				string sPath = System.IO.Path.GetDirectoryName( sFilePath );
				string sFileName = sLine.Trim().Substring( "#include".Length + 1 ).TrimInside();
				string sSubPath = System.IO.Path.GetDirectoryName( sFileName );
				string combinedPath = Path.Combine( sPath, Path.Combine( sSubPath, System.IO.Path.GetFileNameWithoutExtension( sFileName ) ) );
				if (this.LoadFile( combinedPath ) == false )
					return false;

				continue;
			}

			// NEW SECTION CREATION
			if ( sLine[ 0 ] == '[' )
			{	
				if ( sLine.IndexOf( ']' ) == -1 )
				{
					Debug.LogError( " SectionMap::LoadFile:Invalid Section definition at line |" + iLine + "| in file |" + sFilePath + "| " );
					return false;
				}

				// Create a new section
				if (this.Section_Create( sLine.TrimInside(), sFilePath, iLine ) == false )
				{
					return false;
				}
				continue;
			}

			// NEW ARRAY DATA LIST
			if ( sLine[ 0 ] == '\'' )
			{
				if ( sLine.IndexOf( '\'', 1 ) == -1 )
				{
					Debug.LogError( " SectionMap::LoadFile:Invalid ArrayData definition at line |" + iLine + "| in file |" + sFilePath + "| " );
					return false;
				}

				// Create a new arrayData
				if (this.ArrayData_Create( sLine.TrimInside(), sFilePath, iLine ) == false )
				{
					return false;
				}
				continue;
			}

			// READING SECTION
			if (this.m_ReadingPhase == EReadingPhase.READING_SECTION )
			{
				// KEY = VALUE
				KeyValue pKeyValue = Utils.String.GetKeyValue( sLine );
				if ( pKeyValue.IsOK )
				{
					if (this.m_CurrentSection == null )
					{
						Debug.LogError( " SectionMap::LoadFile:No section created to insert KeyValue at line |" + iLine + "| in file |" + sFilePath + "| " );
						return false;
					}

					// SECTION LIST
					if ( pKeyValue.Value == "{" )
					{
						this.m_ReadingPhase = EReadingPhase.READING_SECTION_LIST;
						this.m_CurrentLineValue = new cLineValue( pKeyValue.Key, ELineValueType.MULTI );
						this.m_CurrentMultiValue = new cMultiValue( null );
						continue;
					}

					if (this.Section_Add( pKeyValue, sFilePath, iLine ) == false )
					{
						return false;
					}
					continue;
				}
			}

			// READING SECTION LIST
			if (this.m_ReadingPhase == EReadingPhase.READING_SECTION_LIST )
			{
				string trimmedLine = sLine.Trim( new char[] { ',', '.', ' ' } );
				if ( trimmedLine == "}" )
				{
					this.m_CurrentLineValue.Set(this.m_CurrentMultiValue );
					this.m_CurrentSection.Add(this.m_CurrentLineValue );
					this.m_ReadingPhase = EReadingPhase.READING_SECTION;
					continue;
				}

				this.m_CurrentMultiValue.Add( trimmedLine );
				continue;
			}

			// READING ARRAY DATA LIST
			if (this.m_ReadingPhase == EReadingPhase.READING_ARRAYDATA )
			{
				if (this.ArrayData_Add( sLine, sFilePath, iLine ) == false )
				{
					return false;
				}
				continue;
			}

			// NO CORRECT LINE DETECTED
			Debug.LogError( "SectionMap::LoadFile:Incorrect line " + iLine + " in file " + sFilePath );
			return false;
		}

		this.Section_Close();
		this.ArrayData_Close();
		this.m_FilePaths.Add( sFilePath );
		this.IsOK = true;
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// IsLoaded
	// CHECK IF A FILE IS ALREADY LOADED BY PATH
	public bool IsLoaded( string sFilePath )
	{
		if (this.m_FilePaths.Count < 1 )
			return false;

		return this.m_FilePaths.Find( (s) => s == sFilePath ) != null;
	}


	//////////////////////////////////////////////////////////////////////////
	// HasFileElement
	// CHECK AND RETURN IN CASE IF SECTION ALREADY EXISTS
	public bool HasFileElement( string identifier, ref Section result )
	{
		return this.m_SectionMap.TryGetValue( identifier, out result );
	}


	//////////////////////////////////////////////////////////////////////////
	// HasFileElement
	// CHECK AND RETURN IN CASE IF SECTION ALREADY EXISTS
	public bool HasFileElement( string identifier, ref ArrayData result )
	{
		return this.m_ArrayDataMap.TryGetValue( identifier, out result );
	}


	//////////////////////////////////////////////////////////////////////////
	// NewSection
	// CREATE AND SAVE A NEW EMPTY SECTION
	public Section NewSection ( string SecName, string Context )
	{
		Section pSec = null;
		if (this.HasFileElement( SecName, ref pSec  ) )
		{
			pSec.Destroy();
		}

		pSec = new Section( SecName, context: Context );
		this.m_SectionMap[SecName] = pSec; // Adding in this way will overwrite section, if already exists
		return pSec;
	}


	//////////////////////////////////////////////////////////////////////////
	// bGetSection
	/// <summary> Retrieve a section, return true if section exists otherwise false </summary>
	public bool GetSection( string SectionName, ref Section section )
	{
		return this.m_SectionMap.TryGetValue( SectionName, out section );
	}


	//////////////////////////////////////////////////////////////////////////
	// bGetSection
	public	bool	GetSection<T>( string identifier, T outer ) where T : class
	{
		if ( outer == null )
			return false;

		Section section = null;
		bool bHadGoodResult = this.GetSection( identifier, ref section );
		if ( bHadGoodResult == false )
		{
			return false;
		}
		
		System.Type classType = typeof(T);
		FieldInfo[] fieldInfos = classType.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );

		string[] sectionKeys = section.GetKeys();
		foreach ( FieldInfo fieldInfo in fieldInfos )
		{
			string fieldName = fieldInfo.Name;

			// Remove module prefix if exists
			if ( fieldName.StartsWith( "m_" ) ) fieldName = fieldName.Substring( 2 );

			bool bIsEnum = fieldInfo.FieldType.IsEnum;
			if ( bIsEnum )
			{
				fieldName = fieldName.Insert( 0, "e" );
			}

			// Check if outer field is found in section
			int index = System.Array.FindIndex( sectionKeys, key => fieldName == key );
			if ( index == -1 )
			{
				return false;
			}

			cLineValue lineValue = section[index];
			if ( lineValue.Type == ELineValueType.SINGLE )
			{
				object valueToAssign = null;
				if ( bIsEnum )
				{
					Utils.Converters.StringToEnum( lineValue.Value.ToString(), fieldInfo.FieldType, ref valueToAssign );
				}
				else
				{
					valueToAssign = lineValue.Value.ToSystemObject();
				}

				fieldInfo.SetValue( outer, System.Convert.ChangeType( valueToAssign, fieldInfo.FieldType ) );
//				Debug.Log( "Set of " + fieldInfo.Name + " of " + classType.Name + " To: " + lineValue.Value.ToString() );
			}
			
			System.Type elementType = null;
			if ( lineValue.Type == ELineValueType.MULTI && lineValue.MultiValue.DeductType( ref elementType ) )
			{
				if ( elementType == typeof( Vector2 ) )
				{
					fieldInfo.SetValue( outer, section.AsVec2( fieldName, Vector2.zero ) );
					continue;
				}

				if ( elementType == typeof( Vector3 ) )
				{
					fieldInfo.SetValue( outer, section.AsVec3( fieldName, Vector3.zero ) );
					continue;
				}

				if ( elementType == typeof( Vector4 ) )
				{
					if ( fieldInfo.FieldType == typeof( Vector4 ) )
					{
						fieldInfo.SetValue( outer, section.AsVec4( fieldName, Vector4.zero ) );
					}

					if ( fieldInfo.FieldType == typeof( Color ) )
					{
						fieldInfo.SetValue( outer, section.AsColor( fieldName, Color.clear ) );
					}
					continue;
				}							

				if ( fieldInfo.FieldType.IsArray == true )
				{
					object[] result = lineValue.MultiValue.ValueList.ConvertAll	// Get a list of converted cvalues to requested type
					(
						new System.Converter<cValue, object> ( ( cValue v ) => { return v.ToSystemObject(); } )
					)
					.ToArray(); 
					fieldInfo.SetValue( outer, result /*lineValue.MultiValue.ValueList.ToArray()*/ );
					continue;
				}

				Debug.Log( "Set of " + fieldInfo.Name + " of " + classType.Name + " is impossible!! " );
			}
		}
	
		
		return bHadGoodResult;
	}


	//////////////////////////////////////////////////////////////////////////
	// bGetArrayData
	/// <summary> Retrieve an array data, return true if section exists otherwise false </summary>
	public bool bGetArrayData( string identifier, ref ArrayData result )
	{
		return this.m_ArrayDataMap.TryGetValue( identifier, out result );
	}



	//////////////////////////////////////////////////////////////////////////
	// GetSectionsByContext
	/// <summary> Return an array of sections that shares a context in this instance of sectionMap </summary>
	public	Section[]	GetSectionsByContext( string context )
	{
		List<Section> results = new List<Section>();
		{
			foreach( Section sec in this.m_SectionMap.Values )
			{
				if ( sec.Context == context ) results.Add( sec );
			}
		}
		return results.ToArray();
	}

	/*
	//////////////////////////////////////////////////////////////////////////
	// PrintMap
	// PRINT IN A READABLE FORMAT THE MAP SECTION
	public	void	PrintMap()
	{
		foreach( Section section in m_SectionMap )
		{
			Debug.Log( "Section: " + section.GetName() );

			foreach( cLineValue lineValue in section )
			{
				string skey = lineValue.Key;
				string sValue = "";

				if ( lineValue.Type == LineValueType.SINGLE )
				{
					sValue = lineValue.Value.ToSystemObject().ToString();
					Debug.Log( skey + " = " + sValue );
					continue;
				}
				if ( lineValue.Type == LineValueType.MULTI )
				{
					foreach( cValue value in lineValue.MultiValue.ValueList )
					{
						sValue += value.ToSystemObject().ToString() + ", ";
					}
					Debug.Log( skey + " = " + sValue );
					continue;
				}
			}
			Debug.Log( "|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||" );
		}
	}
	*/
	/*
	public	void	SaveToBuffer( ref string buffer )
	{
		string internalBuffer = "";
		foreach( Section section in this )
		{
			section.SaveToBuffer( ref internalBuffer );
			internalBuffer += "\n";
		}

		System.IO.File.WriteAllText( "AllSections.ini", internalBuffer );
	}
	*/

	public	void	SaveContextSections( string Context )
	{
		Section[] sections = this.GetSectionsByContext( Context );

		if ( sections.Length == 0 )
			return;

		string internalBuffer = "";
		foreach( Section section in sections )
		{
			section.SaveToBuffer( ref internalBuffer );
			internalBuffer += "\n";
		}

		System.IO.File.WriteAllText( Context + ".ini", internalBuffer );
	}
}