
using UnityEngine;
using System.Collections.Generic;

namespace Database {

	//* TYPES */
	[System.Serializable]
	public enum ELineValueType : byte	{ SINGLE, MULTI };

	[System.Serializable]
	public class LineValue
	{
		[SerializeField]
		private	string						sKey				= string.Empty;
		[SerializeField]
		private	string						sRawValue			= string.Empty;

		[SerializeField]
		private	ELineValueType				iType				= 0;

		[SerializeField]
		private	MultiValue					pMultiValue			= null;
		[SerializeField]
		private	Value						pValue				= null;

		public	bool						IsOK
		{
			get; private set;
		}


		// Can be NONE, SINGLE, MULTI, KEYONLY
		public 	ELineValueType		Type 						{ get {return this.iType; } }
		public 	string				Key							{ get { return ( string )this.sKey.Clone(); } }
		public 	string				RawValue 					{ get { return ( string )this.sRawValue.Clone(); } }
		public 	Value				Value						{ get { return this.pValue; } }
		public 	MultiValue			MultiValue					{ get { return this.pMultiValue;	} }



		// Type can be Single or Multi
		/////////////////////////////////////////////////////////////////////////////////
		// CONSTRUCTOR
		public LineValue( string Key, ELineValueType Type )
		{
			this.iType = Type; this.sKey = Key; this.sRawValue = Key;
		}


		/////////////////////////////////////////////////////////////////////////////////
		// CONSTRUCTOR
		public LineValue( LineValue clone ) : this( clone.sKey, clone.sRawValue )
		{}


		/////////////////////////////////////////////////////////////////////////////////
		// CONSTRUCTOR
		public LineValue ( string Key, string sLine )
		{
			this.sKey = Key;
			this.sRawValue = ( ( sLine.Length > 0 ) ? sLine : "" );

			if ( sLine.IndexOf( ',' ) > -1 )
			{ // Supposing is a MultiVal string
				this.iType = ELineValueType.MULTI;
				Value[] vValues = Utils.String.RecognizeValues( sLine );
				if ( vValues.Length < 1 )
					return;

				this.pMultiValue = new MultiValue( vValues );
		
			}
			else
			{ // Single value
				this.iType = ELineValueType.SINGLE;
				Value pValue = Utils.String.RecognizeValue( sLine );
				if ( pValue == null ) {
					UnityEngine.Debug.LogError( " cLineValue::Constructor: for key " + Key + " value type is undefined" );
					return;
				}
				this.pValue = pValue;
			}
			this.IsOK = true;
		}



		/////////////////////////////////////////////////////////////////////////////////
		public bool	GetAsSingle( ref Value value )
		{
			bool bResult = this.iType == ELineValueType.SINGLE;
			if ( bResult )
			{
				value = this.pValue;
			}
			return bResult;
		}


		/////////////////////////////////////////////////////////////////////////////////
		public bool	GetAsMulti( ref MultiValue multiValue )
		{
			bool bResult = this.iType == ELineValueType.MULTI;
			if ( bResult )
			{
				multiValue = this.pMultiValue;
			}
			return bResult;
		}


		/////////////////////////////////////////////////////////////////////////////////
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
		public 	LineValue Set( Value _Value )
		{
			this.pValue			= _Value;
			this.pMultiValue	= null;
			this.iType			= ELineValueType.SINGLE;
			return this;
		}


		/////////////////////////////////////////////////////////////////////////////////
		public 	LineValue Set( MultiValue _MultiValue )
		{
			this.pMultiValue	= _MultiValue;
			this.pValue			= null;
			this.iType			= ELineValueType.MULTI;
			return this;
		}
		
	

	}


}