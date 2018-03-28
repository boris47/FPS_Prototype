

namespace CFG_Reader {

	public class cValue {

		private	object	m_Value
		{
			get;
			set;
		}

		public	cValue( object value )
		{
			m_Value = value;
		}
	
		public	cValue( System.Type type )
		{
			if ( type == typeof ( bool		) )	{ m_Value = false;	return; }
			if ( type == typeof ( int		) )	{ m_Value = 0;		return; }
			if ( type == typeof ( float		) )	{ m_Value = 0.0f;	return; }
			if ( type == typeof ( string	) )	{ m_Value = "";		return; }
			m_Value = null;
		}
	

		public T1 As<T1>()
		{
			T1 result;
			try
			{
				result = (T1) m_Value;
			}
			catch ( System.Exception )
			{
				result = default( T1 );
			}
			return result;
		}

		///////////////////////////////////////////////////////////////////////////////

		public	bool			ToBool()			{	return As<bool>();			}
		public	int				ToInteger()			{	return As<int>();			}
		public	float			ToFloat()			{	return As<float>();			}
		public override string	ToString()			{	return m_Value.ToString();	}
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