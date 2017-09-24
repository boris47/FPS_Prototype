using System;
using System.Collections .Generic;
using System.Linq;
using System.Text;
using UnityEngine;


//* TYPES */
public enum ValueTypes			{ NONE, BOOLEAN, INTEGER, FLOAT, STRING };
public enum LineValueType		{ SINGLE, MULTI };

public struct KeyValue {
	public	string	Key, Value;
	public	bool	IsOK;
};


namespace Utils {

	public static class Base {

		public static KeyValue GetKeyValue( string Line ) {

			KeyValue Result;

			Result.IsOK = false;
			Result.Key = Result.Value = "";

			if ( ! String.IsValid( ref Line ) ) return Result;

			int iEqualSign = 0;

			for ( int i = 0; i < Line.Length; i++ )
				if ( Line[ i ]  == '=' ) { iEqualSign = i; break; }

			if ( iEqualSign > 0 ) { // Key Value Pair
				string sKey = Line.Substring( 0, iEqualSign ).Trim();
				string sValue = Line.Substring( iEqualSign + 1 );
				if ( sValue.Length > 0 ) sValue = sValue.Trim();
				if ( sKey.Length > 0 ) {
					Result.Key = sKey;
					Result.Value = sValue;
					Result.IsOK = true;
					return Result;
				}
			}
			return Result;
		}

	}

}