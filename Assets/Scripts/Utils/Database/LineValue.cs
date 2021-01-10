
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
		public 	ELineValueType		Type 						{ get {return iType; } }
		public 	string				Key							{ get { return ( string )sKey.Clone(); } }
		public 	string				RawValue 					{ get { return ( string )sRawValue.Clone(); } }
		public 	Value				Value						{ get { return pValue; } }
		public 	MultiValue			MultiValue					{ get { return pMultiValue;	} }



		// Type can be Single or Multi
		/////////////////////////////////////////////////////////////////////////////////
		// CONSTRUCTOR
		public LineValue( string Key, ELineValueType Type )
		{
			iType = Type; sKey = Key; sRawValue = Key;
		}


		/////////////////////////////////////////////////////////////////////////////////
		// CONSTRUCTOR
		public LineValue( LineValue clone ) : this( clone.sKey, clone.sRawValue )
		{}


		/////////////////////////////////////////////////////////////////////////////////
		// CONSTRUCTOR
		public LineValue ( string Key, string sLine )
		{
			sKey = Key;
			sRawValue = ( ( sLine.Length > 0 ) ? sLine : "" );

			if ( sLine.IndexOf( ',' ) > -1 )
			{ // Supposing is a MultiVal string
				iType = ELineValueType.MULTI;
				Value[] vValues = Utils.String.RecognizeValues( sLine );
				if ( vValues.Length < 1 )
					return;

				pMultiValue = new MultiValue( vValues );
		
			}
			else
			{ // Single value
				iType = ELineValueType.SINGLE;
				Value pValue = Utils.String.RecognizeValue( sLine );
				if ( pValue == null ) {
					UnityEngine.Debug.LogError( " cLineValue::Constructor: for key " + Key + " value type is undefined" );
					return;
				}
				this.pValue = pValue;
			}
			IsOK = true;
		}



		/////////////////////////////////////////////////////////////////////////////////
		public bool	GetAsSingle( ref Value value )
		{
			bool bResult = iType == ELineValueType.SINGLE;
			if ( bResult )
			{
				value = pValue;
			}
			return bResult;
		}


		/////////////////////////////////////////////////////////////////////////////////
		public bool	GetAsMulti( ref MultiValue multiValue )
		{
			bool bResult = iType == ELineValueType.MULTI;
			if ( bResult )
			{
				multiValue = pMultiValue;
			}
			return bResult;
		}


		/////////////////////////////////////////////////////////////////////////////////
		public void Destroy()
		{
			pValue = null;
			pMultiValue = null;
		}


		/////////////////////////////////////////////////////////////////////////////////
		public 	bool IsKey( string Key )
		{
			return (sKey == Key );
		}


		/////////////////////////////////////////////////////////////////////////////////
		public 	void Clear()
		{
			pValue		= null;
			pMultiValue = null;
		}
		

		/////////////////////////////////////////////////////////////////////////////////
		public 	LineValue Set( Value _Value )
		{
			pValue			= _Value;
			pMultiValue	= null;
			iType			= ELineValueType.SINGLE;
			return this;
		}


		/////////////////////////////////////////////////////////////////////////////////
		public 	LineValue Set( MultiValue _MultiValue )
		{
			pMultiValue	= _MultiValue;
			pValue			= null;
			iType			= ELineValueType.MULTI;
			return this;
		}
		
	

	}


}