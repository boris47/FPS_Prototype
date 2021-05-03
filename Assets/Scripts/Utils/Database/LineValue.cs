
using UnityEngine;

namespace Database
{

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
		private	MultiValue					m_MultiValue			= null;
		[SerializeField]
		private	Value						m_SingleValue				= null;

		public	bool						IsOK
		{
			get; private set;
		}


		// Can be NONE, SINGLE, MULTI, KEYONLY
		public		ELineValueType		Type				=> iType;
		public		string				Key					=> sKey;
		public		string				RawValue			=> sRawValue;
		public		Value				Value				=> m_SingleValue;
		public		MultiValue			MultiValue			=> m_MultiValue;
		public		bool				IsKey(string Key) => (sKey == Key);


		/////////////////////////////////////////////////////////////////////////////////
		public LineValue( string Key, ELineValueType Type )
		{
			iType = Type; sKey = Key; sRawValue = Key;
		}


		/////////////////////////////////////////////////////////////////////////////////
		public LineValue( LineValue clone ) : this( clone.sKey, clone.sRawValue )
		{}


		/////////////////////////////////////////////////////////////////////////////////
		// CONSTRUCTOR
		public LineValue ( string Key, string sLine )
		{
			sKey = Key;
			sRawValue = ( ( sLine.Length > 0 ) ? sLine : "" );
			IsOK = false;

			if ( sLine.IndexOf( ',' ) > -1 ) // Supposing is a MultiVal string
			{
				iType = ELineValueType.MULTI;
				Value[] vValues = Utils.String.RecognizeValues( sLine );
				if ( vValues.Length >= 0 )
				{
					m_MultiValue = new MultiValue( vValues );
					IsOK = true;
				}
			}
			else // Single value
			{
				iType = ELineValueType.SINGLE;
				Value pValue = Utils.String.RecognizeValue( sLine );
				if (!pValue)
				{
					UnityEngine.Debug.LogError( $"cLineValue::Constructor: for key {Key} value type is undefined" );
				}
				else
				{
					m_SingleValue = pValue;
					IsOK = true;
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////////
		public bool	GetAsSingle( out Value result )
		{
			if (iType == ELineValueType.SINGLE)
			{
				result = m_SingleValue;
				return true;
			}
			result = null;
			return false;
		}


		/////////////////////////////////////////////////////////////////////////////////
		public bool	GetAsMulti( out MultiValue result )
		{
			if (iType == ELineValueType.MULTI)
			{
				result = m_MultiValue;
				return true;
			}
			result = null;
			return false;
		}


		/////////////////////////////////////////////////////////////////////////////////
		public void Destroy()
		{
			Clear();
		}


		/////////////////////////////////////////////////////////////////////////////////
		public 	void Clear()
		{
			m_SingleValue = null;
			m_MultiValue  = null;
		}
		

		/////////////////////////////////////////////////////////////////////////////////
		public 	LineValue Set( Value _Value )
		{
			m_SingleValue		= _Value;
			m_MultiValue		= null;
			iType				= ELineValueType.SINGLE;
			return this;
		}


		/////////////////////////////////////////////////////////////////////////////////
		public 	LineValue Set( MultiValue _MultiValue )
		{
			m_SingleValue		= null;
			m_MultiValue		= _MultiValue;
			iType				= ELineValueType.MULTI;
			return this;
		}
	}
}