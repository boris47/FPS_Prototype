
using UnityEngine;

namespace CFG_Reader {

	public class cLineValue {

		private	string						sKey				= string.Empty;
		private	string						sRawValue			= string.Empty;

		private	LineValueType				iType				= 0;

		private	cMultiValue					pMultiValue			= null;
		private	cValue						pValue				= null;

		public	bool						IsOK
		{
			get; private set;
		}


		// Can be NONE, SINGLE, MULTI, KEYONLY
		public 	LineValueType		Type 				{ get {return iType; } }
		public 	string				Key					{ get { return ( string ) sKey.Clone(); } }
		public 	string				RawValue 			{ get { return ( string ) sRawValue.Clone(); } }
		public 	cValue				Value				{ get { return pValue; } }
		public 	cMultiValue			MultiValue			{ get { return pMultiValue;	} }



		// Type can be Single or Multi
		public cLineValue( string Key, LineValueType Type )
		{
			iType = Type; sKey = Key;
		}

		public cLineValue ( string Key, string sLine )
		{
			sKey = Key;
			sRawValue = ( ( sLine.Length > 0 ) ? sLine : "" );

			if ( sLine.IndexOf( ',' ) > -1 )
			{				// Supposing is a MultiVal string
				iType = LineValueType.MULTI;
				cValue[] vValues = Utils.String.RecognizeValues( sLine );
				if ( vValues.Length < 1 )
					return;

				pMultiValue = new cMultiValue( vValues );
		
			}
			else
			{ // Single value
				iType = LineValueType.SINGLE;
				cValue pValue = Utils.String.RecognizeValue( sLine );
				if ( pValue == null ) {
					 Debug.LogError( " cLineValue::Constructor: for key " + Key + " value type is undefined" );
					return;
				}
				this.pValue = pValue;
			}
			IsOK = true;
		}

		public void Destroy()
		{
			pValue = null;
			pMultiValue = null;
		}



		/////////////////////////////////////////////////////////////////////////////////


		public 	bool IsKey( string Key )					{ return ( sKey == Key ); }

		public 	void Clear()								{ pValue = null; pMultiValue = null; }
		
		public 	cLineValue Set( cValue _Value )				{ pValue = _Value; return this; }
		public 	cLineValue Set( cMultiValue _MultiValue )	{ pMultiValue = _MultiValue; return this;	}
		
	

	}


}