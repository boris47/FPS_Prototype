﻿
namespace Database {

	
	public class cValue {

		private	readonly	object			m_Value	= null;
		private readonly 	System.Type		m_Type	= null;


		///////////////////////////////////////////////////////////////////////////////

		public	cValue( object value )
		{
			m_Value = value;
			m_Type = value.GetType();
		}


		///////////////////////////////////////////////////////////////////////////////

		public	bool	Is<T>()
		{	
			System.Type requiredType = typeof(T);
			return m_Type == requiredType;
		}


		///////////////////////////////////////////////////////////////////////////////

		public T As<T>()
		{
			T result = default( T );
			try
			{
				result = (T) System.Convert.ChangeType( m_Value, typeof(T) );
			}
			catch( System.Exception e )
			{
				UnityEngine.Debug.LogException( e );
			}

			// Alternative: 
		//	T result = (T) System.Convert.ChangeType( m_Value, typeof(T) );
			/*
			T result;
			try
			{
				result = (T) m_Value;
			}
			catch ( System.Exception )
			{
				result = default( T );
			}
			*/
			return result;
		}


		///////////////////////////////////////////////////////////////////////////////

		public	new 	System.Type	GetType()
		{
			return m_Type;
		}


		private	string InternalToString()
		{
			string result = null;
			if ( m_Type == typeof(float) )
			{
				result = ((float)m_Value).ToString("0.0000000");
			}
			else
			{
				result = m_Value.ToString();
			}

			return result;
		}

		///////////////////////////////////////////////////////////////////////////////

		public	bool			ToBool()			{	return As<bool>();			}
		public	int				ToInteger()			{	return As<int>();			}
		public	float			ToFloat()			{	return As<float>();			}
		public override string	ToString()			{	return InternalToString();	}
		public	object			ToSystemObject()	{	return m_Value;				}


		///////////////////////////////////////////////////////////////////////////////

		public static implicit operator bool	( cValue v )		{	return v.ToBool();		}
		public static implicit operator int		( cValue v )		{	return v.ToInteger();	}
		public static implicit operator float	( cValue v )		{	return v.ToFloat();		}
		public static implicit operator string	( cValue v )		{	return v.ToString();	}

		///////////////////////////////////////////////////////////////////////////////

		public static implicit operator cValue	( bool b )			{	return new cValue( b );	}
		public static implicit operator cValue	( int i )			{	return new cValue( i );	}
		public static implicit operator cValue	( float f )			{	return new cValue( f );	}
		public static implicit operator cValue	( string s )		{	return new cValue( s );	}

		///////////////////////////////////////////////////////////////////////////////

	}


}