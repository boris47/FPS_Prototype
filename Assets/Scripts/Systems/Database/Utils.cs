
namespace DatabaseCore
{
	using System.Linq;
	using System.Globalization;

	public struct KeyValue
	{
		public string Key, Value;
		public bool IsOK;
	};

	public class Utils
	{
		//////////////////////////////////////////////////////////////////////////
		/// <summary> Search for comment char and discard all on the right side of comment char otherwise original string is returned </summary>
		public static string CleanComments(in string InString)
		{
			int index = InString.IndexOf(';');
			if (index >= 0)
			{
				return InString.Substring(0, index);
			}
			
			return InString;
		}

		//////////////////////////////////////////////////////////////////////////
		private static bool IsNumber(in char InChar) => InChar > 47 && InChar < 58;

		//////////////////////////////////////////////////////////////////////////
		private static bool IsValidChar(in char InChar)
		{
			return ( InChar > 64 && InChar < 91  ) ||		// A - Z
					( InChar > 96 && InChar < 123 ) ||		// a - z
					( InChar == 58 ) 						// : ( Double dot )
			;
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return the System.Type of value reading the entire string <para><b>(Boolean, Int32, Single, String)</b></para> </summary>
		private static System.Type ReturnValueType(in string InLine)
		{
			bool bCouldBeString = false;
			bool bCouldBeNumber = false;
			bool bDotFound = false;

			string cleanLine = CleanComments(InLine).Trim();
			for (int i = 0, count = cleanLine.Length; i < count; i++)
			{
				char Char = cleanLine[i];
				if (Char == ' ') continue;									// skip parsing spaces
				if (Char == '.')											// (Dot) Useful for number determination
				{
					if (bDotFound)
					{
						bCouldBeNumber = false;
						bCouldBeString = true;								// A double dot equals a string for sure
						continue;
					}
					bDotFound = true;
					// cleanLine.IsValidIndex(i + 1)
					if (!bCouldBeString && (i + 1) < cleanLine.Length && IsNumber(cleanLine[i + 1]))
					{
						bCouldBeNumber = true;
					}
					continue;
				}
				if (IsNumber(Char) && !bCouldBeString)						// is number and not a string
				{
					bCouldBeNumber = true;									// Ok, now is a number
					continue;
				}
				if (IsValidChar(Char))										// is char [ A-Z ] or [ a-z ] or :
				{
					bCouldBeString = true;
					bCouldBeNumber = false;									// if was a number now is a string, never more a number
					break;
				}
			}

			if (bCouldBeNumber)												// try understand if is a int or float type
			{
				return bDotFound ? typeof(System.Single) : typeof(System.Int32);
			}

			if (bCouldBeString)												// try understand if is a string or boolean type
			{
				return ((cleanLine.ToLower() == "true") || (cleanLine.ToLower() == "false")) ? typeof(System.Boolean) : typeof(System.String);
			}
			return null;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a cValue object if value is identified, otherwise null. <para>Can recognize <b>Boolean, Int32, Single, String</b></para> </summary>
		public static		Value RecognizeValue( in string line )
		{
			Value OutValue = null;
			System.Type type = ReturnValueType(line);
			if (type == typeof(bool))
			{
				OutValue = (line.ToLower() == "true");
			}

			if (type == typeof(int))
			{
				OutValue = int.Parse(line);
			}

			if (type == typeof(float))
			{
				OutValue = float.Parse(line.TrimEnd('f', ' '), CultureInfo.InvariantCulture);
			}

			if (type == typeof(string))
			{
				OutValue = line.Trim();
			}
			return OutValue;
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
			{							// try parse it
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
		/// <summary> Return a KeyValue for the given a string. <para>The format should be String = String </para> </summary>
		public static	KeyValue	GetKeyValue(in string InLine)
		{
			KeyValue Result = new KeyValue() { IsOK = false, Key = "", Value = "" };

			string cleanLine = CleanComments(InLine).Trim();
			if (cleanLine.Any())
			{
				int iEqualSign = cleanLine.IndexOf('=');
				if (iEqualSign > -1)
				{   // Key Value Pair
					string sKey = cleanLine.Substring(0, iEqualSign).Trim();
					string sValue = cleanLine.Substring(iEqualSign + 1).Trim();
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
	};
}
