
using UnityEngine;
using System.Collections.Generic;

namespace Database {

	//* TYPES */
	[System.Serializable]
	public enum ELineValueType : byte	{ SINGLE, MULTI };

	[System.Serializable]
	public class cLineValue {

		[SerializeField]
		private	string						sKey				= string.Empty;
		[SerializeField]
		private	string						sRawValue			= string.Empty;

		[SerializeField]
		private	ELineValueType				iType				= 0;

		[SerializeField]
		private	cMultiValue					pMultiValue			= null;
		[SerializeField]
		private	cValue						pValue				= null;

		public	bool						IsOK
		{
			get; private set;
		}


		// Can be NONE, SINGLE, MULTI, KEYONLY
		public 	ELineValueType		Type 						{ get {return this.iType; } }
		public 	string				Key							{ get { return ( string )this.sKey.Clone(); } }
		public 	string				RawValue 					{ get { return ( string )this.sRawValue.Clone(); } }
		public 	cValue				Value						{ get { return this.pValue; } }
		public 	cMultiValue			MultiValue					{ get { return this.pMultiValue;	} }



		// Type can be Single or Multi
		/////////////////////////////////////////////////////////////////////////////////
		// CONSTRUCTOR
		public cLineValue( string Key, ELineValueType Type )
		{
			this.iType = Type; this.sKey = Key; this.sRawValue = Key;
		}


		/////////////////////////////////////////////////////////////////////////////////
		// CONSTRUCTOR
		public cLineValue( cLineValue clone ) : this( clone.sKey, clone.sRawValue )
		{}


		/////////////////////////////////////////////////////////////////////////////////
		// CONSTRUCTOR
		public cLineValue ( string Key, string sLine )
		{
			this.sKey = Key;
			this.sRawValue = ( ( sLine.Length > 0 ) ? sLine : "" );

			if ( sLine.IndexOf( ',' ) > -1 )
			{ // Supposing is a MultiVal string
				this.iType = ELineValueType.MULTI;
				cValue[] vValues = Utils.String.RecognizeValues( sLine );
				if ( vValues.Length < 1 )
					return;

				this.pMultiValue = new cMultiValue( vValues );
		
			}
			else
			{ // Single value
				this.iType = ELineValueType.SINGLE;
				cValue pValue = Utils.String.RecognizeValue( sLine );
				if ( pValue == null ) {
					UnityEngine.Debug.LogError( " cLineValue::Constructor: for key " + Key + " value type is undefined" );
					return;
				}
				this.pValue = pValue;
			}
			this.IsOK = true;
		}



		/////////////////////////////////////////////////////////////////////////////////
		public bool	GetAsSingle( ref cValue value )
		{
			bool bResult = this.iType == ELineValueType.SINGLE;
			if ( bResult )
			{
				value = this.pValue;
			}
			return bResult;
		}


		/////////////////////////////////////////////////////////////////////////////////
		public bool	GetAsMulti( ref cMultiValue multiValue )
		{
			bool bResult = this.iType == ELineValueType.MULTI;
			if ( bResult )
			{
				multiValue = this.pMultiValue;
			}
			return bResult;
		}

		public void Destroy()
		{
			this.pValue = null;
			this.pMultiValue = null;
		}



		/////////////////////////////////////////////////////////////////////////////////
		public 	bool IsKey( string Key )
		{
			return (this.sKey == Key );
		}


		/////////////////////////////////////////////////////////////////////////////////
		public 	void Clear()
		{
			this.pValue		= null;
			this.pMultiValue = null;
		}
		

		/////////////////////////////////////////////////////////////////////////////////
		public 	cLineValue Set( cValue _Value )
		{
			this.pValue		= _Value;
			this.pMultiValue = null;
			this.iType		= ELineValueType.SINGLE;
			return this;
		}


		/////////////////////////////////////////////////////////////////////////////////
		public 	cLineValue Set( cMultiValue _MultiValue )
		{
			this.pMultiValue	= _MultiValue;
			this.pValue		= null;
			this.iType		= ELineValueType.MULTI;
			return this;
		}
		
	

	}


}