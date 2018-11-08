
using System;
using System.Collections.Generic;
using Database;
using UnityEngine;



namespace Utils {

	public static class String {


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Replace all '\\' with a dot each one
		/// </summary>
		public	static		string ToDotStr( string FilePath )
		{
			return FilePath.Replace( '\\', '.' ).Replace( '/', '.' );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Search for comment char and discard all presente on the right side of comment char, default char is ';'
		/// </summary>
		public	static		void CleanComments( ref string str, char commentChar = ';' )
		{
			if ( str.Length < 1 ) return;
			for ( int i = 0; i < str.Length; i++ )
			{
				if ( str[ i ] == commentChar )
				{
					str = str.Remove( i );
					return;
				}
			}

		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return if string contains at last one letter
		/// </summary>
		public	static		bool ContainsLetter( string str )
		{
			bool found = false;
			for ( int i = 0; i < str.Length && found == false; i++ )
			{
				if ( char.IsLetter( str[ i ] ) ) // true if is letter
				{
					found = true;
				}
			}
			return found;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return if string contains at last one digit
		/// </summary>
		public	static		bool ContainsDigit( string str )
		{
			bool found = false;
			for ( int i = 0; i < str.Length && found == false; i++ )
			{
				if ( char.IsDigit( str[ i ] ) )  // true if is a number
				{
					found = true;
				}
			}
			return found;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return true for non empty string, that non contains at last one number or one letter
		/// </summary>
		public	static		bool IsValid( ref string str )
		{
			CleanComments( ref str );
			return ( ( str.Length > 0 )  && ( ( ContainsLetter( str ) == true || ContainsDigit( str ) == true ) ) );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return true for valid chars ( A - Z, ':' )
		/// </summary>
		private	static		bool IsValidChar( char Char )
		{
			return ( ( Char > 64 && Char < 91  ) || // A - Z
					 ( Char > 96 && Char < 123 ) || // a - z
					 ( Char == 58 ) 				// : ( Double dot )
				 );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return the System.Type of value reading the entire string ( bool, int, float, string )
		/// </summary>
		private	static		global::System.Type ReturnValueType( string sLine )
		{
			CleanComments( ref sLine );

			bool b_IsString = false;
			bool b_IsNumber = false;
			bool b_DotFound = false;
			for ( int i = 0; i < sLine.Length ; i++ )
			{
				char Char = sLine[ i ];
				if ( Char == 32 ) continue;								// skip parsing spaces
				if ( Char == 46 )										// (Dot) Useful for number determination
				{
					b_DotFound = true;
					continue;
				}
				if ( Char > 47 && Char < 58 && b_IsString == false )	// is number and not a str
				{
					b_IsNumber = true;									// ok, now is a number
					continue;
				}
				if ( IsValidChar( Char ) )								// is char [ A-Z ] or [ a-z ] or :
				{
					b_IsString = true;
					b_IsNumber = false;									// if was a number now is a string, never more a number
					break;
				}
			}

			if ( b_IsNumber )											// try understand if is a int or float type
			{
				if ( b_DotFound )										// because of dot is probably a float value
					return typeof( float );
				else
					return typeof( int );								// No dot found so is probably an integer
			}

			if ( b_IsString )											// try understand if is a string or boolean type
			{
				if ( ( sLine.ToLower() == "true" ) || ( sLine.ToLower() == "false" ) )
				{
					return typeof( bool );
				} else
					return typeof( string );
			}

			return null;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return a cValue object if value is identified, otherwise null
		/// </summary>
		public	static		cValue RecognizeValue( string line )
		{
			global::System.Type type = ReturnValueType( line );
			if ( type == typeof( bool ) )
			{
				return ( line.ToLower() == "true" ) ? true : false;
			}

			if ( type == typeof( int ) )
			{
				return Int32.Parse( line );
			}

			if ( type == typeof( float ) )
			{
				return float.Parse( line );
			}

			if ( type == typeof( string ) )
			{
				return line.Trim();
			}
			return null;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Parse a string and return a list of values
		/// </summary>
		public	static		cValue[] RecognizeValues( string line )
		{
			string[] values = line.Split( ',' );
			if ( values.Length > 0 )
			{
				cValue[] cValues = new cValue[ values.Length ];
				for ( int i = 0; i < values.Length; i++ )
				{
					cValues[ i ] = RecognizeValue( values[ i ] );
				}
				return cValues;
			}
			return null;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return an array of KeyValue on the same line
		/// </summary>
		public	static	KeyValue[]	GetKeyValues( string line )
		{
			string[] keyValues = line.Split( ',' );

			KeyValue[] values = null;

			if ( keyValues.Length == 0 )	// It can be that on this line there is only one Key Value
			{								// try parse it
				values = new KeyValue[1];
				values[0] = GetKeyValue( line );
				return values;
			}

			// Multiple results
			values = new KeyValue[keyValues.Length];
			for ( int i = 0; i < keyValues.Length; i++ )
			{
				values[ i ] = GetKeyValue( keyValues[ i ] );
			}
			return values;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return a KeyValue fìgiven a string the fourmat should be String = String
		/// </summary>
		public	static	KeyValue	GetKeyValue( string Line )
		{
			KeyValue Result = new KeyValue() { IsOK = false, Key = "", Value = "" };

			if ( Utils.String.IsValid( ref Line ) == true )
			{
				int iEqualSign = Line.IndexOf( '=' );

				if ( iEqualSign > -1 )
				{	// Key Value Pair
					string sKey = Line.Substring( 0, iEqualSign ).Trim();
					string sValue = Line.Substring( iEqualSign + 1 );
					if ( sValue.Length > 0 ) sValue = sValue.Trim();

					if ( sKey.Length > 0 )
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

