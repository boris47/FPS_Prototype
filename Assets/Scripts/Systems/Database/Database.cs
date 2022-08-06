
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using DatabaseCore;

public static class Database
{
	private const string MAIN_CONFIGS_FILEPATH = "Configs/All";

	public class LocalInstance: System.IDisposable
	{
		private Dictionary<string, Section>		m_SectionMap		= new Dictionary<string, Section>();
		private List<string>					m_FilePaths			= new List<string>();
		
		public LocalInstance()
		{}

		public LocalInstance(in string InFilePath) => Utils.CustomAssertions.IsTrue(TryLoadFile(InFilePath));

		public bool TryLoadFile(in string InFilePath) => new Parser(ref m_SectionMap, ref m_FilePaths).LoadFile(InFilePath);
		public bool IsLoaded(in string InFilePath) => Implementation.IsFileLoaded_Impl(m_FilePaths, InFilePath);
		public bool ContainsSection(in string InSectionName) => Implementation.ContainsSection_Impl(m_SectionMap, InSectionName);
		public Section NewSection(in string InSectionName, in string InContext) => Implementation.NewSection_Impl(m_SectionMap, InSectionName, InContext);
		public bool TryGetSection(in string InSectionName, out Section OutSection) => Implementation.TryGetSection_Impl(m_SectionMap, InSectionName, out OutSection);
		public bool TrySectionToOuter<T>(in Section InSection, T InOuter, bool InSkipUndefinedFields = false) where T : class => Implementation.TrySectionToOuter_Impl(InSection, InOuter, InSkipUndefinedFields);
		public Section[] GetSectionsByContext(in string InContext) => Implementation.GetSectionsByContext_Impl(m_SectionMap, InContext);

		void System.IDisposable.Dispose()
		{
			// Finalize
		}
	}

	public static class Global
	{
		// CONTAINERS
		private static Dictionary<string, Section>		m_SectionMap		= new Dictionary<string, Section>();
		private static List<string>						m_FilePaths			= new List<string>();
		static Global()
		{
			Utils.CustomAssertions.IsTrue(TryLoadFile(MAIN_CONFIGS_FILEPATH));
			Debug.Log($"Database.Global: Loaded {MAIN_CONFIGS_FILEPATH}");
		}

		public static void Reload()
		{
			m_SectionMap.Clear();
			m_FilePaths.Clear();
			Utils.CustomAssertions.IsTrue(TryLoadFile(MAIN_CONFIGS_FILEPATH));
		}

		public static bool TryLoadFile(in string InFilePath)
		{
			Dictionary<string, Section>		localSectionMap			= new Dictionary<string, Section>();
			List<string>					localFilePaths			= new List<string>();

			// Avoid re-load files
			localFilePaths.AddRange(m_FilePaths);

			if (new Parser(ref localSectionMap, ref localFilePaths).LoadFile(InFilePath))
			{
				m_SectionMap.AddRange(localSectionMap, bOverwrite: true);
				// Just add not already added filePath
				localFilePaths.ForEach(fp => m_FilePaths.AddUnique(fp));
				return true;
			}
			return false;
		}

		public static bool IsLoaded(in string InFilePath) => Implementation.IsFileLoaded_Impl(m_FilePaths, InFilePath);

		public static bool ContainsSection(in string InSectionName) => Implementation.ContainsSection_Impl(m_SectionMap, InSectionName);

		public static Section NewSection(in string InSectionName, in string InContext) => Implementation.NewSection_Impl(m_SectionMap, InSectionName, InContext);

		public static bool TryGetSection(in string InSectionName, out Section OutSection) => Implementation.TryGetSection_Impl(m_SectionMap, InSectionName, out OutSection);

		public static bool TrySectionToOuter<T>(in Section InSection, T InOuter, bool InSkipUndefinedFields = false) where T : class => Implementation.TrySectionToOuter_Impl(InSection, InOuter, InSkipUndefinedFields);

		public static Section[] GetSectionsByContext(in string InContext) => Implementation.GetSectionsByContext_Impl(m_SectionMap, InContext);

	//	public static void Save(in string filePath = null) => Implementation.Save_Impl(m_SectionMap, filePath);
	}

	private static class Implementation
	{
		public static Section NewSection_Impl(in Dictionary<string, Section> InSectionMap, in string InSectionName, in string InContext)
		{
			if (TryGetSection_Impl(InSectionMap, InSectionName, out Section foundSection))
			{
				foundSection.Destroy();
			}

			// Adding in this way will overwrite section, if already exists
			return InSectionMap[InSectionName] = new Section(InSectionName: InSectionName, InContext: InContext);
		}

		public static bool IsFileLoaded_Impl(in List<string> InFilePaths, in string InFilePath) => InFilePaths.IndexOf(InFilePath) > -1;

		public static bool ContainsSection_Impl(in Dictionary<string, Section> InSectionMap, in string InSectionName) => InSectionMap.ContainsKey(InSectionName);

		public static bool TryGetSection_Impl(in Dictionary<string, Section> InSectionMap, in string InSectionName, out Section InSection) => InSectionMap.TryGetValue(InSectionName, out InSection);

		public static bool TrySectionToOuter_Impl<T>(in Section InSection, T InOuter, bool InSkipUndefinedFields) where T : class
		{
			Utils.CustomAssertions.IsNotNull(InSection);
			Utils.CustomAssertions.IsNotNull(InOuter);

			System.Type classType = typeof(T);
			FieldInfo[] fieldInfos = classType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (FieldInfo fieldInfo in fieldInfos)
			{
				string fieldName = fieldInfo.Name;

				// Remove module prefix if exists
				if (fieldName.StartsWith("m_")) fieldName = fieldName.Substring(2);

				if (InSection.TryGetLineValue(fieldName, out LineValue lineValue))
				{
					if (lineValue.Type == ELineValueType.SINGLE)
					{
						if (fieldInfo.FieldType.IsEnum)
						{
							if (Utils.Converters.StringToEnum(lineValue.Value.ToString(), fieldInfo.FieldType, out object value))
							{
								fieldInfo.SetValue(InOuter, value);
							}
							else
							{
								Debug.LogError($"Assingation failed for {fieldInfo.Name} of {classType.Name} because impossible to convert to enum {fieldInfo.FieldType.Name}!!");
								return false;
							}
						}
						else
						{
							fieldInfo.SetValue(InOuter, System.Convert.ChangeType(lineValue.Value.StoredValue, fieldInfo.FieldType));
						}
						//Debug.Log($"Set of {fieldInfo.Name} of {classType.Name} To: {lineValue.Value.ToString()}");
					}

					if (lineValue.Type == ELineValueType.MULTI && lineValue.MultiValue.TryDeductType(out System.Type elementType))
					{
						if (elementType == typeof(Vector2))
						{
							fieldInfo.SetValue(InOuter, InSection.AsVec2(fieldName, Vector2.zero));
							continue;
						}

						if (elementType == typeof(Vector3))
						{
							fieldInfo.SetValue(InOuter, InSection.AsVec3(fieldName, Vector3.zero));
							continue;
						}

						if (elementType == typeof(Vector4))
						{
							if (fieldInfo.FieldType == typeof(Vector4))
							{
								fieldInfo.SetValue(InOuter, InSection.AsVec4(fieldName, Vector4.zero));
							}

							if (fieldInfo.FieldType == typeof(Color))
							{
								fieldInfo.SetValue(InOuter, InSection.AsColor(fieldName, Color.clear));
							}
							continue;
						}

						if (fieldInfo.FieldType.IsArray)
						{
							object[] result = System.Array.ConvertAll(lineValue.MultiValue.ValueList, v => v.StoredValue);
							fieldInfo.SetValue(InOuter, result);
							continue;
						}

						Debug.LogError($"Set of {fieldInfo.Name} of {classType.Name} is impossible!!");
					}
				}
				else
				{
					if (!InSkipUndefinedFields)
					{
						Debug.LogError($"Assingation failed for {fieldInfo.Name} of {classType.Name} because field doesn't exist in section!!");
						return false;
					}
				}
			}
			return true;
		}

		public static Section[] GetSectionsByContext_Impl(in Dictionary<string, Section> InSectionMap, in string InContext)
		{
			string localContext = InContext;
			return InSectionMap.Values.Where(section => section.Context == localContext).ToArray();
		}

		/*
		public static void Save_Impl(in Dictionary<string, Section> sectionMap, in string filePath)
		{
			if (string.IsNullOrEmpty(filePath)) // Save each section in associated file
			{
				Dictionary<string, List<Section>> MappedEntitesSections = new Dictionary<string, List<Section>>();
				foreach (Section entitySection in sectionMap.Values)
				{
					string fileName = entitySection.FilePath;

					// Only savable section are selected
					if (!string.IsNullOrEmpty(fileName))
					{
						List<Section> sectionList;
						if (!MappedEntitesSections.TryGetValue(fileName, out sectionList))
						{
							sectionList = new List<Section>();
							MappedEntitesSections.Add(fileName, sectionList);
						}
						sectionList.Add(entitySection);
					}
				}

				foreach(KeyValuePair<string, List<Section>> KeyPair in MappedEntitesSections)
				{
					string FilePath = KeyPair.Key;
					string buffer = string.Join("\n", KeyPair.Value.Select(section => section.Stringify()));
					System.IO.File.WriteAllText(FilePath, buffer);
				}
			}
			else
			{
				System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
				foreach (var section in sectionMap.Values)
				{
					stringBuilder.Append(section.Stringify());
					stringBuilder.Append("\n");
				}
				string buffer = stringBuilder.ToString();
				System.IO.File.WriteAllText(filePath, buffer);
			}
		}
		*/
	}

	private class Parser
	{
//		private static readonly char[] EscapeCharToRemove   = new char[] { '\n','\b','\t','\r' };
//		private static readonly char[] SectionListTrimChars = new char[] { ',', '.', ' ' };

		// READING PHASES
		private enum EReadingPhase
		{
			NONE					= 0,
			READING_SECTION			= 1,
			// TODO Deprecate me
			READING_SECTION_LIST	= 2,
		}

		// CONTAINERS
		private static Dictionary<string, Section>			m_SectionMap		= new Dictionary<string, Section>();
		private static List<string>							m_FilePaths			= new List<string>();

		// INTERNAL VARS
		private static	EReadingPhase						m_ReadingPhase		= EReadingPhase.NONE;
		private static	Section								m_CurrentSection	= null;
		private static	LineValue							m_CurrentLineValue	= null;
		private static	MultiValue							m_CurrentMultiValue	= null;


		public Parser(ref Dictionary<string, Section> sectionMap, ref List<string> filePaths)
		{
			m_SectionMap	= sectionMap;
			m_FilePaths		= filePaths;
		}


		//////////////////////////////////////////////////////////////////////////
		// LoadFile
		// LOAD A FILE AND ALL INCLUDED FILES
		public bool LoadFile(in string InFilePath)
		{
			if (Implementation.IsFileLoaded_Impl(m_FilePaths, InFilePath))
			{
				return true;
			}

			string filePath;
			if (!Utils.String.TryConvertFromAssetPathToResourcePath(InFilePath, out filePath) && !Utils.String.TryConvertFromAbsolutePathToResourcePath(InFilePath, out filePath))
			{
				return false;
			}

			TextAsset textAsset = Resources.Load(filePath) as TextAsset;
			if (!textAsset)
			{
				Debug.LogError($"Reader::LoadFile:Error opening file: {filePath}");
				return false;
			}

			// Split, clean and select valid lines
			string[] lines = textAsset.text.Split('\n').Select(l => DatabaseCore.Utils.CleanComments(l).Trim()).Where(l => l.Any()).ToArray();

			// Parse each line
			for (int iLine = 1, lineCount = lines.Length; iLine < lineCount + 1; ++iLine)
			{
				string line = lines[iLine - 1];

				// INCLUSION
				/// Able to include file in same directory of root file
				if (line.StartsWith("#include"))
				{
					if (m_ReadingPhase != EReadingPhase.NONE)
					{
						Debug.LogError($" SectionMap::LoadFile:trying to load another file while {0} at line |{iLine}| in file |{filePath}| ");
						return false;
					}

					string path = System.IO.Path.GetDirectoryName(filePath);
					string fileName = line.Trim().Substring("#include".Length + 1).Trim();
					string subPath = System.IO.Path.GetDirectoryName(fileName);
					string combinedPath = System.IO.Path.Combine(path, subPath, System.IO.Path.GetFileNameWithoutExtension(fileName));
					if (!LoadFile(combinedPath))
					{
						return false;
					}
					continue;
				}

				// NEW SECTION CREATION
				if (line[0] == '[')
				{
					// Create a new section
					if (!Section_Create(line.TrimInside(), filePath, iLine))
					{
						return false;
					}
					continue;
				}

				// READING SECTION
				if (m_ReadingPhase == EReadingPhase.READING_SECTION)
				{
					// KEY = VALUE
					KeyValue pKeyValue = DatabaseCore.Utils.GetKeyValue(line);
					if (pKeyValue.IsOK)
					{
						if (m_CurrentSection == null)
						{
							Debug.LogError($" SectionMap::LoadFile:No section created to insert KeyValue at line |{iLine}| in file |{filePath}| ");
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

						if (!Section_Add(pKeyValue, filePath, iLine))
						{
							return false;
						}
						continue;
					}
				}

				// READING SECTION LIST
				if (m_ReadingPhase == EReadingPhase.READING_SECTION_LIST)
				{
					if (line[0] == '}')
					{
						m_CurrentLineValue.Set(m_CurrentMultiValue);
						m_CurrentSection.Add(m_CurrentLineValue);
						m_ReadingPhase = EReadingPhase.READING_SECTION;
						continue;
					}

					m_CurrentMultiValue.Add(line);
					continue;
				}

				// NO CORRECT LINE DETECTED
				Debug.LogError($"SectionMap::LoadFile:Incorrect line {iLine} in file {filePath}");
				return false;
			}

			Section_Close();
			m_FilePaths.Add(filePath);
			return true;
		}

		//////////////////////////////////////////////////////////////////////////
		private static bool ExtractSectionInfo(in string InFilePath, in int InLineNumber, in string InLine, out string OutSectionName, out string OutSectionContext, out string[] OutSectionMothers)
		{
			OutSectionName = null;
			OutSectionContext = "None";
			OutSectionMothers = new string[0];

			// Format expected [SECTION_NAME](CONTEXT): MOTHER01, MOTHER02, ...

			int startIndex = InLine.IndexOf('['), endIndex = 0;
			if (startIndex >= 0)
			{
				endIndex = InLine.IndexOf(']', startIndex + 1);
				if (endIndex > 0)
				{
					OutSectionName = InLine.Substring(startIndex + 1, endIndex - 1).Trim();
					startIndex = endIndex + 1;
				}
				else
				{
					Debug.LogError($"SectionMap::Section_Create:{InFilePath}:[{InLineNumber}]: Section name is malformed, expected format [SECTION_NAME]!");
					return false;
				}
			}

			// Context
			{
				startIndex = InLine.IndexOf('(', endIndex);
				if (startIndex > 0)
				{
					endIndex = InLine.IndexOf(')', startIndex + 1);
					if (endIndex > startIndex + 1)
					{
						int length = endIndex - startIndex - 1;
						OutSectionContext = InLine.Substring(startIndex + 1, length).Trim();
					}
				}
				startIndex = endIndex;
			}

			int mothersIndex = InLine.IndexOf(':', startIndex);
			if (mothersIndex > 0)
			{
				string[] mothers = InLine.Substring(mothersIndex + 1).Split(',').Select(s => s.Trim()).Where(s => s.Any()).ToArray();
				if (mothers.Count() == 0)
				{
					Debug.LogError($"SectionMap::Section_Create:{InFilePath}:[{InLineNumber}]: Section Mothers bad definition!");
					return false;
				}
				else
				{
					OutSectionMothers = mothers;
				}
			}
			
			return OutSectionName.IsNotNull();
		}

		//////////////////////////////////////////////////////////////////////////
		// CREATE A NEW SECTION WHILE READING FILE
		private static bool Section_Create(in string sLine, in string sFilePath, in int iLine)
		{
			if (m_CurrentSection.IsNotNull())
			{
				Section_Close();
			}

			bool bOutResult = true;
			if (bOutResult = ExtractSectionInfo(sFilePath, iLine, sLine, out string OutSectionName, out string OutSectionContext, out string[] OutSectionMothers))
			{
				if (Implementation.ContainsSection_Impl(m_SectionMap, OutSectionName))
				{
					Debug.LogError($"SectionMap::Section_Create:{sFilePath}:[{iLine}]: Section \"{OutSectionName}\" already exists!");
					return false;
				}

				foreach(string motherName in OutSectionMothers)
				{
					if (!Implementation.TryGetSection_Impl(m_SectionMap, motherName, out Section motherSection))
					{
						Debug.LogError($"SectionMap::Section_Create:{sFilePath}:[{iLine}]: Section requested for inheritance \"{motherName}\" doesn't exist!");
						bOutResult = false;
					}
				}

				if (bOutResult)
				{
					m_ReadingPhase = EReadingPhase.READING_SECTION;
					m_CurrentSection = new Section(OutSectionName, sFilePath, OutSectionContext);

					foreach (string motherName in OutSectionMothers)
					{
						if (Implementation.TryGetSection_Impl(m_SectionMap, motherName, out Section motherSection))
						{
							m_CurrentSection += motherSection;
						}
					}
				}
			}
			return bOutResult;
		}


		//////////////////////////////////////////////////////////////////////////
		// INSERT A KEY VALUE PAIR INSIDE THE ACTUAL PARSING SECTION
		private static bool Section_Add(in KeyValue InPair, in string InFilePath, in int InLineNumber)
		{
			LineValue pLineValue = new LineValue(InPair.Key, InPair.Value);
		//	if (!pLineValue.IsOK)
		//	{
		//		Debug.LogError($" SectionMap::Section_Add:LineValue invalid for key |{InPair.Key}| in Section |{m_CurrentSection.GetSectionName()}| in file |{InFilePath}|");
		//		return false;
		//	}
			m_CurrentSection.Add(pLineValue);
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// DEFINITELY SAVE THE PARSING SECTION
		private static void Section_Close()
		{
			if (m_CurrentSection.IsNotNull())
			{
				m_SectionMap.Add(m_CurrentSection.GetSectionName(), m_CurrentSection);
			}
			m_CurrentSection = null;
			m_ReadingPhase = EReadingPhase.NONE;
		}
	}
}


