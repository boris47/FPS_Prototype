
using System.Globalization;

using Database;
using UnityEngine;

namespace Utils
{
	public static class String
	{
		public static bool IsAssetsPath(in string path) => path.StartsWith("Assets/");


		public static bool IsResourcesPath(in string path) => !path.StartsWith("Assets/");

		public	static	bool	IsAbsolutePath( in string path )
		{
			return !string.IsNullOrWhiteSpace(path)
			&& path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) == -1
			&& System.IO.Path.IsPathRooted(path) //  whether the specified path string contains a root.
			&& !System.IO.Path.GetPathRoot(path).Equals(System.IO.Path.DirectorySeparatorChar.ToString(), System.StringComparison.Ordinal);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> </summary>
		public	static	bool	ConvertFromAssetPathToResourcePath( ref string resourcePath )
		{
			const string AssetPathPrefix = "Assets/Resources/";
			const int AssetPathPrefixLength = 17;

			if ( string.IsNullOrEmpty( resourcePath ) || global::System.IO.Path.HasExtension( resourcePath ) == false )
			{
				return false;
			}

			// Assets/Resources/SkyCubeMaps/Clear/00-00.png
			string result = resourcePath;

			result = global::System.IO.Path.ChangeExtension( resourcePath, null );
			// Assets/Resources/SkyCubeMaps/Clear/00-00

			if ( result.StartsWith( AssetPathPrefix ) == false )
			{
				return false;
			}
			result = result.Remove( 0, AssetPathPrefixLength );
			// Resources/SkyCubeMaps/Clear/00-00

			resourcePath = result;
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> </summary>
		public static bool ConvertFromResourcePathToAssetPath(ref string resourcePath)
		{
			if (!string.IsNullOrEmpty(resourcePath) && global::System.IO.Path.HasExtension(resourcePath))
			{
				// resourcePath -> SkyCubeMaps/Clear/00-00.png
				string AssetPathPrefix = System.IO.Path.Combine(Application.dataPath, "Resources");

				// SkyCubeMaps/Clear/00-00
				if (!global::System.IO.Path.ChangeExtension(resourcePath, null).StartsWith(AssetPathPrefix))
				{
					//E:/SourceTree/8_FPS_Prototype2017/Assets/Resources/Configs/Assets/Resources/SkyCubeMaps/Clear/00-00
					resourcePath = System.IO.Path.Combine(AssetPathPrefix, resourcePath);
					return true;
				}
			}
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> </summary>
		public static bool ConvertFromAbsolutePathToResourcePath(ref string resourcePath)
		{
			int index = resourcePath.IndexOf("Resources");
			if (!string.IsNullOrEmpty(resourcePath) && index >= 0)
			{
				// E:/SourceTree/8_FPS_Prototype2019/Assets/Resources/Configs/WeatherPresets/af3_medium_clear_2.ltx
				string result = resourcePath;

				// Remove extension
				if (System.IO.Path.HasExtension(resourcePath))
				{
					result = System.IO.Path.ChangeExtension(resourcePath, null);
				}
				// E:/SourceTree/8_FPS_Prototype2019/Assets/Resources/Configs/WeatherPresets/af3_medium_clear_2

				resourcePath = result.Remove(0, index + 9 /*'Resource'*/ + 1 /*'/'*/ );
				// Configs/WeatherPresets/af3_medium_clear_2
				return true;
			}
			return false;
		}

		private static readonly string[] DEFAULT_COMMENT_CHARS = { ";", "//" };

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Search for comment char and discard all presente on the right side of comment char, default char is ';' </summary>
		public	static		void CleanComments(ref string str, string[] commentChars = null)
		{
			commentChars = commentChars ?? DEFAULT_COMMENT_CHARS;
			foreach (string cchar in commentChars)
			{
				int index = str.IndexOf(cchar);
				if (index >= 0)
				{
					str = str.Remove(index);
					return;
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return if string contains at last one letter </summary>
		public	static		bool ContainsLetter(in string str)
		{
			bool found = false;
			for (int i = 0; i < str.Length && found == false; i++)
			{
				if (char.IsLetter(str[i])) // true if is letter
				{
					found = true;
				}
			}
			return found;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return if string contains at last one digit </summary>
		public	static		bool ContainsDigit(in string str)
		{
			bool found = false;
			for (int i = 0; i < str.Length && found == false; i++)
			{
				if (char.IsDigit(str[i]))  // true if is a number
				{
					found = true;
				}
			}
			return found;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>  </summary>
		public	static		bool ContainsExtraValidChars(in string str, in char[] extraValidChars)
		{
			bool found = false;
			if (extraValidChars.IsNotNull())
			{
				for (int i = 0; i < str.Length && found == false; i++)
				{
					foreach (char validChar in extraValidChars)
					{
						if (str[i] == validChar)
						{
							found = true;
							break;
						}
					}
				}
			}
			else
			{
				found = true; // With no filter we can allow everything
			}
			return found;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return true for non empty string, that non contains at last one number or one letter or a block closing char </summary>
		public	static		bool IsValid( in string str, in char[] extraValidChars = null )
		{
			return (str.Length > 0) && (ContainsLetter(str) || ContainsDigit(str) || ContainsExtraValidChars(str, extraValidChars));
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return true for valid chars ( A - Z, a - z,  ':' ) </summary>
		private	static		bool IsValidChar( in char Char )
		{
			return ( ( Char > 64 && Char < 91  ) || // A - Z
					 ( Char > 96 && Char < 123 ) || // a - z
					 ( Char == 58 ) 			 	// : ( Double dot )
				 );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return the System.Type of value reading the entire string ( bool, int, float, string ) </summary>
		private static global::System.Type ReturnValueType(string sLine)
		{
			CleanComments(ref sLine);

			bool b_IsString = false;
			bool b_IsNumber = false;
			bool b_DotFound = false;
			bool b_Determinated = false;
			for (int i = 0; i < sLine.Length && b_Determinated == false; i++)
			{
				char Char = sLine[i];
				if (Char == 32) continue;                               // skip parsing spaces
				if (Char == 46)											// (Dot) Useful for number determination
				{
					b_DotFound = true;
					b_Determinated = true;
					continue;
				}
				if (Char > 47 && Char < 58 && b_IsString == false)		// is number and not a str
				{
					b_IsNumber = true;									// ok, now is a number
					continue;
				}
				if (IsValidChar(Char))									// is char [ A-Z ] or [ a-z ] or :
				{
					b_IsString = true;
					b_IsNumber = false;                                 // if was a number now is a string, never more a number
					b_Determinated = true;
					break;
				}
			}

			if (b_IsNumber)												// try understand if is a int or float type
			{
				if (b_DotFound)											// because of dot is probably a float value
				{
					return typeof(float);
				}
				else
				{
					return typeof(int);                             // No dot found so is probably an integer
				}
			}

			if (b_IsString)                                         // try understand if is a string or boolean type
			{
				if ((sLine.ToLower() == "true") || (sLine.ToLower() == "false"))
				{
					return typeof(bool);
				}
				else
				{
					return typeof(string);
				}
			}
			return null;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a cValue object if value is identified, otherwise null </summary>
		public	static		Value RecognizeValue( in string line )
		{
			System.Type type = ReturnValueType(line);
			if (type == typeof(bool))
			{
				return (line.ToLower() == "true") ? true : false;
			}

			if (type == typeof(int))
			{
				return System.Int32.Parse(line);
			}

			if (type == typeof(float))
			{
				return float.Parse(line.TrimEnd('f', ' '), CultureInfo.InvariantCulture);
			}

			if (type == typeof(string))
			{
				return line.Trim();
			}
			return null;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Parse a string and return a list of values </summary>
		public	static		Value[] RecognizeValues( in string line )
		{
			string[] values = line.Split(',');
			if (values.Length > 0)
			{
				Value[] cValues = new Value[values.Length];
				for (int i = 0; i < values.Length; i++)
				{
					cValues[i] = RecognizeValue(values[i]);
				}
				return cValues;
			}
			return null;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return an array of KeyValue on the same line </summary>
		public	static	KeyValue[]	GetKeyValues( in string line )
		{
			string[] keyValues = line.Split(',');

			KeyValue[] values = null;

			if (keyValues.Length == 0)  // It can be that on this line there is only one Key Value
			{                               // try parse it
				values = new KeyValue[1];
				values[0] = GetKeyValue(line);
				return values;
			}

			// Multiple results
			values = new KeyValue[keyValues.Length];
			for (int i = 0; i < keyValues.Length; i++)
			{
				values[i] = GetKeyValue(keyValues[i]);
			}
			return values;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a KeyValue fìgiven a string the fourmat should be String = String </summary>
		public	static	KeyValue	GetKeyValue( string Line )
		{
			KeyValue Result = new KeyValue() { IsOK = false, Key = "", Value = "" };

			CleanComments(ref Line);
			if (IsValid(Line))
			{
				int iEqualSign = Line.IndexOf('=');

				if (iEqualSign > -1)
				{   // Key Value Pair
					string sKey = Line.Substring(0, iEqualSign).Trim();
					string sValue = Line.Substring(iEqualSign + 1);
					if (sValue.Length > 0) sValue = sValue.Trim();

					if (sKey.Length > 0)
					{
						Result.Key = sKey;
						Result.Value = sValue;
						Result.IsOK = true;
					}
				}
			}
			return Result;
		}

	}
}

