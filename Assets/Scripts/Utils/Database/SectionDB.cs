
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using Database;


namespace asdqwe
{

	internal class qwe
	{

	}

	public class asd
	{

	}

}


public abstract class SectionDB
{
	public class LocalDB
	{
		private Dictionary<string, Section>		m_SectionMap		= new Dictionary<string, Section>();
	//	private Dictionary<string, ArrayData>	m_ArrayDataMap		= new Dictionary<string, ArrayData>();
		private List<string>					m_FilePaths			= new List<string>();
		public	static bool						IsOK
		{
			get; private set;
		}

		public LocalDB()
		{
			System.Diagnostics.Debugger.Break();
		}

		public LocalDB(in string filePath)
		{
			TryLoadFile(filePath);
		}

		public bool TryLoadFile(in string filePath) => IsOK = new Parser(ref m_SectionMap, /*ref m_ArrayDataMap, */ref m_FilePaths).LoadFile(filePath);

		public bool IsLoaded(in string filePath) => Implementation.IsFileLoaded_Impl(m_FilePaths, filePath);

		public bool ContainsSection(in string sectionName) => Implementation.ContainsSection_Impl(m_SectionMap, sectionName);

	//	public bool ContainsArrayData(in string arrayDataName) => Implementation.ContainsArrayData_Impl(m_ArrayDataMap, arrayDataName);

		public Section NewSection(in string sectionName, in string context) => Implementation.NewSection_Impl(m_SectionMap, sectionName, context);

		public bool TryGetSection(in string sectionName, out Section section) => Implementation.TryGetSection_Impl(m_SectionMap, sectionName, out section);

		public bool TrySectionToOuter<T>(in Section section, T outer) where T : class => Implementation.TrySectionToOuter_Impl(section, outer);

	//	public bool TryGetArrayData(in string identifier, out ArrayData result) => Implementation.TryGetArrayData_Impl(m_ArrayDataMap, identifier, out result);

		public Section[] GetSectionsByContext(in string context) => Implementation.GetSectionsByContext_Impl(m_SectionMap, context);

		public void SaveContextSections(in string context) => System.IO.File.WriteAllText($"{context}.ini", Implementation.SaveContextSections_Impl(m_SectionMap, context));
	}

	public static class GlobalDB
	{
		// CONTAINERS
		private static Dictionary<string, Section>		m_SectionMap		= new Dictionary<string, Section>();
	//	private static Dictionary<string, ArrayData>	m_ArrayDataMap		= new Dictionary<string, ArrayData>();
		private static List<string>						m_FilePaths			= new List<string>();

		public static bool TryLoadFile(in string filePath)
		{
			Dictionary<string, Section>		local_DB_SectionMap			= new Dictionary<string, Section>();
	//		Dictionary<string, ArrayData>	local_DB_ArrayDataMap		= new Dictionary<string, ArrayData>();
			List<string>					local_DB_FilePaths			= new List<string>();

			// Avoid re-load files
			local_DB_FilePaths.AddRange(m_FilePaths);

			if (new Parser(ref local_DB_SectionMap, /*ref local_DB_ArrayDataMap, */ref local_DB_FilePaths).LoadFile(filePath))
			{
				m_SectionMap.AddRange(local_DB_SectionMap, bOverwrite: true);
	//			m_ArrayDataMap.AddRange(local_DB_ArrayDataMap, bOverwrite: true);
				// Just add not already added filePath
				local_DB_FilePaths.ForEach(fp => m_FilePaths.AddUnique(fp));
				return true;
			}
			return false;
		}

		public static bool IsLoaded(in string filePath) => Implementation.IsFileLoaded_Impl(m_FilePaths, filePath);

		public static bool ContainsSection(in string sectionName) => Implementation.ContainsSection_Impl(m_SectionMap, sectionName);

	///	public static bool ContainsArrayData(in string arrayDataName) => Implementation.ContainsArrayData_Impl(m_ArrayDataMap, arrayDataName);

		public static Section NewSection(in string sectionName, in string context) => Implementation.NewSection_Impl(m_SectionMap, sectionName, context);

		public static bool TryGetSection(in string sectionName, out Section section) => Implementation.TryGetSection_Impl(m_SectionMap, sectionName, out section);

		public static bool TrySectionToOuter<T>(in Section section, T outer) where T : class => Implementation.TrySectionToOuter_Impl(section, outer);

	//	public static bool TryGetArrayData(in string identifier, out ArrayData result) => Implementation.TryGetArrayData_Impl(m_ArrayDataMap, identifier, out result);

		public static Section[] GetSectionsByContext(in string context) => Implementation.GetSectionsByContext_Impl(m_SectionMap, context);

		public static void SaveContextSections(in string context) => System.IO.File.WriteAllText($"{context}.ini", Implementation.SaveContextSections_Impl(m_SectionMap, context));
	}

	private static class Implementation
	{
		public static Section NewSection_Impl(in Dictionary<string, Section> sectionMap, in string sectionName, in string context)
		{
			if (TryGetSection_Impl(sectionMap, sectionName, out Section foundSection))
			{
				foundSection.Destroy();
			}

			// Adding in this way will overwrite section, if already exists
			return sectionMap[sectionName] = new Section(sectionName: sectionName, context: context);
		}

		public static bool IsFileLoaded_Impl(in List<string> filePaths, in string sFilePath) => filePaths.IndexOf(sFilePath) > -1;

		public static bool ContainsSection_Impl(in Dictionary<string, Section> sectionMap, in string sectionName) => sectionMap.ContainsKey(sectionName);

	//	public static bool ContainsArrayData_Impl(in Dictionary<string, ArrayData> arrayDataMap, in string ArrayDataName) => arrayDataMap.ContainsKey(ArrayDataName);

		public static bool TryGetSection_Impl(in Dictionary<string, Section> sectionMap, in string SectionName, out Section section) => sectionMap.TryGetValue(SectionName, out section);

	//	public static bool TryGetArrayData_Impl(in Dictionary<string, ArrayData> arrayDataMap, in string identifier, out ArrayData result) => arrayDataMap.TryGetValue(identifier, out result);

		public static bool TrySectionToOuter_Impl<T>(in Section section, T outer) where T : class
		{
			if ( outer == null )
				return false;
			
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
				if (!section.HasKey(fieldName))
				{
					return false;
				}

				LineValue lineValue = section.GetLineValue(fieldName);
				if ( lineValue.Type == ELineValueType.SINGLE )
				{
					object valueToAssign = null;
					if ( bIsEnum )
					{
						Utils.Converters.StringToEnum( lineValue.Value.ToString(), fieldInfo.FieldType, out valueToAssign );
					}
					else
					{
						valueToAssign = lineValue.Value.ToSystemObject();
					}

					fieldInfo.SetValue( outer, System.Convert.ChangeType( valueToAssign, fieldInfo.FieldType ) );
	//				Debug.Log( "Set of " + fieldInfo.Name + " of " + classType.Name + " To: " + lineValue.Value.ToString() );
				}
			
				if ( lineValue.Type == ELineValueType.MULTI && lineValue.MultiValue.DeductType( out System.Type elementType) )
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
						object[] result = System.Array.ConvertAll(lineValue.MultiValue.ValueList, v => v.ToSystemObject());
						fieldInfo.SetValue( outer, result );
						continue;
					}

					Debug.Log( $"Set of {fieldInfo.Name} of {classType.Name} is impossible!! ");
				}
			}
	
			return true;
		}

		public static Section[] GetSectionsByContext_Impl(in Dictionary<string, Section> sectionMap, in string context)
		{
			string localContext = context;
			return sectionMap.Values.Where(section => section.Context == localContext).ToArray();
		}

		public static string SaveContextSections_Impl(in Dictionary<string, Section> sectionMap, in string context)
		{
			string buffer = "";
			Section[] sections = GetSectionsByContext_Impl(sectionMap, context);
			foreach(Section section in sections)
			{
				section.SaveToBuffer(ref buffer);
				buffer += "\n";
			}
			return buffer;
		}
	}

	private class Parser
	{
		private static readonly char[] EscapeCharToRemove   = new char[] { '\n','\b','\t','\r' };
		private static readonly char[] SectionListTrimChars = new char[] { ',', '.', ' ' };

		// READING PHASES
		private enum EReadingPhase
		{
			NONE					= 0,
			READING_SECTION			= 1,
			READING_SECTION_LIST	= 2,
	//		READING_ARRAYDATA		= 3
		}

		// CONTAINERS
		private static Dictionary<string, Section>			m_SectionMap		= new Dictionary<string, Section>();
	//	private static Dictionary<string, ArrayData>		m_ArrayDataMap		= new Dictionary<string, ArrayData>();
		private static List<string>							m_FilePaths			= new List<string>();

		// INTERNAL VARS
		private static	EReadingPhase						m_ReadingPhase		= EReadingPhase.NONE;
		private static	Section								m_CurrentSection	= null;
	//	private static	ArrayData							m_CurrentArrayData	= null;
		private static	LineValue							m_CurrentLineValue	= null;
		private static	MultiValue							m_CurrentMultiValue	= null;


		public Parser(ref Dictionary<string, Section> sectionMap, /*ref Dictionary<string, ArrayData> arrayDataMap,*/ ref List<string> filePaths)
		{
			m_SectionMap	= sectionMap;
	//		m_ArrayDataMap	= arrayDataMap;
			m_FilePaths		= filePaths;
		}


		//////////////////////////////////////////////////////////////////////////
		// LoadFile
		// LOAD A FILE AND ALL INCLUDED FILES
		public bool LoadFile( in string InFilePath )
		{
			if (Implementation.IsFileLoaded_Impl(m_FilePaths, InFilePath))
				return true;

			string filePath = InFilePath;

			if (Utils.String.IsAssetsPath(filePath))
			{
				Utils.String.ConvertFromAssetPathToResourcePath(ref filePath);
			}
			else
			if (Utils.String.IsAbsolutePath(filePath))
			{
				Utils.String.ConvertFromAbsolutePathToResourcePath(ref filePath);
			}

			TextAsset textAsset = Resources.Load(filePath) as TextAsset;
			if (textAsset == null)
			{
				Debug.LogError($"Reader::LoadFile:Error opening file: {filePath}");
				Application.Quit();
				return false;
			}

			// Remove escape chars to avoid the presence inside strings
			string[] lines = textAsset.text.Split('\n');
		//	for (int i = 0; i < lines.Length; i++)
		//	{
		//		lines[i] = lines[i].TrimInside(EscapeCharToRemove);
		//	}

			// Parse each line
			for (int iLine = 1; iLine < lines.Length + 1; iLine++)
			{
				string line = lines[iLine - 1].TrimInside(EscapeCharToRemove);

				Utils.String.CleanComments(ref line);
				if (Utils.String.IsValid(line) == false)
					continue;

				// INCLUSION
				/// Able to include file in same dir of root file
				if ((line[0] == '#') && line.Contains("#include"))
				{
					if (m_ReadingPhase != EReadingPhase.NONE)
					{
						Debug.LogErrorFormat(" SectionMap::LoadFile:trying to load another file while {0} at line |{1}| in file |{2}| ", m_ReadingPhase, iLine, filePath);
						return false;
					}

					string sPath = System.IO.Path.GetDirectoryName(filePath);
					string sFileName = line.Trim().Substring("#include".Length + 1).TrimInside();
					string sSubPath = System.IO.Path.GetDirectoryName(sFileName);
					string combinedPath = System.IO.Path.Combine(sPath, System.IO.Path.Combine(sSubPath, System.IO.Path.GetFileNameWithoutExtension(sFileName)));
					if (LoadFile(combinedPath) == false)
						return false;

					continue;
				}

				// NEW SECTION CREATION
				if (line[0] == '[')
				{
					if (line.IndexOf(']') == -1)
					{
						Debug.LogErrorFormat(" SectionMap::LoadFile:Invalid Section definition at line |{0}| in file |{1}| ", iLine, filePath);
						return false;
					}

					// Create a new section
					if (Section_Create(line.TrimInside(), filePath, iLine) == false)
					{
						return false;
					}
					continue;
				}

				// NEW ARRAY DATA LIST
				if (line[0] == '\'')
				{
					if (line.IndexOf('\'', 1) == -1)
					{
						Debug.LogErrorFormat(" SectionMap::LoadFile:Invalid ArrayData definition at line |{0}| in file |{1}| ", iLine, filePath);
						return false;
					}

					// Create a new arrayData
				//	if (ArrayData_Create(line.TrimInside(), filePath, iLine) == false)
				//	{
				//		return false;
				//	}
					continue;
				}

				// READING SECTION
				if (m_ReadingPhase == EReadingPhase.READING_SECTION)
				{
					// KEY = VALUE
					KeyValue pKeyValue = Utils.String.GetKeyValue(line);
					if (pKeyValue.IsOK)
					{
						if (m_CurrentSection == null)
						{
							Debug.LogErrorFormat(" SectionMap::LoadFile:No section created to insert KeyValue at line |{0}| in file |{1}| ", iLine, filePath);
							return false;
						}

						// SECTION LIST
						if (pKeyValue.Value == "{")
						{
							m_ReadingPhase = EReadingPhase.READING_SECTION_LIST;
							m_CurrentLineValue = new LineValue(pKeyValue.Key, ELineValueType.MULTI);
							m_CurrentMultiValue = new MultiValue(null);
							continue;
						}

						if (Section_Add(pKeyValue, filePath, iLine) == false)
						{
							return false;
						}
						continue;
					}
				}

				// READING SECTION LIST
				if (m_ReadingPhase == EReadingPhase.READING_SECTION_LIST)
				{
					string trimmedLine = line.Trim(SectionListTrimChars);
					if (trimmedLine == "}")
					{
						m_CurrentLineValue.Set(m_CurrentMultiValue);
						m_CurrentSection.Add(m_CurrentLineValue);
						m_ReadingPhase = EReadingPhase.READING_SECTION;
						continue;
					}

					m_CurrentMultiValue.Add(trimmedLine);
					continue;
				}

				// READING ARRAY DATA LIST
	//			if (m_ReadingPhase == EReadingPhase.READING_ARRAYDATA)
	//			{
	//				if (ArrayData_Add(line, filePath, iLine) == false)
	//				{
	//					return false;
	//				}
	//				continue;
	//			}

				// NO CORRECT LINE DETECTED
	//			Debug.LogError($"SectionMap::LoadFile:Incorrect line {iLine} in file {filePath}");
				return false;
			}

			Section_Close();
	//		ArrayData_Close();
			m_FilePaths.Add(filePath);
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// CREATE A NEW SECTION WHILE READING FILE
		private static bool Section_Create( in string sLine, in string sFilePath, in int iLine )
		{
			if (m_CurrentSection != null)
			{
				Section_Close();
			}

			int sectionNameCloseChar = sLine.IndexOf("]");
			string sSectionName = sLine.Substring(1, sectionNameCloseChar - 1);

			if (Implementation.ContainsSection_Impl(m_SectionMap,sSectionName))
			{
				Debug.LogErrorFormat("SectionMap::Section_Create:{0}:[{1}]: Section \"{2}\" already exists!", sFilePath, iLine, sSectionName);
				return false;
			}

			m_ReadingPhase = EReadingPhase.READING_SECTION;

			string context = System.IO.Path.GetFileNameWithoutExtension(sFilePath);
			m_CurrentSection = new Section(sSectionName, context);

			// Get the name of mother section, if present
			int iIndex = sLine.IndexOf(":", sectionNameCloseChar);
			if (iIndex > 0)
			{
				string[] mothers = sLine.Substring(iIndex + 1).Split(',');
				if (mothers.Length == 0)
				{
					Debug.LogError($"SectionMap::Section_Create:{sFilePath}:[{iLine}]: Section Mothers bad definition!");
					return false;
				}

				foreach (string motherName in mothers)
				{
					if (Implementation.TryGetSection_Impl(m_SectionMap, motherName, out Section motherSection))
					{
						m_CurrentSection += motherSection;
					}
					else
					{
						Debug.LogErrorFormat("SectionMap::Section_Create:{0}:[{1}]: Section requested for inheritance \"{2}\" doesn't exist!", sFilePath, iLine, motherName);
					}
				}
			}

			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// INSERT A KEY VALUE PAIR INSIDE THE ACTUAL PARSING SECTION
		private static bool Section_Add( in KeyValue Pair, in string sFilePath, in int iLine )
		{
			LineValue pLineValue = new LineValue( Pair.Key , Pair.Value );
			if ( pLineValue.IsOK == false )
			{
				Debug.LogErrorFormat( " SectionMap::Section_Add:LineValue invalid for key |{0}| in Section |{1}| in file |{2}|", Pair.Key, m_CurrentSection.GetSectionName(), sFilePath);
				return false;
			}
			m_CurrentSection.Add( pLineValue );
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// DEFINITELY SAVE THE PARSING SECTION
		private static void Section_Close()
		{
			if (m_CurrentSection != null )
				m_SectionMap.Add(m_CurrentSection.GetSectionName(), m_CurrentSection );

			m_CurrentSection = null;
			m_ReadingPhase = EReadingPhase.NONE;
		}

		/*
		//////////////////////////////////////////////////////////////////////////
		// CREATE A NEW ARRAY DATA LIST WHILE READING FILE
		private static bool ArrayData_Create( in string sLine, in string sFilePath, in int iLine )
		{
			if (m_CurrentArrayData != null )
			{
				ArrayData_Close();
			}

			int ArrayDataNameCloseChar = sLine.IndexOf( "\'", 1 );
			string sArrayDataName = sLine.Substring( 1, ArrayDataNameCloseChar - 1 );

			if (Implementation.ContainsArrayData_Impl(m_ArrayDataMap, sArrayDataName))
			{
				Debug.LogErrorFormat( "SectionMap::ArrayData_Create:{0}:[{1}]: ArrayData \"{2}\" already exists!", sFilePath, iLine, sArrayDataName );
				return false;
			}
		
			m_ReadingPhase = EReadingPhase.READING_ARRAYDATA;

			string context = System.IO.Path.GetFileNameWithoutExtension( sFilePath );
			m_CurrentArrayData = new ArrayData( sArrayDataName, context: context );

			// Get the name of mother , if present
			int iIndex = sLine.IndexOf( ":", ArrayDataNameCloseChar );
			if ( iIndex > 0 )
			{
				string[] mothers = sLine.Substring( iIndex + 1 ).Split( ',' );
				if ( mothers.Length == 0 )
				{
					Debug.LogErrorFormat( "SectionMap::ArrayData_Create:{0}:[{1}]: ArrayData Mothers bad definition!", sFilePath, iLine );
					return false;
				}

				foreach ( string motherName in mothers )
				{
					if (Implementation.TryGetArrayData_Impl(m_ArrayDataMap, motherName, out ArrayData mother))
					{
						m_CurrentArrayData += mother;
					} else
					{
						Debug.LogErrorFormat( "SectionMap::ArrayData_Create:{0}:[{1}]: ArrayData requested for inheritance \"{2}\" doesn't exist!", sFilePath, iLine, motherName );
					}
				}
			}
			
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// INSERT VALUE INSIDE THE ACTUAL PARSING ARRAY DATA LIST
		private	static bool ArrayData_Add( in string sLine, in string sFilePath, in int iLine )
		{
			LineValue pLineValue = new LineValue( sFilePath + "_"+ iLine , sLine );
			if ( pLineValue.IsOK == false )
			{
				Debug.LogErrorFormat( " SectionMap::ArrayData_Add: LineValue invalid at line |{0}| in Section |{1}| in file |{2}|",	iLine, m_CurrentArrayData.GetArrayDataName(), sFilePath );
				return false;
			}
			m_CurrentArrayData.Add( pLineValue );
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// DEFINITELY SAVE THE PARSING SECTION
		private static void ArrayData_Close()
		{
			if (m_CurrentArrayData != null )
				m_ArrayDataMap.Add(m_CurrentArrayData.GetArrayDataName(), m_CurrentArrayData );

			m_CurrentArrayData = null;
			m_ReadingPhase = EReadingPhase.NONE;
		}
		*/

		
	}
}


