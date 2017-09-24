using System;
using System.Collections .Generic;
using System.Linq;
using System.Text;
using UnityEngine;



namespace Utils {

	public static class String {

		public static string ToDotStr( string FilePath ) {
			return FilePath.Replace( '\\', '.' ).Replace( '/', '.' );
		}

		public static void CleanComments( ref string str ) {

			if ( str.Length < 1 ) return;
			for ( int i = 0; i < str.Length; i++ ) {
				if ( str[ i ] == ';' ) {
					str = str.Remove( i );
					return;
				}
			}

		}

		public static bool ContainsAlpha( string str ) {

			for ( int i = 0; i < str.Length; i++ ) {
				if ( char.IsLetter( str[ i ] ) ) {
					return true;
				}
			}
			return false;

		}

		public static bool ContainsDigit( string str ) {

			for ( int i = 0; i < str.Length; i++ ) {
				if ( char.IsDigit( str[ i ] ) ) {
					return true;
				}
			}
			return false;

		}

		public static bool IsValid( ref string str ) {

			CleanComments( ref str );
			if ( ( str.Length < 1 )  || ( !ContainsAlpha( str ) && !ContainsDigit( str ) ) ) return false;

			return true;
		}

		// Only contains letters and ':'
		private static bool IsValidChar( char Char ) {

			if ( ( Char > 64 && Char < 91  ) || // A - Z
				 ( Char > 96 && Char < 123 ) || // a - z
				 ( Char == 58 ) 				// : ( Double dot )
				 )
				 return true;
			return false;
		}

		// Return the type of value reading the string
		private static byte ReturnValueType( string sLine ) {
			bool b_IsString = false, b_IsNumber = false, b_DotFound = false;
			for ( int i = 0; i < sLine.Length ; i++ ) {

				char Char = sLine[ i ];
				if ( Char == 32 ) continue;								// skip parsing spaces
				if ( Char == 46 ) b_DotFound = true;					// (Dot)Useful for number determination
				if ( Char > 47 && Char < 58 && !b_IsString ) {			// is number and not a str
					b_IsNumber = true;									// ok, now is a number
				}
				if ( IsValidChar( Char ) ) {						// is char [ A-Z ] or [ a-z ] or :
					b_IsString = true; b_IsNumber = false;				// if was a number now is a string, never more a number
					break;
				}
			}

			if ( b_IsNumber ) {										// try understand if is a int or float type
				if ( b_DotFound ) return ( byte )ValueTypes.FLOAT;	// because of dot is probably a float value
				else return ( byte )ValueTypes.INTEGER;             // No dot found so is probably an integer
			}

			if ( b_IsString ) {										// try understand if is a string or boolean type
				if ( ( sLine.ToLower() == "true" ) || ( sLine.ToLower() == "false" ) ) {
					return ( byte )ValueTypes.BOOLEAN;
				} else return ( byte )ValueTypes.STRING;
			}

			return ( byte )ValueTypes.NONE;
		}

		// Return a cValue object if value is identified, otherwise null
		public static cValue RecognizeValue( string sLine ) {	
		
			switch( ReturnValueType( sLine ) ) {
				case ( byte ) ValueTypes.NONE:	break;
				case ( byte ) ValueTypes.INTEGER:	{
						int i = -1;
						Int32.TryParse( sLine, out i );
						return new cValue( i );
				}
				case ( byte ) ValueTypes.BOOLEAN:	{ 
					return ( sLine.ToLower() == "true" ) ? ( new cValue( true ) ) : ( new cValue( false ) );
				}
				case ( byte ) ValueTypes.FLOAT:{
						float f = 0.0f;
						float.TryParse( sLine, out f );
						return new cValue( f );
				}
			
				case ( byte ) ValueTypes.STRING:	{
						return ( new cValue( sLine ) );
				}
			}
			return null;
		}


		// parse a string and return a list of values
		public static List < cValue > RecognizeValues( string _Line ) {
			List < cValue > Values = new List < cValue > ();
			string Line = _Line;
			int Start = 0;
			for ( int i = 0; i < Line.Length; i++ ) {
				if ( Line[ i ] == ',' ) {
					string Result = ( Line.Substring( Start, i - Start ) ).Trim();
					Values.Add( RecognizeValue( Result ) );
					Start = i + 1;
				}
			}
			cValue Value = RecognizeValue( Line.Substring( Start ) );	// last value is not followed by a colon
			Values.Add( Value );					// So we save the last part of string entirely
			return Values;
		}

	}

}

