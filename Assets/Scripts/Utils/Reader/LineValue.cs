
using System.Collections.Generic;
using UnityEngine;

public class cLineValue {

	string				sKey				= "";
	string				sRawValue			= "";

	LineValueType		iType				= 0;

	cMultiValue			pMultiValue			= null;
	cValue				pValue				= null;

	bool				bIsOK				= false;


	// Can be NONE, SINGLE, MULTI, KEYONLY
	public 	LineValueType Type 							{ get {return iType; } }
	public 	string Key									{ get { return ( string ) sKey.Clone(); } }
	public 	string RawValue 							{ get { return ( string ) sRawValue.Clone(); } }
	public 	cValue		Value							{ get { return pValue; } }
	public 	cMultiValue	MultiValue						{ get { return pMultiValue;	} }






	// Type can be Single or Multi
	public cLineValue( string Key, LineValueType Type ) {
		iType = Type; sKey = Key;
	}

	public cLineValue ( string Key, string sLine ) {

		sKey = Key;
		sRawValue = ( ( sLine.Length > 0 ) ? sLine : "" );

		if ( sLine.IndexOf( ',' ) > -1 ) {				// Supposing is a MultiVal string
			iType = LineValueType.MULTI;
			List < cValue  > vValues = Utils.String.RecognizeValues( sLine );
			if ( vValues.Count < 1 ) return;
			pMultiValue = new cMultiValue( vValues );
		
		} else { // Single value
			iType = LineValueType.SINGLE;
			cValue pValue = Utils.String.RecognizeValue( sLine );
			if ( pValue == null ) {
				 Debug.LogError( " cLineValue::Constructor: for key " + Key + " value type is undefined" );
				return;
			}
			this.pValue = pValue;
		}
	
		bIsOK = true;

	}

	public void Destroy() { pValue = null; pMultiValue = null; }

	public bool IsOK() { return bIsOK; }



	/////////////////////////////////////////////////////////////////////////////////


	public 	bool IsKey( string Key )					{ return ( sKey == Key ); }

	public 	void Clear()								{ pValue = null; pMultiValue = null; }
		
	public 	cLineValue Set( cValue _Value )				{ pValue = _Value; return this; }
	public 	cLineValue Set( cMultiValue _MultiValue )	{ pMultiValue = _MultiValue; return this;	}
		
	

}
