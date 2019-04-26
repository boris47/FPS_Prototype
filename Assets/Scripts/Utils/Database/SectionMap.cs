
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Database;
using System;
using System.IO;


public class SectionMap : IEnumerable/*Foreach feature*/ {

	// READING PHASES
	private	static int							READING_NOTHING		= 0;
	private	static int							READING_SECTION		= 1;
///	private static int							READING_LIST		= 2;	// TODO Implement this

	// CONTAINERS
	private	Dictionary < string, Section >		m_SectionMap		= new Dictionary < string, Section > ();
	private	List < string >						m_FilePaths			= new List < string >();

	// INTERNAL VARS
	private	int									m_ReadingPhase		= READING_NOTHING;
	private	Section								m_CurrentSection	= null;

	public	bool								IsOK
	{
		get; private set;
	}


	IEnumerator IEnumerable.GetEnumerator()
	{
		return (IEnumerator) GetEnumerator();
	}
	
	public Dictionary<string, Section>.ValueCollection.Enumerator  GetEnumerator()
    {
        return m_SectionMap.Values.GetEnumerator();
    }


	//////////////////////////////////////////////////////////////////////////
	// Section_Create
	// CREATE A NEW SECTION WHILE READING FILE
	private bool Section_Create( string sLine, string sFilePath, int iLine )
	{
		if ( m_CurrentSection != null )
		{
			Section_Close();
		}

		Section bump = null;
		int squareBracketIndex = sLine.IndexOf( "]", 0 );
		string sSectionName = sLine.Substring( 1, squareBracketIndex - 1 );
		if ( HasFileElement( sSectionName, ref bump ) )
		{
			Debug.LogError( "SectionMap::Section_Create:" + sFilePath + ":[" + iLine + "]: Section \"" + sSectionName + "\" already exists!" );
			return false;
		}

		m_ReadingPhase = READING_SECTION;

		string context = System.IO.Path.GetFileNameWithoutExtension( sFilePath );
		m_CurrentSection = new Section( sSectionName, context: context );

		// Get the name of mother section, if present
		int iIndex = sLine.IndexOf( ":", squareBracketIndex );
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
				if ( bGetSection( motherName, ref motherSection ) )
				{
					m_CurrentSection += motherSection;
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
			Debug.LogError( " SectionMap::Section_Add:LineValue invalid for key |" + Pair.Key + "| in Section |" + m_CurrentSection.Name() + "| in file |" + sFilePath + "|" );
			return false;
		}
		m_CurrentSection.Add( pLineValue );
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// Section_Close
	// DEFINITELY SAVE THE PARSING SECTION
	private void Section_Close()
	{
		if ( m_CurrentSection != null )
			m_SectionMap.Add( m_CurrentSection.Name(), m_CurrentSection );

		m_CurrentSection = null;
		m_ReadingPhase = READING_NOTHING;
	}

	private static	char[]	EscapeCharToRemove = new char[] { '\n','\b','\t','\r' };

	//////////////////////////////////////////////////////////////////////////
	// LoadFile
	// LOAD A FILE AND ALL INCLUDED FILES
	public bool LoadFile( string sFilePath )
	{
		if ( IsLoaded( sFilePath ) )
			return true;

		IsOK = false;

		// release mode
		TextAsset pTextAsset = Resources.Load( sFilePath ) as TextAsset;
		if ( pTextAsset == null )
		{
			Debug.LogError( "Reader::LoadFile:Error opening file: " + sFilePath );
			Application.Quit();
			return false;
		}

		// Remove escape chars ti avoid the presence inside strings
		string[] vLines = pTextAsset.text.Split( '\n' );
		for ( int i = 0; i < vLines.Length; i++ )
		{
			vLines[i] = vLines[i].TrimInside( EscapeCharToRemove );
		}

		// Parse each line
		for ( int iLine = 1; iLine < vLines.Length+1; iLine++ )
		{
			string sLine = vLines[ iLine-1 ];

			if ( Utils.String.IsValid( ref sLine ) == false )
				continue;

			// INCLUSION
			/// Able to include file in same dir of root file
			if ( ( sLine[ 0 ] == '#' ) && sLine.Contains( "#include" ) )
			{
				string sPath = System.IO.Path.GetDirectoryName( sFilePath );
				string sFileName = sLine.Trim().Substring( "#include".Length + 1 ).TrimInside();
				string sSubPath = System.IO.Path.GetDirectoryName( sFileName );
				string combinedPath = Path.Combine( sPath, Path.Combine( sSubPath, System.IO.Path.GetFileNameWithoutExtension( sFileName ) ) );
				if ( LoadFile( combinedPath ) == false )
					return false;

				continue;
			}

			// SECTION CREATION
			if ( ( sLine[ 0 ] == '[' ) )
			{	
				if ( sLine.IndexOf( ']' ) == -1 )
				{
					Debug.LogError( " SectionMap::LoadFile:Invalid Section definition at line |" + iLine + "| in file |" + sFilePath + "| " );
					return false;
				}

				// Create a new section
				if ( Section_Create( sLine.TrimInside(), sFilePath, iLine ) == false )
				{
					return false;
				}
				continue;
			}

			// INSERTION
			KeyValue pKeyValue = Utils.String.GetKeyValue( sLine );
			if ( pKeyValue.IsOK )
			{
				if ( m_CurrentSection == null )
				{
					Debug.LogError( " SectionMap::LoadFile:No section created to insert KeyValue at line |" + iLine + "| in file |" + sFilePath + "| " );
					return false;
				}

				if ( m_ReadingPhase != READING_SECTION )
				{
					Debug.LogError( " SectionMap::LoadFile:Trying to insert a KeyValue into a non section type FileElement, line \"" + sLine + "\" of file " + sFilePath + "!" );
					continue;
				}

				if ( Section_Add( pKeyValue, sFilePath, iLine ) == false )
				{
					return false;
				}
				continue;
			}

			// NO CORRECT LINE DETECTED
			Debug.LogError( "SectionMap::LoadFile:Incorrect line " + iLine + " in file " + sFilePath );
			return false;
		}

		Section_Close();
		m_FilePaths.Add( sFilePath );
		IsOK = true;
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// IsLoaded
	// CHECK IF A FILE IS ALREADY LOADED BY PATH
	public bool IsLoaded( string sFilePath )
	{
		if ( m_FilePaths.Count < 1 )
			return false;

		return m_FilePaths.Find( (s) => s == sFilePath ) != null;
	}


	//////////////////////////////////////////////////////////////////////////
	// HasFileElement
	// CHECK AND RETURN IN CASE IF SECTION ALREADY EXISTS
	public bool HasFileElement( string SecName, ref Section pSec )
	{
		return m_SectionMap.TryGetValue( SecName, out pSec );
	}


	//////////////////////////////////////////////////////////////////////////
	// NewSection
	// CREATE AND SAVE A NEW EMPTY SECTION
	public Section NewSection ( string SecName, string Context )
	{
		Section pSec = null;
		if ( HasFileElement( SecName, ref pSec  ) )
		{
			pSec.Destroy();
		}

		pSec = new Section( SecName, context: Context );
		m_SectionMap[SecName] = pSec; // Adding in this way will overwrite section, if already exists
		return pSec;
	}


	//////////////////////////////////////////////////////////////////////////
	// GetSection
	/// <summary> Retrieve a section, return true if section exists otherwise false </summary>
	public bool bGetSection( string SectionName, ref Section section )
	{
		return m_SectionMap.TryGetValue( SectionName, out section );
	}


	//////////////////////////////////////////////////////////////////////////
	// GetSectionsByContext
	/// <summary> Return an array of sections that shares a context in this instance of sectionMap </summary>
	public	Section[]	GetSectionsByContext( string context )
	{
		List<Section> results = new List<Section>();
		{
			foreach( Section sec in m_SectionMap.Values )
			{
				if ( sec.Context == context ) results.Add( sec );
			}
		}
		return results.ToArray();
	}


	//////////////////////////////////////////////////////////////////////////
	// PrintMap
	// PRINT IN A READABLE FORMAT THE MAP SECTION
	public	void	PrintMap()
	{
		foreach( Section section in this )
		{
			Debug.Log( "Section: " + section.Name() );

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
					foreach( cValue value in lineValue.MultiValue.ValueArray )
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


	public	void	SaveContextSections( string Context )
	{
		Section[] sections = GetSectionsByContext( Context );

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